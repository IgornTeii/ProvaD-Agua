# Prova d'Água

> **Quando a água sobe, o seu direito fica registrado.**

Projeto da **Global Solution 2026/2 — CitizenTech** (FIAP, Engenharia de Software, turma 4ESOA).

🎥 **Vídeo pitch:** _TODO: link público (YouTube, Drive ou Vimeo), testado em aba anônima_

[![ci](https://github.com/IgornTeii/ProvaD-Agua/actions/workflows/ci.yml/badge.svg)](https://github.com/IgornTeii/ProvaD-Agua/actions/workflows/ci.yml)

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
13. [Limitações conhecidas](#13-limitações-conhecidas)
14. [Uso de IA generativa](#14-uso-de-ia-generativa)
15. [Integrantes](#15-integrantes)

---

## 1. O problema

Soluções de enchente costumam resolver o **antes**: previsão e alerta. Quase ninguém resolve o **depois**.

Quem perde móveis, eletrodomésticos, carro ou dias de trabalho num alagamento precisa **provar que foi atingido** para pedir auxílio, seguro ou saque emergencial. Hoje essa prova depende de foto, protocolo, vistoria e semanas de espera, e quem mais sofre é justamente quem tem menos condição de esperar.

**Um exemplo real:** no temporal de 10 de fevereiro de 2020, o Centro de Gerenciamento de Emergências Climáticas (CGE) da Prefeitura registrou 132 pontos de alagamento em São Paulo, 66 deles intransitáveis ([ISTOÉ, 10/02/2020](https://istoe.com.br/sp-chuva-forte-provoca-alagamento-e-interdita-marginais/)). A Av. Antônio Munhoz Bonilha, um dos 8 pontos monitorados neste protótipo, estava na lista de alagamentos do CGE naquele dia.

## 2. O cidadão beneficiado

Moradores de ruas e bairros que alagam com frequência, principalmente famílias de baixa renda, para quem um alagamento significa perda de bens e de renda.

**Persona:** Dona Maria mora numa rua que alaga toda vez que chove forte. Depois de cada enchente, precisa juntar fotos e enfrentar filas para comprovar o prejuízo. Com o Prova d'Água, ela digita o nome da rua, vê a comprovação do seu endereço e mostra o código para o órgão de auxílio, sem fila e sem precisar provar sozinha o que a rua inteira viveu.

## 3. A solução

Sensores instalados na rua medem **nível de água** e **chuva** continuamente. Quando uma **IA confirma** que a subida da água é um alagamento real, e não ruído do sensor, o sistema:

1. registra o evento de alagamento com início, fim e nível máximo atingido;
2. **avisa por SMS (simulado)** os moradores que cadastraram o celular;
3. **emite automaticamente uma comprovação por endereço** da área afetada, com um código de verificação;
4. indica se o endereço é **elegível a auxílio** (pico a partir de 30 cm);
5. mostra tudo num **mapa de São Paulo** e num **histórico** pesquisável por data e por rua.

### A história de um alagamento

| Momento | O que acontece no sistema |
|---|---|
| Começa a chover | Cada leitura dos sensores é gravada (imutável: é a prova) |
| Nível passa de 15 cm | Abre um **evento Suspeito** e o ponto fica **amarelo** no mapa |
| IA analisa a sequência de leituras | **Alagamento** → Confirmado (ponto **vermelho**, SMS enviado) · **Ruído** → Descartado · **Inconclusivo** → aguarda o próximo ciclo |
| Água sobe e desce | O evento guarda o **pico** atingido |
| Água fica abaixo de 15 cm por 18 leituras seguidas | O evento é **Encerrado** |
| No mesmo instante | Uma **comprovação por endereço** é emitida, com código `PDA-XXXX-XXXX` |
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
| **Cidadão** | Consulta a situação da rua, recebe o aviso e pega a comprovação do seu endereço |
| **Defesa Civil** | Define os limites (suspeita e elegibilidade) e acompanha o mapa e o histórico |
| **Prefeitura, órgãos de auxílio e seguradoras** | Verificam a autenticidade das comprovações pelo código |
| **Sensores (IoT)** | Medem nível de água e chuva na rua |

## 5. ODS atendidos

- **ODS 11 — Cidades e comunidades sustentáveis:** reduz o impacto de desastres sobre a população urbana (meta 11.5).
- **ODS 13 — Ação contra a mudança global do clima:** fortalece a resiliência a eventos climáticos extremos (meta 13.1).
- **ODS 10 — Redução das desigualdades:** encurta o caminho até o auxílio para quem mais precisa e menos consegue esperar.

## 6. Arquitetura

O diagrama completo está no PDF de entrega.

```
 ┌──────────────┐  POST /readings   ┌───────────────────────────┐   POST /classify   ┌──────────────┐
 │  Simulador   │ ────────────────► │        API (.NET 10)      │ ─────────────────► │  IA (Python) │
 │  IoT (mock)  │   X-Sensor-Key    │  Clean Architecture + DDD │ ◄───────────────── │ scikit-learn │
 └──────▲───────┘                   └─────────────┬─────────────┘  flood/noise/...   └──────────────┘
        │ inicia cenários                         │
        │                                  ┌──────▼──────┐
        │                                  │ PostgreSQL  │──────► Dashboard (Power BI)
        │                                  └──────▲──────┘
        │                                         │ consultas
 ┌──────┴─────────────────────────────────────────┴──────┐
 │              Site (React + TypeScript)                 │
 │  mapa · minha rua · histórico · comprovações · simular │
 └────────────────────────────────────────────────────────┘
```

**Princípio:** serviços separados num único repositório. Nenhum serviço importa código de outro; eles só se comunicam por HTTP. Cada um tem seu próprio Dockerfile.

**A IA opina, a API decide:** a IA só classifica uma sequência de leituras. Quem aplica a regra de negócio (confirmar, descartar, encerrar, emitir comprovação) é a API. Assim a regra fica num lugar só, e o modelo pode ser trocado sem alterar o domínio.

### Estrutura do repositório

```
prova-dagua/
├── api/                    # C# ASP.NET Core (.NET 10)
│   ├── src/
│   │   ├── ProvaDagua.Domain           # Regras de negócio puras
│   │   ├── ProvaDagua.Application      # Casos de uso e contratos
│   │   ├── ProvaDagua.Infrastructure   # EF Core, PostgreSQL, cliente da IA
│   │   └── ProvaDagua.Api              # Controllers, autenticação do sensor, worker
│   └── tests/ProvaDagua.UnitTests
├── iot/                    # Python: sensores simulados, cenários e servidor do simulador
│   └── tests/
├── ai/                     # Python: dataset, treino, métricas e serviço de classificação
│   └── tests/
├── web/                    # React + TypeScript (Vite)
│   └── tests/              # unit/ (Vitest) e e2e/ (Playwright)
├── data/raw/               # Dados brutos gerados para o treino da IA
├── tests/smoke/            # Teste do sistema inteiro rodando no Docker
├── dashboard/              # Power BI (.pbix) e prints
├── docs/                   # Diagramas, evidências e PDF
├── .github/workflows/      # CI (GitHub Actions)
└── docker-compose.yml
```

### Decisões técnicas

| Parte | Tecnologia | Motivo |
|---|---|---|
| API | C# / ASP.NET Core (.NET 10 LTS) + EF Core | Versão com suporte longo; Clean Architecture com Result pattern |
| Banco | PostgreSQL 17 (Docker) | Índices únicos e parciais garantem as regras de idempotência |
| IA | Python + scikit-learn (Random Forest) + Flask | Classificação explicável e mensurável, sem custo de API |
| IoT | Simulador em Python (mock), só biblioteca padrão | Permitido pelo regulamento; reproduz uma enchente sob demanda |
| Site | React 19 + TypeScript + Vite, Leaflet (OpenStreetMap) | Mapa real de São Paulo, sem chave de API |
| Dashboard | Power BI Desktop + DAX | — |
| CI | GitHub Actions | Gratuito em repositório público |

## 7. Como executar

### Pré-requisito

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Subir tudo

```bash
docker compose up --build -d
```

Na primeira vez demora alguns minutos: as imagens são construídas, a IA é treinada dentro do build e as tabelas do banco são criadas automaticamente quando a API sobe.

| Serviço | Endereço |
|---|---|
| **Site** | http://localhost:3000 |
| API (Swagger) | http://localhost:5016/swagger |
| IA | http://localhost:8000/health |
| Simulador | http://localhost:8100/scenarios |
| PostgreSQL | `localhost:5432` (usuário, senha e banco: `provadagua`) |

### Simular um alagamento

1. Abra http://localhost:3000/simular.
2. Em **Ponto escolhido**, selecione a rua; depois escolha o cenário (por exemplo, **Alagamento de verdade**).
3. Clique em **Simular** e acompanhe as etapas, o gráfico e o mapa.
4. Quando terminar, abra **Comprovações** e escolha a rua para ver as comprovações emitidas.

| Cenário | O que simula | Resultado esperado |
|---|---|---|
| Alagamento de verdade | Chuva forte, a água sobe, fica alta e baixa | Confirmado, encerrado e comprovações emitidas |
| Enxurrada rápida | Subida brusca | Confirmado mesmo com a subida rápida |
| Alagamento sem chuva no bairro | Água vem de outro bairro | A IA reconhece pelo comportamento da água |
| Carro jogando água no sensor | Espirro de poucos segundos | Suspeita aberta e descartada pela IA |
| Lixo parado embaixo do sensor | Obstrução de alguns minutos | Suspeita aberta e descartada pela IA |
| Chuva fraca | Água sobe poucos centímetros | Nada acontece |

O simulador também tem uma linha de comando: `docker compose run --rm simulator python main.py flood`.

### Começar do zero

```bash
docker compose down -v
docker compose up --build -d
```

### Rodar os testes

| Parte | Comando | Pré-requisito |
|---|---|---|
| API | `cd api && dotnet test` | .NET SDK 10 |
| IA | `cd ai && pip install -r requirements-dev.txt && python -m pytest` | Python 3.13 |
| Simulador | `cd iot && pip install -r requirements-dev.txt && python -m pytest` | Python 3.13 |
| Site | `cd web && npm ci && npm test` | Node.js 22 |
| Site no navegador | `cd web && npx playwright install chromium && npm run test:e2e` | Node.js 22 |
| Sistema inteiro | `docker compose up --build -d && python tests/smoke/smoke.py --site http://localhost:3000` | Docker e Python |

## 8. Área 1 — IA e automação

**Problema que a IA resolve:** um sensor de nível não distingue um alagamento real de um carro jogando água nele. Uma regra fixa ("acima de X cm = alagou") gera falsos alarmes e, portanto, comprovações e auxílios indevidos.

**Como funciona:** a IA recebe uma **janela de leituras** (nunca uma leitura isolada) e calcula 8 características da sequência: nível máximo, médio e oscilação, tendência de subida, maior salto entre leituras, fração e maior sequência acima de 15 cm, e chuva média. Um **Random Forest** classifica em **alagamento** ou **ruído**. Com menos de 6 leituras, ou confiança abaixo de 80%, a resposta é **inconclusiva**.

**Automação:** um worker em segundo plano na API roda em ciclos (30 s em produção, 2 s na demonstração). A cada ciclo ele envia à IA as leituras de cada evento Suspeito e aplica o resultado. Também encerra os eventos cuja água baixou e emite as comprovações. Se a IA estiver fora do ar, o evento continua Suspeito: o sistema nunca confirma nem descarta "por garantia".

**Métrica (antes × depois)** em 1.000 janelas de teste separadas do treino (dados simulados):

| | Regra fixa (≥ 15 cm por 3 leituras) | Modelo |
|---|---|---|
| Acurácia | 75,6% | **99,9%** |
| Precisão (alagamento) | 67,4% | **100%** |
| Recall (alagamento) | 99,0% | **99,8%** |
| Falsos alarmes em 500 ruídos | 239 | **0** |
| Alagamentos perdidos em 500 | 5 | **1** |

> As métricas foram medidas em dados gerados pelo simulador, não em dados reais de campo. O próximo passo é recalibrar com leituras de sensores instalados.

O CI retreina o modelo a cada commit e **falha se ele ficar abaixo de 95%** de acurácia, precisão ou recall, ou se deixar de ganhar da regra fixa.

**Demonstração (entrada → saída):**

```bash
curl -X POST http://localhost:8000/classify -H "Content-Type: application/json" \
  -d '{"readings":[{"waterLevelCm":16,"rainfallMmPerHour":42},{"waterLevelCm":19,"rainfallMmPerHour":45},
       {"waterLevelCm":23,"rainfallMmPerHour":47},{"waterLevelCm":27,"rainfallMmPerHour":50},
       {"waterLevelCm":31,"rainfallMmPerHour":52},{"waterLevelCm":34,"rainfallMmPerHour":51}]}'
```

```json
{ "label": "flood", "confidence": 1.0 }
```

Com um carro jogando água no sensor (um único pico de 38 cm sem chuva), a resposta é `{ "label": "noise", "confidence": 0.995 }`. Com menos de 6 leituras, é `inconclusive`.

Para retreinar: `cd iot && python generate_dataset.py` e depois `cd ai && python train.py`. O resultado fica em `ai/metrics.json`. O treino usa semente fixa: com as mesmas versões das bibliotecas, os números se repetem a cada execução.

## 9. Área 2 — Big Data e visualização

_Status: ⏳ dashboard em desenvolvimento_

**Estrutura dos dados (camadas):**

| Camada | Conteúdo | Onde está |
|---|---|---|
| **Bruto** | Leitura como chegou do sensor. Nunca é apagada: é a prova | Tabela `readings` e `data/raw/training_readings.csv` |
| **Tratado** | Sem duplicatas e sem valores impossíveis, com datas em UTC | Validação no domínio e índices únicos do banco |
| **Analítico** | Eventos, comprovações e agregados para o dashboard | Tabelas `flood_events` e `flood_certificates` |

**Ingestão e processamento:** sensor → `POST /readings` (chave do sensor) → validação → gravação → evento Suspeito → classificação pela IA → evento Confirmado ou Descartado → encerramento → comprovações.

**Dashboard (Defesa Civil):** _TODO_
- Alagamentos por bairro e período
- Endereços atingidos e elegíveis a auxílio
- Falsos alarmes evitados pela IA
- Tempo entre o início do evento e a confirmação

**Funções analíticas (DAX):** _TODO_

## 10. Área 3 — MVP, IoT e experiência digital

### IoT — 2 sensores (simulados)

| Sensor | Equivalente real | Mede | Frequência |
|---|---|---|---|
| **Nível de água** | Ultrassônico HC-SR04, instalado a 2 m acima da rua | Distância até a superfície, convertida em altura da água (cm) | A cada 10 s |
| **Pluviômetro** | Pluviômetro de báscula | Intensidade da chuva (mm/h) | A cada 10 s |

O simulador reproduz defeitos reais: ruído de medição, perda de eco do ultrassônico e obstruções (lixo, carro) que o sensor "enxerga" como água.

**Por que dois sensores:** o nível sozinho não distingue um alagamento de um carro jogando água no sensor. **A chuva é o contexto** que a IA usa para decidir.

### 8 pontos reais de São Paulo

Os sensores ficam em ruas que costumam alagar (Rua Amazonas em São Miguel Paulista, Av. Luiz Ignácio de Anhaia Mello, Av. Aricanduva, Av. Jacu-Pêssego, Av. do Estado, Av. Pompeia, Av. Doutor Ricardo Jafet e Av. Antônio Munhoz Bonilha), com 3 endereços cobertos cada. As coordenadas são aproximadas. Os locais são reais; os alagamentos são simulados.

### Protótipo — telas

| Tela | O que o cidadão faz |
|---|---|
| **Início** | Entende o que é o serviço, vê se algum ponto está alagando agora e escolhe o que quer fazer |
| **Mapa agora** | Vê os 8 pontos de São Paulo: verde normal, amarelo suspeita, vermelho alagamento confirmado |
| **Minha rua** | Busca o endereço, vê a situação com gráfico da água, recebe orientações e cadastra o celular para o aviso |
| **Histórico** | Pesquisa alagamentos por data, rua ou bairro e abre os detalhes de cada um |
| **Comprovações** | Escolhe a rua e vê, imprime ou compartilha as comprovações |
| **Verificar código** | Órgão público confere se uma comprovação é autêntica |
| **Simulador** | Dispara um dos 6 cenários e acompanha cada etapa ao vivo |

Acessibilidade: fonte Atkinson Hyperlegible (feita para baixa visão), botões para aumentar o texto e alto contraste, leitura em voz alta, atalho para pular ao conteúdo, navegação por teclado e layout para celular. Os telefones de emergência (199, 193 e 192) ficam no rodapé de todas as páginas.

### Endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/readings` | Ingestão de leituras (exige `X-Sensor-Key`): 202, 400 ou 401 |
| `GET` | `/sensors` | Os 8 pontos com a situação atual |
| `GET` | `/sensors/{id}/status` | Última leitura e evento aberto de um sensor |
| `GET` | `/addresses?search=` | Busca de endereços cobertos |
| `GET` | `/addresses/{id}/notifications` | Avisos enviados ao endereço (telefone mascarado) |
| `POST` | `/alerts/subscriptions` | Cadastro do celular para o aviso |
| `GET` | `/flood-events` | Histórico com filtros por sensor, status, período e rua |
| `GET` | `/flood-events/{id}` | Detalhe de um alagamento |
| `GET` | `/certificates?addressId=` ou `?floodEventId=` | Comprovações de um endereço ou de um alagamento |
| `GET` | `/certificates/{code}/verify` | Verifica a autenticidade (404 se não existe) |

### Fluxo funcional completo

1. No **Simulador**, o usuário dispara "Alagamento de verdade" na Rua Amazonas.
2. As leituras chegam em `POST /readings`. Ao passar de 15 cm, abre um evento Suspeito e o ponto fica amarelo no mapa.
3. O worker envia a janela à IA, que responde "alagamento". O evento é confirmado, o ponto fica vermelho e quem cadastrou o celular recebe o aviso.
4. A água baixa. Depois de 18 leituras abaixo de 15 cm, o evento é encerrado e cada um dos 3 endereços recebe uma comprovação.
5. Em **Comprovações**, o morador escolhe a rua e vê a comprovação com o código.
6. Em **Verificar código**, um órgão público digita o código e confirma que ela é autêntica.

Esse fluxo é testado automaticamente no CI (`tests/smoke/smoke.py`), junto com o caso de ruído: um splash precisa ser descartado pela IA.

## 11. Área 4 — DevOps, segurança e qualidade

### Organização

Monorepo com serviços independentes, versionado no Git. A API usa Clean Architecture com 4 projetos, e a direção das dependências é garantida pelo compilador.

### DevOps

- **Docker:** cada serviço tem seu Dockerfile, e o `docker-compose.yml` sobe os 5 (banco, IA, API, simulador e site) com healthchecks e dependências.
- **CI (GitHub Actions):** cada push roda 6 jobs:

| Job | O que valida |
|---|---|
| API .NET | build e testes xUnit |
| IA Python | testes e quality gate do modelo, com a tabela de métricas no resumo da execução |
| Simulador IoT | testes dos sensores, cenários e servidor |
| Site (unitários) | tipos, lint e testes com Vitest |
| Site (navegador) | testes com Playwright no desktop e no celular |
| Sistema completo | sobe tudo com Docker Compose e roda um alagamento e um splash de ponta a ponta |

### Segurança

| Prática | Como foi aplicada |
|---|---|
| **Autenticação do sensor** | Cada sensor envia uma chave no cabeçalho `X-Sensor-Key`. **A identidade do sensor vem da chave, nunca do corpo da requisição**: uma chave vazada só compromete o próprio sensor e não permite forjar alagamentos em outras ruas |
| **Validação de entrada** | O domínio recusa leituras impossíveis (nível fora de 0–500 cm, chuva negativa, data no futuro), o cadastro exige celular com 10 ou 11 dígitos e a IA e o simulador validam o formato do que recebem |
| **Integridade e idempotência** | Índices únicos impedem leituras duplicadas, mais de um evento aberto por sensor, mais de uma comprovação por endereço por evento e mais de um aviso por cadastro por evento |
| **Comprovação não adivinhável** | Código `PDA-XXXX-XXXX` gerado com gerador criptográfico, sem caracteres ambíguos (sem 0, O, 1, I ou L) |
| **Privacidade** | O telefone nunca volta inteiro pela API, só mascarado, por exemplo `(11) •••••-4321`. As fontes do site são servidas pelo próprio site, sem enviar o IP do cidadão a terceiros |
| **CORS restrito** | A API só aceita chamadas do navegador vindas do próprio site |

_Observação: as chaves de sensor deste repositório são de desenvolvimento. Em produção, viriam de variáveis de ambiente ou de um cofre de segredos e seriam armazenadas como hash._

### Testes

São 81 testes automatizados, mais o teste do sistema inteiro:

| Parte | Ferramenta | Testes |
|---|---|---|
| Domínio da API | xUnit | 8 |
| IA | pytest | 19 |
| Simulador IoT | pytest | 25 |
| Site | Vitest + Testing Library | 21 |
| Site no navegador | Playwright (desktop e celular) | 4 cenários × 2 telas |
| Sistema completo | `tests/smoke/smoke.py` | 1 fluxo de ponta a ponta |

O plano de testes com casos, resultados esperados e evidências está no PDF.

## 12. Impacto e como medimos

| Indicador | Hoje | Com o Prova d'Água |
|---|---|---|
| Tempo até a comprovação | Semanas (foto, protocolo, vistoria) | Logo depois que a água baixa |
| Quem prova o alagamento | O próprio cidadão | O sensor, de forma automática |
| Falsos alarmes (dados simulados) | 239 em 500 com a regra fixa | 0 em 500 com a IA |
| Aviso ao morador | Nenhum | SMS quando o alagamento é confirmado |

**Como ampliar:** os limites de suspeita e de elegibilidade são parâmetros definidos pela Defesa Civil de cada município. O mesmo sistema escala para qualquer cidade, bastando instalar sensores e cadastrar os endereços cobertos.

## 13. Limitações conhecidas

- As métricas da IA foram medidas em dados simulados.
- Se a IA ficar fora do ar durante todo o alagamento, o evento fica Suspeito e não gera comprovação.
- A IA analisa as leituras mais recentes do sensor, e não uma janela presa ao início do evento.
- As comprovações são públicas por código, sem login. O próximo passo é a integração com o gov.br.
- O SMS é simulado: a mensagem é gravada e mostrada no site, mas não é enviada a uma operadora.

## 14. Uso de IA generativa

Conforme o regulamento, declaro as ferramentas de IA generativa usadas e para quê. Todo o conteúdo gerado foi lido, entendido e revisado por mim antes de entrar no projeto.

| Ferramenta | Uso |
|---|---|
| Claude (Anthropic) | Apoio na ideação, em dúvidas de arquitetura e de erros, e na geração e revisão de código e documentação |

## 15. Integrantes

| Nome | RM |
|---|---|
| Igor Nunes Teixeira | _TODO_ |
