# InfoTrack – Solicitor Search

A .NET 8 Web API + React/TypeScript SPA that scrapes conveyancing solicitor listings from [solicitors.com](https://www.solicitors.com/conveyancing.html) by location and presents them in an insightful report.

---

## Architecture

```
InfoTrack/
├── src/
│   ├── InfoTrack.API/        # .NET 8 Web API (backend)
│   └── InfoTrack.Web/        # React 18 + TypeScript SPA (frontend)
└── InfoTrack.sln
```

### Backend highlights
| Concern | Implementation |
|---|---|
| HTTP layer | `HttpClient` with custom headers |
| HTML parsing | `HtmlAgilityPack` (HTML traversal only — all extraction logic is custom) |
| Persistence | EF Core **in-memory** database (zero config) |
| API docs | Swagger UI at `/swagger` |
| CORS | Pre-configured for Vite dev server (`localhost:5173`) |

### Frontend highlights
- **React 18** + **TypeScript** (strict mode)
- **Vite** dev server with API proxy — no CORS issues in dev
- **Recharts** for bar charts in the report view
- Tabs: Search · Locations · History

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 18+ |
| npm | 9+ |

---

## Running locally

### 1. Start the API

```bash
cd src/InfoTrack.API
dotnet run
```

The API starts on `http://localhost:5000` (or `https://localhost:5001`).  
Swagger UI: `http://localhost:5000/swagger`

### 2. Start the SPA

In a second terminal:

```bash
cd src/InfoTrack.Web
npm install
npm run dev
```

Open `http://localhost:5173` in your browser.

---

## Usage

1. **Search tab** — Click **Run Search** to scrape solicitors.com for all active locations. Results appear as an interactive report with charts.
2. **Locations tab** — Add, remove, or enable/disable search locations. Defaults: London, Birmingham, Leeds, Manchester, Sheffield, Bradford, Liverpool, Bristol.
3. **History tab** — Browse past searches and reopen any previous report.

---

## API Reference

| Method | Endpoint | Description |
|---|---|---|
| `GET`  | `/api/locations` | List all locations |
| `POST` | `/api/locations` | Add a location `{ "name": "..." }` |
| `PUT`  | `/api/locations/{id}` | Update name/active flag |
| `DELETE` | `/api/locations/{id}` | Remove a location |
| `PATCH` | `/api/locations/{id}/toggle` | Toggle active state |
| `POST` | `/api/solicitors/search` | Run scrape (optional body: `{ "locations": [...] }`) |
| `GET`  | `/api/solicitors/{id}` | Get raw result for a search |
| `GET`  | `/api/history` | List past searches |
| `GET`  | `/api/history/{id}/report` | Full report with stats |

---

## Database

The application uses an **EF Core in-memory database** — no installation or connection string required.  
Data is seeded with the 8 default locations on startup and persists for the lifetime of the process.

If you want persistent storage, swap `UseInMemoryDatabase` in `Program.cs` for:

```csharp
// SQL Server / LocalDB
options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));

// PostgreSQL
options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
```

Then add the connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=InfoTrack;Trusted_Connection=True;"
  }
}
```

And run `dotnet ef database update` after adding a migration.

---

## Design decisions

- **No scraping framework** — the scraper uses `HttpClient` + `HtmlAgilityPack` for DOM traversal. All XPath selectors and field-extraction logic are written explicitly, making the logic fully auditable and easy to adapt when the target site changes.
- **Cascade selector strategy** — the scraper tries multiple XPath patterns per field, falling back gracefully if the site markup changes.
- **Polite scraping** — 300–500 ms delays between requests to avoid hammering the target server.
- **In-memory DB** — keeps the project runnable with zero infrastructure. Trivially swappable for SQL Server / Postgres.
- **Clean separation** — controllers are thin; all business logic lives in typed service interfaces.
