# Phase 1 Data Model – Web tabanlı Flappy Bird klonu

## BirdState
| Alan | Tip | Açıklama |
|------|-----|----------|
| `PositionY` | `float` | Ekran koordinatlarında kuşun dikey konumu (piksel cinsinden). |
| `VelocityY` | `float` | Her karede kuşun düşüş/çıkış hızını belirler. |
| `IsAlive` | `bool` | Kuşun çarpışma yaşayıp yaşamadığını belirtir. |
| `LastInputTimestamp` | `double` | Son oyuncu girişinin zaman damgası (ms). |
| `PendingImpulse` | `bool` | Bir sonraki karede zıplama uygulanması gerekip gerekmediğini bildirir. |

- **Durum geçişleri**: Başlangıçta `IsAlive = true`, çarpışma gerçekleştiğinde `false` ve hız sıfırlanır. Yeni oyun başladığında durum varsayılan değerlere döner.

## PipePair
| Alan | Tip | Açıklama |
|------|-----|----------|
| `PositionX` | `float` | Pipe çiftinin yatay konumu. |
| `GapCenterY` | `float` | Borular arasındaki boşluğun merkezi. |
| `GapSize` | `float` | Boşluğun yüksekliği (160–220 px). |
| `VelocityX` | `float` | Boruların sola doğru hareket hızı. |
| `Passed` | `bool` | Kuş bu pipe çiftini geçti mi? |

- **İlişkiler**: `ObstacleSpawner` yeni `PipePair` örnekleri üretir ve `GameState.Obstacles` koleksiyonuna ekler.

## GameState
| Alan | Tip | Açıklama |
|------|-----|----------|
| `Bird` | `BirdState` | Oyun alanındaki tek kuşun durumu. |
| `Obstacles` | `List<PipePair>` | Aktif boru çiftlerinin listesi. |
| `Score` | `int` | Anlık skor. |
| `HighScore` | `int` | LocalStorage'dan yüklenen en yüksek skor. |
| `IsPaused` | `bool` | Oyun döngüsü geçici olarak durduruldu mu? |
| `LastFrameDelta` | `float` | Son karedeki delta süresi (ms). |
| `Seed` | `int` | Deterministik RNG için kullanılan seed. |

- **Lifecycle**: Yeni oyunda `Score = 0`, LocalStorage'dan `HighScore` okunur. Çarpışma sonrası `IsPaused = true`, oyuncu restart seçeneğiyle `GameState` sıfırlanır.

## InputQueue
| Alan | Tip | Açıklama |
|------|-----|----------|
| `Timestamp` | `double` | Girişin alındığı zaman. |
| `Type` | `InputType` (enum) | `Click`, `SpaceKey`, `Touch` gibi değerler. |

- **Amaç**: Debouncing uygulamasını ve testlerde giriş sıralarını doğrulamayı kolaylaştırır.

## ScoreSnapshot
| Alan | Tip | Açıklama |
|------|-----|----------|
| `Score` | `int` | Oyun bittiğinde ulaşılan skor. |
| `HighScore` | `int` | Güncellenmiş en yüksek skor. |
| `Timestamp` | `DateTime` | Kaydın alındığı zaman.

- **Kullanım**: Quickstart ve manuel testler için skor değişimlerini raporlar; LocalStorage güncellemeleriyle eşleştirilir.
