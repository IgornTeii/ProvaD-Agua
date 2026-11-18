# Dashboard Prova d'Água — resumo para continuar

Entrega: dashboard Power BI para a Defesa Civil (GS 2026/2, FIAP, CitizenTech). Pergunta do dash: **"o que aconteceu na cidade?"**

## 1. Estado atual

| Item | Situação |
|---|---|
| Docker (db, ai, api, simulator, web) | Funcionando. Postgres em **localhost:5433** (a 5432 estava ocupada por outro container). `docker-compose.override.yml` na raiz do repo faz o remapeamento; o compose original não foi alterado |
| Dados | Gerados pelo simulador (3 rodadas, 6 cenários, 8 sensores) e **redistribuídos no tempo no banco** (04 a 17/11/2026), com 8 eventos de pico reduzido (18–28 cm) para haver endereços não elegíveis. **É dado sintético** e isso deve constar no PDF |
| raw | `dashboard/data/raw/*.csv` — 7 tabelas exportadas do Postgres, sem alteração |
| trusted / refined | Gerados por `dashboard/pipeline.py` (`cd dashboard` e `python pipeline.py`) |
| Power BI | Modelo montado, medidas criadas, **página 1 pronta (falta polimento final)**. Salvar em `dashboard/prova-dagua.pbix` |
| Pendente | Ver seção 6 |

Totais conferidos com os CSVs: 30 eventos (16 confirmados, 14 descartados), taxa de descarte 46,7%, 48 comprovações, 24 endereços atingidos, 18 elegíveis, tempo médio até confirmação 3,17 s, duração média 28,24 s.

## 2. Fluxo de dados (texto-base para o PDF)

Postgres (sistema real) → **raw**: CSVs idênticos às tabelas, sem alteração → **trusted** (`pipeline.py`): tipos corretos, datas de UTC para Brasília (America/Sao_Paulo), sem duplicatas (por Id e por código de comprovação), sem leituras impossíveis (nível fora de 0–300 cm, chuva fora de 0–300 mm/h); relatório de qualidade em `data/relatorio_qualidade.csv` (neste conjunto nada precisou ser removido) → **refined** (`pipeline.py`): modelo em estrela com `fato_eventos`, `fato_comprovacoes`, `dim_sensor`, `dim_endereco`, `dim_calendario`, com colunas derivadas (flags, tempos, endereços atingidos/elegíveis) → Power BI lê só a refined.

Decisão: pipeline em Python (visto desde o 1º ano), pois transformar antes do BI deixa o dash mais leve e o DAX só agrega. Power Query fica restrito a tipos.

## 3. Modelo e Power Query

Importar só os 5 CSVs de `dashboard/data/refined`. **Armadilha de localidade:** o Power BI em pt-BR lê `47.6` como 476. Colunas decimais devem usar **Alterar tipo → Usando localidade → Inglês (EUA)**: `PicoCm` (nas duas fatos), `Latitude`, `Longitude`, e os quatro tempos de `fato_eventos` (`SegundosAteConfirmacao`, `DuracaoSegundos`, `MinutosAteConfirmacao`, `DuracaoMinutos`) como `type number` (não inteiro). Desligar a detecção automática de tipos em Arquivo → Opções.

Relacionamentos (1 → *, direção única):
- `dim_sensor[SensorId]` → `fato_eventos[SensorId]`
- `dim_calendario[Data]` → `fato_eventos[DataEvento]` (marcar como tabela de datas)
- `fato_eventos[EventoId]` → `fato_comprovacoes[EventoId]`
- `dim_endereco[EnderecoId]` → `fato_comprovacoes[EnderecoId]`

**Não** relacionar `dim_endereco[SensorId]` com `dim_sensor` (criaria dois caminhos). Segmentações de rua/bairro/zona sempre de `dim_sensor`. `Latitude` e `Longitude` de `dim_sensor` com categoria de dados correspondente (só habilita se forem número decimal).

## 4. Medidas DAX (tabela `_Medidas`, criada vazia no Power Query; medidas via Nova medida)

```dax
Alagamentos Confirmados = SUM(fato_eventos[FoiConfirmado])
Falsos Alarmes Evitados = SUM(fato_eventos[FoiDescartado])
Taxa de Descarte % = DIVIDE([Falsos Alarmes Evitados], [Falsos Alarmes Evitados] + [Alagamentos Confirmados])
Endereços Atingidos = DISTINCTCOUNT(fato_comprovacoes[EnderecoId])
Endereços Elegíveis = CALCULATE(DISTINCTCOUNT(fato_comprovacoes[EnderecoId]), fato_comprovacoes[ElegivelAuxilio] = 1)
Tempo Médio até Confirmação (s) = AVERAGE(fato_eventos[SegundosAteConfirmacao])
Duração Média (s) = AVERAGE(fato_eventos[DuracaoSegundos])
Pico Máximo (cm) = CALCULATE(MAX(fato_eventos[PicoCm]), fato_eventos[FoiConfirmado] = 1)
Confirmados Período Anterior =
VAR dias = DATEDIFF(MIN(dim_calendario[Data]), MAX(dim_calendario[Data]), DAY) + 1
RETURN CALCULATE([Alagamentos Confirmados], DATEADD(dim_calendario[Data], -dias, DAY))
Variação vs Período Anterior % = DIVIDE([Alagamentos Confirmados] - [Confirmados Período Anterior], [Confirmados Período Anterior])
```

