import { describe, expect, it, vi } from "vitest";
import { api, NotFoundError } from "../../src/api";

const fetchMock = () => vi.mocked(fetch);
const calledUrl = () => String(fetchMock().mock.calls[0][0]);

describe("cliente da API", () => {
  it("monta a busca de histórico só com os filtros preenchidos", async () => {
    await api.getFloodEvents({ street: "Amazonas", from: "", sensorId: undefined });

    expect(calledUrl()).toBe("http://localhost:5016/flood-events?street=Amazonas");
  });

  it("protege caracteres especiais no código da comprovação", async () => {
    await api.verifyCertificate("  PDA/AB12  ");

    expect(calledUrl()).toBe("http://localhost:5016/certificates/PDA%2FAB12/verify");
  });

  it("transforma 404 em NotFoundError", async () => {
    fetchMock().mockResolvedValueOnce(new Response(null, { status: 404 }));

    await expect(api.verifyCertificate("PDA-0000-0000")).rejects.toBeInstanceOf(NotFoundError);
  });

  it("mostra a mensagem de erro que a API devolveu", async () => {
    fetchMock().mockResolvedValueOnce(Response.json({ detail: "Telefone inválido." }, { status: 400 }));

    await expect(api.subscribe("id", "123")).rejects.toThrow("Telefone inválido.");
  });

  it("aceita resposta 204 sem corpo", async () => {
    fetchMock().mockResolvedValueOnce(new Response(null, { status: 204 }));

    await expect(api.subscribe("id", "11999999999")).resolves.toBeUndefined();
  });
});
