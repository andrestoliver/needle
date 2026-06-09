# Needle

Needle é um diário social de álbuns inspirado no Letterboxd. Usuários podem
registrar álbuns ouvidos, publicar avaliações e resenhas, acompanhar outras
pessoas e receber notificações sobre atividades relevantes.

O projeto é uma POC de estudo. Seu objetivo principal não é entregar um produto
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
- Montar uma pipeline de CI/CD com GitHub Actions.
- Registrar decisões arquiteturais e seus trade-offs.

## Escopo inicial

A primeira versão deve permitir:

1. Cadastrar álbuns manualmente.
2. Registrar que um usuário ouviu um álbum.
3. Avaliar um álbum de 1 a 5 estrelas.
4. Escrever uma resenha.
5. Consultar avaliações de um álbum.
6. Calcular a nota média de um álbum.

Funcionalidades sociais, mensageria e integrações externas serão adicionadas em
etapas posteriores, quando houver um problema concreto que justifique cada uma.

## Catálogo de álbuns

Álbuns fazem parte do domínio do Needle. Inicialmente, serão cadastrados
manualmente e persistidos no PostgreSQL.

Em uma etapa futura, o MusicBrainz poderá ser usado como fonte externa para
importação sob demanda:

1. O usuário pesquisa no catálogo externo.
2. O Needle apresenta uma quantidade limitada de resultados.
3. Somente o álbum escolhido é persistido localmente.
4. O modelo externo é convertido para o modelo do Needle.

O sistema não deve sincronizar o catálogo completo nem armazenar áudio, imagens
ou respostas JSON brutas. Capas devem ser referenciadas por URL. Essa estratégia
mantém o consumo de disco pequeno e permite estudar integração HTTP, cache,
rate limiting e Anti-Corruption Layer sem tornar a POC dependente do
MusicBrainz.

## Estratégia arquitetural

O Needle começará como um **monólito modular**.

Essa escolha reduz o custo operacional durante as primeiras etapas e permite
descobrir as fronteiras do domínio antes de distribuí-las pela rede. Criar
microsserviços desde o início adicionaria deploys, observabilidade distribuída,
mensageria e falhas de rede antes de sabermos se essas fronteiras são boas.

Isso não significa construir um sistema sem separação. O código seguirá DDD e
Clean Architecture, com dependências apontando de fora para dentro:

```text
Api -> Infrastructure -> Application -> Domain
          Application -----------------> Domain
```

O projeto `Domain` não deve depender de banco de dados, mensageria, ASP.NET Core
ou detalhes de infraestrutura. O projeto `Api` será o ponto de composição da
aplicação.

Estrutura inicial planejada:

```text
Needle.slnx
src/
  Needle.Api/
  Needle.Application/
  Needle.Domain/
  Needle.Infrastructure/
tests/
  Needle.UnitTests/
  Needle.IntegrationTests/
```

Novos projetos só devem ser criados quando representarem uma fronteira ou
necessidade real. DDD não exige uma classe, interface ou projeto para cada
conceito.

## Evolução planejada

### Fase 1 - Fundação

- Criar a solution e os projetos.
- Modelar o domínio mínimo de álbuns e avaliações.
- Implementar os primeiros casos de uso.
- Adicionar testes unitários.

### Fase 2 - Persistência e API

- Adicionar PostgreSQL e migrations.
- Criar endpoints REST.
- Adicionar testes de integração.
- Dockerizar a API e o banco.

### Fase 3 - Catálogo externo

- Integrar com MusicBrainz usando `HttpClient`.
- Implementar importação sob demanda.
- Tratar timeout, indisponibilidade, rate limit e cache.
- Isolar o modelo externo com uma Anti-Corruption Layer.

### Fase 4 - Recursos sociais

- Adicionar usuários e seguidores.
- Publicar atividades de avaliações.
- Construir um feed simples.

### Fase 5 - Eventos e Kafka

- Introduzir eventos de negócio como `AlbumReviewed`.
- Implementar Transactional Outbox.
- Estudar particionamento, consumer groups, ordenação, retenção e replay.
- Garantir idempotência nos consumidores.

### Fase 6 - Tarefas e RabbitMQ

- Transformar eventos relevantes em tarefas de notificação.
- Implementar workers, retry com backoff e DLQ.
- Comparar eventos duráveis no Kafka com comandos de execução no RabbitMQ.

### Fase 7 - Observabilidade

- Instrumentar logs estruturados, métricas e tracing com OpenTelemetry.
- Usar Prometheus, Grafana e uma ferramenta de visualização de traces.
- Propagar contexto entre HTTP, Kafka e RabbitMQ.

### Fase 8 - CI/CD

- Executar build, análise e testes no GitHub Actions.
- Construir imagens Docker.
- Aplicar versionamento e validações de qualidade.

O roteiro é uma direção, não um contrato. Cada tecnologia deve entrar para
resolver um problema observável, acompanhada de uma discussão sobre alternativas
e custo operacional.

## Infraestrutura prevista

- .NET
- ASP.NET Core
- PostgreSQL
- Kafka
- RabbitMQ
- Docker e Docker Compose
- OpenTelemetry
- Prometheus
- Grafana
- GitHub Actions

Os componentes não precisam executar todos ao mesmo tempo. Durante o
desenvolvimento local, somente a infraestrutura necessária para a etapa atual
deve ser iniciada, reduzindo o uso de memória e disco.

## Forma de trabalho

O desenvolvimento será incremental:

1. Definir um pequeno objetivo.
2. Explicar a decisão e os conceitos envolvidos.
3. Registrar trade-offs e alternativas relevantes.
4. Implementar apenas o incremento atual.
5. Escrever e executar os testes correspondentes.
6. Revisar o resultado antes de avançar.

Agentes podem escrever o código, mas decisões de domínio e arquitetura devem
permanecer explícitas e compreensíveis para o mantenedor. As regras detalhadas
para agentes estão em [AGENTS.md](AGENTS.md).

## Commits

O projeto adota [Conventional Commits](https://www.conventionalcommits.org/):

```text
<tipo>(escopo opcional): <descrição curta>
```

Exemplos:

```text
feat: add album review
fix: prevent duplicate listening record
test: cover album rating rules
docs: document MusicBrainz integration decision
chore: configure solution tooling
refactor: extract rating value object
```

O tipo do commit é obrigatório. Emojis não fazem parte do padrão do projeto,
mantendo compatibilidade direta com changelogs, versionamento semântico e outras
automações.

## Fora do escopo inicial

- Streaming ou armazenamento de música.
- Armazenamento local de capas.
- Sincronização integral de catálogos externos.
- Sistema de recomendação.
- Frontend.
- Autenticação completa.
- Kubernetes.
- Microsserviços independentes antes de existirem motivos para extração.

