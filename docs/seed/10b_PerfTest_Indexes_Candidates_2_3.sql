/* Candidate indexes 2 and 3, Standard COPY ONLY (uat-accounting-perf on MSSQLSERVER2022). Not for production.
   Revert (when you decide): DROP INDEX IX_PerfTest_InvoiceDetails_BLD_Status_Client ON dbo.Accounting_InvoiceDetails;
                             DROP INDEX IX_PerfTest_PolicyDetailsLineItems_LineType ON dbo.Accounting_PolicyDetailsLineItems;
   sqlcmd -I -b */
SET NOCOUNT ON;
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name IN (N'IX_PerfTest_InvoiceDetails_BLD_Status_Client', N'IX_PerfTest_PolicyDetailsLineItems_LineType')) THROW 50002, 'Index already exists. Aborting.', 1;
DECLARE @s datetime2 = SYSDATETIME();
CREATE NONCLUSTERED INDEX IX_PerfTest_InvoiceDetails_BLD_Status_Client ON dbo.Accounting_InvoiceDetails (BillingType_LineItems_Directions_Id, Status, Client_Id) INCLUDE (Invoice_Id, Amount, LOB_Id);
PRINT 'InvoiceDetails index built in ' + CAST(DATEDIFF(SECOND, @s, SYSDATETIME()) AS varchar(10)) + ' s';
SET @s = SYSDATETIME();
CREATE NONCLUSTERED INDEX IX_PerfTest_PolicyDetailsLineItems_LineType ON dbo.Accounting_PolicyDetailsLineItems (LineType, Status, Client_Id) INCLUDE (PolicyDetails_Id, Premium);
PRINT 'PolicyDetailsLineItems index built in ' + CAST(DATEDIFF(SECOND, @s, SYSDATETIME()) AS varchar(10)) + ' s';
SELECT CAST(i.name AS varchar(60)) idx, CAST(SUM(ps.used_page_count) * 8 / 1024 AS int) size_mb
FROM sys.indexes i JOIN sys.dm_db_partition_stats ps ON ps.object_id = i.object_id AND ps.index_id = i.index_id
WHERE i.name IN (N'IX_PerfTest_InvoiceDetails_BLD_Status_Client', N'IX_PerfTest_PolicyDetailsLineItems_LineType') GROUP BY i.name;
