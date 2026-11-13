# Iteration 1: Slow Query Analysis

**Labelling.** Local test-environment findings only (SQL Server 2022 Standard test copy, about 2.49M invoices, fragmented Invoices index). Not production measurements. SQL Server Express is not used in UAT or production, so this document makes no comparison with Express. Addendum 1 was captured on an earlier local test engine; it is kept only as evidence of the plan patterns (row-count estimates, temp-table staging), and its timings are not used for any conclusion. Evidence sources: Query Store (estimated plans, runtime stats), stored-procedure source read from the local DB (read-only), benchmark CSVs in `docs/results/`. **Root causes below are evidence-based hypotheses; none has been proven by testing a fix.** No change has been applied.

## Cross-cutting causes (seen in many slow procedures)
| # | Pattern | Evidence | Where |
|---|---|---|---|
| X1 | **Catch-all optional filters**: `(@p IS NULL OR col = @p)` in one static statement. One cached plan must serve every parameter combination, so it cannot seek on the supplied filter. | Source text. `OPTION (RECOMPILE)` appears only in Invoice Production. | Transaction History, Earned/Unearned, statements, 1099, AR/AP |
| X2 | **Leading-wildcard search**: `InvoiceNo LIKE '%' + @x + '%'`, `PolicyNumber LIKE '%...%'`, `Name LIKE '%...%'`. Cannot use an index even if one existed. | Source text; no index leads with `InvoiceNo` (verified earlier). | Transaction History (lines 43, 48, 49), AP/AR search, Invoice Production |
| X3 | **Date range barely changes cost**: reports cost the same for 1 month as for 3 years, so the filter is applied late. | Benchmark: narrow/year/full times within about 10% for reports 3, 9, 13, 15. | most statements |
| X4 | **Large temp-table staging**: whole tables copied into `#temp` first (for example `SELECT AID.* INTO #TempAccounting_InvoiceDetails_Inv...`: 15.3M-row InvoiceDetails scan), then filtered. 12 to 17 temp tables per statement report, few or no indexes on them. | Query Store statement text and plans (`Clustered Index Scan InvoiceDetails 15,308,000 rows`). | Invoice Production, Tax/Fee, Paid BDX, statements |
| X5 | **Withdrawn.** Serial plans were an artifact of the first test engine and do not apply to the Standard test copy (parallel plans run there, DOP 10). | - | - |
| X6 | **Float money columns and many joins with `Status` flags in every join**: join predicates carry `Status = 1` and `Client_Id` on every table; clustered keys lead with `Status`. | Schema (D2/D3) and source. | all |
| X7 | **Unpaged results**: statement reports return up to 2.1M rows in one result set. | Benchmark rows. | reports 2, 3, 9, 13, 15 |

## Per-item analysis (slowest first)
| Item | Time | Evidence | Likely cause (hypothesis) | Candidate change (needs approval) |
|---|---|---|---|---|
| **Earned vs Unearned** (rpt 16) | TIMEOUT >300 s | Plan: clustered scan of `Accounting_Carrier_GroupingDetails` (cost 1,219) feeding nested loops; 14 CONVERTs; 5 temp tables; `STRING_SPLIT` + `TRY_CAST` filters (lines 100, 104); `GetAgencyHierarchy` table function (line 101) | Early joins of 8 tables plus InvoiceDetails before date/agent filters (X1, X4); nested loops over 1M+ rows | Pre-filter invoices by date and client into an indexed temp table before joins |
| **BDX** (rpt 14) | TIMEOUT >300 s (0 reads recorded: it did not complete a statement within the limit or was not captured) | 35 KB procedure, 4 ISNULL-on-column predicates, 17 CONVERT/CAST | Not investigated further. Needs a longer-timeout run with the actual plan | Run once with a 15-minute timeout and capture the actual plan |
| **Make payments entities** | 218 s, CPU 209 s, only 2.7M reads | 57 KB procedure, 41 temp tables, 22 ISNULL-on-column, 9 CONVERT | CPU-bound: row-by-row style logic or heavy compute on in-memory temp tables, not I/O | Needs actual plan; do not guess |
| **AP search** (page 1: 202 s; by invoice no: 99 s) | 72M reads for page 1 | Dynamic SQL builds `INV.InvoiceNo LIKE '%'+@InvoiceNo+'%'` (line 449); 17 temp tables; same shape for AR but AR by invoice no takes 1.1 s | AP path materializes all outstanding AP rows first (X4), then filters; AR path filters earlier | Filter by invoice number before building the staging table; align AP with AR logic |
| **Direct Billed Comm Statement** (rpt 3) | 289 s, 50M reads | Query Store: one statement accounts for 749 s of total time; the same stats pattern as rpts 9, 15 (shared template) | X3, X4, X7: full staging, date range barely used, 854K rows returned unpaged | Push date filters into the first staging query; consider paging |
| **Invoice Production** (rpt 1) | 272 s, 180M reads for 100 rows | Plan: nested loops, 2,010,700 estimated rows, subtree cost 2,194,240 (the largest in the system); correlated `STUFF(... FOR XML PATH)` string aggregation (lines 232-237); invoice filter `= @x OR LIKE '%'+@x+'%' OR TRY_CAST(REPLACE(InvoiceNo,'INV','') AS INT) = ...` (line 256); paging applied after all rows built | Per-invoice correlated XML aggregation over 2M invoices; non-sargable invoice filter; page applied last | Apply paging before string aggregation; replace the three-way invoice filter with one exact/prefix match |
| **Direct/Insured Comm Statement** (rpt 15) | 264 s, 53M reads | Plan: 1.27M-row hash match, nested loops over 607K rows, Index Spool (cost 1,370) | Same template as rpt 3 plus insured-bill branch; spool indicates repeated rescans | Same as rpt 3 |
| **Paid BDX** (rpt 17) | 152 s, only 1.0M reads | Plan: hash matches 0.8M to 4.6M rows; InvoiceDetails full scan 15.3M rows; CPU 143 s | CPU-bound hashing of millions of rows to return 100 | Filter before hashing; paging earlier |
| **Transaction History by Policy / Invoice number** | 121 s / 78 s (102M / 59M reads) | Source lines 48 and 49: `LIKE '%'+@x+'%'` repeated across three UNION branches; X1 catch-all | Leading-wildcard scans over 4.6M transaction rows in every branch | Exact/prefix match when the full number is supplied; index on `InvoiceNo` |
| **Transaction Report** (rpt 5) | 42 to 110 s; 63M reads for 0 rows in narrow windows | Plan: nested loops and hash matches (cost about 300) on 17,838 estimated rows; 0 returned | The date window is applied after the join, not before | Move the date predicate to the first table read |
| **Insured Billed / Agency Billed / Underwriter statements** | 58 to 120 s | Same family | As rpt 3 | As rpt 3 |
| **AR search page 1** | 66 s, 24M reads | X1, X4 | As AP search | |

