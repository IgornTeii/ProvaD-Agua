import { Link } from "react-router-dom";
import { api } from "../api";
import { useAsync } from "../useAsync";
import { formatCm, formatDateTime } from "../format";
import { Empty, ErrorMessage, Loading } from "./States";

export default function CertificateList({ addressId }: { addressId: string }) {
  const certificates = useAsync(() => api.getCertificates({ addressId }), [addressId]);

  if (certificates.loading && !certificates.data) return <Loading />;
  if (certificates.error) return <ErrorMessage text="Não foi possível carregar as comprovações." />;
  if (certificates.data?.length === 0)
    return <Empty text="Ainda não há comprovações para este endereço. Elas são emitidas automaticamente quando um alagamento termina." />;

  return (
    <ul className="list">
      {certificates.data?.map((c) => (
        <li key={c.id}>
          <Link to={`/verificar/${c.verificationCode}`} className="list-item">
            <span className="code">{c.verificationCode}</span>
            <span className="muted">
              Alagamento de {formatDateTime(c.floodStartedAt)} · pico {formatCm(c.peakWaterLevelCm)}
            </span>
            <span className={`badge ${c.isEligibleForAid ? "badge-ok" : "badge-neutral"}`}>
              {c.isEligibleForAid ? "Elegível a auxílio" : "Registrado, sem elegibilidade a auxílio"}
            </span>
            <span className="link-hint">Abrir comprovação →</span>
          </Link>
        </li>
      ))}
    </ul>
  );
}
