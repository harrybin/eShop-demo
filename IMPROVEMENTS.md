# eShop Repository: Improvement Roadmap

**Date:** April 2, 2026  
**Scope:** Architecture, features, security, testing, dependencies, and developer experience

---

## Executive Summary

This eShop reference implementation has solid foundations (Aspire orchestration, microservice patterns, modern .NET stack) but has identifiable gaps in:
- **Consistency:** duplicated validation patterns, inconsistent endpoint mapping, mixed naming conventions across APIs
- **Features:** checkout and order tracking depth, worker reliability, cart/session persistence
- **Quality gates:** missing code coverage reporting, incomplete test coverage for workers/webhooks/identity
- **Security/Perf:** development credentials exposed in code path, DbContext pooling disabled, no health check dependencies, missing response caching
- **Updates:** dependency versions need strategic grouping and CI validation

This document prioritizes concrete, actionable improvements to reduce delivery risk and improve customer experience.

---

## Part 1: Code Quality & Architecture Improvements

### 1. Eliminate Duplicated Request Validation Pattern
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** Maintainability, code clarity

**Current State:**
- `Ordering.API/Apis/OrdersApi.cs` has identical `requestId == Guid.Empty` checks in 3+ endpoints (lines 27-29, 56-58, 132-134)
- Same pattern likely repeated in other APIs

**Recommendation:**
- Extract validation to an `IEndpointFilter` or reusable FluentValidation rule
- Apply globally via middleware or per route group
- **Files affected:** `src/Ordering.API/Apis/OrdersApi.cs`, similar patterns in other API files
- **Expected outcome:** 15+ lines of duplication removed, consistent validation behavior

---

### 2. Standardize Endpoint Mapping Conventions
**Severity:** Medium | **Effort:** 2-3 days | **Impact:** Developer experience, consistency

**Current State:**
- `Catalog.API/Apis/CatalogApi.cs`: `MapCatalogApi()` returns `IEndpointRouteBuilder`, no API versioning in return type
- `Ordering.API/Apis/OrdersApi.cs`: `MapOrdersApiV1()` returns `RouteGroupBuilder` with `.HasApiVersion(1.0)`
- `Webhooks.API/Apis/WebHooksApi.cs`: follows Ordering pattern
- `Basket.API`: No REST endpoint extension (gRPC-only, architectural mismatch)
- No cancellation token support in any handler (violates dotnet-api.instructions.md requirement)

**Recommendation:**
- Standardize all REST APIs to return `RouteGroupBuilder` with explicit versioning
- Add `CancellationToken ct` parameter to all async handlers
- Consider adding Basket REST parity endpoints or document gRPC-first strategy
- **Files affected:** `src/Catalog.API/Apis/CatalogApi.cs`, `src/Ordering.API/Apis/OrdersApi.cs`, `src/Webhooks.API/Apis/WebHooksApi.cs`, `src/Basket.API/Program.cs`
- **Expected outcome:** Consistent API shape, graceful shutdown support, improved IDE discoverability

---

### 3. Align Service Defaults Naming Convention
**Severity:** Low | **Effort:** 1 day | **Impact:** Clarity, consistency

**Current State:**
- `Basket.API`, `OrderProcessor`: call `AddBasicServiceDefaults()`
- `Catalog`, `Ordering`, `Webhooks`, `Identity`: call `AddServiceDefaults()`
- Naming suggests different behavior but likely incomplete refactoring

**Recommendation:**
- Audit both methods to confirm behavior equivalence
- Standardize naming across all projects (recommend `AddServiceDefaults()` as base)
- Document in code comments what each does
- **Files affected:** `src/Basket.API/Program.cs`, `src/OrderProcessor/Program.cs`, and consumer APIs

---

### 4. Unify API Versioning & OpenAPI Setup
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** API documentation accuracy, consistency

