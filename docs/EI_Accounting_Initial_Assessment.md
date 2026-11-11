# EI Accounting – Initial Assessment (Code-Based, Read-Only)

Status: **DRAFT FOR REVIEW. No application files, database objects or configuration were modified.** This is the only file created, and it is new (nothing existing was changed). Per `docs/refactoring_plan.md` §16, no implementation will start until this assessment is reviewed and approved.

## 0. Scope and evidence limits

**Evidence base:** source code and the 111 `.sql` scripts in `Accounting.Web/SQLMigrations/` only. Findings were gathered by three read-only code reviews of the Web app and reports, the data layer and SQL scripts, and the scheduler and integrations. I spot-checked none of them line by line myself, so line numbers should be confirmed before any change is made.

**Not available, so nothing below is a measured result:**
- No production or UAT database, execution plans, Query Store, wait statistics, deadlock or blocking data.
- No application telemetry, no report timing or failure logs, and no per-tenant data volumes.
- The core table schema is not in the repo. Only `Accounting_Bill`, `Accounting_BillLineItems` and `Accounting_LU_AmountDueBasedOnType` are defined. Index coverage on invoices, payments, journal entries and similar tables **cannot be assessed from the repo**.
- The repo has no test project. There is no automated regression safety net today.

**Labels used:** **CONFIRMED** = seen directly in code. **POTENTIAL** = plausible but needs runtime or DB evidence. Per the plan (§6.3), a CONFIRMED code pattern is not the same as a confirmed *root cause* of a measured slowdown. Nothing here has been tied to a measured slow report yet.

---

## 1. Understanding of the architecture (plan §16 items 1–2)

**Stack:** .NET 8 solution with five projects. SQL Server accessed through stored procedures via ADO.NET.

| Project | Role |
|---|---|
| `Accounting.Entities` | POCOs and DTOs. No logic. |
| `Accounting.DBUtilities` | Data access. `SQLHelper` (static connection string) plus repositories calling stored procedures. |
| `Accounting.Utilities` | PDF, Excel, CSV and Zip helpers, EWS email, encryption and OTP, plus integration clients (QBO, Sage, ChecKeeper, EI_Connect, Microsoft). Static `IConfiguration` and `IMemoryCache` holder (`WebAppReferenceUtility`). |
| `Accounting.Web` | MVC and JSON API host, legacy `Startup.cs`. In-process session, OpenID Connect SSO, OTP and trusted-device login, Basic auth for APIs. Razor runtime compilation on. |
| `Accounting.SchedulerJobs` | Separate ASP.NET Core host. A `BackgroundService` polls DB-stored cron jobs and runs eight job types. It references only `Entities` and has its **own** parallel data and utility layer. |

**Request flow (Web):** session JSON → `UserSession` (including `Client_Id`, the tenant key) → `BaseController` / `SessionSecurityFilterAttribute` → controller → repository → `SQLHelper` → stored procedure. `Client_Id` is passed as a parameter into stored procedures. Tenant filtering therefore lives in the SQL, not in a C# layer.

**Report flow:** `ReportsController` → `ReportService` / `ReportsUtility` → report metadata read from `LU_SQLQueries`, `LU_SQLInputParameters`, output-table and dynamic-filter tables → query or stored procedure executed → full `DataSet` in memory → regrouped into new DataTables → Excel, CSV or PDF built in memory → returned as base64 inside JSON.

**Tenancy model:** shared database, `Client_Id` column (879 references in SQL). Single tenant key, passed everywhere. The scheduler iterates per client from DB job config.

---

## 2. Critical accounting workflows and dependencies (plan §16 item 3)

Observed from names and structure only. I did **not** analyze the business rules, and none are proposed for change.

- **Invoicing / AR / AP:** search and detail stored procedures (`GetAgencyInvoices_V4`, `ARSearchResults_V3`, `APSearchResult_V1`, `GetInvoicedAgency`).
- **Payments:** payment search, `MakePayments` bills (`Accounting_Bill` and line items), check printing, ePay / CorePay / CoverPay entities.
- **Journal entries:** "Post Journal Entries" script, a Journal Entry report.
- **Commissions and statements:** agency, direct and insured bill statements, Tax/Fee pass-through, BDX, 1099, Earned/Unearned Premium Commission.
- **Notices (scheduler):** courtesy reminder, intent to cancel, notice of cancellation.
- **Bill process queue (scheduler):** pending queue, events, retry configuration, completed events.
- **External sync:** QBO and Sage (accounting), ChecKeeper (checks), EI_Connect, Microsoft (EWS email, SSO).

