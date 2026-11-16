import { Link, useParams } from "react-router-dom";
import { api, type FloodEventStatus, type SensorStatus } from "../api";
import { useAsync } from "../useAsync";
import { useAddress } from "../components/useAddress";
import { formatCm, formatDateTime, formatRain } from "../format";
import { Empty, ErrorMessage, Loading } from "../components/States";
import AlertSignup from "../components/AlertSignup";
import SpeakButton from "../components/SpeakButton";
import { CircleCheck, FileCheck, Phone, TriangleAlert } from "lucide-react";

const REFRESH_MS = 10_000;

const EVENT_LABEL: Record<FloodEventStatus, string> = {
  Suspected: "Em análise",
  Confirmed: "Alagamento confirmado",
  Closed: "Encerrado",
  Dismissed: "Descartado (não era alagamento)",
};

function situation(status: SensorStatus) {
  if (status.openEvent?.status === "Confirmed")
    return { tone: "danger", title: "Alagamento confirmado", text: "A água subiu na sua rua. Evite sair e proteja seus bens." };
  if (status.openEvent?.status === "Suspected")
    return { tone: "warning", title: "Em análise", text: "O nível da água subiu. Estamos verificando se é um alagamento real." };
  return { tone: "ok", title: "Sem alagamento", text: "O nível da água na sua rua está normal." };
}

const spokenSummary = (title: string, text: string, status: SensorStatus) => {
  const reading = status.latestReading;
  const levels = reading
    ? ` Água na rua: ${formatCm(reading.waterLevelCm).replace("cm", "centímetros")}. Chuva: ${formatRain(reading.rainfallMmPerHour).replace("mm/h", "milímetros por hora")}.`
    : "";
  return `${title}. ${text}${levels}`;
};

export default function AddressPage() {
  const { addressId } = useParams();
  const address = useAddress(addressId);
  const sensorId = address.data?.sensorId;

  const status = useAsync(
    () => (sensorId ? api.getSensorStatus(sensorId) : Promise.resolve(null)),
    [sensorId],
    REFRESH_MS,
  );
  const events = useAsync(
    () => (sensorId ? api.getFloodEvents({ sensorId }) : Promise.resolve([])),
    [sensorId],
    REFRESH_MS,
  );

  if (address.loading && !address.data) return <Loading />;
  if (address.error) return <ErrorMessage text="Não foi possível carregar o endereço." />;
  if (!address.data) return <ErrorMessage text="Endereço não encontrado." />;

  const now = status.data ? situation(status.data) : null;
  const history = (events.data ?? []).filter((e) => e.status === "Confirmed" || e.status === "Closed");

  return (
    <section>
      <Link to="/minha-rua" className="back">← Trocar endereço</Link>
      <h1>{address.data.street}, {address.data.number}</h1>
      <p className="muted">{address.data.neighborhood} · {address.data.city}</p>

      {status.error && <ErrorMessage text="Sem conexão com o sensor no momento." />}

      {now && status.data && (
        <div className={`status status-${now.tone}`} role="status" aria-live="polite">
          <p className="status-title">
            {now.tone === "ok" ? <CircleCheck size={30} aria-hidden="true" /> : <TriangleAlert size={30} aria-hidden="true" />}
            {now.title}
          </p>
          <p>{now.text}</p>

          {status.data.latestReading && (
            <dl className="metrics">
              <div>
                <dt>Água na rua</dt>
                <dd>{formatCm(status.data.latestReading.waterLevelCm)}</dd>
              </div>
              <div>
                <dt>Chuva</dt>
                <dd>{formatRain(status.data.latestReading.rainfallMmPerHour)}</dd>
              </div>
            </dl>
          )}
          <p className="small">
            Última medição: {formatDateTime(status.data.latestReading?.measuredAt ?? null)} · atualiza sozinho
          </p>
          <SpeakButton text={spokenSummary(now.title, now.text, status.data)} />
        </div>
      )}

      {now?.tone === "danger" && (
        <div className="guidance">
          <h2>O que fazer agora</h2>
          <ul>
            <li>Se a água entrar em casa, desligue a energia no disjuntor geral.</li>
            <li>Leve documentos, remédios e pessoas idosas ou com crianças para um lugar alto.</li>
            <li>Não atravesse ruas alagadas a pé nem de carro.</li>
            <li>Tire fotos dos danos: elas complementam a sua comprovação.</li>
          </ul>
          <a href="tel:199" className="button"><Phone size={20} aria-hidden="true" /> Ligar para a Defesa Civil (199)</a>
        </div>
      )}

      <Link to={`/endereco/${address.data.id}/comprovacoes`} className="button">
        <FileCheck size={20} aria-hidden="true" /> Ver minhas comprovações
      </Link>

      <AlertSignup addressId={address.data.id} />

      <h2>Alagamentos registrados nesta rua</h2>
      {events.loading && !events.data && <Loading />}
      {history.length === 0 && !events.loading && <Empty text="Nenhum alagamento registrado até agora." />}

      <ul className="list">
        {history.map((event) => (
          <li key={event.id} className="list-item">
            <strong>{EVENT_LABEL[event.status]}</strong>
            <span className="muted">
              Início {formatDateTime(event.startedAt)}
              {event.closedAt && ` · fim ${formatDateTime(event.closedAt)}`}
              {` · pico ${formatCm(event.peakWaterLevelCm)}`}
            </span>
          </li>
        ))}
      </ul>
    </section>
  );
}
