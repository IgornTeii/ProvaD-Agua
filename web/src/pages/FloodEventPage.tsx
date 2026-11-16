import { Link, useParams } from "react-router-dom";
import { api, NotFoundError } from "../api";
import { useAsync } from "../useAsync";
import { formatCm, formatDateTime } from "../format";
import { EVENT_STATUS } from "../status";
import { Empty, ErrorMessage, Loading } from "../components/States";

export default function FloodEventPage() {
  const { eventId = "" } = useParams();
  const event = useAsync(() => api.getFloodEvent(eventId), [eventId]);
  const certificates = useAsync(() => api.getCertificates({ floodEventId: eventId }), [eventId]);

  if (event.loading && !event.data) return <Loading />;
  if (event.error instanceof NotFoundError) return <ErrorMessage text="Alagamento não encontrado." />;
  if (event.error || !event.data) return <ErrorMessage text="Não foi possível carregar este alagamento." />;

  const e = event.data;
  const status = EVENT_STATUS[e.status];

  return (
    <section>
      <Link to="/historico" className="back">← Voltar ao histórico</Link>
      <h1>{e.sensorName}</h1>
      <p className="muted">{e.neighborhood} · São Paulo</p>

      <div className={`list-item tone-${status.tone}`}>
        <strong>{status.label}</strong>
        <span className="muted">
          Início {formatDateTime(e.startedAt)}
          {e.closedAt && ` · fim ${formatDateTime(e.closedAt)}`} · nível máximo {formatCm(e.peakWaterLevelCm)}
        </span>
      </div>

      {e.status === "Dismissed" && (
        <p className="lead">
          Essa foi uma suspeita que a IA descartou: não era alagamento de verdade, por isso não gera comprovação.
        </p>
      )}

      {(e.status === "Suspected" || e.status === "Confirmed") && (
        <p className="lead">Este alagamento ainda está em andamento. As comprovações saem sozinhas quando a água baixar.</p>
      )}

      {e.status === "Closed" && (
        <>
          <h2>Qual é o seu endereço?</h2>
          <p className="muted">Escolha o seu endereço para ver as suas comprovações.</p>

          {certificates.loading && <Loading />}
          {certificates.error && <ErrorMessage text="Não foi possível carregar as comprovações." />}
          {certificates.data?.length === 0 && <Empty text="As comprovações deste alagamento ainda estão sendo emitidas." />}

          <ul className="list">
            {certificates.data?.map((c) => (
              <li key={c.id}>
                <Link to={`/endereco/${c.address.id}/comprovacoes`} className="list-item">
                  <strong>{c.address.street}, {c.address.number}</strong>
                  <span className={`badge ${c.isEligibleForAid ? "badge-ok" : "badge-neutral"}`}>
                    {c.isEligibleForAid ? "Elegível a auxílio" : "Registrado, sem elegibilidade a auxílio"}
                  </span>
                  <span className="link-hint">Ver minhas comprovações →</span>
                </Link>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
