import { useState, type ReactNode } from "react";
import { NavLink } from "react-router-dom";
import {
  AArrowDown, AArrowUp, Contrast, FileCheck, FlaskConical, History, House,
  Map as MapIcon, MapPin, Menu, Phone, ShieldCheck, X,
} from "lucide-react";
import { usePreferences } from "../usePreferences";

const LINKS = [
  { to: "/", label: "Início", icon: House },
  { to: "/mapa", label: "Mapa agora", icon: MapIcon },
  { to: "/minha-rua", label: "Minha rua", icon: MapPin },
  { to: "/historico", label: "Histórico", icon: History },
  { to: "/comprovacoes", label: "Comprovações", icon: FileCheck },
  { to: "/verificar", label: "Verificar código", icon: ShieldCheck },
  { to: "/simular", label: "Simulador", icon: FlaskConical },
];

function AccessibilityBar() {
  const preferences = usePreferences();

  return (
    <div className="a11y-bar no-print">
      <div className="a11y-inner">
        <a href="#conteudo" className="skip-link">Pular para o conteúdo</a>
        <div className="a11y-tools" role="group" aria-label="Acessibilidade">
          <span className="a11y-label">Acessibilidade</span>
          <button type="button" onClick={preferences.shrink} disabled={!preferences.canShrink} aria-label="Diminuir o texto">
            <AArrowDown size={18} aria-hidden="true" /> A-
          </button>
          <button type="button" onClick={preferences.grow} disabled={!preferences.canGrow} aria-label="Aumentar o texto">
            <AArrowUp size={18} aria-hidden="true" /> A+
          </button>
          <button type="button" onClick={preferences.toggleContrast} aria-pressed={preferences.highContrast}>
            <Contrast size={18} aria-hidden="true" /> Alto contraste
          </button>
        </div>
      </div>
    </div>
  );
}

function Header() {
  const [open, setOpen] = useState(false);
  const close = () => setOpen(false);

  return (
    <header className="topbar no-print">
      <div className="topbar-inner">
        <NavLink to="/" className="brand" onClick={close} aria-label="Prova d'Água, página inicial">
          <span className="brand-drop" aria-hidden="true" />
          <span>
            Prova d'Água
            <small>Comprovação automática de alagamentos</small>
          </span>
        </NavLink>

        <button
          type="button"
          className="menu-button"
          aria-expanded={open}
          aria-controls="menu-principal"
          onClick={() => setOpen((o) => !o)}
        >
          {open ? <X size={22} aria-hidden="true" /> : <Menu size={22} aria-hidden="true" />}
          Menu
        </button>

        <nav id="menu-principal" aria-label="Menu principal" className={open ? "open" : ""}>
          {LINKS.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === "/"} onClick={close}>
              <Icon size={18} aria-hidden="true" />
              {label}
            </NavLink>
          ))}
        </nav>
      </div>
    </header>
  );
}

function Footer() {
  return (
    <footer className="footer no-print">
      <div className="footer-inner">
        <div>
          <p className="footer-title">Prova d'Água</p>
          <p>
            Registra alagamentos automaticamente com sensores e inteligência artificial e emite a comprovação para
            os moradores atingidos, sem fila e sem papelada.
          </p>
          <p className="footer-note">
            Protótipo acadêmico (FIAP · Global Solution 2026). Os locais são reais; os alagamentos mostrados são simulados.
          </p>
        </div>
        <div>
          <p className="footer-title">Em emergência, ligue</p>
          <ul className="emergency">
            <li><a href="tel:199"><Phone size={16} aria-hidden="true" /> 199 · Defesa Civil</a></li>
            <li><a href="tel:193"><Phone size={16} aria-hidden="true" /> 193 · Corpo de Bombeiros</a></li>
            <li><a href="tel:192"><Phone size={16} aria-hidden="true" /> 192 · SAMU</a></li>
          </ul>
        </div>
      </div>
    </footer>
  );
}

export default function Layout({ children }: { children: ReactNode }) {
  return (
    <div className="app">
      <AccessibilityBar />
      <Header />
      <main id="conteudo" className="content" tabIndex={-1}>
        {children}
      </main>
      <Footer />
    </div>
  );
}