Dependency note: the scheduler and the web app both implement bill-process and report logic through **separate code paths** (duplicated, see §5). Any optimization must be applied to, and validated against, both.

---

## 3. Findings (plan §6, §8; item 6)

Severity is a **provisional, code-only** ranking, to be re-ranked once real timing data exists.

### 3.1 Findings that likely explain "reports time out / fail / are slow"

| # | Finding | Status | Where | Provisional severity |
|---|---|---|---|---|
| F1 | **Every SQL command has `CommandTimeout = 0` (wait forever).** `ExecuteDataset` sets 300 s but the later `PrepareCommand` overwrites it with 0. A slow query holds a request thread and a connection until the browser or IIS gives up. In the scheduler it blocks the single job loop. | CONFIRMED | `DBUtilities/SQLHelper.cs:121,394` (scheduler copy ~`:117`) | High |
| F2 | **No async in the data layer.** All 152+ repository calls are synchronous, and about 30 more `Task.Run(...).Result` calls sit on API paths. Slow reports and syncs exhaust thread-pool threads, which slows unrelated pages. | CONFIRMED | `Accounting.DBUtilities` (whole project); `API/FinancialServicesController.cs`, `API/CommonServiceController.cs` | High |
| F3 | **Whole-result materialization.** Report data is loaded into a `DataSet`, copied into a new all-string DataTable, `.Copy()`'d again per output table, built into a file in memory, then base64-encoded into JSON. Peak memory is roughly 5× or more the file size and the payload is 33% larger. | CONFIRMED | `ReportsUtility.cs:326-420`; `ReportService.cs:1634-1769`; `ReportsController.cs:1490` | High |
| F4 | **Agent-statement PDF generation is sequential and unbounded:** per agency, one DB call, one HTML build, one synchronous `HttpWebRequest` to the PDF service (infinite timeout), and one log insert, then a zip in memory. | CONFIRMED | `ReportService.cs:1744-1925`; `HttpServiceUtility.cs:136,161` | High |
| F5 | **Report metadata is re-read from the DB on every run, with N+1 queries** (per report, per query, per output table, per filter). Nothing is cached. | CONFIRMED | `ReportsUtility.cs:34-66,285-290,436` | Medium |
| F6 | **Non-sargable and catch-all SQL patterns:** leading-wildcard `LIKE '%x%'` across several joined tables with `OR` (about 164 uses), `(@p IS NULL OR col = @p)` catch-alls, and 30 `OPTION (RECOMPILE)` uses. These force scans, and RECOMPILE costs CPU on every call. | CONFIRMED (patterns); impact POTENTIAL | e.g. `[01272026 ] GetTransactionHistroySearchResults Modification.sql:42-48,100-105` | High (needs plans) |
| F7 | **Paging happens after the full result set is built** in several report and search procedures (OFFSET applied in the final select). The template-based reports (DirectBill, AgencyBill, InsuredBill, AgencyDetailed, Underwriter) have **no paging or total count at all**. | CONFIRMED | `ReportsUtility.cs:692-699`; `[05022026] GetAgencyInvoices Optimize SP.sql` | Medium |
| F8 | **Default READ COMMITTED with reports reading through writers.** NOLOCK appears in only 6 files, with no RCSI evidence in the repo. Reports can block behind writers and vice versa. | POTENTIAL (needs DB setting and blocking data) | SQL scripts; DB config | Medium–High |
| F9 | **Unindexed temp tables:** the index creation lines in at least one statement report are commented out. | CONFIRMED | `[04222026] AgencyBillAccountStatementReport Modification.sql` | Medium |
| F10 | **Indexing is largely not scripted in the repo.** 54 `CREATE INDEX` matches, nearly all commented out. The 3 active ones cluster on `(Status DESC, Bill_Id)`, a low-selectivity leading column. Whether `Client_Id` is indexed on core tables is **unknown**. | CONFIRMED (repo); real state UNKNOWN | `[01272026] MakePayments-*_Table.sql` | Needs DB check |
| F11 | **Row mapping by reflection with per-row string work** (`GetProperties()`, `ToLower/Trim/Replace` per column per property, no cached property map). CPU and allocation cost grows with rows × columns. | CONFIRMED | `SQLHelper.cs:955-1075` | Medium |
| F12 | **Dynamic report SQL built by string replace of placeholders** from `LU_SQLQueries`: plan-cache churn, and an injection surface. | CONFIRMED | `ReportsUtility.cs:316,323,629` | Medium (performance) / needs security review |

