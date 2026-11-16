import type { SimulatedReading } from "../simulatorApi";
import { formatCm, formatRain } from "../format";

const WIDTH = 320;
const HEIGHT = 120;
const SUSPICION_CM = 15;

export default function WaterChart({ readings, total }: { readings: SimulatedReading[]; total: number }) {
  const maxLevel = Math.max(60, ...readings.map((r) => r.waterLevelCm * 1.1));
  const x = (index: number) => (index / Math.max(total - 1, 1)) * WIDTH;
  const y = (level: number) => HEIGHT - (level / maxLevel) * HEIGHT;

  const line = readings.map((r, i) => `${x(i)},${y(r.waterLevelCm)}`).join(" ");
  const area = readings.length ? `0,${HEIGHT} ${line} ${x(readings.length - 1)},${HEIGHT}` : "";
  const last = readings.at(-1);

  return (
    <figure className="chart">
      <div className="chart-now">
        <span>Água agora <strong>{last ? formatCm(last.waterLevelCm) : "—"}</strong></span>
        <span>Chuva <strong>{last ? formatRain(last.rainfallMmPerHour) : "—"}</strong></span>
      </div>

      <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} role="img" aria-label="Nível da água ao longo da simulação">
        <polygon points={area} className="chart-area" />
        <polyline points={line} className="chart-line" />
        <line x1="0" x2={WIDTH} y1={y(SUSPICION_CM)} y2={y(SUSPICION_CM)} className="chart-threshold" />
      </svg>

      <figcaption className="small">A linha tracejada é o nível de suspeita: {formatCm(SUSPICION_CM)}.</figcaption>
    </figure>
  );
}
