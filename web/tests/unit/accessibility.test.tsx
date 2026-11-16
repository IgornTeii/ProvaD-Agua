import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import Layout from "../../src/components/Layout";

function renderLayout() {
  return render(
    <MemoryRouter>
      <Layout>
        <p>conteúdo</p>
      </Layout>
    </MemoryRouter>,
  );
}

const root = () => document.documentElement;

describe("barra de acessibilidade", () => {
  it("começa no tamanho padrão sem poder diminuir", () => {
    renderLayout();

    expect(screen.getByRole("button", { name: "Diminuir o texto" })).toBeDisabled();
    expect(root().style.fontSize).toBe("112.5%");
  });

  it("aumenta o texto até o limite e depois desabilita o botão", async () => {
    const user = userEvent.setup();
    renderLayout();
    const grow = screen.getByRole("button", { name: "Aumentar o texto" });

    await user.click(grow);
    await user.click(grow);
    await user.click(grow);

    expect(grow).toBeDisabled();
    expect(root().style.fontSize).toBe("157.5%");
  });

  it("liga e desliga o alto contraste", async () => {
    const user = userEvent.setup();
    renderLayout();
    const contrast = screen.getByRole("button", { name: /alto contraste/i });

    await user.click(contrast);
    expect(contrast).toHaveAttribute("aria-pressed", "true");
    expect(root().dataset.contrast).toBe("high");

    await user.click(contrast);
    expect(root().dataset.contrast).toBe("normal");
  });

  it("lembra a escolha do usuário na próxima visita", async () => {
    const user = userEvent.setup();
    const first = renderLayout();
    await user.click(screen.getByRole("button", { name: /alto contraste/i }));
    first.unmount();

    renderLayout();

    expect(screen.getByRole("button", { name: /alto contraste/i })).toHaveAttribute("aria-pressed", "true");
  });

  it("oferece um atalho para pular direto ao conteúdo", () => {
    renderLayout();

    expect(screen.getByRole("link", { name: "Pular para o conteúdo" })).toHaveAttribute("href", "#conteudo");
    expect(screen.getByRole("main")).toHaveAttribute("id", "conteudo");
  });
});
