import { Link } from "react-router-dom";
import {
  Bell, BrainCircuit, ChevronRight, CloudRain, FileCheck, History,
  Map as MapIcon, MapPin, ShieldCheck, TriangleAlert, CircleCheck,
} from "lucide-react";
import { api } from "../api";
import { useAsync } from "../useAsync";

const STEPS = [
  {
    icon: CloudRain,
    title: "Sensores medem a rua",
    text: "Em pontos que costumam alagar, sensores medem a altura da água e a chuva a cada poucos segundos.",
  },
  {
    icon: BrainCircuit,
    title: "A inteligência artificial confirma",
    text: "Ela diferencia um alagamento de verdade de um carro jogando água no sensor, para não gerar alarme falso.",
  },
  {
    icon: FileCheck,
    title: "Você recebe a comprovação",
    text: "Quando a água baixa, cada endereço atingido ganha uma comprovação com código, sem fila e sem papelada.",
  },
];

const ACTIONS = [
  { to: "/minha-rua", icon: MapPin, title: "Ver a situação da minha rua", text: "Saiba se está alagando agora e cadastre o celular para ser avisado." },
  { to: "/comprovacoes", icon: FileCheck, title: "Pegar minha comprovação", text: "Veja e imprima as comprovações dos alagamentos que atingiram seu endereço." },
  { to: "/mapa", icon: MapIcon, title: "Ver o mapa agora", text: "Acompanhe ao vivo os pontos monitorados em São Paulo." },
  { to: "/historico", icon: History, title: "Ver onde já alagou", text: "Busque alagamentos por data, rua ou bairro." },
  { to: "/verificar", icon: ShieldCheck, title: "Sou de um órgão público", text: "Confira se uma comprovação apresentada é autêntica." },
];

const QUESTIONS = [
  {
    q: "Preciso me cadastrar para ter a comprovação?",
    a: "Não. A comprovação é emitida automaticamente para todos os endereços atingidos. O cadastro do celular é opcional e serve só para receber o aviso quando alagar.",
  },
  {
    q: "Para que serve a comprovação?",
    a: "Ela registra que o seu endereço foi atingido, com data, horário e altura máxima da água. Você pode apresentá-la à prefeitura, à Defesa Civil ou a uma seguradora ao pedir auxílio ou indenização.",
  },
  {
    q: "O que quer dizer \"elegível a auxílio\"?",
    a: "Quer dizer que a água passou de 30 cm, altura em que costuma entrar nas casas. Esse limite é um exemplo; na prática, quem define é a Defesa Civil de cada cidade.",
  },
  {
    q: "Como alguém confere se a comprovação é verdadeira?",
    a: "Cada comprovação tem um código, como PDA-7K3M-9XQ2. Qualquer órgão pode digitá-lo em \"Verificar código\" e ver os dados originais.",
  },
  {
    q: "E se a inteligência artificial errar?",
    a: "Nos nossos testes com dados simulados, ela acertou 99,8% dos casos e reduziu os alarmes falsos de 53% para 0,2%. Com sensores reais esse número tende a ser menor, e os limites podem ser ajustados pela Defesa Civil.",
  },
];

function CityNow() {
  const sensors = useAsync(api.getSensors, [], 5000);
  if (!sensors.data) return null;

  const confirmed = sensors.data.filter((s) => s.status === "Confirmed").length;
  const suspected = sensors.data.filter((s) => s.status === "Suspected").length;
  const calm = confirmed === 0 && suspected === 0;

  return (
    <Link to="/mapa" className={`city-now ${calm ? "city-now-ok" : "city-now-alert"}`}>
      {calm ? <CircleCheck size={28} aria-hidden="true" /> : <TriangleAlert size={28} aria-hidden="true" />}
      <span>
        <strong>
          {calm
            ? `Agora: nenhum alagamento nos ${sensors.data.length} pontos monitorados`
            : `Agora: ${confirmed} alagamento(s) confirmado(s) e ${suspected} em análise`}
        </strong>
        <span>Ver no mapa</span>
      </span>
      <ChevronRight size={24} aria-hidden="true" />
    </Link>
  );
}

export default function HomePage() {
  return (
    <div className="home">
      <section className="hero">
        <p className="eyebrow">Para moradores de áreas que alagam em São Paulo</p>
        <h1>Alagou na sua rua? A prova já está registrada.</h1>
        <p className="hero-text">
          O Prova d'Água acompanha os pontos que alagam na cidade e, quando a água sobe de verdade, registra tudo
          sozinho. Depois, cada morador atingido recebe uma comprovação para pedir auxílio, sem precisar provar nada.
        </p>
        <div className="actions">
          <Link to="/minha-rua" className="button"><MapPin size={20} aria-hidden="true" /> Consultar minha rua</Link>
          <Link to="/comprovacoes" className="button button-secondary"><FileCheck size={20} aria-hidden="true" /> Pegar minha comprovação</Link>
        </div>
        <CityNow />
      </section>

      <section aria-labelledby="como-funciona">
        <h2 id="como-funciona">Como funciona</h2>
        <ol className="steps-grid">
          {STEPS.map(({ icon: Icon, title, text }, index) => (
            <li key={title} className="step-card">
              <span className="step-number">{index + 1}</span>
              <Icon size={32} aria-hidden="true" className="step-icon" />
              <h3>{title}</h3>
              <p>{text}</p>
            </li>
          ))}
        </ol>
      </section>

      <section aria-labelledby="o-que-fazer">
        <h2 id="o-que-fazer">O que você pode fazer aqui</h2>
        <div className="action-grid">
          {ACTIONS.map(({ to, icon: Icon, title, text }) => (
            <Link key={to} to={to} className="action-card">
              <Icon size={28} aria-hidden="true" />
              <span>
                <strong>{title}</strong>
                <span>{text}</span>
              </span>
              <ChevronRight size={22} aria-hidden="true" className="action-arrow" />
            </Link>
          ))}
        </div>
        <p className="tip">
          <Bell size={20} aria-hidden="true" />
          <span>Quer ser avisado quando alagar? Entre em <Link to="/minha-rua">Minha rua</Link>, escolha seu endereço e cadastre seu celular.</span>
        </p>
      </section>

      <section aria-labelledby="duvidas">
        <h2 id="duvidas">Perguntas frequentes</h2>
        <div className="faq">
          {QUESTIONS.map(({ q, a }) => (
            <details key={q}>
              <summary>{q}</summary>
              <p>{a}</p>
            </details>
          ))}
        </div>
      </section>
    </div>
  );
}
