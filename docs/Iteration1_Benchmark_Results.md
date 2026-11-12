# Iteration 1: Local Benchmark Results (Reports and Pages)

**Scope and labelling.** All figures are **local test-environment findings** on SQL Server 2022 **Standard** (local `MSSQLSERVER2022` instance, database `uat-accounting-perf`, a restored copy of the test database), about 2.49M invoices. They are not production measurements and must not be extrapolated to production. SQL Server Express is not used in UAT or production, so no Express figures are used or compared in this document. No connection strings or credentials appear here.

Raw data: `docs/results/standard_report_benchmark.csv` (reports) and `docs/results/standard_page_benchmark.csv` (pages). Harness: `docs/seed/06_ReportBenchmark.ps1` (300 s timeout per call).

## Test conditions and limits (read before using these numbers)
- Parallel plans enabled (SQL Server 2022 Standard, compatibility level 150). The copy was verified identical to the source database (178 tables, 566 procedures, 2,492,000 invoices, 0 duplicate invoice numbers).
- A shared 31.7 GB workstation that also runs other SQL Server instances (54 other databases on this one), browsers and other programs. It is **not** a production server.
- The runs were repeatedly stopped by low system memory (four times). They were finished in smaller batches after capping this instance's `max server memory` at 12,288 MB (revert note at the end of `DB_Change_and_TestData_Log.md`). Memory pressure may have inflated some timings; this was not measured separately. Repeat runs of the same procedure differed by up to about 35% (for example Invoice Production 547 s and 790 s), so treat single timings as approximate.
- Reports 1 to 13 come from the run that was killed after report 13; 14 to 17 and the page calls were run in separate batches. Slow reports ran once; reports under 60 s ran twice.
- The Invoices clustered index was 88.3% fragmented at test time (a side effect of the invoice renumbering), which inflates reads and times.
- The first run (sections 1 and 2) did not pass `@InvoiceDate`, so 11 reports may not have applied the date window. Those 11 were re-run with it applied; see section 1b, which supersedes the section 1 figures for those reports.

## Dataset under test
890,000 policies; 2,492,000 invoices (all unique, `INV` + 11 digits); 15,308,000 invoice details; 1,744,411 payments; 4,635,147 payment-application line items; 0 orphans; 1,495,220 paid / 249,191 partial / 747,589 unpaid.

## How to read the scenarios
"Scenario" is the date range (and page size) passed to a report. Each report was run with several inputs.

| Scenario | Input | Purpose |
|---|---|---|
| narrow | 2026-01-01 to 2026-01-31 (one month), page size 100 | Smallest window; the usual "last month" run |
| year | all of 2025, page size 100 | Medium window |
| full | 2025-01-01 to 2027-12-31 (three years), page size 100 | Covers all test data |
| full-download-10000 | full range, page size 10,000 | Mimics a large page or export; paged reports only |
| single | one fixed input | Used by every call in the pages benchmark (section 2) |

- Unpaged statement reports return everything regardless of page size.
- After a timeout, that report's wider windows were skipped.
- "Worst" and "Best" in section 1 are the slowest and fastest times across a report's scenarios, not repeat runs.
- The test data spans several years, so "narrow" is not a small slice of rows for every report.

## 1. Reports (17), worst scenario per report, slowest first
| # | Report | Worst (s) | Best (s) | Rows returned (total matching) | Logical reads | CPU ms | Result |
|---|---|---|---|---|---|---|---|
| 15 | Direct/Insured Comm Statement | 312.0 | 278.7 | 2,136,301 | 52.7M | 629.6K | OK |
| 14 | BDX | 305.4 | - | none | 0 | 0 | **TIMEOUT** |
| 1 | Invoice Production | 300.8 | - | none | 634K | 172.3K | **TIMEOUT** |
| 16 | Earned vs Unearned | 300.4 | - | none | 290K | 343.8K | **TIMEOUT** |
| 4 | Payment Report | 300.2 | 132.7 | none | 0 | 0 | **TIMEOUT** (see section 3, item 8) |
| 2 | Agency Billed Acct Statement | 233.9 | 150.6 | 1,700,740 | 3.4M | 401.4K | OK |
| 9 | Insured Billed Comm Statement | 213.9 | 188.4 | 1,281,901 | 33.9M | 449.1K | OK |
| 3 | Direct Billed Comm Statement | 134.0 | 118.1 | 854,701 | 21.0M | 312.7K | OK |
| 5 | Transaction Report | 125.6 | 83.0 | 100 (4,635,147) | 176.3M | 457.0K | OK |
| 8 | Tax/Fee Passthrough | 113.0 | 52.0 | 10,000 (2,492,000) | 6.5M | 282.0K | OK |
| 13 | Underwriter Comm Statement | 111.1 | 87.9 | 854,409 | 12.8M | 215.0K | OK |
| 17 | Paid BDX | 85.1 | 67.4 | 100 (581,449) | 961K | 225.2K | OK |
| 7 | Accounts Payable | 70.3 | 57.4 | 100 (2,492,000) | 436K | 203.7K | OK |
| 6 | Accounts Receivable | 32.6 | 26.4 | 100 (2,648,596) | 622K | 66.6K | OK |
| 12 | Agency Detailed Statement | 0.86 | 0.09 | 300 | 116 | 157 | OK, tiny result (not a valid load test) |
| 11 | 1099 | 0.39 | 0.17 | 0 (0) | 23K | 452 | OK, returned 0 rows (not a valid load test; see note below the table) |
| 10 | Journal Entries | 0.11 | 0.01 | 0 (0) | 2 | 0 | OK, empty (journals not generated) |

