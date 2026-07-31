# App Flow — تدفق النظام
## منصة حجز ملاعب البادل

---

## 1. تدفق العميل (Customer Flow)

```mermaid
flowchart TD
    A[Landing Page] --> B[اختيار اللغة عربي/إنجليزي]
    B --> C[اختيار التاريخ]
    C --> D[عرض الأوقات المتاحة]
    D --> E[اختيار ساعة أو أكثر — سلة الحجز]
    E --> F{يريد حجز يوم آخر ضمن نفس العملية؟}
    F -->|نعم| C
    F -->|لا| G[إدخال رقم الهاتف - إجباري]
    G --> H[الاسم والبريد - اختياري]
    H --> I[اختيار طريقة الدفع]
    I -->|دفع عند الوصول| J[مراجعة الطلب]
    I -->|دفع إلكتروني| K[Thawani Checkout]
    K -->|نجاح| J
    K -->|فشل| I
    J --> L[تأكيد الحجز]
    L --> M[صفحة النجاح + رمز الحجز]
```

### تفاصيل كل خطوة

| الخطوة | التفاصيل التقنية |
|---|---|
| Landing | عرض تعريف بسيط بالمنشأة + زر "احجز الآن" |
| اختيار اللغة | Toggle يبدّل i18next locale فوريًا + اتجاه الصفحة (RTL/LTR) |
| اختيار التاريخ | Date Picker يمنع اختيار تواريخ ماضية |
| عرض الأوقات | `GET /api/customer/availability?date=...` → قائمة سلوتات (بدون أسماء ملاعب) |
| سلة الحجز | إدارة محلية (state) لكل السلوتات المختارة عبر تواريخ/ساعات متعددة |
| بيانات العميل | Validation: رقم هاتف عُماني بصيغة صحيحة (إجباري)، بريد بصيغة صحيحة إن أُدخل |
| الدفع | Thawani Checkout Session أو وسم "دفع عند الوصول" |
| التأكيد | `POST /api/customer/book` → Transaction تُخصّص الملاعب عشوائيًا لكل سلوت |
| النجاح | عرض رمز مرجعي (Booking Reference) + ملخص + تفاصيل بدون اسم الملعب |

---

## 2. تدفق الإدارة (Admin Flow)

```mermaid
flowchart TD
    A[Login] --> B[Dashboard - نظرة عامة]
    B --> C[إدارة الملاعب]
    B --> D[إدارة الحجوزات]
    B --> E[إدارة التسعير]
    B --> F[إدارة العروض]
    B --> G[التقويم/الإغلاقات]
    B --> H[التقارير]
    B --> I[الإعدادات]

    C --> C1[إضافة/تعديل/حذف ملعب]
    C --> C2[تحديد ساعات العمل]

    G --> G1[إغلاق ملعب/عدة ملاعب/الكل]
    G --> G2[لتاريخ محدد أو مدى تواريخ]

    D --> D1[فلترة: ملعب/تاريخ/حالة/دفع/هاتف]
    D --> D2[تفاصيل حجز فردي]

    E --> E1[سعر الساعة لكل ملعب]
    F --> F1[إنشاء عرض حسب عدد الساعات]
```

---

## 3. منطق الحجز التفصيلي (Booking Engine Sequence)

```mermaid
sequenceDiagram
    participant C as Customer
    participant API as Booking API
    participant DB as Database
    participant PAY as Thawani

    C->>API: GET /availability?date=X
    API->>DB: احسب السلوتات المتاحة (كل الملاعب - المحجوز - المغلق)
    DB-->>API: قائمة سلوتات (بدون أسماء ملاعب)
    API-->>C: عرض الأوقات المتاحة

    C->>API: POST /book { slots[], phone, paymentMethod }
    API->>DB: BEGIN TRANSACTION
    API->>DB: تحقق مجدداً من توفر كل سلوت (lock)
    alt أحد السلوتات لم يعد متاحًا
        API-->>C: خطأ: السلوت X لم يعد متاحًا
        API->>DB: ROLLBACK
    else كل السلوتات متاحة
        API->>DB: اختر ملعبًا عشوائيًا لكل سلوت
        API->>DB: أنشئ Booking + BookingItems (status: Pending)
        API->>DB: COMMIT
        alt دفع إلكتروني
            API->>PAY: أنشئ Checkout Session
            PAY-->>C: إعادة توجيه لصفحة الدفع
            PAY->>API: Webhook: نجاح/فشل الدفع
            API->>DB: تحديث حالة الحجز والدفع
        else دفع عند الوصول
            API->>DB: تحديث الحالة إلى Confirmed
        end
        API-->>C: تأكيد + رمز الحجز
    end
```

---

## 4. حالات الحجز (Booking Status State Machine)

```mermaid
stateDiagram-v2
    [*] --> Pending: إنشاء الحجز
    Pending --> Confirmed: دفع عند الوصول / دفع إلكتروني ناجح
    Pending --> Cancelled: انتهاء مهلة الدفع / فشل الدفع
    Confirmed --> Completed: انتهاء وقت الحجز
    Confirmed --> Cancelled: إلغاء من الإدارة
```
