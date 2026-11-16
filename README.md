# Prova d'Água

> **Quando a água sobe, o seu direito fica registrado.**

Projeto da **Global Solution 2026/2 — CitizenTech** (FIAP, Engenharia de Software, turma 4ESOA).

🎥 **Vídeo pitch:** _TODO: link público (YouTube, Drive ou Vimeo), testado em aba anônima_

---

## Sumário

1. [O problema](#1-o-problema)
2. [O cidadão beneficiado](#2-o-cidadão-beneficiado)
3. [A solução](#3-a-solução)
4. [Ecossistema](#4-ecossistema)
5. [ODS atendidos](#5-ods-atendidos)
6. [Arquitetura](#6-arquitetura)
7. [Como executar](#7-como-executar)
8. [Área 1 — IA e automação](#8-área-1--ia-e-automação)
9. [Área 2 — Big Data e visualização](#9-área-2--big-data-e-visualização)
10. [Área 3 — MVP, IoT e experiência digital](#10-área-3--mvp-iot-e-experiência-digital)
11. [Área 4 — DevOps, segurança e qualidade](#11-área-4--devops-segurança-e-qualidade)
12. [Impacto e como medimos](#12-impacto-e-como-medimos)
13. [Validação com pessoas reais](#13-validação-com-pessoas-reais)
14. [Uso de IA generativa](#14-uso-de-ia-generativa)
15. [Integrantes](#15-integrantes)

---

## 1. O problema

Soluções de enchente costumam resolver o **antes**: previsão e alerta. Quase ninguém resolve o **depois**.

Quem perde móveis, eletrodomésticos, carro ou dias de trabalho num alagamento precisa **provar que foi atingido** para pedir auxílio, seguro ou saque emergencial. Hoje essa prova depende de foto, protocolo, vistoria e semanas de espera, e quem mais sofre é justamente quem tem menos condição de esperar.

_TODO: inserir um dado real sobre alagamentos (fonte pública: Defesa Civil, IBGE, S2iD)._

## 2. O cidadão beneficiado

Moradores de ruas e bairros que alagam com frequência, principalmente famílias de baixa renda, para quem um alagamento significa perda de bens e de renda.

**Persona:** Dona Maria mora numa rua que alaga toda vez que chove forte. Depois de cada enchente, precisa juntar fotos e enfrentar filas para comprovar o prejuízo. Com o Prova d'Água, ela consulta o endereço e recebe uma comprovação com código verificável, sem fila e sem precisar provar sozinha o que a rua inteira viveu.

## 3. A solução

Sensores instalados na rua medem **nível de água** e **chuva** continuamente. Quando uma **IA confirma** que a subida da água é um alagamento real, e não ruído do sensor, o sistema:

1. registra o evento de alagamento com início, fim e nível máximo atingido;
2. **emite automaticamente uma comprovação por endereço** da área afetada, com um código de verificação;
3. indica se o endereço é **elegível a auxílio** (pico acima de um limite definido pela Defesa Civil);
4. alimenta um **dashboard** para a Defesa Civil acompanhar ruas atingidas, impacto e tempo de resposta.

### A história de um alagamento

| Momento | O que acontece no sistema |
|---|---|
| Começa a chover | Cada leitura dos sensores é gravada (imutável: é a prova) |
| Nível passa de 15 cm | Abre um **evento Suspeito** para o sensor |
| IA analisa a sequência de leituras | **Alagamento** → evento Confirmado · **Ruído** → evento Descartado · **Inconclusivo** → aguarda o próximo ciclo |
| Água sobe e desce | O evento guarda o **pico** atingido |
| Água baixa | O evento é **Encerrado** |
| Logo em seguida | Uma **comprovação por endereço** é emitida, com código verificável |
| Dias depois | Cidadão ou órgão público consulta e verifica a comprovação pelo código |

### Ciclo de vida do evento

```
Suspeito ──► Confirmado ──► Encerrado
    │
    └──────► Descartado
```

Só anda para frente, um passo de cada vez. Qualquer outra transição é recusada pelo domínio.

## 4. Ecossistema

| Participante | Papel |
|---|---|
| **Cidadão** | Consulta a situação do bairro e a comprovação do seu endereço |
| **Defesa Civil** | Define os limites (suspeita e elegibilidade), acompanha o dashboard |
| **Prefeitura / órgãos de auxílio e seguradoras** | Verificam a autenticidade das comprovações pelo código |
| **Sensores (IoT)** | Medem nível de água e chuva na rua |

## 5. ODS atendidos

- **ODS 11 — Cidades e comunidades sustentáveis:** reduz o impacto de desastres sobre a população urbana (meta 11.5).
- **ODS 13 — Ação contra a mudança global do clima:** fortalece a resiliência a eventos climáticos extremos (meta 13.1).
- **ODS 10 — Redução das desigualdades:** encurta o caminho até o auxílio para quem mais precisa e menos consegue esperar.

## 6. Arquitetura

_TODO: inserir o diagrama de arquitetura (também vai no PDF)._

```
 ┌──────────────┐  POST /readings   ┌───────────────────────────┐   POST /classify   ┌──────────────┐
 │  Simulador   │ ────────────────► │        API (.NET 10)      │ ─────────────────► │  IA (Python) │
 │  IoT (mock)  │   X-Sensor-Key    │  Clean Architecture + DDD │ ◄───────────────── │ scikit-learn │
 └──────────────┘                   └─────────────┬─────────────┘   flood/noise/...  └──────────────┘
                                                  │
                                           ┌──────▼──────┐
                                           │ PostgreSQL  │──────► Dashboard (Power BI)
                                           └──────▲──────┘
                                                  │
                                    ┌─────────────┴─────────────┐
                                    │   Web (React + TS)        │
                                    │   4 telas do cidadão      │
                                    └───────────────────────────┘
```

**Princípio:** serviços separados num único repositório. Nenhum serviço importa código de outro; eles só se comunicam por HTTP. Cada um tem seu próprio README e Dockerfile.

**A IA opina, a API decide:** a IA só classifica uma sequência de leituras. Quem aplica a regra de negócio (confirmar, descartar, emitir comprovação) é a API. Assim a regra fica num lugar só, e o modelo pode ser trocado sem alterar o domínio.

### Estrutura do repositório

```
prova-dagua/
├── api/                  # C# ASP.NET Core (.NET 10)
│   ├── src/
│   │   ├── ProvaDagua.Domain           # Regras de negócio puras
│   │   ├── ProvaDagua.Application      # Casos de uso e contratos
│   │   ├── ProvaDagua.Infrastructure   # EF Core, PostgreSQL, implementações
│   │   └── ProvaDagua.Api              # Controllers, autenticação, composição
│   └── tests/ProvaDagua.UnitTests
├── simulator/            # Python: sensores simulados e cenários
├── ai/                   # Python: treino, avaliação e serviço de classificação
├── data/                 # Pipeline e camadas de dados
├── web/                  # React + TypeScript
├── dashboard/            # Power BI (.pbix) e prints
├── docs/                 # Arquitetura, plano de testes, prompts, evidências
├── .github/workflows/    # CI
└── docker-compose.yml
```

### Decisões técnicas

| Parte | Tecnologia | Motivo |
|---|---|---|
| API | C# / ASP.NET Core (.NET 10 LTS) | Versão com suporte longo; Clean Architecture com Result pattern |
| Banco | PostgreSQL 17 (Docker) | Índices únicos e parciais garantem as regras de idempotência |
| IA | Python + scikit-learn | Classificação explicável e mensurável, sem custo de API |
| IoT | Simulador em Python (mock) | Permitido pelo regulamento; reproduz uma enchente sob demanda |
| Front-end | React + TypeScript | — |
| Dashboard | Power BI Desktop + DAX | — |
| CI/CD | GitHub Actions | Gratuito em repositório público |

## 7. Como executar

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Python 3.10+
- _TODO: Node.js (para o front-end)_

### Passo a passo

```bash
# 1. Subir o banco (na raiz do repositório)
docker compose up -d

# 2. Criar as tabelas (na pasta api)
cd api
dotnet tool restore
dotnet ef database update -p src/ProvaDagua.Infrastructure -s src/ProvaDagua.Api

# 3. Rodar a API
dotnet run --project src/ProvaDagua.Api
# Swagger: http://localhost:5016/swagger

# 4. Em outro terminal, simular um alagamento
cd simulator
python3 main.py flood
```

_TODO: quando o docker-compose subir todos os serviços, substituir os passos acima por um único `docker compose up`._

### Cenários do simulador

| Comando | O que simula |
|---|---|
| `python3 main.py normal` | Dia comum, sem chuva |
| `python3 main.py light_rain` | Chuva fraca, nível sobe sem alagar |
| `python3 main.py flood` | Alagamento real: sobe, fica no pico e baixa |
| `python3 main.py splash --sensor-key dev-key-sensor-02` | Ruído: carro joga água no sensor |

Opções: `--api-url`, `--sensor-key`, `--interval` (segundos reais entre envios).

## 8. Área 1 — IA e automação

_Status: ⏳ em desenvolvimento_

**Problema que a IA resolve:** um sensor de nível não distingue um alagamento real de um carro jogando água nele. Uma regra fixa ("acima de X cm = alagou") gera falsos alarmes e, portanto, comprovações e auxílios indevidos.

**Como funciona:** a IA recebe uma **janela de leituras** (não uma leitura isolada) e classifica em **alagamento**, **ruído** ou **inconclusivo**, a partir de características (features) da sequência:

- **Tendência:** o nível sobe de forma contínua ou dá um salto e volta?
- **Duração:** ficou alto por minutos ou por segundos?
- **Chuva junto:** a subida acompanha a chuva?
- **Oscilação:** variação suave ou picos isolados?

**Automação:** um processo em segundo plano na API consulta a IA a cada ciclo para cada evento Suspeito e aplica o resultado. Se a IA estiver fora do ar, o evento continua Suspeito (nunca confirma nem descarta "por garantia").

**Métrica (antes × depois):** _TODO: acurácia, precisão e recall do modelo comparados com a regra fixa de limite, num conjunto de teste separado do treino._

**Demonstração (entrada → saída):** _TODO: exemplo de janela de leituras e resposta da IA._

**Prompts / regras de automação:** _TODO: documentar no PDF._

## 9. Área 2 — Big Data e visualização

_Status: ⏳ em desenvolvimento_

**Estrutura dos dados (camadas):**

| Camada | Conteúdo |
|---|---|
| **Bruto** | Leitura como chegou do sensor. Nunca é apagada: é a prova |
| **Tratado** | Sem duplicatas e sem valores impossíveis, com datas em UTC |
| **Analítico** | Agregados prontos para o dashboard (ex.: nível máximo por rua por hora) |

**Ingestão e processamento:** sensor → `POST /readings` → validação → gravação → classificação → evento → comprovação.

**Dashboard (Defesa Civil):** _TODO_
- Eventos por bairro e período
- Endereços atingidos e elegíveis a auxílio
- Tempo entre o fim do alagamento e a emissão da comprovação
- Acerto da IA

**Funções analíticas (DAX):** _TODO_

## 10. Área 3 — MVP, IoT e experiência digital

### IoT — 2 sensores (simulados)

| Sensor | Equivalente real | Mede | Frequência |
|---|---|---|---|
| **Nível de água** | Ultrassônico HC-SR04, instalado a uma altura fixa acima da rua | Altura da água (cm) | A cada 10 s |
| **Pluviômetro** | Pluviômetro de báscula | Intensidade da chuva (mm/h) | A cada 10 s |

**Por que dois sensores:** o nível sozinho não distingue um alagamento de um carro jogando água no sensor. **A chuva é o contexto** que a IA usa para decidir.

### Protótipo — 4 telas

_Status: ⏳ em desenvolvimento_

1. Situação agora no meu bairro
2. Consultar meu endereço
3. Minha comprovação (com código verificável)
4. Mapa e histórico dos eventos

### Endpoints da API

| Método | Rota | Descrição | Status |
|---|---|---|---|
| `POST` | `/readings` | Ingestão de leituras (exige `X-Sensor-Key`) | ✅ |
| `GET` | `/flood-events` | Alagamentos confirmados | ⏳ |
| `GET` | `/certificates?address=` | Comprovação do endereço | ⏳ |
| `GET` | `/certificates/{code}/verify` | Verifica a autenticidade de uma comprovação | ⏳ |

Respostas do `POST /readings`: **202** (aceita; o processamento segue em segundo plano), **400** (dado inválido), **401** (chave ausente ou inválida).

### Fluxo funcional completo

_TODO: descrever o uso de ponta a ponta (do sensor até o cidadão verificando a comprovação)._

## 11. Área 4 — DevOps, segurança e qualidade

### Organização
Monorepo com serviços independentes, versionado no Git. API em Clean Architecture com 4 projetos; a direção das dependências é garantida pelo compilador.

### DevOps
_TODO: workflow do GitHub Actions (build + testes a cada commit), Dockerfiles e docker-compose com todos os serviços._

### Segurança

| Prática | Como foi aplicada |
|---|---|
| **Autenticação do sensor** | Cada sensor envia uma chave no cabeçalho `X-Sensor-Key`. **A identidade do sensor vem da chave, nunca do corpo da requisição**: uma chave vazada só compromete o próprio sensor, e não permite forjar alagamentos em outras ruas |
| **Validação de entrada** | O domínio recusa leituras fisicamente impossíveis (nível fora de 0–500 cm, chuva negativa, data no futuro) |
| **Integridade e idempotência** | Índices únicos no banco impedem leituras duplicadas, mais de um evento aberto por sensor e mais de uma comprovação por endereço por evento |
| **Comprovação não adivinhável** | Código no formato `PDA-XXXX-XXXX` gerado com gerador criptográfico, sem caracteres ambíguos |

_Observação: as chaves de sensor deste repositório são de desenvolvimento. Em produção, viriam de variáveis de ambiente e seriam armazenadas como hash._

### Testes
_TODO: plano com 5+ casos e evidências. Já implementados (testes unitários do domínio):_

| # | Caso | Resultado esperado |
|---|---|---|
| 1 | Encerrar evento ainda Suspeito | Falha `only_confirmed_can_be_closed`; estado não muda |
| 2 | Confirmar evento Suspeito | Status vira Confirmado |
| 3 | Confirmar duas vezes | Segunda tentativa falha |
| 4 | Registrar nível maior e menor que o pico | Pico só aumenta |
| 5 | Criar leitura com nível negativo | Falha `invalid_water_level` |
| 6 | Descartar evento já confirmado | Falha `only_suspected_can_be_dismissed` |

Testes de integração manuais (`api/readings.http`): leitura válida (202), sem chave (401), nível negativo (400), leitura repetida (202 sem duplicar no banco).

Executar: `cd api && dotnet test`

## 12. Impacto e como medimos

| Indicador | Hoje | Com o Prova d'Água |
|---|---|---|
| Tempo até a comprovação | Semanas (foto, protocolo, vistoria) | Minutos após o fim do alagamento |
| Quem prova o alagamento | O próprio cidadão | O sensor, de forma automática |
| Falsos alarmes | Regra fixa de nível | _TODO: redução medida com a IA_ |

**Como ampliar:** os limites de suspeita e de elegibilidade são parâmetros, definidos pela Defesa Civil de cada município. O mesmo sistema escala para qualquer cidade, bastando instalar sensores e cadastrar os endereços cobertos.

## 13. Validação com pessoas reais

_TODO: registrar as conversas com moradores de áreas que alagam:_
1. Onde alaga e o que acontece?
2. Como você fica sabendo hoje, e como comprova o prejuízo?
3. O que mudaria a sua vida depois de um alagamento?
4. **O que mudou no projeto depois dessas conversas.**

## 14. Uso de IA generativa

Conforme o regulamento, declaramos as ferramentas de IA generativa utilizadas e para quê. Todo o conteúdo gerado foi lido, compreendido, revisado e adaptado pelo autor.

| Ferramenta | Uso |
|---|---|
| Claude (Anthropic) | Apoio à ideação e ao recorte do problema, discussão de arquitetura e modelagem de domínio (DDD), explicação de erros, revisão de código, rascunho do simulador e desta documentação |
| _TODO_ | _Completar com qualquer outra ferramenta usada (ex.: GitHub Copilot)_ |

## 15. Integrantes

| Nome | RM |
|---|---|
| _TODO_ | _TODO_ |