# منصة حجز ملاعب البادل (Padel Booking Platform)

منصة إلكترونية ثنائية اللغة (عربي / إنجليزي) لحجز ملاعب البادل، بدون حاجة لإنشاء حساب من طرف العميل، مع لوحة تحكم كاملة للإدارة.

> **حالة المشروع الحالية:** قيد التطوير — راجع [`docs/06-Engineering-Plan.md`](./docs/06-Engineering-Plan.md) لمعرفة آخر مرحلة مكتملة.
> وثائق التخطيط الكاملة (PRD, TDD, App Flow, DB Schema, Design Brief, API Spec) موجودة في مجلد [`docs/`](./docs/).

---

## التقنيات المستخدمة (Tech Stack)

| الطبقة | التقنية |
|---|---|
| Backend | ASP.NET Core 8 Web API (Clean Architecture: Api / Application / Domain / Infrastructure) |
| ORM | Entity Framework Core 8 + Pomelo.EntityFrameworkCore.MySql |
| قاعدة البيانات | MySQL 8 (عبر Docker Compose) |
| Frontend | React 18 + TypeScript + Vite + Tailwind CSS v4 + shadcn/ui |
| Auth | JWT Bearer (لوحة التحكم فقط) + BCrypt |
| Validation | FluentValidation |
| CQRS | MediatR (v12 — النسخة المجانية آخر إصدار قبل تغيير الترخيص) |
| الدفع | بوابة Thawani (Sandbox) |
| التوثيق (API) | Swagger / OpenAPI 3 |

---

## خطوات التشغيل (Getting Started)

### المتطلبات
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (أو أحدث، بشرط توفر net8.0 runtime)
- [Node.js](https://nodejs.org/) 20+
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1) قاعدة البيانات (MySQL عبر Docker)

```bash
cp .env.example .env
# عدّل القيم داخل .env إذا لزم (خصوصًا إذا كان المنفذ 3306 مستخدمًا محليًا لديك — 
# افتراضيًا نستخدم المنفذ 3307 لتفادي أي تعارض مع تثبيت MySQL محلي)
docker compose up -d
```

### 2) الباكند (ASP.NET Core API)

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Padel.Infrastructure --startup-project src/Padel.Api
dotnet run --project src/Padel.Api
```
سيعمل الـ API افتراضيًا على `https://localhost:5xxx` مع Swagger UI متاح في بيئة التطوير على `/swagger`.

> ملاحظة: تأكد من تحديث connection string في `backend/src/Padel.Api/appsettings.Development.json` ليطابق المنفذ الذي اخترته في `.env`.

### 3) الفرونت اند (React)

```bash
cd frontend
npm install
npm run dev
```
سيعمل تطبيق الويب افتراضيًا على `http://localhost:5173`.

---

## بيانات الدخول إلى لوحة التحكم (Admin Credentials)

_سيتم تحديث هذا القسم عند إضافة الـ seed data (المرحلة 1 من خطة التنفيذ)._

---

## هيكلة المشروع

```
PadelAura/
├── backend/            # ASP.NET Core 8 solution (Clean Architecture)
│   └── src/
│       ├── Padel.Api/
│       ├── Padel.Application/
│       ├── Padel.Domain/
│       └── Padel.Infrastructure/
├── frontend/           # React + Vite + TypeScript SPA
├── database/           # ملفات قاعدة البيانات (mysql-data محلي عبر docker volume)
├── docs/               # وثائق التخطيط الكاملة (PRD, TDD, ERD, API Spec, ...)
├── docker-compose.yml  # MySQL container
└── .env.example
```

---

## ملاحظات إضافية

- منفذ MySQL الافتراضي في `docker-compose.yml` هو **3307** (وليس 3306) لتفادي التعارض مع أي خادم MySQL محلي مثبت مسبقًا على الجهاز.
- تم تثبيت `MediatR` على الإصدار **12.4.1** تحديدًا (بدلاً من أحدث إصدار) لأن الإصدارات 13+ أصبحت تتطلب ترخيصًا تجاريًا مدفوعًا.
- راجع [`docs/06-Engineering-Plan.md`](./docs/06-Engineering-Plan.md) لخطة التنفيذ الكاملة مرحلة بمرحلة، وقائمة الأولويات في حال ضاق الوقت.