## Corrections to earlier statements
- The "0 rows" shown for Transaction History by number and for Make payments entities may be a harness reading artifact: the `TotalRecords` output returned 5 and 2 for those calls, so matches exist. The row count column comes from the first result set only. **Still to confirm.**
- Earlier statements about edition memory limits are withdrawn: they referred to the first test engine and are not relevant to the Standard test copy.

## What is NOT yet known
- Actual (as opposed to estimated) plans: Query Store holds estimated plans only. Actual plans for the top 9 items would confirm each hypothesis. Each run is 1 to 5 minutes.
- Why plans are serial (X5).
- Behaviour after an index rebuild (the Invoices clustered index is 88% fragmented).
- Behaviour on a production-sized server and under concurrency.

## Proposed next steps (none started; all need your approval except analysis)
1. Capture actual plans for the top 9 items (read-only, runs one call at a time with `SET STATISTICS XML ON`, about 30 to 45 min total).
2. Rebuild the Invoices clustered index (and re-run the benchmark) to separate fragmentation effects from query-design effects. This is a data-structure change to the test database: needs approval.
3. Prototype fixes on **copies** of the slowest procedures (new names, test DB only), compare results row for row with the originals, and re-measure. Needs approval; the originals stay untouched.
4. Application-side (already approved in principle): finite command timeouts and file-based report delivery, sized from these measurements.

---

# Addendum 1: ACTUAL execution plans (captured 2026-11-11 on the first test engine, narrow window; plan patterns only, timings not used)
Method: `docs/seed/07_CapturePlans.ps1` ran 13 slow calls once each with `SET STATISTICS XML ON` (read-only, actual row counts and per-operator CPU/elapsed). Plans are stored locally in the scratchpad (not in the repo; 9.8 MB). Wall times include plan-collection overhead. All 13 completed (no timeouts at the longer 20 to 30 minute caps).

## Note on this capture
All plans in this addendum were serial because the engine used for the first capture cannot generate parallel plans. The wall times in the table below are therefore not used for conclusions; the operator-level findings (row-count estimates versus actual rows, temp-table staging, per-invoice aggregation) are what matter, and the same patterns were confirmed on the Standard test copy (Addendum 2 onward).

## Measured causes per call (actual rows vs optimizer estimate)
| Call | Wall (capture) | Dominant cost (from the plan) | Estimate vs actual |
|---|---|---|---|
| R14 BDX | 593 s | One INSERT into a temp table: nested loops and sort over 1,993,600 rows, 582 s elapsed (339 s CPU) | est 11,594 vs actual 1,993,600 (about 170x under) |
| R16 Earned vs Unearned | 467 s | Hash joins over **15,308,000** InvoiceDetails rows, 275 s CPU, serial | est 55,477 vs actual 15,308,000 (about 275x under) |
| R01 Invoice Production | 312 s | Correlated `STUFF(... FOR XML)` executed per invoice: 2,492,000 iterations, 185 s of 312 s (nested loops, UDX, sort) | est 2,043,240 actual 2,492,000 (estimate fine; the pattern itself is the cost) |
| R03 Direct Billed | 272 s | Nested loops and hash joins building a temp table, 218 s elapsed | est 17,873 vs actual 498,400 (about 28x under) |
| R15 Direct/Insured | 257 s | Index Spool executed 1,246,000 times (67 s) plus 110 s temp-table insert | spool est 1 vs actual 1,246,000 |
| P2a AP search page 1 | 244 s | Nested loops feeding `#APResults`: 1,691,000 rows, 123 s | est 23,681 vs actual 1,691,000 (about 70x under) |
| P5a Make payments entities | 242 s | Many 4.6M-row temp-table inserts (20 to 29 s each); returns 0 rows | est 0.8M to 2.7M vs actual 4.6M |
| R17 Paid BDX | 124 s | Hash/sort/aggregate over 1.7M to 1.5M rows, 39 s each, 3 spill warnings | est 2.6M vs actual 0.58M (over) |
| P2b AP search by invoice no | 118 s | Builds a 15.3M-row temp copy of InvoiceDetails (16 s + index inserts 10 to 11 s) then a 1.25M-row aggregate, **before** applying the invoice-number filter | est 1 vs actual 1,246,000 |
| P1a AR search page 1 | 110 s | Nested loops and hash chain producing 1,530,759 rows into a temp table (102 s) | est 13,199 vs actual 1,530,759 (about 115x under) |
| R05 Transaction Report | 78 s | The `SET @TotalRecords = (SELECT COUNT(*) ...)` statement alone takes **75 s**, and returns 0 for this window | est 8,522 vs actual 0 |
| P4c / P4b Transaction History by policy / invoice | 24 s / 5.6 s | Now far faster than the first benchmark run (121 s / 78 s). Actual operators total only 2.5 to 13.5 s. | est 100, actual 2 to 5 |

