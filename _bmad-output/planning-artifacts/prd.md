---
stepsCompleted:
  - step-01-init
  - step-02-discovery
  - step-02b-vision
  - step-02c-executive-summary
  - step-03-success
  - step-04-journeys
  - step-05-domain
  - step-06-innovation
  - step-07-project-type
  - step-08-scoping
  - step-09-functional
  - step-10-nonfunctional
  - step-11-polish
  - step-12-complete
classification:
  projectType: web_app
  domain: e-commerce
  complexity: medium
  projectContext: brownfield
inputDocuments:
  - docs/README.md
  - docs/LOCAL_SETUP.md
  - docs/adr/ADR-001.md
  - docs/adr/ADR-002.md
  - docs/adr/ADR-003.md
  - docs/adr/ADR-004.md
  - IMPROVEMENTS.md
workflowType: 'prd'
---

# Product Requirements Document - eShop

**Author:** Harry
**Date:** April 2, 2026

## Executive Summary

eShop ("AdventureWorks") is a production-representative .NET 9 reference e-commerce application demonstrating microservices architecture, distributed system patterns, and modern .NET Aspire orchestration. It serves as both a **learning platform** for .NET developers and a **validated architectural template** for teams building real e-commerce systems. The system delivers a complete online shopping experience — product browsing, cart management, checkout, order lifecycle, identity, and webhook delivery — across six independently deployable services and two event-driven background workers.

**Target users:** .NET developers, architects, and engineering teams seeking authoritative reference patterns for distributed, cloud-ready e-commerce systems.

**Problem solved:** Teams building e-commerce microservices face high accidental complexity when assembling Aspire orchestration, event-driven workflows, gRPC/REST API design, Blazor frontend integration, and identity together. eShop eliminates that bootstrapping cost with a fully integrated, tested, and documented starting point.

### What Makes This Special

eShop is the official .NET reference implementation — authored and maintained against the latest .NET SDK, Aspire, and cloud-native conventions. Its differentiator is **breadth with coherence**: every service follows consistent patterns (minimal APIs, cancellation tokens, API versioning, DbContext pooling, ProblemDetails), architectural decisions are documented as ADRs, test coverage spans unit through functional tests for every service, and a live improvement roadmap tracks ongoing production-readiness hardening. It is not a toy sample — it is a production-representative system that teaches by example at every layer.

**Core insight:** The gap between "hello world" and production-ready microservices is where most developers lose weeks. eShop bridges that gap with a running, tested, documented system that reflects real engineering tradeoffs.

## Project Classification

| Attribute | Value |
|---|---|
| **Project Type** | Web Application (Blazor + Microservices REST/gRPC APIs) |
| **Domain** | E-commerce / Retail |
| **Complexity** | Medium — distributed services, event-driven processing, multi-service auth, no regulated-industry constraints |
| **Project Context** | Brownfield — active codebase with 25+ implemented improvements, 2 in-progress features, full test suites |
| **Platform** | .NET 9, .NET Aspire, RabbitMQ, PostgreSQL, Redis |

## Success Criteria

### User Success

- A developer can clone eShop and run it locally in under 10 minutes using `run-local.ps1`
- Every pattern in the codebase is directly mappable to a production microservice scenario
- Order history and tracking features are fully implemented end-to-end (no partial states)
- Documentation (ADRs, LOCAL_SETUP, README) answers the top 5 "how do I…" questions without external research

### Business Success

- eShop is the canonical .NET 9 reference for microservices e-commerce — cited in official .NET docs and learning paths
- All improvement roadmap in-progress items reach completion (order status timeline, startup enforcement validation)
- Zero open "partial" implementation items in the roadmap
- Active test suite coverage provides confidence baseline for downstream adopters

### Technical Success

- All service test suites (unit + functional) pass green in CI, including Catalog.FunctionalTests
- No pending EF Core migrations at service startup
- Code coverage ≥ 70% per service
- Playwright E2E suite passes on Chromium and Firefox in CI
- HTTP client 30s timeout enforced across all services (already in ServiceDefaults)
- Production appsettings startup validation enforcement implemented (currently partial)

### Measurable Outcomes

- 0 broken tests in CI across all test projects
- 0 partially-implemented features in the active roadmap
- ≥ 70% line coverage per service
- Local developer setup time ≤ 10 minutes from clone to running

