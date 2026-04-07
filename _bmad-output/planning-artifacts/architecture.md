---
stepsCompleted:
  - 1
  - 2
  - 3
  - 4
  - 5
  - 6
  - 7
inputDocuments:
  - _bmad-output/planning-artifacts/prd.md
  - README.md
  - docs/LOCAL_SETUP.md
  - docs/adr/ADR-001.md
  - docs/adr/ADR-002.md
  - docs/adr/ADR-003.md
  - docs/adr/ADR-004.md
  - IMPROVEMENTS.md
workflowType: 'architecture'
project_name: 'eShop'
user_name: 'Harry'
date: '2026-04-02'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
eShop defines 41 functional requirements across nine capability areas: catalog discovery, basket/session handling, checkout, order lifecycle and tracking, identity, webhooks, platform operations, developer reference experience, and optional AI integration. Architecturally, this means the system must support both end-user commerce flows and developer-facing extensibility/documentation flows without treating either as secondary.

The dominant capability chain is: catalog browse -> basket -> authenticated checkout -> asynchronous order progression -> order history/tracking -> external webhook notification. This implies a transactional boundary at checkout, asynchronous state propagation after order submission, and a clear separation between request/response APIs and background event processing.

**Non-Functional Requirements:**
The PRD defines 19 non-functional requirements. The most architecture-driving ones are:
- reliable local orchestration and startup
- environment-safe security configuration
- eventual consistency across asynchronous workers
- responsive user-facing behavior under normal development/demo conditions
- production-ready health signaling
- accessibility baseline expectations for adopters
- integration isolation so optional AI and simulated payment processing do not destabilize core commerce flows

**Scale & Complexity:**
This is a brownfield, medium-domain, high-architecture-complexity system. The complexity comes less from business rules and more from distributed coordination, multiple transport styles, and the need to remain teachable as a reference implementation.

- Primary domain: distributed web commerce reference application
- Complexity level: high
- Estimated architectural components: 10-12 major runtime components including frontend, APIs, workers, event bus, persistence, identity, and orchestration

### Technical Constraints & Dependencies

- Basket is intentionally gRPC-first and should not be forced into REST parity in the current wave.
- Ordering architecture assumes EF Core DbContext pooling compatibility.
- Identity must use development-only signing behavior in development and managed signing material in production.
- Order progression is event-driven through RabbitMQ-connected workers rather than synchronous orchestration.
- Local execution depends on .NET Aspire orchestration, Docker availability, and documented local scripts.
- The architecture must preserve testability across unit, functional, and Playwright E2E layers.
- Two roadmap items remain architecturally open and must be accounted for: order status timeline completion and production startup validation enforcement.

### Cross-Cutting Concerns Identified

- Authentication, authorization, and secure token handling
- Service-to-service communication consistency across REST, gRPC, and integration events
- Health checks, readiness signaling, and startup validation
- Observability, structured logging, and diagnosability
- Persistence boundaries and migration safety
- Developer workflow consistency and local reproducibility
- Test fixture architecture and CI reliability
- Security headers, privacy boundaries, and adopter-facing compliance concerns

## Starter Template Evaluation

### Primary Technology Domain

Distributed .NET 9 full-stack web application based on project requirements analysis.

The system combines:
- Blazor WebApp for the user-facing frontend
- ASP.NET Core minimal APIs for service endpoints
- gRPC for Basket service contracts
- Worker services for asynchronous order progression
- .NET Aspire AppHost for orchestration
- Shared service defaults for observability and HTTP client behavior

### Starter Options Considered

**Option 1: Official .NET built-in templates + Aspire composition**
Current official building blocks include:
- `dotnet new blazor`
- `dotnet new webapi`
- `dotnet new grpc`
- `dotnet new worker`

Current Aspire guidance favors selecting a C# or TypeScript AppHost and composing the solution from service projects rather than relying on a single monolithic starter template.

**Option 2: Brownfield foundation already present in this repository**
The current eShop repository already contains:
- AppHost orchestration
- Blazor frontend
- microservice boundaries
- worker services
- identity integration
- event bus wiring
- local developer workflow
- test infrastructure
- ADR-backed architecture constraints

