# Report benchmark harness (Windows PowerShell 5.1; uses System.Data.SqlClient).
# Calls each report's stored procedure (the one in Accounting_LU_SQLQueries) the way the application's
# ReportsUtility does: Client_Id + report filters + PageIndex/PageSize/@TotalRecords OUTPUT for paged reports.
# LOCAL EXPRESS TEST ONLY. Measures the database side; the C# pipeline (DataSet copy, grouping, Excel/PDF
# build, base64) is NOT included, so these are lower bounds for what a user waits for.
# Usage: powershell -NoProfile -File 06_ReportBenchmark.ps1 -OutDir <dir> [-TimeoutSec 300] [-Only 1,6,...]
param([string]$OutDir, [int]$TimeoutSec = 300, [int[]]$Only, [string]$Mode = 'reports',   # Mode: reports | pages
      [string]$Server = '(localdb)\MSSQLLocalDB', [string]$Database = 'uat-accounting',
      [string]$PageIds = '')   # pages mode only: semicolon-separated ids, e.g. 'P1a;P2a' (empty = all)

Add-Type -AssemblyName System.Data
$sb = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$sb['Data Source'] = $Server; $sb['Initial Catalog'] = $Database
$sb['Integrated Security'] = $true; $sb['TrustServerCertificate'] = $true
$connStr = $sb.ConnectionString

$types = @{ Client_Id='BigInt'; PageIndex='Int'; PageSize='Int'; TotalRecords='Int'; Years='Int'; Month_Id='TinyInt';
            OutstandingBalances='Bit'; Simplified='Bit'; IncludeDirectBill='Bit'; IncludeInsuredBill='Bit';
            FromDate='DateTime'; ToDate='DateTime'; DateBookedFrom='DateTime'; DateBookedTo='DateTime';
            ReportingDateFrom='Date'; ReportingDateTo='Date' }

# window = from/to (yyyy-MM-dd). Data: policy effective dates 2025-01-01..2026-06-23, due dates to Aug 2026.
$windows = @{ narrow = @('2026-01-01','2026-01-31'); year = @('2025-01-01','2025-12-31'); full = @('2025-01-01','2027-12-31') }

function New-Params($rep, $w, $paged, $pageSize) {
    $f = $windows[$w][0]; $t = $windows[$w][1]
    $p = [ordered]@{ Client_Id = 1 }
    switch ($rep.Date) {
        'InvoiceEffective' { $p.InvoiceEffectiveFromDate = $f; $p.InvoiceEffectiveToDate = $t }
        'Payment'          { $p.PaymentFromDate = $f; $p.PaymentToDate = $t }
        'Transaction'      { $p.TransactionFromDate = $f; $p.TransactionToDate = $t }
        'JE'               { $p.FromDate = $f; $p.ToDate = $t }
        '1099'             { $p.DateBookedFrom = $f; $p.DateBookedTo = $t }
        'PaidBDX'          { $p.ReportingDateFrom = $f; $p.ReportingDateTo = $t }
        'BDX'              { if ($w -eq 'narrow') { $p.Years = 2026; $p.Month_Id = 1 } else { $p.Years = 2025; $p.Month_Id = 6 } }
    }
    foreach ($k in $rep.Extra.Keys) { $p[$k] = $rep.Extra[$k] }
    if ($paged) { $p.PageIndex = 1; $p.PageSize = $pageSize }
    return $p
}

