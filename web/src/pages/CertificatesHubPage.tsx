import { useState } from "react";
import { Link } from "react-router-dom";
import { api, type Address } from "../api";
import { useAsync } from "../useAsync";
import CertificateList from "../components/CertificateList";
import { ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";

function groupByNeighborhood(addresses: Address[]) {
  const groups = new Map<string, Address[]>();
  for (const address of addresses)
    groups.set(address.neighborhood, [...(groups.get(address.neighborhood) ?? []), address]);
  return [...groups.entries()].sort(([a], [b]) => a.localeCompare(b, "pt-BR"));
}

export default function CertificatesHubPage() {
  const addresses = useAsync(() => api.searchAddresses(), []);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  if (addresses.loading && !addresses.data) return <Loading />;
  if (addresses.error || !addresses.data) return <ErrorMessage text="Não foi possível carregar os endereços." />;

  const addressId = selectedId ?? addresses.data[0]?.id;
  const selected = addresses.data.find((a) => a.id === addressId);

  return (
    <section>
      <PageHeader
        title="Comprovações"
        description="A comprovação é o documento que mostra que o seu endereço foi atingido por um alagamento, com data, horário e altura da água."
        tip="Escolha o seu endereço. Toque em uma comprovação para ver os detalhes e imprimir ou salvar em PDF."
      />

      <label className="field">
        <span>Endereço</span>
        <select value={addressId} onChange={(e) => setSelectedId(e.target.value)}>
          {groupByNeighborhood(addresses.data).map(([neighborhood, group]) => (
            <optgroup key={neighborhood} label={neighborhood}>
              {group.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.street}, {a.number}
                </option>
              ))}
            </optgroup>
          ))}
        </select>
      </label>

      {selected && (
        <>
          <p className="muted">
            {selected.fullAddress} · <Link to={`/endereco/${selected.id}`}>ver a situação desta rua</Link>
          </p>
          <CertificateList addressId={selected.id} />
        </>
      )}
    </section>
  );
}