Notas: `Taxa de Descarte %` formatada como porcentagem com 1 casa. Tempos em segundos porque o simulador comprime o tempo (valores em minutos ficariam 0,05 e 0,47). A comparação com o período anterior só funciona com período selecionado (ex.: 11 a 17/11 contra 04 a 10/11); com o intervalo todo fica em branco. Medidas opcionais para o gráfico da página 1 (escondem zeros): `Confirmados (gráfico)` e `Falsos alarmes (gráfico)` com `IF(v = 0, BLANK(), v)`.

## 5. Design (padrão a seguir em todas as páginas)

Baseado no site do projeto. Tema importável: `dashboard/tema-prova-dagua.json` (Exibir → Temas → Procurar temas). Fonte: **Segoe UI** (a Atkinson Hyperlegible do site não vem no Power BI).

**Página:** personalizada **1920 × 1080**. Margem 30 px, espaço entre blocos 18 px.

**Cores:** marca `#0a2540` (cabeçalho, valores), `#0f4c81`, `#1565c0` (azul principal), `#5cc8ff` (ciano, subtítulo), fundo da página `#f6f8fb`, visual branco `#ffffff`, borda `#d6dfe9`, texto `#142334`, texto secundário `#4a5a6c`, hover/selecionado claro `#e6f0fb`.
**Significado das cores:** azul = alagamento real e impacto (confirmados `#1565c0`, atingidos `#0f4c81`); âmbar `#d99a00` = trabalho da IA (falsos alarmes, taxa de descarte); verde `#1b6e3a` = elegível a auxílio; vermelho `#b0191f` só para alertas e extremos.

**Textos:** título "Prova d'Água" 32 pt branco; subtítulo 16 pt `#5cc8ff`; valor dos cartões 44 pt `#0a2540`; rótulo dos cartões 15 pt `#4a5a6c`; título de visual 20 pt; eixos e legendas 15 pt `#4a5a6c`; cabeçalho de segmentação 14 pt Semibold `#4a5a6c`; valores de segmentação 15 pt `#142334`.

**Cabeçalho:** retângulo `#0a2540` (0,0, 1920 × 105), título em X30 Y12, subtítulo em X30 Y62.
**Segmentações** (Y 123, altura 84): Período X30 L300 (Entre); Zona X348 L280; Bairro X646 L400; Rua X1064 L510; Status X1592 L298 (as quatro últimas, lista suspensa).
**Cartões** (Y 225, altura 150): X30, 406, 782, 1158 (L358) e 1534 (L356). Barra de destaque lateral na cor do significado, alinhados à esquerda, rótulos em minúsculas ("Alagamentos confirmados"), via "Renomear para este visual".
**Gráfico da página 1** (X30 Y393 L1860 A657): colunas agrupadas por dia, confirmados azul e falsos alarmes âmbar, rótulos de dados ligados, só grade horizontal `#d6dfe9`, eixo X categórico.
**Sombra** nos cartões e gráfico: cor `#0a2540`, transparência 92%, desfoque 16, distância 4, ângulo 90°, tamanho 0 (segmentações: 95%, desfoque 12, distância 3).
Dica: formatar um visual e copiar com o Pincel de formatação. Duplicar a página 1 como base das outras (cabeçalho e segmentações vêm junto).

## 6. O que falta fazer

**Página 1 (acabamento):** desligar o subtítulo automático do gráfico e renomear legenda e cartões ("Falsos alarmes evitados", "Taxa de descarte", "Endereços atingidos/elegíveis"); título "Período" com mesma fonte das outras segmentações; texto da Zona igual ao das outras; opcional usar medidas sem zero no gráfico.

**Página 2 — Onde:** mapa dos 8 pontos (`dim_sensor` Latitude/Longitude, tamanho = Alagamentos Confirmados, dica com Rua e Bairro; se o visual de mapa estiver bloqueado, usar dispersão Longitude × Latitude); barras de confirmados por Bairro e por Zona; barras de Pico Máximo (cm) por Rua.
**Página 3 — IA e tempos:** cartões Tempo Médio até Confirmação (s) e Duração Média (s); rosca Confirmados vs Falsos Alarmes; Confirmados Período Anterior e Variação %.
**Página 4 — Auxílio:** cartões Atingidos e Elegíveis; tabela de comprovações (Código PDA, Endereço, Pico, Elegível).

**Geral:**
- Sincronizar as segmentações em todas as páginas (Exibir → Sincronizar segmentações).
- Opcional: traduzir `Status` para português (Encerrado, Descartado, Confirmado) no `pipeline.py`, hoje está em inglês.
- Salvar o `.pbix` em `dashboard/prova-dagua.pbix`.
- Prints PNG de cada página em `dashboard/prints`.
- Texto para o PDF: cada medida (nome, fórmula, o que responde) e o parágrafo do fluxo raw → trusted → refined (base na seção 2). Mencionar que os dados são sintéticos e redistribuídos no tempo, e que a limpeza não removeu linhas porque o simulador já gera dados limpos.
- Nada foi commitado no repositório. `docker-compose.override.yml`, `dashboard/` e os CSVs estão só locais.
- Backup do banco antes do ajuste (temporário, copie se quiser guardar): `C:\Users\cayom\AppData\Local\Temp\claude\59060a8e49034419\aa947f03-df06-46c9-889f-b73ab1581bae\scratchpad\backup_antes_ajuste.sql`.

## 7. Como subir o ambiente de novo

```bash
cd ProvaD-Agua
docker compose up -d        # o override mantém o Postgres na porta 5433
cd dashboard
python pipeline.py          # regenera trusted e refined a partir da raw
```
Se regerar dados no simulador, reexportar a raw antes do pipeline. A raw atual reflete o banco já ajustado.
