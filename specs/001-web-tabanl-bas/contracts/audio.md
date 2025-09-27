# Audio Interop Contract

## Purpose
Oyun içi ses efektlerini (`flap`, `score`, `hit`) tarayıcı üzerinden çalmak ve tekrar kullanılabilir ses kuyruğu oluşturmak.

## JavaScript API
```ts
interface AudioInterop {
  preloadAssets(manifest: AudioAssetManifest): Promise<void>;
  playEffect(effectName: "flap" | "score" | "hit"): void;
  stopAll(): void;
}

interface AudioAssetManifest {
  root: string; // "wwwroot/assets"
  files: Record<string, string>; // { flap: "flap.mp3", ... }
}
```

## C# Expectations
- `AudioService.InitializeAsync()` manifesti LocalStorage ya da config yerine sabit sözlükten üretir.
- `AudioService.PlayAsync(AudioEffect effect)` C# enum'unu JS tarafına string olarak geçirir.
- Her çağrı tek karede idempotent olmalı; ardışık aynı efekt çağrısı overlay loguna yazılmalı.

## Error Handling
- Eksik dosya ismi durumunda JS `AudioMissingError` fırlatır; C# bunu yakalayıp debug overlay'e yazar.
- Tarayıcı otomatik oynatmayı engellerse C# kullanıcıya "Tıklayarak sesi etkinleştir" mesajı gösterir.

## Tests (to be created)
- `AudioInteropTests.PreloadSetsAllFiles`
- `AudioInteropTests.PlayEffectRoutesEnum`
- `AudioInteropTests.MissingAssetRaisesError`