**Current State:**
- `Catalog`, `Ordering`: have `AddApiVersioning()` + `AddDefaultOpenApi()`
- `Identity`, `Basket`: missing versioning setup (Basket is gRPC-only, but Identity is REST without versioning)
- Inconsistent error response formats

**Recommendation:**
- Add `AddApiVersioning()`, `AddDefaultOpenApi()`, and `AddProblemDetails()` to all REST APIs
- Move common setup to `eShop.ServiceDefaults/Extensions.cs` if applicable
- Document versioning strategy (default to v1.0)
- **Files affected:** `src/Identity.API/Program.cs`, `src/eShop.ServiceDefaults/Extensions.cs`
- **Expected outcome:** Consistent OpenAPI schemas, RFC 7807 error responses across all APIs

---

### 5. Centralize Logging Pattern & Remove Duplication
**Severity:** Low | **Effort:** 1 day | **Impact:** Code clarity, maintenance burden

**Current State:**
- `Ordering.API/Apis/OrdersApi.cs` (lines 36, 65, 110): identical `LogInformation("Sending command: {CommandName}...")` structure

**Recommendation:**
- Create shared logging helpers in `eShop.Shared` or API extension layer
- Apply consistently across endpoint handlers
- **Expected outcome:** Reduced duplication, standardized logging depth/fields

---

### 6. Standardize DB Context & Configuration Pooling
**Severity:** High | **Effort:** 2-3 days | **Impact:** Performance (+30-50% under load)

**Current State:**
- `Catalog.API`: Custom `configureDbContextOptions` with pgvector support
- `Ordering.API`: **Connection pooling explicitly disabled** (comment cites DbContext constructor error)
- `Webhooks.API`: Standard `AddNpgsqlDbContext` without enrichment

**Recommendation:**
- Investigate and fix the DbContext pooling issue in Ordering.API (likely refactor constructor or use factory pattern)
- Align all APIs on consistent EF Core configuration (pooling enabled, enrichment applied)
- Document any service-specific constraints
- **Files affected:** `src/Ordering.API/Extensions/Extensions.cs`, `src/Catalog.API/Extensions/Extensions.cs`, `src/Webhooks.API/Extensions/Extensions.cs`
- **Expected outcome:** Consistent database connection efficiency, improved throughput

---

### 7. Standardize Options Binding & Validation
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** Runtime safety, configuration clarity

**Current State:**
- `Catalog.API`, `WebhookClient`: use `AddOptions<T>().BindConfiguration()`
- `PaymentProcessor`: inline `BindConfiguration()`
- Other APIs: no validation convention (errors discovered at runtime)

**Recommendation:**
- Standardize all configuration binding via `AddOptions<T>()` in Program.cs
- Add validation rules via `ValidateOnStart()` or `FluentValidation` rules
- **Files affected:** `src/PaymentProcessor/Program.cs`, `src/OrderProcessor/Program.cs`, `src/Webhooks.API/Program.cs`, etc.
- **Expected outcome:** Config errors caught at startup, not in the middle of requests

---

## Part 2: Security Improvements

### 1. Lock Down Production Identity Configuration
**Severity:** Critical | **Effort:** 1-2 days | **Impact:** Prevents secret key exposure in production

**Current State:**
- `Identity.API/Program.cs` (lines 28-29, 36-37):
  ```csharp
  // TODO: Remove this line in production.
  options.KeyManagement.Enabled = false;
  ...
  // TODO: Not recommended for production
  .AddDeveloperSigningCredential();
  ```
- Development-only settings are not environment-gated

**Recommendation:**
- Environment-gate all dev-only settings behind `if (app.Environment.IsDevelopment())`
- Use Azure Key Vault / secure configuration management in production for signing keys
- Document key management strategy in ADR or wiki
- **Files affected:** `src/Identity.API/Program.cs`
- **Expected outcome:** Zero risk of leaked signing credentials in production deployments

---

### 2. Add Security Headers to All Microservices
**Severity:** Medium | **Effort:** 1 day | **Impact:** Defense-in-depth against web attacks

