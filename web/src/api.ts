const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5016";

export type FloodEventStatus = "Suspected" | "Confirmed" | "Closed" | "Dismissed";
export type SensorStatusName = "Normal" | "Suspected" | "Confirmed";

export interface Address {
  id: string;
  street: string;
  number: string;
  neighborhood: string;
  city: string;
  sensorId: string;
  fullAddress: string;
}

export interface Reading {
  waterLevelCm: number;
  rainfallMmPerHour: number;
  measuredAt: string;
}

export interface FloodEvent {
  id: string;
  sensorId: string;
  sensorName: string;
  neighborhood: string;
  latitude: number;
  longitude: number;
  status: FloodEventStatus;
  peakWaterLevelCm: number;
  startedAt: string;
  confirmedAt: string | null;
  closedAt: string | null;
  dismissedAt: string | null;
}

export interface SensorOnMap {
  id: string;
  name: string;
  neighborhood: string;
  zone: string;
  latitude: number;
  longitude: number;
  status: SensorStatusName;
  latestReading: Reading | null;
}

export interface SensorStatus {
  sensorId: string;
  latestReading: Reading | null;
  openEvent: FloodEvent | null;
}

export interface Certificate {
  id: string;
  verificationCode: string;
  floodEventId: string;
  address: Address;
  peakWaterLevelCm: number;
  floodStartedAt: string;
  floodClosedAt: string;
  isEligibleForAid: boolean;
  issuedAt: string;
}

export interface AlertNotification {
  id: string;
  phone: string;
  message: string;
  sentAt: string;
}

export interface FloodEventFilter {
  sensorId?: string;
  from?: string;
  to?: string;
  street?: string;
}

export class NotFoundError extends Error {}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, init);

  if (response.status === 404) throw new NotFoundError(path);
  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail ?? `Erro ${response.status} ao chamar ${path}`);
  }

  return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>);
}

const query = (params: Record<string, string | undefined>) =>
  new URLSearchParams(Object.entries(params).filter(([, v]) => v) as [string, string][]).toString();

export const api = {
  searchAddresses: (search = "") => request<Address[]>(`/addresses?${query({ search })}`),

  getSensors: () => request<SensorOnMap[]>("/sensors"),

  getSensorStatus: (sensorId: string) =>
    request<SensorStatus>(`/sensors/${encodeURIComponent(sensorId)}/status`),

  getFloodEvents: (filter: FloodEventFilter = {}) =>
    request<FloodEvent[]>(`/flood-events?${query({ ...filter })}`),

  getFloodEvent: (id: string) => request<FloodEvent>(`/flood-events/${encodeURIComponent(id)}`),

  getCertificates: (filter: { addressId?: string; floodEventId?: string }) =>
    request<Certificate[]>(`/certificates?${query(filter)}`),

  verifyCertificate: (code: string) =>
    request<Certificate>(`/certificates/${encodeURIComponent(code.trim())}/verify`),

  getNotifications: (addressId: string) =>
    request<AlertNotification[]>(`/addresses/${encodeURIComponent(addressId)}/notifications`),

  subscribe: (addressId: string, phone: string) =>
    request<void>("/alerts/subscriptions", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ addressId, phone }),
    }),
};
