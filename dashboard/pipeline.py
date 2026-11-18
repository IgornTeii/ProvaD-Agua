"""Pipeline do data lake do Prova d'Água: raw -> trusted -> refined.

raw      CSVs exportados do Postgres, sem alteração (não é lida/escrita aqui, só lida).
trusted  dados limpos: tipos corretos, datas em horário de Brasília, sem duplicatas,
         sem leituras impossíveis.
refined  modelo final em estrela para o Power BI (fatos + dimensões).

Uso (a partir da pasta dashboard):  python pipeline.py
"""
from pathlib import Path

import pandas as pd

BASE = Path(__file__).parent / "data"
RAW, TRUSTED, REFINED = BASE / "raw", BASE / "trusted", BASE / "refined"
TZ = "America/Sao_Paulo"  # Brasília (UTC-3)

NIVEL_MIN_CM, NIVEL_MAX_CM = 0, 300  # leitura fora disso é impossível
CHUVA_MIN, CHUVA_MAX = 0, 300        # mm/h

relatorio = []  # qualidade dos dados: linhas antes/depois de cada tabela


def ler(nome: str) -> pd.DataFrame:
    return pd.read_csv(RAW / f"{nome}.csv")


def para_brasilia(serie: pd.Series) -> pd.Series:
    """UTC (texto) -> horário de Brasília, sem fuso (naive), pronto para o Power BI."""
    return pd.to_datetime(serie, utc=True, format="ISO8601").dt.tz_convert(TZ).dt.tz_localize(None)


def registrar(tabela: str, antes: int, depois: int, motivo: str = "") -> None:
    relatorio.append({"tabela": tabela, "linhas_raw": antes, "linhas_trusted": depois,
                      "removidas": antes - depois, "motivo": motivo})


# ---------------------------------------------------------------- raw -> trusted
def construir_trusted() -> dict[str, pd.DataFrame]:
    out = {}

    s = ler("sensors")
    n = len(s)
    s = s.drop_duplicates("Id")
    for c in ("Id", "Name", "Neighborhood", "Zone"):
        s[c] = s[c].str.strip()
    out["sensors"] = s
    registrar("sensors", n, len(s), "duplicatas por Id")

    a = ler("addresses")
    n = len(a)
    a = a.drop_duplicates("Id")
    for c in ("Street", "Number", "Neighborhood", "City", "SensorId"):
        a[c] = a[c].astype(str).str.strip()
    out["addresses"] = a
    registrar("addresses", n, len(a), "duplicatas por Id")

    r = ler("readings")
    n = len(r)
    r = r.drop_duplicates("Id")
    r["MeasuredAt"] = para_brasilia(r["MeasuredAt"])
    r["ReceivedAt"] = para_brasilia(r["ReceivedAt"])
    r = r.dropna(subset=["SensorId", "WaterLevelCm", "RainfallMmPerHour", "MeasuredAt"])
    r = r[r["WaterLevelCm"].between(NIVEL_MIN_CM, NIVEL_MAX_CM)
          & r["RainfallMmPerHour"].between(CHUVA_MIN, CHUVA_MAX)]
    out["readings"] = r
    registrar("readings", n, len(r), "duplicatas, nulos e leituras impossíveis (nível/chuva fora da faixa)")

    e = ler("flood_events")
    n = len(e)
    e = e.drop_duplicates("Id")
    for c in ("StartedAt", "ConfirmedAt", "ClosedAt", "DismissedAt"):
        e[c] = para_brasilia(e[c])
    e = e[e["Status"].isin(["Suspected", "Confirmed", "Closed", "Dismissed"]) & (e["PeakWaterLevelCm"] >= 0)]
    out["flood_events"] = e
    registrar("flood_events", n, len(e), "duplicatas, status inválido, pico negativo")

    c = ler("flood_certificates")
    n = len(c)
    c = c.drop_duplicates("Id").drop_duplicates("VerificationCode")
    for col in ("FloodStartedAt", "FloodClosedAt", "IssuedAt"):
        c[col] = para_brasilia(c[col])
    c["IsEligibleForAid"] = c["IsEligibleForAid"].astype(str).str.lower().isin(["t", "true", "1"])
    out["flood_certificates"] = c
    registrar("flood_certificates", n, len(c), "duplicatas por Id e por código")

    for nome in ("alert_subscriptions", "alert_notifications"):
        t = ler(nome)
        n = len(t)
        t = t.drop_duplicates("Id")
        for col in [x for x in t.columns if x.endswith("At")]:
            t[col] = para_brasilia(t[col])
        out[nome] = t
        registrar(nome, n, len(t), "duplicatas por Id")

    TRUSTED.mkdir(parents=True, exist_ok=True)
    for nome, df in out.items():
        df.to_csv(TRUSTED / f"{nome}.csv", index=False, date_format="%Y-%m-%d %H:%M:%S")
    return out


