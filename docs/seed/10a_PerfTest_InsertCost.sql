/* Write-cost probe, Standard COPY ONLY (uat-accounting-perf). Inserts copies of existing rows inside a transaction
   and ROLLS BACK (no data kept; identity values are consumed). Run before and after creating the candidate indexes.
   sqlcmd -I -b */
SET NOCOUNT ON;
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @res TABLE (tbl varchar(50), rows_ int, ms int);
DECLARE @t varchar(60), @n int, @cols nvarchar(max), @sql nvarchar(max), @s datetime2;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT * FROM (VALUES ('Accounting_InvoiceDetails', 200000), ('Accounting_PolicyDetailsLineItems', 100000)) v(t,n);
OPEN c; FETCH NEXT FROM c INTO @t, @n;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @cols = STRING_AGG(CAST(QUOTENAME(name) AS nvarchar(max)), ',') FROM sys.columns
     WHERE object_id = OBJECT_ID('dbo.' + @t) AND is_identity = 0 AND is_computed = 0 AND system_type_id <> 189;
    SET @sql = N'INSERT INTO dbo.' + @t + N' (' + @cols + N') SELECT TOP (' + CAST(@n AS varchar(12)) + N') ' + @cols + N' FROM dbo.' + @t + N' WHERE Client_Id = 1;';
    CHECKPOINT;
    SET @s = SYSDATETIME();
    BEGIN TRAN; EXEC (@sql); DECLARE @r int = @@ROWCOUNT; ROLLBACK;
    INSERT @res VALUES (@t, @r, DATEDIFF(MILLISECOND, @s, SYSDATETIME()));
    FETCH NEXT FROM c INTO @t, @n;
END
CLOSE c; DEALLOCATE c;
SELECT tbl, rows_, ms FROM @res;
