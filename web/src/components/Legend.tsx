import { TONE_COLOR, type Tone } from "../status";

export default function Legend({ items }: { items: { tone: Tone; label: string }[] }) {
  return (
    <ul className="legend">
      {items.map((item) => (
        <li key={item.label}>
          <span className="legend-dot" style={{ background: TONE_COLOR[item.tone] }} aria-hidden="true" />
          {item.label}
        </li>
      ))}
    </ul>
  );
}
