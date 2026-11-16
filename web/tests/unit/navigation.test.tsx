import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import App from "../../src/App";

vi.mock("../../src/components/CityMap", () => ({
  default: () => <div data-testid="map" />,
}));

const PAGES = [
  { link: "Mapa agora", heading: "Mapa agora" },
  { link: "Minha rua", heading: "Minha rua" },
  { link: "Histórico", heading: "Histórico" },
  { link: "Comprovações", heading: "Comprovações" },
  { link: "Verificar código", heading: "Verificar código" },
  { link: "Simulador", heading: "Simulador" },
  { link: "Início", heading: "Alagou na sua rua? A prova já está registrada." },
];

function renderApp(path = "/") {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <App />
    </MemoryRouter>,
  );
}

const menu = () => within(screen.getByRole("navigation"));

describe("navegação pelo menu", () => {
  it("abre cada página sem ficar em branco, mesmo quando scrollTo devolve uma Promise", async () => {
    const user = userEvent.setup();
    renderApp();

    for (const page of PAGES) {
      await user.click(menu().getByRole("link", { name: page.link }));
      expect(await screen.findByRole("heading", { level: 1, name: page.heading })).toBeInTheDocument();
    }
  });

  it("navega pelo menu duas vezes seguidas sem quebrar", async () => {
    const user = userEvent.setup();
    renderApp();

    for (const page of [...PAGES, ...PAGES]) {
      await user.click(menu().getByRole("link", { name: page.link }));
      expect(await screen.findByRole("heading", { level: 1, name: page.heading })).toBeInTheDocument();
    }
  });

  it("marca o link da página atual", async () => {
    const user = userEvent.setup();
    renderApp();

    await user.click(menu().getByRole("link", { name: "Histórico" }));

    expect(menu().getByRole("link", { name: "Histórico" })).toHaveAttribute("aria-current", "page");
  });

  it("volta ao topo da página a cada troca de rota", async () => {
    const user = userEvent.setup();
    renderApp();

    await user.click(menu().getByRole("link", { name: "Minha rua" }));

    expect(window.scrollTo).toHaveBeenLastCalledWith(0, 0);
  });

  it("fecha o menu do celular depois de escolher uma página", async () => {
    const user = userEvent.setup();
    renderApp();

    const menuButton = screen.getByRole("button", { name: /menu/i });
    await user.click(menuButton);
    expect(menuButton).toHaveAttribute("aria-expanded", "true");

    await user.click(menu().getByRole("link", { name: "Mapa agora" }));
    expect(menuButton).toHaveAttribute("aria-expanded", "false");
  });

  it("mostra os telefones de emergência em todas as páginas", () => {
    renderApp("/historico");

    expect(screen.getByRole("link", { name: /199/ })).toHaveAttribute("href", "tel:199");
    expect(screen.getByRole("link", { name: /193/ })).toHaveAttribute("href", "tel:193");
    expect(screen.getByRole("link", { name: /192/ })).toHaveAttribute("href", "tel:192");
  });
});
