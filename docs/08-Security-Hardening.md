# Security Hardening Notes — ملاحظات التحصين الأمني

This document records a security remediation pass performed against the codebase: what was
fixed, why, and — just as importantly — what was deliberately **not** built and why, so a future
reader doesn't mistake an intentional trade-off for an oversight.

---

## What was fixed

### 1. Rate limiting (`Padel.Api/Program.cs`)
`docs/02-TDD.md` §"الأداء" explicitly requires rate limiting on login and booking to prevent
brute-force/spam booking. Implemented using ASP.NET Core's built-in
`Microsoft.AspNetCore.RateLimiting` (no new package — part of the net8.0 shared framework),
partitioned by remote IP address, fixed-window:

| Policy | Endpoint | Limit |
|---|---|---|
| `login` | `POST /api/auth/login` | 5 / 5 min |
| `booking` | `POST /api/customer/book` | 8 / 1 min |
| `lookup` | `GET /api/customer/bookings/{reference}` | 20 / 1 min |
| `availability` | `GET /api/customer/availability` | 30 / 1 min |
| `webhook` | `POST /api/payment/webhook` | 30 / 1 min |

Rejections return a `429` with a `Retry-After` header and a ProblemDetails-shaped body, matching
`GlobalExceptionHandler`'s existing error style. `lookup`'s limit also narrows the brute-force
window for guessing a booking reference (the reference is the only credential needed to view a
booking's status — see the accepted-limitation note below for what this doesn't fully close).

### 2. Default admin credential gating (`DbSeeder.cs`, `Program.cs`)
The seeded `admin@padel.local` / `Padel@12345` login is now gated behind `Seed:CreateDefaultAdmin`
— `true` in `appsettings.Development.json`, absent (so `false`) in production `appsettings.json`.
**If you deploy this outside local development, rotate this password immediately** — either by
changing it directly in the `admins` table after first boot, or by never setting
`Seed:CreateDefaultAdmin` to `true` outside Development and creating a real admin account through
another channel.

### 3. Login timing side-channel (`LoginCommandHandler.cs`)
An unknown email previously short-circuited past the bcrypt `Verify` call, making it measurably
faster than a wrong-password attempt for a real email — even though both return the identical
`"Invalid email or password."` message. Now always verifies against a fixed dummy hash when no
admin is found, so both paths cost the same.