This is stronger than any fresh starter because it already encodes the real decisions this architecture must preserve.

### Selected Starter: Existing eShop Brownfield Foundation

**Rationale for Selection:**
A new starter template should not be used for this architecture effort. The project already has the correct foundational shape and the architectural task is to formalize, constrain, and evolve that shape consistently. Current official starters are useful reference points, but they are insufficient replacements for the existing composed system.

**Initialization Command:**

```bash
No new starter initialization command.
Use the existing repository as the architecture baseline.
```

**Architectural Decisions Provided by Existing Foundation:**

**Language & Runtime:**
- C# on .NET 9 across services and orchestration
- Blazor WebApp for frontend delivery
- .NET MAUI HybridApp present as an expansion path

**Transport & Integration:**
- REST for most APIs
- gRPC-first Basket service
- RabbitMQ-backed integration events for order progression

**Build Tooling:**
- .NET SDK-driven solution structure
- AppHost orchestration for local and distributed execution
- documented local scripts for HTTP-first developer setup

**Testing Framework:**
- unit tests, functional tests, and Playwright E2E already established
- architecture must preserve fixture-based service testing and CI validation

**Code Organization:**
- service-per-boundary structure
- shared defaults and shared components
- dedicated worker services for asynchronous lifecycle progression

**Development Experience:**
- Aspire dashboard
- local orchestration scripts
- existing ADRs
- improvement roadmap
- repo already optimized as a reference implementation rather than a blank template

**Note:** The first implementation stories should evolve the existing foundation, not recreate it from a new starter.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Retain the current distributed service-based architecture centered on Aspire AppHost orchestration
- Standardize persistence on PostgreSQL for transactional data and Redis for basket/session state
- Preserve mixed communication patterns: REST for most APIs, gRPC for Basket, RabbitMQ for asynchronous order progression
- Keep Identity as the single authentication authority and enforce production-safe signing strategy
- Treat testability, startup validation, and observability as first-class architectural concerns

**Important Decisions (Shape Architecture):**
- Keep the Blazor WebApp as the primary frontend shell with shared component reuse across WebApp and HybridApp
- Keep asynchronous order progression in workers rather than moving orchestration into synchronous APIs
- Centralize cross-cutting operational behavior in `eShop.ServiceDefaults`
- Preserve service-per-boundary decomposition rather than merging services for convenience

**Deferred Decisions (Post-MVP):**
- Basket REST facade
- SignalR-based real-time order updates
- production-grade distributed tracing expansion
- response caching strategy
- stronger accessibility enforcement automation

### Data Architecture

**Primary transactional store:** PostgreSQL  
**Current version guidance:** Current major line is PostgreSQL 18, with 18.3 listed in current official releases.

**Decision:**
- Keep PostgreSQL as the system-of-record database for transactional services.
- Maintain service-owned persistence boundaries rather than introducing a shared relational schema across services.
- Preserve EF Core migrations per service boundary.
- Keep Redis dedicated to basket/session and short-lived operational state, not as a substitute system of record.

**Rationale:**
- The repo already uses PostgreSQL-backed service persistence patterns.
- The PRD requires reliability, migration safety, and brownfield continuity more than experimentation with alternate stores.
- The architecture must support order history evolution without collapsing bounded contexts.

**Validation & migration strategy:**
- Each service owns its schema evolution.
- Startup validation must fail fast when required configuration is missing.
- Migration checks remain part of functional validation.

### Authentication & Security

**Identity authority:** Duende IdentityServer in Identity.API  
**Decision:**
- Keep Identity.API as the central identity provider and token issuer.
- Preserve development-only signing credentials in development and require managed signing material in production.
- Keep authenticated boundaries around checkout, order history, and user-specific operations.
- Promote security headers and startup configuration validation as shared platform concerns via service defaults or equivalent shared middleware.

**Rationale:**
- ADR-003 already fixes the production-signing boundary.
- The system is user-facing and stores PII in order history.
- Security consistency matters more than adding alternative auth models.

**Security posture decisions:**
- Encrypt traffic in production environments.
- Keep no real payment card data inside the reference implementation.
- Fail fast for invalid security-sensitive configuration.

### API & Communication Patterns

