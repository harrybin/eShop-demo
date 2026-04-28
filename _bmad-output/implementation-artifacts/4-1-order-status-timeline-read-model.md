# Story 4.1: Complete order status timeline read model and UI timeline

Status: in-progress

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

- [x] Introduce or finalize status history persistence model in Ordering data layer (AC: 1)
  - [x] Add migration and update data access paths used by order status transitions
- [x] Extend query path for order details/history with timeline entries (AC: 2, 4)
  - [x] Normalize labels and ordering rules for lifecycle states
- [x] Complete timeline rendering in WebApp order details surface (AC: 3)
  - [x] Reuse shared status semantics from UX spec and avoid page-specific vocabulary drift
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

- `dotnet build src/Ordering.Infrastructure/Ordering.Infrastructure.csproj`
- `dotnet build src/Ordering.API/Ordering.API.csproj`
- `dotnet build src/WebApp/WebApp.csproj`
- `dotnet test --project tests/Ordering.UnitTests/Ordering.UnitTests.csproj --filter "FullyQualifiedName~OrderAggregateTest|FullyQualifiedName~OrdersWebApiTest"`
- `dotnet test --project tests/Ordering.UnitTests/Ordering.UnitTests.csproj`
- `dotnet test --project tests/Ordering.FunctionalTests/Ordering.FunctionalTests.csproj -- --filter-method "*OrderingApiTests.OrderingContextHasNoPendingMigrations" --filter-method "*OrderingApiTests.GetOrderReturnsPendingTimelineForInFlightOrder" --filter-method "*OrderingApiTests.GetOrderSynthesizesTimelineForHistoricalOrderWithoutHistoryRows"` (blocked by PostgreSQL timeout during host startup)
- `dotnet build eShop.slnx`

### Completion Notes List

- Added `OrderStatusHistory` domain entity and aggregate-owned status-history recording for all order lifecycle transitions.
- Added `OrderStatusHistoryEntityTypeConfiguration`, wired `DbSet<OrderStatusHistory>` into `OrderingContext`, and added migration `20260428142500_AddOrderStatusHistory`.
- Extended order-details query contract with timeline entries and deterministic fallback timeline synthesis for historical orders without persisted history rows.
- Updated Orders API query interfaces to pass cancellation tokens through all query methods.
- Updated WebApp order details models and UI to render API-provided timeline entries with shared status semantics via `OrderStatusSemantics`.
- Added/updated unit tests in Ordering UnitTests for aggregate status-history behavior and Orders API query wrappers.
- Added functional tests for timeline contract behavior, but execution is currently blocked by PostgreSQL connection timeouts in functional test host startup.

### File List

- src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs
- src/Ordering.Domain/AggregatesModel/OrderAggregate/OrderStatusHistory.cs
- src/Ordering.Infrastructure/OrderingContext.cs
- src/Ordering.Infrastructure/EntityConfigurations/OrderEntityTypeConfiguration.cs
- src/Ordering.Infrastructure/EntityConfigurations/OrderStatusHistoryEntityTypeConfiguration.cs
- src/Ordering.Infrastructure/Migrations/20260428142500_AddOrderStatusHistory.cs
- src/Ordering.API/Application/Queries/IOrderQueries.cs
- src/Ordering.API/Application/Queries/OrderQueries.cs
- src/Ordering.API/Application/Queries/OrderViewModel.cs
- src/Ordering.API/Apis/OrdersApi.cs
- src/WebApp/Services/OrderDetailsRecord.cs
- src/WebApp/Services/OrderStatusSemantics.cs
- src/WebApp/Components/Pages/User/OrderDetails.razor
- src/WebApp/Components/Pages/User/OrderDetails.razor.css
- tests/Ordering.UnitTests/Domain/OrderAggregateTest.cs
- tests/Ordering.UnitTests/Application/OrdersWebApiTest.cs
- tests/Ordering.FunctionalTests/OrderingApiTests.cs

### Change Log

- 2026-04-28: Implemented order status history persistence and timeline projection/query rendering across Ordering API and WebApp; added timeline unit/functional tests; functional execution blocked by PostgreSQL timeout in test host startup.