### 3.2 Findings that likely explain "pages are slow"

| # | Finding | Status | Where | Provisional severity |
|---|---|---|---|---|
| F13 | **`BaseController` runs a DB call on every request** (`GetClients`) and, when a `PolicyDetails_Id` is present, **10+ further queries** (agent, insured, interest, carrier, finance, underwriter, invoices and an invoice loop) **before the action even starts**. No caching. | CONFIRMED | `BaseController.cs:55-125` | High (every Policy Details page) |
| F14 | **Razor runtime compilation is enabled unconditionally** (production included), and `AddControllersWithViews()` is registered twice. | CONFIRMED | `Startup.cs:40-41` | Low–Medium |
| F15 | **In-process session**, 1-day idle timeout, with the `UserSession` JSON deserialized on every request (twice: `BaseController` and the filter). | CONFIRMED | `Startup.cs:30-33`; `BaseController.cs`; `SessionSecurityFilterAttribute.cs:108` | Low–Medium; also a scale-out limitation |
| F16 | **External calls on the request path:** Sage and QBO syncs (`.Result` blocking, infinite HTTP timeout) and EWS email sent in-request, including the **login OTP email**. A slow upstream blocks login and finance actions. | CONFIRMED | `FinancialServicesController.cs:2794,3401,3811`; `LoginController.cs:232`; `HttpServiceUtility.cs:161,260,377,571` | High |
| F17 | **Lookup cache has no expiry or refresh**, and per-request metadata lookups aren't cached at all. | CONFIRMED | `LookupUtility.cs:26-114` | Low–Medium |
| F18 | **Synchronous file logger with a global lock** (`File.AppendAllText` per call, `Directory.Exists` each time). It is a serialization point where it is used. | CONFIRMED | `FileLogger.cs:6-47` (both Web and scheduler copies) | Low |
| F19 | **EWS token acquired per send:** a new MSAL client and token round trip per email, with `.Result`. QBO token cache is 1 hour regardless of real expiry, with no stampede lock. | CONFIRMED | `EWS_EmailUtility.cs:60-70`; scheduler `EmailUtility.cs:59-95`; `QBOUtility.cs:833-843` | Medium |

### 3.3 Scheduler (affects all tenants at once)

| # | Finding | Status | Where | Provisional severity |
|---|---|---|---|---|
| F20 | **Jobs run one at a time in a single loop** (`await ExecuteJobAsync` inline). One slow job delays every tenant's due jobs. A job that overruns its window is missed, since `IsDue` only looks back one poll interval. Combined with F1 (no timeout), one hung procedure stops all scheduling. | CONFIRMED | `SchedulerService.cs:16-56` | High |
| F21 | **"ThreadOne" is not parallelism.** `MakePaymentBillProcessJob` and `...ThreadOne` are near-identical copies, both sequential, run by the same loop. Whether the stored procedure prevents double-processing if both run at once is unknown. | CONFIRMED / POTENTIAL (double-processing) | `Jobs/MakePaymentBillProcessJob*.cs` | Needs DB check |
| F22 | **N+1 per invoice in notice jobs:** per-invoice PDF HTTP call, email, log insert and queue insert, all sequential. Settings are fetched 5× per job, and `noticeLogs.Any(...)` is O(n·m). The email throttle sleeps inside the single loop. | CONFIRMED | `CourtesyReminderJob.cs`, `IntentToCancelJob.cs`, `NoticeOfCancellationJob.cs` | Medium |
| F23 | **Job configuration is read once at startup.** Changes need a restart. A malformed cron or timezone is silently treated as "never due". | CONFIRMED | `ConfigurationHelper.cs:41`; `CronHelper.cs:23` | Low (reliability) |
| F24 | **Manual job trigger bypasses the `RunningJobs` guard.** | CONFIRMED | `AccountingSchedulerJobsController.cs:157` | Medium (reliability) |

