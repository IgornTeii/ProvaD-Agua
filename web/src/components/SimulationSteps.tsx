import { Link } from "react-router-dom";
import { api, type Certificate, type FloodEvent } from "../api";
import type { Simulation } from "../simulatorApi";
import { useAsync } from "../useAsync";

type StepState = "done" | "active" | "pending" | "skipped" | "rejected";

interface Step {
  title: string;
  state: StepState;
  detail?: string;
}

interface Outcome {
  event: FloodEvent | null;
  addressId: string | null;
  certificates: Certificate[];
}

async function loadOutcome(simulation: Simulation): Promise<Outcome> {
  const startedAt = Date.parse(simulation.startedAt) - 1000;
  const events = await api.getFloodEvents({ sensorId: simulation.sensorId });
  const event = events.find((e) => Date.parse(e.startedAt) >= startedAt) ?? null;

  const addresses = (await api.searchAddresses()).filter((a) => a.sensorId === simulation.sensorId);
  const addressId = addresses[0]?.id ?? null;

  const certificates =
    event?.status === "Closed" && addressId
      ? await api.getCertificates({ addressId, floodEventId: event.id })
      : [];

  return { event, addressId, certificates };
}

function buildSteps(simulation: Simulation, outcome: Outcome | null): Step[] {
  const running = simulation.status === "running";
  const event = outcome?.event ?? null;
  const status = event?.status;
  const dismissed = status === "Dismissed";

  const readingsStep: Step = {
    title: "O sensor está enviando leituras",
    state: running ? "active" : "done",
    detail: `${simulation.sent} de ${simulation.total} leituras enviadas`,
  };

  const suspicionStep: Step = event
    ? { title: "A água passou do nível de suspeita", state: "done", detail: "Uma suspeita de alagamento foi aberta." }
    : running
      ? { title: "A água passou do nível de suspeita", state: "pending" }
      : { title: "A água não chegou ao nível de suspeita", state: "skipped", detail: "Nenhuma suspeita foi aberta. Tudo normal." };

  const aiStep: Step =
    status === "Suspected"
      ? { title: "A IA está analisando", state: "active", detail: "Ela olha como a água se comporta, não só a altura." }
      : status === "Confirmed" || status === "Closed"
        ? { title: "A IA confirmou: é alagamento", state: "done" }
        : dismissed
          ? { title: "A IA descartou: não era alagamento", state: "rejected", detail: "A suspeita foi encerrada sem gerar comprovação." }
          : { title: "A IA analisa a suspeita", state: event || running ? "pending" : "skipped" };

  const closeStep: Step =
    status === "Closed"
      ? { title: "A água baixou e o alagamento foi encerrado", state: "done" }
      : status === "Confirmed"
        ? { title: "Aguardando a água baixar", state: "active" }
        : { title: "A água baixa e o alagamento é encerrado", state: dismissed || (!event && !running) ? "skipped" : "pending" };

  const count = outcome?.certificates.length ?? 0;
  const certificatesStep: Step =
    count > 0
      ? { title: "Comprovações emitidas", state: "done", detail: "Os moradores da rua já podem consultar." }
      : status === "Closed"
        ? { title: "Emitindo as comprovações", state: "active" }
        : { title: "As comprovações são emitidas", state: dismissed || (!event && !running) ? "skipped" : "pending" };

  return [readingsStep, suspicionStep, aiStep, closeStep, certificatesStep];
}

export default function SimulationSteps({ simulation }: { simulation: Simulation }) {
  const outcome = useAsync(() => loadOutcome(simulation), [simulation.id], 2000);
  const steps = buildSteps(simulation, outcome.data);
  const certificate = outcome.data?.certificates[0];

  return (
    <div>
      <ol className="steps">
        {steps.map((step) => (
          <li key={step.title} className={`step step-${step.state}`}>
            <span className="step-dot" aria-hidden="true" />
            <div>
              <strong>{step.title}</strong>
              {step.detail && <p className="small">{step.detail}</p>}
            </div>
          </li>
        ))}
      </ol>

      {certificate && outcome.data?.addressId && (
        <div className="actions">
          <Link to={`/verificar/${certificate.verificationCode}`} className="button">
            Ver comprovação {certificate.verificationCode}
          </Link>
          <Link to={`/endereco/${outcome.data.addressId}`} className="button button-secondary">
            Ver a rua
          </Link>
        </div>
      )}
    </div>
  );
}