# ---------------------------------------------------------- trusted -> refined
def construir_refined(t: dict[str, pd.DataFrame]) -> None:
    sensores, enderecos = t["sensors"], t["addresses"]
    eventos, certs = t["flood_events"].copy(), t["flood_certificates"].copy()

    # DimSensor: uma linha por rua monitorada
    dim_sensor = sensores.rename(columns={
        "Id": "SensorId", "Name": "Rua", "Neighborhood": "Bairro", "Zone": "Zona",
        "Latitude": "Latitude", "Longitude": "Longitude"})

    # DimEndereco
    dim_endereco = enderecos.rename(columns={
        "Id": "EnderecoId", "Street": "Rua", "Number": "Numero",
        "Neighborhood": "Bairro", "City": "Cidade"})
    dim_endereco["Endereco"] = dim_endereco["Rua"] + ", " + dim_endereco["Numero"]

    # FatoEventos: uma linha por evento, já com as métricas derivadas
    por_evento = certs.groupby("FloodEventId").agg(
        EnderecosAtingidos=("AddressId", "nunique"),
        EnderecosElegiveis=("AddressId", lambda x: x[certs.loc[x.index, "IsEligibleForAid"]].nunique()))
    f = eventos.rename(columns={
        "Id": "EventoId", "PeakWaterLevelCm": "PicoCm", "StartedAt": "InicioEm",
        "ConfirmedAt": "ConfirmadoEm", "ClosedAt": "EncerradoEm", "DismissedAt": "DescartadoEm"})
    f = f.merge(por_evento, left_on="EventoId", right_index=True, how="left")
    f[["EnderecosAtingidos", "EnderecosElegiveis"]] = f[["EnderecosAtingidos", "EnderecosElegiveis"]].fillna(0).astype(int)
    f["DataEvento"] = f["InicioEm"].dt.date
    f["FoiConfirmado"] = f["Status"].isin(["Confirmed", "Closed"]).astype(int)
    f["FoiDescartado"] = (f["Status"] == "Dismissed").astype(int)
    f["SegundosAteConfirmacao"] = (f["ConfirmadoEm"] - f["InicioEm"]).dt.total_seconds().round(2)
    f["DuracaoSegundos"] = (f["EncerradoEm"] - f["InicioEm"]).dt.total_seconds().round(2)
    f["MinutosAteConfirmacao"] = (f["SegundosAteConfirmacao"] / 60).round(4)
    f["DuracaoMinutos"] = (f["DuracaoSegundos"] / 60).round(4)

    # FatoComprovacoes: uma linha por endereço atingido em cada evento
    fc = certs.rename(columns={
        "Id": "ComprovacaoId", "FloodEventId": "EventoId", "AddressId": "EnderecoId",
        "VerificationCode": "Codigo", "PeakWaterLevelCm": "PicoCm", "FloodStartedAt": "InicioEm",
        "FloodClosedAt": "EncerradoEm", "IssuedAt": "EmitidoEm"})
    fc["ElegivelAuxilio"] = fc["IsEligibleForAid"].astype(int)
    fc = fc.drop(columns="IsEligibleForAid")
    fc["DataEvento"] = fc["InicioEm"].dt.date

    # DimCalendario: um dia por linha, cobrindo o período dos eventos
    ini, fim = pd.Timestamp(f["DataEvento"].min()), pd.Timestamp(f["DataEvento"].max())
    dias = pd.date_range(ini, fim, freq="D")
    nomes_mes = ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho",
                 "agosto", "setembro", "outubro", "novembro", "dezembro"]
    nomes_dia = ["segunda", "terça", "quarta", "quinta", "sexta", "sábado", "domingo"]
    cal = pd.DataFrame({"Data": dias})
    cal["Ano"] = cal["Data"].dt.year
    cal["MesNumero"] = cal["Data"].dt.month
    cal["Mes"] = cal["MesNumero"].map(lambda m: nomes_mes[m - 1])
    cal["Dia"] = cal["Data"].dt.day
    cal["DiaSemanaNumero"] = cal["Data"].dt.dayofweek
    cal["DiaSemana"] = cal["DiaSemanaNumero"].map(lambda d: nomes_dia[d])

    REFINED.mkdir(parents=True, exist_ok=True)
    fmt = {"date_format": "%Y-%m-%d %H:%M:%S", "index": False}
    f.to_csv(REFINED / "fato_eventos.csv", **fmt)
    fc.to_csv(REFINED / "fato_comprovacoes.csv", **fmt)
    dim_sensor.to_csv(REFINED / "dim_sensor.csv", **fmt)
    dim_endereco.to_csv(REFINED / "dim_endereco.csv", **fmt)
    cal.to_csv(REFINED / "dim_calendario.csv", date_format="%Y-%m-%d", index=False)


if __name__ == "__main__":
    trusted = construir_trusted()
    construir_refined(trusted)
    rel = pd.DataFrame(relatorio)
    rel.to_csv(BASE / "relatorio_qualidade.csv", index=False)
    print(rel.to_string(index=False))
    print(f"\ntrusted: {TRUSTED}\nrefined: {REFINED}")
