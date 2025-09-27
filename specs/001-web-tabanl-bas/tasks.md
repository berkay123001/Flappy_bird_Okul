# Tasks: Web tabanlı Flappy Bird klonu

**Input**: Design documents from `/specs/001-web-tabanl-bas/`
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
   → quickstart.md: Extract scenarios → integration & polish tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting, asset pipeline
   → Tests: contract tests, unit tests, integration/performance harness
   → Core: models, services, interop wrappers, components
   → Integration: DI wiring, JS module registration, asset deployment
   → Polish: regression tests, documentation, performance snapshots
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have model tasks?
   → All core flows covered by integration tests?
9. Return: SUCCESS (tasks ready for execution)
```

## Path Conventions
- **Blazor WebAssembly**: `src/Client/` for Razor components, services, and interop helpers; `src/Client/wwwroot/assets/` for static files copied from repository `Assets`.
- **Tests**: `tests/Client.Tests/` for unit tests, `tests/Client.Integration/` for browser-style scenarios, `tests/Client.Performance/` for frame budget checks.
- JS interop modules live under `src/Client/wwwroot/js/` and are imported via `IJSRuntime`.

## Phase 3.1: Setup
- [x] T001 Doğrula: `src/Client/Client.csproj` Blazor WASM projesi mevcut mu, yoksa oluştur; çözümü güncelle ve proje referanslarını düzenle.
- [x] T002 `.editorconfig`, Roslyn analizörleri ve `Directory.Build.props` dosyasında anayasa gerektirdiği kod stili/uyarıları etkinleştir.
- [x] T003 [P] Build öncesi `/Assets` içeriğini `src/Client/wwwroot/assets` altına kopyalayan MSBuild hedefini ve asset lisans belgelerini ekle.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [x] T004 [P] `tests/Client.Tests/Interop/AnimationInteropTests.cs` içinde requestAnimationFrame döngüsü için sözleşme testlerini yaz.
- [ ] T005 [P] `tests/Client.Tests/Interop/AudioInteropTests.cs` dosyasında ses preload/çalma sözleşmesini test et.
- [ ] T006 [P] `tests/Client.Tests/Interop/StorageInteropTests.cs` dosyasında LocalStorage get/set davranışını doğrula.
- [ ] T007 [P] `tests/Client.Tests/Services/RngProviderTests.cs` dosyasında seedlenebilir RNG sekanslarını test et.
- [ ] T008 [P] `tests/Client.Tests/Physics/PhysicsEngineTests.cs` dosyasında delta tabanlı hız/güncelleme hesaplarını test et.
- [ ] T009 [P] `tests/Client.Tests/Gameplay/ObstacleSpawnerTests.cs` dosyasında pipe boşluğu ve hız aralıklarını test et.
- [ ] T010 [P] `tests/Client.Tests/Gameplay/ScoreServiceTests.cs` dosyasında skor/HighScore güncellemelerini ve LocalStorage çağrılarını test et.
- [ ] T011 [P] `tests/Client.Tests/Input/InputHandlerTests.cs` dosyasında giriş debouncing ve multi-device olay haritalamasını test et.
- [ ] T012 [P] `tests/Client.Integration/GameFlow/GameFlowTests.cs` içinde temel kullanıcı hikâyesini (başlat, skor al, game over, restart) otomasyona dök.
- [ ] T013 [P] `tests/Client.Performance/FrameBudget/FrameBudgetTests.cs` ile frame-time ölçümlerinin 16.7 ms sınırını koruduğunu doğrula.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T014 [P] `src/Client/Models/BirdState.cs` dosyasında kuş durumu modelini oluştur (konum, hız, bayraklar).
- [ ] T015 [P] `src/Client/Models/PipePair.cs` dosyasında boru çifti modelini tanımla.
- [ ] T016 [P] `src/Client/Models/GameState.cs` dosyasında oyun durumu ve seed alanını uygula.
- [ ] T017 [P] `src/Client/Models/InputQueue.cs` dosyasında giriş kuyruğu veri yapısını oluştur.
- [ ] T018 [P] `src/Client/Models/ScoreSnapshot.cs` dosyasında skor kaydı modelini ekle.
- [ ] T019 `src/Client/Services/Physics/RngProvider.cs` dosyasında `IRngProvider` ve deterministik RNG sağlayıcısını uygula.
- [ ] T020 `src/Client/Services/Physics/PhysicsEngine.cs` dosyasında delta tabanlı fizik hesapları ve çarpışma kontrollerini yaz.
- [ ] T021 `src/Client/Services/ObstacleSpawner.cs` dosyasında RNG tabanlı pipe üretimini uygula.
- [ ] T022 `src/Client/Services/ScoreService.cs` dosyasında skor artırma, high score kıyaslama ve event yayınlarını uygula.
- [ ] T023 `src/Client/Services/StorageService.cs` dosyasında LocalStorage get/set sarmalayıcısını yaz ve hata senaryosu fallback'i ekle.
- [ ] T024 `src/Client/Services/AudioService.cs` dosyasında efekt çalma ve engellenen autoplay durumlarını yönet.
- [ ] T025 `src/Client/Interop/AnimationInterop.cs` dosyasında JS modül çağrılarını kapsülleyen C# sınıfını uygula.
- [ ] T026 `src/Client/Interop/AudioInterop.cs` dosyasında ses JS çağrılarını kapsülleyen sınıfı uygula.
- [ ] T027 `src/Client/Interop/StorageInterop.cs` dosyasında LocalStorage JS çağrılarını kapsülleyen sınıfı uygula.
- [ ] T028 `src/Client/Services/InputHandlerService.cs` dosyasında keyboard/mouse/touch olayları için debouncing mantığını uygula.
- [ ] T029 `src/Client/Diagnostics/DebugOverlayService.cs` dosyasında FPS, frame-time ve çarpışma metriklerini izleyen servisi yaz.
- [ ] T030 `src/Client/Services/GameLoopService.cs` dosyasında requestAnimationFrame döngüsünü yönet, delta hesapla ve servisleri orkestre et.
- [ ] T031 `src/Client/Components/Game/GameBoard.razor(.cs)` bileşeninde render, servis abonelikleri ve çarpışma tetiklemelerini uygula.
- [ ] T032 `src/Client/Components/Hud/HudPanel.razor(.cs)` bileşeninde skor/high score gösterimini ve restart kontrollerini ekle.
- [ ] T033 `src/Client/Components/Shared/AssetPreloader.razor` bileşeninde asset preload sürecini ve hata loglamasını uygula.
- [ ] T034 `src/Client/wwwroot/js/animation.js` dosyasında requestAnimationFrame JS modülünü oluştur.
- [ ] T035 `src/Client/wwwroot/js/audio.js` dosyasında ses efektlerini yöneten JS modülünü oluştur.
- [ ] T036 `src/Client/wwwroot/js/storage.js` dosyasında LocalStorage yardımcı fonksiyonlarını oluştur.

## Phase 3.4: Integration
- [ ] T037 `src/Client/Program.cs` dosyasında tüm servisleri, interop sınıflarını ve overlay'i DI konteynerine kaydet.
- [ ] T038 `src/Client/Client.csproj` ve `wwwroot/index.html` dosyalarında JS modül referanslarını ekle ve asset kopyalama hedefini doğrula.
- [ ] T039 `src/Client/wwwroot/assets-manifest.json` (veya eşdeğer) dosyasını oluşturarak asset preload manifestini tanımla.
- [ ] T040 Playwright tabanlı `tests/Client.Integration/GameFlow/GameFlowTests.cs` senaryosuna tarayıcı profilleri (Chromium/Firefox) ekleyip giriş cihazı varyasyonlarını kapsa.

## Phase 3.5: Polish
- [ ] T041 `tests/Client.Tests/Gameplay/ScoreServiceTests.cs` dosyasında edge-case regresyon testlerini (maks skor, ardışık skor) ekle.
- [ ] T042 Performans snapshotını (`frame time`, bellek, bundle boyutu) `docs/performance.md` içine kaydet ve önceki değerlerle kıyasla.
- [ ] T043 [P] `specs/001-web-tabanl-bas/quickstart.md` ve manuel QA notlarını güncelleyip LocalStorage reset adımlarını ekle.
- [ ] T044 Kod inceleme turu sonrası tekrar eden fizik/giriş mantığını refaktör ederek sadeleştir (`src/Client/Services/Physics` ve `InputHandlerService`).
- [ ] T045 Quickstart adımlarını takip ederek manuel smoke test raporu oluştur ve depo kökündeki `manual-testing.md` dosyasına ekle.

## Dependencies
- T001–T003 tamamlanmadan sonraki adımlara geçme.
- T004–T013 testleri yazılıp kırmızıya düşmeden T014 sonrası uygulama görevlerine başlanamaz.
- T019 T020'den önce gereklidir; T020 aynı zamanda T030 için ön koşuldur.
- T021 ve T022 tamamlanmadan T031 bağımlı bileşen mantığı bitmiş sayılmaz.
- T023 ve T027 bitmeden high score entegrasyonu (T032) kapatılamaz.
- T030 tamamlanmadan T037 DI ve T038 yapılandırma adımlarına geçme.
- T033 ve T039 asset hazırlığı bitmeden T042 performans ölçümleri sağlıksız olur.
- Polish aşaması (T041–T045) tüm çekirdek ve entegrasyon adımları tamamlandıktan sonra yürütülmelidir.

## Parallel Execution Example
```
# Test dalgasını paralel yürüt:
/specs/001-web-tabanl-bas/tasks run T004
/specs/001-web-tabanl-bas/tasks run T005
/specs/001-web-tabanl-bas/tasks run T006
/specs/001-web-tabanl-bas/tasks run T007
```
- Aynı anda farklı dosyaları hedefleyen [P] işlerini tetikleyin; CI kaynaklarını aşırı kullanmayın.

## Notes
- [P] işaretli görevler farklı dosyalarda çalıştığından paralel yürütülebilir.
- TDD sırasına uyun: önce testler kırmızıya düşmeli.
- Her görevin sonunda commit yapın ve CI'yi koşturun.
- Asset lisanslarını korumayı ve debug overlay'i etkin kılmayı unutmayın.

## Validation Checklist
- [x] Tüm sözleşmeler için test görevleri eklendi.
- [x] Tüm modeller için görevler planlandı.
- [x] Tüm testler uygulama işlerinden önce geliyor.
- [x] [P] görevler aynı dosyayı paylaşmıyor.
- [x] Her görev kesin bir dosya yolu belirtiyor.
```}