### 3.4 Observations outside "performance" (flagged only; no action proposed under this plan)

These are not performance items, and the plan forbids mixing functional changes into performance work, so each needs a separate decision from you.

- **Certificate validation is disabled process-wide** (`ServicePointManager...= delegate { return true; }`) in about 11 places, and `RestExtension` also disables it. Security-relevant.
- **Dynamic report SQL by string replace** (F12) is an injection surface.
- **Static shared-secret comparisons use plain `!=`** (scheduler controller token). `MemoryCacheController` exposes cache keys behind a shared token. The scheduler's Swagger UI is enabled in all environments.
- **Financial columns typed `FLOAT`:** `Accounting_Bill.Due` and `Amount_Applied` are `FLOAT`, not `DECIMAL`. This is a precision question, **not** something to change for performance. Flagged for your attention only.
- **Static, mutable, per-tenant state** (`QBOUtility._Token`, `_MemoryCache`; `WebAppReferenceUtility`, `ConfigurationHelper`) is a multi-tenant correctness risk under concurrency.
- **Fixed IV in AES encryption** (deterministic).
- **Possible connection leaks:** `GetNewTransaction()` and `ExecuteReader` leave disposal to callers (not audited). The `InstallmentPlansRepository` per-row procedure calls run inside one open transaction, holding locks across all of them.

---

## 4. Multi-tenant assessment (plan §7)

- **Tenant key:** `Client_Id` (a single column, 879 references). It is always passed as a parameter.
- **Whether every stored procedure and report filters by tenant is UNVERIFIED.** Reports resolve `PredefinedParameter_Id` and fall back to `client_Id`. This needs a DB-level audit of the report stored procedures, which is a data-isolation check, not just a performance one.
- `Accounting_Bill` has **no `Client_Id` column** in its definition, so tenant scoping for bills requires a join back to other tables.
- **Noisy-neighbor risks (POTENTIAL, supported by code):** F1 and F2 (shared thread pool, infinite timeouts), F20 (shared scheduler loop), F8 (shared READ COMMITTED locking), and parameter sniffing on a skewed `Client_Id` (a single cached plan for `sp_executesql` calls). There is no concurrency limit on report execution.
- **Static state:** QBO token and cache statics are shared across tenants (see §3.4).
- **Needs data:** per-tenant row counts, per-tenant report timing, and plan variance by `Client_Id`.

---

## 5. Duplication (affects how changes must be rolled out)

- `MakePaymentBillProcessJob` ≈ `...ThreadOne` (about 320 lines each).
- The three notice jobs (about 165 lines each) share one loop. The AR, AP and Invoice Production report jobs are near-identical.
- `EmailUtility` (scheduler) ≈ `EWS_EmailUtility`. `RestExtension` ≈ `HttpServiceUtility`. Scheduler `SQLHelper` (~1074 lines) probably duplicates `DBUtilities` (POTENTIAL, not verified).
- The report grouping and `ImportRow` code is copy-pasted in about 6 places in `ReportService.cs`.
- `ReportsController.cs` is ~2,277 lines, much of it commented-out code.
- Several stored procedures exist in multiple successive script versions. **Which version is live in the DB is not determinable from the repo.**

---

## 6. Initial performance investigation plan and methodology (plan §16 items 4–6)

**Step A – Get evidence first (requires your decision on access):**
1. Query Store / `sys.dm_exec_query_stats` top N by duration, CPU and reads. Missing-index DMVs. Wait stats. Blocking and deadlock history. TempDB usage.
2. DB settings: RCSI, compatibility level, MAXDOP, statistics and index maintenance jobs, and `Client_Id` index coverage on core tables.
3. Application telemetry or logs: slowest endpoints, report durations, failures and timeouts, per tenant.
4. Data volumes: row counts per core table, and per tenant.
5. A list from you of the reports and pages users complain about most, and which tenants.

**Step B – Rank by evidence (plan §6.3):** each issue is classified as *Confirmed performance issue*, *Potential issue needing investigation*, *Architectural limitation* or *Improvement achievable without major refactoring*. Priority comes from execution time, frequency, tenant impact, failure rate, data volume and business importance, not from how bad the code looks.

**Step C – Trace the top offenders end to end:** controller → service → procedure, with the actual plan, input/output/side effects, transaction boundary and the equivalence baseline, before proposing any change (plan §4.5).

