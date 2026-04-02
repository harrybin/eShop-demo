---
description: "Use when the user references an attached prompt file or asks to follow a named workflow. Covers required context loading, progress updates, and complete execution from discovery to file edits. Keywords: follow instructions file, attached prompt, iterate, clarify, draft and save."
name: "Prompt Workflow Execution"
---

# Prompt Workflow Execution

- Must load and follow any prompt file explicitly referenced by the user before making edits.
- Must extract concrete rules from the current conversation and turn them into concise, reusable guidance.
- If rules are ambiguous, must ask targeted clarification questions after saving an initial draft.
- Must use short progress updates during tool usage and state what will happen next.
- Before each batch of tool calls, must include one sentence that states why the batch is being run and the expected outcome.
- After read-only discovery, must provide a concise progress summary and the next action.
- Must complete the full loop in one turn when feasible: discover, draft, save, identify ambiguities, and request final refinements.
- Must keep instructions focused on one concern per file and avoid mixing unrelated coding standards.