**REST:** Primary external/internal HTTP API shape  
**gRPC:** Basket-specific contract surface  
**Messaging:** RabbitMQ-backed integration events  
**Current version guidance:** RabbitMQ official site currently shows 4.2.5 as the release signal.

**Decision:**
- Keep REST for Catalog, Ordering, Identity, and Webhooks APIs.
- Keep Basket gRPC-first with no forced REST parity in the current architecture wave.
- Keep RabbitMQ as the asynchronous transport for order lifecycle events between services and workers.
- Standardize minimal API conventions, versioning strategy, health endpoints, and ProblemDetails behavior across REST services.

**Rationale:**
- ADR-001 explicitly chooses gRPC-first Basket.
- ADR-004 explicitly chooses event-driven order progression.
- The PRD and improvement roadmap both emphasize consistent API shape and operational clarity.

**Error and contract strategy:**
- REST services expose versioned APIs.
- Integration events remain explicit contract boundaries between bounded services.
- Request/response flows stop at checkout; downstream order progression remains asynchronous.

### Frontend Architecture

**Primary frontend:** Blazor WebApp  
**Shared UI reuse:** WebAppComponents across web and HybridApp  
**Current platform guidance:** `dotnet new blazor` is the current official Blazor starter direction; no new starter is selected because the existing repo remains the baseline.

**Decision:**
- Keep Blazor WebApp as the primary user experience surface.
- Preserve shared UI/component reuse between WebApp and HybridApp.
- Do not introduce a separate SPA framework into the architecture.
- Treat accessibility baseline and responsive design as architecture-supported quality requirements, but not as separate frontend platform rewrites.

**Rationale:**
- The repository already encodes the desired frontend shape.
- The PRD centers on coherence and reference value, not polyglot frontend experimentation.
- Adding another frontend stack would reduce consistency for downstream implementers.

**Interaction model decisions:**
- Current frontend remains request/response plus refresh-based status visibility.
- Real-time UI push is deferred beyond MVP.
- Browser support follows the current Playwright-validated matrix.

### Infrastructure & Deployment

**Orchestration:** .NET Aspire AppHost  
**Local runtime dependencies:** Docker + AppHost + service containers/resources  
**Current version guidance:** .NET 9 official download page currently lists SDK 9.0.312 and ASP.NET Core Runtime 9.0.14. Redis official site is directing users to Redis 8.

**Decision:**
- Keep .NET Aspire AppHost as the local orchestration and distributed app composition layer.
- Keep Docker-backed local dependencies as part of the expected developer workflow.
- Standardize service defaults for observability, HTTP client defaults, and shared health behavior.
- Preserve Redis as the basket/session state store.
- Architect production deployment as an evolution of the Aspire-composed system, not a separate hand-built deployment model.

**Rationale:**
- The repo already documents local run flows and dashboard usage.
- Developer reproducibility is a primary business and technical success criterion.
- The architecture must support both local reference usage and future Azure-oriented deployment.

**Operational decisions:**
- `/alive` remains the baseline health endpoint across services.
- Startup validation becomes mandatory for production-safe config.
- Structured logging and diagnostics remain required platform capabilities.

### Decision Impact Analysis

**Implementation Sequence:**
1. Preserve the existing bounded-context service topology.
2. Formalize shared platform behaviors in service defaults and hosting conventions.
3. Complete the two open roadmap obligations: order status timeline and startup validation.
4. Strengthen operational consistency and test reliability before adding phase-2 features.

**Cross-Component Dependencies:**
- Identity decisions affect WebApp, checkout, webhook ownership, and protected APIs.
- Messaging decisions affect Ordering, OrderProcessor, PaymentProcessor, and Webhooks.
- Data-store decisions affect Ordering, Catalog, Webhooks, basket/session handling, and migration strategy.
- Service-default decisions affect every runnable service and all future extensions.

## Implementation Patterns & Consistency Rules

### Pattern Categories Defined

**Critical Conflict Points Identified:**
8 areas where AI agents could make incompatible choices if left unspecified:
- API route and versioning conventions
- DTO and JSON naming
- service registration and endpoint mapping patterns
- event naming and payload structure
- error handling and status code usage
- test placement and fixture structure
- configuration/options validation
- logging and health endpoint conventions

