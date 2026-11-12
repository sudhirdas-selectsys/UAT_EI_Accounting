/*==============================================================================
 15_PerfTest_AgentPayouts.sql   (LOCAL TEST DATA ONLY)
 Purpose: the 1099 report sums payments with EntityType_Id = 1 (Agency/Broker) and
 PaymentDirection_Id = 2 (Outgoing). The generated test data had none, so the report
 returned empty amounts. This adds outgoing agent-commission payouts on the test copy.

 Guarded to database [uat-accounting-perf] only. Additive: inserts only, no existing row
 is updated or deleted. All rows are tagged by Payments.ReferenceNumber LIKE 'BillPT%'.
 Usage (sqlcmd): -v Commit=0 (dry run, rolls back) or -v Commit=1.

 Model (assumptions, to be stated with any result):
  P1 One outgoing payment per agent per calendar month, dated the last day of the month
     in which the customer paid the invoice (Invoices.PaidOn); status Completed (14);
     type alternates Check (6) / Bill (7) by agent (the application uses Check/Bill for
     outgoing payments, MakePaymentService.PaymentDetails).
  P2 Each payout applies, per fully paid invoice, the full Amount of the invoice's
     Accounts Payable 'Agency/Broker Commission' line (EntityType 1) in one payment
     application and one line item.
  P3 NOT done (deviations from what the application writes): no Accounting_Bill /
     Bill line items / Payments_BillsMapping rows, no journal entries, and the paid flags
     of the payable InvoiceDetails lines are NOT updated (kept unchanged so earlier
     benchmarks stay comparable). Payable lines therefore still look unpaid in the AP
     report; this is a known limit of this seed.
==============================================================================*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @Commit bit = $(Commit);
DECLARE @Client bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client IS NULL THROW 50002, 'Test client not found. Aborting.', 1;
IF EXISTS (SELECT 1 FROM dbo.Accounting_Payments WHERE Client_Id = @Client AND ReferenceNumber LIKE 'BillPT%')
    THROW 50003, 'Agent payouts already seeded. Aborting.', 1;
DECLARE @t0 datetime2 = SYSDATETIME();

BEGIN TRAN;

SELECT ApLineId = ID.Id, InvId = ID.Invoice_Id, AgentId = ID.Entity_Id, ID.Amount,
       PayMonthEnd = EOMONTH(I.PaidOn)
INTO #Ap
FROM dbo.Accounting_InvoiceDetails ID
JOIN dbo.Accounting_Invoices I ON I.Id = ID.Invoice_Id AND I.Paid = 1 AND I.Client_Id = @Client AND I.Status = 1
JOIN dbo.Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id
JOIN dbo.Accounting_LU_LineItems_Directions LLD ON LLD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LLD.Direction_Id = 2
JOIN dbo.Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LLD.LineItems_Clients_Id
JOIN dbo.Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Name = 'Agency/Broker Commission'
WHERE ID.EntityType_Id = 1 AND ID.Client_Id = @Client AND ID.Status = 1
  AND EXISTS (SELECT 1 FROM dbo.Accounting_AgentDetails A WHERE A.AgentDetail_Id = ID.Entity_Id AND A.Client_Id = @Client);

INSERT INTO dbo.Accounting_Payments
    (EntityType_Id, Entity_Id, PaymentDate, Amount, RemainingAmount, PaymentType_Id, ReferenceNumber,
     PaymentStatus_Id, PaymentDirection_Id, Client_Id)
SELECT 1, AgentId, PayMonthEnd, SUM(Amount), 0,
       CASE WHEN AgentId % 2 = 0 THEN 6 ELSE 7 END,
       'BillPT' + CAST(AgentId AS varchar(20)) + '-' + CONVERT(varchar(8), PayMonthEnd, 112),
       14, 2, @Client
FROM #Ap GROUP BY AgentId, PayMonthEnd ORDER BY AgentId, PayMonthEnd;
DECLARE @cPay int = @@ROWCOUNT;

INSERT INTO dbo.Accounting_PaymentApplications (Payment_Id, Invoice_Id, AppliedDate, TotalAppliedAmount)
SELECT p.Payment_Id, a.InvId, a.PayMonthEnd, a.Amount
FROM #Ap a JOIN dbo.Accounting_Payments p
  ON p.Client_Id = @Client AND p.ReferenceNumber = 'BillPT' + CAST(a.AgentId AS varchar(20)) + '-' + CONVERT(varchar(8), a.PayMonthEnd, 112)
ORDER BY p.Payment_Id, a.InvId;
DECLARE @cPa int = @@ROWCOUNT;

INSERT INTO dbo.Accounting_PaymentApplicationLineItems (PaymentApplication_Id, InvoiceLineItem_Id, AppliedAmount, Is_SplitPayment)
SELECT pa.PaymentApplication_Id, a.ApLineId, a.Amount, 0
FROM #Ap a
JOIN dbo.Accounting_Payments p ON p.Client_Id = @Client
 AND p.ReferenceNumber = 'BillPT' + CAST(a.AgentId AS varchar(20)) + '-' + CONVERT(varchar(8), a.PayMonthEnd, 112)
JOIN dbo.Accounting_PaymentApplications pa ON pa.Payment_Id = p.Payment_Id AND pa.Invoice_Id = a.InvId
ORDER BY pa.PaymentApplication_Id;
DECLARE @cPali int = @@ROWCOUNT;

-- reconciliation: any failure rolls back
DECLARE @bad int = 0;
SELECT @bad += COUNT(*) FROM dbo.Accounting_Payments p
  JOIN (SELECT Payment_Id, s = SUM(TotalAppliedAmount) FROM dbo.Accounting_PaymentApplications GROUP BY Payment_Id) x ON x.Payment_Id = p.Payment_Id
  WHERE p.ReferenceNumber LIKE 'BillPT%' AND ABS(p.Amount - x.s) > 0.005;
SELECT @bad += COUNT(*) FROM dbo.Accounting_PaymentApplications pa
  JOIN dbo.Accounting_Payments p ON p.Payment_Id = pa.Payment_Id AND p.ReferenceNumber LIKE 'BillPT%'
  JOIN (SELECT PaymentApplication_Id, s = SUM(AppliedAmount) FROM dbo.Accounting_PaymentApplicationLineItems GROUP BY PaymentApplication_Id) l ON l.PaymentApplication_Id = pa.PaymentApplication_Id
  WHERE ABS(pa.TotalAppliedAmount - l.s) > 0.005;
IF @cPa <> (SELECT COUNT(*) FROM #Ap) OR @cPali <> (SELECT COUNT(*) FROM #Ap) SET @bad += 1;

SELECT payments = @cPay, applications = @cPa, line_items = @cPali, bad_checks = @bad,
       total_paid = (SELECT SUM(Amount) FROM #Ap), seconds = DATEDIFF(SECOND, @t0, SYSDATETIME()), commit_requested = @Commit;
IF @bad <> 0 OR @Commit = 0 BEGIN ROLLBACK; PRINT 'ROLLED BACK'; END ELSE BEGIN COMMIT; PRINT 'COMMITTED'; END
