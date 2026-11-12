/* Write-cost probe for the PolicyDetails (InsuredDetail_Id, Client_Id) index. Standard COPY ONLY (uat-accounting-perf).
   Inserts 100,000 copies of existing Accounting_PolicyDetails rows inside a transaction and ROLLS BACK (no data kept;
   identity values are consumed). Runs 3 times with the index IX_PerfTest_PolicyDetails_InsuredDetail_Id enabled, then
   DISABLES it, runs 3 times, and REBUILDS it (the index is restored; it is never dropped).
   sqlcmd -I -b */
SET NOCOUNT ON;
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PerfTest_PolicyDetails_InsuredDetail_Id' AND object_id = OBJECT_ID(N'dbo.Accounting_PolicyDetails') AND is_disabled = 0)
    THROW 50002, 'Test index missing or already disabled. Aborting.', 1;
DECLARE @res TABLE (state varchar(20), run int, rows_ int, ms int);
DECLARE @cols nvarchar(max), @sql nvarchar(max), @s datetime2, @i int, @r int;
SELECT @cols = STRING_AGG(CAST(QUOTENAME(name) AS nvarchar(max)), ',') FROM sys.columns
 WHERE object_id = OBJECT_ID('dbo.Accounting_PolicyDetails') AND is_identity = 0 AND is_computed = 0 AND system_type_id <> 189;
SET @sql = N'INSERT INTO dbo.Accounting_PolicyDetails (' + @cols + N') SELECT TOP (100000) ' + @cols + N' FROM dbo.Accounting_PolicyDetails WHERE Client_Id = 1;';
DECLARE @pass int = 1;
WHILE @pass <= 2
BEGIN
    IF @pass = 2 ALTER INDEX IX_PerfTest_PolicyDetails_InsuredDetail_Id ON dbo.Accounting_PolicyDetails DISABLE;
    SET @i = 1;
    WHILE @i <= 3
    BEGIN
        CHECKPOINT;
        SET @s = SYSDATETIME();
        BEGIN TRAN; EXEC (@sql); SET @r = @@ROWCOUNT; ROLLBACK;
        INSERT @res VALUES (CASE @pass WHEN 1 THEN 'with index' ELSE 'index disabled' END, @i, @r, DATEDIFF(MILLISECOND, @s, SYSDATETIME()));
        SET @i += 1;
    END
    SET @pass += 1;
END
ALTER INDEX IX_PerfTest_PolicyDetails_InsuredDetail_Id ON dbo.Accounting_PolicyDetails REBUILD;
SELECT state, run, rows_, ms FROM @res ORDER BY state DESC, run;
SELECT name, is_disabled FROM sys.indexes WHERE name = N'IX_PerfTest_PolicyDetails_InsuredDetail_Id';
