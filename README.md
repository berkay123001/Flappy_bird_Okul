# Flappy Bird C# (Blazor WebAssembly)

Klasik Flappy Bird oyununu [.NET 8](https://dotnet.microsoft.com/) ve [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) kullanarak tarayıcı üzerinde yeniden yorumlayan okul projesi. Deterministik fizik motoru, dinamik engel üretimi ve duyarlı HUD bileşenleri ile tüm oyun döngüsü WebAssembly tarafında çalışır.

## ✨ Özellikler

- **Gerçek zamanlı oyun döngüsü:** `requestAnimationFrame` üzerinde çalışan yapı `AnimationInterop` ve `GameLoopService` tarafından yönetilir.
- **Deterministik fizik:** `PhysicsEngine` sınıfı sabit yerçekimi ve zıplama ivmesi ile kuş hareketini hesaplar.
- **Engel üretimi ve skor:** `ObstacleSpawner` ve `ScoreService`, RNG sağlayıcısı üzerinden skor tablolarını günceller, yüksek skor hafızada tutulur.
- **Blazor bileşenleri:** `GameBoard`, `HudPanel` gibi Razor bileşenleri oyun sahnesi, HUD ve kaplamaları render eder.
- **JS interop katmanı:** `AudioInterop`, `StorageInterop` ve `AnimationInterop` JavaScript ile .NET arasındaki köprüyü kurar.
- **Test kapsamı:** Fizik, giriş kuyruğu, ses ve depolama servisleri xUnit ile test edilir.
- **GitHub Pages dağıtımı:** `.github/workflows/deploy-pages.yml` iş akışı projeyi otomatik olarak statik siteye dönüştürür.

## 🧩 Mimari Genel Bakış

```
FlappyBirdCSharp/
├─ src/Client/               # Blazor WASM istemci uygulaması
│  ├─ Components/            # Razor bileşenleri (GameBoard, HUD, ortak UI)
│  ├─ Services/              # Oyun döngüsü, fizik, ses, depolama, skor servisleri
│  ├─ Interop/               # Animasyon, ses, depolama için JS interop sınıfları
│  └─ wwwroot/               # Statik varlıklar (sprite, ses, derleme çıktıları)
├─ tests/                    # Otomatik test projeleri
│  ├─ Client.Tests/          # Oyun mekaniği, fizik, interop vb. birim testleri
│  ├─ Client.Integration/    # Oyun akışı senaryoları için (şu an WIP) entegrasyon testi
│  └─ Client.Performance/    # Kısadevre performans ve frame bütçesi kontrolleri
├─ docs/                     # Tasarım notları ve ders niteliğinde rehberler
└─ .github/workflows/        # GitHub Pages dağıtım iş akışı
```

## 📦 Bağımlılıklar

`src/Client/Client.csproj` projesinde kullanılan temel NuGet paketleri:

| Paket | Sürüm | Açıklama |
| --- | --- | --- |
| `Microsoft.AspNetCore.Components.WebAssembly` | 8.0.20 | Blazor WebAssembly çalışma zamanı |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | 8.0.20 | Yerel geliştirme sunucusu ve hot reload |
| `Microsoft.AspNetCore.Components.Analyzers` | 8.0.20 | Blazor kod kalitesi uyarıları |
| `Microsoft.NET.ILLink.Tasks` *(otomatik)* | 8.0.20 | Publish sırasında trimmer/linker desteği |
| `Microsoft.NET.Sdk.WebAssembly.Pack` *(otomatik)* | 8.0.20 | WebAssembly SDK hedefleri |
| `Roslynator.Analyzers` | 4.8.0 | Ek Roslyn kod analizörleri |

> ℹ️ **Sprite ve ses dosyaları** [samuelcust/flappy-bird-assets](https://github.com/samuelcust/flappy-bird-assets) deposundan alınmıştır. İlgili lisans ve kredi bilgileri `src/Client/wwwroot/assets/README.md` dosyasında ayrıca yer alır.

## 🚀 Başlangıç

1. **Gereksinimler**
   - [.NET SDK 8.0](https://dotnet.microsoft.com/download)
   - (İsteğe bağlı) Node.js – ilave JS araçları eklemek isterseniz

2. **Bağımlılıkları yükleyin**
   ```bash
   dotnet restore
   ```

3. **Oyunu yerelde çalıştırın**
   ```bash
   dotnet watch --project src/Client/Client.csproj run
   ```
   Blazor geliştirme sunucusu varsayılan olarak `https://localhost:5001` adresinde açılır. Oyun alanına tıklayarak veya boşluk tuşuna basarak kuşu uçurmaya başlayabilirsiniz.

4. **Statik barındırma için yayınlayın**
   ```bash
   dotnet publish src/Client/Client.csproj -c Release -o publish --nologo /p:BasePath=/Flappy_bird_Okul
   ```
   GitHub Pages iş akışı bu adımı otomatikleştirir; `<base href>` değerini repo adına göre günceller ve SPA yönlendirmesi için `404.html` üretir.

## ✅ Testler

Aşağıdaki komutla tüm test katmanlarını tek seferde çalıştırabilirsiniz:

```bash

```

- `tests/Client.Tests` → Fizik, RNG, interop ve servislerin birim testleri
- `tests/Client.Performance` → Oyun döngüsünün kare bütçesini kontrol eden testler
- `tests/Client.Integration` → Oyun akışı senaryoları için hazırlanan (şimdilik tamamlanmamış) entegrasyon seti

## 🛠️ Katkı Rehberi

1. Depoyu forklayın, özellik dalınızı oluşturun.
2. Yeni oyun mekaniği eklerken birim testlerini güncellediğinizden emin olun.
3. PR açmadan önce `dotnet test` çıktısının yeşil olduğundan emin olun.
4. Yeni sprite veya ses dosyaları ekliyorsanız `src/Client/wwwroot/assets/README.md` dosyasına kaynak bilgisini ekleyin ve lisans koşullarına uyun.

## 📄 Lisans ve Teşekkür

- Oyun kaynak kodu: MIT Lisansı (repo kökünde).
- Sprite ve ses dosyaları: [samuelcust/flappy-bird-assets](https://github.com/samuelcust/flappy-bird-assets) projesinin lisans koşulları; ayrıntılar `src/Client/wwwroot/assets/` klasöründe.
- Proje, .NET 8, Blazor WebAssembly ve xUnit ekosistemi kullanılarak geliştirildi.

Keyifli uçuşlar! 🐦💨
Enjoy flying! 🐦💨
