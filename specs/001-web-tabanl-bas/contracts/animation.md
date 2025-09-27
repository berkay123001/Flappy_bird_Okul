# Animation Interop Contract

## Purpose
Tarayıcıdaki `requestAnimationFrame` API'sini kullanarak Blazor oyun döngüsünü deterministik şekilde yönetmek.

## JavaScript API
```ts
interface AnimationInterop {
  startLoop(dotnetObjRef: DotNetObject, callbackMethod: string): number;
  stopLoop(loopId: number): void;
}
```
- `startLoop` aynı anda yalnızca bir aktif loop döndürmeli; yeni çağrı eski döngüyü kapatmalı.
- Dönen `loopId` C# tarafında saklanarak `stopLoop` çağrılarında kullanılacak.
- JS tarafı her karede `DotNet.invokeMethodAsync(callbackMethod, timestamp)` tetikleyecek.

## C# Expectations
```csharp
Task OnAnimationFrame(double timestamp);
```
- `timestamp` JS yüksek çözünürlüklü zaman damgasıdır (ms).
- C# tarafı ardışık zaman damgalarından delta hesaplar ve `GameLoopService.UpdateAsync(delta)` çağırır.

## Error Handling
- Eğer JS `startLoop` başarısız olursa promise red sebebi `AnimationInteropError` olarak dönmeli.
- C# tarafı hata aldığında fallback olarak loop'u durdurup kullanıcıya oyun başlatılamadı uyarısı gösterir.

## Tests (to be created)
- `AnimationInteropTests.StartLoopRegistersCallback`
- `AnimationInteropTests.StopLoopCancelsRequest`
- `AnimationInteropTests.DeltaComputationIsPositive`
