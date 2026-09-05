# 📋 Session Tracker

A full-stack, multi-tenant web application for freelancers and tutors to track work sessions, sync with Google Calendar, and manage earnings — all from a single dashboard.

**🔗 Live Demo:** [work-tracker-production-3e52.up.railway.app](https://work-tracker-production-3e52.up.railway.app)

---

## ✨ Features

- **🔐 Google OAuth 2.0 Authentication** — Secure sign-in with Google, supporting multiple users with full data isolation via JWT claims
- **📅 Google Calendar Sync** — One-click sync pulls sessions from your Google Calendar; deletions in Calendar are reflected automatically on next sync
- **💰 Earnings Dashboard** — Real-time earnings with per-session hourly rates, completed vs. paid breakdown, and animated summary stats — all computed server-side
- **📊 Server-Side Filtering & Pagination** — Filter by title (case-insensitive), status, and date range entirely in EF Core; results paginated at 50/page
- **💵 Bulk Rate Management** — Set a default hourly rate per user; apply it to any subset of sessions using the filter-scoped rate widget
- **🏷️ Session Statuses** — Pending, Completed, Canceled, and Paid — each with optional notes/reference fields and color-coded UI
- **✏️ Inline Editing** — Edit hourly rates, durations, statuses, and notes directly in the table with auto-save on blur
- **📧 Nightly Digest Emails** — Automated daily email summaries of the previous day's sessions, sent via Resend HTTP API
- **🗑️ Session Management** — Full CRUD with delete confirmation and real-time UI updates

## 🏗️ Architecture

Built using **Clean Architecture** with clear separation of concerns:

```
SessionTrackerApi/
├── API/                    # Controllers (Auth, Sessions, User)
├── Application/            # Business logic, MediatR handlers, interfaces
│   ├── Features/
│   │   ├── Sessions/
│   │   │   ├── Commands/   # Sync, Update, Delete, BulkUpdateRate, SetDefaultHourlyRate
│   │   │   └── Queries/    # GetSessions (paginated), GetSessionsSummary, SessionQueryExtensions
│   │   └── Users/
│   ├── BackgroundServices/ # SessionReminderWorker (nightly digest)
│   └── Interfaces/         # IAppDbContext, IGoogleCalendarService
├── Domain/                 # Entities: Session, User, UserGoogleToken
├── Infrastructure/         # EF Core DbContext, Google Calendar, EmailService
└── wwwroot/                # Single-page frontend (HTML/CSS/JS)
```

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 10, C# |
| **Architecture** | Clean Architecture, CQRS with MediatR |
| **Database** | PostgreSQL (Production), SQLite (Development) |
| **ORM** | Entity Framework Core — migrations + schema fallback via `ALTER TABLE IF NOT EXISTS` |
| **Authentication** | Google OAuth 2.0, JWT Bearer Tokens |
| **External APIs** | Google Calendar API v3 |
| **Email** | Resend HTTP API (Railway blocks SMTP) |
| **Frontend** | Vanilla HTML/CSS/JavaScript (SPA) |
| **Deployment** | Railway (Docker), GitHub CI/CD |

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A Google Cloud project with OAuth 2.0 credentials and Calendar API enabled

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/YassoKhalid/Work-Tracker.git
   cd Work-Tracker
   ```

2. **Configure secrets** — Create `appsettings.Development.json`:
   ```json
   {
     "GoogleAuth": {
       "ClientId": "YOUR_GOOGLE_CLIENT_ID",
       "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
       "RedirectUri": "http://localhost:8080/api/auth/callback"
     },
     "Resend": {
       "ApiKey": "YOUR_RESEND_API_KEY"
     },
     "Jwt": {
       "Key": "YOUR_32_CHAR_SECRET",
       "Issuer": "SessionTrackerApi",
       "Audience": "SessionTrackerClient"
     }
   }
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```
   Open [http://localhost:8080](http://localhost:8080) in your browser.

### Google Cloud Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project and enable the **Google Calendar API**
3. Configure the **OAuth consent screen** (External, add your test email)
4. Create **OAuth 2.0 Client ID** credentials (Web application)
5. Add `http://localhost:8080/api/auth/callback` to **Authorized redirect URIs**

## 📦 Deployment (Railway)

The app is configured for one-click deployment on Railway:

1. Connect your GitHub repo to a new Railway project
2. Add a **PostgreSQL** database plugin
3. Set the following environment variables on the web service:

   | Variable | Description |
   |----------|-------------|
   | `DATABASE_URL` | PostgreSQL connection URI (from the Postgres plugin) |
   | `GoogleAuth__ClientId` | Google OAuth Client ID |
   | `GoogleAuth__ClientSecret` | Google OAuth Client Secret |
   | `GoogleAuth__RedirectUri` | `https://your-app.up.railway.app/api/auth/callback` |
   | `Jwt__Key` | A secure 32+ character secret key |
   | `Jwt__Issuer` | `SessionTrackerApi` |
   | `Jwt__Audience` | `SessionTrackerClient` |
   | `Resend__ApiKey` | Resend API key for digest emails |

> **Note:** Railway blocks SMTP — email is sent via the [Resend](https://resend.com) HTTP API instead.

## 🧩 Key Design Decisions

- **Multi-Tenancy via JWT Claims** — Every API request extracts `UserId` from the JWT token; users can only access their own data
- **Per-User OAuth Tokens** — Google refresh tokens are stored per user, enabling independent calendar sync per account
- **Dual Database Provider** — Automatic detection of `DATABASE_URL` switches between PostgreSQL (production) and SQLite (development); schema is kept up-to-date via `ALTER TABLE IF NOT EXISTS` fallbacks for Railway deployments where `Migrate()` is blocked
- **CQRS Pattern** — Commands and Queries are cleanly separated via MediatR; shared filter logic is extracted into `SessionQueryExtensions` to avoid duplication between `GetSessionsQuery` and `GetSessionsSummaryQuery`
- **Server-Side Aggregation** — Earnings and hours are computed in a dedicated `/api/sessions/summary` endpoint using EF Core projections, not in the browser
- **Server-Side Filtering** — All search/status/date filters are applied as EF Core `WHERE` clauses using `ILike` for case-insensitive PostgreSQL search; no client-side `.filter()` loops

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
