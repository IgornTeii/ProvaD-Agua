import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { api, type FloodEvent } from "../api";
import { useAsync } from "../useAsync";
import { formatCm, formatDateTime } from "../format";
import { EVENT_STATUS, type Tone } from "../status";
import CityMap, { type MapPoint } from "../components/CityMap";
import Legend from "../components/Legend";
import { Empty, ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";

const today = () => new Date().toLocaleDateString("en-CA", { timeZone: "America/Sao_Paulo" });

const daysAgo = (days: number) => {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toLocaleDateString("en-CA", { timeZone: "America/Sao_Paulo" });
};

function worstTone(events: FloodEvent[]): Tone {
  if (events.some((e) => e.status === "Closed" || e.status === "Confirmed")) return "danger";
  if (events.some((e) => e.status === "Suspected")) return "warning";
  return "neutral";
}

function toPoints(events: FloodEvent[]): MapPoint[] {
  const bySensor = new Map<string, FloodEvent[]>();
  for (const event of events) bySensor.set(event.sensorId, [...(bySensor.get(event.sensorId) ?? []), event]);

  return [...bySensor.values()].map((group) => {
    const floods = group.filter((e) => e.status === "Closed" || e.status === "Confirmed").length;
    const first = group[0];

    return {
      id: first.sensorId,
      latitude: first.latitude,
      longitude: first.longitude,
      tone: worstTone(group),
      label: `${first.sensorName} · ${floods} alagamento(s)`,
      popup: (
        <div className="popup">
          <strong>{first.sensorName}</strong>
          <span>{first.neighborhood}</span>
          <span>{floods} alagamento(s) e {group.length - floods} suspeita(s) no período</span>
        </div>
      ),
    };
  });
}

export default function HistoryPage() {
  const [form, setForm] = useState({ from: daysAgo(7), to: today(), street: "" });
  const [filter, setFilter] = useState(form);

  const events = useAsync(() => api.getFloodEvents(filter), [filter]);

  const submit = (e: FormEvent) => {
    e.preventDefault();
    setFilter(form);
  };

  return (
    <section className="wide">
      <PageHeader
        title="Histórico"
        description="Veja onde alagou em um dia, num período ou numa rua. Toque em um alagamento para pegar as comprovações dos endereços atingidos."
        tip="Para ver um único dia, coloque a mesma data em De e Até."
      />

      <form className="filters" onSubmit={submit}>
        <label className="field">
          <span>De</span>
          <input type="date" value={form.from} onChange={(e) => setForm({ ...form, from: e.target.value })} />
        </label>
        <label className="field">
          <span>Até</span>
          <input type="date" value={form.to} onChange={(e) => setForm({ ...form, to: e.target.value })} />
        </label>
        <label className="field field-grow">
          <span>Rua ou bairro (opcional)</span>
          <input
            type="search"
            value={form.street}
            placeholder="Ex.: Pompeia"
            onChange={(e) => setForm({ ...form, street: e.target.value })}
          />
        </label>
        <button type="submit" className="button">Buscar</button>
      </form>

      <Legend
        items={[
          { tone: "danger", label: "Alagou" },
          { tone: "warning", label: "Suspeita em análise" },
          { tone: "neutral", label: "Só suspeitas descartadas" },
        ]}
      />

      <CityMap points={toPoints(events.data ?? [])} height="45vh" />

      {events.loading && <Loading />}
      {events.error && <ErrorMessage text="Não foi possível buscar o histórico." />}
      {events.data?.length === 0 && <Empty text="Nenhum alagamento ou suspeita encontrado com esses filtros." />}

      <ul className="list">
        {events.data?.map((event) => (
          <li key={event.id}>
            <Link to={`/alagamento/${event.id}`} className={`list-item tone-${EVENT_STATUS[event.status].tone}`}>
              <strong>{event.sensorName} · {EVENT_STATUS[event.status].label}</strong>
              <span className="muted">
                {event.neighborhood} · início {formatDateTime(event.startedAt)}
                {event.closedAt && ` · fim ${formatDateTime(event.closedAt)}`} · pico {formatCm(event.peakWaterLevelCm)}
              </span>
              {event.status === "Closed" && <span className="link-hint">Ver as comprovações →</span>}
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}
