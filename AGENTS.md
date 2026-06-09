# AGENTS.md

Este arquivo define as regras para agentes que trabalham no Needle. Elas se
aplicam a todo o repositório, salvo quando um `AGENTS.md` mais específico definir
regras adicionais em um subdiretório.

## Objetivo do projeto

Needle é uma POC educacional para aprofundar arquitetura e engenharia de
software em .NET. Entregar código funcional é necessário, mas não suficiente:
o agente deve tornar decisões, trade-offs e comportamento em produção
compreensíveis para o mantenedor.

## Forma de colaboração

- Trabalhar em incrementos pequenos e verificáveis.
- Não gerar toda a arquitetura ou todas as fases de uma só vez.
- Antes de implementar uma decisão relevante, explicar o problema que ela
  resolve, os trade-offs e ao menos uma alternativa viável.
- Questionar requisitos ou soluções que adicionem complexidade sem benefício
  claro.
- Preferir exemplos de negócio do Needle a definições puramente abstratas.
- Ao concluir uma etapa, informar o que mudou, como foi validado e quais riscos
  permanecem.
- Não introduzir uma tecnologia apenas porque ela consta no roteiro. Primeiro,
  identificar o problema que justifica seu uso.

## Arquitetura obrigatória

O sistema começa como um monólito modular e segue DDD, Clean Architecture e
Clean Code de forma pragmática.

As dependências devem apontar para o domínio:

```text
Needle.Api -> Needle.Infrastructure -> Needle.Application -> Needle.Domain
Needle.Api ---------------------------> Needle.Application
Needle.Infrastructure ----------------> Needle.Domain
```

### Domain

- Contém entidades, value objects, regras e eventos de domínio.
- Não referencia ASP.NET Core, Entity Framework Core, Kafka, RabbitMQ,
  OpenTelemetry ou qualquer adaptador externo.
- Não possui atributos ou modelos ditados por persistência ou transporte.
- Deve proteger invariantes dentro do próprio modelo sempre que possível.
- Não deve ser anêmico apenas para facilitar CRUD.

### Application

- Contém casos de uso e coordena o domínio.
- Define portas necessárias para persistência, relógio, mensageria e serviços
  externos.
- Não conhece detalhes de HTTP, banco, brokers ou serialização.
- Não duplica regras que pertencem ao domínio.

### Infrastructure

- Implementa as portas definidas pelas camadas internas.
- Contém detalhes de PostgreSQL, Entity Framework Core, clientes HTTP,
  mensageria, cache e telemetria quando forem introduzidos.
- Modelos externos devem ser convertidos antes de atravessar a fronteira para
  as camadas internas.

### Api

- Expõe os endpoints HTTP e configura a composição da aplicação.
- Faz validações de formato e transporte, deixando regras de negócio para as
  camadas internas.
- Não acessa diretamente o banco ou brokers.
- Não contém regras de domínio.

Essas regras orientam a direção das dependências, mas não justificam abstrações
sem uso. Não criar interfaces, services, repositories, factories ou handlers
genéricos apenas para imitar um diagrama.

## Estrutura da solution

A estrutura padrão é:

```text
Needle.sln
src/
  Needle.Api/
    Needle.Api.csproj
  Needle.Application/
    Needle.Application.csproj
  Needle.Domain/
    Needle.Domain.csproj
  Needle.Infrastructure/
    Needle.Infrastructure.csproj
tests/
  Needle.UnitTests/
    Needle.UnitTests.csproj
  Needle.IntegrationTests/
    Needle.IntegrationTests.csproj
```

- Código de produção deve ficar em `src/`.
- Projetos de teste devem ficar em `tests/`.
- Cada `.csproj` deve ficar dentro da pasta do próprio projeto.
- Novos projetos exigem uma justificativa arquitetural concreta.
- Organização interna deve acompanhar conceitos e funcionalidades do domínio,
  evitando pastas genéricas que misturem responsabilidades.

## Regras de implementação

- Usar a versão de .NET fixada pelo repositório.
- Respeitar nullable reference types e tratar warnings relevantes.
- Preferir tipos de domínio a strings e números primitivos quando houver
  invariantes ou significado de negócio.
