/*
 Adds a missing access path on Accounting_PolicyDetails for joins on InsuredDetail_Id.
 Evidence (LOCAL TEST on a restored copy, SQL Server 2022 Standard, about 890,000 policies; not a production
 measurement): the Payment Report joins insured to policy on InsuredDetail_Id. No existing index on this table
 has InsuredDetail_Id as a key, so each outer row scanned the Status = 1 rows. With this index the unchanged
 USP_Accounting_PaymentReport went from 111-121 s to 2-6 s for page 1 (page size 100); row counts were identical.
 See docs/Iteration1_SlowQuery_Analysis.md (Addenda 3b and 4).
 Size on the test copy: about 26 MB. Write cost (local test, rolled-back insert of 100,000 policy rows, 3 runs each): 27.2 to 36.3 s with the index (mean 31.7 s) vs 24.8 to 29.5 s with it disabled (mean 27.4 s), about 16% slower on average but the ranges overlap (run-to-run variation on the shared machine is of the same size). Not measured: updates, behaviour under concurrent load.
 Additive and re-runnable (skips if the index exists). Apply manually, like the other scripts in this folder.
 Revert: DROP INDEX NCIX_Accounting_PolicyDetails_InsuredDetail_Id_Client_Id ON dbo.Accounting_PolicyDetails;
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'NCIX_Accounting_PolicyDetails_InsuredDetail_Id_Client_Id'
                 AND object_id = OBJECT_ID(N'dbo.Accounting_PolicyDetails'))
BEGIN
    CREATE NONCLUSTERED INDEX NCIX_Accounting_PolicyDetails_InsuredDetail_Id_Client_Id
        ON dbo.Accounting_PolicyDetails (InsuredDetail_Id, Client_Id)
        INCLUDE (Status);
END
GO