$reports = @(
 @{Id=1;  Name='Invoice Production';           SP='USP_Accounting_InvoiceProductionCRMAssignmentReport_2026_06_09'; Paged=$true;  Date='InvoiceEffective'; Extra=@{}},
 @{Id=2;  Name='Agency Billed Acct Statement'; SP='USP_Accounting_AgencyBillAccountStatementReport_20260624';      Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1}},
 @{Id=3;  Name='Direct Billed Comm Statement'; SP='USP_Accounting_DirectBillCommissionStatementReport';            Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1}},
 @{Id=4;  Name='Payment Report';               SP='USP_Accounting_PaymentReport';                                  Paged=$true;  Date='Payment';          Extra=@{}},
 @{Id=5;  Name='Transaction Report';           SP='USP_Accounting_TransactionReport';                              Paged=$true;  Date='Transaction';      Extra=@{}},
 @{Id=6;  Name='Accounts Receivable';          SP='USP_Accounting_AccountsReceivableReport_06102026';              Paged=$true;  Date='InvoiceEffective'; Extra=@{OutstandingBalances=1}},
 @{Id=7;  Name='Accounts Payable';             SP='USP_Accounting_AccountsPayableReport';                          Paged=$true;  Date='InvoiceEffective'; Extra=@{}},
 @{Id=8;  Name='Tax/Fee Passthrough';          SP='USP_Accounting_Tax_FeePassthroughReportByLOBs';                 Paged=$true;  Date='InvoiceEffective'; Extra=@{}},
 @{Id=9;  Name='Insured Billed Comm Statement';SP='USP_Accounting_InsuredBilledCommissionStatementReport';         Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1}},
 @{Id=10; Name='Journal Entries';              SP='USP_Accounting_JEReport';                                       Paged=$true;  Date='JE';               Extra=@{}},
 @{Id=11; Name='1099';                         SP='USP_Accounting_RetailAgent1099Report';                          Paged=$true;  Date='1099';             Extra=@{}},
 @{Id=12; Name='Agency Detailed Statement';    SP='USP_Accounting_AgencyDetailedAccountStatementReport';           Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1}},
 @{Id=13; Name='Underwriter Comm Statement';   SP='USP_Accounting_UnderwriterCommissionStatementReport';          Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1}},
 @{Id=14; Name='BDX';                          SP='USP_Accounting_BDXReport';                                      Paged=$true;  Date='BDX';              Extra=@{}},
 @{Id=15; Name='Direct/Insured Comm Statement';SP='USP_Accounting_Direct&InsuredBillCommissionStatementReport';    Paged=$false; Date='InvoiceEffective'; Extra=@{OutstandingBalances=1;Simplified=1;IncludeDirectBill=1;IncludeInsuredBill=1}},
 @{Id=16; Name='Earned vs Unearned';           SP='USP_Accounting_EarnedUnearnedPremiumCommissionReport';          Paged=$true;  Date='InvoiceEffective'; Extra=@{}},
 @{Id=17; Name='Paid BDX';                     SP='USP_Accounting_GetPaidBDXReport';                               Paged=$true;  Date='PaidBDX';          Extra=@{}}
)
if ($Only) { $reports = $reports | Where-Object { $Only -contains $_.Id } }

$csv  = Join-Path $OutDir 'report_benchmark.csv'
$inv  = Join-Path $OutDir 'report_invoice_numbers.txt'
$log  = Join-Path $OutDir 'report_benchmark.log'
if (-not (Test-Path $csv)) { 'ReportId,Report,Scenario,Run,Status,WallSec,Rows,ResultSets,LogicalReads,CpuMs,TotalRecords,Note' | Set-Content $csv }

