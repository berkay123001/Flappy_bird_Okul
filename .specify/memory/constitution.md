<!--
Sync Impact Report
- Version change: N/A → 1.0.0
- Modified principles: None (initial adoption)
- Added sections: Core Principles, Technical Standards, Development Workflow & Quality Gates, Governance
- Removed sections: None
- Templates requiring updates:
	- ✅ .specify/templates/plan-template.md
	- ✅ .specify/templates/spec-template.md
	- ✅ .specify/templates/tasks-template.md
- Follow-up TODOs: None
-->

# FlappyBirdCSharp Constitution

## Core Principles

### P1. Clean, Modular Game Architecture
- MUST implement gameplay logic inside C# partial classes or dedicated services, keeping Blazor component markup declarative.
- MUST keep each component focused on a single responsibility and share logic through dependency-injected services or clearly named utility classes.
- SHOULD document any non-trivial public method with XML comments to aid reuse.
*Rationale*: Modular code is faster to review, easier to test, and keeps the project maintainable as mechanics evolve.

### P2. Deterministic Game Loop & Physics
- MUST base all game state updates on a time delta sourced from the browser requestAnimationFrame loop to prevent frame-dependent behavior.
- MUST centralize physics rules (gravity, jump impulse, collision detection) in a single engine module that exposes pure functions for validation.
- MUST provide deterministic simulation helpers that accept seeded inputs so automated tests can assert outcomes.
*Rationale*: Deterministic rules ensure predictable gameplay and make regression testing feasible.

### P3. Performance-First WebAssembly Delivery
- MUST sustain 60 frames per second on mid-range hardware; detect drops by wiring a lightweight frame-time monitor.
- MUST avoid unnecessary allocations inside per-frame code paths and prefer struct-based calculations where possible.
- SHOULD lazy-load optional assets and audio to reduce initial download and startup cost.
*Rationale*: Flappy Bird relies on responsive input; keeping the WASM payload and loop tight preserves playability.

### P4. Testability & Observability
- MUST create unit or integration tests for core systems (physics, scoring, obstacle spawning) before shipping substantial changes.
- MUST expose lightweight diagnostic hooks (e.g., debug overlay toggles, structured console logs) that can be activated without rebuilding.
- SHOULD capture and store performance snapshots when optimizing to show measurable gains.
*Rationale*: Observable systems highlight regressions early and give confidence during refactors.

### P5. Asset Integrity & Licensing
- MUST source all art and audio from the `/Assets/flappy-bird-assets-master` directory (or its future maintained successor) and reference them via pipeline-friendly paths.
- MUST retain original licenses and attribution files within the repository and copy notices into any public release notes.
- MUST compress or preprocess assets before inclusion in builds to avoid runtime decoding costs.
*Rationale*: Respecting asset provenance and optimization keeps the project compliant and performant.

## Technical Standards

- Stack MUST use C# with .NET 8 targeting Blazor WebAssembly; any deviation requires governance approval.
- Build pipeline MUST include `dotnet` analyzers and formatters to enforce clean code guidance.
- Browser compatibility MUST be verified on the latest Chromium and Firefox releases; document known issues for other engines.
- Runtime MUST provide responsive controls on desktop and touch devices with latency ≤ 80 ms from input to frame update.
- Persistent storage (if introduced) MUST rely on browser-friendly mechanisms such as IndexedDB or local storage with explicit sync strategies.

## Development Workflow & Quality Gates

- Every feature plan MUST include a Constitution Check summarizing compliance with principles P1–P5.
- Pull requests MUST link to failing tests demonstrating the change when touching gameplay-critical code.
- Code reviews MUST reject submissions lacking performance validation when per-frame logic is modified.
- Release candidates MUST undergo a smoke test covering: startup latency, frame stability, audio playback, and collision correctness.
- Regression tracking MUST log observed FPS, memory footprint, and bundle size deltas for each release.

## Governance

- This constitution supersedes conflicting team conventions; breaking principles requires a documented exception approved by maintainer consensus.
- Amendments MUST be proposed via pull request that updates this document, accompanies a changelog entry, and bumps the version per semantic rules.
- MINOR version bumps occur when new principles or sections are introduced without removing existing mandates; MAJOR bumps require explicit migration guidance for deprecated principles.
- Compliance reviews MUST occur at feature kickoff (plan), post-design, and prior to release freeze; violations are tracked until resolved.
- Documentation stewards MUST ensure plan/spec/tasks templates remain synchronized within five days of any amendment.

**Version**: 1.0.0 | **Ratified**: 2025-09-26 | **Last Amended**: 2025-09-26