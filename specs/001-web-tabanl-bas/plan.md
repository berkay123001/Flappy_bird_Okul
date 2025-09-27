
# Implementation Plan: Web tabanlı Flappy Bird klonu

**Branch**: `001-web-tabanl-bas` | **Date**: 2025-09-26 | **Spec**: `/home/berkayhsrt/FlappyBirdCSharp/specs/001-web-tabanl-bas/spec.md`
**Input**: Feature specification from `/home/berkayhsrt/FlappyBirdCSharp/specs/001-web-tabanl-bas/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Blazor WebAssembly üzerinde çalışan Flappy Bird klonunda kuşun requestAnimationFrame tabanlı oyun
döngüsüyle kontrol edilmesi, LocalStorage üzerinden yüksek skor tutulması ve tüm görsel/ses varlıklarının
`wwwroot/assets` yolundan servis edilmesi gerekir. Animasyonlar ve sesler JS Interop kullanılarak
C# tarafından tetiklenecek, kullanıcı girişi fare tıklaması veya boşluk tuşu ile yönetilecek ve 60 FPS
performans hedefi sağlanacaktır.

## Technical Context
**Language/Version**: C# (.NET 8, Blazor WebAssembly)  
**Primary Dependencies**: Blazor WebAssembly SDK, JS Interop (`IJSRuntime`), CSS-only layout  
**Storage**: Browser LocalStorage via JS Interop (no backend persistence)  
**Testing**: xUnit + bUnit for component/service tests, Playwright tabanlı smoke/perf harness  
**Target Platform**: Web browsers (Chromium & Firefox desktop/mobile) compiling to WASM
**Project Type**: Web single-project (Client WASM app)  
**Performance Goals**: Sustain 60 FPS with ≤16.7 ms frame budget; input latency ≤80 ms  
**Constraints**: No external UI libraries; assets/audio must reside in `wwwroot/assets`; requestAnimationFrame drives loop; deterministic physics  
**Scale/Scope**: Single-player arcade loop, single screen UI, limited asset set (bird, pipes, background, three SFX)

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **P1 – Clean, Modular Game Architecture**: Gameplay döngüsü `GameLoopService`, `PhysicsEngine`, `ObstacleSpawner` gibi C# servislerinde toplanacak; Razor bileşenleri yalnızca görsel durum bağlayacak.
- [x] **P2 – Deterministic Game Loop & Physics**: requestAnimationFrame delta değeri tek fizik motoru tarafından tüketilecek, kuş ve boru güncellemeleri saf fonksiyonlarla yönetilecek.
- [x] **P3 – Performance-First WebAssembly Delivery**: 60 FPS hedefi için frame-time telemetrisi, tahsis minimizasyonu ve önceden yüklenen varlıklar planlandı; ağır hesaplamalar per-frame döngüden uzak tutulacak.
- [x] **P4 – Testability & Observability**: Seedlenebilir rastgelelik, deterministik yardımcılar ve debug overlay/loglama ile testlenebilirlik sağlanacak; çekirdek sistemlere test önceliği verilecek.
- [x] **P5 – Asset Integrity & Licensing**: Tüm varlıklar `/Assets` kaynağından alınarak build aşamasında `wwwroot/assets` altına kopyalanacak; lisans dosyaları korunacak.

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
ios/ or android/
<!--
   ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
   for this feature. Delete unused options and expand the chosen structure with
   real paths (e.g., apps/admin, packages/something). The delivered plan must
   not include Option labels.
-->
```
src/
└── Client/
   ├── Components/
   │   ├── Game/
   │   │   ├── GameBoard.razor
   │   │   └── GameBoard.razor.cs
   │   ├── Hud/
   │   │   ├── HudPanel.razor
   │   │   └── HudPanel.razor.cs
   │   └── Shared/
   │       └── AssetPreloader.razor
   ├── Services/
   │   ├── GameLoopService.cs
   │   ├── Physics/
   │   │   └── PhysicsEngine.cs
   │   ├── ObstacleSpawner.cs
   │   ├── ScoreService.cs
   │   └── AudioService.cs
   ├── Interop/
   │   ├── AnimationInterop.cs
   │   └── AudioInterop.cs
   ├── Models/
   │   ├── BirdState.cs
   │   ├── PipePair.cs
   │   └── GameState.cs
   ├── Diagnostics/
   │   └── DebugOverlayService.cs
   ├── Program.cs
   └── wwwroot/
      └── assets/

tests/
├── Client.Tests/
│   ├── Physics/
│   │   └── PhysicsEngineTests.cs
│   ├── Gameplay/
│   │   ├── ObstacleSpawnerTests.cs
│   │   └── ScoreServiceTests.cs
│   └── Diagnostics/
│       └── FrameBudgetTests.cs
└── Client.Integration/
   └── GameFlowTests.cs
```

**Structure Decision**: Tek Blazor WASM istemci projesi; servisler ve modeller sorumluluklarına göre
alt klasörlere ayrıldı, testler `tests/Client.*` altında gruplanıyor.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
    - requestAnimationFrame ve C# interop latency etkileri → araştırma görevi
    - Fizik sabitleri (yerçekimi, jump impulse) ve minimum pipe boşluğu → araştırma görevi
    - Çoklu giriş (hızlı tıklama/tuş basımı) debouncing stratejileri → araştırma görevi
    - LocalStorage yüksek skor formatı ve migration stratejisi → araştırma görevi

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with tüm requestAnimationFrame/JS interop, LocalStorage persistence ve deterministik
fizik parametreleri netleştirilmiş olarak tamamlanması

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API/Interop contracts** from functional requirements:
   - JS Interop çağrıları (requestAnimationFrame başlatma/durdurma, audio play/stop, LocalStorage get/set) için TypeScript benzeri imzalar tanımla
   - `ObstacleSpawner` ile `GameLoopService` arasında deterministic RNG paylaşım protokolünü dokümante et
   - Çıktıları `/contracts/` altında Markdown dosyaları olarak sakla

3. **Generate contract tests** from contracts:
   - Her interop sözleşmesi için bir test dosyası (AnimationInteropTests, AudioInteropTests, StorageInteropTests)
   - Beklenen JS fonksiyon adları, parametreleri ve hata durumları doğrulanmalı
   - Testler başlangıçta başarısız olmalı (henüz uygulama yok)

4. **Extract test scenarios** from user stories:
   - Her kullanıcı hikâyesi → entegrasyon testi (giriş, çarpışma, skor artışı)
   - Quickstart testi = hikâye adımları + LocalStorage yüksek skor kalıcılığı doğrulaması

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add yalnızca bu planla gelen yeni teknoloji/kararlar (requestAnimationFrame interop, LocalStorage stratejisi, asset pipeline notları)
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- `.specify/templates/tasks-template.md` temel alınarak taslak oluştur
- Phase 1 çıktılarındaki her interop sözleşmesi için [P] test + uygulama görevleri üret
- BirdState, PipePair, GameState gibi her varlık için model/servis görevleri ekle
- Kullanıcı hikâyeleri ve quickstart doğrulamasından entegrasyon ve performans testleri türet
- Tüm görevler deterministik fizik, JS interop ve asset pipeline gerekliliklerini yansıtmalı

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 24-28 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `/memory/constitution.md`*