**Main pattern: the optimizer underestimates intermediate row counts by 30x to 275x** (estimates of thousands against actual millions), so it picks nested-loop and spool strategies suited to a few rows. This is typical when many `Status`/`Client_Id`/optional-parameter predicates and temp tables without statistics hide the true cardinality. This is a hypothesis consistent with the plans; fixing it has not been tested.

## Other measured findings
- **Spill warnings** (sort/hash writing to tempdb) occur in 10 of 13 calls (most: R03 14, P5a 10, R17 12, P2a 8, R01 8, R14 8).
- **Missing-index hints** appear in 11 of 13 calls. Highest-impact hints are on `Accounting_InvoiceDetails` (impact about 99%), `Accounting_Invoices` (about 42 to 46%) and on **temp tables** (impact 90 to 99.9%, for example `#APResults`, `#ARResults`, `#TempBasePolicyDetails_InvProd`, `#TempAccounting_InvoiceDetails_DirectBill`). The temp-table hints mean the procedures read their own staging tables without indexes. SQL Server's hints are advisory and the exact definitions must be reviewed before any index is proposed.
- **Whole-table copies**: the AP search by invoice number copies all 15.3M InvoiceDetails rows into a temp table before filtering to 2 results (about 30 s of the 118 s).
- **Transaction History**: the earlier 78 s and 121 s were largely cold-cache effects. Warm, they take 5.6 s and 24 s. Wildcard `LIKE` is still present. The earlier benchmark numbers for these two calls overstate steady-state cost, and the cold-versus-warm effect has not been measured separately for the other calls.
- Transaction History by number returns matches (5 and 2) while the harness result sets read 0 rows, so those "0 rows" are a harness reading artifact, not a defect. (Confirms the earlier suspicion.)

## Revised candidate changes (still NOT applied; all need approval)
1. Index the staging (temp) tables the procedures build, or build them with a clustered key, and pre-filter by client and date before the big joins (addresses the hints on `#APResults`, `#ARResults`, `#TempBase*`).
2. Replace the per-invoice `STUFF(... FOR XML)` in Invoice Production with a set-based aggregate, and apply paging before it.
3. Stop copying all of InvoiceDetails into a temp table for single-invoice lookups (AP/AR search by invoice number).
4. Add `OPTION (RECOMPILE)` or split the catch-all predicates for the procedures that use `(@p IS NULL OR ...)`, so the plan matches the supplied filters.
5. Count query in Transaction Report: restrict by date before counting.
6. Consider candidate indexes on `Accounting_InvoiceDetails` and `Accounting_Invoices` from the hints, tested on the copy first.
7. (Done: the full benchmark was re-run on the Standard test copy; see `Iteration1_Benchmark_Results.md`.)
Re-measurement plan: prototype on renamed copies in the test DB only, compare output row for row with the originals, then re-time.

---

# Addendum 2: actual plans on SQL Server 2022 Standard (2026-11-12)
Method: `docs/seed/07_CapturePlans.ps1` against the restored copy `uat-accounting-perf` (narrow window, 900 s cap; the machine was shared and memory-limited, see the limits in `Iteration1_Benchmark_Results.md`). Three of the slowest calls were captured: Payment Report, Make payments entities, Invoice Production. Plan files are in the scratchpad (not in the repo). Capture wall times: Payment Report 146 s, Make payments entities 332 s (completed, returned no rows in the harness), Invoice Production 517 s.

## Payment Report (133 to 300 s): cause found in the plan
- Statement 1 plan, DOP 10. A **Lazy Table Spool over `Accounting_PolicyDetails` (est 890,000 rows) was rewound 727 times and returned 652,237,503 rows** (643M read), about 1.18M ms of CPU, and the surrounding nested loops about 1.37M ms CPU. The optimizer estimated the outer side at about 200 rows (actual 1,164).
- Source: the procedure joins payment to insured to policy with `LEFT JOIN Accounting_PolicyDetails APD ON ins.InsuredDetail_Id = APD.InsuredDetail_Id AND APD.Status=1 AND ins.Client_Id = APD.Client_Id` (lines 62 and 112), after a chain of `LEFT JOIN ... ON x.Id = p.Entity_Id AND p.EntityType_Id = n` per entity type. With the row count underestimated, the 2022 optimizer chose to replay a spooled copy of all policies for each outer row (about 737 x 890,000) instead of looking policies up by insured.
- This is a plan choice that depends on the optimizer's row estimate rather than on a stable property of the procedure. **Not yet tested at this point:** that an index on `PolicyDetails (Client_Id, InsuredDetail_Id)` or a join rewrite removes the spool (no change was made).

## Make payments entities (timeout at 300 s, 332 s with capture)
A 15.3M-row temp copy of InvoiceDetails built with an index before insert (Index Insert 39 s, Sort 30 s), then several 4.6M-row temp-table inserts (each 30 to 50 s elapsed even with parallelism). Estimates remain far under actual (35,007 estimated vs 4,635,147 actual rows on one insert). Parallelism (DOP 10) reduces some wall time but adds total CPU.

## Invoice Production (timeout at 300 s, 517 s with capture)
The per-invoice `STUFF(... FOR XML PATH)` runs for all 2,492,000 invoices, in a parallel plan: about 233 s elapsed and about 937,000 ms CPU for that statement alone. Estimate 35,305 vs actual 2,492,000. Parallelism does not help this construct because the work is per-invoice string aggregation.

## Other observations
- Plan reason codes on Standard: `MaxDOPSetToOne` 16 and 7 statements in the Make payments and Invoice Production plans. The procedures contain **no `MAXDOP` hint** (searched all 16 exported procedures), so the reason is not explained yet.
- Spill warnings: 7 statements (Make payments), 5 (Invoice Production).

## Updated conclusions
1. The plan patterns hold on the Standard test copy: row-count underestimates (30x to 275x), per-invoice aggregation, whole-table temp copies and unindexed staging tables.
2. Parallel plans do not remove these causes, and plans depend heavily on row estimates (Payment Report).
3. Candidate fix for Payment Report (needs approval, not applied): test an index on `Accounting_PolicyDetails (Client_Id, InsuredDetail_Id)` and/or restructure the LEFT JOIN chain on a renamed copy of the procedure, compare output row for row, and re-time.