- Usar operações assíncronas para I/O e propagar `CancellationToken`.
- Evitar estado global mutável e dependências ocultas.
- Não capturar exceções sem adicionar contexto, traduzir uma fronteira ou
  executar uma estratégia de recuperação.
- Não registrar dados sensíveis.
- Não adicionar pacotes sem explicar sua necessidade e verificar se a
  plataforma já oferece solução suficiente.
- Manter mudanças relacionadas ao incremento atual; evitar refatorações
  paralelas sem necessidade.
- Comentários devem explicar decisões ou limitações, não repetir o código.

## Testes obrigatórios

O repositório deve possuir projetos de testes unitários e de integração.

### Testes unitários

- Cobrir invariantes do domínio, value objects e casos de uso relevantes.
- Ser rápidos, determinísticos e independentes de rede, disco, banco e brokers.
- Testar comportamento observável, não detalhes privados da implementação.
- Evitar mocks de entidades e value objects.

### Testes de integração

- Cobrir fronteiras reais como API, PostgreSQL, migrations e adaptadores
  externos.
- Usar dependências reais em containers quando isso representar melhor o
  comportamento de produção.
- Isolar dados entre testes e garantir execução repetível.
- Não substituir todos os adaptadores por mocks e chamar o resultado de teste
  de integração.

Toda correção de bug deve incluir, quando viável, um teste que falhe antes da
correção. Toda mudança deve executar ao menos os testes afetados. Se algum teste
não puder ser executado, o agente deve declarar isso explicitamente.

## Mensageria e consistência

Quando Kafka ou RabbitMQ forem introduzidos:

- Definir explicitamente se a mensagem representa um evento ocorrido ou um
  comando para executar uma ação.
- Consumidores devem ser idempotentes.
- Retry deve ter limite e backoff; retries infinitos são proibidos.
- Poison messages devem ter uma estratégia observável, como DLQ.
- Publicação de eventos após uma transação de banco deve considerar
  Transactional Outbox.
- Contratos de mensagem devem ser versionáveis e não devem expor entidades
  internas diretamente.
- Logs e traces devem permitir acompanhar a mensagem entre produtores e
  consumidores.

## Integrações externas

- MusicBrainz deve ser tratado como fonte externa, não como dono do domínio.
- Importações devem ocorrer sob demanda.
- Não sincronizar catálogos completos.
- Não persistir respostas JSON brutas, áudio ou imagens.
- Aplicar timeout e respeitar limites de uso do provedor.
- Não permitir que DTOs externos atravessem para o domínio.

## Documentação de decisões

Decisões com impacto relevante devem ser registradas no README ou em um ADR,
incluindo:

- contexto e problema;
- decisão adotada;
- alternativas consideradas;
- consequências e trade-offs.

Um ADR é preferível quando a decisão afeta vários módulos, infraestrutura,
contratos ou operação em produção.

## Commits

Mensagens de commit devem obrigatoriamente seguir Conventional Commits:

```text
<tipo>(escopo opcional): <descrição curta no imperativo>
```

Tipos comuns:

- `feat`: nova funcionalidade;
- `fix`: correção de defeito;
- `test`: criação ou alteração de testes;
- `docs`: documentação;
- `refactor`: mudança interna sem alterar comportamento;
- `perf`: melhoria de desempenho;
- `chore`: manutenção e ferramentas;
- `build`: sistema de build ou dependências;
- `ci`: pipeline de integração ou entrega contínua.

Exemplos válidos:

```text
feat(reviews): add album rating
fix(catalog): reject duplicate external album
test(domain): cover rating boundaries
docs: record modular monolith decision
ci: run integration tests
```

- O tipo deve ser minúsculo.
- A descrição deve ser curta, específica e sem ponto final.
- Não usar mensagens genéricas como `update`, `changes` ou `fix stuff`.
- Não usar emojis no início da mensagem, pois algumas automações esperam que o
  tipo seja o primeiro token.
- Um commit deve representar uma unidade coerente de mudança.

## Critério de conclusão

Uma etapa só está concluída quando:

1. O comportamento solicitado foi implementado.
2. As fronteiras arquiteturais foram respeitadas.
3. Os testes relevantes existem e passam.
4. A documentação afetada foi atualizada.
5. O agente explicou decisões, trade-offs e impacto em produção.

