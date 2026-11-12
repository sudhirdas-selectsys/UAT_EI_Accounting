/*
 Addendum to 01_PerfTest_Client_Seed.sql (approved by the user).
 Why: for Insured Bill the C# builds the invoice from "Premium AR" (premium minus
 commission) plus a "Commission AR" line. The first seed had no Commission AR row
 for billing type 2, so insured invoice amounts would not equal their AR details.
 What: map the client's existing "Total Commission" (LineItem 12) AR direction row
 to billing type 2 (Insured Bill). Inserts ONE row through the application's own
 procedure. No other rows are touched.
 Run with: sqlcmd -I (QUOTED_IDENTIFIER ON).
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'uat-accounting' THROW 50001, 'Wrong database. Aborting.', 1;

DECLARE @Client_Id bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client_Id IS NULL THROW 50002, 'Test client not found. Aborting.', 1;

DECLARE @lic bigint = (SELECT LineItems_Clients_Id FROM dbo.Accounting_LU_LineItems_Clients
                       WHERE Client_Id = @Client_Id AND LineItem_Id = 12 AND Status = 1);
DECLARE @lid int = (SELECT LineItems_Directions_Id FROM dbo.Accounting_LU_LineItems_Directions
                    WHERE LineItems_Clients_Id = @lic AND Direction_Id = 1 AND Status = 1);
IF @lic IS NULL OR @lid IS NULL THROW 50003, 'Total Commission AR row not found. Aborting.', 1;

IF EXISTS (SELECT 1 FROM dbo.Accounting_LU_BillingType_LineItems_Directions
           WHERE BillingType_Id = 2 AND LineItems_Directions_Id = @lid AND Status = 1)
    THROW 50004, 'Mapping already exists. Aborting.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @bld int = 0;
    EXEC dbo.USP_Accounting_InsertBillingType_LineItems_Directions
         @BillingType_LineItems_Directions_Id = @bld OUTPUT,
         @BillingType_Id = 2,
         @LineItems_Directions_Id = @lid;
    COMMIT TRANSACTION;
    SELECT @bld AS New_BillingType_LineItems_Directions_Id, @lid AS LineItems_Directions_Id, @lic AS LineItems_Clients_Id;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