function Invoke-One($rep, $scenario, $w, $pageSize, $run) {
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    $script:reads = 0L; $script:cpu = 0L
    $handler = [System.Data.SqlClient.SqlInfoMessageEventHandler] {
        param($s, $e)
        foreach ($m in $e.Errors) {
            $txt = $m.Message
            foreach ($mm in [regex]::Matches($txt, 'logical reads (\d+)')) { $script:reads += [int64]$mm.Groups[1].Value }
            if ($txt -match 'SQL Server Execution Times' -and $txt -match 'CPU time = (\d+) ms') { $script:cpu += [int64]$Matches[1] }
        }
    }
    $conn.add_InfoMessage($handler); $conn.FireInfoMessageEventOnUserErrors = $true
    $status = 'OK'; $note = ''; $rows = 0; $sets = 0; $tot = ''
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $conn.Open()
        $c0 = $conn.CreateCommand(); $c0.CommandText = 'SET STATISTICS IO ON; SET STATISTICS TIME ON;'; [void]$c0.ExecuteNonQuery()
        $cmd = $conn.CreateCommand(); $cmd.CommandType = 'StoredProcedure'; $cmd.CommandText = $rep.SP; $cmd.CommandTimeout = $TimeoutSec
        if ($rep.Fixed) { $p = $rep.Fixed } else { $p = New-Params $rep $w $rep.Paged $pageSize }
        foreach ($k in $p.Keys) {
            if ($rep.Fixed) {   # pages mode: infer the SQL type from the .NET type of the value
                $v = $p[$k]
                $dt = if ($v -is [string]) { 'NVarChar' } elseif ($v -is [int]) { 'Int' } elseif ($v -is [long]) { 'BigInt' } elseif ($v -is [byte]) { 'TinyInt' } elseif ($v -is [bool]) { 'Bit' } elseif ($v -is [datetime]) { 'DateTime' } else { 'NVarChar' }
            } else { $dt = if ($types.ContainsKey($k)) { $types[$k] } else { 'NVarChar' } }
            $par = $cmd.Parameters.Add('@' + $k, [System.Data.SqlDbType]::$dt)
            if ($dt -eq 'NVarChar') { $par.Size = 200 }
            $par.Value = $p[$k]
        }
        if ($rep.Paged) { $o = $cmd.Parameters.Add('@TotalRecords', [System.Data.SqlDbType]::Int); $o.Direction = 'Output' }
        $rd = $cmd.ExecuteReader()
        do {
            $sets++
            $invCols = @(); for ($i = 0; $i -lt $rd.FieldCount; $i++) { if ($rd.GetName($i) -match '(?i)invoice\s*(no|number|#|num)') { $invCols += $i } }
            while ($rd.Read()) {
                $rows++
                if ($invCols.Count -gt 0 -and $run -eq 1 -and $script:invSeen.Count -lt 20000) {
                    foreach ($i in $invCols) { $v = $rd.GetValue($i); if ($v -isnot [DBNull]) { [void]$script:invSeen.Add([string]$v) } }
                }
            }
        } while ($rd.NextResult())
        $rd.Close()
        if ($rep.Paged -and $o.Value -isnot [DBNull]) { $tot = [string]$o.Value }
    } catch [System.Data.SqlClient.SqlException] {
        if ($_.Exception.Number -eq -2) { $status = 'TIMEOUT'; $note = "no result within $TimeoutSec s" } else { $status = 'ERROR'; $note = ($_.Exception.Message -replace '[\r\n,]', ' ') }
        if ($note.Length -gt 160) { $note = $note.Substring(0,160) }
    } catch { $status = 'ERROR'; $note = ($_.Exception.Message -replace '[\r\n,]', ' '); if ($note.Length -gt 160) { $note = $note.Substring(0,160) } }
    finally { $sw.Stop(); try { $conn.Close() } catch {} }
    $line = '{0},"{1}",{2},{3},{4},{5},{6},{7},{8},{9},{10},"{11}"' -f $rep.Id, $rep.Name, $scenario, $run, $status, [math]::Round($sw.Elapsed.TotalSeconds,2), $rows, $sets, $script:reads, $script:cpu, $tot, $note
    Add-Content $csv $line; Add-Content $log ("{0} {1}" -f (Get-Date -Format HH:mm:ss), $line)
    return @{ Status = $status; Wall = $sw.Elapsed.TotalSeconds }
}

$script:invSeen = New-Object 'System.Collections.Generic.HashSet[string]'

