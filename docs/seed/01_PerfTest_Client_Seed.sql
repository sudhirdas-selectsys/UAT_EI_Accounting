/*
===============================================================================
 DRAFT FOR REVIEW - NOT EXECUTED
 Purpose : Create the synthetic performance-test client and the per-client
           configuration the application expects before policies/invoices can
           exist for it. Local Express database only.
 Target  : [uat-accounting] on (localdb)\MSSQLLocalDB  (guarded below)
 Touches : Accounting_Clients, Accounting_LU_LineItems_Clients,
           Accounting_LU_LineItems_Directions,
           Accounting_LU_BillingType_LineItems_Directions,
           Accounting_LU_PaymentPlans, Accounting_Users.
           INSERTS ONLY. Nothing is updated or deleted.
 Uses the application's own stored procedures wherever C# uses them:
           USP_Accounting_InsertUpdateLineItems_Clients
           USP_Accounting_InsertUpdateLineItems_Directions
           USP_Accounting_InsertBillingType_LineItems_Directions
 Direct INSERTs are used only where C# has no creation path (client, payment
 plan row shape, user), as the C# code never creates a client.

 PRECONDITION (already done, see docs/DB_Change_and_TestData_Log.md #2):
   Accounting_LU_LineItems_Clients identity reseeded to 2000, so new ids do not
   collide with orphan LineItems_Clients_Id 1-1663 rows in LineItems_Directions.

 WHERE THE LINE-ITEM CONFIGURATION COMES FROM (evidence, not guesses):
   The 19 "base" rows in Accounting_LU_LineItems_Directions
   (LineItems_Clients_Id IS NULL) and their billing-type mappings in
   Accounting_LU_BillingType_LineItems_Directions form the default template.
   This script mirrors that template for billing types 1 (Agency), 2 (Insured)
   and 3 (Direct) for the line items the generator will use. The only addition
   is EntityType 7 (State) on the Surplus Lines AP row, which is the most
   common client pattern among the orphan rows (232 of the most common profiles).

 ASSUMPTIONS THAT NEED YOUR CONFIRMATION (search for "REVIEW"):
   R1 AccountingType_Id (Cash/Accrual) and AccountingService_Id.
   R2 Line-item flags (Is_Passthrough, Is_Excluded, Is_Commissionable,
      TreatAsPremium): the base template carries none; all set to 0 except
      noted. Real clients vary.
   R3 Fee/tax line items limited to: Surplus Lines (tax), Inspection Fee,
      MGA Fee. Other line items are not configured for this client.
   R4 Payment plan: 25% down, 2 monthly installments => 3 invoices/policy,
      matching the accepted row model.
   R5 User password is a placeholder; set a real one through the application.
   R6 LU_ReportTemplates is NOT seeded here (needs template content I do not
      have). See bottom of file.
===============================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'uat-accounting'
    THROW 50001, 'Wrong database. Aborting.', 1;

IF EXISTS (SELECT 1 FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT')
    THROW 50002, 'Test client already exists. Aborting (script is not re-runnable).', 1;

IF EXISTS (SELECT 1 FROM dbo.Accounting_LU_LineItems_Clients)
   AND (SELECT MAX(LineItems_Clients_Id) FROM dbo.Accounting_LU_LineItems_Clients) < 1664
    THROW 50003, 'LU_LineItems_Clients contains ids inside the orphan range. Aborting.', 1;

IF IDENT_CURRENT('dbo.Accounting_LU_LineItems_Clients') < 1664
    THROW 50004, 'LU_LineItems_Clients identity is not reseeded above 1663. Aborting.', 1;

BEGIN TRY
BEGIN TRANSACTION;

DECLARE @Client_Id bigint;

------------------------------------------------------------------------------
-- 1. Client
--    REVIEW R1: AccountingType_Id 1=Cash, 2=Accrual. AccountingService_Id 7 =
--    "EI New API": journal entries are only created for Intuit(1)/Sage(6), so
--    7 keeps external journal calls out of the test. PaymentService_Id left
--    NULL (ePay/CorePay not exercised). Corporation_Id/Logo left NULL.
------------------------------------------------------------------------------
INSERT INTO dbo.Accounting_Clients
    (CompanyName, CompanyCode, Address, City, State, ZipCode, Email, Phone,
     Status, CreatedOn, AccountingService_Id, AccountingType_Id, PaymentService_Id)
VALUES
    ('PERFTEST_CLIENT', 'PERFTEST', 'Synthetic performance-test client', 'Testville', 'TX', '75001',
     'perftest@example.invalid', '000-000-0000',
     1, GETDATE(), 7, 2 /* REVIEW R1: 2 = Accrual */, NULL);

SET @Client_Id = SCOPE_IDENTITY();

