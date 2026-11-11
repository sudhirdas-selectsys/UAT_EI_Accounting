insert into Accounting_Reports(Name, Status) values ('Tax Filing Report' , 1)
insert into Accounting_LU_SQLQueries(SQLQuery, Is_StoredProcedure, Description) values('USP_Accounting_TaxtFilingReport', 1, 'TaxFilingReport' )
insert into Accounting_ReportsQueries (Report_Id, SQLQuery_Id, Sequence, Status) values (8, 13, 8,1)

insert into Accounting_ReportsSettings(Client_Id, Report_Id, Sequence, Status) values (1, 8, 8, 1)
INSERT INTO Accounting_ReportsSettings (Client_Id, Report_Id, Sequence, Status)
VALUES
(10001, 8, 8, 1),
(10002, 8, 8, 1),
(10003, 8, 8, 1),
(10004, 8, 8, 1),
(10005, 8, 8, 1),
(10006, 8, 8, 1),
(10007, 8, 8, 1),
(10008, 8, 8, 1),
(10009, 8, 8, 1),
(10010, 8, 8, 1),
(10011, 8, 8, 1),
(10013, 8, 8, 1),
(10014, 8, 8, 1),
(10015, 8, 8, 1),
(10016, 8, 8, 1),
(10017, 8, 8, 1),
(10018, 8, 8, 1),
(10019, 8, 8, 1);


insert into Accounting_ReportOutputTables(SQLQuery_Id, Name, TableIndex, Hide, Enable_Paging, Status) values (13, 'Tax Filing Report', 0, 0, 0, 1)
insert into Accounting_ReportOutputTableParameters(ReportOutputTable_Id, ParameterName, DisplayName, Enable_Sorting, Hide, Enable_Total, Status) values(11, 'TotalRecords', 'TotalRecords', 0, 0, 0, 1)




insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id, PredefinedParameter_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13, 'Client_Id', 'Client ID', 0, 2, 1,1, 0, 0, 1, 3, 1, 0, 0)

insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id, 
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'PageIndex', 'Page Index', 0, 2, 1, 0, 0, 1, 4, 1, 0, 0)


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id, 
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'PageSize', 'Page Size', 0, 2, 1, 0, 0, 1, 5, 1, 0, 0)

insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id, 
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'TotalRecords', 'Total Records',1, 2, 1, 0, 0, 1, 6, 1, 0, 0)

insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter,  HTMLControlType_Id, DataDictionaryType_Id,
RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceDate', 'Invoice Date', 0, 3, 1, 'Invoice Date', 0, 1, 0, 7, 1, 0, 0 )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
Dependency_SQLParameter_IdForRequired, RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceCreatedFromDate', 'Created From Date', 0, 6, 6, 17, 'InvoiceCreatedDate', 1, 1, 0, 12, 1, 0, 0   )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
Dependency_SQLParameter_IdForRequired, RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceCreatedToDate', 'Created To Date', 0, 6, 6, 17, 'InvoiceCreatedDate',  1, 1, 0, 14, 1, 0, 0 )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
FormatType_Id, RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceEffectiveFromDate', 'Effective From Date', 0, 6, 6, 18, 'InvoiceEffectiveDate', 1, 1, 0, 12, 1, 0, 0 )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
Dependency_SQLParameter_IdForRequired, RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceEffectiveToDate', 'Effective To Date', 0, 6, 6, 18,  'InvoiceEffectiveDate', 1, 1, 0, 14, 1, 0, 0 )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
DataDictionaryType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'Range', 'Range', 0, 1, 5, 3, 0, 1, 0, 10, 1, 0, 0  )



insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
DataDictionaryType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'AgentDetail_Id', 'Agent/Broker', 0, 1, 5, 5, 0, 1, 0, 9, 1, 0, 0 )



insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
DataDictionaryType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'WholesalerDetail_Id', 'Wholesaler/MGA', 0, 1, 5, 6, 0, 1, 0, 15, 1, 0, 0 )


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
DataDictionaryType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'CarrierDetail_Id', 'Carrier', 0, 1, 5, 7, 0, 1, 0, 13, 1, 0, 0)


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
DataDictionaryType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'LOB_Id', 'LOBs', 0, 1, 5,8, 0, 1, 0, 11, 1, 0, 0)


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id, HTMLControlType_Id,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceNumber', 'Invoice Number', 0, 1, 1, 0, 1, 0, 16, 1, 0, 0)


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter,  HTMLControlType_Id,
DataDictionaryType_Id, RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'InvoiceEffectiveDate', 'Invoice Effective Date', 0, 3, 2, 'InvoiceEffectiveDate',  0, 1, 0, 8, 0, 0,0   )

insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id,  HTMLControlType_Id,
RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'DateFrom', 'From Date', 0, 6, 6, 'Date', 1, 1, 0, 1, 0, 0, 0)


insert into Accounting_LU_SQLInputParameters(SQLQuery_Id, ParameterName, DisplayName, Is_OutputParameter, DataType_Id,  HTMLControlType_Id,
 RangeGroupName,
ApplyRangeValidation, ShowInputField, IsRequired, Sequence, Status, AllowFilterWith_Null, AllowFilterWith_Empty)
values (13,'DateTo', 'To Date', 0, 6, 6, 'Date', 1, 1, 0, 2, 0, 0, 0 )



-- newly created TaxFilingReport similar to Invoice Production Report except at the end, have three columns for every tax: <Tax Name>-Invoiced, <Tax Name>-Collected, <Tax Name>-Paid
CREATE PROCEDURE [dbo].[USP_Accounting_TaxFilingReport]    
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
    @InvoiceNumber VARCHAR(200) = NULL,    
    @CarrierDetail_Id BIGINT = NULL,    
    @LOB_Id INT = NULL,    
    @TotalRecords INT OUT    
)    
AS    
BEGIN    
    SET NOCOUNT ON;    
    
        
    DECLARE @CurrentDate DATETIME2(0) = GETDATE();    
    DECLARE @CompareDateFrom DATETIME2(0);    
    DECLARE @CompareDateTo DATETIME2(0);    
    DECLARE @Offset INT = 0;    
    DECLARE @NextFetch INT = 10000;    
    
    IF @InvoiceDate = 'InvoiceCreatedDate'    
       AND @InvoiceCreatedFromDate IS NOT NULL    
       AND @InvoiceCreatedToDate   IS NOT NULL    
    BEGIN    
        SET @CompareDateFrom = TRY_CONVERT(DATETIME2(0), @InvoiceCreatedFromDate);    
        SET @CompareDateTo   = DATEADD(DAY, 1, TRY_CONVERT(DATETIME2(0), @InvoiceCreatedToDate));    
    END    
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'    
            AND @InvoiceEffectiveFromDate IS NOT NULL    
            AND @InvoiceEffectiveToDate   IS NOT NULL    
    BEGIN    
        SET @CompareDateFrom = TRY_CONVERT(DATETIME2(0), @InvoiceEffectiveFromDate);    
        SET @CompareDateTo   = DATEADD(DAY, 1, TRY_CONVERT(DATETIME2(0), @InvoiceEffectiveToDate));    
    END    
    
    IF @PageSize IS NULL OR @PageSize <= 0 SET @PageSize = 10000;    
    --IF @PageIndex IS NULL OR @PageIndex <= 0 SET @PageIndex = 1;    
    
    -- Keep your current paging math (0-based PageIndex expected)    
    SET @Offset    = @PageSize * @PageIndex;    
    SET @NextFetch = @PageSize;    
    
      
    CREATE TABLE #FilteredInvoices    
    (    
        ID INT IDENTITY(1,1) PRIMARY KEY,    
        PolicyId BIGINT,    
        InvoiceId BIGINT NOT NULL,    
        SortDate DATETIME2(0),    
        INDEX IX_InvoiceId (InvoiceId)    
    );    
    
    SELECT @TotalRecords = COUNT(*)    
    FROM Accounting_PolicyDetails PD    
    INNER JOIN Accounting_Invoices I    
        ON I.PolicyDetails_Id = PD.Id AND I.Status = 1    
    WHERE PD.Status = 1    
      AND PD.Client_Id = @Client_Id    
      AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)    
      AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)    
      AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber)    
      AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)    
      AND (    
            @CompareDateFrom IS NULL OR @CompareDateTo IS NULL    
            OR (@InvoiceDate = 'InvoiceEffectiveDate' AND PD.EffectiveDate >= @CompareDateFrom AND PD.EffectiveDate < @CompareDateTo)    
            OR (@InvoiceDate = 'InvoiceCreatedDate'   AND I.CreatedOn     >= @CompareDateFrom AND I.CreatedOn     < @CompareDateTo)    
          )    
      AND (@LOB_Id IS NULL OR EXISTS (    
            SELECT 1    
            FROM Accounting_InvoiceDetails AID    
            WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id    
      ));    
    
    INSERT INTO #FilteredInvoices (PolicyId, InvoiceId, SortDate)    
    SELECT    
        PD.Id,    
        I.Id,    
        PD.EffectiveDate    
    FROM Accounting_PolicyDetails PD    
    INNER JOIN Accounting_Invoices I    
        ON I.PolicyDetails_Id = PD.Id AND I.Status = 1    
    WHERE PD.Status = 1    
      AND PD.Client_Id = @Client_Id    
      AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)    
      AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)    
      AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber)    
      AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)    
      AND (    
            @CompareDateFrom IS NULL OR @CompareDateTo IS NULL    
            OR (@InvoiceDate = 'InvoiceEffectiveDate' AND PD.EffectiveDate >= @CompareDateFrom AND PD.EffectiveDate < @CompareDateTo)    
            OR (@InvoiceDate = 'InvoiceCreatedDate'   AND I.CreatedOn     >= @CompareDateFrom AND I.CreatedOn     < @CompareDateTo)    
          )    
      AND (@LOB_Id IS NULL OR EXISTS (    
            SELECT 1    
            FROM Accounting_InvoiceDetails AID    
            WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id    
      ))    
    ORDER BY PD.EffectiveDate DESC    
    OFFSET @Offset ROWS FETCH NEXT @NextFetch ROWS ONLY    
    OPTION (RECOMPILE);    
    
      
    SELECT    
        PolicyDetails_Id,    
        CAST(MAX(HouseCommission - AgentCommission) AS DECIMAL(18,2)) AS HouseCommissionPct,    
        CAST(MAX(AgentCommission) AS DECIMAL(18,2)) AS AgentCommissionPct,    
        CAST(MAX(HouseCommission) AS DECIMAL(18,2)) AS TotalCommissionPct    
    INTO #PDL_Agg    
    FROM Accounting_PolicyDetailsLineItems PDL    
    INNER JOIN #FilteredInvoices FI on PDL.PolicyDetails_Id = FI.PolicyId    
    GROUP BY PolicyDetails_Id    
    OPTION (RECOMPILE);    
    
       
    SELECT     
        I.Id AS InvoiceId,    
        CAST(SUM(CASE WHEN LIC.Is_Passthrough = 1 THEN ID.Amount ELSE 0 END) AS DECIMAL(18,2)) AS PassthroughFees,    
        CAST(SUM(CASE WHEN LIC.Is_Passthrough = 0 THEN ID.Amount ELSE 0 END) AS DECIMAL(18,2)) AS RetainedFees    
    INTO #InvoiceFees    
    FROM Accounting_InvoiceDetails ID    
    INNER JOIN Accounting_Invoices I ON I.Id = ID.Invoice_Id AND I.Status = 1    
    INNER JOIN Accounting_PolicyDetails PD ON PD.Id = I.PolicyDetails_Id AND PD.Status = 1    
    INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1    
    INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
    INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1    
    INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1    
    WHERE LI.LineItemType_Id = 4    
      AND PD.Client_Id = @Client_Id    
      AND I.Id IN (SELECT InvoiceId FROM #FilteredInvoices)    
    GROUP BY I.Id;    
    
       
    DECLARE @cols NVARCHAR(MAX);    
    
    ;WITH LIs AS    
    (    
        SELECT DISTINCT LI.Name    
        FROM Accounting_InvoiceDetails ID    
        JOIN Accounting_Invoices I    
          ON I.Id = ID.Invoice_Id AND I.Status = 1    
        JOIN Accounting_PolicyDetails PD    
          ON PD.Id = I.PolicyDetails_Id AND PD.Status = 1    
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD    
          ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1    
        JOIN Accounting_LU_LineItems_Directions LD    
          ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
        JOIN Accounting_LU_LineItems_Clients LIC    
          ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1    
        JOIN Accounting_LU_LineItems LI    
          ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1    
        WHERE LI.LineItemType_Id = 3    
          AND PD.Client_Id = @Client_Id    
          AND I.Id IN (SELECT InvoiceId FROM #FilteredInvoices)    
          AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber)    
    )    
    SELECT @cols =    
          STRING_AGG(QUOTENAME(CONCAT(name,' - ','Invoiced')), ',') + ','    
        + STRING_AGG(QUOTENAME(CONCAT(name,' - ','Collected')), ',') + ','    
        + STRING_AGG(QUOTENAME(CONCAT(name,' - ','Paid')), ',')    
    FROM LIs;    
    
      
    IF @cols IS NULL OR LTRIM(RTRIM(@cols)) = ''    
    BEGIN    
        SELECT    
            AC.CompanyName AS Company,    
            AC.CompanyName AS Branch,    
            ID.Name AS [Insured Name],    
            AD.Name AS [Agent Broker Name],    
            PD.RiskState AS [Primary State],    
            CD.Name AS [Issuing Company],    
            CASE WHEN PD.WholesalerDetail_Id IS NULL THEN CD.Name ELSE WD.Name END AS [Billing Company],    
            PD.PolicyNumber AS [Policy Number],    
            PD.TransactionType AS [Transaction Type],    
            CASE    
                WHEN PD.TransactionType = 'CAN' THEN 'Cancelled'    
                WHEN GETDATE() < PD.ExpirationDate THEN 'Active'    
                ELSE 'Expired'    
            END AS [Policy Status],    
            CD.CarrierStatus AS [Carrier Status],    
            LOB.LOBName AS [LOB Name],    
            PD.Submission_Id AS [SID #],    
            I.InvoiceNo AS [Invoice No #],    
            CONVERT(VARCHAR(10), I.CreatedOn, 101) AS [Invoice Created],    
            CONVERT(VARCHAR(10), AJE.CreatedOn, 101) AS [Invoice Booked],    
            CONVERT(VARCHAR(10), PD.EffectiveDate, 101) AS [Policy Effective],    
            CONVERT(VARCHAR(10), PD.ExpirationDate, 101) AS [Policy Expiration],    
            CONVERT(VARCHAR(10),    
                CASE    
                    WHEN PD.TransactionType IN ('NEW','REN') THEN PD.EffectiveDate    
                    WHEN PD.TransactionType = 'END' THEN PD.EndorsementEffectiveDate    
                    WHEN PD.TransactionType = 'CAN' THEN PD.CancelDate    
                END,    
            101) AS [Transaction Effective Date],    
            CASE WHEN ISNULL(LTRIM(RTRIM(PD.PaymentPlan)), '') = '' THEN 'Term Policy' ELSE PD.PaymentPlan END AS [Payment Plan],    
            PD.BillType AS [Bill Type],    
            Details.TotalPremium AS [Total Premium],    
            Details.TotalFees AS [Total Fees],    
            CAST(ISNULL(IFZ.PassthroughFees, 0.00) AS DECIMAL(18,2)) AS [Passthrough Fees],    
            CAST(ISNULL(IFZ.RetainedFees, 0.00)  AS DECIMAL(18,2)) AS [Retained Fees],    
            Details.TotalTaxes AS [Total Taxes],    
            Details.TotalPremium + Details.TotalFees + Details.TotalTaxes AS [Total Policy Billed Amount],    
            CAST(PDL.TotalCommissionPct AS DECIMAL(18,2)) As [Total Commission %],    
            Details.TotalAgentBrokerCommission + Details.TotalHouseCommission AS [Total Commission],    
            CAST(PDL.HouseCommissionPct AS DECIMAL(18,2))  As [House Commission %],    
            Details.TotalHouseCommission AS [Total House Commission],    
            CAST(PDL.AgentCommissionPct AS DECIMAL(18,2))  As [Agency/Broker Commission %],    
            Details.TotalAgentBrokerCommission AS [Total Agent Broker Commission],    
            Details.TotalARInvoiced AS [Total AR Invoiced],    
            I.DueDate AS [Due Date],    
            Details.ARAmountPaid AS [AR Amount Paid],    
            Details.ARBalanceDue AS [AR Balance Due],    
            Details.ARCurrent AS [AR Current],    
            Details.AR1_30,    
            Details.AR31_60,    
            Details.AR61_90,    
            Details.AROver90 AS [AR_Over_90],    
            Details.TotalAPInvoiced AS [Total AP Invoiced],    
            Details.APAmountPaid AS [AP Amount Paid],    
            Details.APBalanceDue AS [AP Balance Due],    
            Details.APCurrent AS [AP Current],    
            Details.AP1_30,    
            Details.AP31_60,    
            Details.AP61_90,    
            Details.APOver90 AS [AP_Over_90],    
            UW.Name AS Underwriter,    
            -- Fallback totals when no dynamic columns exist:    
            (I.Amount) AS ARInvoiced,    
            (I.Amount - I.RemainingBalance) AS ARCollected,    
            (I.Amount - I.RemainingBalance) AS APPaid    
        FROM #FilteredInvoices AS FI    
        INNER JOIN Accounting_PolicyDetails PD ON PD.Id = FI.PolicyId    
        INNER JOIN Accounting_Invoices I ON I.Id = FI.InvoiceId    
        INNER JOIN Accounting_Clients AC ON AC.Client_Id = PD.Client_Id    
        INNER JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id    
        INNER JOIN Accounting_InsuredDetails ID ON ID.InsuredDetail_Id = PD.InsuredDetail_Id    
        INNER JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id = PD.AgentDetail_Id    
        INNER JOIN Accounting_UnderWriterDetails UW ON UW.UnderWriterDetail_Id = PD.UnderWriterDetail_Id AND UW.Status = 1    
    LEFT JOIN Accounting_JournalEntries AJE ON AJE.Source_Id = I.Id AND AJE.Status = 1 AND AJE.SourceType_Id = 2    
        LEFT JOIN #PDL_Agg PDL on PDL.PolicyDetails_Id = FI.PolicyId    
        LEFT JOIN Accounting_WholesalerDetails WD ON WD.WholesalerDetail_Id = PD.WholesalerDetail_Id    
        OUTER APPLY (    
            SELECT TOP 1 AL.Name AS LOBName    
            FROM Accounting_InvoiceDetails AID    
            INNER JOIN Accounting_LU_LOBs AL ON AL.LOB_Id = AID.LOB_Id    
            WHERE AID.Invoice_Id = I.Id AND AID.Status = 1    
            ORDER BY AID.Id    
        ) AS LOB    
        OUTER APPLY (    
            SELECT    
                CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND ((LUD_Name = 'Accounts Receivable') OR (BillingType = 'Direct Bill' AND LUD_Name = 'Internal')) THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,    
                CAST(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalFees,    
                CAST(SUM(CASE WHEN IsHouseFee = 1 THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseFee,    
                CAST(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPayableFee,    
                CAST(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCarrierFees,    
                CAST(SUM(CASE WHEN LIT_Name = 'TAX' AND LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalTaxes,    
                CAST(SUM(CASE WHEN LIT_Name = 'Commission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,    
                CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,    
                CAST(SUM(CASE WHEN LI_Code = 'TotalCommissionPercentage' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,    
                CAST(SUM(CASE WHEN DetailView.LI_Name = 'Agency/Broker Commission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalARInvoiced,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN Amount - RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARAmountPaid,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARBalanceDue,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAPInvoiced,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN Amount - RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APAmountPaid,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APBalanceDue,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARCurrent,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR1_30,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR31_60,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR61_90,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AROver90,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APCurrent,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP1_30,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP31_60,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP61_90,    
                CAST(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APOver90    
            FROM (    
                SELECT    
                    ID.Amount,    
                    ID.RemainingBalance,    
                    I.DueDate AS DueDateLocal,    
                    LIT.Name AS LIT_Name,    
                    LI.Code AS LI_Code,    
                    LI.Name AS LI_Name,    
                    LUD.Name AS LUD_Name,    
                    BT.Name AS BillingType,    
                    CASE WHEN LIT.Name = 'FEE'    
                          AND LUD.Name = 'Accounts Receivable'    
                          AND NOT EXISTS (    
                                SELECT 1    
                                FROM Accounting_InvoiceDetails ID_AP    
                                INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD_AP    
                                  ON BLD_AP.BillingType_LineItems_Directions_Id = ID_AP.BillingType_LineItems_Directions_Id AND BLD_AP.Status = 1    
                                INNER JOIN Accounting_LU_LineItems_Directions LD_AP    
                                  ON LD_AP.LineItems_Directions_Id = BLD_AP.LineItems_Directions_Id AND LD_AP.Status = 1    
                                INNER JOIN Accounting_LU_LineItems_Clients LIC_AP    
                                  ON LIC_AP.LineItems_Clients_Id = LD_AP.LineItems_Clients_Id AND LIC_AP.Status = 1    
                                INNER JOIN Accounting_LU_LineItems LI_AP    
                                  ON LI_AP.LineItem_Id = LIC_AP.LineItem_Id AND LI_AP.Status = 1    
                                INNER JOIN Accounting_LU_LineItemTypes LIT_AP    
                                  ON LIT_AP.LineItemType_Id = LI_AP.LineItemType_Id AND LIT_AP.Status = 1    
                                INNER JOIN Accounting_LU_Directions LUD_AP    
                                  ON LUD_AP.Direction_Id = LD_AP.Direction_Id AND LUD_AP.Status = 1    
                                WHERE ID_AP.Invoice_Id = I.Id AND ID_AP.Status = 1    
                                  AND LIT_AP.Name = 'FEE' AND LUD_AP.Name = 'Accounts Payable'    
                                  AND LI_AP.LineItem_Id = LI.LineItem_Id    
                          )    
                         THEN 1 ELSE 0 END AS IsHouseFee    
                FROM Accounting_InvoiceDetails ID    
                INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1    
                INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
                INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1    
                INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1    
                INNER JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1    
                INNER JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1    
                INNER JOIN Accounting_LU_BillingTypes BT ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1    
                WHERE ID.Invoice_Id = I.Id AND ID.Status = 1    
            ) AS DetailView    
        ) AS Details    
        LEFT JOIN #InvoiceFees IFZ ON IFZ.InvoiceId = I.Id    
        ORDER BY FI.SortDate DESC;    
    
        DROP TABLE #InvoiceFees;    
        DROP TABLE #PDL_Agg;    
        DROP TABLE #FilteredInvoices;    
        RETURN;    
    END    
    
      
    DECLARE @sql NVARCHAR(MAX) = N'    
    SELECT    
        AC.CompanyName AS Company,    
        AC.CompanyName AS Branch,    
        ID.Name AS [Insured Name],    
        AD.Name AS [Agent Broker Name],    
        PD.RiskState AS [Primary State],    
        CD.Name AS [Issuing Company],    
        CASE WHEN PD.WholesalerDetail_Id IS NULL THEN CD.Name ELSE WD.Name END AS [Billing Company],    
        PD.PolicyNumber AS [Policy Number],    
        PD.TransactionType AS [Transaction Type],    
        CASE    
            WHEN PD.TransactionType = ''CAN'' THEN ''Cancelled''    
            WHEN GETDATE() < PD.ExpirationDate THEN ''Active''    
            ELSE ''Expired''    
        END AS [Policy Status],    
        CD.CarrierStatus AS [Carrier Status],    
        LOB.LOBName AS [LOB Name],    
        PD.Submission_Id AS [SID #],    
        I.InvoiceNo AS [Invoice No #],    
        CONVERT(VARCHAR(10), I.CreatedOn, 101) AS [Invoice Created],    
        CONVERT(VARCHAR(10), AJE.CreatedOn, 101) AS [Invoice Booked],    
        CONVERT(VARCHAR(10), PD.EffectiveDate, 101) AS [Policy Effective],    
        CONVERT(VARCHAR(10), PD.ExpirationDate, 101) AS [Policy Expiration],    
        CONVERT(VARCHAR(10),    
            CASE    
                WHEN PD.TransactionType IN (''NEW'',''REN'') THEN PD.EffectiveDate    
                WHEN PD.TransactionType = ''END'' THEN PD.EndorsementEffectiveDate    
                WHEN PD.TransactionType = ''CAN'' THEN PD.CancelDate    
            END, 101) AS [Transaction Effective Date],    
        CASE WHEN ISNULL(LTRIM(RTRIM(PD.PaymentPlan)), '''') = '''' THEN ''Term Policy'' ELSE PD.PaymentPlan END AS [Payment Plan],    
        PD.BillType AS [Bill Type],    
        Details.TotalPremium AS [Total Premium],    
        Details.TotalFees AS [Total Fees],    
        ISNULL(IFZ.PassthroughFees, 0.00) AS [Passthrough Fees],    
        ISNULL(IFZ.RetainedFees, 0.00)  AS [Retained Fees],    
        Details.TotalTaxes AS [Total Taxes],    
        Details.TotalPremium + Details.TotalFees + Details.TotalTaxes AS [Total Policy Billed Amount],    
        CAST(PDL.TotalCommissionPct AS DECIMAL(18,2)) As [Total Commission %],    
        Details.TotalAgentBrokerCommission + Details.TotalHouseCommission AS [Total Commission],    
        CAST(PDL.HouseCommissionPct AS DECIMAL(18,2))  As [House Commission %],    
        Details.TotalHouseCommission AS [Total House Commission],    
        CAST(PDL.AgentCommissionPct AS DECIMAL(18,2))  As [Agency/Broker Commission %],    
        Details.TotalAgentBrokerCommission AS [Total Agent Broker Commission],    
        Details.TotalARInvoiced AS [Total AR Invoiced],    
        I.DueDate AS [Due Date],    
        Details.ARAmountPaid AS [AR Amount Paid],    
        Details.ARBalanceDue AS [AR Balance Due],    
        Details.ARCurrent AS [AR Current],    
        Details.AR1_30,    
        Details.AR31_60,    
        Details.AR61_90,    
        Details.AROver90 AS [AR_Over_90],    
        Details.TotalAPInvoiced AS [Total AP Invoiced],    
        Details.APAmountPaid AS [AP Amount Paid],    
        Details.APBalanceDue AS [AP Balance Due],    
        Details.APCurrent AS [AP Current],    
        Details.AP1_30,    
        Details.AP31_60,    
        Details.AP61_90,    
        Details.APOver90 AS [AP_Over_90],    
        UW.Name AS Underwriter,' + CHAR(10) + '    
        ' + @cols + '    
    
    FROM #FilteredInvoices AS FI    
    INNER JOIN Accounting_PolicyDetails PD ON PD.Id = FI.PolicyId    
    INNER JOIN Accounting_Invoices I ON I.Id = FI.InvoiceId    
    INNER JOIN Accounting_Clients AC ON AC.Client_Id = PD.Client_Id    
    INNER JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id    
    INNER JOIN Accounting_InsuredDetails ID ON ID.InsuredDetail_Id = PD.InsuredDetail_Id    
    INNER JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id = PD.AgentDetail_Id    
    INNER JOIN Accounting_UnderWriterDetails UW ON UW.UnderWriterDetail_Id = PD.UnderWriterDetail_Id AND UW.Status = 1    
    LEFT JOIN Accounting_JournalEntries AJE ON AJE.Source_Id = I.Id AND AJE.Status = 1 AND AJE.SourceType_Id = 2    
    LEFT JOIN #PDL_Agg PDL on PDL.PolicyDetails_Id = FI.PolicyId    
    LEFT JOIN Accounting_WholesalerDetails WD ON WD.WholesalerDetail_Id = PD.WholesalerDetail_Id    
    OUTER APPLY (    
        SELECT TOP 1 AL.Name AS LOBName    
        FROM Accounting_InvoiceDetails AID    
        INNER JOIN Accounting_LU_LOBs AL ON AL.LOB_Id = AID.LOB_Id    
        WHERE AID.Invoice_Id = I.Id AND AID.Status = 1    
        ORDER BY AID.Id    
    ) AS LOB    
    OUTER APPLY (    
        SELECT    
            CAST(SUM(CASE WHEN LIT_Name = ''PREMIUM'' AND ((LUD_Name = ''Accounts Receivable'') OR (BillingType = ''Direct Bill'' AND LUD_Name = ''Internal'')) THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,    
            CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalFees,    
            CAST(SUM(CASE WHEN IsHouseFee = 1 THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseFee,    
            CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPayableFee,    
            CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCarrierFees,    
            CAST(SUM(CASE WHEN LIT_Name = ''TAX'' AND LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalTaxes,    
            CAST(SUM(CASE WHEN LIT_Name = ''Commission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,    
            CAST(SUM(CASE WHEN LI_Code = ''MGACommission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,    
            CAST(SUM(CASE WHEN LI_Code = ''TotalCommissionPercentage'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,    
            CAST(SUM(CASE WHEN DetailView.LI_Name = ''Agency/Broker Commission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalARInvoiced,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' THEN Amount - RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARAmountPaid,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARBalanceDue,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAPInvoiced,    
CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' THEN Amount - RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APAmountPaid,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APBalanceDue,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARCurrent,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR1_30,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR31_60,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR61_90,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AROver90,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APCurrent,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP1_30,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP31_60,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP61_90,    
            CAST(SUM(CASE WHEN DetailView.LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APOver90    
        FROM (    
            SELECT    
                ID.Amount,    
                ID.RemainingBalance,    
                I.DueDate AS DueDateLocal,    
                LIT.Name AS LIT_Name,    
                LI.Code AS LI_Code,    
                LI.Name AS LI_Name,    
                LUD.Name AS LUD_Name,    
                BT.Name AS BillingType,    
                CASE WHEN LIT.Name = ''FEE''    
                      AND LUD.Name = ''Accounts Receivable''    
                      AND NOT EXISTS (    
                            SELECT 1    
                            FROM Accounting_InvoiceDetails ID_AP    
                            INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD_AP    
                              ON BLD_AP.BillingType_LineItems_Directions_Id = ID_AP.BillingType_LineItems_Directions_Id AND BLD_AP.Status = 1    
                            INNER JOIN Accounting_LU_LineItems_Directions LD_AP    
                              ON LD_AP.LineItems_Directions_Id = BLD_AP.LineItems_Directions_Id AND LD_AP.Status = 1    
                            INNER JOIN Accounting_LU_LineItems_Clients LIC_AP    
                              ON LIC_AP.LineItems_Clients_Id = LD_AP.LineItems_Clients_Id AND LIC_AP.Status = 1    
                            INNER JOIN Accounting_LU_LineItems LI_AP    
                              ON LI_AP.LineItem_Id = LIC_AP.LineItem_Id AND LI_AP.Status = 1    
                            INNER JOIN Accounting_LU_LineItemTypes LIT_AP    
                              ON LIT_AP.LineItemType_Id = LI_AP.LineItemType_Id AND LIT_AP.Status = 1    
                            INNER JOIN Accounting_LU_Directions LUD_AP    
                              ON LUD_AP.Direction_Id = LD_AP.Direction_Id AND LUD_AP.Status = 1    
                            WHERE ID_AP.Invoice_Id = I.Id AND ID_AP.Status = 1    
                              AND LIT_AP.Name = ''FEE'' AND LUD_AP.Name = ''Accounts Payable''    
                              AND LI_AP.LineItem_Id = LI.LineItem_Id    
                      )    
                     THEN 1 ELSE 0 END AS IsHouseFee    
            FROM Accounting_InvoiceDetails ID    
            INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1    
            INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
            INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1    
            INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1    
            INNER JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1    
            INNER JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1    
            INNER JOIN Accounting_LU_BillingTypes BT ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1    
            WHERE ID.Invoice_Id = I.Id AND ID.Status = 1    
        ) AS DetailView    
    ) AS Details    
    LEFT JOIN #InvoiceFees IFZ ON IFZ.InvoiceId = I.Id    
    LEFT JOIN (    
        SELECT InvoiceId, ' + @cols + '    
        FROM (    
            SELECT    
                I.Id AS InvoiceId,    
                CONCAT(LI.Name, '' - '', Metric) AS ColName,    
                Value    
            FROM Accounting_InvoiceDetails ID    
            JOIN Accounting_Invoices I    
              ON I.Id = ID.Invoice_Id AND I.Status = 1    
            JOIN Accounting_PolicyDetails PD    
              ON PD.Id = I.PolicyDetails_Id AND PD.Status = 1    
            JOIN Accounting_LU_BillingType_LineItems_Directions BLD    
              ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1    
            JOIN Accounting_LU_LineItems_Directions LD    
              ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1    
            JOIN Accounting_LU_Directions LUD    
              ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1    
            JOIN Accounting_LU_LineItems_Clients LIC    
              ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1    
            JOIN Accounting_LU_LineItems LI    
              ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1    
            CROSS APPLY (VALUES    
                (''Invoiced'',  CASE WHEN LUD.Name = ''Accounts Receivable'' THEN ID.Amount ELSE 0 END),    
                (''Collected'', CASE WHEN LUD.Name = ''Accounts Receivable'' THEN ISNULL(ID.Amount,0) - ISNULL(ID.RemainingBalance,0) ELSE 0 END),    
                (''Paid'',      CASE WHEN LUD.Name = ''Accounts Payable''   THEN ISNULL(ID.Amount,0) - ISNULL(ID.RemainingBalance,0) ELSE 0 END)    
            ) v(Metric, Value)    
            WHERE LI.LineItemType_Id = 3    
              AND I.Id IN (SELECT InvoiceId FROM #FilteredInvoices)    
              AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber)    
        ) src    
        PIVOT (SUM(Value) FOR ColName IN (' + @cols + ')) p    
    ) p ON p.InvoiceId = I.Id    
    ORDER BY FI.SortDate DESC;';    
    
    EXEC sp_executesql    
        @sql,    
        N'@CurrentDate DATETIME2(0), @InvoiceNumber VARCHAR(200)',    
        @CurrentDate = @CurrentDate, @InvoiceNumber = @InvoiceNumber;    
    
       
    DROP TABLE #InvoiceFees;    
    DROP TABLE #PDL_Agg;    
    DROP TABLE #FilteredInvoices;    
END 


-- Download Excel option provided for Tax Filing Report
	insert into Accounting_ReportFileDownloadSettings(Report_Id, FileType_Id, Status) values (8, 1, 1)