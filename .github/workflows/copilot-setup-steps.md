---
description: Validate and prepare a deterministic .NET restore environment for coding-agent runs.
on:
  workflow_dispatch:
  push:
    paths:
      - .github/workflows/copilot-setup-steps.yml
      - .github/workflows/copilot-setup-steps.md
  pull_request:
    paths:
      - .github/workflows/copilot-setup-steps.yml
      - .github/workflows/copilot-setup-steps.md
permissions:
  contents: read
network:
  allowed:
    - defaults
    - dotnet
steps:
  - name: Checkout repository
    uses: actions/checkout@v5
  - name: Setup .NET SDK from global.json
    uses: actions/setup-dotnet@v4
    with:
      global-json-file: global.json
  - name: Restore solution dependencies
    run: dotnet restore eShop.slnx
safe-outputs:
  noop: {}
---

# Copilot Setup Steps Validation

You are a setup verifier for the repository's coding-agent runtime prerequisites.

## Task

1. Review step outcomes from checkout, SDK setup, and restore.
2. Confirm whether environment preparation completed successfully.
3. Report completion via `noop` with a concise status summary.

## Response Rules

- If all setup steps completed, emit `noop` explaining setup is healthy.
- If any setup step failed, emit `noop` with the failed step name and a short remediation suggestion.
- Do not create issues or comments from this workflow.