**Current State:**
- Only `Identity.API/Quickstart/SecurityHeadersAttribute.cs` implements headers (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy)
- REST APIs (Catalog, Ordering, Webhooks, Basket) lack security headers

**Recommendation:**
- Move `SecurityHeadersAttribute` or create middleware in `eShop.ServiceDefaults/Extensions.cs`
- Apply globally to all microservices
- Document in ADR why each header is included
- **Files affected:** `src/eShop.ServiceDefaults/Extensions.cs`, all API `Program.cs` files
- **Expected outcome:** Consistent XSS, clickjacking, and MIME sniffing defenses

---

### 3. Implement Response Caching Strategy
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** Reduced DB load, improved latency

**Current State:**
- No `CacheControl` or response caching headers
- Repeated reads to Catalog, health checks bypass cache

**Recommendation:**
- Add `[OutputCache]` attribute or `CacheControl` middleware for:
  - Catalog items: 1-hour max-age (invalidate on mutation)
  - Health checks: 10-second max-age
  - Profile/user data: 5-minute max-age
- Use conditional ETag headers for stale-while-revalidate patterns
- **Files affected:** `src/Catalog.API/Apis/CatalogApi.cs`, health check endpoints
- **Expected outcome:** 40%+ reduction in read-heavy endpoint load, improved client performance

---

## Part 3: Performance & Observability Improvements

### 1. Add Production-Safe Health Check Endpoints
**Severity:** Medium | **Effort:** 1 day | **Impact:** Enables Kubernetes graceful shutdown, better diagnostics

**Current State:**
- `eShop.ServiceDefaults/Extensions.cs` (lines 114-123): Health checks only mapped in development
- No dependency health checks (Redis, RabbitMQ, PostgreSQL)

**Recommendation:**
- Create both public and internal health check groups:
  - `/alive` (public, no auth): app-is-responsive only
  - `/ready` (internal, requires auth token): includes dependency checks
- Add dependency checks (PostgreSQL, Redis, RabbitMQ connectivity)
- Document security model (allow `/alive` for LB, require `/ready` for Kubernetes)
- **Files affected:** `src/eShop.ServiceDefaults/Extensions.cs`
- **Expected outcome:** Kubernetes and load balancers can detect and fail over unhealthy instances

---

### 2. Set Request Timeouts on HTTP Clients & gRPC
**Severity:** Low | **Effort:** 1 day | **Impact:** Prevents connection pool exhaustion

**Current State:**
- No explicit timeout configuration on HttpClient or gRPC service options

**Recommendation:**
- Set default request timeout to 30 seconds in `ConfigureHttpClientDefaults`
- Set gRPC operation timeout similarly
- Allow per-endpoint tuning via explicit TimeSpan parameters
- **Files affected:** `src/eShop.ServiceDefaults/Extensions.cs`, all `Program.cs` files with HTTP/gRPC clients
- **Expected outcome:** Prevents infinite hangs, improves debugging experience

---

### 3. Enable Code Coverage Reporting in CI
**Severity:** Medium | **Effort:** 1 day | **Impact:** Visibility into test quality, quality gates

**Current State:**
- CI doesn't collect or report code coverage metrics despite `.gitignore` including coverage patterns

**Recommendation:**
- Add `--collect:"XPlat Code Coverage"` to `.github/workflows/pr-validation.yml`
- Use `codecov/codecov-action` to report and trend metrics
- Set minimum coverage gate (recommend 70% line coverage)
- **Files affected:** `.github/workflows/pr-validation.yml`
- **Expected outcome:** Identify under-tested code paths, prevent coverage regressions

---

## Part 4: Test & Quality Improvements

### 1. Expand Functional Test Coverage for Missing APIs
**Severity:** High | **Effort:** 3-5 days | **Impact:** Prevents regressions in event-driven services