### Naming Patterns

**Database Naming Conventions:**
- Preserve existing EF Core and database naming already present in each bounded context.
- New tables and columns must follow the established naming of the owning service rather than introducing a repo-wide rewrite.
- Foreign keys and indexes should use the conventions already generated and maintained inside the service's persistence layer.
- No cross-service shared schema naming conventions should be introduced.

**API Naming Conventions:**
- REST endpoints use plural resource-oriented naming where REST APIs already follow that model.
- Basket remains gRPC-first and must not be renamed or reshaped into REST conventions by default.
- Route parameters use ASP.NET Core route-token conventions such as `{id}`.
- API versioning remains explicit where REST services already version route groups.

**Code Naming Conventions:**
- Public C# types use PascalCase.
- Local variables and parameters use camelCase.
- File names match primary type names for C# source files.
- New shared abstractions must be named for business/domain meaning, not infrastructure convenience.
- Avoid introducing alternate naming styles in the same service boundary.

### Structure Patterns

**Project Organization:**
- Preserve service-per-boundary organization already present under `src/`.
- Keep tests under `tests/` by test project, not co-located with production source.
- Shared runtime defaults belong in `eShop.ServiceDefaults`.
- Shared cross-UI components belong in `WebAppComponents`.
- Do not create new `common` layers unless the reuse is cross-service and justified.

**File Structure Patterns:**
- API endpoint mapping stays close to the owning service/API project.
- Persistence code remains inside the owning infrastructure or API project as currently structured.
- Background workflow logic remains in worker projects rather than moving into request handlers.
- Production appsettings remain per service, with validation enforced at startup.

### Format Patterns

**API Response Formats:**
- Use the native ASP.NET Core minimal API response model already established in the repo.
- Successful responses should remain direct resource/result responses unless an existing API already uses a wrapper.
- Error responses should align to ProblemDetails/RFC 7807 for REST APIs where applicable.
- Health endpoints remain lightweight and operationally focused.

**Data Exchange Formats:**
- JSON field naming should follow existing ASP.NET serialization defaults for the relevant service contracts.
- External integration contracts must remain explicit and version-tolerant.
- Date/time values should use interoperable standard representations already compatible with .NET JSON defaults.
- Event payloads should stay explicit and additive rather than polymorphic or loosely typed.

### Communication Patterns

**Event System Patterns:**
- Integration event names follow explicit domain-event style names such as `OrderStatusChangedToStockConfirmedIntegrationEvent`.
- Event payloads should be strongly typed and domain-specific.
- New long-running business transitions should use integration events rather than synchronous orchestration when they cross service boundaries.
- Event consumers must assume eventual consistency and design for idempotent handling.

**State Management Patterns:**
- UI state should remain local to the Blazor component/page unless multiple screens require shared coordination.
- Server-owned business state remains authoritative; UI state should not become a second system of record.
- Basket/session state remains in Redis-backed infrastructure, while transactional state remains in PostgreSQL-backed services.
- Avoid introducing a second cross-app state-management abstraction unless required by a real feature boundary.

### Process Patterns

**Error Handling Patterns:**
- Request validation failures return clear client errors, not generic server failures.
- Cross-cutting validation belongs in reusable filters/middleware where patterns already exist.
- Background-service failures must be observable through structured logs.
- User-facing error handling and operator-facing diagnostics must remain separate concerns.

**Loading State Patterns:**
- Frontend loading states should be page- or component-scoped by default.
- No silent long-running operations in the UI; asynchronous flows must have explicit status visibility.
- Order progression remains asynchronous and should not be represented as immediate synchronous completion in UI logic.
- Operational startup state belongs in health/readiness behavior, not ad hoc custom probes.

### Enforcement Guidelines

**All AI Agents MUST:**
- Preserve existing bounded-context and project-boundary structure.
- Reuse established service defaults, API patterns, and test fixture patterns before inventing new abstractions.
- Keep REST, gRPC, and messaging contracts aligned with existing architectural decisions and ADRs.
- Add startup validation, logging, and test coverage in the same style as the owning service.
- Avoid repo-wide refactors disguised as local feature work.

