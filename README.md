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
├── client/                  register UI — React frontend (Vite, port 5173)
├── admin/                   back-office UI for the laptop (Vite, port 5174)
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

4. **Back office** (product management + month-end reports, Admin login only):

   ```
   cd admin
   npm install
   npm run dev
   ```

   Runs on http://localhost:5174. Add products manually here: name,
   department, barcode (leave empty for a register quick button), cost
   price, opening stock (recorded as an Adjustment movement) and any number
   of sell units. Month End gives sales/profit/stock tables with CSV export
   and current stock valuation at cost.

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

1. Build both frontends into the API's wwwroot:

   ```
   cd client && npm run build
   cd ../admin && npm run build
   ```

   Copy `client/dist/*` into `AnseNouveau.API/wwwroot/` and `admin/dist/*`
   into `AnseNouveau.API/wwwroot/admin/` (Program.cs serves the register at
   `/` and the back office at `/admin`).

   The API binds `http://0.0.0.0:5000` in production (`Urls` in
   appsettings.json), so the back office is reachable from a laptop on the
   shop network at `http://<counter-pc-ip>:5000/admin` — allow port 5000
   through Windows Firewall on the counter PC.

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
