# AnseNouveau — POS System

Local-first POS + inventory system for a small grocery shop on Sint Maarten.
ASP.NET Core Web API (raw ADO.NET, no ORM) + SQL Server + React (Vite).

## Layout

```
AnseNouveau.sln
├── AnseNouveau.API          controllers, DTOs, JWT auth, Program.cs
├── AnseNouveau.Business     services + IService interfaces
├── AnseNouveau.DataAccess   ADO.NET repository implementations
├── AnseNouveau.Domain       models + IRepository interfaces (zero dependencies)
├── client/                  React frontend (Vite, port 5173)
└── database/                AnseNouveau_Dev.sql (schema + seed)
```

## Running (dev)

1. **Database** — with the SQL container (`sql-server-mac`, localhost:1433) up:

   ```
   sqlcmd -S localhost,1433 -U sa -P <password> -i database/AnseNouveau_Dev.sql
   ```

   Seeded logins: **Admin / PIN 1234**, **Cashier / PIN 5678**.

2. **API** — put the real sa password into `AnseNouveau.API/appsettings.Development.json`
   (`ConnectionStrings:DefaultConnection`; the committed file has a placeholder), then:

   ```
   dotnet run --project AnseNouveau.API
   ```

   Runs on http://localhost:5000 (Swagger at /swagger in dev).

3. **Frontend**:

   ```
   cd client
   npm install
   npm run dev
   ```

   Runs on http://localhost:5173. The API base URL defaults to
   `http://localhost:5000/api`; override with `VITE_API_URL` if needed.

## Auth

PIN login (BCrypt-hashed) → JWT bearer token; an Axios request interceptor adds
the header. Everything except `POST /api/auth/login` and `GET /api/auth/users`
requires a token. Price changes, user management and reports require the
Admin role.

## Notes for merging with main

- Projects target **net8.0** (the only SDK reachable in the build sandbox).
  Bump `<TargetFramework>` in the four csproj files to match main — nothing
  else depends on the version. Classic `.sln` is used for the same reason;
  swap for the `.slnx` on main if preferred.
- POS quick buttons are the active products **without a barcode**, grouped by
  department. If main has an `IsQuickButton` column instead, it's a one-line
  filter change in `client/src/pages/PosPage.jsx` (`quickProducts`) plus the
  column in `ProductRepository`.
- `AmountTendered`/`ChangeGiven` are stored in the tender currency;
  `TenderRate` is the snapshot for converting back to base. `TotalAmount` is
  always base currency (EUR), so expected cash and the Z-report are in EUR.
- Business dates use a fixed UTC−4 offset (`Business/BusinessDay.cs`) since
  sale times are stored in UTC and Sint Maarten has no DST.

## Deployment (counter PC, Windows)

1. Build the frontend into the API's wwwroot:

   ```
   cd client
   npm run build
   ```

   Copy `client/dist/*` into `AnseNouveau.API/wwwroot/` (Program.cs already
   has `UseStaticFiles` + `MapFallbackToFile("index.html")`).

2. Publish self-contained:

   ```
   dotnet publish AnseNouveau.API -c Release -r win-x64 --self-contained -o publish
   ```

   Set the production connection string (SQL Server Express) and a real
   `Jwt:Key` in `appsettings.json` next to the exe.

3. Install as a Windows service (or a scheduled task at logon):

   ```
   sc create AnseNouveau binPath="C:\ansenouveau\AnseNouveau.API.exe" start=auto
   ```

4. Kiosk browser on the counter screen:

   ```
   chrome --kiosk http://localhost:5000
   ```

The whole system runs on the one PC; no internet required.
