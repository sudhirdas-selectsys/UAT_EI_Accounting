/*
 Cleanup of the v1 calibration load (user-approved: "Delete and reload with fixed generator").
 Deletes ONLY transactional rows written by 03_PerfTest_Generator.sql v1 for PERFTEST_CLIENT:
   PaymentApplicationLineItems, PaymentApplications, Payments, InvoiceDetails, Invoices,
   PolicyPaymentSchedule, PolicyDetailsLineItems, PolicyDetails, InsuredDetails, PerfTest_LoadManifest rows.
 KEEPS: the client, line-item config, payment plan, user, shared entities (agents, carriers,
        underwriters, wholesalers), the manifest table itself, all lookup data.
 Safety: aborts unless the current row counts equal what the v1 manifest recorded.
 Deletes in chunks of 100,000 so the transaction log stays small. Run with sqlcmd -I -b.
*/
SET NOCOUNT ON;
IF DB_NAME() <> N'uat-accounting' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @Client bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client IS NULL THROW 50002, 'Test client not found. Aborting.', 1;

DECLARE @expPol int = (SELECT SUM(Policies) FROM dbo.PerfTest_LoadManifest WHERE Client_Id = @Client AND Outcome = 'COMMITTED'),
        @expInv int = (SELECT SUM(Invoices) FROM dbo.PerfTest_LoadManifest WHERE Client_Id = @Client AND Outcome = 'COMMITTED');
IF (SELECT COUNT(*) FROM dbo.Accounting_PolicyDetails) <> @expPol
   OR (SELECT COUNT(*) FROM dbo.Accounting_Invoices) <> @expInv
   OR EXISTS (SELECT 1 FROM dbo.Accounting_PolicyDetails WHERE Client_Id <> @Client)
   OR EXISTS (SELECT 1 FROM dbo.Accounting_Invoices WHERE Client_Id <> @Client OR Client_Id IS NULL)
    THROW 50003, 'Row counts differ from the manifest or other clients exist. Aborting, nothing deleted.', 1;

DECLARE @n int, @tot int;

SET @tot = 0; WHILE 1 = 1 BEGIN
  DELETE TOP (100000) li FROM dbo.Accounting_PaymentApplicationLineItems li
    JOIN dbo.Accounting_PaymentApplications pa ON pa.PaymentApplication_Id = li.PaymentApplication_Id
    JOIN dbo.Accounting_Payments p ON p.Payment_Id = pa.Payment_Id WHERE p.Client_Id = @Client;
  SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'PaymentApplicationLineItems deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN
  DELETE TOP (100000) pa FROM dbo.Accounting_PaymentApplications pa
    JOIN dbo.Accounting_Payments p ON p.Payment_Id = pa.Payment_Id WHERE p.Client_Id = @Client;
  SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'PaymentApplications deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_Payments WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'Payments deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_InvoiceDetails WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'InvoiceDetails deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_Invoices WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'Invoices deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_PolicyPaymentSchedule WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'PolicyPaymentSchedule deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_PolicyDetailsLineItems WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'PolicyDetailsLineItems deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_PolicyDetails WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'PolicyDetails deleted: ' + CAST(@tot AS varchar(20));

SET @tot = 0; WHILE 1 = 1 BEGIN DELETE TOP (100000) FROM dbo.Accounting_InsuredDetails WHERE Client_Id = @Client; SET @n = @@ROWCOUNT; SET @tot += @n; IF @n = 0 BREAK; END
PRINT 'InsuredDetails deleted: ' + CAST(@tot AS varchar(20));

DELETE FROM dbo.PerfTest_LoadManifest WHERE Client_Id = @Client;
PRINT 'Manifest rows deleted: ' + CAST(@@ROWCOUNT AS varchar(20));

SELECT 'remaining rows' AS what,
  (SELECT COUNT(*) FROM dbo.Accounting_PolicyDetails) AS policies, (SELECT COUNT(*) FROM dbo.Accounting_Invoices) AS invoices,
  (SELECT COUNT(*) FROM dbo.Accounting_InvoiceDetails) AS details, (SELECT COUNT(*) FROM dbo.Accounting_Payments) AS payments,
  (SELECT COUNT(*) FROM dbo.Accounting_PaymentApplications) AS apps, (SELECT COUNT(*) FROM dbo.Accounting_PaymentApplicationLineItems) AS pali,
  (SELECT COUNT(*) FROM dbo.Accounting_InsuredDetails) AS insureds, (SELECT COUNT(*) FROM dbo.PerfTest_LoadManifest) AS manifest;
SELECT 'kept' AS what,
  (SELECT COUNT(*) FROM dbo.Accounting_Clients WHERE Client_Id = @Client) AS client,
  (SELECT COUNT(*) FROM dbo.Accounting_LU_LineItems_Clients WHERE Client_Id = @Client) AS lic,
  (SELECT COUNT(*) FROM dbo.Accounting_AgentDetails WHERE Client_Id = @Client) AS agents,
  (SELECT COUNT(*) FROM dbo.Accounting_CarrierDetails WHERE Client_Id = @Client) AS carriers,
  (SELECT COUNT(*) FROM dbo.Accounting_UnderWriterDetails WHERE Client_Id = @Client) AS uws,
  (SELECT COUNT(*) FROM dbo.Accounting_WholesalerDetails WHERE Client_Id = @Client) AS whs,
  (SELECT COUNT(*) FROM dbo.Accounting_LU_PaymentPlans WHERE Client_Id = @Client) AS plans,
  (SELECT COUNT(*) FROM dbo.Accounting_Users WHERE Client_Id = @Client) AS users;
