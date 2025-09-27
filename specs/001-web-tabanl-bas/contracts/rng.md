# RNG Coordination Contract

## Purpose
`GameLoopService` ile `ObstacleSpawner` arasında deterministik pipe üretimi için ortak RNG kullanımını tanımlamak.

## Interface
```csharp
public interface IRngProvider
{
    int CurrentSeed { get; }
    void Reseed(int seed);
    float NextFloat(); // 0.0 <= value < 1.0
}
```

- `GameLoopService` her oyun başlangıcında `Reseed(Environment.TickCount)` çağırır ve seed'i `GameState.Seed` alanında saklar.
- `ObstacleSpawner` pipe yüksekliğini `NextFloat()` kullanarak belirler; testlerde sabit seed ile deterministik değerler üretilir.

## Guarantees
- Her `NextFloat` çağrısı tekrar üretilebilir olmalı; aynı seed aynı sırayı verir.
- Servis thread-safe olmak zorunda değildir (tek thread).

## Tests (to be created)
- `RngProviderTests.SameSeedSameSequence`
- `ObstacleSpawnerTests.GapWithinBounds`
- `GameLoopServiceTests.ReseedResetsSequence`
