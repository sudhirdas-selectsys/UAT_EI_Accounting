/*
 Part of the approved 50,000-invoice calibration load (see docs/DB_Change_and_TestData_Log.md).
 Creates (inserts only):
   * dbo.PerfTest_LoadManifest  - new table recording every generator batch
   * shared entity rows for the test client: 300 agents, 12 carriers, 8 underwriters, 25 wholesalers
 Insureds are created per policy by the generator.
 Run with: sqlcmd -I -b -i 03a_PerfTest_Entities_and_Manifest.sql
 Not re-runnable: aborts if entities already exist.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'uat-accounting' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @Client bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client IS NULL THROW 50002, 'Test client not found. Aborting.', 1;
IF EXISTS (SELECT 1 FROM dbo.Accounting_AgentDetails WHERE Client_Id = @Client)
    THROW 50003, 'Entities already exist for the test client. Aborting.', 1;

BEGIN TRY
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.PerfTest_LoadManifest') IS NULL
    CREATE TABLE dbo.PerfTest_LoadManifest (
        Batch_Id        int IDENTITY(1,1) PRIMARY KEY,
        Client_Id       bigint      NOT NULL,
        StartPolicySeq  int         NOT NULL,
        EndPolicySeq    int         NOT NULL,
        Policies        int         NOT NULL,
        Invoices        int         NOT NULL,
        InvoiceDetails  int         NOT NULL,
        Payments        int         NOT NULL,
        PaymentAppLineItems int     NOT NULL,
        Outcome         varchar(20) NOT NULL,   -- COMMITTED / DRYRUN
        StartedOn       datetime2   NOT NULL,
        FinishedOn      datetime2   NOT NULL,
        DurationMs      int         NOT NULL,
        DataFileMB      int         NULL
    );

DECLARE @n TABLE (i int);
INSERT INTO @n SELECT TOP (300) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM sys.all_objects;

DECLARE @states TABLE (i int, st varchar(2));
INSERT INTO @states VALUES (0,'TX'),(1,'FL'),(2,'CA'),(3,'NY'),(4,'IL'),(5,'GA'),(6,'OH'),(7,'PA'),(8,'NC'),(9,'AZ');

INSERT INTO dbo.Accounting_AgentDetails
    (Client_Id, AgentReference_Id, Name, Address1, City, State, ZipCode, Email, ProducerType, AgentStatus)
SELECT @Client, 'PT-AG-' + RIGHT('0000' + CAST(n.i AS varchar(10)), 4), 'PerfTest Agency ' + CAST(n.i AS varchar(10)),
       CAST(n.i AS varchar(10)) + ' Main St', 'Testville', s.st, '75' + RIGHT('000' + CAST(n.i AS varchar(10)), 3),
       'agency' + CAST(n.i AS varchar(10)) + '@example.invalid', 'Agency', 'Active'
FROM @n n JOIN @states s ON s.i = n.i % 10;

INSERT INTO dbo.Accounting_CarrierDetails (Client_Id, CarrierReference_Id, Name, Address1, City, State, ZipCode, CarrierStatus)
SELECT TOP (12) @Client, 'PT-CA-' + CAST(n.i AS varchar(10)), 'PerfTest Carrier ' + CAST(n.i AS varchar(10)),
       '1 Carrier Way', 'Hartford', s.st, '06101', 'Active'
FROM @n n JOIN @states s ON s.i = n.i % 10 ORDER BY n.i;

INSERT INTO dbo.Accounting_UnderWriterDetails (Client_Id, UWReference_Id, Name, Address1, City, State, ZipCode)
SELECT TOP (8) @Client, 'PT-UW-' + CAST(n.i AS varchar(10)), 'PerfTest Underwriter ' + CAST(n.i AS varchar(10)),
       '1 UW Plaza', 'Austin', s.st, '73301'
FROM @n n JOIN @states s ON s.i = n.i % 10 ORDER BY n.i;

INSERT INTO dbo.Accounting_WholesalerDetails (Client_Id, WholesalerReference_Id, Name, DisplayName, IsActive)
SELECT TOP (25) @Client, 'PT-WH-' + CAST(n.i AS varchar(10)), 'PerfTest Wholesaler ' + CAST(n.i AS varchar(10)),
       'PerfTest Wholesaler ' + CAST(n.i AS varchar(10)), 1
FROM @n n ORDER BY n.i;

COMMIT TRANSACTION;

SELECT (SELECT COUNT(*) FROM dbo.Accounting_AgentDetails WHERE Client_Id=@Client)       AS Agents,
       (SELECT COUNT(*) FROM dbo.Accounting_CarrierDetails WHERE Client_Id=@Client)     AS Carriers,
       (SELECT COUNT(*) FROM dbo.Accounting_UnderWriterDetails WHERE Client_Id=@Client) AS Underwriters,
       (SELECT COUNT(*) FROM dbo.Accounting_WholesalerDetails WHERE Client_Id=@Client)  AS Wholesalers;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
