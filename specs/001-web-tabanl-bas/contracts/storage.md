# Storage Interop Contract

## Purpose
LocalStorage üzerinden yüksek skor değerini güvenilir şekilde saklamak ve okumak.

## JavaScript API
```ts
interface StorageInterop {
  getNumber(key: string, defaultValue: number): number;
  setNumber(key: string, value: number): void;
  remove(key: string): void;
}
```
- `key` olarak `flappyBird.highScore` kullanılacak.
- `getNumber` sayı olmayan değerle karşılaşırsa `defaultValue` döndürüp veriyi temizleyecek.

## C# Expectations
- `StorageService.GetHighScoreAsync()` LocalStorage'ı sorgular ve negatif değerleri sıfırlar.
- `StorageService.SetHighScoreAsync(int score)` sadece yeni skor önceki değerden büyükse günceller.
- Serileştirme yapılmaz; değerler `int` olarak kalır.

## Error Handling
- LocalStorage desteklenmiyorsa JS tarafı `StorageUnavailableError` fırlatır. C# tarafı session bazlı `FallbackHighScoreProvider` ile devam eder.

## Tests (to be created)
- `StorageInteropTests.GetReturnsDefaultWhenMissing`
- `StorageInteropTests.SetPersistsValue`
- `StorageInteropTests.StorageUnavailableTriggersFallback`
