---
description: "Use when validating code changes before completion in eShop. Covers impacted test selection, targeted-first execution, risk-based expansion, failure handling, and transparent reporting of what was or was not run. Keywords: run tests, unit tests, functional tests, validation, skipped tests, test failures."
name: "Test Validation Workflow"
---

# Test Validation Workflow

- Must identify impacted services and map them to the relevant test projects first.
- Must run targeted tests for the changed area before broad test suites.
- Must escalate from targeted tests to broader suites when change risk is medium or high.
- Must treat shared infrastructure or cross-service changes as high risk.
- Must run related unit tests for logic changes even when functional tests are out of scope.
- Must run related functional tests when behavior crosses API or integration boundaries.
- Must state environmental prerequisites when required for functional tests.
- Must never claim tests were executed if they were not run.
- Must report exact commands or test targets and their result status.
- Must list what was skipped or not run and provide a concrete reason.
- Must treat failing tests as blockers unless an explicit exception is requested.
- Must document whether failures are introduced by the change or pre-existing.
- Must provide a clear next step whenever testing cannot be completed.
