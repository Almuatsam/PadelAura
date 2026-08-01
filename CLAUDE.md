# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Bilingual (Arabic/English) padel court booking platform. Customers book courts without creating an
account; admins manage courts, schedules, closures, pricing, and bookings through a separate panel.
This was built as a timed technical assessment — see `docs/06-Engineering-Plan.md` for the phase
breakdown. Phases 0–7 (setup through QA) plus a security hardening pass are complete; only Phase 8
(delivery: README, public repo submission) remains. Full specs live in `docs/`: `01-PRD.md`,
`02-TDD.md`, `03-App-Flow.md`, `04-Backend-Schema.md`, `05-Design-Brief.md`, `07-API-Spec.yaml`,
`08-Security-Hardening.md` (what was hardened in a later security pass, and the trade-offs
deliberately left as-is — read this before touching auth, rate limiting, or admin seeding).

## Commands

### Database (MySQL via Docker)
```bash
cp .env.example .env      # first time only; default port is 3307, not 3306
docker compose up -d
```
MySQL's container port is bound to `127.0.0.1` only (see `docker-compose.yml`) — not reachable from
outside the host.

### Backend (`backend/`, ASP.NET Core 8)
```bash
dotnet restore
dotnet ef database update --project src/Padel.Infrastructure --startup-project src/Padel.Api
dotnet run --project src/Padel.Api      # http://localhost:5109, Swagger UI at /swagger in Development
dotnet build
dotnet format                            # formatting/analyzer fixes
```
Add a migration after changing entities or EF configurations:
```bash
dotnet ef migrations add <Name> --project src/Padel.Infrastructure --startup-project src/Padel.Api
```
`Padel.Application.Tests` (xUnit + FluentAssertions + NSubstitute + EF Core InMemory, 57 tests)
covers handler/validator logic; run a single test with
`dotnet test --filter "FullyQualifiedName~<Name>"`. There is no `Padel.Api`-level integration test
project — HTTP-layer behavior (row-locking concurrency, rate limiting, security headers) is
verified live against the running dev server instead (curl-based checks, documented in each
phase's commit history).

If a rebuild fails with a file-lock error (`MSB3026`/`MSB3027`), a background `dotnet run` process
is holding the DLLs — stop it first (e.g. find the PID listening on 5109 and kill it) before
rebuilding.

### Frontend (`frontend/`, React + Vite + TypeScript)
```bash
npm install
npm run dev        # http://localhost:5173
npm run build       # tsc -b && vite build
npm run lint        # oxlint
npm run preview
```
Needs its own `frontend/.env` (git-ignored, separate from the repo-root `.env`) with
`VITE_API_BASE_URL` pointing at the running backend, e.g. `http://localhost:5109/api`. No frontend
test runner is configured yet.

### CI
`.github/workflows/ci.yml` runs on push/PR to `main`: backend job builds, runs the full test suite,
and fails on any known-vulnerable NuGet package; frontend job builds, lints, and runs a
non-blocking `npm audit` (see `docs/08-Security-Hardening.md` for why it's non-blocking — one
known `react-router-dom` advisory has no fix released yet).

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
- **Padel.Application** — CQRS via MediatR v12 (pinned to 12.4.1 because v13+ requires a paid
  commercial license, do not upgrade past 12.x). Organized by feature folder mirroring the API's
  route groups: `Auth/Login`, `Courts/{Commands,Queries}`, `Closures`, `Bookings/{CreateBooking,
  GetAvailability, GetBookingByReference, GetAdminBookings, ProcessPaymentWebhook, Services}`,
  `Promotions`, `Dashboard`. Each command/query has a matching FluentValidation validator run
  automatically by `Common/Behaviors/ValidationBehavior.cs` (a MediatR pipeline behavior — every
  handler gets validated input for free, no per-handler validation calls needed).
  `IApplicationDbContext` (`Common/Interfaces/`) is the abstraction handlers depend on, implemented
  by `PadelDbContext`; `ICurrentAdminService` resolves the authenticated admin from the JWT for
  audit logging, implemented in `Padel.Api` (Application has no HttpContext concept).
- **Padel.Infrastructure** — EF Core + Pomelo MySQL provider. `PadelDbContext` picks up entity
  configurations automatically via `ApplyConfigurationsFromAssembly`, so new entities need a
  matching `IEntityTypeConfiguration<T>` class in `Persistence/Configurations/` to be mapped —
  adding the `DbSet` alone is not enough. `DependencyInjection.AddInfrastructure` reads the
  `ConnectionStrings:Default` config value. `DbSeeder.SeedAsync` runs on every startup (idempotent,
  checks `.Any()` before inserting) and creates seed courts + a seed promotion always, and the
  default admin only when `Seed:CreateDefaultAdmin` is enabled (on in Development, off in
  production by default — see `docs/08-Security-Hardening.md` for the rotation requirement if you
  do enable it outside local dev). `Identity/BCryptPasswordHasher.cs` and
  `Identity/JwtTokenGenerator.cs` back the two `Padel.Application.Common.Interfaces` abstractions
  used by login. `Payments/ThawaniClient.cs` talks to the Thawani checkout API per
  `docs/08-Payment-Integration.md`'s content (pasted into this project's chat history, not a
  committed file — see git history/plan notes if you need the exact API shapes again).
