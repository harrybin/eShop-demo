---
description: "Use when the user asks for a review of code changes. Enforces findings-first output, severity ordering, precise file and line citations, behavioral risk emphasis, open questions, and residual risk reporting when no findings are found. Keywords: code review, severity, findings, risk, line references, testing gaps."
name: "Severity-First Code Review"
---

# Severity-First Code Review

- Must present findings first, ordered by severity from highest to lowest.
- Must prioritize defects, regressions, security issues, and missing validation over style feedback.
- Must include a precise file and line citation for every finding.
- Must describe the behavioral impact of each finding, not only the code smell.
- Must explain failure conditions and likely runtime consequences.
- Must keep each finding actionable and specific.
- Must avoid burying critical issues inside long summaries.
- Must include open questions or assumptions after findings when context is missing.
- Must include a brief change-summary only after findings and questions.
- Must explicitly state no findings when none are detected.
- Must still report residual risk or testing gaps when no findings are detected.
- Must avoid speculative claims that are not supported by observable code paths.