### Areas needing deeper analysis
1. The report stored procedures behind the heaviest reports (AR, AP, Invoice Production, Tax/Fee, Transaction/Journal, statements). I read none of them line by line.
2. Which procedure version is live (duplicate scripts).
3. Tenant filtering coverage across all procedures.
4. The `BaseController` 10+ query chain: what it feeds, and whether each query is needed for every action.
5. Whether `PageSize`/`PageIndex` = 0 means "all rows" in the report procedures (`DownloadReport` sends 0 when absent).
6. Transaction boundaries and the lock window in write paths (`InstallmentPlansRepository`, payments, bill process).
7. The PDF conversion service's latency and capacity.

---

## 7. Proposed phased approach (plan §16 item 7)

Ordered by evidence-independent safety first. Each phase needs your approval, a rollback plan and equivalence checks.

| Phase | Theme | Examples | Business-logic risk |
|---|---|---|---|
| 0 | **Baseline and observability** (no behavior change) | Capture timings, Query Store, per-report duration and failure logging, a regression baseline harness (existing outputs saved for comparison) | None |
| 1 | **Safe operational guards**, to be approved explicitly | Finite command timeouts (F1), connection and command disposal, a per-job timeout in the scheduler | Low, but it changes failure behavior (a query that used to run forever will now fail), so it needs approval |
| 2 | **Database tuning, evidence-driven** | Targeted indexes from real plans, enabling temp-table indexes, statistics, parameter-sniffing fixes, and RCSI evaluation | Low if joins and filters are untouched. Requires a plan comparison and result diff per procedure. |
| 3 | **Application-layer caching and call reduction** | Cache report metadata (F5), lazy or conditional `BaseController` queries (F13), cache lookups with expiry (F17), reuse tokens (F19) | Low–Medium: stale-data and tenant-key risks in cache keys. Needs a per-tenant cache design. |
| 4 | **Report pipeline** | Reduce DataSet copies (F3), streaming and downloading files rather than base64-in-JSON, bounded concurrency for statement PDFs (F4), pagination where agreed | Medium. Output format and timing change, so it needs approval (plan §4.10). |
| 5 | **Async and scheduler isolation** | Async data access, remove `.Result` blocking, run scheduler jobs per tenant concurrently with a cap (F20), move email and external calls off the request path | Medium–High. Changes concurrency and sequencing, so it needs explicit approval and double-processing analysis. |
| 6 | **Consolidation of duplicates** | Merge copy-pasted jobs and report code | Medium. Strictly structural, behind regression tests. |

Quick wins that look achievable without changing business logic (**subject to approval and evidence**): F1 timeouts, F9 temp-table indexes, F14 and F18, metadata caching (F5), reuse of the MSAL client and token (F19), and moving the OTP email out of the request path.

---

## 8. Risks of performance optimization (plan §16 item 8)

- **Behavior changes disguised as performance changes:** timeouts that now fail, paging that changes totals, caching that serves stale data, concurrency that changes processing order.
- **Parameter sniffing and plan regressions**, particularly with skewed tenants.
- **Index changes** can slow writes and cause blocking during creation, so they need an online build and an off-hours window.
- **Double-processing** if scheduler concurrency is introduced without the claim or lock behavior of the stored procedures being known (F21).
- **Tenant data leakage** through shared caches keyed incorrectly (every cache key must include `Client_Id`).
- **No automated tests exist today**, so there is no regression safety net until the baseline harness is built.
- **Superseded script versions** mean the repo may not match the live DB. Changes to scripts alone may not reflect production.
- **Duplicated logic** means a fix applied in one place can leave the same defect in another.

---

## 9. Strategy for preserving business functionality (plan §16 item 9)

- Existing behavior is the reference. Nothing is changed without the plan §4.5 trace (inputs, outputs, side effects, transactions, sequence).
- No joins, filters, tenant filters, validations or calculation order are removed or reordered.
- Performance-only changes and anything that can alter behavior (§7 Phases 1, 4 and 5) are kept in **separate** changesets with their own approval.
- Financial types (including the `FLOAT` columns) are not touched.
- Tenant-specific configuration is read, never rewritten.
- Anything unclear is documented as a question instead of assumed (see §10).

## 10. Functional equivalence testing approach (plan §16 item 10)

