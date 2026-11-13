# Iteration 1: Decision Summary

**Scope and labelling.** Every number here is a **local test finding** from the restored copy `uat-accounting-perf` (SQL Server 2022 Standard, about 2.49M synthetic invoices, one shared workstation). Nothing is a production measurement or a production prediction. **No change has been applied outside that test copy**, and the source database `uat-accounting` was not modified. Details and raw data are in `Iteration1_Benchmark_Results.md`, `Iteration1_SlowQuery_Analysis.md` (Addenda 1 to 9) and `DB_Change_and_TestData_Log.md`. No connection strings or credentials appear in these documents.

## Bottom line
- Two slow calls have a **tested, output-verified fix** on the copy: Payment Report (one index; re-confirmed after a statistics refresh: 123.5 s without it, 2.7 s with it) and Invoice Production (procedure rewrite V4). A third candidate, Transaction history by PolicyNumber, **did not hold up** (see change 3). A further group of "problems" (1099, page calls with 0 rows, missing date window) turned out to be test-input errors and is closed (section 5).
- The causes are mostly the same few patterns: optimizer row estimates far below actual, whole-table copies into temp tables before filtering or paging, per-row string aggregation, and a missing join index.
- Several slow calls still have no tested fix (section 3).
- Everything in section 1 needs your approval before it goes anywhere near the application.

## 1. Changes proven on the test copy (each needs your approval to apply or recommend)

| # | Change | Local test result | Verified how | Not tested / risk |
|---|---|---|---|---|
| 1 | **Index** `Accounting_PolicyDetails (InsuredDetail_Id, Client_Id) INCLUDE (Status)` (26 MB, built in 2.8 s) | Payment Report, page 1 of 100: **111 to 121 s down to 2 to 6 s**, procedure unchanged | Row totals unchanged; unpaged output identical (0 differing rows) | Write cost of a 12th index on `PolicyDetails`, concurrent load, other procedures |
| 2 | **Invoice Production rewrite (V4)**: stage page-only data after paging, plus a clustered index on one staging temp table | No filters: **531.6 s down to 63.9 s** (re-run after the statistics refresh: 546 s down to 46.3 s). LOB filter: 519.5 s (V2) down to 244.5 s. Insured-name filter: no gain (255.9 s vs 256.6 s) | Row-hash multiset compare, 0 differences in all three cases (cases 2 and 3 compared with the V2 prototype, not the original) | One 3-day window; other filters, wider windows, large pages and concurrency untested; V4 slower than V3 with no filters (63.9 s vs 35.8 s), not investigated |
| 3 | **Transaction history by PolicyNumber rewrite (V2): NOT RECOMMENDED, baseline was unstable** | Earlier today the original took 130 to 137 s and V2 about 1.2 s. After the statistics on `Accounting_PolicyDetails` were refreshed (01:06, 6% sample; likely triggered by my own write-cost test, not proven), the original takes **2.8 to 3.7 s** and starts from a PolicyDetails seek instead of a scan of all invoices | Plan re-captured; the merged single-procedure form returned 0 row-hash differences in 2 cases | The slow plan came from stale statistics, so the rewrite gain is not supported. No migration script was written. Whether production statistics could be in the same state is unknown |

**Recommendation.** Change 1 is the smallest and best supported; apply it first. Change 2 (Invoice Production) is a procedure change for the application team after the remaining filter combinations are tested. For change 3, check statistics freshness on `Accounting_PolicyDetails`, `Accounting_Invoices` and the payment tables on the real database before any rewrite.

**Delivered (2026-11-13):** the migration script for change 1 is in the repo as `Accounting.Web/SQLMigrations/[11132026] PolicyDetails InsuredDetail_Id Index.sql` (additive, re-runnable, revert command in its header). It has only been syntax-checked (parse-only); it has not been run anywhere. Write cost on the test copy (inserting 100,000 policy rows, 3 runs each): mean 31.7 s with the index vs 27.4 s without, about 16% slower on average, with overlapping ranges (27.2 to 36.3 s vs 24.8 to 29.5 s); updates and concurrent load were not measured. Applying it to any shared UAT or production database is a separate step for you or the application team.

**Also delivered (2026-11-13):** `Accounting.Web/SQLMigrations/[11132026] Invoice Production Report Performance.sql` (the tested V4 logic as an ALTER of the live procedure; header lists the evidence, the untested cases, and a warning to diff against the deployed definition first) and the revert script `docs/seed/18_InvoiceProduction_OriginalDefinition_Revert.sql`. Both were syntax-checked (parse-only) and not run anywhere. The no-filter case was repeated after the statistics refresh (2026-11-13, same 3-day window, one run each): original 546 s, V4 46.3 s, 18,458 rows each, 0 row-hash differences (Addendum 8, re-check). Other filters, wider windows and concurrency remain untested.