## Product Scope

### MVP — Minimum Viable Product

- Complete all in-progress IMPROVEMENTS.md items (order status timeline, production config startup validation)
- Green CI across all test suites
- Full functional test coverage for all 8 services

### Growth Features (Post-MVP)

- Response caching strategy (catalog, health checks)
- Security headers middleware in ServiceDefaults
- REST facade for Basket API if client demand increases
- Performance baseline benchmarks

### Vision (Future)

- Azure deployment reference architecture (azd-ready)
- .NET MAUI hybrid app (HybridApp already in repo) production-complete
- Chaos engineering / resilience test scenarios
- Full OpenTelemetry distributed tracing demo

## User Journeys

### Journey 1: Alex — The .NET Developer Adopting Microservices

**Opening Scene:** Alex is a mid-level .NET developer at a consultancy. Their client wants a new e-commerce platform and the architect has mandated microservices + Aspire. Alex has only built monoliths. They Google ".NET microservices e-commerce example" and land on eShop.

**Rising Action:** Alex clones the repo, runs `run-local.ps1`, and in 8 minutes has a live Aspire dashboard showing all 8 services healthy. They navigate to the Blazor WebApp, browse the catalog, add items to the basket, and complete a checkout. They then open `Ordering.API/Apis/OrdersApi.cs` and see a clean minimal API with cancellation tokens, a `ValidRequestIdFilter`, and `mediator.Send()` — patterns they can copy directly.

**Climax:** Alex opens `docs/adr/ADR-004.md` and reads exactly why `OrderProcessor` uses event-driven integration rather than synchronous calls. They have their "aha!" moment: "I don't need to invent this architecture — it's already justified here."

**Resolution:** Alex presents the architecture to their client referencing the ADRs. Their confidence is high. They fork eShop and start stripping out the AdventureWorks catalog to replace it with the client's domain. The patterns hold.

**Requirements revealed:** Fast local setup, working Aspire dashboard, navigable documentation, ADR coverage, copyable code patterns.

---

### Journey 2: Sam — The Shopper (Happy Path)

**Opening Scene:** Sam visits the AdventureWorks store, looking for a specific brand of outdoor gear. They're on a laptop, signed out.

**Rising Action:** Sam browses the catalog, finds a product, and sees details. They add it to the basket. They're prompted to log in. They authenticate via the Identity service, return to the now-persisted basket, and check out.

**Climax:** The order is submitted. Sam sees an order confirmation. They navigate to order history and see the order with status tracking.

**Resolution:** Sam receives webhook-driven status updates as the order moves through grace period → stock confirmed → payment processed → shipped.

**Requirements revealed:** Catalog browsing, basket management, checkout flow, order confirmation, order history view, status tracking timeline, webhook delivery.

---

### Journey 3: Sam — The Shopper (Session Edge Case)

**Opening Scene:** Same as Journey 2, but Sam adds items to the basket, closes the browser tab, and returns 20 minutes later.

**Rising Action:** Sam re-opens the store and signs in. Their basket items are still there (Redis-backed session persistence). Sam also tries to check out with an empty basket — the system handles validation gracefully.

**Resolution:** Sam completes checkout undisturbed. The `SessionPersistenceTest.spec.ts` E2E test in the repo maps exactly to this journey.

**Requirements revealed:** Cart persistence across sessions, graceful validation error handling, auth-aware basket state.

---

### Journey 4: Jordan — The Platform Operator / DevOps Engineer

**Opening Scene:** Jordan is deploying eShop to a staging environment and needs to verify all services are healthy before routing traffic.

**Rising Action:** Jordan hits `/alive` on each service — all return 200. They check the Aspire dashboard. They test a startup with a misconfigured connection string and expect the service to fail fast with a clear error rather than crash mid-request.

**Climax:** Ordering.API starts and immediately throws a startup validation error because the DB connection string placeholder was not replaced. Jordan fixes it and restarts.

**Resolution:** All services green. Jordan sets up Kubernetes liveness probes pointing to `/alive`.

**Requirements revealed:** `/alive` health endpoint on all services, startup config validation enforcement (currently partial), clear startup failure messaging.

---

### Journey 5: Casey — The Developer Extending eShop