**Current State:**
- **Missing tests:**
  - Identity API: 0 tests (only used as dependency in Ordering tests)
  - Webhooks API: 0 tests (no webhook registration/delivery/retry scenarios)
  - OrderProcessor: 0 tests (no integration event handling validation)
  - PaymentProcessor: 0 tests (no payment event processing)
  - EventBus/EventBusRabbitMQ: 0 tests

**Recommendation:**
- Create `tests/Identity.FunctionalTests` with Aspire fixture for auth flows
- Create `tests/Webhooks.FunctionalTests` with webhook registration, delivery, and retry scenarios
- Create `tests/OrderProcessor.FunctionalTests` for order state machine and event handling
- Create `tests/PaymentProcessor.FunctionalTests` for payment event flows
- **Expected outcome:** 20+ additional integration scenarios covered, confidence in async pipelines

---

### 2. Fix Solution Filter & Test Discoverability
**Severity:** Low | **Effort:** 1 day | **Impact:** Developer experience, local test consistency

**Current State:**
- `eShop.Web.slnf` does not include `tests/ClientApp.UnitTests/ClientApp.UnitTests.csproj`
- Local developers may skip these tests when running `dotnet test eShop.Web.slnf`

**Recommendation:**
- Add `ClientApp.UnitTests` to solution filter
- Document test discovery strategy (required vs. optional test projects)
- **Files affected:** `eShop.Web.slnf`
- **Expected outcome:** `dotnet test` runs all tests consistently

---

### 3. Expand E2E Playwright Test Suite
**Severity:** Medium | **Effort:** 2-3 days | **Impact:** Catches regressions in user-critical flows

**Current State:**
- Only 3 Playwright specs: AddItem, RemoveItem, Browse
- Missing: checkout flow, payment failure handling, order history, auth failures, cart persistence
- Single browser (Chrome); no cross-browser coverage

**Recommendation:**
- Add checkout flow with valid/invalid payment scenarios
- Add order history and tracking test
- Add auth failure and session timeout scenarios
- Add cart persistence and recovery flow
- Enable Firefox/Safari runs in nightly workflow
- **Files affected:** `e2e/`, `playwright.config.ts`, `.github/workflows/playwright.yml`
- **Expected outcome:** Critical user journeys backed by automation, cross-browser confidence

---

### 4. Improve Aspire Fixture Reliability
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** Reduces flaky CI builds

**Current State:**
- `CatalogApiFixture.cs`: uses `pgvector` image "latest" tag (non-deterministic)
- `OrderingApiFixture.cs`: spins up 2 PostgreSQL containers + Identity API; potential startup ordering issues
- No container startup retry logic

**Recommendation:**
- Pin all image versions in fixtures (e.g., `postgres:16-alpine`, specific pgvector semver)
- Add explicit container startup health checks with retry logic
- Document expected startup sequence
- **Files affected:** `tests/Catalog.FunctionalTests/CatalogApiFixture.cs`, `tests/Ordering.FunctionalTests/OrderingApiFixture.cs`
- **Expected outcome:** Reduced flakiness, faster local test iteration

---

### 5. Add Unit Tests for Basket API REST Endpoints
**Severity:** Low | **Effort:** 1 day | **Impact:** API contract validation

**Current State:**
- `tests/Basket.UnitTests/BasketServiceTests.cs`: only 3 tests, all gRPC service focused
- No REST endpoint testing (if applicable)

**Recommendation:**
- Add gRPC endpoint contract tests (request/response serialization)
- If REST endpoints exist alongside gRPC, add parity tests
- **Files affected:** `tests/Basket.UnitTests/`
- **Expected outcome:** Basket API contract confidence

---

## Part 5: Feature Improvements

### 1. Strengthen Checkout & Payment Flow
**Severity:** High | **Effort:** 5-7 days | **Impact:** Direct revenue impact, customer experience

**Current State:**
- E2E suite lacks checkout scenario
- No documented retry/recovery for payment failures
- Limited error messaging for users

