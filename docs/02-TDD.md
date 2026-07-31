# Technical Design Document (TDD)
## منصة حجز ملاعب البادل

**الإصدار:** 1.0

---

## 1. القيود التقنية المفروضة (Hard Constraints)

بحسب وثيقة الاختبار، هذه القيود **إلزامية ولا يمكن تجاوزها**:

- **Backend:** يجب استخدام **.NET 8** أو **Laravel** فقط. لا يُسمح بأي إطار عمل آخر.
- **Frontend:** إما موقع ويب بـ **React**، أو تطبيق إلكتروني بـ **Flutter**. لا يُشترط تطبيق الاثنين، لكن تطبيقهما معًا نقطة إضافية **بشرط** ألا يؤثر ذلك على جودة أي منهما (يُفضَّل نظام واحد ممتاز على نظامين بجودة أقل).
- **بوابة الدفع:** Thawani (Sandbox environment).
- **التسليم:** مستودع GitHub عام + ملف README شامل.

> **القرار المعتمد لهذا المشروع:** Backend بـ **.NET 8**، Frontend **ويب فقط بـ React** (بدون Flutter)، للتركيز على جودة نظام واحد بدل التشتت بين نظامين — تماشيًا مع التوصية الصريحة في وثيقة الاختبار.

---

## 2. البنية المعمارية العامة (High-Level Architecture)

```
┌─────────────────────┐
│   React (Vite/TS)   │  ← Customer Web + Admin Dashboard (SPA)
└──────────┬───────────┘
           │ HTTPS / REST (JSON)
┌──────────▼───────────┐
│   ASP.NET Core 8 API │  ← Clean/Layered Architecture
│  (Controllers → CQRS │
│   Handlers → Domain) │
└──────────┬───────────┘
           │ EF Core
┌──────────▼───────────┐
│       MySQL 8        │
└───────────────────────┘
           │
┌──────────▼───────────┐
│   Thawani Payment     │  ← Sandbox API (Checkout Session)
│        API             │
└───────────────────────┘
```

## 3. نمط البنية الخلفية (Backend Architecture Pattern)

نعتمد **Clean Architecture** مبسّطة (4 طبقات) لضمان قابلية التوسع والاختبار:

```
src/
├── Padel.Api/              # Controllers, Middleware, DI, Swagger
├── Padel.Application/      # Use Cases (CQRS: Commands/Queries), DTOs, Validators (FluentValidation)
├── Padel.Domain/           # Entities, Value Objects, Domain Rules, Enums
└── Padel.Infrastructure/   # EF Core, Repositories, Thawani Client, SMS (future), Migrations
```

- **CQRS خفيف** عبر MediatR: كل عملية (مثل `CreateBookingCommand`, `GetAvailabilityQuery`) لها Handler مستقل → كود منظم وسهل الاختبار.
- **Repository + Unit of Work** فوق EF Core لعزل منطق الوصول للبيانات.
- **FluentValidation** لكل الـ DTOs الواردة (Validation منفصل عن الـ Controllers).
- **AutoMapper** أو Mapping يدوي بسيط بين Entities و DTOs.
- **Global Exception Middleware** يعيد استجابات خطأ موحّدة (Problem Details / RFC 7807).

## 4. المكدس التقني (Tech Stack)

### Backend
| المكوّن | التقنية |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 (Code-First + Migrations) |
| قاعدة البيانات | MySQL 8 (عبر Pomelo.EntityFrameworkCore.MySql) |
| Auth | JWT Bearer (لوحة التحكم فقط) + BCrypt لتشفير كلمات المرور |
| Validation | FluentValidation |
| التوثيق | Swagger / Swashbuckle (OpenAPI 3) |
| الاختبار | xUnit + Moq + FluentAssertions |
| اللوق | Serilog |

### Frontend
| المكوّن | التقنية |
|---|---|
| Framework | React 18 + TypeScript + Vite |
| التوجيه | React Router v6 |
| إدارة حالة السيرفر | React Query (TanStack Query) |
| النماذج | React Hook Form + Zod |
| التنسيق | TailwindCSS + shadcn/ui |
| الترجمة | i18next / react-i18next (عربي RTL / إنجليزي LTR) |
| الحركات | Framer Motion |
| الأيقونات | lucide-react |

### البنية التحتية
| المكوّن | التقنية |
|---|---|
| الحاويات | Docker + docker-compose |
| السيرفر العكسي | Nginx |
| الاستضافة | Ubuntu VPS (أو أي خادم Demo) |
| CI (اختياري) | GitHub Actions (build + test) |

---

