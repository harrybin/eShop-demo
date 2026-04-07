---
project_name: 'eShop'
user_name: 'Harry'
date: '2026-04-07'
sections_completed: ['technology_stack', 'language_csharp', 'aspdotnet_core', 'testing', 'code_quality', 'development_workflow', 'critical_rules']
status: 'complete'
rule_count: 36
optimized_for_llm: true
existing_patterns_found: 10
---

# Project Context for AI Agents

High-signal implementation constraints for this repository.

---

## Technology Stack & Versions

- .NET 9.0, SDK 10.0.100, C# (warnings as errors)
- ASP.NET Core minimal APIs + gRPC
- API versioning via Asp.Versioning
- EF Core + PostgreSQL (Npgsql) + Pgvector
- Redis for cache/session
- RabbitMQ event bus + worker processors
- .NET Aspire orchestration
- OpenTelemetry and structured logging
- MSTest SDK 4.0.2 + Playwright 1.42.x

## Critical Implementation Rules

### Language-Specific Rules (C#)

- Async methods take CancellationToken as final parameter and forward it through all I/O.
- Use GlobalUsings.cs for shared imports; avoid repeating common usings per file.
- Keep strong typing; avoid dynamic and weakly typed payloads.
- For data access, compose IQueryable then materialize with ToListAsync(ct).
- Use AsNoTracking for read-only queries.
- Catch specific exceptions and log with context.

### Framework-Specific Rules (ASP.NET Core / gRPC)

- Program.cs order: AddServiceDefaults -> AddApplicationServices -> API-specific registration.
- MapDefaultEndpoints before custom mappings.
- Keep Program.cs orchestration-only; move wiring into extension methods.
- Use route groups with explicit versions (v1/v2) and minimal branching inside handlers.
- Endpoint handlers are static and DI-driven, including AsParameters where applicable.
- Endpoint metadata is required: WithName, WithSummary, WithDescription, WithTags.
- Return typed HTTP results and explicit ProblemDetails metadata for error cases.
- Basket API stays gRPC-first (ADR-001). No REST parity endpoints.
- Identity signing credentials are development-only; production must use managed signing keys.

### Testing Rules

- Run targeted tests first based on impacted services.
- Unit tests for logic changes; functional tests for API/integration changes.
- Escalate to broader suites for shared or high-risk changes.
- Treat failing tests as blockers unless explicitly exempted.
- Report exact test commands and results; do not claim tests not run.
- Keep tests isolated; no order dependency or shared mutable state.

### Code Quality & Style Rules

- Zero warnings required; TreatWarningsAsErrors is mandatory.
- Naming: PascalCase for types/members, camelCase for locals/parameters.
- Keep APIs/contracts explicit; avoid magic strings and hidden behavior.
- Use structured logging and never log secrets or sensitive data.
- Keep comments focused on why, not what.

### Development Workflow Rules

- Read relevant ADRs before implementing architectural changes.
- Keep commits atomic and PR descriptions explicit about behavior changes.
- Ensure CI-relevant suites pass before merge.
- Update affected docs when contracts/setup/architecture changes.

### Critical Don't-Miss Rules

- Basket remains gRPC-only by architecture decision.
- Do not hardcode secrets; use env vars or secure secret stores.
- Do not block on async with Result/Wait.
- Do not share DbContext across concurrent operations.
- Use EventBus for cross-service order progression; avoid synchronous service chaining.
- Response-shape breaking changes require versioned API evolution.
- OrderingContext constructor changes must preserve AddDbContextPool compatibility.
- Event-driven processors must enforce idempotency and retries with correlation-aware telemetry.
- In production, startup must fail fast if signing key material is missing.

---

## Agent Checkpoint

Before finishing a change, verify:
1. ADR constraints are respected (gRPC-only Basket, pooled OrderingContext, secure Identity signing).
2. CancellationToken is propagated in async call chains.
3. Error responses use ProblemDetails and endpoint metadata is complete.
4. Tests for impacted areas pass and are reported with exact commands.
5. No secrets are hardcoded and build is warning-free.

---

## Usage Guidelines

### For AI Agents

- Read this file before implementing.
- Follow the strictest applicable rule when uncertain.
- Prioritize consistency with existing patterns over novel style choices.

### For Team Members

- Update this file when stack versions, ADRs, or recurring review findings change.
- Remove rules that become redundant due to tooling or defaults.
- Keep content concise and implementation-focused.

---

Last Updated: 2026-04-07
Status: Ready for AI Agent Integration
Optimization Level: Compact