**Recommendation:**
- Implement multi-step checkout UI in WebApp (cart review, shipping, payment, confirmation)
- Add resilient payment retry flow in `Ordering.API` endpoints
- Implement idempotency key support for payment safety
- Return rich error messages for payment failures (user-actionable)
- Create integration tests for checkout lifecycle
- **Files affected:** `src/WebApp/Program.cs`, `src/Ordering.API/Apis/OrdersApi.cs`, `src/Ordering.API/Extensions/`
- **Expected outcome:** 2-5% increase in checkout completion rate, reduced support burden

---

### 2. Add Order History & Tracking
**Severity:** High | **Effort:** 3-4 days | **Impact:** Customer retention, operational visibility

**Current State:**
- No order status timeline or tracking history visible to users
- No order-level detail view

**Recommendation:**
- Add order list endpoint with pagination and filtering (status, date range)
- Add order detail endpoint with line-item breakdown and status history
- Implement WebApp UI for order history and tracking page
- Add webhook-based status notifications (order confirmed, shipped, delivered)
- **Files affected:** `src/Ordering.API/Apis/OrdersApi.cs`, `src/WebApp/Program.cs`, `src/Webhooks.API/`
- **Expected outcome:** Improved customer satisfaction, reduced "where's my order?" support tickets

---

### 3. Enhance Cart & Session Persistence
**Severity:** Medium | **Effort:** 2-3 days | **Impact:** Cart abandonment reduction, UX continuity

**Current State:**
- Cart state may not persist across sessions or devices
- No documented session recovery strategy

**Recommendation:**
- Tie cart persistence to user identity (via Identity API)
- Implement cart merge logic when user logs in (combine anonymous + authenticated carts)
- Add Redis-backed session storage for fast retrieval
- Implement 30-day cart recovery ("Your cart is still there")
- **Files affected:** `src/Basket.API/`, `src/WebApp/Program.cs`, `src/Identity.API/`
- **Expected outcome:** 5-10% reduction in cart abandonment

---

### 4. Improve Webhook & Event Reliability
**Severity:** High | **Effort:** 3-4 days | **Impact:** Confidence in async business flows

**Current State:**
- Webhook delivery retry strategy undocumented
- No idempotency key support for webhook handlers
- OrderProcessor and PaymentProcessor lack observable failure handling

**Recommendation:**
- Add idempotency key to webhook payloads and handlers
- Implement exponential backoff retry (3 retries, 1s/10s/60s intervals)
- Add dead-letter queue observability for failed webhooks
- Implement webhook endpoint heartbeat/status API
- **Files affected:** `src/Webhooks.API/`, `src/OrderProcessor/`, `src/PaymentProcessor/`, `src/EventBus/`
- **Expected outcome:** Visibility into failed async operations, improved resilience

---

### 5. Add Support for Order Cancellation & Returns
**Severity:** Medium | **Effort:** 4-5 days | **Impact:** Customer satisfaction, operations management

**Current State:**
- No order cancellation flow
- No return/refund mechanism

**Recommendation:**
- Implement order cancellation endpoint (time-gated: cancel within X hours only)
- Implement return request endpoint with item selection
- Track return state through OrderProcessor
- Add webhook events for refund processing
- **Files affected:** `src/Ordering.API/Apis/OrdersApi.cs`, `src/OrderProcessor/`, `src/Webhooks.API/`
- **Expected outcome:** Improved customer experience, operational tooling for support team

---

## Part 6: Dependency & Update Strategy

### 1. Strategic Dependency Upgrade Plan
**Severity:** Medium | **Effort:** 1-2 weeks (phased) | **Impact:** Security patches, bug fixes, performance improvements

**Current Versions (as of April 2026):**
- .NET SDK: 10.0.100
- Aspire: 13.2.0
- Entity Framework Core: 10.0.5
- OpenTelemetry: 1.15.0
- Playwright: 1.42.1

