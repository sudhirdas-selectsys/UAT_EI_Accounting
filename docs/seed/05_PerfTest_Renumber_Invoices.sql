/*
 Invoice-number correction for the performance-test dataset (user request:
 "unique, correctly formatted, free of duplicates; length 11; no broken relationships").

 Problem found: the loaded invoices were numbered 'INV' + RIGHT('000000' + seq, 6): the 6-digit format wraps
 after 999,999, so 2,004,800 invoices had only 1,000,000 distinct numbers (plus an overlap from the sequence
 restart). Also note USP_Accounting_GenerateInvoiceNo pads with only SIX zeros, so passing length 11 does not
 zero-pad sequence values below 10,000 (INV0000001 = 10 chars). This script therefore pads fully:
      'INV' + RIGHT(REPLICATE('0', 11) + seq, 11)   =>  always 14 characters, e.g. INV00001685601

 What it does:
   * takes a RANGE of new numbers from Seq_Accounting_GenerateInvoiceNo (sys.sp_sequence_get_range), so the
     sequence is never restarted or reused: uniqueness is guaranteed by the sequence;
   * assigns them in Invoice Id order to every invoice that is not already in the new format;
   * updates ONLY Accounting_Invoices.InvoiceNo (UpdatedOn untouched). Foreign keys point at Invoices.Id, not at
     InvoiceNo, so no relationship changes. The three tables that copy InvoiceNo by value are checked empty first.
   * proves afterwards: uniqueness, format, and that counts and money totals of invoices, details, payments,
     applications and application line items are unchanged.
 Re-runnable (only renumbers rows not already in the new format). Run with sqlcmd -I -b.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'uat-accounting' THROW 50001, 'Wrong database. Aborting.', 1;
DECLARE @Client bigint = (SELECT Client_Id FROM dbo.Accounting_Clients WHERE CompanyName = 'PERFTEST_CLIENT');
IF @Client IS NULL THROW 50002, 'Test client not found. Aborting.', 1;
IF EXISTS (SELECT 1 FROM dbo.Accounting_Invoices WHERE Client_Id <> @Client OR Client_Id IS NULL)
    THROW 50003, 'Invoices of other clients exist. Aborting (test data only).', 1;
IF EXISTS (SELECT 1 FROM dbo.Accounting_PaymentLevelAppliedInvoices_Corrections)
   OR EXISTS (SELECT 1 FROM dbo.Accounting_PaymentInvoiceCorrectionLogDetails)
   OR EXISTS (SELECT 1 FROM dbo.Accounting_SchedulerInvoiceNoticeLogs)
    THROW 50004, 'A table holding InvoiceNo by value is not empty. Aborting.', 1;

DECLARE @pat varchar(100) = 'INV' + REPLICATE('[0-9]', 11);   -- 58 characters (varchar(30) truncated it in the first attempt)

-- snapshot of everything that must NOT change
DECLARE @s TABLE (k varchar(30), v decimal(38,4));
INSERT @s SELECT 'invoices',     COUNT(*)                    FROM dbo.Accounting_Invoices;
INSERT @s SELECT 'inv_amount',   SUM(CAST(Amount AS decimal(38,4)))           FROM dbo.Accounting_Invoices;
INSERT @s SELECT 'inv_remaining',SUM(CAST(RemainingBalance AS decimal(38,4))) FROM dbo.Accounting_Invoices;
INSERT @s SELECT 'inv_paid',     SUM(CAST(Paid AS int))      FROM dbo.Accounting_Invoices;
INSERT @s SELECT 'inv_policy_ids', SUM(CAST(PolicyDetails_Id AS decimal(38,0))) FROM dbo.Accounting_Invoices;
INSERT @s SELECT 'details',      COUNT(*)                    FROM dbo.Accounting_InvoiceDetails;
INSERT @s SELECT 'det_amount',   SUM(CAST(Amount AS decimal(38,4)))           FROM dbo.Accounting_InvoiceDetails;
INSERT @s SELECT 'payments',     COUNT(*)                    FROM dbo.Accounting_Payments;
INSERT @s SELECT 'pay_amount',   SUM(CAST(Amount AS decimal(38,4)))           FROM dbo.Accounting_Payments;
INSERT @s SELECT 'apps',         COUNT(*)                    FROM dbo.Accounting_PaymentApplications;
INSERT @s SELECT 'app_invoice_ids', SUM(CAST(Invoice_Id AS decimal(38,0)))   FROM dbo.Accounting_PaymentApplications;
INSERT @s SELECT 'pali',         COUNT(*)                    FROM dbo.Accounting_PaymentApplicationLineItems;
INSERT @s SELECT 'pali_amount',  SUM(CAST(AppliedAmount AS decimal(38,4)))    FROM dbo.Accounting_PaymentApplicationLineItems;

DECLARE @todo int = (SELECT COUNT(*) FROM dbo.Accounting_Invoices WHERE InvoiceNo IS NULL OR LEN(InvoiceNo) <> 14 OR InvoiceNo NOT LIKE @pat);
PRINT 'Invoices to renumber: ' + CAST(@todo AS varchar(20));
IF @todo > 0
BEGIN
    DECLARE @first sql_variant;
    EXEC sys.sp_sequence_get_range @sequence_name = N'dbo.Seq_Accounting_GenerateInvoiceNo', @range_size = @todo, @range_first_value = @first OUTPUT;
    DECLARE @f bigint = CAST(@first AS bigint);

    SELECT Id, NewNo = CAST('INV' + RIGHT(REPLICATE('0', 11) + CAST(@f + ROW_NUMBER() OVER (ORDER BY Id) - 1 AS varchar(20)), 11) AS varchar(100))
    INTO #Map FROM dbo.Accounting_Invoices WHERE InvoiceNo IS NULL OR LEN(InvoiceNo) <> 14 OR InvoiceNo NOT LIKE @pat;
    CREATE UNIQUE CLUSTERED INDEX cx_map ON #Map (Id);

    -- validate BEFORE touching data
    IF (SELECT COUNT(DISTINCT NewNo) FROM #Map) <> @todo THROW 50010, 'Generated numbers are not unique. Aborting, nothing changed.', 1;
    IF EXISTS (SELECT 1 FROM #Map WHERE LEN(NewNo) <> 14 OR NewNo NOT LIKE @pat) THROW 50011, 'Generated number has wrong format. Aborting, nothing changed.', 1;
    -- Collision check against numbers that are NOT being renumbered. The new numbers are one contiguous block
    -- [@minNo, @maxNo] of zero-padded 14-char values, so any collision must fall inside that block: one scan,
    -- no join (a join on the un-indexed InvoiceNo column ran as a nested loop and never finished in the first attempt).
    DECLARE @minNo varchar(100) = (SELECT MIN(NewNo) FROM #Map), @maxNo varchar(100) = (SELECT MAX(NewNo) FROM #Map);
    IF EXISTS (SELECT 1 FROM dbo.Accounting_Invoices i
               WHERE LEN(i.InvoiceNo) = 14 AND i.InvoiceNo BETWEEN @minNo AND @maxNo
                 AND NOT EXISTS (SELECT 1 FROM #Map m WHERE m.Id = i.Id))
        THROW 50012, 'A new number already exists. Aborting, nothing changed.', 1;

    DECLARE @lo bigint = (SELECT MIN(Id) FROM #Map), @hi bigint = (SELECT MAX(Id) FROM #Map), @step bigint = 100000, @done int = 0;
    WHILE @lo <= @hi
    BEGIN
        UPDATE inv SET InvoiceNo = m.NewNo
        FROM dbo.Accounting_Invoices inv JOIN #Map m ON m.Id = inv.Id
        WHERE inv.Id >= @lo AND inv.Id < @lo + @step;
        SET @done += @@ROWCOUNT;
        SET @lo += @step;
    END
    PRINT 'Renumbered: ' + CAST(@done AS varchar(20)) + '  (first number: INV' + RIGHT(REPLICATE('0', 11) + CAST(@f AS varchar(20)), 11) + ')';
END

-- post-checks
DECLARE @r TABLE (chk varchar(90), bad int);
INSERT @r SELECT 'P1 duplicate invoice numbers', COUNT(*) FROM (SELECT InvoiceNo FROM dbo.Accounting_Invoices GROUP BY InvoiceNo HAVING COUNT(*) > 1) d;
INSERT @r SELECT 'P2 invoice number not INV + 11 digits (14 chars)', COUNT(*) FROM dbo.Accounting_Invoices WHERE InvoiceNo IS NULL OR LEN(InvoiceNo) <> 14 OR InvoiceNo NOT LIKE @pat;
INSERT @r SELECT 'P3 invoices total changed',   CASE WHEN (SELECT COUNT(*) FROM dbo.Accounting_Invoices) = (SELECT v FROM @s WHERE k='invoices') THEN 0 ELSE 1 END;
INSERT @r SELECT 'P4 invoice money/flags/policy links changed',
        CASE WHEN (SELECT SUM(CAST(Amount AS decimal(38,4))) FROM dbo.Accounting_Invoices) = (SELECT v FROM @s WHERE k='inv_amount')
              AND (SELECT SUM(CAST(RemainingBalance AS decimal(38,4))) FROM dbo.Accounting_Invoices) = (SELECT v FROM @s WHERE k='inv_remaining')
              AND (SELECT SUM(CAST(Paid AS int)) FROM dbo.Accounting_Invoices) = (SELECT v FROM @s WHERE k='inv_paid')
              AND (SELECT SUM(CAST(PolicyDetails_Id AS decimal(38,0))) FROM dbo.Accounting_Invoices) = (SELECT v FROM @s WHERE k='inv_policy_ids') THEN 0 ELSE 1 END;
INSERT @r SELECT 'P5 details count/amount changed',
        CASE WHEN (SELECT COUNT(*) FROM dbo.Accounting_InvoiceDetails) = (SELECT v FROM @s WHERE k='details')
              AND (SELECT SUM(CAST(Amount AS decimal(38,4))) FROM dbo.Accounting_InvoiceDetails) = (SELECT v FROM @s WHERE k='det_amount') THEN 0 ELSE 1 END;
INSERT @r SELECT 'P6 payments/applications/line items changed',
        CASE WHEN (SELECT COUNT(*) FROM dbo.Accounting_Payments) = (SELECT v FROM @s WHERE k='payments')
              AND (SELECT SUM(CAST(Amount AS decimal(38,4))) FROM dbo.Accounting_Payments) = (SELECT v FROM @s WHERE k='pay_amount')
              AND (SELECT COUNT(*) FROM dbo.Accounting_PaymentApplications) = (SELECT v FROM @s WHERE k='apps')
              AND (SELECT SUM(CAST(Invoice_Id AS decimal(38,0))) FROM dbo.Accounting_PaymentApplications) = (SELECT v FROM @s WHERE k='app_invoice_ids')
              AND (SELECT COUNT(*) FROM dbo.Accounting_PaymentApplicationLineItems) = (SELECT v FROM @s WHERE k='pali')
              AND (SELECT SUM(CAST(AppliedAmount AS decimal(38,4))) FROM dbo.Accounting_PaymentApplicationLineItems) = (SELECT v FROM @s WHERE k='pali_amount') THEN 0 ELSE 1 END;
INSERT @r SELECT 'P7 orphan rows (details/applications without invoice)',
        (SELECT COUNT(*) FROM dbo.Accounting_InvoiceDetails d WHERE NOT EXISTS (SELECT 1 FROM dbo.Accounting_Invoices i WHERE i.Id = d.Invoice_Id))
      + (SELECT COUNT(*) FROM dbo.Accounting_PaymentApplications a WHERE a.Invoice_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Accounting_Invoices i WHERE i.Id = a.Invoice_Id));
SELECT chk, bad FROM @r;
SELECT 'min/max InvoiceNo' AS what, MIN(InvoiceNo) AS min_no, MAX(InvoiceNo) AS max_no, COUNT(*) AS invoices, COUNT(DISTINCT InvoiceNo) AS distinct_no FROM dbo.Accounting_Invoices;
IF EXISTS (SELECT 1 FROM @r WHERE bad <> 0) THROW 50098, 'Post-checks failed. See table above.', 1;
