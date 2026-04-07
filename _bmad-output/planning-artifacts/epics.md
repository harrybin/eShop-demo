---
stepsCompleted:
  - 1
  - 2
inputDocuments:
  - _bmad-output/planning-artifacts/prd.md
  - _bmad-output/planning-artifacts/architecture.md
  - _bmad-output/planning-artifacts/ux-design-specification.md
workflowType: 'epics-stories'
project_name: 'eShop'
user_name: 'Harry'
date: '2026-04-07'
---

# eShop - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for eShop, decomposing the requirements from the PRD, UX Design if it exists, and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Shoppers can browse the product catalog without authentication.
FR2: Shoppers can view product details including name, description, price, and image.
FR3: Shoppers can filter the catalog by brand and/or product type.
FR4: Shoppers can navigate paginated catalog results.
FR5: The system can expose catalog data via a versioned REST API.
FR6: Shoppers can add products to a basket.
FR7: Shoppers can view and modify basket contents.
FR8: The system preserves basket state across authenticated sessions.
FR9: The system associates a basket with a user identity upon login.
FR10: Shoppers can proceed to checkout from the basket.
FR11: Authenticated shoppers can submit an order from their basket.
FR12: The system generates a unique order with a request identifier to prevent duplicate submissions.
FR13: Shoppers receive an order confirmation upon successful order submission.
FR14: The system clears the basket after a successful order.
FR15: Authenticated shoppers can view their order history.
FR16: Shoppers can view the current status of an individual order.
FR17: The system displays an order status timeline for each order.
FR18: The system progresses orders through lifecycle states via event-driven integration.
FR19: The system notifies registered webhook subscribers of order status changes.
FR20: Users can register and log in via the Identity service.
FR21: The system gates checkout and order history behind authenticated sessions.
FR22: The system uses environment-appropriate signing credentials.
FR23: Users can log out and their session state is cleared.
FR24: Authenticated users can register a webhook subscription for order events.
FR25: Users can list their existing webhook subscriptions.
FR26: Users can delete a webhook subscription.
FR27: The system rejects duplicate webhook subscriptions.
FR28: The system delivers integration event payloads to registered webhook endpoints.
FR29: Every service exposes a /alive health endpoint when responsive.
FR30: Services fail fast at startup when required configuration values are missing or invalid.
FR31: The system can be fully orchestrated and run locally via .NET Aspire with a single command.
FR32: Developers can access an Aspire dashboard showing service health and traces.
FR33: All services emit structured logs consumable by diagnostics tooling.
FR34: Developers can find architectural decision rationale in ADR documents for all major design choices.
FR35: Developers can run the full local setup using the documented local run flow.
FR36: Every service has a corresponding functional test suite executable in CI.
FR37: The system provides unit tests for application-layer handlers and filters.
FR38: Developers can extend the system by adding new services following documented patterns.
FR39: CI validates test suites, code coverage, and end-to-end scenarios on pull requests.
FR40: The system supports connecting to Azure OpenAI for optional AI-enhanced scenarios when configured.
FR41: AI features are disabled by default and do not affect core commerce flows when unconfigured.

### NonFunctional Requirements

NFR1: Full local startup should complete within 10 minutes from clone using documented local run flow.
NFR2: Core user-facing operations should remain responsive under normal development/demo load.
NFR3: Requests should not hang beyond the configured 30-second HTTP client timeout.
NFR4: Developer signing credentials must be development-only.
NFR5: Production deployments must use managed signing keys from a secure store.
NFR6: No real payment card data may be stored/processed/transmitted.
NFR7: PII in order data must be protected in transit and by auth boundaries.
NFR8: Security-sensitive configuration errors must fail fast at startup.
NFR9: REST services should provide consistent baseline security headers for production readiness.
NFR10: Every runnable service must expose /alive for liveness probing.
NFR11: Functional and unit test suites must pass in CI before release readiness.
NFR12: Order lifecycle processing must remain eventually consistent across async workers.
NFR13: Async failures must be observable and diagnosable via logs/tooling.
NFR14: Startup/config failures must be deterministic at startup, not runtime surprises.
NFR15: Web app should meet WCAG 2.1 AA baseline for production adopters.
NFR16: Core commerce flows should be operable by keyboard and assistive technologies.
NFR17: Inter-service integration across REST/gRPC/events must remain reliable.
NFR18: External webhook delivery must be idempotent at subscription level.
NFR19: Optional Azure OpenAI integration must be isolated from core commerce behavior.

### Additional Requirements

- Use existing eShop brownfield repository as baseline; do not scaffold a new starter template.
- Preserve service-per-boundary architecture with AppHost orchestration and ServiceDefaults reuse.
- Keep Basket API gRPC-first; do not add REST parity by default.
- Maintain asynchronous order progression through RabbitMQ-backed workers.
- Preserve PostgreSQL as system of record and Redis for basket/session state.
- Keep service-owned schema/migration boundaries and migration validation in testing.
- Enforce startup validation as mandatory production-safety behavior.
- Maintain explicit API versioning and ProblemDetails alignment across REST APIs.
- Preserve ADR constraints and architecture consistency rules as implementation gates.
- Keep test strategy layered: unit, functional, and Playwright E2E.
- Preserve local developer reproducibility via documented scripts/AppHost workflows.
- Track and complete roadmap-critical items: order status timeline and startup validation enforcement.

### UX Design Requirements