**Pattern Enforcement:**
- Validate new work against ADRs, PRD FRs/NFRs, and service-local conventions.
- Treat deviations from route, contract, or persistence patterns as architecture changes, not implementation details.
- Record justified pattern changes in new ADRs or architecture updates rather than silently drifting.

### Pattern Examples

**Good Examples:**
- Add a new REST endpoint by extending the existing minimal API mapping style of the owning service.
- Add a new async order-related workflow by publishing and consuming typed integration events.
- Add a new service by following AppHost registration, service defaults, per-service config, and dedicated test-project patterns.

**Anti-Patterns:**
- Adding REST parity to Basket by default.
- Moving worker-owned business progression into synchronous API handlers.
- Introducing a new shared `utilities` project for one feature-specific helper.
- Returning custom ad hoc error payloads where the service otherwise uses standard error semantics.
- Writing tests in a completely different structure from the rest of the repo.

## Project Structure & Boundaries

### Repository Structure

The architecture should continue to use the current repository as the authoritative structural baseline:

```text
eShop/
|- src/
|  |- Basket.API/
|  |- Catalog.API/
|  |- ClientApp/
|  |- eShop.AppHost/
|  |- eShop.ServiceDefaults/
|  |- EventBus/
|  |- EventBusRabbitMQ/
|  |- HybridApp/
|  |- Identity.API/
|  |- IntegrationEventLogEF/
|  |- Ordering.API/
|  |- Ordering.Domain/
|  |- Ordering.Infrastructure/
|  |- OrderProcessor/
|  |- PaymentProcessor/
|  |- Shared/
|  |- WebApp/
|  |- WebAppComponents/
|  |- WebhookClient/
|  \- Webhooks.API/
|- tests/
|  |- Basket.UnitTests/
|  |- Catalog.FunctionalTests/
|  |- ClientApp.UnitTests/
|  |- Identity.FunctionalTests/
|  |- Ordering.FunctionalTests/
|  |- Ordering.UnitTests/
|  |- OrderProcessor.FunctionalTests/
|  |- PaymentProcessor.FunctionalTests/
|  \- Webhooks.FunctionalTests/
|- e2e/
|  |- AddItemTest.spec.ts
|  |- BrowseItemTest.spec.ts
|  |- CheckoutFlowTest.spec.ts
|  |- RemoveItemTest.spec.ts
|  \- SessionPersistenceTest.spec.ts
|- build/
|  |- acr-build/
|  |- local/
|  \- multiarch-manifests/
|- docs/
|- artifacts/
\- _bmad-output/
  \- planning-artifacts/
```

### Boundary Definitions

**Frontend boundaries:**
- `WebApp` is the primary user-facing commerce shell.
- `WebAppComponents` contains reusable UI components shared across frontend surfaces.
- `HybridApp` remains a consumer of shared UI and service contracts, not a place to redefine backend rules.
- `ClientApp` stays isolated as its own application surface and test target instead of becoming a general dumping ground for frontend logic.

**API boundaries:**
- `Catalog.API` owns catalog browsing, item details, and catalog-facing data contracts.
- `Basket.API` owns basket/session behaviors and remains the gRPC-first boundary.
- `Ordering.API` owns checkout submission, order query surfaces, and customer order history endpoints.
- `Identity.API` owns authentication, authorization, and token issuance.
- `Webhooks.API` owns webhook registration, delivery orchestration, and external callback management.

**Domain and persistence boundaries:**
- `Ordering.Domain` owns ordering business rules and domain model behavior.
- `Ordering.Infrastructure` owns ordering persistence, integration plumbing, and database-facing implementation.
- No other service should directly depend on ordering persistence internals.
- Redis remains isolated to basket/session operational state; PostgreSQL-backed persistence remains inside owning services.

**Worker and integration boundaries:**
- `OrderProcessor` owns asynchronous order progression triggered by integration events.
- `PaymentProcessor` owns payment simulation and related downstream state transitions.
- `EventBus` and `EventBusRabbitMQ` own the messaging abstraction and RabbitMQ transport implementation.
- `IntegrationEventLogEF` remains the persistence support layer for reliable event publication patterns where already used.