**Recommendation:**
Execute updates in three waves to reduce rollback impact:

**Wave 1: Platform Baseline** (1-2 days)
- Review .NET 10 service releases and apply latest .NET 10.0.x patch
- Update Aspire to latest 13.x.x release (compatibility matrix with .NET 10)
- Run full CI suite + manual smoke tests

**Wave 2: Observability & Telemetry** (2-3 days)
- Update OpenTelemetry packages to latest stable
- Update any pre-release API instrumentation packages to stable if available
- Validate metric/trace export pipeline

**Wave 3: Test & Development Tooling** (1-2 days)
- Update Playwright to latest stable
- Update test frameworks (MsTest.Sdk, xUnit)
- Update Grpc.Tools
- Run full test suite

**Files affected:** `Directory.Packages.props`, `global.json`, `package.json`
- **Expected outcome:** Up-to-date dependencies, reduced security exposure

---

### 2. Dependency Version Pinning Strategy
**Severity:** Low | **Effort:** 1 day | **Impact:** Reproducibility, CI stability

**Current State:**
- `Directory.Packages.props` uses good versioning discipline overall
- Some preview versions (Aspire.Azure.AI.OpenAI marked `-preview.1`)
- Aspire fixture uses image "latest" tag (non-deterministic)

**Recommendation:**
- Remove all `-preview` version suffixes where stable alternatives exist
- Add `allowPrerelease: false` to `global.json` (currently `allowPrerelease: true`)
- Pin all test fixture image versions explicitly (no "latest" tags)
- Document version upgrade cadence (quarterly for major, monthly for patch)
- **Files affected:** `Directory.Packages.props`, `global.json`, `tests/*/Fixture.cs`
- **Expected outcome:** Reproducible builds, easier debugging of version-related issues

---

### 3. Production Configuration Review
**Severity:** Medium | **Effort:** 1-2 days | **Impact:** Deployment safety, feature parity

**Current State:**
- Development-only features not consistently gated (e.g., database auto-migrations, dev signing credentials)
- No clear environment-to-appsettings mapping

**Recommendation:**
- Create `appsettings.Production.json` templates for each microservice
- Document required production environment variables
- Audit all `if (app.Environment.IsDevelopment())` checks
- Add startup validation that production configs are not using dev-only features
- **Files affected:** All `appsettings.json` and `Program.cs` files
- **Expected outcome:** Safe production builds, clear deployment checklists

---

## Part 7: Developer Experience Improvements

### 1. Add Local Development Troubleshooting Guide
**Severity:** Low | **Effort:** 1-2 days | **Impact:** Faster developer onboarding

**Create:** `docs/LOCAL_SETUP.md`

**Content:**
- Docker prerequisite setup and validation
- Common issues (port conflicts, slow startup, database connection timeouts)
- How to access Aspire dashboard
- How to run individual services vs. full suite
- Health check URLs and expected responses
- Common database migration issues

**Expected outcome:** New contributors save 2-4 hours of debugging

---

### 2. Improve Local Run Script Output
**Severity:** Low | **Effort:** 1 day | **Impact:** Developer clarity

**File:** `build/local/run-local.ps1`

**Recommendation:**
- Print a summary table of all service URLs after startup:
  ```
  Service           URL                         Status
  ─────────────────────────────────────────────────────
  WebApp            http://localhost:5045      ✓ Ready
  Catalog API       http://localhost:8000      ✓ Ready
  Ordering API      http://localhost:8001      ✓ Ready
  ...
  ```
- Add estimated startup time
- Add link to Aspire dashboard

**Expected outcome:** Clearer local development experience

---

### 3. Add Architecture Decision Records (ADRs)
**Severity:** Low | **Effort:** 2-3 days | **Impact:** Knowledge sharing