------------------------------------------------------------------------------
-- 2. Line-item configuration (mirrors the base template rows)
--    Map: LineItem, Direction (1 AR, 2 AP, 3 Internal, 4 Calculated),
--         EntityType (NULL = decided by C# from the policy), billing types.
------------------------------------------------------------------------------
DECLARE @Map TABLE (
    Seq int IDENTITY(1,1),
    LineItem_Id bigint,
    Direction_Id tinyint,
    EntityType_Id tinyint NULL,
    BillingTypes varchar(20),
    Note varchar(80));

INSERT INTO @Map (LineItem_Id, Direction_Id, EntityType_Id, BillingTypes, Note) VALUES
 (1 , 1, NULL, '1,2'  , 'Premium AR (base LID 1)'),
 (2 , 2, NULL, '1,2'  , 'Carrier Remittance AP (base LID 2)'),
 (3 , 2, NULL, '2,3'  , 'Agency/Broker Commission AP (base LID 3)'),
 (3 , 4, NULL, '1,3'  , 'Agency/Broker Commission Calculated (base LID 15)'),
 (5 , 1, NULL, '1,2'  , 'Surplus Lines tax AR (base LID 5)'),
 (5 , 2, 7   , '1,2'  , 'Surplus Lines tax AP -> State; base LID 6, entity 7 per common client pattern'),
 (7 , 1, NULL, '1,2'  , 'Inspection Fee AR (base LID 8)'),
 (10, 1, NULL, '1,2'  , 'MGA Fee AR (base LID 13)'),
 (11, 3, NULL, '1,2,3', 'MGA Commission Internal (base LID 11)'),
 (12, 1, NULL, '3'    , 'Total Commission AR (base LID 16)'),
 (13, 3, NULL, '3'    , 'Total Premium Internal (base LID 17)'),
 (14, 3, NULL, '3'    , 'Total Commission % Internal (base LID 18)'),
 (15, 1, NULL, '1'    , 'House Commission AR (base LID 19)');

-- One LineItems_Clients row per distinct line item.
-- REVIEW R2: flags all 0 (Is_Passthrough, Is_Excluded, Is_Commissionable).
-- TreatAsPremium has no parameter in the app procedure; it stays at the table default.
DECLARE @LicMap TABLE (LineItem_Id bigint PRIMARY KEY, LineItems_Clients_Id bigint);

DECLARE @li bigint, @lic bigint;
DECLARE li_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT LineItem_Id FROM @Map ORDER BY LineItem_Id;
OPEN li_cur;
FETCH NEXT FROM li_cur INTO @li;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @lic = 0;
    EXEC dbo.USP_Accounting_InsertUpdateLineItems_Clients
         @LineItems_Clients_Id = @lic OUTPUT,
         @LineItem_Id = @li,
         @Client_Id = @Client_Id,
         @Is_Passthrough = 0,
         @Is_Excluded = 0,
         @Is_Commissionable = 0;
    INSERT INTO @LicMap VALUES (@li, @lic);
    FETCH NEXT FROM li_cur INTO @li;
END
CLOSE li_cur; DEALLOCATE li_cur;

-- Directions (one per LIC + direction) and billing-type mappings.
DECLARE @seq int, @dir tinyint, @ent tinyint, @bts varchar(20), @lid int, @bld int;
DECLARE map_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT Seq, LineItem_Id, Direction_Id, EntityType_Id, BillingTypes FROM @Map ORDER BY Seq;
OPEN map_cur;
FETCH NEXT FROM map_cur INTO @seq, @li, @dir, @ent, @bts;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @lic = LineItems_Clients_Id FROM @LicMap WHERE LineItem_Id = @li;
    SET @lid = 0;
    EXEC dbo.USP_Accounting_InsertUpdateLineItems_Directions
         @LineItems_Directions_Id = @lid OUTPUT,
         @Direction_Id = @dir,
         @LineItems_Clients_Id = @lic,
         @EntityType_Id = @ent;

    DECLARE @bt tinyint;
    DECLARE bt_cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT CAST(value AS tinyint) FROM STRING_SPLIT(@bts, ',');
    OPEN bt_cur;
    FETCH NEXT FROM bt_cur INTO @bt;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @bld = 0;
        EXEC dbo.USP_Accounting_InsertBillingType_LineItems_Directions
             @BillingType_LineItems_Directions_Id = @bld OUTPUT,
             @BillingType_Id = @bt,
             @LineItems_Directions_Id = @lid;
        FETCH NEXT FROM bt_cur INTO @bt;
    END
    CLOSE bt_cur; DEALLOCATE bt_cur;

    FETCH NEXT FROM map_cur INTO @seq, @li, @dir, @ent, @bts;
END
CLOSE map_cur; DEALLOCATE map_cur;

------------------------------------------------------------------------------
-- 3. Payment plan (REVIEW R4)
--    25% down, 2 monthly installments, anchored on Effective Date.
--    => down payment + 2 installments = 3 invoices per policy.
--    Code 'PT3' is what the generator will pass as PaymentPlan.
------------------------------------------------------------------------------
INSERT INTO dbo.Accounting_LU_PaymentPlans
    (Name, Client_Id, Carrier_Id, DownPaymentPercent, DownPaymentMinimum, DownPaymentTermDays,
     PaymentAnchorType_Id, NumberOfPayments, InstallmentBasisType_Id, IntervalUnitType_Id,
     IntervalValue, FirstPaymentOffsetDays, FixedDayOfMonth, IsSetBillDateEqualDue, LeadTime,
     IsInstallmentFeeSetup, IsAutoSendInvoices, Status, CreatedOn, Code)
VALUES
    (N'PerfTest 25% down + 2 monthly', @Client_Id, NULL, 25, 0, 0,
     1 /* Effective Date */, 2, 1 /* Effective Date */, 2 /* Months */,
     1, NULL, NULL, 1, 0,
     0, 0, 1, GETDATE(), N'PT3');

------------------------------------------------------------------------------
-- 4. User for report/page access (REVIEW R5)
--    Role 9 = Customer Admin. Password is a placeholder, NOT a working
--    credential: set a real one through the application before UI use.
------------------------------------------------------------------------------
INSERT INTO dbo.Accounting_Users
    (Client_Id, Username, Password, FirstName, LastName, Email, Role_Id, Is_APIUser,
     Status, CreatedOn, DefaultClient_Id)
VALUES
    (@Client_Id, 'perftest.user', 'PLACEHOLDER-SET-VIA-APP', 'Perf', 'Test',
     'perftest.user@example.invalid', 9, 0, 1, GETDATE(), @Client_Id);

COMMIT TRANSACTION;

SELECT @Client_Id AS NewClient_Id;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

/*
===============================================================================
 POST-APPLY VERIFICATION (read-only; run after the script above)
===============================================================================
-- a) First LIC id issued is >= 1664 and none collide with orphans
SELECT MIN(LineItems_Clients_Id), MAX(LineItems_Clients_Id), COUNT(*)
FROM Accounting_LU_LineItems_Clients;                       -- expect 11 rows, ids >= 2000

-- b) What the application will read, per billing type (same proc C# calls)
DECLARE @c bigint = (SELECT Client_Id FROM Accounting_Clients WHERE CompanyName='PERFTEST_CLIENT');
EXEC USP_Accounting_GetLineItems_ClientsByBillingType_Id @Client_Id=@c, @BillingType_Id=1;
EXEC USP_Accounting_GetLineItems_ClientsByBillingType_Id @Client_Id=@c, @BillingType_Id=2;
EXEC USP_Accounting_GetLineItems_ClientsByBillingType_Id @Client_Id=@c, @BillingType_Id=3;
-- expect: no row whose LineItem_Id is outside {1,2,3,5,7,10,11,12,13,14,15}

-- c) Row counts written by this script
SELECT 'Clients', COUNT(*) FROM Accounting_Clients WHERE CompanyName='PERFTEST_CLIENT'
UNION ALL SELECT 'LIC', COUNT(*) FROM Accounting_LU_LineItems_Clients WHERE Client_Id=@c
UNION ALL SELECT 'PaymentPlans', COUNT(*) FROM Accounting_LU_PaymentPlans WHERE Client_Id=@c
UNION ALL SELECT 'Users', COUNT(*) FROM Accounting_Users WHERE Client_Id=@c;

===============================================================================
 NOT SEEDED HERE
===============================================================================
 * Accounting_LU_ReportTemplates (Report_Id, Client_Id, Name, Code, Template):
   needs the HTML template content used by the template-based reports
   (Direct Bill, Agency Bill, Insured Bill, Agency Detailed, Underwriter ...).
   Please provide the template source (or confirm these reports should be
   excluded from the test), then it can be seeded with @Client_Id.
 * Optional ClientSettings keys (IsSingleInvoiceCreated, BillingDueDateRule*,
   DefaultCommissionRule, HasDefaultPaymentTypes, ShowCarrierReference_IdInDropdown):
   none seeded, so the application uses its defaults (installment path whenever
   a PaymentPlan is supplied). Tell me if a report needs one of them.
 * Operating bank, chart of accounts, commission rules: not required for the
   NEW-policy/manual-payment flow per the C# trace; Payments.OperatingBank_Id
   stays NULL unless you want a bank row.
 * Policy, agent, carrier, insured, underwriter rows: created by the generator.

===============================================================================
 ROLLBACK (documentation only; NOT run; deleting rows needs your approval)
===============================================================================
 All rows are tagged by the new Client_Id (and LineItems_Clients_Id >= 2000):
   Accounting_BillingType_... / LineItems_Directions rows whose
   LineItems_Clients_Id is in the new LIC ids, then LIC, PaymentPlans,
   Users, Clients - in that order.
===============================================================================
*/
