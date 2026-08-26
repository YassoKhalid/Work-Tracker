# 📋 Session Tracker

A full-stack, multi-tenant web application for freelancers and tutors to track work sessions, sync with Google Calendar, and manage earnings — all from a single dashboard.

**🔗 Live Demo:** [work-tracker-production-3e52.up.railway.app](https://work-tracker-production-3e52.up.railway.app)

---

## ✨ Features

- **🔐 Google OAuth 2.0 Authentication** — Secure sign-in with Google, supporting multiple users with full data isolation
- **📅 Google Calendar Sync** — One-click sync pulls session events directly from your Google Calendar
- **💰 Earnings Dashboard** — Real-time earnings calculation with per-session hourly rates, status tracking, and monthly totals
- **📊 Smart Filtering** — Filter sessions by status (Pending, Completed, Canceled) and search by title
- **✏️ Inline Editing** — Edit hourly rates, update statuses, and add cancellation reasons directly in the table with auto-save
- **📧 Nightly Digest Emails** — Automated daily email summaries of yesterday's sessions sent to each user
- **🗑️ Session Management** — Full CRUD operations with delete confirmation and real-time UI updates

## 🏗️ Architecture

Built using **Clean Architecture** with clear separation of concerns:

```
SessionTrackerApi/
├── API/                    # Controllers (Auth, Sessions)
├── Application/            # Business logic, MediatR handlers, interfaces
│   ├── Features/           # CQRS Commands & Queries
│   ├── BackgroundServices/ # Nightly email worker
│   └── Interfaces/         # Abstractions (IAppDbContext, IGoogleCalendarService)
├── Domain/                 # Entities (Session, User, UserGoogleToken)
├── Infrastructure/         # EF Core DbContext, Google Calendar, Email services
└── wwwroot/                # Single-page frontend (HTML/CSS/JS)
```

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 10, C# |
| **Architecture** | Clean Architecture, CQRS with MediatR |
| **Database** | PostgreSQL (Production), SQLite (Development) |
| **Authentication** | Google OAuth 2.0, JWT Bearer Tokens |
| **External APIs** | Google Calendar API v3 |
| **Email** | SMTP (Gmail) with HTML templates |
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
     "EmailSettings": {
       "SenderEmail": "your-email@gmail.com",
       "SenderPassword": "YOUR_GMAIL_APP_PASSWORD"
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
   | `EmailSettings__SenderEmail` | Gmail address for sending digests |
   | `EmailSettings__SenderPassword` | Gmail App Password |

## 🧩 Key Design Decisions

- **Multi-Tenancy via JWT Claims** — Each API request extracts `UserId` from the JWT token, ensuring users can only access their own data
- **Per-User OAuth Tokens** — Google refresh tokens are stored per user in the database, enabling calendar sync for each individual account
- **Dual Database Provider** — Automatic detection of `DATABASE_URL` env var switches between PostgreSQL (production) and SQLite (development)
- **CQRS Pattern** — Commands (sync, update, delete) and Queries (get sessions) are cleanly separated using MediatR

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
