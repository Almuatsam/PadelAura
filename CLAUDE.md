# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Bilingual (Arabic/English) padel court booking platform. Customers book courts without creating an
account; admins manage courts, schedules, closures, pricing, and bookings through a separate panel.
This is a timed technical assessment — see `docs/06-Engineering-Plan.md` for the deadline, phase
breakdown, and what to cut first if time runs short. Full specs live in `docs/`:
`01-PRD.md`, `02-TDD.md`, `03-App-Flow.md`, `04-Backend-Schema.md`, `05-Design-Brief.md`,
`07-API-Spec.yaml`, `08-Security-Hardening.md` (what was hardened in a later security pass, and the
trade-offs deliberately left as-is — read this before touching auth, rate limiting, or admin
seeding).

## Commands

### Database (MySQL via Docker)
```bash
cp .env.example .env      # first time only; default port is 3307, not 3306
docker compose up -d
```

### Backend (`backend/`, ASP.NET Core 8)
```bash
dotnet restore
dotnet ef database update --project src/Padel.Infrastructure --startup-project src/Padel.Api
dotnet run --project src/Padel.Api      # Swagger UI at /swagger in Development
dotnet build
dotnet format                            # formatting/analyzer fixes
```
Add a migration after changing entities or EF configurations:
```bash
dotnet ef migrations add <Name> --project src/Padel.Infrastructure --startup-project src/Padel.Api
```
`Padel.Application.Tests` (xUnit + FluentAssertions + NSubstitute + EF Core InMemory) covers
handler/validator logic; run a single test with `dotnet test --filter "FullyQualifiedName~<Name>"`.
There is no `Padel.Api`-level integration test project — HTTP-layer behavior (row-locking
concurrency, rate limiting, security headers) is verified live against the running dev server
instead (see each phase's plan/commit history for the curl-based verification approach used).

### Frontend (`frontend/`, React + Vite + TypeScript)
```bash
npm install
npm run dev        # http://localhost:5173
npm run build       # tsc -b && vite build
npm run lint        # oxlint
npm run preview
```
No frontend test runner is configured yet.

## Architecture

### Backend: Clean Architecture, 4 projects
`Padel.Domain` → `Padel.Application` → `Padel.Infrastructure` → `Padel.Api` (dependency direction
goes inward; `Api` references `Application` and `Infrastructure`, `Infrastructure` references only
`Application`, `Application` references only `Domain`).

- **Padel.Domain** — entities only, no external dependencies. Entities (`Booking`, `BookingItem`,
  `Court`, `CourtSchedule`, `CourtClosure`, `Customer`, `Admin`, `Payment`, `Promotion`,
  `PricingRule`, `AuditLog`) derive from `Entity` (`backend/src/Padel.Domain/Common/Entity.cs`) and
  follow a rich-domain-model style: private setters, state mutated only through named methods
  (`Court.Update`, `Booking.Confirm`, `Booking.Cancel`, `Promotion.ReplaceRules`, etc.), not public
  property assignment.
- **Padel.Application** — intended to hold CQRS handlers (MediatR v12 — pinned to 12.4.1 because
  v13+ requires a paid commercial license, do not upgrade past 12.x) and FluentValidation validators.
  Currently just scaffolded (csproj + package refs only, no handlers yet).
- **Padel.Infrastructure** — EF Core + Pomelo MySQL provider. `PadelDbContext` picks up entity
  configurations automatically via `ApplyConfigurationsFromAssembly`, so new entities need a
  matching `IEntityTypeConfiguration<T>` class in `Persistence/Configurations/` to be mapped —
  adding the `DbSet` alone is not enough. `DependencyInjection.AddInfrastructure` reads the
  `ConnectionStrings:Default` config value. `DbSeeder.SeedAsync` runs on every startup (idempotent,
  checks `.Any()` before inserting) and creates the default admin, seed courts, and a seed
  promotion.
- **Padel.Api** — `Program.cs` composition root: Serilog console logging, Swagger (dev only),
  CORS restricted to `Cors:AllowedOrigins` from config, JWT bearer auth wired up via
  `AddAuthentication`/`AddJwtBearer`. Cross-cutting concerns added in the security hardening pass
  (see `docs/08-Security-Hardening.md`): rate limiting (`AddRateLimiter`, named policies applied
  per-controller via `[EnableRateLimiting]`), baseline security response headers + `UseHsts()`
  outside Development, and `ICurrentAdminService` (resolves the JWT `sub` claim, used by admin
  command handlers to write `AuditLog` rows). Migrations run automatically at startup
  (`db.Database.MigrateAsync()`) followed by seeding — the default admin seed is now gated behind
  `Seed:CreateDefaultAdmin` (see that doc for the rotation requirement) — no separate manual
  migration step needed to boot the app locally, but `dotnet ef migrations add` is still required
  after model changes.

### Core business rules that shape the design (see `docs/02-TDD.md` §5.1 and `docs/01-PRD.md`)
These are the highest-priority, least-negotiable parts of the system per the engineering plan:
- **Court identity is hidden from customers.** Availability and booking confirmation must never
  expose which physical court (`Court.Name`) was booked — only the time slot.
- **Random court assignment.** When a booking is confirmed, an available court for that slot is
  chosen at random among the eligible ones (not first-available), and a time slot stays "available"
  to customers as long as at least one underlying court is free.
- **Booking must be atomic and race-safe.** Court assignment + `Booking`/`BookingItem` creation has
  to happen inside a transaction with correct isolation (or optimistic concurrency) so two
  simultaneous bookings can never double-book the same court/slot — this is the part most likely to
  be scrutinized, so any change to booking logic needs a concurrency-safety argument, not just a
  happy-path test.
- Invalid bookings (past times, closed/unavailable periods) must be rejected server-side.

### Frontend
Vite + React 19 + TypeScript, Tailwind v4, shadcn/ui (`components.json`: style `radix-nova`, RTL
enabled, path alias `@` → `frontend/src`). Currently just the Vite scaffold plus one shadcn
`Button` component (`frontend/src/components/ui/button.tsx`) — routing (`react-router-dom`), data
fetching (`@tanstack/react-query` + `axios`), forms (`react-hook-form` + `zod`), and i18n
(`i18next`/`react-i18next`, Arabic/English + RTL) are dependencies already installed but not yet
wired into `App.tsx`. `VITE_API_BASE_URL` (see `.env.example`) is how the frontend should reach the
API.

### Environment / config
`.env` (git-ignored, copy from `.env.example`) drives `docker-compose.yml` MySQL credentials/port
and is the reference for JWT and Thawani payment gateway (sandbox) settings — actual backend config
still needs to read the equivalents from `appsettings.*.json` / configuration providers, don't
hardcode secrets into `appsettings.json`.
