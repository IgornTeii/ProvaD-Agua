import { useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api, NotFoundError } from "../api";
import { useAsync } from "../useAsync";
import { formatCm, formatDateTime } from "../format";
import { ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";
export default function VerifyPage() {
  const { code } = useParams();
  const navigate = useNavigate();
  const [typed, setTyped] = useState(code ?? "");

  const certificate = useAsync(
    () => (code ? api.verifyCertificate(code) : Promise.resolve(null)),
    [code],
  );

  const submit = (e: FormEvent) => {
    e.preventDefault();
    if (typed.trim()) navigate(`/verificar/${typed.trim().toUpperCase()}`);
  };

  const c = certificate.data;

  return (
    <section>
      <div className="no-print">
        <PageHeader
          title="Verificar código"
          description="Para órgãos públicos, seguradoras e o próprio morador: digite o código da comprovação para conferir se ela é autêntica."
          tip="O código fica no topo da comprovação e tem o formato PDA-XXXX-XXXX."
        />

        <form onSubmit={submit} className="inline-form">
          <label className="field">
            <span>Código da comprovação</span>
            <input
              value={typed}
              onChange={(e) => setTyped(e.target.value)}
              placeholder="PDA-XXXX-XXXX"
              autoCapitalize="characters"
              spellCheck={false}
            />
          </label>
          <button type="submit" className="button">Verificar</button>
        </form>
      </div>

      {code && certificate.loading && <Loading text="Verificando..." />}
      {certificate.error instanceof NotFoundError && (
        <ErrorMessage text="Código não encontrado. Confira se foi digitado corretamente." />
      )}
      {certificate.error && !(certificate.error instanceof NotFoundError) && (
        <ErrorMessage text="Não foi possível verificar agora. Tente de novo em instantes." />
      )}

      {c && (
        <article className="certificate">
          <p className="badge badge-ok">✓ Comprovação autêntica</p>
          <h2>Comprovação de alagamento</h2>
          <p className="code code-large">{c.verificationCode}</p>

          <dl className="details">
            <div><dt>Endereço</dt><dd>{c.address.fullAddress}</dd></div>
            <div><dt>Início do alagamento</dt><dd>{formatDateTime(c.floodStartedAt)}</dd></div>
            <div><dt>Fim do alagamento</dt><dd>{formatDateTime(c.floodClosedAt)}</dd></div>
            <div><dt>Nível máximo da água</dt><dd>{formatCm(c.peakWaterLevelCm)}</dd></div>
            <div>
              <dt>Elegibilidade a auxílio</dt>
              <dd>{c.isEligibleForAid ? "Elegível (nível acima do limite da Defesa Civil)" : "Não elegível"}</dd>
            </div>
            <div><dt>Emitida em</dt><dd>{formatDateTime(c.issuedAt)}</dd></div>
          </dl>

          <p className="small">
            Registro automático feito por sensores de nível de água e chuva, com o alagamento confirmado por análise
            de inteligência artificial. Confira a autenticidade em Prova d'Água › Verificar.
          </p>

          <button type="button" className="button no-print" onClick={() => window.print()}>
            Imprimir ou salvar em PDF
          </button>
        </article>
      )}
    </section>
  );
}
