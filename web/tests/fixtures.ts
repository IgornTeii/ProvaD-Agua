export const address = {
  id: "00000000-0000-0000-0001-000000000001",
  street: "Rua Amazonas",
  number: "120",
  neighborhood: "São Miguel Paulista",
  city: "São Paulo",
  sensorId: "sensor-01",
  fullAddress: "Rua Amazonas, 120 - São Miguel Paulista, São Paulo",
};

export const reading = {
  waterLevelCm: 4,
  rainfallMmPerHour: 0,
  measuredAt: "2026-11-16T20:00:00Z",
};

export const sensor = {
  id: "sensor-01",
  name: "Rua Amazonas",
  neighborhood: "São Miguel Paulista",
  zone: "Leste",
  latitude: -23.4935,
  longitude: -46.4429,
  status: "Normal",
  latestReading: reading,
};

export const floodEvent = {
  id: "11111111-1111-1111-1111-111111111111",
  sensorId: "sensor-01",
  sensorName: "Rua Amazonas",
  neighborhood: "São Miguel Paulista",
  latitude: -23.4935,
  longitude: -46.4429,
  status: "Closed",
  peakWaterLevelCm: 62,
  startedAt: "2026-11-15T18:00:00Z",
  confirmedAt: "2026-11-15T18:05:00Z",
  closedAt: "2026-11-15T20:00:00Z",
  dismissedAt: null,
};

export const certificate = {
  id: "22222222-2222-2222-2222-222222222222",
  verificationCode: "PDA-AB12-CD34",
  floodEventId: floodEvent.id,
  address,
  peakWaterLevelCm: 62,
  floodStartedAt: floodEvent.startedAt,
  floodClosedAt: floodEvent.closedAt,
  isEligibleForAid: true,
  issuedAt: "2026-11-15T20:01:00Z",
};

export const scenario = {
  id: "flood",
  name: "Alagamento",
  description: "A água sobe com chuva forte e depois baixa.",
  expected: "A IA confirma e as comprovações são emitidas.",
};

export function responseFor(url: string): unknown {
  const { pathname } = new URL(url);

  if (pathname === "/sensors") return [sensor];
  if (pathname.startsWith("/sensors/")) return { sensorId: sensor.id, latestReading: reading, openEvent: null };
  if (pathname === "/addresses") return [address];
  if (pathname.endsWith("/notifications")) return [];
  if (pathname.startsWith("/addresses/")) return address;
  if (pathname === "/flood-events") return [floodEvent];
  if (pathname.startsWith("/flood-events/")) return floodEvent;
  if (pathname === "/certificates") return [certificate];
  if (pathname.endsWith("/verify")) return certificate;
  if (pathname === "/scenarios") return [scenario];
  if (pathname === "/health") return { status: "ok" };
  return [];
}
