/*
===============================================================================
 EI Accounting performance-test data generator (batch)
 Local Express database only.  Test client: PERFTEST_CLIENT.
 Usage : sqlcmd -S (localdb)\MSSQLLocalDB -E -C -d uat-accounting -I -b
               -v StartSeq=1 -v EndSeq=1800 -v Commit=1 -i 03_PerfTest_Generator.sql
         Commit=0 runs everything, prints reconciliation results and ROLLS BACK
         (identity/sequence values are consumed; harmless).
 Prereqs: 01_PerfTest_Client_Seed.sql, 02_..._Addendum.sql, 03a_..._Entities_and_Manifest.sql

 WHAT IT WRITES (inserts only), per policy sequence number n in [StartSeq, EndSeq]:
   Accounting_InsuredDetails, Accounting_PolicyDetails, Accounting_PolicyDetailsLineItems,
   Accounting_PolicyPaymentSchedule, Accounting_Invoices, Accounting_InvoiceDetails,
   Accounting_Payments, Accounting_PaymentApplications, Accounting_PaymentApplicationLineItems,
   PerfTest_LoadManifest (committed batches only).
 Invoice numbers (v3): a range is taken from Seq_Accounting_GenerateInvoiceNo and formatted
   'INV' + 11 zero-padded digits (length 11 as requested; v1/v2 used the application's 6-digit formula, which
   wraps after 999,999). Each batch validates format, in-batch uniqueness and non-existence before inserting.
 Versions: v1 CHECKSUM hashing (defective distribution) -> v2 multiplicative hashes -> v3 11-digit invoice numbers.

 BUSINESS-RULE SOURCE: formulas mirror C# (FinancialServicesController.GenerateInvoice /
 ScheduledInvoices / GetInvoiceDetailsWithInstallmentPlan, MakePaymentsController.ApplyPayments).
 Money is float with ROUND(...,2), as in C# (double + Math.Round).
 ASSUMPTIONS / DEVIATIONS (to be listed in the assessment):
   A1 NEW policies only; no journal entries (client accounting service = EI New API).
   A2 Premiums are multiples of $80, commission 15% total / 10% agent, so every split is an
      exact value and T-SQL ROUND (away from zero) cannot differ from C# banker's rounding.
   A3 Single-invoice policies (10%): detail amounts use the installment formulas with ratio 1
      and fees included (GetInvoiceDetails was not read line by line).
   A4 One payment per paid invoice (the accepted model was about 0.5 payments per invoice).
   A5 Partial payments apply 50% of every AR detail line; invoice RemainingBalance =
      Amount - applied (the C# partial branch does not clearly update the invoice header).
   A6 Schedule rows keep Paid = 0 (C# behaviour for schedule paid flags not traced).
   A7 Detail/invoice PaidOn = payment date (C# uses GETDATE()).
===============================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Start int = $(StartSeq), @End int = $(EndSeq), @Commit bit = $(Commit);
DECLARE @t0 datetime2 = SYSDATETIME();

IF DB_NAME() <> N'uat-accounting' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @Client bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client IS NULL THROW 50002, 'Test client not found. Aborting.', 1;
DECLARE @Plan int = (SELECT PaymentPlan_Id FROM dbo.Accounting_LU_PaymentPlans WHERE Client_Id = @Client AND Code = N'PT3' AND Status = 1);
IF @Plan IS NULL THROW 50003, 'Payment plan PT3 not found. Aborting.', 1;
IF OBJECT_ID('dbo.PerfTest_LoadManifest') IS NULL THROW 50004, 'Manifest table missing. Run 03a first.', 1;
IF EXISTS (SELECT 1 FROM dbo.PerfTest_LoadManifest WHERE Client_Id = @Client AND Outcome = 'COMMITTED'
           AND StartPolicySeq <= @End AND EndPolicySeq >= @Start)
    THROW 50005, 'Range overlaps an already committed batch. Aborting.', 1;
IF EXISTS (SELECT 1 FROM dbo.Accounting_PolicyDetails WHERE Client_Id = @Client AND Submission_Id BETWEEN 700000000 + @Start AND 700000000 + @End)
    THROW 50006, 'Policies already exist in this range. Aborting.', 1;
DECLARE @mb int = (SELECT SUM(size) / 128 FROM sys.database_files WHERE type = 0);
IF @mb > 9300 THROW 50007, 'Data file is near the Express 10 GB cap. Aborting.', 1;

DROP TABLE IF EXISTS #N, #Ag, #Ca, #Uw, #Wh, #P0, #P1, #P, #I0, #I, #Cfg, #Tpl, #S, #AR, #PayCalc, #Pm, #Pa;

BEGIN TRY
BEGIN TRANSACTION;

-- ---------------------------------------------------------------- batch numbers
SELECT TOP (@End - @Start + 1) n = CAST(@Start - 1 + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS int)
INTO #N FROM sys.all_objects a CROSS JOIN sys.all_objects b;

-- ---------------------------------------------------------------- shared entities
SELECT rn = ROW_NUMBER() OVER (ORDER BY AgentDetail_Id) - 1, id = AgentDetail_Id INTO #Ag FROM dbo.Accounting_AgentDetails WHERE Client_Id = @Client AND Status = 1;
SELECT rn = ROW_NUMBER() OVER (ORDER BY CarrierDetail_Id) - 1, id = CarrierDetail_Id INTO #Ca FROM dbo.Accounting_CarrierDetails WHERE Client_Id = @Client AND Status = 1;
SELECT rn = ROW_NUMBER() OVER (ORDER BY UnderWriterDetail_Id) - 1, id = UnderWriterDetail_Id INTO #Uw FROM dbo.Accounting_UnderWriterDetails WHERE Client_Id = @Client AND Status = 1;
SELECT rn = ROW_NUMBER() OVER (ORDER BY WholesalerDetail_Id) - 1, id = WholesalerDetail_Id INTO #Wh FROM dbo.Accounting_WholesalerDetails WHERE Client_Id = @Client AND Status = 1;
DECLARE @nAg int = (SELECT COUNT(*) FROM #Ag), @nCa int = (SELECT COUNT(*) FROM #Ca),
        @nUw int = (SELECT COUNT(*) FROM #Uw), @nWh int = (SELECT COUNT(*) FROM #Wh);
IF @nAg = 0 OR @nCa = 0 OR @nUw = 0 OR @nWh = 0 THROW 50008, 'Shared entities missing. Aborting.', 1;

-- ---------------------------------------------------------------- policy staging
-- Deterministic multiplicative (Fibonacci-style) hash, a different odd 32-bit multiplier per attribute:
--   bucket(m) = floor( frac(n * M / 2^32) * m ).  (CHECKSUM of small ints was far too patterned: v1 used only
--   75 of 300 agents; fixed in v2.)
SELECT n.n,
       k     = 5 + CAST(((CAST(n.n AS bigint) * 2654435761 + 12345) % 4294967296) * 121 / 4294967296 AS int),
       bt    = CAST(CASE WHEN (n.n - 1) % 10 < 5 THEN 1 WHEN (n.n - 1) % 10 < 8 THEN 2 ELSE 3 END AS tinyint),
       inst  = CAST(CASE WHEN ((n.n - 1) / 10) % 10 = 0 THEN 0 ELSE 1 END AS bit),
       eff   = CAST(DATEADD(DAY, (n.n * 37) % 540, '20250101') AS date),
       lobslot = n.n % 5,
       ag    = CAST(((CAST(n.n AS bigint) * 2246822519 + 12345) % 4294967296) * @nAg / 4294967296 AS int),
       ca    = CAST(((CAST(n.n AS bigint) * 3266489917 + 12345) % 4294967296) * @nCa / 4294967296 AS int),
       uw    = CAST(((CAST(n.n AS bigint) * 668265263  + 12345) % 4294967296) * @nUw / 4294967296 AS int),
       whflag= CASE WHEN ((CAST(n.n AS bigint) * 2860486313 + 12345) % 4294967296) * 10 / 4294967296 < 3 THEN 1 ELSE 0 END,
       wh    = CAST(((CAST(n.n AS bigint) * 374761393  + 12345) % 4294967296) * @nWh / 4294967296 AS int),
       st    = CHOOSE((n.n % 10) + 1, 'TX','FL','CA','NY','IL','GA','OH','PA','NC','AZ')
INTO #P0 FROM #N n;

SELECT p.*, Prem = CAST(80 * p.k AS float),
       AgentId = ag.id, CarrierId = ca.id, UwId = uw.id,
       WholesalerId = CASE WHEN p.whflag = 1 THEN wh.id ELSE CAST(0 AS bigint) END,
       LobId = lob.LOB_Id, LobName = lob.Name
INTO #P1
FROM #P0 p
JOIN #Ag ag ON ag.rn = p.ag JOIN #Ca ca ON ca.rn = p.ca JOIN #Uw uw ON uw.rn = p.uw JOIN #Wh wh ON wh.rn = p.wh
JOIN dbo.Accounting_LU_LOBs lob ON lob.LOB_Id = CHOOSE(p.lobslot + 1, 1, 2, 11, 12, 13);

-- money (float, ROUND as in C#)
SELECT p.*, AGT = ROUND(p.Prem * 10e0 / 100e0, 2), COMM = ROUND(p.Prem * 15e0 / 100e0, 2) INTO #P FROM #P1 p;
ALTER TABLE #P ADD HSE float, FSL float, FINS float, FMGA float, FT float, B float, D float, REM float, BASEAMT float, LASTAMT float,
                   InsuredId bigint, PolicyId bigint;
UPDATE #P SET HSE  = ROUND(COMM - AGT, 2),
              FSL  = CASE WHEN bt < 3 THEN ROUND(Prem * 2.5e0 / 100e0, 2) ELSE 0e0 END,
              FINS = CASE WHEN bt < 3 THEN 25e0 ELSE 0e0 END,
              FMGA = CASE WHEN bt < 3 THEN 50e0 ELSE 0e0 END,
              B    = CASE bt WHEN 1 THEN ROUND(Prem - AGT, 2) WHEN 2 THEN Prem ELSE COMM END;
UPDATE #P SET FT = FSL + FINS + FMGA, D = ROUND(B * 25e0 / 100e0, 2);
UPDATE #P SET REM = ROUND(B - D, 2);
UPDATE #P SET BASEAMT = ROUND(REM / 2e0, 2);
UPDATE #P SET LASTAMT = ROUND(REM - BASEAMT, 2);

-- ---------------------------------------------------------------- insureds (one per policy)
INSERT INTO dbo.Accounting_InsuredDetails (InsuredReference_Id, Name, Address1, City, State, ZipCode, Client_Id)
SELECT 'PT-INS-' + CAST(n AS varchar(12)), 'PerfTest Insured ' + CAST(n AS varchar(12)),
       CAST(n AS varchar(12)) + ' Insured Ln', 'Testville', st, '75001', @Client
FROM #P ORDER BY n;
UPDATE p SET InsuredId = i.InsuredDetail_Id
FROM #P p JOIN dbo.Accounting_InsuredDetails i ON i.Client_Id = @Client AND i.InsuredReference_Id = 'PT-INS-' + CAST(p.n AS varchar(12));

-- ---------------------------------------------------------------- policies
INSERT INTO dbo.Accounting_PolicyDetails
    (Client_Id, Submission_Id, PolicyNumber, Corporation, Location, RiskState, PaymentPlan, BillType,
     EffectiveDate, ExpirationDate, PolicyAction, PolicyType, TransactionType, PolicyDescription, PostAsShell,
     WholesalerDetail_Id, UnderWriterDetail_Id, InsuredDetail_Id, CarrierDetail_Id, AgentDetail_Id, Source_Id)
SELECT @Client, CAST(700000000 AS bigint) + n, 'PT-POL-' + RIGHT('00000000' + CAST(n AS varchar(12)), 8),
       'PerfTest Corp', 'HQ', st, CASE WHEN inst = 1 THEN 'PT3' END,
       CHOOSE(bt, 'Agency', 'Insured', 'Direct'),
       eff, DATEADD(YEAR, 1, eff), 'NEW', 'Commercial', 'NEW', 'Synthetic performance-test policy', 1,
       NULLIF(WholesalerId, 0), UwId, InsuredId, CarrierId, AgentId, 1
FROM #P ORDER BY n;
DECLARE @cPol int = @@ROWCOUNT;
UPDATE p SET PolicyId = pd.Id
FROM #P p JOIN dbo.Accounting_PolicyDetails pd ON pd.Client_Id = @Client AND pd.Submission_Id = CAST(700000000 AS bigint) + p.n;

-- ---------------------------------------------------------------- policy line items
INSERT INTO dbo.Accounting_PolicyDetailsLineItems
    (PolicyDetails_Id, Submission_Id, LineType, HouseCommission, AgentCommission, Coverage, PolicyDetailDescription,
     FeeTaxAmount, Premium, FeeTaxable, EffectiveDate, State, Is_Commissionable, BrokerCommissionAmount, CommissionAmount, Client_Id)
SELECT p.PolicyId, CAST(700000000 AS bigint) + p.n, v.LineType, v.Hse, v.Agt, p.LobName, v.Descr,
       v.Fee, v.Prem, 0, p.eff, p.st, v.IsComm, v.Brk, v.Cm, @Client
FROM #P p
CROSS APPLY (VALUES
   (1, 'PREMIUM', CAST(15 AS float), CAST(10 AS float), 'Premium',        CAST(0 AS float), p.Prem,           CAST(1 AS bit), p.AGT, p.COMM),
   (2, 'FEE',     CAST(NULL AS float), CAST(NULL AS float), 'Inspection Fee', p.FINS,       CAST(0 AS float), CAST(0 AS bit), CAST(NULL AS float), CAST(NULL AS float)),
   (3, 'FEE',     CAST(NULL AS float), CAST(NULL AS float), 'MGA Fee',        p.FMGA,       CAST(0 AS float), CAST(0 AS bit), CAST(NULL AS float), CAST(NULL AS float)),
   (4, 'TAX',     CAST(NULL AS float), CAST(NULL AS float), 'Surplus Lines',  p.FSL,        CAST(0 AS float), CAST(0 AS bit), CAST(NULL AS float), CAST(NULL AS float))
) v(ord, LineType, Hse, Agt, Descr, Fee, Prem, IsComm, Brk, Cm)
WHERE v.ord = 1 OR p.bt < 3;

-- ---------------------------------------------------------------- payment schedule (installment policies)
INSERT INTO dbo.Accounting_PolicyPaymentSchedule
    (PolicyDetail_Id, PaymentPlan_Id, InstallmentNo, IsDownPayment, DueDate, BillDate, Amount, Paid, SequenceNo, Client_Id)
SELECT p.PolicyId, @Plan, v.InstNo, v.IsDown, v.Due, v.Due, v.Amt, 0, v.Seq, @Client
FROM #P p
CROSS APPLY (VALUES
   (CAST(NULL AS tinyint), CAST(1 AS bit), p.eff,                  p.D,        CAST(1 AS tinyint)),
   (CAST(1 AS tinyint),    CAST(0 AS bit), DATEADD(MONTH, 1, p.eff), p.BASEAMT,  CAST(2 AS tinyint)),
   (CAST(2 AS tinyint),    CAST(0 AS bit), DATEADD(MONTH, 2, p.eff), p.LASTAMT,  CAST(3 AS tinyint))
) v(InstNo, IsDown, Due, Amt, Seq)
WHERE p.inst = 1;

-- ---------------------------------------------------------------- invoice staging
SELECT p.n, p.PolicyId, p.bt, p.inst, p.Prem, p.AGT, p.COMM, p.HSE, p.FSL, p.FINS, p.FMGA, p.FT, p.B,
       p.AgentId, p.CarrierId, p.WholesalerId, p.InsuredId, p.LobId, p.eff, i = v.i,
       InstNo = CASE WHEN p.inst = 1 THEN v.i ELSE 1 END,
       S = CASE WHEN p.inst = 0 THEN p.B WHEN v.i = 0 THEN p.D WHEN v.i = 1 THEN p.BASEAMT ELSE p.LASTAMT END,
       FeeFlag = CASE WHEN p.inst = 0 OR v.i = 0 THEN 1 ELSE 0 END,
       Due = CASE WHEN p.inst = 0 THEN DATEADD(DAY, 30, p.eff) WHEN v.i = 0 THEN p.eff ELSE DATEADD(MONTH, v.i, p.eff) END
INTO #I0
FROM #P p JOIN (VALUES (0), (1), (2)) v(i) ON p.inst = 1 OR v.i = 0;

SELECT i0.*,
       r      = CASE WHEN i0.inst = 0 THEN 1e0 ELSE ROUND(i0.S / i0.B, 10) END,
       Amt    = i0.S + CASE WHEN i0.FeeFlag = 1 THEN i0.FT ELSE 0e0 END,
       Sched  = s.PolicyPaymentSchedule_Id
INTO #I
FROM #I0 i0
LEFT JOIN dbo.Accounting_PolicyPaymentSchedule s
       ON i0.inst = 1 AND s.PolicyDetail_Id = i0.PolicyId AND s.SequenceNo = i0.i + 1 AND s.Client_Id = @Client;
ALTER TABLE #I ADD Premr float, Agtr float, Hser float, InvId bigint;
UPDATE #I SET Premr = ROUND(Prem * r, 2), Agtr = ROUND(AGT * r, 2), Hser = ROUND(HSE * r, 2);

-- Invoice numbers (v3, user requirement: length 11, unique, correctly formatted, validated before insert).
-- A RANGE is taken from Seq_Accounting_GenerateInvoiceNo (the same sequence USP_Accounting_GenerateInvoiceNo uses),
-- so numbers are never reused. Format: 'INV' + 11 zero-padded digits (14 chars). Note: calling
-- USP_Accounting_GenerateInvoiceNo with length 11 would NOT zero-pad values below 10,000 (its pad literal has only
-- six zeros), hence the explicit REPLICATE('0', 11).
DECLARE @cntInv int = (SELECT COUNT(*) FROM #I), @firstSeq sql_variant;
EXEC sys.sp_sequence_get_range @sequence_name = N'dbo.Seq_Accounting_GenerateInvoiceNo', @range_size = @cntInv, @range_first_value = @firstSeq OUTPUT;
DECLARE @f0 bigint = CAST(@firstSeq AS bigint);
ALTER TABLE #I ADD InvoiceNo varchar(100);
UPDATE i SET InvoiceNo = 'INV' + RIGHT(REPLICATE('0', 11) + CAST(@f0 + r.rn - 1 AS varchar(20)), 11)
FROM #I i JOIN (SELECT n, i, rn = ROW_NUMBER() OVER (ORDER BY n, i) FROM #I) r ON r.n = i.n AND r.i = i.i;
DECLARE @invPat varchar(100) = 'INV' + REPLICATE('[0-9]', 11);
IF EXISTS (SELECT 1 FROM #I WHERE InvoiceNo IS NULL OR LEN(InvoiceNo) <> 14 OR InvoiceNo NOT LIKE @invPat)
    THROW 50020, 'Generated invoice number has wrong format.', 1;
IF EXISTS (SELECT InvoiceNo FROM #I GROUP BY InvoiceNo HAVING COUNT(*) > 1)
    THROW 50021, 'Duplicate invoice numbers inside the batch.', 1;
-- Non-existence check without a join: this batch's numbers are one contiguous block, so any existing 14-char number
-- equal to one of them must lie inside [min,max] (a join on the un-indexed InvoiceNo is a nested loop at 2M rows).
DECLARE @minNo varchar(100) = (SELECT MIN(InvoiceNo) FROM #I), @maxNo varchar(100) = (SELECT MAX(InvoiceNo) FROM #I);
IF EXISTS (SELECT 1 FROM dbo.Accounting_Invoices WHERE LEN(InvoiceNo) = 14 AND InvoiceNo BETWEEN @minNo AND @maxNo)
    THROW 50022, 'A generated invoice number already exists in Accounting_Invoices.', 1;

INSERT INTO dbo.Accounting_Invoices (PolicyDetails_Id, InvoiceNo, InstallmentNo, DueDate, Amount, RemainingBalance, Schedule_Id, Client_Id)
SELECT PolicyId, InvoiceNo, InstNo, Due, Amt, Amt, Sched, @Client
FROM #I;
DECLARE @cInv int = @@ROWCOUNT;
UPDATE i SET InvId = inv.Id
FROM #I i JOIN dbo.Accounting_Invoices inv
  ON inv.Client_Id = @Client AND inv.PolicyDetails_Id = i.PolicyId AND inv.InstallmentNo = i.InstNo AND inv.Status = 1;

-- ---------------------------------------------------------------- invoice details
SELECT bt = BLD.BillingType_Id, li = LIC.LineItem_Id, dir = LID.Direction_Id, bld = BLD.BillingType_LineItems_Directions_Id
INTO #Cfg
FROM dbo.Accounting_LU_LineItems_Clients LIC
JOIN dbo.Accounting_LU_LineItems_Directions LID ON LID.LineItems_Clients_Id = LIC.LineItems_Clients_Id AND LID.Status = 1
JOIN dbo.Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.LineItems_Directions_Id = LID.LineItems_Directions_Id AND BLD.Status = 1
WHERE LIC.Client_Id = @Client AND LIC.Status = 1;

CREATE TABLE #Tpl (bt tinyint, li bigint, dir tinyint, kind varchar(12));
INSERT INTO #Tpl VALUES
 (1,1,1,'PREM_AR'),(1,2,2,'PREM_AP'),(1,15,1,'HSE_AR'),(1,11,3,'MGA_IN'),(1,3,4,'AGT_C'),
 (1,5,1,'FEE'),(1,5,2,'FEE'),(1,7,1,'FEE'),(1,10,1,'FEE'),
 (2,1,1,'PREM_AR'),(2,2,2,'PREM_AP'),(2,12,1,'COMM_AR'),(2,3,2,'AGT_AP'),(2,11,3,'MGA_IN'),
 (2,5,1,'FEE'),(2,5,2,'FEE'),(2,7,1,'FEE'),(2,10,1,'FEE'),
 (3,3,2,'AGT_AP'),(3,12,1,'COMM_AR'),(3,11,3,'MGA_IN'),(3,13,3,'TOTPREM_IN'),(3,14,3,'COMMPCT_IN');

INSERT INTO dbo.Accounting_InvoiceDetails
    (Invoice_Id, Amount, RemainingBalance, Entity_Id, EntityType_Id, BillingType_LineItems_Directions_Id, Paid, LOB_Id, Client_Id)
SELECT x.InvId, x.Amt, x.Amt, x.EntId, x.EntType, x.bld,
       CASE WHEN x.bt = 3 AND x.kind IN ('MGA_IN', 'TOTPREM_IN', 'COMMPCT_IN') THEN NULL ELSE CAST(0 AS bit) END,
       x.LobId, @Client
FROM (
  SELECT i.InvId, i.bt, i.LobId, t.kind, c.bld,
         Amt = CASE t.kind
                 WHEN 'PREM_AR'    THEN i.Premr - (i.Agtr + i.Hser)
                 WHEN 'PREM_AP'    THEN i.Premr - (i.Agtr + i.Hser)
                 WHEN 'HSE_AR'     THEN i.Hser
                 WHEN 'MGA_IN'     THEN i.Hser
                 WHEN 'AGT_C'      THEN i.Agtr
                 WHEN 'AGT_AP'     THEN i.Agtr
                 WHEN 'COMM_AR'    THEN i.Agtr + i.Hser
                 WHEN 'TOTPREM_IN' THEN i.Premr
                 WHEN 'COMMPCT_IN' THEN CAST(15 AS float)
                 WHEN 'FEE'        THEN CASE WHEN i.FeeFlag = 1
                                             THEN CASE t.li WHEN 5 THEN i.FSL WHEN 7 THEN i.FINS WHEN 10 THEN i.FMGA ELSE 0e0 END
                                             ELSE 0e0 END
               END,
         EntType = CAST(CASE
                 WHEN t.kind = 'PREM_AR'                    THEN CASE i.bt WHEN 1 THEN 1 ELSE 2 END
                 WHEN t.kind IN ('PREM_AP', 'TOTPREM_IN')   THEN CASE WHEN i.WholesalerId > 0 THEN 6 ELSE 3 END
                 WHEN t.kind = 'COMM_AR'                    THEN CASE WHEN i.bt = 2 THEN 2 WHEN i.WholesalerId > 0 THEN 6 ELSE 3 END
                 WHEN t.kind IN ('HSE_AR', 'AGT_C', 'AGT_AP') THEN 1
                 WHEN t.kind IN ('MGA_IN', 'COMMPCT_IN')    THEN 4
                 WHEN t.kind = 'FEE'                        THEN CASE i.bt WHEN 1 THEN 1 ELSE 2 END
               END AS tinyint),
         EntId = CAST(CASE
                 WHEN t.kind = 'PREM_AR'                    THEN CASE i.bt WHEN 1 THEN i.AgentId ELSE i.InsuredId END
                 WHEN t.kind IN ('PREM_AP', 'TOTPREM_IN')   THEN CASE WHEN i.WholesalerId > 0 THEN i.WholesalerId ELSE i.CarrierId END
                 WHEN t.kind = 'COMM_AR'                    THEN CASE WHEN i.bt = 2 THEN i.InsuredId WHEN i.WholesalerId > 0 THEN i.WholesalerId ELSE i.CarrierId END
                 WHEN t.kind IN ('HSE_AR', 'AGT_C', 'AGT_AP') THEN i.AgentId
                 WHEN t.kind IN ('MGA_IN', 'COMMPCT_IN')    THEN @Client
                 WHEN t.kind = 'FEE'                        THEN CASE i.bt WHEN 1 THEN i.AgentId ELSE i.InsuredId END
               END AS bigint)
  FROM #I i
  JOIN #Tpl t ON t.bt = i.bt
  JOIN #Cfg c ON c.bt = t.bt AND c.li = t.li AND c.dir = t.dir
) x
WHERE x.Amt <> 0 AND x.EntId > 0 AND x.EntType > 0;
DECLARE @cDet int = @@ROWCOUNT;

-- ---------------------------------------------------------------- payments / applications
SELECT i.InvId, i.n, i.i, i.bt, i.Due, i.AgentId, i.InsuredId, i.CarrierId, i.WholesalerId,
       state = CASE WHEN b.bucket < 6 THEN 'F' WHEN b.bucket = 6 THEN 'P' ELSE 'U' END   -- 60% paid, 10% partial, 30% unpaid
INTO #S FROM #I i
CROSS APPLY (SELECT bucket = CAST(((CAST(i.n AS bigint) * 2127912214 + CAST(i.i AS bigint) * 3432918353 + 12345) % 4294967296) * 10 / 4294967296 AS int)) b;

SELECT DetId = d.Id, InvId = d.Invoice_Id, d.Amount, s.state,
       Applied = CASE s.state WHEN 'F' THEN d.Amount ELSE ROUND(d.Amount / 2e0, 2) END
INTO #AR
FROM dbo.Accounting_InvoiceDetails d
JOIN #S s ON s.InvId = d.Invoice_Id
JOIN #Cfg c ON c.bld = d.BillingType_LineItems_Directions_Id AND c.dir = 1
WHERE d.Client_Id = @Client AND s.state <> 'U';

SELECT s.InvId, s.state, s.Due, s.bt, s.n, s.AgentId, s.InsuredId, s.CarrierId, s.WholesalerId,
       TotalApplied = SUM(a.Applied)
INTO #PayCalc
FROM #S s JOIN #AR a ON a.InvId = s.InvId
GROUP BY s.InvId, s.state, s.Due, s.bt, s.n, s.AgentId, s.InsuredId, s.CarrierId, s.WholesalerId;

INSERT INTO dbo.Accounting_Payments
    (EntityType_Id, Entity_Id, PaymentDate, Amount, RemainingAmount, PaymentType_Id, ReferenceNumber,
     PaymentStatus_Id, PaymentDirection_Id, Client_Id)
SELECT CASE bt WHEN 1 THEN 1 WHEN 2 THEN 2 ELSE CASE WHEN WholesalerId > 0 THEN 6 ELSE 3 END END,
       CASE bt WHEN 1 THEN AgentId WHEN 2 THEN InsuredId ELSE CASE WHEN WholesalerId > 0 THEN WholesalerId ELSE CarrierId END END,
       Due, TotalApplied, 0, CHOOSE(1 + CAST(((InvId * 1597334677 + 12345) % 4294967296) * 3 / 4294967296 AS int), 1, 4, 6),
       'PT' + CAST(InvId AS varchar(20)), 14, 1, @Client
FROM #PayCalc ORDER BY InvId;
DECLARE @cPay int = @@ROWCOUNT;

SELECT PayId = p.Payment_Id, pc.InvId INTO #Pm
FROM #PayCalc pc JOIN dbo.Accounting_Payments p ON p.Client_Id = @Client AND p.ReferenceNumber = 'PT' + CAST(pc.InvId AS varchar(20)) AND p.Status = 1;

INSERT INTO dbo.Accounting_PaymentApplications (Payment_Id, Invoice_Id, AppliedDate, TotalAppliedAmount)
SELECT pm.PayId, pc.InvId, pc.Due, pc.TotalApplied FROM #PayCalc pc JOIN #Pm pm ON pm.InvId = pc.InvId ORDER BY pc.InvId;

SELECT PaId = pa.PaymentApplication_Id, pa.Invoice_Id INTO #Pa
FROM dbo.Accounting_PaymentApplications pa JOIN #Pm pm ON pm.PayId = pa.Payment_Id;

INSERT INTO dbo.Accounting_PaymentApplicationLineItems (PaymentApplication_Id, InvoiceLineItem_Id, AppliedAmount, Is_SplitPayment)
SELECT pa.PaId, a.DetId, a.Applied, 0 FROM #AR a JOIN #Pa pa ON pa.Invoice_Id = a.InvId ORDER BY a.DetId;
DECLARE @cPali int = @@ROWCOUNT;

UPDATE d SET RemainingBalance = CASE a.state WHEN 'F' THEN 0e0 ELSE ROUND(d.Amount - a.Applied, 2) END,
             Paid = CASE a.state WHEN 'F' THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,
             PaidOn = CASE a.state WHEN 'F' THEN pc.Due END,
             UpdatedOn = GETDATE()
FROM dbo.Accounting_InvoiceDetails d JOIN #AR a ON a.DetId = d.Id JOIN #PayCalc pc ON pc.InvId = a.InvId;

UPDATE inv SET RemainingBalance = CASE pc.state WHEN 'F' THEN 0e0 ELSE ROUND(inv.Amount - pc.TotalApplied, 2) END,
               Paid = CASE pc.state WHEN 'F' THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,
               PaidOn = CASE pc.state WHEN 'F' THEN pc.Due END,
               UpdatedOn = GETDATE()
FROM dbo.Accounting_Invoices inv JOIN #PayCalc pc ON pc.InvId = inv.Id;

-- ---------------------------------------------------------------- reconciliation (any failure aborts + rolls back)
DECLARE @bad TABLE (chk varchar(80), cnt int);
INSERT INTO @bad
SELECT 'V1 invoice Amount <> sum of AR detail Amounts', COUNT(*)
FROM #I i
LEFT JOIN (SELECT d.Invoice_Id, s = SUM(d.Amount)
           FROM dbo.Accounting_InvoiceDetails d JOIN #Cfg c ON c.bld = d.BillingType_LineItems_Directions_Id AND c.dir = 1
           WHERE d.Invoice_Id IN (SELECT InvId FROM #I) GROUP BY d.Invoice_Id) x ON x.Invoice_Id = i.InvId
WHERE x.s IS NULL OR ABS(i.Amt - x.s) > 0.005;
INSERT INTO @bad SELECT 'V2 fully paid invoice with RemainingBalance <> 0', COUNT(*)
FROM dbo.Accounting_Invoices inv JOIN #PayCalc pc ON pc.InvId = inv.Id WHERE pc.state = 'F' AND (inv.RemainingBalance <> 0 OR inv.Paid <> 1);
INSERT INTO @bad SELECT 'V3 partial invoice remaining <> Amount - applied', COUNT(*)
FROM dbo.Accounting_Invoices inv JOIN #PayCalc pc ON pc.InvId = inv.Id WHERE pc.state = 'P' AND ABS(inv.RemainingBalance - (inv.Amount - pc.TotalApplied)) > 0.005;
INSERT INTO @bad SELECT 'V4 application total <> sum of its line items', COUNT(*)
FROM dbo.Accounting_PaymentApplications pa
JOIN #Pa x ON x.PaId = pa.PaymentApplication_Id
JOIN (SELECT PaymentApplication_Id, s = SUM(AppliedAmount) FROM dbo.Accounting_PaymentApplicationLineItems GROUP BY PaymentApplication_Id) l
  ON l.PaymentApplication_Id = pa.PaymentApplication_Id
WHERE ABS(pa.TotalAppliedAmount - l.s) > 0.005;
INSERT INTO @bad SELECT 'V5 payment amount <> application total', COUNT(*)
FROM dbo.Accounting_Payments p JOIN #Pm pm ON pm.PayId = p.Payment_Id
JOIN dbo.Accounting_PaymentApplications pa ON pa.Payment_Id = p.Payment_Id WHERE ABS(p.Amount - pa.TotalAppliedAmount) > 0.005;
INSERT INTO @bad SELECT 'V6 policy/invoice counts not as staged', CASE WHEN @cPol = (SELECT COUNT(*) FROM #P) AND @cInv = (SELECT COUNT(*) FROM #I)
                                                                   AND (SELECT COUNT(*) FROM #I WHERE InvId IS NULL) = 0 THEN 0 ELSE 1 END;
INSERT INTO @bad SELECT 'V7 invoice without any detail row', COUNT(*) FROM #I i
WHERE NOT EXISTS (SELECT 1 FROM dbo.Accounting_InvoiceDetails d WHERE d.Invoice_Id = i.InvId);
IF EXISTS (SELECT 1 FROM @bad WHERE cnt <> 0)
BEGIN
    SELECT * FROM @bad;
    THROW 50099, 'Reconciliation failed. Batch rolled back.', 1;
END

IF @Commit = 0
    SELECT agents_used = (SELECT COUNT(DISTINCT AgentId) FROM #P), of_agents = @nAg,
           carriers_used = (SELECT COUNT(DISTINCT CarrierId) FROM #P), of_carriers = @nCa,
           uw_used = (SELECT COUNT(DISTINCT UwId) FROM #P), wholesaler_pct = (SELECT CAST(100.0 * SUM(CASE WHEN WholesalerId > 0 THEN 1 ELSE 0 END) / COUNT(*) AS decimal(5,1)) FROM #P),
           inv_paid = (SELECT COUNT(*) FROM #PayCalc WHERE state = 'F'), inv_partial = (SELECT COUNT(*) FROM #PayCalc WHERE state = 'P'),
           inv_unpaid = (SELECT COUNT(*) FROM #S WHERE state = 'U'),
           max_pol_per_agent = (SELECT MAX(c) FROM (SELECT COUNT(*) c FROM #P GROUP BY AgentId) z),
           pay_types = (SELECT STRING_AGG(CAST(t AS varchar(3)) + ':' + CAST(c AS varchar(10)), ',') FROM (SELECT PaymentType_Id t, COUNT(*) c FROM dbo.Accounting_Payments WHERE Client_Id = @Client AND ReferenceNumber IN (SELECT 'PT' + CAST(InvId AS varchar(20)) FROM #PayCalc) GROUP BY PaymentType_Id) z);

DECLARE @ms int = DATEDIFF(MILLISECOND, @t0, SYSDATETIME());
DECLARE @mb2 int = (SELECT SUM(size) / 128 FROM sys.database_files WHERE type = 0);
IF @Commit = 1
    INSERT INTO dbo.PerfTest_LoadManifest (Client_Id, StartPolicySeq, EndPolicySeq, Policies, Invoices, InvoiceDetails, Payments,
                                           PaymentAppLineItems, Outcome, StartedOn, FinishedOn, DurationMs, DataFileMB)
    VALUES (@Client, @Start, @End, @cPol, @cInv, @cDet, @cPay, @cPali, 'COMMITTED', @t0, SYSDATETIME(), @ms, @mb2);

SELECT Outcome = CASE WHEN @Commit = 1 THEN 'COMMITTED' ELSE 'DRYRUN (rolled back)' END,
       Policies = @cPol, Invoices = @cInv, InvoiceDetails = @cDet, Payments = @cPay, PaymentAppLineItems = @cPali,
       DurationMs = @ms, DataFileMB = @mb2;
SELECT chk, cnt FROM @bad;

IF @Commit = 1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
