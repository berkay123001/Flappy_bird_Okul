# Phase 0 Research – Web tabanlı Flappy Bird klonu

## requestAnimationFrame ve JS Interop döngüsü
- **Karar**: Oyun döngüsü C# tarafında `GameLoopService` içerisinde tutulacak ve `AnimationInterop.RequestAnimationFrameAsync` ile delta süresi alınacak. JavaScript tarafı kare başına tek callback tetikleyecek, C# tarafı `DateTime.UtcNow` yerine JS'ten dönen yüksek çözünürlüklü zaman damgasını kullanacak.
- **Gerekçe**: requestAnimationFrame tarayıcı tarafından optimize edildiğinden, delta hesaplamasını JS'te bırakmak frame sapmalarını azaltır. Tek merkezden yönetilen hizmet deterministik güncellemeyi kolaylaştırır.
- **Alternatifler**: `IJSInProcessRuntime` ile senkron çağrılar denenebilirdi fakat WebAssembly'de bloklama riski doğurur. `Timer` kullanımı reddedildi çünkü 60 FPS hedefini tutturmak zordur.

## Fizik sabitleri ve pipe boşluğu
- **Karar**: Yerçekimi ivmesi `9.8f` baz alınarak piksel/saniye^2 cinsinden normalize edilecek; kuşun ani sıçrama ivmesi 250 piksel/saniye olarak başlatılacak. Pipe çiftleri arasındaki dikey boşluk minimum 160 piksel, maksimum 220 piksel olacak ve sabit zorluk için aynı aralıkta kalacak.
- **Gerekçe**: Piksel tabanlı değerler testlerde kolay hesaplanır. Sabit aralık oyuncuların beklediği zorluk seviyesini korur.
- **Alternatifler**: Dinamik zorluk ayarı düşünülmedi çünkü gereksinimler sabit zorluk istiyor. Fizik değerlerini config dosyasına taşımak sonraki sürümlere bırakıldı.

## Giriş debouncing stratejisi
- **Karar**: Fare tıklaması ve boşluk tuşu olayları 120 ms "cooldown" ile sınırlandırılacak; bu süre içinde gelen ek girdiler yok sayılacak. Farklı giriş cihazları için event listener'lar tek noktada (`InputHandlerService`) toplanacak.
- **Gerekçe**: Çok hızlı ardışık girişler fizik motorunu aşırı tetiklemesini engeller. Tek servis debouncing uygulayarak test edilebilirliği artırır.
- **Alternatifler**: Girdiyi fizik motorunda filtrelemek düşünüldü ancak bileşen sorumluluklarını karıştırırdı. Daha kısa cooldown yüksek frekanslı zıplamalara izin verip oyunu kolaylaştıracaktı.

## LocalStorage yüksek skor saklama formatı
- **Karar**: LocalStorage key'i `flappyBird.highScore` olacak ve değer `int` olarak saklanacak. İlk açılışta değer bulunamazsa `0` kabul edilecek. Güncellemeler `StorageInterop.SetAsync` ile yapılacak.
- **Gerekçe**: Basit anahtar/değer yaklaşımı hem JS hem C# tarafında hızlı. Versiyonlamaya ihtiyaç yok.
- **Alternatifler**: JSON obje veya IndexedDB düşünülmedi; gereksiz karmaşıklık katardı. Cookie kullanımı gizlilik gerekçeleriyle tercih edilmedi.

## Asset pipeline ve performans gözlemleri
- **Karar**: Proje kökündeki `Assets` klasörü build öncesinde `wwwroot/assets` altına kopyalanacak. Ses dosyaları `.mp3` tercih edilecek (kısa süreli efektler), görseller `.png` olarak sıkıştırılacak. İlk yükleme sırasında `AssetPreloader` bileşeni texture ve sesleri tarayıcı önbelleğine alacak.
- **Gerekçe**: Tek kaynak yolu asset bütünlüğünü korur, uzun vadede lisans takibini kolaylaştırır. Ön yükleme, oyunun ilk karede takılmasını önler.
- **Alternatifler**: CDN veya lazy-load stratejileri başlangıç sürümünde ertelendi. Arka plan müziği istenmediği için ek bant genişliği maliyeti yok.

## Test ve gözlemlenebilirlik gereksinimleri
- **Karar**: Fizik motoru seedlenebilir `Random` kullanacak; testler `FixedDelta` ile çalışacak. Debug overlay fps, kare süresi ve son çarpışma pozisyonunu gösterecek.
- **Gerekçe**: Deterministik sonuçlar olmadan regresyon testleri anlamsız olur. Overlay, performans düşüşlerini sahada yakalamayı kolaylaştırır.
- **Alternatifler**: Tam profiler entegrasyonu ileride değerlendirilecek; başlangıç için hafif overlay yeterli.