---

# Addendum 3: Payment Report join rewrite, test result (Standard copy only, 2026-11-12)
Test object: `dbo.USP_Accounting_PaymentReport_PerfTest_V2`, created only in `uat-accounting-perf` on `MSSQLSERVER2022` (`docs/seed/08_PaymentReport_PerfTest_V2.sql`). The original procedure and the source database were not changed. The rewrite replaces each `LEFT JOIN Accounting_PolicyDetails APD ... LEFT JOIN Accounting_Invoices I ...` pair (three places) with one `OUTER APPLY` that returns the same policy/invoice rows per insured (including one NULL row when there is none). Everything else is the original text.

**Output check (narrow window, unpaged):** original and rewrite each returned 155,750 rows; `EXCEPT` in both directions found 0 differing rows; `@TotalRecords` matched (155,750). Unpaged timing: original 15.7 s, rewrite 19.7 s (the unpaged call is not the slow case).

**Paged timing (page 1, page size 100, local Standard test, one run each, machine shared):**
| Window | Original | Rewrite | Rows | TotalRecords (both) |
|---|---|---|---|---|
| narrow (Jan 2026) | 111.2 s | **37.2 s** | 100 | 155,750 |
| year (2025) | 118.5 s | **41.7 s** | 100 | 1,703,779 |
| full (2025-2027) | 120.8 s | **40.0 s** | 100 | 2,753,651 |

About 3x faster for the same page size and identical total counts. The remaining cost is explained in 3b below. Limits: the 100 returned rows were not compared row by row (rows of one payment have no guaranteed order, so page boundaries can differ); single runs; no plan captured for the rewrite at this point. The remaining time is probably the `@TotalRecords` count query (it repeats the joins over all matching payments), not yet confirmed.

**Next candidates (not applied, need approval):** capture the rewrite's actual plan to find what remains; test an index on `Accounting_PolicyDetails (Client_Id, InsuredDetail_Id)`; apply the same OUTER APPLY pattern only where the same join chain exists. Any production change to the procedure would be a separate approval.

## Addendum 3b: actual plan of the rewrite (Standard copy, narrow window, capture wall 38.6 s)
- **The count statement is not the problem:** `SET @TotalRecords = (SELECT COUNT(*) ...)` ran in parallel (DOP 10) in about 3.6 s.
- **The paged select (about 35 s of the 38.6 s, serial, DOP 1) is dominated by one operator:** a Clustered Index Seek on `Accounting_PolicyDetails` (`CLU_Accounting_PolicyDetails_Status_Id`) that **read 108,578,616 rows to return 40**, with a 35.0 s elapsed. The optimizer estimated 1 row for it.
- **Cause (verified from the index list, read-only):** no index on `Accounting_PolicyDetails` has `InsuredDetail_Id` as a key. The join `ins.InsuredDetail_Id = APD.InsuredDetail_Id` therefore seeks only on the clustered key's first column (`Status = 1`, about 890,000 rows) and filters `InsuredDetail_Id` row by row. It runs once per outer row (about 122 executions x 890,000 = 108.6M rows). The original plan reached the same cost through a spool over the same table (652M rows served). Both plans are paying for the missing `InsuredDetail_Id` access path; the rewrite reduced the repetition but did not remove it.
- **Existing PolicyDetails indexes (11):** clustered `(Status DESC, Id)`, unique `(Id)`, and nine non-clustered indexes whose leading keys are `Client_Id`, `EffectiveDate`, `Status` or `Client_Id/Status`. None leads with `InsuredDetail_Id`.
- **Not applied, not yet tested:** a candidate index such as `PolicyDetails (InsuredDetail_Id, Client_Id) INCLUDE (Status)` on the Standard copy, with the same plan capture and timing repeated. If the cause is as described, both the original and the rewrite should drop sharply.

---

# Addendum 4: index test (Standard copy only, 2026-11-12)
Test object: `IX_PerfTest_PolicyDetails_InsuredDetail_Id` on `Accounting_PolicyDetails (InsuredDetail_Id, Client_Id) INCLUDE (Status)`, created only in `uat-accounting-perf` on `MSSQLSERVER2022` (`docs/seed/09_PerfTest_Index_PolicyDetails_InsuredDetail.sql`). Size 26 MB, built in 2.8 s. The source database and both procedures were not changed.

**Paged Payment Report, page 1, page size 100, local Standard test, one run each (shared machine):**
| Window | Original, before index | Original, with index | Rewrite, before index | Rewrite, with index | TotalRecords |
|---|---|---|---|---|---|
| narrow | 111.2 s | **2.1 s** | 37.2 s | **2.6 s** | 155,750 |
| year | 118.5 s | **4.3 s** | 41.7 s | **4.2 s** | 1,703,779 |
| full | 120.8 s | **6.2 s** | 40.0 s | **5.1 s** | 2,753,651 |

- **The index alone removes the problem.** The original procedure, unchanged, went from 111 to 121 s down to 2 to 6 s. The join rewrite adds nothing once the index exists, so it is not needed.
- **Row totals are unchanged** (155,750 / 1,703,779 / 2,753,651, same as before the index). Re-running the unpaged comparison after the index: original and rewrite each return 155,750 rows with 0 rows different in either direction. (Unpaged timing is unchanged, original 16.4 s vs rewrite 17.8 s; it was never the slow case.)
- This supports the diagnosis in Addendum 3b: the missing `InsuredDetail_Id` access path on `PolicyDetails` was the cause, on both the original's spool plan and the rewrite's seek plan.

**Not tested / limits:** write cost of the additional (12th) index on `PolicyDetails` (inserts and updates), index maintenance on the source test database, other procedures that might benefit or regress, and behaviour under concurrent load. Single runs only; plan not re-captured after the index.