**Opening Scene:** Casey wants to add a "wishlist" feature to eShop and needs to understand existing patterns before writing code.

**Rising Action:** Casey reads the ADRs, studies how `Catalog.API` and `Ordering.API` register endpoints, handle versioning, and connect to the DB. They follow the `ValidRequestIdFilter` example to understand cross-cutting concerns.

**Climax:** Casey creates a new `Wishlist.API` project, mirrors patterns from Catalog, wires it into Aspire, and adds a functional test fixture following the pattern in `Webhooks.FunctionalTests`.

**Resolution:** The new service passes CI on first PR. Casey made no architectural decisions from scratch — every decision was already documented.

**Requirements revealed:** Pattern consistency across all services, functional test fixture templates, clear extension points in Aspire host.

---

### Journey Requirements Summary

| Journey | Key Capabilities Required |
|---|---|
| Alex (Developer Adopter) | Fast local setup, ADR docs, clean copyable patterns, Aspire dashboard |
| Sam (Happy Path Shopper) | Catalog browse, basket, checkout, order history, status tracking |
| Sam (Session Edge Case) | Session persistence, graceful validation, auth-aware state |
| Jordan (Operator) | `/alive` endpoints, startup validation, Kubernetes-ready health probes |
| Casey (Developer Extender) | Pattern consistency, fixture templates, extension points |

## Domain-Specific Requirements

> **Domain:** E-commerce / Retail | **Complexity:** Medium | **Context:** Reference application — no real transactions, PII, or regulated data in the default implementation.

### Compliance & Regulatory (Adopter Considerations)

- **PCI-DSS:** Payment processing is fully simulated in `PaymentProcessor`. No real card data is stored or transmitted. Adopters integrating a real payment gateway must scope and comply with PCI-DSS independently.
- **GDPR / Data Privacy:** Order history stores PII (buyer name, shipping address, order items). No data retention policy, erasure endpoint, or consent management is implemented. Adopters operating in EU/UK jurisdictions must add these.
- **Authentication:** Identity service uses Duende IdentityServer with developer signing credentials gated to development only (ADR-003). Production deployments require managed signing keys from a secure store (e.g., Azure Key Vault).

### Technical Constraints

- **Security headers:** `SecurityHeadersAttribute` is implemented in Identity.API only. REST APIs (Catalog, Ordering, Webhooks) lack CSP, X-Frame-Options, and X-Content-Type-Options headers. Mitigation: centralize in `eShop.ServiceDefaults`.
- **Session security:** Basket is Redis-backed and tied to authenticated identity. No explicit session expiry or rotation policy beyond ASP.NET Core defaults.
- **Transport:** All inter-service communication is HTTP/gRPC within the Aspire-managed network. External HTTPS termination is handled at the host/reverse proxy layer.

### Risk Mitigations

| Risk | Mitigation |
|---|---|
| Developer credentials leaking to production | ADR-003 enforced; `IsDevelopment()` gate in Identity.API |
| Real payment data in reference app | PaymentProcessor is simulation-only; no payment SDK integrated |
| PII in order history | Documented as adopter responsibility; no erasure API in scope |
| Missing security headers on REST APIs | IMPROVEMENTS.md flags this; planned for ServiceDefaults centralization |

## Web App Specific Requirements

### Project-Type Overview

eShop's frontend is a **Blazor WebApp** (.NET 9) using server-side rendering with interactive islands. Components are shared via the `WebAppComponents` library and reused in the `HybridApp` (MAUI Blazor Hybrid). The web layer communicates with backend services via typed HTTP clients and gRPC (Basket).

### Browser Matrix

| Browser | Support Level |
|---|---|
| Chromium (Chrome, Edge) | Primary — CI-tested via Playwright |
| Firefox | Secondary — CI-tested via Playwright |
| WebKit (Safari) | CI-tested; mobile Safari parity expected |
| Legacy IE/non-Chromium | Not supported |

### Responsive Design

- Standard responsive layout expected for desktop and tablet viewports
- Mobile-first not explicitly documented; no responsive breakpoint specs in current codebase
- `HybridApp` (MAUI) handles native mobile experience separately

### Delivery Expectations

