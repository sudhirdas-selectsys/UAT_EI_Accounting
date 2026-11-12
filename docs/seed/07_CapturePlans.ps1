# Captures ACTUAL execution plans (SET STATISTICS XML ON) for the slowest report/page calls.
# LOCAL EXPRESS TEST ONLY. Read-only: it only executes the same stored procedures as 06_ReportBenchmark.ps1.
# Windows PowerShell 5.1. Usage: powershell -NoProfile -File 07_CapturePlans.ps1 -OutDir <dir> [-Only tag,tag]
# Output: <OutDir>\<tag>\plan_NNN.sqlplan (one file per statement plan), <OutDir>\capture.log
param([string]$OutDir, [string[]]$Only, [string]$Server = '(localdb)\MSSQLLocalDB', [string]$Database = 'uat-accounting')

Add-Type -AssemblyName System.Data
$sb = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$sb['Data Source'] = $Server; $sb['Initial Catalog'] = $Database
$sb['Integrated Security'] = $true; $sb['TrustServerCertificate'] = $true
$connStr = $sb.ConnectionString
$log = Join-Path $OutDir 'capture.log'

$types = @{ Client_Id='BigInt'; PageIndex='Int'; PageSize='Int'; PageNumber='Int'; Years='Int'; Month_Id='TinyInt';
            OutstandingBalances='Bit'; Simplified='Bit'; IncludeDirectBill='Bit'; IncludeInsuredBill='Bit';
            ReportingDateFrom='Date'; ReportingDateTo='Date'; EntityType_Id='TinyInt'; Direction_Id='TinyInt'; BillingType_Id='TinyInt' }
$f = '2026-01-01'; $t = '2026-01-31'   # narrow window, as in the benchmark
$inv = [ordered]@{ Client_Id=1; InvoiceEffectiveFromDate=$f; InvoiceEffectiveToDate=$t }

$items = @(
 @{Tag='R16_EarnedUnearned';  SP='USP_Accounting_EarnedUnearnedPremiumCommissionReport'; Timeout=1200; Paged=$true;  P=[ordered]@{Client_Id=1; InvoiceEffectiveFromDate=$f; InvoiceEffectiveToDate=$t; PageIndex=1; PageSize=100}},
 @{Tag='R14_BDX';             SP='USP_Accounting_BDXReport'; Timeout=1800; Paged=$true; P=[ordered]@{Client_Id=1; Years=2026; Month_Id=1; PageIndex=1; PageSize=100}},
 @{Tag='P5a_MakePayEntities'; SP='USP_Accounting_GetMakePaymentsEntities_V6'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=[long]1; EntityType_Id=[byte]1; Direction_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}},
 @{Tag='P2a_APSearch';        SP='USP_Accounting_GetAPSearchResults_V7'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}},
 @{Tag='P2b_APSearchInvNo';   SP='USP_Accounting_GetAPSearchResults_V7'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; InvoiceNo='INV00006500000'; PageIndex=1; PageSize=50}},
 @{Tag='R03_DirectBilled';    SP='USP_Accounting_DirectBillCommissionStatementReport'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=1; InvoiceEffectiveFromDate=$f; InvoiceEffectiveToDate=$t; OutstandingBalances=1; Simplified=1}},
 @{Tag='R01_InvoiceProduction'; SP='USP_Accounting_InvoiceProductionCRMAssignmentReport_2026_06_09'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=1; InvoiceEffectiveFromDate=$f; InvoiceEffectiveToDate=$t; PageIndex=1; PageSize=100}},
 @{Tag='R15_DirectInsured';   SP='USP_Accounting_Direct&InsuredBillCommissionStatementReport'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=1; InvoiceEffectiveFromDate=$f; InvoiceEffectiveToDate=$t; OutstandingBalances=1; Simplified=1; IncludeDirectBill=1; IncludeInsuredBill=1}},
 @{Tag='R17_PaidBDX';         SP='USP_Accounting_GetPaidBDXReport'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=1; ReportingDateFrom='2025-01-01'; ReportingDateTo='2027-12-31'; PageIndex=1; PageSize=100}},
 @{Tag='P4b_TxnHistInvNo';    SP='USP_Accounting_GetTransactionHistorySearchResults'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=[long]1; InvoiceNumber='INV00006500000'; PageSize=50; PageNumber=1}},
 @{Tag='P4c_TxnHistPolicy';   SP='USP_Accounting_GetTransactionHistorySearchResults'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=[long]1; PolicyNumber='PT-POL-00400000'; PageSize=50; PageNumber=1}},
 @{Tag='R05_TransactionReport'; SP='USP_Accounting_TransactionReport'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=1; TransactionFromDate=$f; TransactionToDate=$t; PageIndex=1; PageSize=100}},
 @{Tag='R04_PaymentReport';   SP='USP_Accounting_PaymentReport'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=1; PaymentFromDate=$f; PaymentToDate=$t; PageIndex=1; PageSize=100}},
 @{Tag='R04v2_PaymentReport_PerfTestV2'; SP='USP_Accounting_PaymentReport_PerfTest_V2'; Timeout=900; Paged=$true; P=[ordered]@{Client_Id=1; PaymentFromDate=$f; PaymentToDate=$t; PageIndex=1; PageSize=100}},
 @{Tag='P1a_ARSearch';      SP='USP_Accounting_GetARSearchResults_V7'; Timeout=900; Paged=$false; P=[ordered]@{Client_Id=[long]1; BillingType_Id=[byte]1; OutstandingBalances=$true; PageIndex=1; PageSize=50}}
)
if ($Only) { $items = $items | Where-Object { $Only -contains $_.Tag } }

