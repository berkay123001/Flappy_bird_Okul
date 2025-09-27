# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Blazor WebAssembly**: `src/Client/` for Razor components, `src/Client/Services/` for gameplay services, `src/Shared/` for shared models, `wwwroot/assets/` for static files.
- **Tests**: `tests/Client.Tests/` for unit tests, `tests/Client.Integration/` for browser-simulated flows, `tests/Client.Performance/` for frame-time monitoring.
- Always sync the concrete layout with `plan.md`; extend or modify paths when the plan chooses an alternative structure.

## Phase 3.1: Setup
- [ ] T001 Validate solution/projects referenced in `plan.md` and ensure the Blazor WASM client is added to the workspace.
- [ ] T002 Configure `.editorconfig`, Roslyn analyzers, and formatting hooks mandated by the constitution.
- [ ] T003 [P] Sync `/Assets` references (optimize/compress if required) and document any new licenses.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T004 [P] Deterministic physics step test in `tests/Client.Tests/Physics/PhysicsEngineTests.cs`.
- [ ] T005 [P] Obstacle spawn sequencing test in `tests/Client.Tests/Gameplay/ObstacleSpawnerTests.cs`.
- [ ] T006 [P] Integration test covering scoring flow in `tests/Client.Integration/Scoring/ScoringFlowTests.cs`.
- [ ] T007 [P] Performance harness capturing frame time metrics in `tests/Client.Performance/FrameBudgetTests.cs`.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T008 [P] Update `GameLoopService` (or equivalent) in `src/Client/Services/` to satisfy physics expectations.
- [ ] T009 [P] Adjust obstacle generator logic in `src/Client/Services/ObstacleSpawner.cs` using deterministic sequencing.
- [ ] T010 [P] Wire component state updates in `src/Client/Components/Bird/BirdComponent.razor.cs` to new service APIs.
- [ ] T011 Implement scoring updates in `src/Client/Services/ScoreService.cs` with observable events.
- [ ] T012 Persist performance diagnostics hooks (e.g., overlay toggles) in `src/Client/Components/Hud/DebugOverlay.razor`.
- [ ] T013 Validate DI registrations and configuration in `Program.cs` / `MauiProgram.cs` to expose new services.
- [ ] T014 Capture structured logs for gameplay-critical transitions.

## Phase 3.4: Integration
- [ ] T015 Connect new services to Razor components and ensure shared models reflect updated state.
- [ ] T016 Verify input handling across desktop, keyboard, and touch interactions with manual harness scripts.
- [ ] T017 Update asset manifests and pre-load configuration in `wwwroot/assets-manifest.json` (if applicable).
- [ ] T018 Run cross-browser checks (Chromium + Firefox) and log differences.

## Phase 3.5: Polish
- [ ] T019 [P] Add regression tests for scoring edge cases (wrap-around, max points) in `tests/Client.Tests/Gameplay/ScoreServiceTests.cs`.
- [ ] T020 Record and store performance snapshot (frame time, memory, bundle size) in `/docs/performance.md`.
- [ ] T021 [P] Update feature quickstart and manual QA checklist.
- [ ] T022 Remove duplicate physics or collision logic identified during review.
- [ ] T023 Execute manual smoke test (startup latency, input responsiveness, collision accuracy) and log results.

## Dependencies
- Tests (T004–T007) must fail before implementation tasks (T008–T014) proceed.
- T008 unlocks T009 and downstream integration tasks.
- T010 and T011 must land before T015 finalizes UI bindings.
- Performance harness (T007) must exist before recording snapshots (T020).
- Polish tasks (T019–T023) execute after implementation is merged.

## Parallel Example
```
# Launch T004–T007 together:
Task: "Deterministic physics test in tests/Client.Tests/Physics/PhysicsEngineTests.cs"
Task: "Obstacle spawn sequencing test in tests/Client.Tests/Gameplay/ObstacleSpawnerTests.cs"
Task: "Scoring flow integration test in tests/Client.Integration/Scoring/ScoringFlowTests.cs"
Task: "Frame budget harness in tests/Client.Performance/FrameBudgetTests.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts/Design Docs**:
   - Each gameplay service or API signature → corresponding test + implementation tasks.
   - New diagnostics or overlays → observability tasks.
  
2. **From Game Models**:
   - Each entity (bird, obstacle, parallax layer, etc.) → component/service update tasks.
   - Shared physics rules → deterministic helper and validation tasks.
  
3. **From User Stories**:
   - Each player interaction → integration test [P].
   - Quickstart scenarios → manual QA checklist updates and documentation tasks.

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task