### 4. Shorter JWT lifetime (`appsettings.json`)
`Jwt:ExpiryMinutes` reduced from 60 to 30 — see the accepted-limitation note below for the full
picture (this alone doesn't add revocation).

### 5. Booking cart size cap (`BookingPolicy.MaxSlotsPerBooking`, `CreateBookingCommandValidator.cs`)
Capped at 20 slots per request. Previously unbounded — a single request could submit an arbitrary
number of slots, each taking a `FOR UPDATE` row/gap lock inside one transaction.

### 6. Admin action audit logging (`ICurrentAdminService`, `CurrentAdminService`, 6 command handlers)
The `AuditLog`/`audit_logs` table has existed since Phase 1 but nothing ever wrote to it. Now every
court create/update/delete, closure create, and promotion create/update writes one row recording
the admin, action, entity type/id, and a small JSON snapshot of the new state.

### 7. Raw-SQL invariant enforcement (`PadelDbContext.GetCourtIdsWithActiveBookingForUpdateAsync`)
The one raw-SQL call in the codebase interpolates court IDs into SQL text — safe today because
callers always source them from the database, never from user input. That assumption is now an
enforced guard (throws on any non-positive ID) instead of only a code comment, so a future change
that threads unvalidated input through this path fails loudly instead of silently becoming an
injection point.

### 8. Security response headers + HSTS (`Program.cs`)
Every response now carries `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and
`Referrer-Policy: strict-origin-when-cross-origin`. `UseHsts()` is enabled outside Development. No
CSP was added — this API returns JSON only, never renders HTML, so a content-security-policy
belongs on the frontend's hosting layer, not here.

### 9. Local MySQL bound to localhost + CI pipeline
`docker-compose.yml`'s MySQL port now binds to `127.0.0.1` only, not `0.0.0.0`. A new
`.github/workflows/ci.yml` builds and tests the backend (failing on any known-vulnerable NuGet
package) and builds/lints the frontend, since the project previously had no CI at all.

---

## Accepted limitations (deliberately not built, and why)

**Role-based authorization is modeled but not enforced.** `AdminRole` has `SuperAdmin` and
`Manager`, and the JWT even carries the role as a claim, but every `[Authorize]`-protected endpoint
treats any authenticated admin identically — there is currently no product requirement that
differentiates what a `Manager` can do versus a `SuperAdmin`. Building enforcement now would mean
inventing permission boundaries nobody asked for, which risks breaking legitimate admin usage for
no product benefit. If multi-level admin permissions become a real requirement, add
`[Authorize(Roles = "SuperAdmin")]` (or a policy) to the specific actions that should be restricted.

**JWT is stored in browser `localStorage`, with no server-side revocation.** Logout
(`frontend/src/lib/auth.tsx`) only deletes the client-side copy of the token; the JWT itself stays
valid until natural expiry (now 30 minutes, was 60) even after logout, a password change, or an
account compromise. The alternative — httpOnly cookies — would require adding CSRF protection back
in as a paired change, since this API currently has none *because* it doesn't need it with
bearer-token auth (a cross-site form can't set a custom `Authorization` header). Migrating storage
without also adding CSRF infrastructure would trade one vulnerability class for another. This is a
real architecture trade-off, not an oversight — revisit if this ever needs to run somewhere the
30-minute exposure window and lack of instant revocation matter more than the added complexity of
cookie+CSRF infrastructure.

**No refresh-token rotation.** Same trade-off as above: a full refresh-token flow needs a new
endpoint, a persisted revocable-token table, and frontend silent-refresh logic. Shortening the
access-token lifetime (item 4 above) is the cheap partial mitigation; the full flow is a real
feature addition, not a config change, and was out of scope for a remediation pass that must not
introduce breaking changes under deadline pressure.

**`react-router-dom` has one known advisory with no fix released.** `npm audit` flags
GHSA-qwww-vcr4-c8h2 ("RSC Mode CSRF Bypass") for the installed `7.18.2`, the range affected is
7.12.0–8.2.0. Checked via `npm view react-router-dom versions` at the time of this hardening pass:
7.18.2 is the newest published 7.x release and no 8.x has shipped yet, so **no non-breaking fix
currently exists**. The advisory is specific to React Server Components mode; this project is a
client-only Vite SPA using `BrowserRouter` with no RSC/SSR anywhere, so the vulnerable code path is
very likely unreachable here. Tracked in `.github/workflows/ci.yml`'s non-blocking `npm audit`
step — revisit when a patched version ships.

**Migrations still auto-run on API boot** (`Program.cs`, `db.Database.MigrateAsync()`). This is a
known anti-pattern for multi-instance production deployments (concurrent migration races), but
this project's deployment model is single-instance, and moving this to a separate release step is
an operational-process change, not a code fix — left as-is, documented here for awareness if the
deployment model ever changes.

**Booking reference enumeration is narrowed, not eliminated.** Item 1's `lookup` rate-limit policy
makes brute-forcing a 6-character reference (36⁶ ≈ 2.2 billion combinations) impractical at
reasonable request rates, but doesn't eliminate the possibility in principle. The response DTO
(`BookingStatusDto`) was already designed to exclude customer PII and court identity, so the
practical impact of a successful guess is limited to schedule/payment-status disclosure, not
personal data. A stronger fix (requiring the phone number used at booking time as a second lookup
factor) would change the public API contract and was out of scope for a non-breaking remediation
pass.