- Primary browser support is defined by the Playwright-covered matrix: Chromium and Firefox are validated in CI, with WebKit compatibility expected.
- The web application relies on standard Blazor SSR behavior and shared HTTP client defaults from `eShop.ServiceDefaults`.
- No separate public page-load SLA is defined in the current reference implementation; measurable performance expectations are captured in the Non-Functional Requirements section.

### SEO Strategy

Not applicable. eShop is an authenticated e-commerce application. Catalog pages are behind login or not indexed. No sitemap, meta tags, or SEO-specific requirements.

### Accessibility Level

The reference app does not currently enforce an accessibility standard in code. For production adopters, the recommended baseline is **WCAG 2.1 AA**.

### Real-Time & Notifications

- Order status progression is **event-driven async** (RabbitMQ integration events) — not real-time UI push
- No SignalR or WebSocket connections in the current implementation
- Order history page requires manual refresh to reflect status changes
- Webhook delivery notifies external subscribers of order events

### Implementation Considerations

- Blazor component library (`WebAppComponents`) must remain decoupled from hosting model to support both WebApp and HybridApp
- All Blazor pages requiring authentication use `[Authorize]` attribute; Identity server handles OIDC flows
- `ClientApp` (TypeScript/Node) exists for any JavaScript-side tooling; unit tests in `ClientApp.UnitTests`

## Project Scoping & Phased Development

This section refines the phase model summarized above and explains the sequencing, delivery boundaries, and risk tradeoffs behind each phase.

### MVP Strategy & Philosophy

**MVP Approach:** Completion MVP — the system already exists and ships value. The MVP goal is closing the two open gaps that undermine the reference app's credibility (partial order status timeline, partial production config validation) and ensuring all test suites pass green. No new features until existing commitments are fulfilled.

**Resource Requirements:** Single engineer / AI-assisted pair sufficient for completion tasks. No new architectural decisions required — all work is within established patterns.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- Alex (Developer Adopter) — fully supported once all patterns are complete and tests green
- Sam (Happy Path + Edge Case) — fully supported; the order status timeline is the one gap
- Jordan (Operator) — partially supported; startup validation enforcement is the open item
- Casey (Developer Extender) — fully supported

**Must-Have Capabilities:**

| Capability | Status |
|---|---|
| Order status timeline in order history | 🔄 In progress — complete this |
| Production config startup validation enforcement | 🔄 In progress — complete this |
| All functional test suites green in CI | ❌ Catalog.FunctionalTests failing — fix required |
| All ADR decisions implemented (no "TODO" deviations) | ✅ Done |
| `/alive` health endpoint on all services | ✅ Done |
| Full E2E Playwright suite passing Chromium + Firefox | ✅ Done |

### Post-MVP Features

**Phase 2 (Growth):**
- Response caching for catalog and health check endpoints
- Security headers middleware centralized in ServiceDefaults
- WCAG 2.1 AA accessibility baseline assessment
- Performance benchmarks (load test baseline with k6)
- REST facade for Basket API (if external client demand warrants)

**Phase 3 (Expansion / Vision):**
- Azure deployment reference (azd-ready, Key Vault signing, managed identity)
- .NET MAUI HybridApp production-complete with feature parity
- Real-time order status via SignalR
- Full OpenTelemetry distributed tracing demo
- Chaos/resilience testing scenarios

### Risk Mitigation Strategy

| Risk Type | Risk | Mitigation |
|---|---|---|
| Technical | Catalog.FunctionalTests failing in CI | Investigate and fix before closing MVP — this is blocking |
| Technical | Order status timeline requires schema migration | Follow existing EF Core migration pattern; test with migration-check functional test |
| Market | Reference app becomes stale vs. .NET 10 | Pin ADR review cadence to .NET major releases |
| Resource | Aspire host changes break service wiring | AppHost integration test or smoke test on startup covers this |

## Functional Requirements

### Catalog & Product Discovery

- FR1: Shoppers can browse the product catalog without authentication
- FR2: Shoppers can view product details including name, description, price, and image
- FR3: Shoppers can filter the catalog by brand and/or product type
- FR4: Shoppers can navigate paginated catalog results
- FR5: The system can expose catalog data via a versioned REST API

### Shopping Cart & Session

- FR6: Shoppers can add products to a basket
- FR7: Shoppers can view and modify basket contents
- FR8: The system preserves basket state across authenticated sessions
- FR9: The system associates a basket with a user identity upon login
- FR10: Shoppers can proceed to checkout from the basket