**Decision needed (not applied outside the Standard copy):** whether to add the index to the source test database (`uat-accounting`) for re-measurement, and whether to recommend it for the application. Either needs your approval.

---

# Addendum 5: candidate indexes from all captured plans (read-only analysis, 2026-11-12)
Method: parsed the missing-index hints in the 13 earlier and 4 Standard actual plans (hints on temp tables excluded) and compared them with the existing indexes on the same tables (read from `uat-accounting-perf`, same as the source). Also looked for base-table operators that read more than 5M rows and 20x more than they return: only one exists (the `PolicyDetails` seek fixed in Addendum 4). **SQL Server's hints are advisory, come from estimated costs, were seen in plans for a narrow window only, and none of the items below except #1 has been tested.** Nothing was created for #2 to #6.

## Existing indexes on the three tables (for comparison)
- `Accounting_InvoiceDetails` (15.3M rows): clustered `(Status, Id)` 1,335 MB; `(Id)`; `(Invoice_Id, Status, LOB_Id)`; `(Invoice_Id, Status, EntityType_Id, Entity_Id)`; `(Invoice_Id, Client_Id, Status, Paid)`. **No index has `BillingType_LineItems_Directions_Id`.**
- `Accounting_Invoices` (2.49M rows): clustered `(Status, Id)`; `(Id)`; `(InstallmentNo, DueDate, Paid, Schedule_Id)`; `(PolicyDetails_Id, InvoiceNo, Client_Id, Status)`. No index leads with `InvoiceNo`, `Client_Id` or `Status` other than the clustered key.
- `Accounting_PolicyDetailsLineItems`: clustered `(Status, Id)`; `(Id)`; `(PolicyDetails_Id, Client_Id, Status)`. **No index has `LineType`.**

## Candidates
| # | Table | Suggested keys (plus a few INCLUDE columns) | Seen in | Hint impact | Status |
|---|---|---|---|---|---|
| 1 | `Accounting_PolicyDetails` | `(InsuredDetail_Id, Client_Id)` INCLUDE `(Status)` | Payment Report (orig and rewrite) | 100% | **Tested on the Standard copy: Payment Report 111-121 s to 2-6 s** (Addendum 4) |
| 2 | `Accounting_InvoiceDetails` | `(BillingType_LineItems_Directions_Id, Status [, Client_Id])` INCLUDE `(Invoice_Id, Amount, LOB_Id)` | AP search page and by invoice no (99%), Transaction Report (90%), AR search page (87%), BDX (81%), Paid BDX (59%) | 59% to 99% | Not tested. Important: the hints propose INCLUDE lists of up to 10 columns on a 15.3M-row table; a narrow version (3 to 4 included columns) would be about 0.5 to 0.8 GB (estimate, not measured) |
| 3 | `Accounting_PolicyDetailsLineItems` | `(LineType, Status, Client_Id)` INCLUDE `(PolicyDetails_Id, Premium)` | Invoice Production, Direct/Insured, Direct Billed | 50% to 61% | Not tested |
| 4 | `Accounting_Invoices` | `(Status, Client_Id)` INCLUDE `(PolicyDetails_Id, InvoiceNo)` | Transaction History by invoice and by policy number | 46% | Not tested. Overlaps the existing `(PolicyDetails_Id, InvoiceNo, Client_Id, Status)` and the clustered key; value unclear |
| 5 | `Accounting_PolicyDetails` | `(Client_Id, PolicyNumber, Status)` | BDX | 12% | Low impact; not tested |
| 6 | `Accounting_Payments`, `Accounting_InsuredDetails` | `(Status, Client_Id)` etc. | Payment Report | 9% to 11% | Low impact; probably unnecessary once #1 exists |

## Cautions
- The earlier finding that **no index leads with `InvoiceNo`** (invoice-number lookups scan about 32K pages) is separate from these hints and is not listed above because no plan hint was produced for the exact-match case.
- The 15.3M-row `InvoiceDetails` already has 5 indexes. Each added index costs storage, slows inserts and updates, and was not costed here. #2 needs a size and write-cost test before anyone recommends it.
- Several of the slowest reports (BDX, Earned vs Unearned, Invoice Production) are dominated by per-row aggregation and temp-table work, not by missing indexes; indexes alone are not expected to fix them.
- Recommended next step if you approve: test #2 (narrow version) and #3 on the Standard copy only, then re-time BDX, AP search, AR search, Invoice Production and Direct Billed, and measure index size and a bulk-insert cost.

---

# Addendum 6: test of candidate indexes 2 and 3 (Standard copy only, 2026-11-12)
Objects created only in `uat-accounting-perf` on `MSSQLSERVER2022` (`docs/seed/10b_PerfTest_Indexes_Candidates_2_3.sql`; write probe `docs/seed/10a_PerfTest_InsertCost.sql`):
- `IX_PerfTest_InvoiceDetails_BLD_Status_Client` on `Accounting_InvoiceDetails (BillingType_LineItems_Directions_Id, Status, Client_Id) INCLUDE (Invoice_Id, Amount, LOB_Id)`: **652 MB**, built in 101 s.
- `IX_PerfTest_PolicyDetailsLineItems_LineType` on `Accounting_PolicyDetailsLineItems (LineType, Status, Client_Id) INCLUDE (PolicyDetails_Id, Premium)`: **138 MB**, built in 24 s.
The source database and all procedures were not changed.

## Write cost (single runs, bulk insert copied from existing rows, rolled back, shared machine)
| Table | Rows | Before | After | Change |
|---|---|---|---|---|
| `Accounting_InvoiceDetails` | 200,000 | 20.8 s | 24.1 s | +16% |
| `Accounting_PolicyDetailsLineItems` | 100,000 | 5.4 s | 6.9 s | +28% |
Updates and concurrent writes were not measured.