### Not recommended from the evidence
- **Candidate indexes on `InvoiceDetails` and `PolicyDetailsLineItems`** (Addendum 6): 790 MB, 16 to 28% slower inserts, and no read gain (AP and AR searches got slower). Both are still on the copy.
- **Payment Report join rewrite** (Addendum 3): it helps only until change 1 exists, then adds nothing.

### Latent issue found (existing, not introduced by any change here)
The paged Transaction history query orders only by `CreatedOn`. Rows loaded in one batch share a timestamp, so page contents inside a tie are not defined and can differ between plans. A unique tie-breaker column would fix it; that is a decision for the application team.

## 2. Earlier approvals and open approvals

- **Already approved** (`Iteration1_Answers.md`): finite, evidence-based command timeouts; report file-delivery change with identical contents. They are not part of the test work described here.
- **Needs approval, nothing applied:**
  - the three changes above;
  - `InsertInstallmentInvoices` and `InsertUpdateInvoices` hardcode an invoice-number length of 6, and `USP_Accounting_GenerateInvoiceNo` padding;
  - an `InvoiceNo` index (no index leads with `InvoiceNo`; lookups scan about 33K reads today and grow with data);
  - rebuilding the 88.3% fragmented Invoices clustered index on the test copy (a side effect of renumbering the test data);
  - any application code change.

## 3. Slow calls without a tested fix (local test, Standard copy)

| Call | Worst time | What is known |
|---|---|---|
| Direct/Insured Comm Statement (report 15) | 311 to 344 s, even for one month | No plan analysis in Addenda 1 to 9 as far as I can tell; not analysed here |
| BDX (14) | timeout (300 s) | Early analysis (Addenda 1 and 2) has the plan patterns; no tested fix |
| Earned vs Unearned (16) | one month 73 s; year window timeout | No tested fix |
| AP search page 1 | 278 s | Searches are far slower than AR (155 s vs 2.3 s for an exact invoice number); no tested fix |
| Make payments entities | timeout (300 s) | Whole-table temp copy of 15.3M detail rows, estimates far below actual (Addendum 2); no tested fix |
| Transaction Report (5) | 125 s | Reads 176.3M pages to return 100 rows |
| Statement reports (2, 3, 9, 13) | 86 to 249 s | Unpaged, up to 1.7M rows; one-month window still 45 to 100% of the full-range cost |
| Invoice Production, one month | timeout (300 s) | Fix 2 was tested only on a 3-day window (18,458 invoices), not on one month |

## 4. Open questions about the results themselves
- Why V2 of Invoice Production did not show V3's LOB-filter slowdown.
- Why V4 is slower than V3 with no filters.
- Run-to-run variation reached about 35% on a shared machine; the key timings have not been repeated on an idle one.
- Query Store aggregation has not been done.
- The source database `uat-accounting` has none of these changes, so nothing has been measured there.

## 5. Test-data and harness corrections found (no longer open)
- **1099 report** returned 0 rows because it filters on agent creation date and the test data had no outgoing agent payments. Payouts were seeded on the copy (change log #18); 1099 now takes about 22 s for the full window only.
- **Page calls with 0 rows** were harness input errors (0-based paging, wrong bill type, a nonexistent reference), now fixed and re-run.
- **Date window:** 11 reports were re-run with `@InvoiceDate` applied; narrowing the date window alone does not fix the slow reports.

## 6. Objects and settings left on the test copy (nothing dropped; revert is your decision)
Indexes `IX_PerfTest_PolicyDetails_InsuredDetail_Id`, `IX_PerfTest_InvoiceDetails_BLD_Status_Client`, `IX_PerfTest_PolicyDetailsLineItems_LineType`; prototype procedures for Payment Report V2, Invoice Production V2/V3/V4 and Transaction history V2; seeded agent payouts (references starting `BillPT`); instance `max server memory` capped at 12,288 MB on `MSSQLSERVER2022`. Revert commands are in `DB_Change_and_TestData_Log.md` and none has been run.

## 7. Suggested order
1. Decide on change 1 (index). If approved, test write cost, then apply to `uat-accounting` and re-measure Payment Report there.
2. Decide whether change 2 (Invoice Production) goes to the application team, and which extra tests you want first (other filter combinations, a wider date window, concurrency). Its no-filter case was re-run with fresh statistics and held (546 s vs 46.3 s). Check statistics freshness on the real database for the Transaction history case.
3. Pick the next slow call from section 3 to analyse (Make payments entities and the AP search have the clearest plan evidence so far).
4. Repeat the headline timings on an idle machine, then revert the memory cap when testing ends.
