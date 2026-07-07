# Needle

Needle é um diário social de álbuns inspirado no Letterboxd. Usuários podem
registrar álbuns, publicar avaliações e resenhas, consultar reviews de outras
pessoas e, futuramente, acompanhar atividades sociais relacionadas a música.

O projeto é uma POC de estudo. O objetivo principal não é entregar um produto
completo, mas exercitar decisões de arquitetura e práticas comuns em sistemas
.NET usados em produção.

## Objetivos de aprendizado

- Construir um sistema inicialmente simples e evoluí-lo com segurança.
- Aplicar DDD, Clean Architecture e Clean Code de forma pragmática.
- Praticar comunicação síncrona e assíncrona.
- Entender quando usar Kafka e quando usar RabbitMQ.
- Implementar idempotência, retry, DLQ e consistência eventual.
- Dockerizar aplicações e infraestrutura.
- Criar testes unitários e de integração relevantes.
- Instrumentar logs, métricas e traces com OpenTelemetry.
- Montar pipelines de CI/CD com GitHub Actions.
- Registrar decisões arquiteturais e seus trade-offs.
- Praticar system design em incrementos pequenos e verificáveis.

## Escopo atual

A API atual permite:

- cadastrar álbuns manualmente;
- consultar álbuns por id;
- pesquisar álbuns no MusicBrainz sem persistir no banco local;
- importar álbuns sob demanda do MusicBrainz;
- criar reviews para álbuns;
- listar reviews de um álbum;
- consultar uma review específica;
- atualizar uma review;
- deletar uma review.

A autenticação atual usa um JWT de desenvolvimento para permitir testes locais.
Endpoints de escrita de review devem usar o usuário autenticado a partir das
claims do token, não um `userId` enviado pelo cliente no payload.

## Endpoints principais

| Método | Rota | Descrição |
| --- | --- | --- |
| `POST` | `/api/albums` | Cadastra um álbum manualmente |
| `GET` | `/api/albums/{id}` | Consulta um álbum local por id |
| `GET` | `/api/catalog/albums?query={query}&limit={limit}` | Pesquisa álbuns no MusicBrainz |
| `POST` | `/api/albums/import` | Importa um álbum do MusicBrainz para o catálogo local |
| `POST` | `/api/albums/{albumId}/reviews` | Cria uma review para um álbum |
| `GET` | `/api/albums/{albumId}/reviews` | Lista reviews de um álbum |
| `GET` | `/api/albums/{albumId}/reviews/{reviewId}` | Consulta uma review específica |
| `PUT` | `/api/albums/{albumId}/reviews/{reviewId}` | Atualiza uma review |
| `DELETE` | `/api/albums/{albumId}/reviews/{reviewId}` | Remove uma review |

## Autenticação

A API possui um endpoint temporário para emissão de JWT em ambiente de estudo:

```http
POST /api/auth/dev-token
```

Esse endpoint existe apenas para facilitar testes locais da POC enquanto ainda
não há login real, BFF ou provedor de identidade externo.

Ele não representa um fluxo seguro de produção. Em um ambiente real, tokens
seriam emitidos por um provedor confiável, como Cognito, Auth0, Keycloak, Azure
AD ou outro Identity Provider, e a API apenas validaria assinatura, issuer,
audience, expiração e permissões do token.

Enquanto esse fluxo real não existe, o token gerado localmente será usado para
remover `userId` dos payloads de reviews e ler o usuário autenticado a partir
das claims.

## Regras de negócio atuais

### Álbuns

- Um álbum pode ser criado manualmente.
- Um álbum pode ser importado do MusicBrainz sob demanda.
- O Needle não sincroniza catálogos completos.
- O `externalId` identifica a origem externa quando o álbum veio do MusicBrainz.
- Álbuns importados preservam a separação entre domínio interno e modelo externo.

### Reviews

- Uma review pertence a um álbum e a um usuário.
- Um usuário pode ter apenas uma review por álbum.
- A nota deve estar entre `0.5` e `5.0`.
- A nota deve avançar em incrementos de `0.5`.
- O texto da review é opcional.
- O texto da review não pode ultrapassar 2000 caracteres.
- Atualizações preservam `CreatedAt` e preenchem `UpdatedAt`.
- Deletes atuais são hard deletes.
- Ownership deve ser validado usando o usuário autenticado nas claims do JWT.

## Catálogo de álbuns

Álbuns fazem parte do domínio do Needle. Eles podem ser cadastrados manualmente
ou importados sob demanda do MusicBrainz.

O MusicBrainz é tratado como uma fonte externa, não como dono do domínio. A API
externa é acessada pela camada de infraestrutura e convertida para modelos da
camada de aplicação antes de atravessar as fronteiras internas.

Fluxo de importação:

1. O usuário pesquisa no catálogo externo.
2. O Needle apresenta uma quantidade limitada de resultados.
3. O usuário escolhe um álbum.
4. Somente o álbum escolhido é persistido localmente.
5. O modelo externo é convertido para o modelo interno do Needle.

O sistema não deve sincronizar o catálogo completo nem armazenar áudio, imagens
ou respostas JSON brutas. Essa estratégia mantém o consumo de disco pequeno e
permite estudar integração HTTP, timeout, rate limiting, cache e
Anti-Corruption Layer sem tornar a POC dependente do MusicBrainz.

## Estratégia arquitetural

O Needle começa como um **monólito modular**.

