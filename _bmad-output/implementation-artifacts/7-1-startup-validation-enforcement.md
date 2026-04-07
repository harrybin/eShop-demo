# Story 7.1: Enforce startup configuration validation across runnable services

Status: ready-for-dev

## Story

As a platform operator,
I want all required production configuration validated at startup,
so that services fail fast with actionable errors instead of failing mid-request.

## Acceptance Criteria

1. All runnable services validate required configuration at startup in non-development environments.
2. Missing/placeholder/invalid critical settings produce deterministic startup failure with clear diagnostic messages.
3. Validation approach is consistent across APIs and workers, with service-specific options binding where needed.
4. Existing production appsettings templates remain valid skeletons and are documented with expected overrides.
5. Functional smoke checks confirm healthy startup when configuration is valid and startup failure when invalid.

## Tasks / Subtasks

- [ ] Define reusable startup validation pattern (AC: 1, 3)
  - [ ] Use strongly typed options with ValidateOnStart where applicable
- [ ] Apply validation to each runnable service (AC: 1, 2)
  - [ ] Basket.API, Catalog.API, Identity.API, Ordering.API, Webhooks.API, OrderProcessor, PaymentProcessor, WebApp
- [ ] Improve failure diagnostics and logging for invalid config (AC: 2)
- [ ] Validate and document appsettings.Production placeholders and required secrets (AC: 4)
- [ ] Add targeted functional checks for startup validation outcomes (AC: 5)

## Dev Notes

- Brownfield focus: production appsettings files already exist but enforcement is marked partial and must be completed.
- Keep Program.cs orchestration-only where possible; prefer extension methods and options classes.
- Maintain cancellation-token and diagnostics conventions already used in services.
- Do not weaken security posture: dev-only signing credentials remain dev-gated; production must require secure key material.

### Project Structure Notes

- Primary locations: src/*/Program.cs and src/*/Extensions/*.cs for each runnable service
- Shared behavior candidate: src/eShop.ServiceDefaults
- Validation tests: tests/*FunctionalTests (service-specific)

### References

- Source: _bmad-output/planning-artifacts/prd.md (FR30, NFR8, NFR14)
- Source: _bmad-output/planning-artifacts/architecture.md (startup validation mandatory, options binding patterns)
- Source: IMPROVEMENTS.md (Configuration: Production appsettings and environment validation marked Partial)

## Dev Agent Record

### Agent Model Used

GPT-5.3-Codex

### Debug Log References

- n/a

### Completion Notes List

- Story context generated from brownfield gap analysis of current implementation state.

### File List

- src/Basket.API/**
- src/Catalog.API/**
- src/Identity.API/**
- src/Ordering.API/**
- src/Webhooks.API/**
- src/OrderProcessor/**
- src/PaymentProcessor/**
- src/WebApp/**
- src/eShop.ServiceDefaults/**
- tests/**FunctionalTests/**
