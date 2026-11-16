
const dateTime = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "short",
  timeStyle: "short",
  timeZone: "America/Sao_Paulo",
});

export const formatDateTime = (iso: string | null) => (iso ? dateTime.format(new Date(iso)) : "—");

export const formatCm = (value: number) =>
  `${value.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} cm`;

export const formatRain = (value: number) =>
  `${value.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} mm/h`;
