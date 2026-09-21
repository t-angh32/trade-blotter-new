# Agent Session Guidelines & Project Memory Protocol

## Mandatory Session Initialization Protocol
1. **Always Check Memory**: At the start of every session (or whenever work resumes), immediately open and inspect [`.\docs\memory\MEMORY.md`](./docs/memory/MEMORY.md).
2. **Review Current Progress**: Understand the project state, active tasks, architectural decisions, and current roadmap items in `MEMORY.md`.
3. **Continuous Updates**: As work progresses, append key milestones, completed tasks, schema choices, and progress notes to `.\docs\memory\MEMORY.md`.

## Active Project Skills
The OpenSpec skills located in [`.\docs\openspec\.agents\skills`](./docs/openspec/.agents/skills) are permanently configured via [`.\.agents\skills.json`](./.agents/skills.json):
- `openspec-apply-change`: Apply proposed spec changes to the codebase.
- `openspec-archive-change`: Archive completed spec changes.
- `openspec-explore`: Explore existing specs and codebase architecture.
- `openspec-propose`: Formulate and propose new OpenSpec changes.
- `openspec-sync-specs`: Synchronize spec documentation.
- `openspec-update-change`: Update draft changes and specs.
