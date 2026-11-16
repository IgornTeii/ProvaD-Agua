import { expect, test, type Page } from "@playwright/test";
import { responseFor } from "../fixtures";

const PAGES = [
  { link: "Mapa agora", heading: "Mapa agora", path: "/mapa" },
  { link: "Minha rua", heading: "Minha rua", path: "/minha-rua" },
  { link: "Histórico", heading: "Histórico", path: "/historico" },
  { link: "Comprovações", heading: "Comprovações", path: "/comprovacoes" },
  { link: "Verificar código", heading: "Verificar código", path: "/verificar" },
  { link: "Simulador", heading: "Simulador", path: "/simular" },
  { link: "Início", heading: "Alagou na sua rua? A prova já está registrada.", path: "/" },
];

const errors: string[] = [];

test.beforeEach(async ({ page }) => {
  errors.length = 0;
  page.on("pageerror", (error) => errors.push(error.message));

  await page.route(/localhost:(5016|8100)/, (route) => route.fulfill({ json: responseFor(route.request().url()) }));
  await page.route(/tile\.openstreetmap\.org/, (route) => route.fulfill({ status: 204 }));
});

async function openMenuLink(page: Page, name: string) {
  const menuButton = page.getByRole("button", { name: /menu/i });
  if (await menuButton.isVisible()) await menuButton.click();
  await page.getByRole("navigation").getByRole("link", { name, exact: true }).click();
}

test("navega por todas as páginas do menu sem recarregar e sem tela branca", async ({ page }) => {
  await page.goto("/");

  for (const item of [...PAGES, ...PAGES]) {
    await openMenuLink(page, item.link);
    await expect(page).toHaveURL(item.path);
    await expect(page.getByRole("heading", { level: 1, name: item.heading })).toBeVisible();
  }

  expect(errors).toEqual([]);
});

test("abre direto qualquer endereço do site", async ({ page }) => {
  for (const item of PAGES) {
    await page.goto(item.path);
    await expect(page.getByRole("heading", { level: 1, name: item.heading })).toBeVisible();
  }

  expect(errors).toEqual([]);
});

test("morador encontra a rua e chega nas comprovações", async ({ page }) => {
  await page.goto("/minha-rua");
  await page.getByLabel("Nome da rua ou do bairro").fill("Amazonas");
  await page.getByRole("link", { name: /Rua Amazonas, 120/ }).click();

  await expect(page.getByRole("heading", { level: 1, name: "Rua Amazonas, 120" })).toBeVisible();

  await page.getByRole("link", { name: /comprovações/i }).first().click();
  await expect(page.getByText("PDA-AB12-CD34")).toBeVisible();
  expect(errors).toEqual([]);
});

test("órgão público confere um código de comprovação", async ({ page }) => {
  await page.goto("/verificar");
  await page.getByRole("textbox").fill("PDA-AB12-CD34");
  await page.getByRole("button", { name: /verificar/i }).click();

  await expect(page.getByText("PDA-AB12-CD34").first()).toBeVisible();
  expect(errors).toEqual([]);
});