- **Padel.Api** — `Program.cs` composition root: Serilog console logging, Swagger (dev only), CORS
  restricted to `Cors:AllowedOrigins` from config, JWT bearer auth via
  `AddAuthentication`/`AddJwtBearer`. Controllers are thin — each action just sends a MediatR
  command/query and maps the result to an HTTP response; all 5 `Admin/*Controller.cs` files carry
  class-level `[Authorize]`, the 3 `Customer/*Controller.cs` files intentionally don't (public
  booking flow, no account). Cross-cutting concerns added in the security hardening pass (see
  `docs/08-Security-Hardening.md`): rate limiting (`AddRateLimiter`, named policies applied
  per-action via `[EnableRateLimiting]` — `login`, `booking`, `lookup`, `availability`, `webhook`),
  baseline security response headers + `UseHsts()` outside Development, and `ICurrentAdminService`
  used by admin command handlers to write `AuditLog` rows. Migrations run automatically at startup
  (`db.Database.MigrateAsync()`) followed by seeding — no separate manual migration step needed to
  boot the app locally, but `dotnet ef migrations add` is still required after model changes.

### Core business rules that shape the design (see `docs/02-TDD.md` §5.1 and `docs/01-PRD.md`)
These are the highest-priority, least-negotiable parts of the system per the engineering plan:
- **Court identity is hidden from customers.** Availability and booking confirmation must never
  expose which physical court (`Court.Name`) was booked — only the time slot. Admin-facing DTOs
  (`GetAdminBookings`) deliberately do expose it; customer-facing DTOs (`GetAvailability`,
  `GetBookingByReference`) deliberately don't — keep that split when touching either side.
- **Random court assignment.** When a booking is confirmed, an available court for that slot is
  chosen at random among the eligible ones (not first-available), and a time slot stays "available"
  to customers as long as at least one underlying court is free.
- **Booking must be atomic and race-safe.** Court assignment + `Booking`/`BookingItem` creation
  happens inside a transaction using `SELECT ... FOR UPDATE` row/gap locking
  (`PadelDbContext.GetCourtIdsWithActiveBookingForUpdateAsync`) so two simultaneous bookings can
  never double-book the same court/slot — this has been live-load-tested repeatedly (most recently
  during the security hardening pass) with concurrent requests against a single remaining court;
  any change to booking logic needs a concurrency-safety argument, not just a happy-path test.
- A Pending `Online` booking only occupies its slot for a grace window
  (`BookingPolicy.PendingPaymentGraceMinutes`) before being reclaimed — an abandoned Thawani
  checkout can't squat on a court forever.
- Invalid bookings (past times, closed/unavailable periods, more than
  `BookingPolicy.MaxSlotsPerBooking` slots) are rejected server-side.

### Frontend
Vite + React 19 + TypeScript, Tailwind v4, TanStack Query, react-hook-form + zod, axios,
react-router-dom, i18next/react-i18next (English/Arabic with RTL, `i18n/index.ts` mirrors the
active language onto `document.documentElement.{dir,lang}` site-wide). shadcn/ui conventions
(`components.json`: style `radix-nova`, RTL enabled, path alias `@` → `frontend/src`) but the UI
primitives in `components/ui/` (`button`, `input`, `dialog`, `select`, `table`, `badge`, `card`,
`switch`, `radio-group`, etc.) are hand-written against the already-installed `radix-ui` unified
package rather than pulled via the shadcn CLI.

Two route trees under a shared `App.tsx` (`QueryClientProvider` → `ToastProvider` → `AuthProvider`
→ `BrowserRouter`):
- **`features/customer/`** — public flow at `/`, `/book`, `/booking/:reference`. `LandingPage` →
  `BookingWizardPage` (local 3-step state machine: `DateSlotStep` for the time-slot grid + cart,
  `ContactPaymentStep`, `ReviewStep` — cart state lives in the wizard, not a global store, since it
  only needs to survive one session) → `BookingConfirmationPage`, which doubles as the page Thawani
  redirects back to after checkout (its success/cancel URLs point here, built server-side in
  `ThawaniClient.cs`).
- **`features/admin/`** — `/admin/login` (public) + `/admin/*` behind `ProtectedRoute` (checks
  client-side auth state for UX; the real enforcement is server-side `[Authorize]` plus the axios
  401 interceptor). `AdminLayout` wraps Dashboard/Courts/Closures/Bookings/Promotions pages with a
  sidebar nav (plus a horizontal nav strip below the `md` breakpoint, so the 640–767px tablet range
  isn't left with no navigation at all).

Each feature's `api/*.ts` files pair a typed request function with a `zod` schema validating the
response shape before it's used — a malformed API response throws immediately instead of silently
propagating. `lib/auth.tsx` stores the JWT in `localStorage` (a documented trade-off, see
`docs/08-Security-Hardening.md` — bearer-token auth means no CSRF exposure, but also no XSS
resistance the way an httpOnly cookie would have).

### Environment / config
`.env` (git-ignored, copy from `.env.example`) drives `docker-compose.yml` MySQL credentials/port
and documents the JWT/Thawani values; the backend actually reads config from
`appsettings.json`/`appsettings.Development.json` (git-tracked, dev-only values are fine there —
see `docs/08-Security-Hardening.md` for what's deliberately empty in the production file so it
fails fast without real secrets configured). `frontend/.env` (separate file, also git-ignored) is
what the Vite dev server actually reads for `VITE_API_BASE_URL`.

## Security

`docs/08-Security-Hardening.md` is the authoritative reference for anything touching auth, rate
limiting, admin seeding, or the JWT/CORS/headers setup in `Program.cs` — read it before changing
any of those. Short version: rate limiting is in place on login/booking/lookup/availability/webhook,
the default admin seed is gated and must be rotated before any non-local deployment, admin
mutations are audit-logged, and a handful of larger changes (RBAC enforcement, refresh tokens,
migrating off `localStorage`) were deliberately left as documented trade-offs rather than built
under deadline pressure — don't "fix" those without reading why first.
