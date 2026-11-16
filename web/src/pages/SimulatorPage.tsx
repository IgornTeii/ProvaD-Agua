import { useState } from "react";
import { api } from "../api";
import { simulator, type Scenario, type Simulation } from "../simulatorApi";
import { useAsync } from "../useAsync";
import { SENSOR_STATUS } from "../status";
import CityMap from "../components/CityMap";
import Legend from "../components/Legend";
import WaterChart from "../components/WaterChart";
import SimulationSteps from "../components/SimulationSteps";
import { ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";

export default function SimulatorPage() {
  const scenarios = useAsync(simulator.scenarios, []);
  const sensors = useAsync(api.getSensors, [], 3000);
  const [sensorId, setSensorId] = useState("sensor-01");
  const [current, setCurrent] = useState<Simulation | null>(null);
  const [startError, setStartError] = useState<string | null>(null);

  const live = useAsync(
    () => (current ? simulator.get(current.id) : Promise.resolve(null)),
    [current?.id],
    current ? 700 : undefined,
  );

  const simulation = current && live.data?.id === current.id ? live.data : current;
  const running = simulation?.status === "running";
  const scenario = scenarios.data?.find((s) => s.id === simulation?.scenarioId);
  const selected = sensors.data?.find((s) => s.id === (simulation?.sensorId ?? sensorId));

  const start = async (chosen: Scenario) => {
    setStartError(null);
    try {
      setCurrent(await simulator.start(chosen.id, sensorId));
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (error) {
      setStartError((error as Error).message);
    }
  };

  if (scenarios.error) return <ErrorMessage text="O simulador não está respondendo. Confira se ele está rodando (docker compose up)." />;
  if (sensors.error && !sensors.data) return <ErrorMessage text="A API não está respondendo. Confira se ela está rodando." />;
  if (!scenarios.data || !sensors.data) return <Loading />;

  return (
    <section className="wide simulator">
      <PageHeader
        title="Simulador"
        description="Ferramenta de demonstração: um sensor de mentira envia leituras para o sistema de verdade, e você acompanha cada decisão acontecendo."
        tip="Escolha um ponto no mapa e depois uma situação. As cores do mapa mudam enquanto a simulação acontece."
      />

      <div className="simulator-grid">
        <div>
          <Legend items={Object.values(SENSOR_STATUS)} />
          <CityMap
            height="42vh"
            points={sensors.data.map((sensor) => ({
              id: sensor.id,
              latitude: sensor.latitude,
              longitude: sensor.longitude,
              tone: SENSOR_STATUS[sensor.status].tone,
              label: sensor.name,
              selected: sensor.id === selected?.id,
              onSelect: running ? undefined : () => setSensorId(sensor.id),
            }))}
          />

          <label className="field">
            <span>Ponto escolhido</span>
            <select value={sensorId} disabled={running} onChange={(e) => setSensorId(e.target.value)}>
              {sensors.data.map((sensor) => (
                <option key={sensor.id} value={sensor.id}>
                  {sensor.name} · {sensor.neighborhood}
                </option>
              ))}
            </select>
          </label>
        </div>

        {simulation && scenario ? (
          <article className="panel">
            <header className="panel-header">
              <div>
                <p className="small">Simulando em {selected?.name} · {selected?.neighborhood}</p>
                <h2>{scenario.name}</h2>
              </div>
              {!running && (
                <button type="button" className="button button-secondary" onClick={() => setCurrent(null)}>
                  Nova simulação
                </button>
              )}
            </header>

            <div className="progress" aria-label="Progresso da simulação">
              <span style={{ width: `${(simulation.sent / simulation.total) * 100}%` }} />
            </div>

            {simulation.rejected > 0 && (
              <ErrorMessage text={`A API recusou ${simulation.rejected} leitura(s). Último erro: ${simulation.lastError}`} />
            )}

            <WaterChart readings={simulation.readings} total={simulation.total} />
            <SimulationSteps simulation={simulation} />
          </article>
        ) : (
          <div className="panel panel-hint">
            <h2>{selected?.name}</h2>
            <p className="muted">{selected?.neighborhood} · zona {selected?.zone}</p>
            <p>Clique em um ponto no mapa para trocar o local, depois escolha uma situação abaixo.</p>
          </div>
        )}
      </div>

      {!running && (
        <>
          <h2>O que vai acontecer em {selected?.name}?</h2>
          {startError && <ErrorMessage text={startError} />}

          <div className="cards">
            {scenarios.data.map((s) => (
              <article key={s.id} className="card">
                <h3>{s.name}</h3>
                <p>{s.description}</p>
                <p className="expected">
                  <strong>O que deve acontecer:</strong> {s.expected}
                </p>
                <button type="button" className="button" onClick={() => start(s)}>
                  Simular
                </button>
              </article>
            ))}
          </div>
        </>
      )}
    </section>
  );
}