1. **Golden baseline first:** for each report and key page, capture current outputs (row sets, totals, files) on a representative copy of data before any change.
2. **Procedure diffing:** run the old and new procedure versions against the same parameters across several tenants and compare result sets exactly (row-by-row, including `DECIMAL` precision, rounding and ordering).
3. **Write-path checks:** compare database side effects (rows written, statuses, journal entries) after identical workflows.
4. **Tenant-isolation tests:** verify that one tenant's request never returns another's data, including from caches.
5. **Concurrency and failure tests:** repeat under load, and with deliberate timeouts and upstream failures.
6. **Per-phase gate:** a phase passes only if outputs are identical and the performance target is met.

---

## 11. Items needing your input before work can proceed

1. **Access:** can I get a copy, or read access, of a production-like database, Query Store and logs? Without it, every finding stays POTENTIAL.
2. **Which reports and pages** are slowest or failing, and for which tenants?
3. **Environment:** which database matches this repo (UAT)? Which procedure versions are live?
4. **Approval policy:** per phase or per change? Are behavior-affecting guards (finite timeouts, F1) acceptable, and what timeout is tolerable?
5. **Report behavior:** is it acceptable to change the delivery mechanism (file download instead of base64-in-JSON, or async generation) as long as file contents are identical?
6. **Out-of-scope observations (§3.4):** do you want the security items and the `FLOAT` columns tracked separately?
7. **Where records and deliverables should live** (this file is in `docs/`; confirm).
8. **Targets:** what response times or timeouts count as acceptable?

## 12. Recommended next step

Approve (or adjust) §6 Step A so evidence can be gathered, and answer §11. I will not change any file or database object until you approve.

---

## 13. Iteration 1 update – verified local database findings (read-only inspection)

Source: read-only queries against the local `uat-accounting` database on `MSSQLLocalDB` (SQL Server 2019 **Express**). Connection details are intentionally not recorded here. These are **local development database** facts, not production findings.

**Correction to F10:** the live database has **340 indexes** (230 non-clustered) on 177 tables. F10 ("indexing is largely not scripted") was based on the repo scripts only and is superseded. The repo's scripts are incremental, and the live schema is the reference.

| ID | Finding | Status |
|---|---|---|
| D1 | Money and balance columns are `float`, not `decimal` (`Invoices.Amount`, `RemainingBalance`, `InvoiceDetails.Amount`, `PolicyDetailsLineItems.Premium`, and others). Not a performance change; flagged only. | Verified (schema) |
| D2 | Every core table clusters on `(Status, Id)` with a separate non-clustered unique index on `Id`. Joins by `Id` alone go through the non-clustered index plus a key lookup. Runtime impact is a hypothesis until measured at volume. | Verified (schema); impact unverified |
| D3 | `Client_Id` is nullable on `Invoices` and `InvoiceDetails`. No `Invoices` index leads with `Client_Id`. | Verified (schema); impact unverified |
| D4 | Overlapping indexes: `PolicyDetails` has 11 (five lead with `Client_Id` or `Status, Client_Id`), `PaymentApplicationLineItems` has 7, and `Payments` has 6. This costs insert speed and storage; read benefit is unproven. | Verified (schema); impact unverified |
| D5 | Query Store is OFF, so no execution history exists. Recovery model SIMPLE, RCSI OFF, compatibility level 150. | Verified |
| D6 | Several versions of the same procedure coexist (for example `GetAgencyInvoices_V3/V7/V9/V10/V11`, `GetMakePaymentsInvoices_V1/V2/V3`, `InsertUpdateInvoices` plus `_V1/_V2/_V3/_BAK`). Which version the application calls still has to be matched against the C# code. | Verified (database) |
| D7 | The database is empty apart from lookup and reference data, and the report configuration rows inserted by the user (17 reports, 14 output tables, 319 input-parameter rows). | Verified |
| D8 | Express limits: 10 GB data file per database, about 1.4 GB buffer pool, 1 socket / 4 cores. The estimated size of a 5M-invoice dataset with realistic related records is about 22 GB (range 13 to 31 GB, **an estimate, not a measurement**), so the user chose to scale the test down (option C, target fixed after a 50,000-invoice calibration). | Verified (limits); estimate unmeasured |

Not yet measured: execution plans, report timings, wait statistics, and anything at volume. Those come after the test data exists.
