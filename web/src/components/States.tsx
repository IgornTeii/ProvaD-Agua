export function Loading({ text = "Carregando..." }: { text?: string }) {
  return <p className="muted" role="status">{text}</p>;
}

export function ErrorMessage({ text }: { text: string }) {
  return <p className="alert alert-error" role="alert">{text}</p>;
}

export function Empty({ text }: { text: string }) {
  return <p className="muted">{text}</p>;
}
