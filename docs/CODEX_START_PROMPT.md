# Prompt to start Codex on XFramework

Paste this into Codex after opening the repository:

> You are working on XFramework. First read `AGENTS.md`, `docs/CODEX_HANDOFF.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md`, `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, and the relevant Outbox docs.
>
> Do not edit files yet. Inspect the actual repository tree, `git status`, project references, current V48 Outbox implementation, tests, and diagnostics. Compare the code to the documentation and report discrepancies, missing files, and the safest next milestone.
>
> Do not assume that a version label guarantees the source is correct. Do not introduce ABP.IO or multi-tenancy. Do not duplicate existing abstractions. Do not claim tests passed unless you run them and show the actual results.
>
> After the inspection, propose a bounded implementation plan with acceptance criteria and tests. Wait for approval before making source changes. For approved source changes, update docs and provide a concise diff/test report. Preserve the full solution and do not omit files from any delivery archive.
