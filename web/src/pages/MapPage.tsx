import { Link } from "react-router-dom";
import { api } from "../api";
import { useAsync } from "../useAsync";
import { formatCm, formatRain } from "../format";
import { SENSOR_STATUS } from "../status";
import CityMap from "../components/CityMap";
import Legend from "../components/Legend";
import { ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";

export default function MapPage() {
  const sensors = useAsync(api.getSensors, [], 3000);

  if (sensors.loading && !sensors.data) return <Loading />;
  if (sensors.error && !sensors.data) return <ErrorMessage text="Não foi possível carregar o mapa agora." />;

  const list = sensors.data ?? [];
  const alerts = list.filter((s) => s.status !== "Normal");

  return (
    <section className="wide">
      <PageHeader
        title="Mapa agora"
        description="Pontos de São Paulo que costumam alagar, monitorados por sensores de nível de água e de chuva. O mapa se atualiza sozinho."
        tip="Toque em um ponto para ver a altura da água e a chuva naquele momento."
      />

      <Legend items={Object.values(SENSOR_STATUS)} />

      <CityMap
        points={list.map((sensor) => ({
          id: sensor.id,
          latitude: sensor.latitude,
          longitude: sensor.longitude,
          tone: SENSOR_STATUS[sensor.status].tone,
          label: `${sensor.name} · ${SENSOR_STATUS[sensor.status].label}`,
          popup: (
            <div className="popup">
              <strong>{sensor.name}</strong>
              <span>{sensor.neighborhood} · zona {sensor.zone}</span>
              <span>{SENSOR_STATUS[sensor.status].label}</span>
              {sensor.latestReading && (
                <span>
                  Água {formatCm(sensor.latestReading.waterLevelCm)} · chuva {formatRain(sensor.latestReading.rainfallMmPerHour)}
                </span>
              )}
            </div>
          ),
        }))}
      />

      <h2>{alerts.length ? "Atenção agora" : "Nenhum alagamento agora"}</h2>
      {alerts.length === 0 && <p className="muted">Todos os pontos monitorados estão normais.</p>}

      <ul className="list">
        {alerts.map((sensor) => (
          <li key={sensor.id} className={`list-item tone-${SENSOR_STATUS[sensor.status].tone}`}>
            <strong>{sensor.name}</strong>
            <span className="muted">{sensor.neighborhood} · {SENSOR_STATUS[sensor.status].label}</span>
          </li>
        ))}
      </ul>

      <div className="actions">
        <Link to="/minha-rua" className="button">Ver a minha rua</Link>
        <Link to="/historico" className="button button-secondary">Ver o histórico</Link>
      </div>
    </section>
  );
}