## Read side (Standard copy, same harness and inputs; single runs; baseline = the earlier Standard run without these indexes)
| Item | Before | With indexes 2 and 3 | Effect |
|---|---|---|---|
| AR search page 1 | 66.6 s | 113.1 s | slower |
| AP search page 1 | 278.2 s | **timeout (300 s)** | slower |
| AP search by invoice no | 154.7 s | 173.7 s | slower |
| Direct Billed Comm Statement (3 windows) | 118 to 134 s | 116 to 138 s | no change |
| Paid BDX (3 windows) | 67 to 85 s | 69 to 80 s | no change |

**No read benefit was seen, and the AP/AR searches got slower.** Index usage counters (`sys.dm_db_index_usage_stats`, since the instance's last statistics reset) show the InvoiceDetails index was used (17 seeks, 7 scans) but the line-items index was **never used** (0 seeks, 0 scans). Timing variation between runs on this shared machine can be large (the same AP search took 278 s in the Standard benchmark before these indexes; see the AP/AR timings in the index test above), so the slowdowns are not proven to be caused by the indexes; but there is no evidence of benefit.

Reports 1, 14 and 16 (Invoice Production, BDX, Earned vs Unearned) were **not re-timed**: the line-items index, which was the one expected to help Invoice Production and Direct Billed, was not used when Direct Billed ran, and these reports are dominated by per-row aggregation and temp-table work (see Addenda 1 and 2).

## Conclusion and recommendation (needs your decision; nothing applied outside the copy)
- **Do not recommend candidates 2 and 3 from this evidence.** They cost 790 MB and 16-28% more insert time and showed no read gain.
- **Candidate 1 (`PolicyDetails (InsuredDetail_Id, Client_Id)`) remains the one proven fix** (Payment Report 111-121 s to 2-6 s).
- The remaining slow items need procedure-level changes (the temp-table staging pattern, per-invoice aggregation, catch-all filters), not more indexes. Missing-index hints on temp tables (90-99.9% impact) point at the staging tables the procedures build themselves.
- The two test indexes are still on the copy. Revert commands are in the header of `10b_...sql` (needs your approval to run, since it drops objects).

---

# Addendum 7: Invoice Production prototype (Standard copy only, 2026-11-12)
Test object: `dbo.USP_Accounting_InvoiceProductionCRMAssignmentReport_PerfTest_V2`, created only in `uat-accounting-perf` on `MSSQLSERVER2022` (`docs/seed/11_InvoiceProduction_PerfTest_V2.sql`). The original procedure and the source database were not changed.

**The change:** the per-invoice `STUFF(... FOR XML PATH)` LOB-name aggregation used to be built for every invoice of the client (about 2.49M invoices) before filtering and paging, although the final select uses it only for the invoices on the returned page. In the prototype the same expression is built after the page is selected (`#FilteredInvoices`) and only for those invoices. Nothing else was edited.

**Test (comparison script `docs/seed/12_CompareProcs.ps1`):** client 1, `@InvoiceDate = 'InvoiceEffectiveDate'`, effective dates 2026-01-01 to 2026-01-03, page 1, page size 100,000, on the Standard copy (shared machine, one run each; both ran with the indexes in Addenda 4 and 6 present):
| | Rows | `@TotalRecords` | Time |
|---|---|---|---|
| Original | 18,458 | 18,458 | 790.4 s |
| Prototype | 18,458 | 18,458 | **475.3 s** |
Every output row was hashed over all columns and compared as a multiset: **0 differing rows** in either direction. About 40% less time (1.66x faster), but still nearly 8 minutes.

**What this shows**
- The LOB aggregation was a real cost (about 315 s of the 790 s here) and moving it is safe for this output.
- The remaining 475 s is elsewhere. The procedure still copies all of `Accounting_InvoiceDetails` for the client into `#TempAccounting_InvoiceDetails_InvProd` (15.3M rows, with its index) and builds the other staging tables for all invoices **before** the date filter or paging apply. That is why a 3-day window with the filter switched on still takes minutes.
- The earlier plan findings (estimates 30x to 275x low, whole-table staging copies) are unchanged.

**Correction to the benchmark harness (found while preparing this test):** this procedure applies its date filter only when `@InvoiceDate` is supplied (lines 38 to 52 of the source). `06_ReportBenchmark.ps1` did not pass `@InvoiceDate`, so the Invoice Production date window was ignored in the earlier runs and every run returned all invoices. This test shows the cost is almost unchanged even with the filter on, because the staging tables are built first, so the earlier conclusion "the date window does not change cost" still holds for this procedure; but it was **not** demonstrated by those runs. **Not yet checked:** which parameters the other 16 reports need to apply their date filters (they may need `@InvoiceDate` or an `@As_Of_Date`). Until that is checked, the earlier "date range barely changes cost" statement for the other reports should be treated as unverified.

**Next candidates (not applied, need approval):**
1. Restrict `#TempBasePolicyDetails_InvProd` and `#TempAccounting_InvoiceDetails_InvProd` to the filtered invoices instead of all client invoices (requires moving the date and other filters earlier; a bigger change, comparison test needed).
2. Index the temp tables the procedure builds, since the plan hints on `#TempBase*` tables had 90 to 99.9% impact.
3. Re-run the benchmark with the correct date parameters for every report.

---

# Addendum 8: Invoice Production prototypes V3 and V4 (Standard test copy only, 2026-11-12)
Test objects: `..._InvoiceProductionCRMAssignmentReport_PerfTest_V3` (`docs/seed/13_...sql`) and `..._PerfTest_V4` (`docs/seed/14_...sql`) on `uat-accounting-perf`. The original procedure, V2 and the source database were not changed. Window 2026-01-01 to 2026-01-03 with `@InvoiceDate = InvoiceEffectiveDate`, page size 100,000. Every output row was hashed over all columns and compared as a multiset (`docs/seed/12_CompareProcs.ps1`). Single runs, shared machine; repeat runs of one procedure differ by up to about 35% (the original took 790 s, 547 s and 532 s in three runs).

**V3** builds the staging tables the returned page needs (invoice details, insured details, journal entries, LOB names, fees, total premium) only after paging, and only for the page's invoices. `#LatestCancelSubmissions` is unchanged because it needs every invoice of a policy number.

**V4** is V3 plus one change: `CREATE CLUSTERED INDEX ... ON #TempAccounting_InvoiceDetails_InvProd (Invoice_Id)`.

| Case | Compared with | Rows | V3 | V4 | Baseline | Row-hash differences |
|---|---|---|---|---|---|---|
| A: no filters | original | 18,458 | 35.8 s | **63.9 s** | 531.6 s (547.4 s in the V3 run) | 0 (V3 and V4) |
| B: insured name `PerfTest Insured 185` | V2 | 25 | 229 s | **256.6 s** | 255.9 s (V2; 345.9 s in the V3 run) | 0 (V3 and V4) |
| C: `LOB_Id = 12` | V2 | 4,944 | **4,832.6 s** | **244.5 s** | 519.5 s (V2; 508.6 s in the V3 run) | 0 (V3 and V4) |

**The V3 regression (case C) and its cause.** V3 took about 81 minutes with a LOB filter. A live plan taken while it ran showed the final SELECT looking up each invoice's detail lines (`ID.Invoice_Id = TBPD.Invoice_Id`, in a correlated subquery and a NOT EXISTS) in a staging table whose only index leads with `Id`. With a LOB filter that table holds the details of every invoice in the window (it must, to apply the filter before paging), so each of the 4,944 outer rows scanned it. Without a filter the table holds only the page's rows, so the scans were cheap. Adding a clustered index on `Invoice_Id` (V4) turns those scans into seeks: case C fell from 4,832.6 s to 244.5 s. The earlier statements in the run were not the problem (details staging about 6.5 minutes, LOB aggregation about 2.5 minutes, fees about 1.5 minutes). Limit: I did not capture V4's own plan, and I did not establish why V2 did not show the same slowdown with the same index.

**What this shows**
- On 3 days of data the rewritten procedure returns **identical rows** to the original (case A) and to V2 (cases B and C) in every mode tested.
- No filters: about 8 times faster than the original (63.9 s against 531.6 s, V4). V4 was slower than V3 here (63.9 s against 35.8 s), probably the cost of building the clustered index; not investigated.
- Insured-name filter: V4 equals V2 (256.6 s against 255.9 s). The filter still forces the full insured and details staging before paging, as in the original.
- LOB filter: V4 about 2 times faster than V2.

**Limits.** Cases B and C compare with V2, not with the original, so they are a weaker check; V2 matched the original once (case A of Addendum 7, 18,458 rows). One 3-day window; other filters (agent, carrier, wholesaler, invoice number, grouping, transaction-date mode) and large page sizes were not tested. The comparison covers the first result set only. Local test timings on a shared, memory-limited machine; no production extrapolation. No change to the application procedure has been made or is proposed without your approval.

**Needs your decision:** whether to recommend this rewrite (V4 logic) to the application team, and the further tests to run first (other filter combinations, a wider date window, concurrency).

## Addendum 9: Transaction history search by PolicyNumber (local test, Standard copy)
**Symptom.** `USP_Accounting_GetTransactionHistorySearchResults` with a PolicyNumber took 151.9 s (200M logical reads, 463 s CPU) to return 4 rows. The same procedure with an InvoiceNumber took 1.8 s (67K reads). (The first-run 0-row results were harness input errors; see `Iteration1_Benchmark_Results.md`.)

**Cause (actual plans, `07_CapturePlans.ps1`).** Both searches drive from a scan of all 2,492,000 invoices. The InvoiceNo filter applies in that scan (1 row left). PolicyNumber exists only on `Accounting_PolicyDetails`, which the plan reaches last (after InvoiceDetails 15.3M rows, payment line items 5.38M, payments), so the `LIKE '%x%'` runs as a leftover predicate on 5.38M probes and keeps 4 rows. The optimizer estimated about 100 rows for the early steps (actual millions) and chose nested-loop lookups. A leading-wildcard LIKE cannot use an index seek, so a PolicyNumber index would not help while the filter keeps that form.

**Prototype (copy only, original untouched).** `USP_Accounting_GetTransactionHistorySearchResults_PerfTest_V2` (`docs/seed/16_TransactionHistory_PerfTest_V2.sql`): with a PolicyNumber it first stages the matching policies (same Client_Id, Status, LIKE filter) into a temp table and joins from it; with no PolicyNumber it calls the original procedure unchanged.

| Case | Original | V2 | Row-hash differences |
|---|---|---|---|
| `PT-POL-00400000` (4 rows) | 136.7 s | 1.2 s | 0 |
| `PT-POL-0040000` (26 rows) | 130.1 s | 1.3 s | 0 |
| `PT-POL-0040`, page 2 (60,491 matches) | 93 s | 6.6 s | 100 (tie ordering, see below) |
| `PT-POL-0040`, all 60,491 rows unpaged | 234.1 s | 116.9 s | 0 |

**Page-2 difference.** The paged query orders only by `APALI.CreatedOn`. The 60,491 matching rows carry 7 distinct CreatedOn values (bulk-load batches), so which 50 rows fall on a later page is not defined and can differ between plans. The unpaged comparison is identical. The original procedure has the same ordering weakness against itself (a latent issue, not introduced by V2); a unique tie-breaker column would fix it and needs the application team's approval.

**Limits.** Only the PolicyNumber filter was tested, not combined with other filters, no concurrency, single runs on a shared machine, local test data only; no production extrapolation. No change to the application procedure has been made or is proposed without your approval.

**Needs your decision:** whether to recommend this rewrite to the application team, after combined-filter tests, and whether to raise the CreatedOn tie-breaking issue.

**Addendum 9, combined-filter test (local test, 2026-11-12, Standard copy).** `PerfTest_V2` vs the original, all with `PolicyNumber = PT-POL-004000` (matches 100 policies), unpaged (`PageSize = 0`) so row order cannot matter; first result set compared as a multiset of row hashes. Single runs.

| Extra filter | Rows | Original | V2 | Row-hash differences |
|---|---|---|---|---|
| Direction_Id = 1 | 527 | 89.5 s | 4.4 s | 0 |
| PaymentType_Id = 4 | 186 | 84.6 s | 1.5 s | 0 |
| PaymentStatus_Id = 14 | 604 | 77.6 s | 1.8 s | 0 |
| EntityName = 'PerfTest Agency 3' | 20 | 79.2 s | 0.9 s | 0 |
| InvoiceNumber = 'INV0000650' | 0 | 53.5 s | 0.8 s | 0 |
| Amount_From 1000, Amount_To 3000 | 85 | 30.2 s | 0.9 s | 0 |
| TransactionDate = 2026-11-11 | 527 | 70.7 s | 1.7 s | 0 |
| Direction 1 + PaymentType 4 + EntityName 'PerfTest' | 181 | 73.7 s | 1.2 s | 0 |
| PolicyNumber matching nothing | 0 | 29.5 s | 0.6 s | 0 |

Limits: the InvoiceNumber case returned 0 rows in both procedures (it checks the empty result, not row content); only unpaged results were compared (page ordering within CreatedOn ties is the separate latent issue above); one policy set, no concurrency, single runs on a shared machine; local test data only, no production extrapolation. The recommendation decision above is unchanged and still needs your approval.

**Addendum 9, correction (2026-11-13): the baseline was not stable; the rewrite is not supported.** After this addendum was written, the statistics on `Accounting_PolicyDetails` were refreshed (last update 01:06:54, 6% sample; likely triggered by the insert and rebuild of the PolicyDetails write-cost test, which I ran later; not proven). The original procedure then took **2.8 s** (4 rows) and **3.7 s** (186 rows, combined filters) instead of 130 to 137 s. The re-captured plan starts from a seek on `Accounting_PolicyDetails` (estimated 32 rows, actual 1) and works outward, instead of scanning all 2.49M invoices first. The earlier slow plan therefore depended on the old statistics (outer estimate about 100 rows, actual millions), not on a fixed property of the procedure. The V2 timings above (1.2 s) were compared against that stale-statistics baseline; the rewrite gives no demonstrated gain over the original now. **Not recommended; no migration script written.** Open question for the real database: whether its statistics on `Accounting_PolicyDetails`, `Accounting_Invoices` and the payment tables are fresh. Also checked: the Payment Report index result (Addendum 4) still holds after the refresh (narrow window page 1: 123.5 s with the test index disabled, 2.7 s enabled, identical total 156,050).

**Addendum 8, re-check after the statistics refresh (2026-11-13).** The no-filter case was repeated on `uat-accounting-perf` after the `Accounting_PolicyDetails` statistics refresh (see Addendum 9 correction), using `docs/seed/12_CompareProcs.ps1` (V4 run first, then the original), client 1, `@InvoiceDate = InvoiceEffectiveDate`, effective dates 2026-01-01 to 2026-01-03, page 1, page size 100,000, one run each on a shared machine:

| | Rows | `@TotalRecords` | Time |
|---|---|---|---|
| V4 | 18,458 | 18,458 | 46.3 s |
| Original | 18,458 | 18,458 | 546 s |

Row-hash multiset comparison: 0 differences. The gain (about 12x) is the same order as before (531.6 s vs 63.9 s), so it comes from building the staging data only for the returned page, not from the earlier stale-statistics plan. Limits unchanged: one 3-day window, no other filters, wider windows or concurrency; local test timing, no production extrapolation.

# Addendum 10: Make payments entities (`USP_Accounting_GetMakePaymentsEntities_V6`), cause and corrected benchmark (Standard test copy only, 2026-11-13)

**Corrected benchmark input.** The harness call returned 0 rows for two reasons that were input errors, not defects. `Direction_Id=1` keeps only invoice lines with `RemainingBalance < 0`, and the test data has none (0 of 3.83M direction-1 agency lines). With `@OutstandingBalances = 1` the procedure also needs `@AmountDueBasedOnType_Id` set to 1 or 2 (the application always sends it, `MakePaymentsRepository.cs`); with it empty, the final filter keeps nothing. Corrected call (`docs/seed/06_ReportBenchmark.ps1`, P5a): client 1, `EntityType_Id=1` (agency), `Direction_Id=2`, `OutstandingBalances=1`, `AmountDueBasedOnType_Id=1`, page 1 of 50, one run, shared machine:

| Call | Time | Rows | Logical reads | CPU |
|---|---|---|---|---|
| Direction 2, no amount-due type (0 rows, filter empties the result) | 365.8 s | 0 | 16.18M | 1,129 s |
| Direction 2, `AmountDueBasedOnType_Id=1` | **338.6 s** | 50 | 16.18M | 1,138 s |

The call finishes inside a 900 s limit but is far over the 300 s limit used earlier, which is why the first run was recorded as a timeout.

**Cause (from the plan capture and source reading).** The procedure builds about 20 staging temp tables for all invoices of the client (base policy and invoice rows, a copy of the invoice detail lines, payment-application sums and the commission and pass-through aggregates) before the dynamic final query. The entity filter, outstanding-balance filter and paging are applied only at the end. In the captured run, 16 temp-table index builds took 194.5 s of the 457 s staging time and the final query took 3 s. The work is the same whether the page asks for 50 rows or 1,000, and a narrow entity filter does not shrink it.

**What was not done.** No rewrite was written or tested. A candidate would restrict the staging tables by the entity type and direction before building them, but the result sets and the later `Accounting_BillProcessInvoiceLineItems` adjustment make equivalence a larger check than for Invoice Production. Local test timings on a shared, memory-limited machine; no production extrapolation.
