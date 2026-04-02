---
description: "Use when implementing or refactoring ASP.NET Core APIs in eShop. Covers Program.cs setup, DI registration, endpoint mapping, options binding, validation, logging, cancellation tokens, API versioning, and OpenAPI consistency. Keywords: Program.cs, minimal API, AddApplicationServices, MapGroup, AddApiVersioning, OpenAPI, ProblemDetails, cancellation token."
name: "ASP.NET Core API Implementation"
---

# ASP.NET Core API Implementation

- Must call service-default extensions first in Program.cs, then API-specific registration.
- Must keep Program.cs orchestration-only by using extension methods for service registration and endpoint mapping.
- Must map default platform endpoints and OpenAPI endpoints for consistency.
- Must organize endpoint routes with route groups and explicit API version declarations.
- Must keep endpoint handlers static and dependency-injected, with clear request models.
- Must use strongly typed options binding for configuration sections.
- Must register service dependencies in clear sections and keep ordering consistent.
- Must return typed HTTP results and explicit problem responses for validation failures.
- Must validate inputs early and fail fast with actionable problem details.
- Must include response metadata that matches real outcomes.
- Must inject ILogger into services and use structured logging.
- Must avoid expensive debug message construction unless debug logging is enabled.
- Must accept a cancellation token in async handlers and pass it through all I/O calls.
- Must keep API version branching minimal and only where behavior truly differs.
- Must preserve endpoint naming and summaries so generated API docs stay accurate.
