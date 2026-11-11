# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ExpertInsured Accounting (UAT) — a .NET 8 solution (`UAT_ExpertInsured_Accounting.sln`) with five projects. No tests, no README, and the directory is not a git repository. Backing store is SQL Server (stored procedures); default dev connection is `(localdb)\MSSQLLocalDB`, database `uat-accounting`.

## Commands

```
dotnet build UAT_ExpertInsured_Accounting.sln
dotnet run --project Accounting.Web            # MVC app + JSON APIs
dotnet run --project Accounting.SchedulerJobs  # background scheduler + Swagger UI (served in all environments)
```

There is no test project, so there is no test or lint command. `Accounting.Web.csproj.user` / `Accounting.SchedulerJobs.csproj.user` hold local Visual Studio launch settings.

## Architecture

Project dependency chain: `Entities` ← `DBUtilities` ← `Utilities` ← `Web`. `SchedulerJobs` references **only** `Entities` and has its own parallel repository/service/utility layer (it does not reuse `DBUtilities`/`Utilities`), so logic such as MakePayment processing exists in both Web and SchedulerJobs.

- **Accounting.Entities** — POCOs only. `DB/` mirrors tables/SP result sets (Lookups, Transactions); `External/` holds DTOs for third-party integrations (ChecKeeper, CorePay, CoverPay, EI, ePay, QBO, Sage, Microsoft); `Internal/` holds app DTOs, `UserSession`, `ServiceResponse`.
- **Accounting.DBUtilities** — data access. `SQLHelper` holds a static `ConnectionString` (set in `Startup` constructor). `Implementations/` + `Interfaces/` are repositories split into `Lookups` and `Transactions`; they call stored procedures through `SQLHelper`.
- **Accounting.Utilities** — shared helpers: `Internal/` (PDF/Excel/CSV/Zip, EWS email, encryption, OTP, trusted-device, lookups) and `External/` integration clients (ChecKeeper, EI_Connect, QBO, Sage, Microsoft). `WebAppReferenceUtility` exposes static `IConfiguration` and `IMemoryCache` set during startup — utilities read config/cache through it rather than DI.
- **Accounting.Web** — MVC + API host. Uses `Startup.cs` (not minimal hosting) for DI. Controllers (`Controllers/`) return Razor views; `API/` has JSON controllers (JSON naming policy is disabled → PascalCase properties). Session-based auth (`SessionSecurityFilterAttribute`, `BaseController`), plus OpenID Connect/SSO (`SSOController`), OTP/trusted-device login, and a `BasicAuthentication` scheme for API calls. Razor runtime compilation is enabled. Reports are driven by lookup tables (`LU_SQLQueries`, `LU_SQLInputParameters`, `ReportFileDownloadSettings`) via `ReportService`.
- **Accounting.SchedulerJobs** — ASP.NET Core host running `SchedulerService` (a `BackgroundService`) that polls every `SchedulerPollingIntervalInSeconds`, evaluates each job's cron expression/time zone (Cronos) from DB-stored `SchedulerClientJobs`, and runs jobs via `JobFactory` (`Jobs/*` implement `ISchedulerJob`). `ConfigurationHelper.Initialize` loads settings and master-lookup IDs from the DB at startup into statics; `RunningJobs` prevents overlapping execution of the same job. Adding a job requires: a class in `Jobs/`, DI registration in `Program.cs`, and a matching DB job record/`JobFactory` mapping.

## Conventions and gotchas

- Database changes live as hand-written scripts in `Accounting.Web/SQLMigrations/`, named `[MMDDYYYY] description.sql`; there is no migration runner — add new scripts in that style and apply manually.
- `appsettings.json` ships with empty values for secrets/endpoints (EWS, SSO, encryption key/IV, tokens); real values come from environment-specific config. Don't commit real secrets.
- Static state is common (`SQLHelper.ConnectionString`, `WebAppReferenceUtility`, `ConfigurationHelper`); be careful when changing initialization order in `Startup`/`Program`.
- Many "type" values (payment status, directions, entity types) are byte IDs resolved from master lookup tables, not hardcoded enums.
