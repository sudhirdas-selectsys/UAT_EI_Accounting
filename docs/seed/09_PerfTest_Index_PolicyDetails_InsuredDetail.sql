/*
 Perf-test index, Standard-edition COPY ONLY (uat-accounting-perf on MSSQLSERVER2022). Not for production.
 Reason: no index on Accounting_PolicyDetails has InsuredDetail_Id as a key; the Payment Report join
 (ins.InsuredDetail_Id = APD.InsuredDetail_Id) scans Status=1 rows (about 890,000) per outer row.
 Revert (when you decide): DROP INDEX IX_PerfTest_PolicyDetails_InsuredDetail_Id ON dbo.Accounting_PolicyDetails;
 Run with sqlcmd -I -b.
*/
SET NOCOUNT ON;
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PerfTest_PolicyDetails_InsuredDetail_Id' AND object_id = OBJECT_ID(N'dbo.Accounting_PolicyDetails'))
    THROW 50002, 'Index already exists. Aborting.', 1;
CREATE NONCLUSTERED INDEX IX_PerfTest_PolicyDetails_InsuredDetail_Id
    ON dbo.Accounting_PolicyDetails (InsuredDetail_Id, Client_Id) INCLUDE (Status);
SELECT CAST(i.name AS varchar(60)) idx, CAST(SUM(ps.used_page_count) * 8 / 1024 AS int) size_mb
FROM sys.indexes i JOIN sys.dm_db_partition_stats ps ON ps.object_id = i.object_id AND ps.index_id = i.index_id
WHERE i.name = N'IX_PerfTest_PolicyDetails_InsuredDetail_Id' GROUP BY i.name;
