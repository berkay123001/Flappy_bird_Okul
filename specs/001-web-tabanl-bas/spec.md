# Feature Specification: Web tabanlı Flappy Bird klonu

**Feature Branch**: `001-web-tabanl-bas`  
**Created**: 2025-09-26  
**Status**: Draft  
**Input**: "Web tabanlı, basit bir Flappy Bird klonu oluştur. Ana özellikleri şunlar olmalı: 1. Oyun başladığında, bir kuş ekranın sol tarafında belirir. 2. Kuş, yerçekimi etkisiyle sürekli aşağı doğru düşer. 3. Oyuncu, fareye tıkladığında veya boşluk tuşuna bastığında kuş yukarı doğru küçük bir sıçrama yapar. 4. Ekranın sağ tarafından, aralarında dikey bir boşluk olan rastgele yüksekliklerde boru çiftleri gelir. 5. Kuş, borulara veya yere çarparsa oyun biter ve \"Game Over\" mesajı gösterilir. 6. Kuş, bir boru çiftini başarıyla geçtiğinde oyuncunun skoru 1 artar. 7. Ekranda anlık skor ve en yüksek skor (high score) görünsün. 8. Tüm görseller (kuş, borular, arka plan) anayasada belirtildiği gibi `t/assets` klasöründen alınmalıdır."

## Execution Flow (main)
```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
   - User types and permissions
   - Data retention/deletion policies  
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Security/compliance needs

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
Casual oyuncu web tarayıcısı üzerinden oyunu açar, kuşu tıklayarak havada tutmaya çalışır ve borular
arasından geçerek skorunu ve en yüksek skorunu artırmak ister.

### Acceptance Scenarios
1. **Given** oyun yüklendiğinde kuş ekranda sol tarafta bekliyor, **When** oyuncu fareye tıklar
veya boşluk tuşuna basarsa, **Then** kuş anında yukarı doğru küçük bir sıçrama yapar ve oyun 60 fps
akışını koruyarak devam eder.
2. **Given** kuş aktif olarak boru çiftleri arasında ilerlerken, **When** kuş bir boruya veya yere
çarparsa, **Then** oyun durur, "Game Over" mesajı görünür ve oyuncunun elde ettiği skor ile en yüksek
skor ekranda gösterilir.

### Edge Cases
- Çok hızlı ardışık tıklamalar ya da tuş basımları olduğunda oyun kuşun hareketini nasıl sınırlar?
- Boru yerleşimleri çok dar olduğunda oyun dengeyi bozmadan nasıl minimum boşluk belirler?
- Asset dağıtım süreci, proje kökündeki `Assets` klasöründen üretim zamanında `wwwroot/assets`
altına taşınan dosyaları kullanarak tutarlı kalmalıdır.
- High score verisi LocalStorage'da tutulduğundan tarayıcı yeniden yüklendiğinde veya sekme kapanıp
açıldığında da korunmalıdır.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: Oyun açıldığında kuş ekranda sol tarafta görünmeli ve oyuncu girişi beklemelidir.
- **FR-002**: Kuş yerçekimi nedeniyle sürekli aşağı doğru ivmelenmeli ve bu davranış tüm tarayıcılarda
tutarlı olmalıdır.
- **FR-003**: Oyuncu fare tıklaması veya boşluk tuşu ile kuşa anında (≤80 ms gecikmeyle) küçük bir
yukarı sıçrama kazandırabilmelidir.
- **FR-004**: Rastgele yüksekliklerdeki boru çiftleri ekranın sağından belirli aralıklarla gelmeli ve
kuş borular arasından geçebilecek kadar boşluk bırakılmalıdır.
- **FR-005**: Kuş boruya veya zemine çarptığında oyun derhal bitmeli, "Game Over" mesajı gösterilmeli ve
oyuncunun skoru ekranda sabitlenmelidir.
- **FR-006**: Kuş her boru çiftini başarıyla geçtiğinde skor 1 artmalı ve anında güncellenmiş skor
ekranda görünmelidir.
- **FR-007**: Ekranda anlık skor ile en yüksek skor aynı anda sunulmalı; en yüksek skor tarayıcı
LocalStorage'ında saklanmalı ve sayfa yenilense bile korunmalıdır.
- **FR-008**: Oyun 60 fps hedefini korumalı ve kare sürelerindeki sapmalar izlenebilir olmalıdır.
- **FR-009**: Tüm görsel ve ses dosyaları yalnızca `wwwroot/assets` klasöründen yüklenmeli; build
süreci proje kökündeki `Assets` klasöründen gerekli kopyalamayı gerçekleştirmelidir.
- **FR-010**: Oyun sırasında zıplama (flap), skor kazanma (score) ve çarpma (hit) ses efektleri
çalınmalı; dosyalar `.mp3` veya `.wav` formatında `wwwroot/assets` klasöründe bulunmalı ve gerekli
durumlarda C# → JavaScript interop ile tetiklenmelidir.
- **FR-011**: Oyun zorluğu sabit kalmalı; boruların hızları ve aralarındaki boşluk oyun boyunca
değişmemelidir.
- **FR-012**: Asset taşıma süreci `/Assets` kökünden `wwwroot/assets` hedefine gerçekleşmeli ve
dağıtım öncesi bütünlük kontrolü yapılmalıdır.

*Tüm kritik gereksinimler netleştirildi; ek sorular plan aşamasında ortaya çıkarsa yeniden
değerlendirilecektir.*

### Key Entities *(include if feature involves data)*
- **Bird**: Oyuncunun kontrol ettiği karakter; mevcut yükseklik, hız, yaşadığı çarpışmalar ve skor ile
ilgili tetikleyiciler.
- **PipePair**: Her biri üst ve alt borudan oluşan engel; dikey boşluk yüksekliği, çıkış zamanı ve hız
gibi parametreleri içerir.
- **Scoreboard**: Anlık skor, en yüksek skor ve oyun durumunu (oyunda, durdu, game over) tutan gösterge;
en yüksek skor tarayıcı LocalStorage'ında saklanan değerle eşitlenir.

## Clarifications

### Session 2025-09-26
- Q: Nihai oyunda hangi asset klasörü kullanılacak? → A: Tüm görsel ve ses dosyaları yalnızca `wwwroot/assets` klasöründen yüklenecek.
- Q: High score bilgisi nasıl kalıcı hale getirilecek? → A: Tarayıcı LocalStorage'ında saklanacak, sayfa yenilense bile korunacak.
- Q: Ses efektleri için kapsam nedir? → A: Flap, score ve hit efektleri `.mp3`/`.wav` olarak `wwwroot/assets`'tan yüklenecek, arka plan müziği olmayacak.
- Q: Oyun zorluğu nasıl ayarlanacak? → A: Zorluk sabit kalacak; boru hızları ve boşlukları değişmeyecek.
- Q: Proje kökündeki `Assets` klasörü nasıl ele alınacak? → A: Kodlama, bu dosyaların build sırasında `wwwroot/assets` altına taşınacağı varsayımıyla yapılacak.

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous  
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified

---

## Execution Status
*Updated by main() during processing*

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---
