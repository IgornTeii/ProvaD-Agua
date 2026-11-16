import { describe, expect, it } from "vitest";
import { formatCm, formatDateTime, formatRain } from "../../src/format";
import { EVENT_STATUS, SENSOR_STATUS } from "../../src/status";

describe("formatação", () => {
  it("mostra data e hora no fuso de São Paulo", () => {
    expect(formatDateTime("2026-11-16T23:30:00Z")).toBe("16/11/2026, 20:30");
  });

  it("mostra um traço quando não há data", () => {
    expect(formatDateTime(null)).toBe("—");
  });

  it("usa vírgula decimal e unidade", () => {
    expect(formatCm(12.34)).toBe("12,3 cm");
    expect(formatRain(40)).toBe("40 mm/h");
  });
});

describe("status", () => {
  it("usa amarelo para suspeita e vermelho para confirmado", () => {
    expect(SENSOR_STATUS.Suspected.tone).toBe("warning");
    expect(SENSOR_STATUS.Confirmed.tone).toBe("danger");
    expect(SENSOR_STATUS.Normal.tone).toBe("ok");
  });

  it("trata suspeita descartada como neutra", () => {
    expect(EVENT_STATUS.Dismissed.tone).toBe("neutral");
  });
});
