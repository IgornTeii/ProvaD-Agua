const SIMULATOR_URL = import.meta.env.VITE_SIMULATOR_URL ?? "http://localhost:8100";

export interface Scenario {
  id: string;
  name: string;
  description: string;
  expected: string;
}

export interface SimulatedReading {
  waterLevelCm: number;
  rainfallMmPerHour: number;
}

export interface Simulation {
  id: string;
  scenarioId: string;
  sensorId: string;
  total: number;
  startedAt: string;
  status: "running" | "finished" | "failed";
  sent: number;
  rejected: number;
  lastError: string | null;
  readings: SimulatedReading[];
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${SIMULATOR_URL}${path}`, init);
  const body = await response.json();
  if (!response.ok) throw new Error(body.error ?? `Erro ${response.status}`);
  return body as T;
}

export const simulator = {
  scenarios: () => request<Scenario[]>("/scenarios"),
  get: (id: string) => request<Simulation>(`/simulations/${id}`),
  start: (scenarioId: string, sensorId: string) =>
    request<Simulation>("/simulations", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ scenarioId, sensorId }),
    }),
};
