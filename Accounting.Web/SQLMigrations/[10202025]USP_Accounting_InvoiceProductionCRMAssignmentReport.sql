
ALTER PROC USP_Accounting_InvoiceProductionCRMAssignmentReport        
(        
    @Client_Id BIGINT = NULL,        
    @InvoiceDate VARCHAR(100) = NULL,        
    @InvoiceCreatedFromDate VARCHAR(200) = NULL,        
    @InvoiceCreatedToDate VARCHAR(200) = NULL,        
    @InvoiceEffectiveFromDate VARCHAR(200) = NULL,        
    @InvoiceEffectiveToDate VARCHAR(200) = NULL,        
    @Range VARCHAR(200) = NULL,        
    @PageIndex INT = NULL,        
    @PageSize INT = NULL,        
    @AgentDetail_Id BIGINT = NULL,        
    @WholesalerDetail_Id BIGINT = NULL,        
    @CarrierDetail_Id BIGINT = NULL,        
    @LOB_Id INT = NULL,        
    @TotalRecords INT OUT        
)        
AS        
BEGIN        
    SET NOCOUNT ON;        
        
    DECLARE @CompareDateFrom DATETIME;        
    DECLARE @CompareDateTo DATETIME;        
        
    IF @InvoiceDate = 'InvoiceCreatedDate'        
    BEGIN        
        SET @CompareDateFrom = TRY_CAST(@InvoiceCreatedFromDate AS DATETIME);        
        SET @CompareDateTo = DATEADD(MILLISECOND, -2, DATEADD(DAY, 1, TRY_CAST(@InvoiceCreatedToDate AS DATETIME)));        
    END        
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'        
    BEGIN        
        SET @CompareDateFrom = TRY_CAST(@InvoiceEffectiveFromDate AS DATETIME);        
        SET @CompareDateTo = DATEADD(MILLISECOND, -2, DATEADD(DAY, 1, TRY_CAST(@InvoiceEffectiveToDate AS DATETIME)));        
    END        
        
    ;WITH BaseData AS (        
        SELECT        
            AC.CompanyName AS Company,        
            AC.CompanyName AS Branch,        
            ID.Name AS [Insured Name],        
            AD.Name AS [Agent Broker Name] ,        
            RiskState AS [Primary State],        
            CD.Name AS [Issuing Company],        
            CASE WHEN PD.WholesalerDetail_Id IS NULL THEN CD.Name ELSE WD.Name END AS [Billing Company],        
            PD.PolicyNumber AS [Policy Number],        
            CONVERT(VARCHAR(10), PD.EffectiveDate, 101) AS [Effective Date],        
            CONVERT(VARCHAR(10), PD.ExpirationDate, 101) AS [Expiration Date],        
            TransactionType AS [Transaction Type],        
            LOB.LOBName AS [LOB Name],     
   PD.Submission_Id As 'SID #',
            I.InvoiceNo As 'Invoice No #',        
            CONVERT(VARCHAR(10), I.CreatedOn, 101) AS [Invoice Created],        
            CONVERT(VARCHAR(10), PD.EffectiveDate, 101) AS [Invoice Booked],        
            CASE WHEN ISNULL(LTRIM(RTRIM(PD.PaymentPlan)), '') = '' THEN 'Term Policy' ELSE PD.PaymentPlan END AS [Payment Plan],        
            PD.BillType AS [Bill Type],        
            Details.TotalPremium AS [Total Premium],        
            Details.TotalFees AS [Total Fees],        
            Details.TotalHouseFee AS [Total House Fee],        
            Details.TotalCarrierFees AS [Total Carrier Fees],        
            Details.TotalTaxes AS [Total Taxes],        
            Details.TotalPremium + Details.TotalFees + Details.TotalTaxes AS [Total Policy Billed Amount],        
            Details.TotalCommission AS [Total Commission],        
            Details.TotalHouseCommission AS [Total House Commission],        
            Details.TotalAgentBrokerCommission AS [Total Agent Broker Commission],        
            Details.TotalARInvoiced AS [Total AR Invoiced],        
   I.DueDate AS [Due Date],        
            Details.ARAmountPaid AS [AR Amount Paid],        
            Details.ARBalanceDue AS [AR Balance Due],        
   --AR Aging Buckets        
   Details.ARCurrent AS [AR Current],        
   Details.AR1_30 ,        
   Details.AR31_60,        
   Details.AR61_90,        
   Details.AROver90 AS [AR_Over_90],        
            Details.TotalAPInvoiced AS [Total AP Invoiced],        
            Details.APAmountPaid AS [AP Amount Paid],        
            Details.APBalanceDue AS [AP Balance Due],        
   --A9 Aging Buckets        
   Details.APCurrent AS [AP Current],        
   Details.AP1_30 ,        
   Details.AP31_60,        
   Details.AP61_90,        
   Details.APOver90 AS [AP_Over_90]         
           
        
        FROM Accounting_PolicyDetails PD        
        INNER JOIN Accounting_Clients AC ON AC.Client_Id = PD.Client_Id        
        INNER JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1 AND CD.Client_Id = @Client_Id        
        INNER JOIN Accounting_InsuredDetails ID ON ID.InsuredDetail_Id = PD.InsuredDetail_Id AND ID.Status = 1 AND ID.Client_Id = @Client_Id        
        INNER JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id = PD.AgentDetail_Id AND AD.Status = 1 AND AD.Client_Id = @Client_Id        
        LEFT JOIN Accounting_WholesalerDetails WD ON WD.WholesalerDetail_Id = PD.WholesalerDetail_Id AND WD.Status = 1 AND WD.Client_Id = @Client_Id        
        INNER JOIN Accounting_Invoices I ON I.PolicyDetails_Id = PD.Id AND I.Status = 1        
        OUTER APPLY (        
            SELECT TOP 1 AL.Name AS LOBName        
            FROM Accounting_InvoiceDetails AID        
            INNER JOIN Accounting_LU_LOBs AL ON AL.LOB_Id = AID.LOB_Id        
            WHERE AID.Invoice_Id = I.Id        
              AND AID.Status = 1        
              AND (@LOB_Id IS NULL OR AID.LOB_Id = @LOB_Id)        
            ORDER BY AID.Id        
        ) AS LOB        
       OUTER APPLY (        
    SELECT        
        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,        
        CAST(SUM(CASE WHEN LIT_Name = 'FEE' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalFees,        
        CAST(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseFee,        
        CAST(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCarrierFees,        
        CAST(SUM(CASE WHEN LIT_Name = 'TAX' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalTaxes,        
        CAST(SUM(CASE WHEN LIT_Name = 'Commission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,        
        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,        
        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalARInvoiced,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END)         
            - SUM(CASE WHEN LUD_Name = 'Accounts Receivable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARAmountPaid,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARBalanceDue,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAPInvoiced,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END)         
            - SUM(CASE WHEN LUD_Name = 'Accounts Payable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APAmountPaid,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APBalanceDue,        
        
        --AR Aging Buckets        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARCurrent,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR1_30,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR31_60,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR61_90,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AROver90,        
          
  -- AP Aging Buckets        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APCurrent,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP1_30,      
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP31_60,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP61_90,        
        CAST(SUM(CASE WHEN LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APOver90        
FROM (        
        SELECT         
            ID.Amount,        
            ID.RemainingBalance,        
            I.DueDate AS DueDateLocal,        
            LIT.Name AS LIT_Name,        
            LI.Code AS LI_Code,        
            LI.Name AS LI_Name,        
            LUD.Name AS LUD_Name        
        FROM Accounting_InvoiceDetails ID        
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD         
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1        
        JOIN Accounting_LU_LineItems_Directions LD         
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
  JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1    
        JOIN Accounting_LU_LineItems LI         
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1        
        JOIN Accounting_LU_LineItemTypes LIT         
            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1        
        JOIN Accounting_LU_Directions LUD         
            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1        
        JOIN Accounting_LU_BillingTypes BT         
            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1        
        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1        
    ) AS DetailView        
) AS Details        
        
          
        WHERE        
            (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
             OR (@InvoiceDate = 'InvoiceEffectiveDate' AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo)        
             OR (@InvoiceDate = 'InvoiceCreatedDate' AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo))        
            AND PD.Status = 1        
            AND PD.Client_Id = @Client_Id        
            AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)        
            AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)        
            AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)        
            AND (@LOB_Id IS NULL OR EXISTS (        
                SELECT 1 FROM Accounting_InvoiceDetails AID WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id)        
            )        
    )        
        
    -- Conditional pagination logic        
    SELECT * FROM BaseData        
    ORDER BY [Effective Date] DESC        
    OFFSET         
        CASE         
            WHEN @PageSize IS NOT NULL AND @PageSize > 0 AND @PageIndex IS NOT NULL THEN @PageSize * @PageIndex         
            ELSE 0         
        END ROWS        
    FETCH NEXT         
        CASE         
            WHEN @PageSize IS NOT NULL AND @PageSize > 0 THEN @PageSize         
            ELSE 1000000         
        END ROWS ONLY;        
        
    -- Total count        
    SET @TotalRecords = (        
        SELECT COUNT(*)        
        FROM Accounting_PolicyDetails PD        
        INNER JOIN Accounting_Invoices I ON I.PolicyDetails_Id = PD.Id AND I.Status = 1        
        WHERE        
            PD.Status = 1        
            AND PD.Client_Id = @Client_Id        
            AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)        
            AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)        
            AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)        
     AND (@LOB_Id IS NULL OR EXISTS (        
                SELECT 1 FROM Accounting_InvoiceDetails AID WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id)        
            )        
            AND (        
                @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
                OR (@InvoiceDate = 'InvoiceEffectiveDate' AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo)        
                OR (@InvoiceDate = 'InvoiceCreatedDate' AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo)        
            )        
    );        
END 