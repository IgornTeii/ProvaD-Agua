import { Link, useParams } from "react-router-dom";
import { useAddress } from "../components/useAddress";
import CertificateList from "../components/CertificateList";
import PageHeader from "../components/PageHeader";

export default function CertificatesPage() {
  const { addressId = "" } = useParams();
  const address = useAddress(addressId);

  return (
    <section>
      <Link to={`/endereco/${addressId}`} className="back">← Ver a situação desta rua</Link>
      <PageHeader
        title="Minhas comprovações"
        description={address.data?.fullAddress ?? "Carregando o endereço..."}
        tip="Apresente o código da comprovação à prefeitura, à Defesa Civil ou à seguradora ao pedir auxílio. Toque nela para imprimir."
      />

      <CertificateList addressId={addressId} />
    </section>
  );
}
