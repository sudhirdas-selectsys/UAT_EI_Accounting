# Database Change and Test-Data Log

Scope: the local development database `uat-accounting` on `MSSQLLocalDB` (SQL Server 2019 Express). Connection details are intentionally not recorded. Every database change made for the performance assessment is listed here. Nothing has been deleted or dropped.

| # | Date | Change | Authorization | Reversible how |
|---|---|---|---|---|
| 1 | 2026-11-11 | `ALTER DATABASE [uat-accounting] SET QUERY_STORE = ON` (READ_WRITE, capture mode ALL, 5-minute intervals, 1000 MB max, wait stats ON). Database-level setting; no data or object changes. | Approved by the user (`Iteration1_Answers` follow-up, question 6). | `ALTER DATABASE ... SET QUERY_STORE = OFF` (on request). |

| 2 | 2026-11-11 | `DBCC CHECKIDENT ('Accounting_LU_LineItems_Clients', RESEED, 2000)` on the empty table. Reason: `Accounting_LU_LineItems_Directions` has 2,528 rows (and BLD has 3,666 linked rows) pointing at `LineItems_Clients_Id` 1-1663 with no matching `LU_LineItems_Clients` row. A new client's ids starting at 1 would inherit those unrelated mappings. No rows were inserted, changed or deleted. | Approved by the user (reseed to 2000). | `DBCC CHECKIDENT (..., RESEED, 0)` while the table is still empty. |

| 3 | 2026-11-11 | Applied `docs/seed/01_PerfTest_Client_Seed.sql` (inserts only). Created test client `PERFTEST_CLIENT` (`Client_Id` = 1); 11 `LU_LineItems_Clients` rows (ids 2000-2010); 13 `LU_LineItems_Directions` rows; 23 `LU_BillingType_LineItems_Directions` rows; 1 payment plan (code `PT3`: 25% down, 2 monthly installments); 1 user (`perftest.user`, Customer Admin, placeholder password, not a working credential). Used the application's own procedures for the line-item rows. The first run failed on `QUOTED_IDENTIFIER` (sqlcmd defaults it off), rolled back completely (verified: no rows written, 0 open transactions), and was re-run with `sqlcmd -I`. | Approved by the user (seed script review). | Rows are tagged by `Client_Id` 1 and `LineItems_Clients_Id` >= 2000. Cleanup would be a delete of those test rows only, with your approval. |

Verified after apply: LIC ids 2000-2010; the orphan `LineItems_Directions` rows (2,528) are untouched; `USP_Accounting_GetLineItems_ClientsByBillingType_Id` returns 8, 7 and 5 distinct line items for billing types 1, 2 and 3, as designed.

Earlier inserts by the user (not by Claude): report configuration rows (17 reports, 14 output tables, 319 input-parameter rows, and related tables).

## Test-data generation
Test client created (change #3). No policies or invoices loaded yet. Next steps: build the generator from the application's creation logic (NEW policies, mixed Agency/Insured/Direct, manual payments, no journals), load a 50,000-invoice calibration set under `Client_Id` 1, then present recalibrated size estimates before scaling. `LU_ReportTemplates` is still unseeded (waiting on template content).