**Platform boundaries:**
- `eShop.AppHost` owns local orchestration and distributed composition.
- `eShop.ServiceDefaults` owns shared service bootstrap behavior such as health, telemetry, HTTP defaults, and common middleware registration.
- `Shared` should remain limited to narrowly justified cross-service primitives and must not evolve into an unbounded common layer.

### Requirements-to-Structure Mapping

**Catalog requirements:**
- Implement in `Catalog.API`, surfaced through `WebApp`, covered by functional tests and Playwright browse flows.

**Basket and session requirements:**
- Implement in `Basket.API` with Redis-backed state, surfaced through `WebApp`, validated by unit/functional tests and session persistence E2E coverage.

**Checkout and ordering requirements:**
- Enter through `WebApp` and `Ordering.API`, then transition into `OrderProcessor` and `PaymentProcessor` through integration events.
- Ordering business rules remain split across `Ordering.Domain` and `Ordering.Infrastructure`.

**Identity and secure user flows:**
- Implement in `Identity.API` and consumed by `WebApp`, protected APIs, and any user-owned webhook operations.

**Webhook requirements:**
- Implement in `Webhooks.API` with outbound callback support coordinated with ordering lifecycle events.

**Developer reference and operational requirements:**
- Supported by `eShop.AppHost`, `eShop.ServiceDefaults`, `build/local`, `docs/`, and the service-local test projects.

### Integration Points

**Internal integration points:**
- `WebApp` integrates with Catalog, Basket, Ordering, Identity, and Webhooks service surfaces.
- `Ordering.API` publishes integration events consumed by `OrderProcessor`, `PaymentProcessor`, and webhook-related flows.
- Shared operational policies are imported from `eShop.ServiceDefaults` into each runnable service.
- AppHost composes all runtime services, dependencies, and local infrastructure resources.

**External integration points:**
- Identity/OIDC flows terminate through `Identity.API`.
- RabbitMQ is the broker for asynchronous domain progression.
- PostgreSQL and Redis are external infrastructure dependencies managed through local orchestration and environment configuration.
- Optional Azure OpenAI integration remains additive and must stay isolated from the core purchase path.

### Source, Test, and Configuration Organization

**Source organization:**
- Keep production code under the owning `src/` project.
- Keep business logic inside the owning boundary instead of moving it into shared helpers.
- Keep endpoint registration, options binding, validation, and service composition in the service that owns the behavior.

**Test organization:**
- Keep service-level tests in the matching `tests/` project.
- Keep browser journey coverage in `e2e/`.
- New features should extend the closest existing test layer first rather than inventing a new test project pattern.

**Configuration organization:**
- Service-local appsettings stay with the owning service.
- Production-safe validation is mandatory at startup for required settings.
- Build and local-run scripts remain under `build/` and should not be duplicated inside service folders.

### Development Workflow Alignment

The structure supports the intended delivery workflow:
- implement feature logic inside the owning bounded context
- expose or extend the correct transport surface: REST, gRPC, or integration event
- register runtime dependencies through existing hosting and service-default patterns
- validate behavior with the nearest unit or functional test project and then with Playwright when the user journey changes
- run the composed system through AppHost and the documented local scripts

This preserves clear ownership, minimizes cross-agent conflicts, and keeps future implementation work aligned with the repo's actual runtime topology.

## Architecture Validation Results

### Coherence Validation

**Decision Compatibility:**
The architectural decisions are coherent and mutually reinforcing. The selected stack of Blazor WebApp, ASP.NET Core APIs, a gRPC-first Basket boundary, RabbitMQ-backed asynchronous workers, PostgreSQL for transactional persistence, Redis for basket/session state, and Aspire AppHost orchestration matches the current repository topology and does not introduce contradictory runtime or ownership patterns. The architecture remains aligned with ADR-backed constraints around Basket transport, event-driven order progression, development-only signing behavior, and ordering persistence assumptions.

**Pattern Consistency:**
The implementation patterns support the architectural decisions effectively. Naming, structure, communication, error handling, configuration validation, and test placement rules all reinforce the existing service-per-boundary model. The document consistently prevents the most likely drift points, especially accidental synchronous orchestration of workflows that are intentionally worker- and event-driven.