## 5. منطق النطاق الحرِج (Critical Domain Logic)

### 5.1 التخصيص العشوائي للملعب (Anonymous Random Court Assignment)

هذا أهم جزء منطقي في النظام. القواعد:

1. العميل **لا يرى أبدًا** أسماء الملاعب — فقط "الوقت متاح" أو "غير متاح".
2. سلوت زمني (مثال: 18:00–19:00 بتاريخ 2026-08-05) يُعتبر **متاحًا** طالما يوجد **ملعب واحد على الأقل** غير محجوز وغير مغلق لهذا التوقيت.
3. عند التأكيد النهائي (بعد الدفع أو اختيار الدفع عند الوصول)، يقوم النظام بـ:
   - قفل الصف (Row Lock / Transaction) لمنع التعارض (Race Condition) عند حجزين متزامنين لنفس اللحظة.
   - جلب كل الملاعب المتاحة لهذا التوقيت (غير مغلقة، غير محجوزة).
   - اختيار ملعب عشوائيًا من بينها (`ORDER BY RAND()` أو Fisher-Yates في التطبيق).
   - إنشاء الحجز وربطه بالملعب المختار.
4. يجب أن تكون هذه العملية **Atomic** (ضمن Database Transaction + Isolation Level مناسب، أو Optimistic Concurrency عبر RowVersion) لتفادي حجز نفس الملعب مرتين في نفس اللحظة.

> **مثال:** 3 ملاعب متاحة الساعة 18:00. يحجز عميلان بنفس اللحظة تقريبًا. يجب أن يحصل كل منهما على ملعب مختلف تلقائيًا، وإذا حجز 3 عملاء فأكثر لنفس الساعة، يُمنع الرابع فورًا (السلوت يختفي من قائمة المتاح).

### 5.2 حساب السعر والعروض (Pricing Engine)

- كل ملعب له `hour_price` أساسي.
- العروض (Promotions) تُطبَّق حسب **عدد الساعات المتتالية المحجوزة في نفس عملية الحجز الواحدة**:
  - مثال: ساعة واحدة = 10 ريال، ساعتان فأكثر = 8 ريال/ساعة.
- خوارزمية الحساب:
  1. تحديد أعلى Tier عرض ينطبق على `total_hours` في السلة.
  2. `subtotal = hours × applicable_rate`.
  3. `discount = (base_price - applicable_rate) × hours` (لأغراض العرض في الفاتورة).
  4. `total = subtotal`.
- العروض تُدار مركزيًا (globally) أو لكل ملعب — يُقرَّر حسب التصميم؛ الافتراضي: عروض عامة تنطبق على كل الملاعب ما لم يُحدَّد خلاف ذلك.

### 5.3 منع تعارض الحجز (Booking Conflict Prevention)

- لا يمكن حجز وقت في الماضي (`start_time < NOW()` في منطقة Asia/Muscat).
- لا يمكن حجز وقت خارج ساعات عمل الملعب.
- لا يمكن حجز وقت ضمن فترة إغلاق (Court Closure).
- عند الحجز المتعدد (أكثر من ساعة/أكثر من يوم ضمن نفس السلة)، يُتحقق من توفر **كل سلوت على حدة** قبل التأكيد النهائي؛ إن فشل أي سلوت، تُرفض العملية كاملة مع رسالة توضح أي الأوقات لم تعد متاحة (Atomic booking للسلة كاملة).

---

## 6. الأمان (Security)

- تشفير كلمات مرور المدراء بـ BCrypt.
- JWT قصير العمر (Access Token) + Refresh Token اختياري.
- Rate Limiting على Endpoints الحساسة (تسجيل الدخول، الحجز) لمنع Brute-force/Spam booking.
- التحقق من صحة كل المدخلات على مستوى الـ API (لا يُعتمد على تحقق الواجهة فقط).
- HTTPS إجباري في بيئة الإنتاج.
- عدم تسريب معرّفات الملاعب أو بيانات داخلية في استجابات واجهة العميل.
- CORS مضبوط بدقة (Origin محدد لا `*`).
- Webhook الدفع من Thawani يُتحقق من توقيعه (Signature verification) قبل تحديث حالة الحجز.

## 7. الأداء (Performance)

- Indexes على: `bookings(court_id, date, start_time)`, `bookings(status)`, `court_closures(court_id, date)`.
- Query واحد محسَّن لحساب التوفر بدل N+1.
- Caching خفيف (In-memory) لساعات عمل الملاعب والعروض (تتغير نادرًا).
- Pagination لكل قوائم لوحة التحكم (الحجوزات، إلخ).