UX-DR1: Implement a confidence-first browse -> basket -> checkout -> status journey with explicit state transitions.
UX-DR2: Standardize semantic status vocabulary across cart, checkout, and order tracking surfaces.
UX-DR3: Implement order status timeline component with milestones and recovery-aware states.
UX-DR4: Implement checkout step rail component with explicit progress, blocked/error states, and resume behavior.
UX-DR5: Implement recovery action panel for auth interruption and async failure scenarios.
UX-DR6: Implement confirmation block pattern for commitment boundaries (order submitted/payment state changes).
UX-DR7: Apply tokenized color system with semantic mappings (primary/secondary/success/warning/error/pending).
UX-DR8: Apply tokenized typography scale with readable hierarchy for shopper and operator contexts.
UX-DR9: Apply 8-point spacing foundation with breakpoint-aware density adaptation.
UX-DR10: Enforce button hierarchy consistency (single primary action per decision zone).
UX-DR11: Enforce form patterns: inline + summary validation, non-destructive recovery, anti-duplicate submission behavior.
UX-DR12: Enforce navigation continuity patterns preserving progress through auth and detail transitions.
UX-DR13: Implement loading/empty/modal/search/filter behavior patterns with clear action guidance.
UX-DR14: Ensure responsive strategy across mobile/tablet/desktop with mobile-first implementation.
UX-DR15: Implement WCAG 2.1 AA baseline including contrast, keyboard navigation, and focus management.
UX-DR16: Ensure status communication is never color-only; include icon/text reinforcement.
UX-DR17: Support screen-reader-friendly async state updates (including aria-live where applicable).
UX-DR18: Ensure touch targets are mobile-accessible and critical actions remain persistently discoverable.
UX-DR19: Validate responsive/accessibility behavior through automated and manual checks across critical flows.
UX-DR20: Use shared component strategy (WebApp/WebAppComponents/HybridApp alignment) to prevent UX drift.

### FR Coverage Map

### FR Coverage Map

FR1: Epic 1 - Browse catalog anonymously
FR2: Epic 1 - View product details
FR3: Epic 1 - Filter by brand/type
FR4: Epic 1 - Paginate catalog results
FR5: Epic 1 - Versioned catalog API
FR6: Epic 2 - Add item to basket
FR7: Epic 2 - Modify basket contents
FR8: Epic 2 - Persist basket across sessions
FR9: Epic 2 - Associate basket with signed-in user
FR10: Epic 2 - Proceed from basket to checkout
FR11: Epic 3 - Submit order from basket
FR12: Epic 3 - Request ID/idempotent order creation
FR13: Epic 3 - Show order confirmation
FR14: Epic 3 - Clear basket after successful order
FR15: Epic 4 - View order history
FR16: Epic 4 - View current order status
FR17: Epic 4 - Show order status timeline
FR18: Epic 4 - Progress lifecycle via async events
FR19: Epic 4 - Publish webhook events for lifecycle changes
FR20: Epic 5 - User register/login
FR21: Epic 5 - Protect checkout/order history with auth
FR22: Epic 5 - Environment-safe signing credentials
FR23: Epic 5 - Logout/session clear
FR24: Epic 6 - Register webhook subscription
FR25: Epic 6 - List webhook subscriptions
FR26: Epic 6 - Delete webhook subscription
FR27: Epic 6 - Reject duplicate webhook subscriptions
FR28: Epic 6 - Deliver webhook payloads
FR29: Epic 7 - Expose /alive per service
FR30: Epic 7 - Fail-fast startup validation
FR31: Epic 7 - Run full app through Aspire orchestration
FR32: Epic 7 - Access Aspire dashboard health/traces
FR33: Epic 7 - Emit structured logs
FR34: Epic 7 - Provide ADR rationale discoverability
FR35: Epic 7 - Preserve local setup workflow
FR36: Epic 7 - Maintain functional test suites per service
FR37: Epic 7 - Maintain unit tests for handlers/filters
FR38: Epic 7 - Enable extensibility using documented patterns
FR39: Epic 7 - CI validates tests/coverage/E2E
FR40: Epic 8 - Support optional Azure OpenAI integration
FR41: Epic 8 - Keep AI features disabled/isolated by default

## Epic List

## Epic List

### Epic 1: Catalog Discovery and Product Selection
Enable shoppers to discover, inspect, and shortlist products quickly through a responsive catalog browsing experience.
**FRs covered:** FR1, FR2, FR3, FR4, FR5

### Epic 2: Basket and Session Continuity
Enable shoppers to build and manage their basket confidently with continuity across auth and session boundaries.
**FRs covered:** FR6, FR7, FR8, FR9, FR10

### Epic 3: Checkout Commitment and Order Capture
Enable authenticated shoppers to place orders with clear confirmation and safe duplicate-prevention behavior.
**FRs covered:** FR11, FR12, FR13, FR14

### Epic 4: Order Lifecycle Transparency and Tracking
Enable shoppers and operators to understand asynchronous order progression via clear statuses, timeline visibility, and event-driven updates.
**FRs covered:** FR15, FR16, FR17, FR18, FR19

### Epic 5: Identity and Access Foundation
Enable secure account access and protected commerce journeys with environment-safe identity behavior.
**FRs covered:** FR20, FR21, FR22, FR23

### Epic 6: Webhook Subscription and Delivery Experience
Enable authenticated users to manage webhook subscriptions and receive reliable event notifications.
**FRs covered:** FR24, FR25, FR26, FR27, FR28

### Epic 7: Platform Reliability, Developer Experience, and Quality Gates
Enable dependable operation, observability, and contributor confidence through consistent runtime behavior, documentation, and test automation.
**FRs covered:** FR29, FR30, FR31, FR32, FR33, FR34, FR35, FR36, FR37, FR38, FR39

### Epic 8: Optional AI Integration Boundaries
Enable optional AI-enhanced capabilities without impacting core commerce reliability when AI is not configured.
**FRs covered:** FR40, FR41
