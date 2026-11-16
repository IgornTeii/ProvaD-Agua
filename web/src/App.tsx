import { useEffect } from "react";
import { Route, Routes, useLocation } from "react-router-dom";
import Layout from "./components/Layout";
import HomePage from "./pages/HomePage";
import MapPage from "./pages/MapPage";
import SearchPage from "./pages/SearchPage";
import AddressPage from "./pages/AddressPage";
import CertificatesPage from "./pages/CertificatesPage";
import CertificatesHubPage from "./pages/CertificatesHubPage";
import HistoryPage from "./pages/HistoryPage";
import FloodEventPage from "./pages/FloodEventPage";
import VerifyPage from "./pages/VerifyPage";
import SimulatorPage from "./pages/SimulatorPage";

function ScrollToTop() {
  const { pathname } = useLocation();
  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);
  return null;
}

export default function App() {
  return (
    <Layout>
      <ScrollToTop />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/mapa" element={<MapPage />} />
        <Route path="/minha-rua" element={<SearchPage />} />
        <Route path="/endereco/:addressId" element={<AddressPage />} />
        <Route path="/endereco/:addressId/comprovacoes" element={<CertificatesPage />} />
        <Route path="/comprovacoes" element={<CertificatesHubPage />} />
        <Route path="/historico" element={<HistoryPage />} />
        <Route path="/alagamento/:eventId" element={<FloodEventPage />} />
        <Route path="/verificar" element={<VerifyPage />} />
        <Route path="/verificar/:code" element={<VerifyPage />} />
        <Route path="/simular" element={<SimulatorPage />} />
      </Routes>
    </Layout>
  );
}
