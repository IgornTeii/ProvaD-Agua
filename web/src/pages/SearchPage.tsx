import { useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { useAsync } from "../useAsync";
import { Empty, ErrorMessage, Loading } from "../components/States";
import PageHeader from "../components/PageHeader";
export default function SearchPage() {
  const [search, setSearch] = useState("");
  const addresses = useAsync(() => api.searchAddresses(search), [search]);

  return (
    <section>
      <PageHeader
        title="Minha rua"
        description="Veja se a sua rua está alagando agora, cadastre seu celular para ser avisado e acesse as suas comprovações."
        tip="Digite parte do nome da rua ou do bairro, por exemplo: Pompeia, Aricanduva ou Amazonas."
      />

      <label className="field">
        <span>Nome da rua ou do bairro</span>
        <input
          type="search"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Ex.: Rua das Flores"
          autoComplete="street-address"
        />
      </label>

      {addresses.loading && !addresses.data && <Loading />}
      {addresses.error && <ErrorMessage text="Não foi possível buscar os endereços. Tente de novo em instantes." />}
      {addresses.data?.length === 0 && <Empty text="Nenhum endereço encontrado com esse nome." />}

      <ul className="list">
        {addresses.data?.map((address) => (
          <li key={address.id}>
            <Link to={`/endereco/${address.id}`} className="list-item">
              <strong>{address.street}, {address.number}</strong>
              <span className="muted">{address.neighborhood} · {address.city}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}
