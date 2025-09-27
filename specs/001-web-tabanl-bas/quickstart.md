# Quickstart – Web tabanlı Flappy Bird klonu

## Önkoşullar
- .NET 8 SDK kurulu
- Desteklenen tarayıcılar: Chromium 124+, Firefox 126+
- `Assets` klasöründeki görsel ve ses dosyalarının hazır olması

## 1. Çalışma Ortamını Hazırlama
1. `dotnet workload restore` komutuyla gerekli workload'u yükleyin (Blazor WASM).
2. `dotnet new gitignore` veya mevcut ise güncel olduğundan emin olun.
3. Build pipeline'a `wwwroot/assets` klasörünü oluşturup `Assets` kaynağından kopyalayan adımı ekleyin.

## 2. Uygulamayı Başlatma
1. `dotnet run --project src/Client/Client.csproj`
2. Tarayıcıda `https://localhost:5001` adresini açın.
3. İlk yüklemede debug overlay'in açılmadığından emin olun (isteğe bağlı `?debug=true`).

## 3. Temel Senaryoyu Doğrulama
1. Oyun yüklenince kuşun sol tarafta durduğunu ve arka planın göründüğünü kontrol edin.
2. Fare tıklayın veya boşluk tuşuna basın → kuşun hemen yukarı sıçradığını ve hızının değiştiğini gözlemleyin.
3. Pipe çiftleri sağdan geldiğinde kuş bir çifte değmeden geçtiğinde skorun +1 arttığını doğrulayın.
4. Kaza veya yere çarpma durumunda "Game Over" mesajının geldiğini ve oyunun durduğunu kontrol edin.
5. "Restart" (varsa) veya sayfayı yenileyerek oyunu tekrar başlatın.

## 4. Yüksek Skor Kalıcılığını Kontrol Etme
1. En az bir pipe geçip skor elde edin ve oyunu bitirin.
2. Tarayıcıyı yenileyin; HUD üzerinde en yüksek skorun korunduğunu doğrulayın.
3. Daha yüksek bir skor elde edip LocalStorage değerinin güncellendiğini teyit edin.

## 5. Ses ve Performans Testi
1. Zıplama sırasında `flap`, skor alınca `score`, çarpışmada `hit` seslerinin çaldığını doğrulayın.
2. Debug overlay'i açın ve frame-time değerlerinin 16.7 ms çevresinde seyrettiğini kontrol edin.
3. FPS düşüşü veya ses hatası olduğunda konsol loglarını inceleyin.

## 6. Temizlik
- LocalStorage kaydını sıfırlamak için debug overlay veya `StorageService` kullanın.
- `dotnet test` komutunu çalıştırarak fizik ve interop testlerinin başarısız durumda olduğunu (TDD başlangıcı) doğrulayın.
