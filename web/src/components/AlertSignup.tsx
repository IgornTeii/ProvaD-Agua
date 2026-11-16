import { useState, type FormEvent } from "react";
import { api } from "../api";
import { useAsync } from "../useAsync";
import { formatDateTime } from "../format";

export default function AlertSignup({ addressId }: { addressId: string }) {
  const [phone, setPhone] = useState("");
  const [state, setState] = useState<"idle" | "saving" | "saved">("idle");
  const [error, setError] = useState<string | null>(null);
  const notifications = useAsync(() => api.getNotifications(addressId), [addressId], 5000);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setState("saving");
    try {
      await api.subscribe(addressId, phone);
      setState("saved");
      setPhone("");
    } catch (err) {
      setError((err as Error).message);
      setState("idle");
    }
  };

  return (
    <div className="alert-box">
      <h2>Me avise quando alagar</h2>
      <p className="muted">Cadastre seu celular e receba uma mensagem assim que um alagamento for confirmado na sua rua.</p>

      <form className="inline-form" onSubmit={submit}>
        <label className="field">
          <span>Celular com DDD</span>
          <input
            type="tel"
            inputMode="tel"
            autoComplete="tel"
            placeholder="(11) 91234-5678"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
          />
        </label>
        <button type="submit" className="button" disabled={state === "saving"}>
          {state === "saving" ? "Salvando..." : "Quero ser avisado"}
        </button>
      </form>

      {state === "saved" && <p className="alert alert-ok">Pronto! Você vai ser avisado quando alagar aqui.</p>}
      {error && <p className="alert alert-error">{error}</p>}

      {!!notifications.data?.length && (
        <>
          <h3>Avisos enviados</h3>
          <ul className="list">
            {notifications.data.map((n) => (
              <li key={n.id} className="message">
                <span className="small">Para {n.phone} · {formatDateTime(n.sentAt)}</span>
                <p>{n.message}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </div>
  );
}
