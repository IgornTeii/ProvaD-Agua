import type { FloodEventStatus, SensorStatusName } from "./api";

export type Tone = "ok" | "warning" | "danger" | "neutral";

export const TONE_COLOR: Record<Tone, string> = {
  ok: "#1b6e3a",
  warning: "#d99a00",
  danger: "#c0262c",
  neutral: "#6b7a8c",
};

export const SENSOR_STATUS: Record<SensorStatusName, { tone: Tone; label: string }> = {
  Normal: { tone: "ok", label: "Normal" },
  Suspected: { tone: "warning", label: "Suspeita em análise" },
  Confirmed: { tone: "danger", label: "Alagamento confirmado" },
};

export const EVENT_STATUS: Record<FloodEventStatus, { tone: Tone; label: string }> = {
  Suspected: { tone: "warning", label: "Suspeita em análise" },
  Confirmed: { tone: "danger", label: "Alagamento em andamento" },
  Closed: { tone: "danger", label: "Alagou" },
  Dismissed: { tone: "neutral", label: "Suspeita descartada" },
};