foreach ($it in $items) {
    $dir = Join-Path $OutDir $it.Tag; New-Item -ItemType Directory -Force $dir | Out-Null
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    $status = 'OK'; $note = ''; $plans = 0; $rows = 0
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $conn.Open()
        $c0 = $conn.CreateCommand(); $c0.CommandText = 'SET STATISTICS XML ON;'; [void]$c0.ExecuteNonQuery()
        $cmd = $conn.CreateCommand(); $cmd.CommandType = 'StoredProcedure'; $cmd.CommandText = $it.SP; $cmd.CommandTimeout = $it.Timeout
        foreach ($k in $it.P.Keys) {
            $v = $it.P[$k]
            $dt = if ($types.ContainsKey($k)) { $types[$k] } elseif ($v -is [string]) { 'NVarChar' } elseif ($v -is [int]) { 'Int' } elseif ($v -is [long]) { 'BigInt' } else { 'NVarChar' }
            $par = $cmd.Parameters.Add('@' + $k, [System.Data.SqlDbType]::$dt)
            if ($dt -eq 'NVarChar') { $par.Size = 200 }
            $par.Value = $v
        }
        if ($it.Paged) { $o = $cmd.Parameters.Add('@TotalRecords', [System.Data.SqlDbType]::Int); $o.Direction = 'Output' }
        $rd = $cmd.ExecuteReader()
        do {
            if ($rd.FieldCount -eq 1 -and $rd.GetName(0) -like '*XML Showplan*') {
                while ($rd.Read()) { $plans++; [IO.File]::WriteAllText((Join-Path $dir ('plan_{0:D3}.sqlplan' -f $plans)), [string]$rd.GetValue(0)) }
            } else { while ($rd.Read()) { $rows++ } }
        } while ($rd.NextResult())
        $rd.Close()
    } catch [System.Data.SqlClient.SqlException] {
        if ($_.Exception.Number -eq -2) { $status = 'TIMEOUT' } else { $status = 'ERROR' }
        $note = ($_.Exception.Message -replace '[\r\n,]', ' '); if ($note.Length -gt 160) { $note = $note.Substring(0,160) }
    } catch { $status = 'ERROR'; $note = ($_.Exception.Message -replace '[\r\n,]', ' '); if ($note.Length -gt 160) { $note = $note.Substring(0,160) } }
    finally { $sw.Stop(); try { $conn.Close() } catch {} }
    Add-Content $log ('{0} {1},{2},{3} s,plans={4},rows={5},{6}' -f (Get-Date -Format HH:mm:ss), $it.Tag, $status, [math]::Round($sw.Elapsed.TotalSeconds,1), $plans, $rows, $note)
}
Add-Content $log ('DONE {0}' -f (Get-Date -Format s))