**Structure Alignment:**
The project structure supports the architectural decisions and mirrors the real repository. Boundaries between frontend surfaces, APIs, workers, shared platform infrastructure, and persistence layers are explicit enough to guide implementation safely. Integration points are well defined and the structure supports the selected transports, test layers, and shared runtime defaults.

### Requirements Coverage Validation

**Feature Coverage:**
The architecture provides support for all major requirement groups identified in the PRD: catalog discovery, basket and session handling, authenticated checkout, asynchronous order lifecycle progression, order history and tracking, identity, webhook registration and delivery, platform and operational consistency, developer reference experience, and optional AI integration. Cross-cutting flows are supported through AppHost orchestration, service defaults, health behavior, and the preserved testing model.

**Functional Requirements Coverage:**
All functional requirement categories are architecturally represented by concrete owning services, components, or platform layers. Catalog behavior maps to `Catalog.API` and frontend surfaces; basket and session handling map to `Basket.API` and Redis-backed infrastructure; checkout and ordering map to `Ordering.API`, `Ordering.Domain`, `Ordering.Infrastructure`, and asynchronous processors; identity maps to `Identity.API`; webhook capabilities map to `Webhooks.API`; and developer-facing operational requirements map to AppHost, ServiceDefaults, build scripts, docs, and the test suite.

**Non-Functional Requirements Coverage:**
Performance is supported through service ownership, asynchronous processing, Redis isolation for short-lived state, and preservation of database boundaries. Security is supported through centralized identity, production-safe signing rules, startup validation, and platform-level consistency guidance. Reliability is supported through worker/event separation, health signaling, structured logging expectations, and multi-layer testing. Accessibility and responsive UX expectations are acknowledged and remain compatible with the chosen frontend architecture. Operational reproducibility is strongly supported through Aspire-based orchestration and documented local workflows.

### Implementation Readiness Validation

**Decision Completeness:**
The critical architectural decisions are documented clearly enough to guide implementation. Core technology selections, transport boundaries, persistence ownership, frontend direction, operational expectations, and implementation guardrails are all present. The document is sufficiently specific for implementation planning without requiring agents to reinterpret the system from first principles.

**Structure Completeness:**
The structure definition is complete enough for implementation handoff. The document captures the real repository shape, defines ownership boundaries across services and supporting projects, maps requirement areas to concrete components, and identifies internal and external integration points.

**Pattern Completeness:**
The pattern guidance is strong across naming, communication, project organization, error handling, loading-state expectations, and enforcement rules. The remaining ambiguity is limited to a few refinement areas where the guidance says to preserve existing service-local patterns rather than naming exact implementation locations or service-specific convergence rules.

### Gap Analysis Results

**Important Gaps:**
- The architecture does not yet define a service-by-service target for REST API versioning convergence, even though it requires explicit and consistent versioning.
- The order status timeline roadmap item is recognized, but the architecture does not yet specify the canonical ownership of the projection or read model that should drive that user-visible timeline.
- Startup validation is declared mandatory, but the architecture does not yet define the exact reusable enforcement pattern that all runnable services should adopt.

**Nice-to-Have Gaps:**
- More explicit examples for where new DTOs, validators, endpoint mappers, and options classes should live in each service style.
- A sharper rule for when code belongs in `Shared` versus remaining in a service-local boundary.
- More explicit accessibility enforcement guidance for frontend implementation handoff.

### Validation Issues Addressed

No critical architectural contradiction or blocking design conflict was found. The identified issues are refinement issues rather than blockers. The architecture is coherent, substantially complete, and suitable for implementation planning, while the remaining gaps are specific areas where additional precision would reduce ambiguity in later implementation stories.

### Architecture Completeness Checklist

**Requirements Analysis**

- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**Architectural Decisions**

- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance considerations addressed

**Implementation Patterns**

- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Communication patterns specified
- [x] Process patterns documented

**Project Structure**

- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY WITH MINOR REFINEMENTS

The architecture is ready to guide implementation work. The remaining gaps should be handled as targeted follow-up clarifications during story creation or implementation planning rather than treated as blockers to moving forward.