### Checkout & Order Placement

- FR11: Authenticated shoppers can submit an order from their basket
- FR12: The system generates a unique order with a request identifier to prevent duplicate submissions
- FR13: Shoppers receive an order confirmation upon successful order submission
- FR14: The system clears the basket after a successful order

### Order Lifecycle & Tracking

- FR15: Authenticated shoppers can view their order history
- FR16: Shoppers can view the current status of an individual order
- FR17: The system displays an order status timeline for each order
- FR18: The system progresses orders through lifecycle states via event-driven integration
- FR19: The system notifies registered webhook subscribers of order status changes

### Identity & Authentication

- FR20: Users can register and log in via the Identity service
- FR21: The system gates checkout and order history behind authenticated sessions
- FR22: The system uses environment-appropriate signing credentials
- FR23: Users can log out and their session state is cleared

### Webhook Management

- FR24: Authenticated users can register a webhook subscription for order events
- FR25: Users can list their existing webhook subscriptions
- FR26: Users can delete a webhook subscription
- FR27: The system rejects duplicate webhook subscriptions
- FR28: The system delivers integration event payloads to registered webhook endpoints

### Platform & Operations

- FR29: Every service exposes a `/alive` health endpoint when responsive
- FR30: Services fail fast at startup when required configuration values are missing or invalid
- FR31: The system can be fully orchestrated and run locally via .NET Aspire with a single command
- FR32: Developers can access an Aspire dashboard showing service health and traces
- FR33: All services emit structured logs consumable by diagnostics tooling

### Developer & Reference Experience

- FR34: Developers can find architectural decision rationale in ADR documents for all major design choices
- FR35: Developers can run the full local setup using the documented local run flow
- FR36: Every service has a corresponding functional test suite executable in CI
- FR37: The system provides unit tests for application-layer handlers and filters
- FR38: Developers can extend the system by adding new services following documented patterns
- FR39: CI validates test suites, code coverage, and end-to-end scenarios on pull requests

### AI Integration

- FR40: The system supports connecting to Azure OpenAI for optional AI-enhanced scenarios when configured
- FR41: AI features are disabled by default and do not affect core commerce flows when unconfigured

## Non-Functional Requirements

### Performance

- Local developer startup for the full application should complete within 10 minutes from clone to running system using the documented local run flow.
- User-facing catalog browsing, basket operations, and authenticated navigation should remain responsive under normal development and demo conditions, with no request hanging beyond the configured 30-second HTTP client timeout.
- The system should support the core happy-path shopper journey without visible degradation during normal multi-service orchestration in Aspire.

### Security

- Developer signing credentials must be limited to development environments only; production deployments must use managed signing keys from a secure store.
- No real payment card data may be stored, processed, or transmitted by the reference implementation.
- Personally identifiable order data must be protected in transit and only accessible to authenticated users with appropriate access.
- Services should fail fast when required security-sensitive configuration is missing or invalid.
- REST-facing services should provide a consistent baseline of HTTP security headers before the project is considered production-ready.

### Reliability

- Every runnable service must expose a responsive `/alive` health endpoint suitable for local orchestration and deployment health probing.
- Functional and unit test suites must pass in CI before the reference app is considered release-ready.
- Order lifecycle processing must remain eventually consistent across asynchronous workers, with failures observable through logs and diagnosable through existing tooling.
- Configuration or startup failures must happen deterministically at startup, not during live request handling.

### Accessibility

- The web application should meet a WCAG 2.1 AA baseline for adopters targeting production use.
- Core commerce flows such as browsing, basket management, authentication, checkout, and order history should be operable with keyboard navigation and understandable with assistive technologies.
- Accessibility should be treated as a product-quality requirement for the public-facing web experience, even if the current reference app does not yet enforce it through automated checks.

### Integration

- The application must support reliable inter-service communication across REST, gRPC, and message-bus patterns already established in the architecture.
- External webhook delivery must be idempotent at subscription level and robust enough for downstream consumers to process order events consistently.
- Optional Azure OpenAI integration must remain isolated from core commerce behavior so that the system remains fully functional when AI configuration is absent.
- Adopters must be able to replace simulated integrations, such as payment processing, with real providers without breaking the overall architectural model.