**1099 (report 11), why it returned 0 rows (local test finding, investigated afterwards by calling the procedure directly):** (1) the procedure filters the date window on `Accounting_AgentDetails.CreatedOn` (when the agent row was created), not on any payment or invoice date. All 300 test agents were created at seeding time (2026-11-11), so the narrow and year windows match no agent; the full window (2025-01-01..2027-12-31) returns all 300. Whether the original 0-row full-window run happened before the agents were seeded was not verified. (2) Even when rows return, `1099 Amount` is NULL for every agent, because the procedure sums payments with `EntityType_Id = 1` and `PaymentDirection_Id = 2` (payments to agents) and the test data has none (all 872,205 agent payments are direction 1). The 0.39 s timing therefore reflects an almost empty result, not the real cost of the report. Making it a valid test needs direction-2 agent payments seeded on the test copy; that is a data change and needs approval.

Scenarios: narrow = 2026-01-01..2026-01-31, year = 2025, full = 2025-01-01..2027-12-31; paged reports at PageSize 100 plus a 10,000-row download.

## 1b. Re-run of 11 reports with the date window applied (`@InvoiceDate = InvoiceEffectiveDate`)
Run 2026-11-12 20:42 to 22:06 on the same local Standard copy, same machine conditions (free memory 5.7 to 7.2 GB). Raw data: `docs/results/standard_report_benchmark_datewindow.csv`. Report 7 year/full rows are missing from that CSV (two reports wrote to it at the same time); they are in the run log and quoted below. Single runs unless two shown.

| # | Report | narrow (s) | year (s) | full (s) | Earlier worst (s) |
|---|---|---|---|---|---|
| 6 | Accounts Receivable | 23.0 / 26.8 | 27.4 / 29.1 | 31.6 / 32.9 (10,000-row page 30.4 / 35.2) | 32.6 |
| 7 | Accounts Payable | 26.5 / 35.7 | 50.6 / 54.1 | 58.5 / 66.1 (10,000-row page 67.8) | 70.3 |
| 8 | Tax/Fee Passthrough | 43.6 / 55.7 | 73.2 | 69.5 (10,000-row page 84.4) | 113.0 |
| 16 | Earned vs Unearned | 73.4 | **TIMEOUT** (300 s) | not run | 300.4 (timeout) |
| 1 | Invoice Production | **TIMEOUT** (300 s) | not run | not run | 300.8 (timeout) |
| 13 | Underwriter Comm Statement | 87.5 | 104.4 | 100.4 | 111.1 |
| 3 | Direct Billed Comm Statement | 85.8 | 107.9 | 118.9 | 134.0 |
| 2 | Agency Billed Acct Statement | 110.8 | 138.5 | 165.0 | 233.9 |
| 9 | Insured Billed Comm Statement | 155.6 | 177.4 | 248.9 | 213.9 |
| 15 | Direct/Insured Comm Statement | 311.2 | 344.0 | 329.2 | 312.0 |
| 12 | Agency Detailed Statement | 0.1 to 0.8 | 0.1 | 0.1 | 0.86 (tiny result, not a valid load test) |

What this shows (local test only):
- The date window is now applied (matching rows grow with the window, for example Accounts Receivable 152,796 / 1,786,983 / 2,648,596). Cost grows with the window for the statement reports and Accounts Payable, but not proportionally: a one-month window still costs roughly 45 to 100% of the full range. Accounts Receivable is nearly flat (23 to 33 s).
- A one-month window still does not finish within 300 s for Invoice Production, and Earned vs Unearned only finishes for the one-month window. Direct/Insured Comm Statement takes about 5 minutes even for one month.
- Insured Billed Comm Statement full (248.9 s) is slower than the earlier 213.9 s, within the run-to-run variation noted above.
- Unchanged and not re-run: reports 4, 5, 10, 11, 14, 17 (they use their own date parameters) and the page benchmark.