**Create:** `docs/adr/` folder with:
- ADR-001: Why gRPC for Basket API vs. REST
- ADR-002: Why DbContext pooling is disabled in Ordering API (and path to re-enable)
- ADR-003: Why development credentials exist in Identity API and production strategy
- ADR-004: Event-driven architecture for OrderProcessor and PaymentProcessor

**Expected outcome:** Reduced knowledge silos, faster onboarding

---

## Prioritized Implementation Roadmap

### Phase 0: Guardrails & Foundation (1 week)
- [ ] Security: Lock down Identity.API production settings
- [ ] CI: Add code coverage reporting and quality gates
- [ ] Testing: Fix solution filter (add ClientApp.UnitTests)

### Phase 1: Consistency & Reliability (1-2 weeks)
- [ ] Code quality: Standardize endpoint mapping and add cancellation tokens
- [ ] Code quality: Eliminate validation duplication
- [ ] Database: Fix DbContext pooling in Ordering.API
- [ ] Observability: Add health check endpoints and dependency checks

### Phase 2: Feature Depth (3-4 weeks)
- [ ] Feature: Implement full checkout flow
- [ ] Feature: Add order history and tracking
- [ ] Testing: Create functional tests for Identity, Webhooks, workers
- [ ] Testing: Expand Playwright E2E scenarios

### Phase 3: Event-Driven Resilience (2 weeks)
- [ ] Feature: Webhook reliability (idempotency, retry, DLQ)
- [ ] Feature: Order cancellation and returns
- [ ] Testing: OrderProcessor and PaymentProcessor functional tests

### Phase 4: Dependency Updates (2 weeks)
- [ ] Dependencies: Execute Wave 1 platform updates (.NET, Aspire)
- [ ] Dependencies: Execute Wave 2 observability updates
- [ ] Dependencies: Execute Wave 3 tooling updates
- [ ] Configuration: Production appsettings and environment validation

### Phase 5: Polish & Documentation (1 week)
- [ ] Docs: Troubleshooting guide and ADRs
- [ ] DX: Enhance local run script output
- [ ] Testing: Database migration tests

---

## Reference: Files Most Impacted by Improvements

| File | Changes |
|------|---------|
| `src/Identity.API/Program.cs` | Lock dev-only settings; versioning; problem details |
| `src/Ordering.API/Apis/OrdersApi.cs` | Add cancellation tokens; centralize validation; add order lifecycle endpoints |
| `src/Ordering.API/Extensions/Extensions.cs` | Fix DbContext pooling |
| `src/eShop.ServiceDefaults/Extensions.cs` | Security headers; health checks with dependencies; timeouts; centralized resilience |
| `src/Catalog.API/Apis/CatalogApi.cs` | Add caching; standardize endpoint mapping |
| `src/WebApp/Program.cs` | Checkout flow; order history UI; cart persistence |
| `.github/workflows/pr-validation.yml` | Code coverage collection and gating |
| `Directory.Packages.props` | Remove preview versions; pin for reproducibility |
| `tests/` (new/expanded) | Identity, Webhooks, OrderProcessor, PaymentProcessor functional tests |
| `e2e/` | Expanded Playwright scenarios (checkout, auth failures, order tracking) |

---

## Success Metrics

After implementing these improvements, track:
- **Quality:** Code coverage ≥70%, test suite passes pre-commit
- **Performance:** Order API P95 latency <500ms (with caching), throughput +30% under load
- **Security:** Zero dev credentials in production builds, all security headers present
- **Features:** Checkout completion rate +2-5%, order history adoption >40% of users
- **Reliability:** Webhook success rate >99.5%, no orphaned orders from async failures
- **Developer velocity:** New contributor onboarding <2 hours (with docs)

---

## Questions & Next Steps

1. **Phasing:** Do you want to implement Phase 0-1 immediately, or prioritize specific features first?
2. **Feature flags:** Should larger UX changes (checkout redesign) use feature flags for gradual rollout?
3. **Timing:** Any hard deadline for security hardening (e.g., before production deployment)?

