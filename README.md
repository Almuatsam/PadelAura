# PadelAura — Padel Court Booking Platform

A bilingual (Arabic/English) padel court booking platform. Customers browse availability and book
courts without creating an account; admins manage courts, schedules, closures, pricing, promotions,
and bookings through a separate authenticated panel.

Built as a timed technical assessment. All planned phases (setup through QA) are complete, plus a
dedicated security hardening pass. Full specs live in [`docs/`](./docs/): [`01-PRD.md`](./docs/01-PRD.md),
[`02-TDD.md`](./docs/02-TDD.md), [`03-App-Flow.md`](./docs/03-App-Flow.md),
[`04-Backend-Schema.md`](./docs/04-Backend-Schema.md), [`05-Design-Brief.md`](./docs/05-Design-Brief.md),
[`06-Engineering-Plan.md`](./docs/06-Engineering-Plan.md), [`07-API-Spec.yaml`](./docs/07-API-Spec.yaml),
and [`08-Security-Hardening.md`](./docs/08-Security-Hardening.md) (what was hardened, and the
trade-offs deliberately left as documented decisions rather than fixed under deadline pressure).

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API — Clean Architecture (Domain → Application → Infrastructure → Api) |
| CQRS | MediatR 12.4.1 (pinned — v13+ requires a paid commercial license) |
| Validation | FluentValidation, run automatically via a MediatR pipeline behavior |
| ORM | Entity Framework Core 8 + Pomelo MySQL provider |
| Database | MySQL 8 (Docker Compose, bound to `127.0.0.1` only) |
| Auth | JWT Bearer (admin panel only) + BCrypt password hashing |
| Payments | Thawani checkout API (sandbox) |
| Testing | xUnit + FluentAssertions + NSubstitute + EF Core InMemory (57 tests) |
| Frontend | React 19 + TypeScript + Vite + Tailwind CSS v4 |
| Frontend data/forms | TanStack Query, react-hook-form + zod, axios |
| i18n | i18next / react-i18next (English + Arabic, full RTL support) |
| UI | shadcn/ui conventions on radix-ui primitives (hand-written, not CLI-generated) |
| CI | GitHub Actions — backend build/test/dependency-audit, frontend build/lint |

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 20+
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Database (MySQL via Docker)

```bash
cp .env.example .env
docker compose up -d
```

MySQL listens on `127.0.0.1:3307` by default (not the standard 3306, to avoid clashing with a local
MySQL install) and is not reachable from outside the host.

### 2. Backend (ASP.NET Core API)

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Padel.Infrastructure --startup-project src/Padel.Api
dotnet run --project src/Padel.Api
```

Runs at `http://localhost:5109`, with Swagger UI at `/swagger` in Development. Migrations and
database seeding (courts + a promotion, always; a default admin, only in Development — see below)
run automatically on startup, so no separate manual migration step is required to boot locally.

`backend/src/Padel.Api/appsettings.Development.json` already points at the Docker Compose database
and Thawani's public UAT sandbox keys, so no edits are needed for local development. To run the 57
backend tests:

```bash
dotnet test
```

### 3. Frontend (React + Vite)

```bash
cd frontend
cp .env.example .env   # points VITE_API_BASE_URL at the backend above
npm install
npm run dev
```

Runs at `http://localhost:5173`.

## Admin panel credentials

A default admin account is seeded automatically **only in the Development environment**
(`Seed:CreateDefaultAdmin: true` in `appsettings.Development.json`; unset/`false` in production):

| Field | Value |
|---|---|
| Email | `admin@padel.local` |
| Password | `Padel@12345` |

Login at `/admin/login`. This is a local-evaluation-only credential — it must be rotated (or the
seed flag left off) before any non-local deployment; see
[`docs/08-Security-Hardening.md`](./docs/08-Security-Hardening.md) for the reasoning.

## Core routes

- **Customer** (public, no account required): `/` (landing), `/book` (date/slot picker → cart →
  contact & payment → review), `/booking/:reference` (confirmation page, also where Thawani
  redirects back to after checkout).
- **Admin** (JWT-protected, `/admin/*`): Dashboard, Courts, Closures, Bookings, Promotions.

## Key design decisions

- **Court identity is hidden from customers.** Availability and booking confirmation only ever
  expose a time slot, never which physical court was assigned — admin views intentionally do show
  it.
- **Random court assignment**, chosen at booking-confirmation time among all eligible courts for
  that slot (not first-available).
- **Race-safe booking.** Court assignment and booking creation happen inside a transaction using
  `SELECT ... FOR UPDATE` row/gap locking, so concurrent requests can never double-book the same
  court/slot — verified under live concurrent-request load testing, not just unit tests.
- **Pending online payments** only hold a slot for a grace window before being reclaimed, so an
  abandoned checkout can't squat on a court indefinitely.

## Project structure

```
PadelAura/
├── backend/            # ASP.NET Core 8 solution (Clean Architecture)
│   └── src/
│       ├── Padel.Api/           # composition root, controllers, middleware
│       ├── Padel.Application/   # CQRS handlers, validators, DTOs (MediatR)
│       ├── Padel.Domain/        # entities, no external dependencies
│       └── Padel.Infrastructure/# EF Core, MySQL, JWT, BCrypt, Thawani client
├── frontend/           # React + Vite + TypeScript SPA
├── database/           # local MySQL data volume (Docker)
├── docs/               # PRD, TDD, app flow, DB schema, design brief, API spec, security notes
├── docker-compose.yml  # MySQL container (localhost-bound)
└── .env.example
```

## Security notes

A dedicated hardening pass covered rate limiting (login/booking/lookup/availability/webhook),
gating the default admin seed, a login timing side-channel fix, a shortened JWT lifetime, a booking
cart size cap, admin action audit logging, hardening the codebase's one raw-SQL call, baseline
security response headers + HSTS, and localhost-only dev database binding, plus a CI pipeline that
fails the build on known-vulnerable NuGet packages. A handful of larger changes — enforcing the
existing RBAC roles, moving off `localStorage` for the JWT, refresh-token rotation — were left as
explicitly documented trade-offs rather than built under deadline pressure. Full detail, including
*why* each trade-off was accepted, is in [`docs/08-Security-Hardening.md`](./docs/08-Security-Hardening.md).

## Additional notes

- MySQL's default port is **3307**, not 3306, to avoid conflicting with a locally installed MySQL
  server.
- `MediatR` is pinned to **12.4.1** specifically — 13+ requires a paid commercial license.
- No frontend test runner is configured; backend logic is covered by 57 xUnit tests. HTTP-layer
  behavior (concurrency locking, rate limiting, security headers) was verified live against the
  running dev server rather than through a separate integration-test project.
