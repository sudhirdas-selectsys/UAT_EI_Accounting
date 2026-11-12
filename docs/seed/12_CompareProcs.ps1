# Compares two stored procedures' first result set as a MULTISET of rows (row order ignored), with timing.
# Windows PowerShell 5.1. LOCAL TEST ONLY. Params are passed as name=value pairs (strings; bigint/int detected).
# Usage: powershell -File 12_CompareProcs.ps1 -ProcA <name> -ProcB <name> -Server <s> -Database <d> -ProcParams "Client_Id=1;PageSize=100000" [-Timeout 1800]
param([string]$ProcA, [string]$ProcB, [string]$Server, [string]$Database, [string]$ProcParams = '', [int]$Timeout = 1800)
Add-Type -AssemblyName System.Data
$cs = "Data Source=$Server;Initial Catalog=$Database;Integrated Security=True;TrustServerCertificate=True"
function Run([string]$proc) {
    $c = New-Object System.Data.SqlClient.SqlConnection $cs; $c.Open()
    $cmd = $c.CreateCommand(); $cmd.CommandType = 'StoredProcedure'; $cmd.CommandText = $proc; $cmd.CommandTimeout = $Timeout
    foreach ($kv in ($ProcParams -split ';' | Where-Object { $_ })) {
        $k, $v = $kv -split '=', 2
        if ($k -in 'Client_Id') { $p = $cmd.Parameters.Add("@$k", [System.Data.SqlDbType]::BigInt); $p.Value = [int64]$v }
        elseif ($k -in 'PageIndex', 'PageSize') { $p = $cmd.Parameters.Add("@$k", [System.Data.SqlDbType]::Int); $p.Value = [int]$v }
        else { $p = $cmd.Parameters.Add("@$k", [System.Data.SqlDbType]::NVarChar, 200); $p.Value = $v }
    }
    $o = $cmd.Parameters.Add('@TotalRecords', [System.Data.SqlDbType]::Int); $o.Direction = 'Output'
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $rd = $cmd.ExecuteReader(); $counts = @{}; $rows = 0; $sha = [Security.Cryptography.SHA256]::Create()
    while ($rd.Read()) {
        $rows++; $parts = New-Object string[] $rd.FieldCount
        for ($i = 0; $i -lt $rd.FieldCount; $i++) { $v = $rd.GetValue($i); $parts[$i] = if ($v -is [DBNull]) { '~NULL~' } else { [string]$v } }
        $h = [Convert]::ToBase64String($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes(($parts -join "`u{1F}"))))
        if ($counts.ContainsKey($h)) { $counts[$h]++ } else { $counts[$h] = 1 }
    }
    $rd.Close(); $sw.Stop(); $tot = $o.Value; $c.Close()
    return @{ Counts = $counts; Rows = $rows; Sec = [math]::Round($sw.Elapsed.TotalSeconds, 1); Total = $tot }
}
$A = Run $ProcB      # run the (faster) candidate first
$B = Run $ProcA
$diff = 0; foreach ($k in $A.Counts.Keys) { if (-not $B.Counts.ContainsKey($k) -or $B.Counts[$k] -ne $A.Counts[$k]) { $diff++ } }
foreach ($k in $B.Counts.Keys) { if (-not $A.Counts.ContainsKey($k)) { $diff++ } }
"{0}: rows={1} total={2} time={3}s" -f $ProcB, $A.Rows, $A.Total, $A.Sec
"{0}: rows={1} total={2} time={3}s" -f $ProcA, $B.Rows, $B.Total, $B.Sec
"distinct row-hash differences (multiset): $diff"
