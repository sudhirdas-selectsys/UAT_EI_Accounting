# Iteration 1: Decision Summary

**Scope and labelling.** Every number here is a **local test finding** from the restored copy `uat-accounting-perf` (SQL Server 2022 Standard, about 2.49M synthetic invoices, one shared workstation). Nothing is a production measurement or a production prediction. **No change has been applied outside that test copy**, and the source database `uat-accounting` was not modified. Details and raw data are in `Iteration1_Benchmark_Results.md`, `Iteration1_SlowQuery_Analysis.md` (Addenda 1 to 9) and `DB_Change_and_TestData_Log.md`. No connection strings or credentials appear in these documents.

## Bottom line
- Three slow calls have a **tested, output-verified fix** on the copy: Payment Report (one index), Invoice Production (procedure rewrite V4) and Transaction history by PolicyNumber (procedure rewrite V2). A fourth group of "problems" (1099, page calls with 0 rows, missing date window) turned out to be test-input errors and is closed (section 5).
- The causes are mostly the same few patterns: optimizer row estimates far below actual, whole-table copies into temp tables before filtering or paging, per-row string aggregation, and a missing join index.
- Several slow calls still have no tested fix (section 3).
- Everything in section 1 needs your approval before it goes anywhere near the application.

## 1. Changes proven on the test copy (each needs your approval to apply or recommend)

| # | Change | Local test result | Verified how | Not tested / risk |
|---|---|---|---|---|
| 1 | **Index** `Accounting_PolicyDetails (InsuredDetail_Id, Client_Id) INCLUDE (Status)` (26 MB, built in 2.8 s) | Payment Report, page 1 of 100: **111 to 121 s down to 2 to 6 s**, procedure unchanged | Row totals unchanged; unpaged output identical (0 differing rows) | Write cost of a 12th index on `PolicyDetails`, concurrent load, other procedures |
| 2 | **Invoice Production rewrite (V4)**: stage page-only data after paging, plus a clustered index on one staging temp table | No filters: **531.6 s down to 63.9 s**. LOB filter: 519.5 s (V2) down to 244.5 s. Insured-name filter: no gain (255.9 s vs 256.6 s) | Row-hash multiset compare, 0 differences in all three cases (cases 2 and 3 compared with the V2 prototype, not the original) | One 3-day window; other filters, wider windows, large pages and concurrency untested; V4 slower than V3 with no filters (63.9 s vs 35.8 s), not investigated |
| 3 | **Transaction history by PolicyNumber rewrite (V2)**: stage matching policies first, then join from them | Policy search: **136.7 s down to 1.2 s** (4 rows); 9 combined-filter cases: original 30 to 90 s, rewrite 0.6 to 4.4 s | 0 row-hash differences in all 9 combined cases and 2 of 3 earlier cases; the third (page 2 of ties) differed only in tie order, and the full unpaged result was identical | One policy set; concurrency untested; the invoice-number case returned 0 rows in both |

**Recommendation.** Change 1 is the smallest and best supported; test its write cost and apply it to `uat-accounting` first if you agree. Changes 2 and 3 are procedure changes for the application team; test the remaining filter combinations and a wider window before recommending them.

**Delivered (2026-11-13):** the migration script for change 1 is in the repo as `Accounting.Web/SQLMigrations/[11132026] PolicyDetails InsuredDetail_Id Index.sql` (additive, re-runnable, revert command in its header). It has only been syntax-checked (parse-only); it has not been run anywhere, and its write cost is still unmeasured. Applying it to any shared UAT or production database is a separate step for you or the application team.

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
2. Decide whether changes 2 and 3 go to the application team, and which extra tests you want first (other filter combinations, a wider date window, concurrency).
3. Pick the next slow call from section 3 to analyse (Make payments entities and the AP search have the clearest plan evidence so far).
4. Repeat the headline timings on an idle machine, then revert the memory cap when testing ends.