## 2. Pages and searches (15 calls), slowest first
| Call | Time (s) | Rows | Logical reads | CPU (s) | Result |
|---|---|---|---|---|---|
| Make payments entities (agency, page 1) | 300.2 | 0 | 2.7M | 570 | **TIMEOUT** |
| AP search, page 1 (outstanding) | 278.2 | 50 | 70.6M | 976 | OK |
| Transaction history by PolicyNumber | 198.4 | 0 | 94.1M | 216 | OK |
| AP search by InvoiceNo (exact) | 154.7 | 2 | 5.3M | 245 | OK |
| Transaction history by InvoiceNumber | 96.6 | 0 | 49.3M | 116 | OK |
| AR search, page 1 (outstanding) | 66.6 | 100 | 23.7M | 374 | OK |
| Transaction history, page 1 | 24.0 | 50 | 0.3M | 65 | OK |
| Payments search, page 1 (all dates) | 15.6 | 50 | 0.1M | 48 | OK |
| AR search by PolicyNumber (exact) | 4.6 | 0 | 0.0M | 8 | OK |
| AR search by InsuredName (partial) | 4.1 | 25 | 0.1M | 7 | OK |
| AR search by InvoiceNo (exact) | 2.3 | 5 | 0.0M | 10 | OK |
| Payments search by ReferenceNumber | 2.3 | 0 | 0.1M | 8 | OK |
| Invoice lookup by number (exact) | 0.54 | 1 | 0.0M | 1 | OK |
| Policy details: policy | 0.01 | 1 | 0.0M | 0 | OK |
| Policy details: invoices of policy | 0.01 | 3 | 0.0M | 0 | OK |

## 3. Observations (evidence-based; plan-level causes are in `Iteration1_SlowQuery_Analysis.md`)
1. Three reports (BDX, Earned vs Unearned, Invoice Production) did not finish within 300 s even for a one-month window. Payment Report also timed out in this run; see item 8.
2. With the date window applied (section 1b), the date range changes cost moderately: a one-month window costs roughly 45 to 100% of the full range, depending on the report. Narrowing the window alone does not fix the slow reports.
3. Total CPU is several times the wall time for most slow items (parallel plans do more total work). The same designs that read millions of pages still do so (for example 21M to 53M reads for the statement reports, 70.6M for the AP search).
4. Transaction Report reads 176.3M pages to return 100 rows.
5. Statement reports (2, 3, 9, 13, 15) are unpaged and return up to 2.1M rows in one result set.
6. AP search is far slower than AR search (an exact invoice-number search takes 155 s vs 2.3 s for AR).
7. No index leads with `InvoiceNo`, so invoice-number lookups scan (about 33K reads); fine now, grows with data. Transaction-history-by-number and make-payments-entities returned 0 rows after heavy reads. **Unverified:** these may be wrong test inputs rather than defects. Not yet investigated.
8. Payment Report: the timeout is caused by a missing index on `Accounting_PolicyDetails (InsuredDetail_Id)`. With that index added on the test copy the unchanged procedure ran in 2 to 6 s (details in the analysis document). Applying it to the application database needs approval.

## 3a. Invoice numbers shown in reports: verification
The harness captured invoice-number columns from report output (capped at 20,000 distinct values; this is a sample, not every row shown). Each captured number was checked against `Accounting_Invoices` (read-only):

| Check | Result |
|---|---|
| Distinct numbers captured | 20,000 |
| Not found in `Accounting_Invoices` | 0 |
| Not `INV` + 11 digits (14 chars) | 0 |
| Matching invoices belonging to another/NULL client | 0 |
| Matching invoices with no policy | 0 |
| AR-direction line total differs from invoice amount (> $0.01) | 0 |
| Unpaid, unapplied invoices with RemainingBalance <> Amount | 0 |

Note: an invoice's `Amount` equals the sum of its **Accounts Receivable** lines only; payable and internal lines are additional, so summing all lines does not equal `Amount` (that is by design, not a defect).
Limitation: the sample does not record which report each number came from, and reports 12 and 10 return no invoice rows.

## 4. Limitations and open items
- Reports 10, 11 and 12 are not valid load tests (tiny or empty results; for 1099 the cause is explained in section 1). Reports 12, 13, 15 have no `LU_ReportTemplates` rows for the test client.
- Four stored procedures referenced from C# are missing in the database (`GetAllInvoicesBasedOnLineItemId`, `GetPaymentsByEntity_Id`, `GetDynamicFilterSQLExpressions`, `Update_LU_LineItems_Directions`).
- Not yet done: Query Store aggregation, investigation of the 0-row page results, repeating the runs on an otherwise idle machine.
- The `max server memory` cap on `MSSQLSERVER2022` is still 12,288 MB; revert instructions are at the end of `DB_Change_and_TestData_Log.md`.

## 5. Approval status
- **Already approved (Iteration1_Answers.md):** finite, evidence-based command timeouts; report file-delivery change with identical contents.
- **Needs approval (not applied):** changing application procs `InsertInstallmentInvoices`/`InsertUpdateInvoices` (hardcoded length 6) and `USP_Accounting_GenerateInvoiceNo` padding; index changes (including an `InvoiceNo` index and the `PolicyDetails (InsuredDetail_Id)` index); rebuilding the fragmented Invoices index; any code change.