Essa escolha reduz o custo operacional durante as primeiras etapas e permite
descobrir as fronteiras do domínio antes de distribuí-las pela rede. Criar
microsserviços desde o início adicionaria deploys, observabilidade distribuída,
mensageria e falhas de rede antes de sabermos se essas fronteiras são boas.

Isso não significa construir um sistema sem separação. O código segue DDD e
Clean Architecture, com dependências apontando de fora para dentro:

```text
Needle.Api -> Needle.Infrastructure -> Needle.Application -> Needle.Domain
Needle.Api ---------------------------> Needle.Application
Needle.Infrastructure ----------------> Needle.Domain
```

## Docker

O projeto pode ser executado localmente com Docker Compose:

```bash
docker compose up -d --build
```

Esse comando sobe:

- `api`: aplicação ASP.NET Core;
- `postgres`: banco PostgreSQL usado pela API;
- `jaeger`: visualização local de traces;
- `prometheus`: coleta local de métricas;
- `grafana`: dashboards locais de métricas.

A API fica disponível em:

```text
http://localhost:5195
```

Para acompanhar logs:

```bash
docker compose logs -f api
```

Para parar os containers:

```bash
docker compose down
```

Os dados do PostgreSQL ficam preservados no volume `needle-postgres-data`.
Para remover containers, rede e volume:

```bash
docker compose down -v
```

## Imagem Docker publicada

A cada merge na `main`, o GitHub Actions publica uma imagem da API no GitHub
Container Registry:

```text
ghcr.io/andrestoliver/needle-api
```

Tags publicadas:

- `latest`: última versão publicada a partir da `main`;
- `<commit-sha>`: versão imutável associada a um commit específico.

Exemplo:

```bash
docker pull ghcr.io/andrestoliver/needle-api:latest
```

A imagem publicada contém apenas a API. Para executar corretamente, ela ainda
precisa receber configurações como connection string do PostgreSQL e JWT.

## Observabilidade

O Needle possui uma stack local de observabilidade para estudo e validação do
comportamento da API em ambiente containerizado.

A stack atual inclui:

- logs estruturados em JSON;
- health checks;
- tracing distribuído com OpenTelemetry e Jaeger;
- métricas com Prometheus;
- dashboards com Grafana.

### Endpoints e ferramentas

| Ferramenta | URL | Uso |
| --- | --- | --- |
| API | `http://localhost:5195` | Endpoints HTTP da aplicação |
| Health live | `http://localhost:5195/health/live` | Verifica se o processo da API está vivo |
| Health ready | `http://localhost:5195/health/ready` | Verifica se a API está pronta e consegue acessar dependências como PostgreSQL |
| Metrics | `http://localhost:5195/metrics` | Endpoint coletado pelo Prometheus |
| Jaeger | `http://localhost:16686` | Visualização de traces distribuídos |
| Prometheus | `http://localhost:9090` | Consulta e coleta de métricas |
| Grafana | `http://localhost:3000` | Dashboards de métricas |

Credenciais locais do Grafana:

```text
user: admin
password: admin
```

### Logs

A API escreve logs estruturados em JSON no console. Em Docker, os logs podem ser
consultados com:

```bash
docker compose logs -f api
```

Logs ajudam a responder:

```text
O que aconteceu?
Qual erro foi registrado?
Quais eventos ocorreram durante a execução?
```

### Health checks

A API expõe dois health checks:

```text
/health/live
/health/ready
```

`/health/live` indica se o processo da API está vivo. Ele não depende de banco
ou serviços externos.

`/health/ready` indica se a API está pronta para receber tráfego real. Ele inclui
a checagem do PostgreSQL.

Essa separação evita confundir uma aplicação viva com uma aplicação pronta para
atender requisições de negócio.

### Tracing

A API usa OpenTelemetry para gerar traces de requisições HTTP recebidas e
chamadas HTTP externas.

Os traces são enviados via OTLP para o Jaeger quando o endpoint OTLP está
configurado.

Fluxo local:

```text
Needle.Api -> OTLP -> Jaeger
```

No Jaeger, selecione o serviço:

```text
Needle.Api
```

Um trace permite enxergar a linha do tempo de uma requisição, por exemplo:

```text
GET /api/catalog/albums
└── HTTP GET musicbrainz.org
```

Isso ajuda a responder:

```text
Por onde essa requisição passou?
Quanto tempo cada etapa levou?
A lentidão está na API ou em uma dependência externa?
```

### Métricas

A API expõe métricas no endpoint:

```text
/metrics
```

O Prometheus coleta esse endpoint periodicamente usando a configuração em:

```text
observability/prometheus/prometheus.yml
```

Fluxo local:

```text
Needle.Api -> /metrics -> Prometheus -> Grafana
```

Métricas ajudam a responder:

```text
Quantas requisições a API recebeu?
Qual a taxa de erro?
Como a latência se comporta ao longo do tempo?
O serviço está disponível?
```

No Prometheus ou Grafana, uma query inicial útil é:

```promql
up
```

Ela indica se os targets monitorados estão disponíveis.

### Grafana

O Grafana usa o Prometheus como datasource e já sobe com provisioning local.

O datasource é configurado em:

```text
observability/grafana/provisioning/datasources/prometheus.yml
```

O dashboard inicial da API é configurado em:

```text
observability/grafana/provisioning/dashboards/needle-api.json
```

URL do datasource dentro do Docker Compose:

```text
http://prometheus:9090
```

Usamos `prometheus` em vez de `localhost` porque o Grafana roda dentro de um
container. Dentro da rede do Docker Compose, serviços se comunicam pelo nome do
serviço.
