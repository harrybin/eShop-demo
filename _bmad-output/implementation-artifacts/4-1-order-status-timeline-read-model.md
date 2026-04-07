# Story 4.1: Complete order status timeline read model and UI timeline

Status: ready-for-dev

## Story

As an authenticated shopper,
I want a complete order status timeline with persisted status history,
so that I can understand where my order is and what happens next.

## Acceptance Criteria

1. Ordering persists order status history events with timestamps and status metadata for each order transition.
2. Order details API returns timeline entries in chronological order using user-friendly status labels.
3. WebApp order details renders a timeline component that maps backend status states to consistent UI semantics.
4. Timeline behavior is deterministic for historical orders and in-flight orders with pending steps.
5. Existing order history and order details behavior remains backward compatible for fields already used by WebApp.
6. Unit and functional tests cover timeline projection/query behavior and API response contract shape.

## Tasks / Subtasks

- [ ] Introduce or finalize status history persistence model in Ordering data layer (AC: 1)
  - [ ] Add migration and update data access paths used by order status transitions
- [ ] Extend query path for order details/history with timeline entries (AC: 2, 4)
  - [ ] Normalize labels and ordering rules for lifecycle states
- [ ] Complete timeline rendering in WebApp order details surface (AC: 3)
  - [ ] Reuse shared status semantics from UX spec and avoid page-specific vocabulary drift
- [ ] Add test coverage in Ordering unit + functional test projects (AC: 6)
  - [ ] Validate empty, partial, and full lifecycle timelines

## Dev Notes

- Brownfield focus: this story completes a partially implemented capability already marked in IMPROVEMENTS.
- Keep event-driven lifecycle architecture intact (Ordering.API -> OrderProcessor/PaymentProcessor via integration events).
- Preserve existing API patterns: minimal APIs, versioned route groups, typed responses, and ProblemDetails alignment.
- Avoid introducing synchronous orchestration for status progression; timeline is a read projection of async workflow state.

### Project Structure Notes

- API/query changes: src/Ordering.API
- Domain/persistence changes: src/Ordering.Domain, src/Ordering.Infrastructure
- UI changes: src/WebApp, optionally src/WebAppComponents for reusable timeline rendering
- Tests: tests/Ordering.UnitTests, tests/Ordering.FunctionalTests

### References

- Source: _bmad-output/planning-artifacts/prd.md (FR15, FR16, FR17, FR18)
- Source: _bmad-output/planning-artifacts/architecture.md (event-driven progression, boundary ownership)
- Source: _bmad-output/planning-artifacts/ux-design-specification.md (Order Status Timeline component and state semantics)
- Source: IMPROVEMENTS.md (Feature: Add order history and tracking marked Partial)

## Dev Agent Record

### Agent Model Used

GPT-5.3-Codex

### Debug Log References

- n/a

### Completion Notes List

- Story context generated from brownfield gap analysis of current implementation state.

### File List

- src/Ordering.API/**
- src/Ordering.Domain/**
- src/Ordering.Infrastructure/**
- src/WebApp/**
- tests/Ordering.UnitTests/**
- tests/Ordering.FunctionalTests/**
