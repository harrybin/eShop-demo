# Story 7.2: Stabilize Catalog functional tests and restore green CI baseline

Status: ready-for-dev

## Story

As a contributor,
I want Catalog functional tests to run reliably in CI,
so that pull requests have trustworthy quality gates and regressions are caught early.

## Acceptance Criteria

1. Catalog.FunctionalTests pass consistently in local and CI execution.
2. Test fixture dependencies are deterministic (image tags pinned, startup sequencing reliable).
3. Flaky setup behavior is eliminated through explicit readiness checks and timeouts.
4. CI workflow reports Catalog functional test result as part of standard validation without intermittent failures.
5. Documentation notes known test prerequisites and troubleshooting for contributors.

## Tasks / Subtasks

- [ ] Reproduce and isolate current Catalog.FunctionalTests failures in CI-like conditions (AC: 1)
- [ ] Harden fixture startup and dependency readiness checks (AC: 2, 3)
  - [ ] Confirm container image tags and health checks are deterministic
- [ ] Update tests and/or fixture config to remove race conditions (AC: 1, 3)
- [ ] Validate CI workflow integration and failure reporting (AC: 4)
- [ ] Update contributor troubleshooting notes for Catalog functional test execution (AC: 5)

## Dev Notes

- Brownfield focus: PRD technical success criteria explicitly calls out green CI including Catalog.FunctionalTests.
- Prefer targeted fixes to fixture/test orchestration, not broad architecture changes.
- Keep test project conventions aligned with existing functional test suites.

### Project Structure Notes

- Test project: tests/Catalog.FunctionalTests
- Related workflow/config: .github/workflows/pr-validation.yml, possibly test infra settings
- Supporting docs: docs/LOCAL_SETUP.md or tests/README.md if needed

### References

- Source: _bmad-output/planning-artifacts/prd.md (technical success criteria mentions Catalog.FunctionalTests)
- Source: _bmad-output/planning-artifacts/architecture.md (test reliability, fixture pattern preservation)
- Source: IMPROVEMENTS.md (remaining quality-gap signal and fixture reliability context)

## Dev Agent Record

### Agent Model Used

GPT-5.3-Codex

### Debug Log References

- n/a

### Completion Notes List

- Story context generated from brownfield gap analysis of current implementation state.

### File List

- tests/Catalog.FunctionalTests/**
- .github/workflows/pr-validation.yml
- docs/LOCAL_SETUP.md
- tests/README.md