# ---- pages mode: the application's search / listing screens (procedures called from C#) ----
# Exact values exist in the loaded data: invoice INV00006500000, policy PT-POL-00400000, insured 400000.
if ($Mode -eq 'pages') {
    $d0 = [datetime]'2025-01-01'; $d1 = [datetime]'2027-12-31'
    $pages = @(
     @{Id='P1a'; Name='AR search page (outstanding, page 1)';       SP='USP_Accounting_GetARSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}},
     @{Id='P1b'; Name='AR search by InvoiceNo (exact)';             SP='USP_Accounting_GetARSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; InvoiceNo='INV00006500000'; PageIndex=1; PageSize=50}},
     @{Id='P1c'; Name='AR search by PolicyNumber (exact)';          SP='USP_Accounting_GetARSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; PolicyNumber='PT-POL-00400000'; PageIndex=1; PageSize=50}},
     @{Id='P1d'; Name='AR search by InsuredName (partial)';         SP='USP_Accounting_GetARSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; InsuredName='Insured 40000'; PageIndex=1; PageSize=50}},
     @{Id='P2a'; Name='AP search page (outstanding, page 1)';       SP='USP_Accounting_GetAPSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}},
     @{Id='P2b'; Name='AP search by InvoiceNo (exact)';             SP='USP_Accounting_GetAPSearchResults_V7'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; InvoiceNo='INV00006500000'; PageIndex=1; PageSize=50}},
     @{Id='P3a'; Name='Payments search (page 1, all dates)';        SP='USP_Accounting_GetPaymentsSearchResults_paging_2026081'; Paged=$true; Fixed=[ordered]@{Client_Id=[long]1; PaymentFromDate=$d0; PaymentToDate=$d1; PageSize=50; PageNumber=1}},
     @{Id='P3b'; Name='Payments search by ReferenceNumber';         SP='USP_Accounting_GetPaymentsSearchResults_paging_2026081'; Paged=$true; Fixed=[ordered]@{Client_Id=[long]1; ReferenceNumber='PT3000000'; PageSize=50; PageNumber=1}},
     @{Id='P4a'; Name='Transaction history search (page 1)';        SP='USP_Accounting_GetTransactionHistorySearchResults'; Paged=$true; Fixed=[ordered]@{Client_Id=[long]1; PageSize=50; PageNumber=1}},
     @{Id='P4b'; Name='Transaction history by InvoiceNumber';       SP='USP_Accounting_GetTransactionHistorySearchResults'; Paged=$true; Fixed=[ordered]@{Client_Id=[long]1; InvoiceNumber='INV00006500000'; PageSize=50; PageNumber=1}},
     @{Id='P4c'; Name='Transaction history by PolicyNumber';        SP='USP_Accounting_GetTransactionHistorySearchResults'; Paged=$true; Fixed=[ordered]@{Client_Id=[long]1; PolicyNumber='PT-POL-00400000'; PageSize=50; PageNumber=1}},
     @{Id='P5a'; Name='Make payments entities (agency, page 1)';   SP='USP_Accounting_GetMakePaymentsEntities_V6'; Paged=$false; Fixed=[ordered]@{Client_Id=[long]1; EntityType_Id=[byte]1; Direction_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}},
     @{Id='P6a'; Name='Invoice lookup by number (exact)';          SP='USP_Accounting_GetInvoicesByInvoiceNumber'; Paged=$false; Fixed=[ordered]@{InvoiceNumber='INV00006500000'; Client_Id=[long]1}},
     @{Id='P7a'; Name='Policy details page: policy';               SP='USP_Accounting_GetPolicyDetails'; Paged=$false; Fixed=[ordered]@{Id=[long]400000}},
     @{Id='P7b'; Name='Policy details page: invoices of policy';   SP='USP_Accounting_GetInvoicesByPolicyDetail_Id'; Paged=$false; Fixed=[ordered]@{PolicyDetails_Id=[long]400000}}
    )
    if ($PageIds) { $want = $PageIds -split ';'; $pages = $pages | Where-Object { $want -contains $_.Id } }
    foreach ($pg in $pages) {
        $r1 = Invoke-One $pg 'single' 'full' 0 1
        if ($r1.Status -eq 'OK' -and $r1.Wall -lt 60) { [void](Invoke-One $pg 'single' 'full' 0 2) }
    }
    Add-Content $log ("DONE pages {0}" -f (Get-Date -Format s))
    return
}

foreach ($rep in $reports) {
    $scen = @(@('narrow','narrow',100), @('year','year',100), @('full','full',100))
    if ($rep.Paged) { $scen += ,@('full-download-10000','full',10000) }
    foreach ($s in $scen) {
        $r1 = Invoke-One $rep $s[0] $s[1] $s[2] 1
        if ($r1.Status -eq 'OK' -and $r1.Wall -lt 60) { [void](Invoke-One $rep $s[0] $s[1] $s[2] 2) }
        if ($r1.Status -ne 'OK') { Add-Content $log "  -> skipping wider scenarios for report $($rep.Id) after $($r1.Status)"; break }
    }
}
$script:invSeen | Set-Content $inv
Add-Content $log ("DONE {0}; distinct invoice numbers displayed by reports: {1}" -f (Get-Date -Format s), $script:invSeen.Count)
