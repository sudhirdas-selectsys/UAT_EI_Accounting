/*
 Invoice Production report: build the per-invoice staging data only for the invoices on the returned page.
 Procedure: USP_Accounting_InvoiceProductionCRMAssignmentReport_2026_06_09  (called through LU_SQLQueries, SQLQuery_Id 1).
 Evidence (LOCAL TEST on a restored copy, SQL Server 2022 Standard, about 2.49M invoices; not a production
 measurement). Window 2026-01-01..2026-01-03 with @InvoiceDate = 'InvoiceEffectiveDate', page size 100000:
   - no filters:           531.6 s -> 63.9 s, 18,458 rows, 0 row-hash differences vs the original procedure
   - insured name filter:  255.9 s (V2 prototype) vs 256.6 s here, 25 rows, 0 differences vs V2 (no gain)
   - LOB filter (LOB_Id 12): 519.5 s (V2 prototype) -> 244.5 s, 4,944 rows, 0 differences vs V2
 What changes (compared with the definition on the test copy, line by line): the staging tables for invoice details,
 insured details, journal entries, LOB names, fees and total premium are filled after paging and only for the page's
 invoices; one clustered index on #TempAccounting_InvoiceDetails_InvProd (Invoice_Id); an OPTION (RECOMPILE) hint moves with its statement.
 Not tested: other filters (agent, carrier, wholesaler, invoice number, grouping, transaction-date mode), windows wider
 than 3 days, large page sizes, concurrency. The no-filter case is slower with the clustered index than without it
 (63.9 s vs 35.8 s for the V3 variant); not investigated. Cases 2 and 3 were compared with an earlier prototype, not the original.
 BEFORE APPLYING: this ALTER replaces the whole procedure. It was generated from the definition on the test copy;
 compare it with the definition currently deployed in the target database and stop if they differ.
 Revert: run docs/seed/18_InvoiceProduction_OriginalDefinition_Revert.sql.
 See docs/Iteration1_SlowQuery_Analysis.md (Addenda 7 and 8).
*/
ALTER PROCEDURE [dbo].[USP_Accounting_InvoiceProductionCRMAssignmentReport_2026_06_09]                        
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
 -- @AgentDetail_Id BIGINT = NULL,                                
    @AgentDetail_Id NVARCHAR(MAX) = NULL,                                
    @WholesalerDetail_Id BIGINT = NULL,                                                                
    @InvoiceNumber VARCHAR(200) = NULL,                                                                
   -- @CarrierDetail_Id BIGINT = NULL,
    @CarrierDetail_Id NVARCHAR(MAX) = NULL,
    @LOB_Id INT = NULL,                                                                  
    @ParentAgentDetail_Id BIGINT = NULL,                      
    @LOB_Carrier_Grouping_Id INT = NULL,
	@TransactionEffectiveFromDate VARCHAR(200) = NULL,
	@TransactionEffectiveToDate VARCHAR(200) = NULL,
	@InsuredName VARCHAR(200) = NULL,                                            
    @TotalRecords INT OUT                                                                    
)                           
AS                        
BEGIN                                                                    
    SET NOCOUNT ON                                                                    
                                                                    
    DECLARE @CurrentDate DATETIME2(0) = GETDATE()                                                                    
    DECLARE @CompareDateFrom DATETIME2(0)                                                                    
    DECLARE @CompareDateTo DATETIME2(0)                                                                    
    DECLARE @Offset INT = 0                                                                    
    DECLARE @NextFetch INT = 10000
	DECLARE @UseEffectiveDateForBookedDate BIT = 0
	DECLARE @ShowReferralSource BIT = 0
                                                                      
    IF @InvoiceDate = 'InvoiceCreatedDate' AND @InvoiceCreatedFromDate IS NOT NULL AND @InvoiceCreatedToDate IS NOT NULL                                                                    
    BEGIN                                                                    
        SET @CompareDateFrom = TRY_CONVERT(DATETIME2(0), @InvoiceCreatedFromDate)                                                                    
        SET @CompareDateTo = DATEADD(DAY, 1, TRY_CONVERT(DATETIME2(0), @InvoiceCreatedToDate))                                                                    
    END                                                                    
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate' AND @InvoiceEffectiveFromDate IS NOT NULL AND @InvoiceEffectiveToDate IS NOT NULL                                                                    
    BEGIN                                                                    
        SET @CompareDateFrom = TRY_CONVERT(DATETIME2(0), @InvoiceEffectiveFromDate)                                                                    
        SET @CompareDateTo = DATEADD(DAY, 1, TRY_CONVERT(DATETIME2(0), @InvoiceEffectiveToDate))       
    END
	ELSE IF @InvoiceDate = 'TransactionEffective' AND @TransactionEffectiveFromDate IS NOT NULL AND @TransactionEffectiveToDate IS NOT NULL
	BEGIN
        SET @CompareDateFrom = TRY_CONVERT(DATETIME2(0), @TransactionEffectiveFromDate);
        SET @CompareDateTo = DATEADD(DAY, 1, TRY_CONVERT(DATETIME2(0), @TransactionEffectiveToDate));
   END
                                                                     
 IF @PageSize IS NULL OR @PageSize <= 0 SET @PageSize = 10000                                   
 IF @PageIndex IS NULL OR @PageIndex <= 0 SET @PageIndex = 1                                                        
                       
 SET @Offset = @PageSize * (@PageIndex - 1)                                     
 --SET @Offset = @PageSize * @PageIndex                                    
 SET @NextFetch = @PageSize                       
                                                                       
                                      
                        
 -- Cache the Data at Once Start                        
 DECLARE @TempAgencyHierarchy_InvProd TABLE (AgentDetail_Id BIGINT);                        
 DECLARE @TempAgentIds_InvProd TABLE (AgentDetail_Id BIGINT);                        
                        
 DROP TABLE IF EXISTS #TempBasePolicyDetails_InvProd;                        
 DROP TABLE IF EXISTS #TempLookup_InvProd;                        
 DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails_InvProd;                        
 DROP TABLE IF EXISTS #PDL_Agg;                        
 DROP TABLE IF EXISTS #TotalFilteredInvoices;                        
 DROP TABLE IF EXISTS #FilteredInvoices;                        
 DROP TABLE IF EXISTS #TempInvoiceFeesCTE_InvProd;                        
 DROP TABLE IF EXISTS #TempTotalPremiumCTE_InvProd;                        
 DROP TABLE IF EXISTS #TempAccounting_InsuredDetails_InvProd;                        
 DROP TABLE IF EXISTS #TempJournalEntries_InvProd;                        
 DROP TABLE IF EXISTS #TempLOB_InvProd;        
 DROP TABLE IF EXISTS #LatestCancelSubmissions;       
                   
 CREATE TABLE #TotalFilteredInvoices (                                                                    
        ID INT IDENTITY(1,1) PRIMARY KEY,                                                                      
        PolicyId BIGINT,                                                        
        InvoiceId BIGINT NOT NULL,                                                                    
        SortDate DATETIME2(0),                                                                      
       INDEX IX_InvoiceId (InvoiceId)                                                                    
    )                        
                  
 CREATE TABLE #FilteredInvoices (                                                                    
        ID INT IDENTITY(1,1) PRIMARY KEY,                                                                      
        PolicyId BIGINT,                                                        
        InvoiceId BIGINT NOT NULL,                                                                    
        SortDate DATETIME2(0),                                                                      
       INDEX IX_InvoiceId (InvoiceId)                                                                    
    )                                 
                         
 -- #TempAgencyHierarchy                         
 IF @ParentAgentDetail_Id IS NOT NULL                        
 BEGIN                        
     INSERT INTO @TempAgencyHierarchy_InvProd (AgentDetail_Id)                        
     SELECT AgentDetail_Id FROM dbo.GetAgencyHierarchy(@ParentAgentDetail_Id);                        
 END                        
                        
 -- #TempAgentIds                         
 IF @AgentDetail_Id IS NOT NULL                        
 BEGIN                        
     INSERT INTO @TempAgentIds_InvProd (AgentDetail_Id)                        
     SELECT TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT) FROM STRING_SPLIT(@AgentDetail_Id, ',')                        
     WHERE TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT) IS NOT NULL;                        
 END                        
              
         
 DECLARE @DueUponReceipt BIT = 0        
        
SELECT         
    @DueUponReceipt =         
        CASE         
            WHEN SettingValue = 'Due Upon Receipt' and SettingKey = 'Reports.InvoiceDueDate' THEN 1        
            ELSE 0        
        END        
FROM Accounting_ClientSettings        
WHERE Client_Id = @Client_Id        
        
 SELECT TOP 1 @UseEffectiveDateForBookedDate =
        CASE
            WHEN SettingValue = 'true' THEN 1
            ELSE 0
        END
FROM Accounting_ClientSettings WHERE Client_Id = @Client_Id  AND SettingKey = 'Reports.InvoiceBookedDate.UseEffectiveDate' AND Status = 1; 

 SELECT TOP 1 @ShowReferralSource =
        CASE
            WHEN LOWER(ISNULL(SettingValue, '')) = 'true' THEN 1
            ELSE 0
        END
FROM Accounting_ClientSettings WHERE Client_Id = @Client_Id AND SettingKey = 'Reports.InvoiceProductionReport.ShowReferralSource' AND Status = 1; 

DECLARE @ShowCarrierCode BIT = 0;
SELECT TOP 1
    @ShowCarrierCode =
        CASE
            WHEN LOWER(ISNULL(SettingValue, '')) = 'true' THEN 1
            ELSE 0
        END
FROM Accounting_ClientSettings
WHERE Client_Id = @Client_Id AND SettingKey = 'Reports.InvoiceProductionReport.ShowCarrierCodeInIssuingCompany' AND Status = 1;
 -- #TempBasePolicyDetails_InvProd Start                                                     
 SELECT IES.PolicyDetails_Id,IES.InvoiceNo,IES.Id AS Invoice_Id,IES.Amount AS InvoicesAmount,IES.RemainingBalance AS InvoicesRemainingBalance,          
CASE WHEN @DueUponReceipt = 1 THEN IES.CreatedOn ELSE IES.DueDate END AS InvoicesDueDate,        
 --IES.DueDate AS InvoicesDueDate,        
 IES.CreatedOn AS InvoicesCreatedOn,                        
 APD.Client_Id,APD.PolicyNumber,APD.AgentDetail_Id,APD.BillType,APD.CarrierDetail_Id,APD.InsuredDetail_Id,APD.EffectiveDate,APD.ExpirationDate,APD.WholesalerDetail_Id,          
 APD.TransactionType,                        
 APD.EndorsementNumber,APD.CreatedOn AS PolicyDetailsCreatedOn,                        
 CD.Name AS CarrierName,APD.UnderWriterDetail_Id,APD.RiskState,APD.Submission_Id,APD.PaymentPlan,APD.EndorsementEffectiveDate,APD.CancelDate,FD.Name as FinanceCompany                    
 INTO #TempBasePolicyDetails_InvProd                        
 FROM Accounting_PolicyDetails APD                        
 INNER JOIN Accounting_Invoices IES ON APD.Id = IES.PolicyDetails_Id AND IES.Status = 1 AND APD.Client_Id = IES.Client_Id AND IES.Client_Id = @Client_Id                        
 LEFT JOIN Accounting_CarrierDetails CD ON APD.CarrierDetail_Id = CD.CarrierDetail_Id AND CD.Status = 1 AND APD.Client_Id = CD.Client_Id                  
 LEFT JOIN Accounting_FinanceDetails FD ON FD.FinanceDetail_Id = APD.FinanceDetail_Id AND FD.Status=1 AND APD.Client_Id = FD.Client_Id                   
 WHERE APD.Status = 1 AND APD.Client_Id = @Client_Id                        
                        
 CREATE NONCLUSTERED INDEX NC_TempBasePolicyDetails_InvProd_ClientPolicy ON #TempBasePolicyDetails_InvProd (Client_Id, PolicyDetails_Id)                        
 INCLUDE (InvoiceNo, Invoice_Id, InvoicesAmount, InvoicesRemainingBalance, InvoicesDueDate);                       
                        
 --#TempBasePolicyDetails_InvProd End                        
                        
 -- #TempAccounting_InsuredDetails_InvProd Start                                                     
 SELECT AINSD.* INTO #TempAccounting_InsuredDetails_InvProd                        
 FROM Accounting_InsuredDetails AINSD                        
 INNER JOIN (SELECT DISTINCT InsuredDetail_Id FROM #TempBasePolicyDetails_InvProd) TBPD ON AINSD.InsuredDetail_Id = TBPD.InsuredDetail_Id                        
 WHERE AINSD.Client_Id = @Client_Id AND AINSD.Status = 1 AND @InsuredName IS NOT NULL                        
                        
 CREATE NONCLUSTERED INDEX NC_TempAccounting_InsuredDetails_InvProd ON #TempAccounting_InsuredDetails_InvProd (Client_Id, InsuredDetail_Id)                        
 -- #TempAccounting_InsuredDetails_InvProd End                        
                        
 --#TempLookup_InvProd Start                        
 SELECT                         
    BLD.BillingType_LineItems_Directions_Id,                        
 BLD.BillingType_Id,                        
    LI.Code        AS LineItemCode,                        
    LI.Name        AS LineItemName,                        
    LI.LineItem_Id,                        
 LI.LineItemType_Id,                        
    LIT.Code       AS LineItemTypeCode,                        
 LIT.Name AS LineItemTypeName,                        
    LUD.Code       AS DirectionCode,                        
 LUD.Name AS DirectionName,                        
    LUD.Direction_Id,                        
 LIC.Is_Passthrough                        
 INTO #TempLookup_InvProd                        
 FROM Accounting_LU_BillingType_LineItems_Directions BLD                        
 JOIN Accounting_LU_LineItems_Directions LD  ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id  AND LD.Status  = 1                        
 JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1 AND LIC.Client_Id = @Client_Id                        
 JOIN Accounting_LU_LineItems LI  ON LI.LineItem_Id  = LIC.LineItem_Id AND LI.Status  = 1                        
 JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id  = LI.LineItemType_Id  AND LIT.Status = 1                        
 JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1                        
 WHERE BLD.Status = 1;                 
                         
 CREATE NONCLUSTERED INDEX NC_TempLookup_InvProd ON #TempLookup_InvProd (BillingType_LineItems_Directions_Id) INCLUDE (LineItemCode, LineItemName, LineItemTypeCode, DirectionCode, Direction_Id);                        
 --#TempLookup_InvProd End                        
                        
 -- #TempAccounting_InvoiceDetails_InvProd Start                                                     
 SELECT AID.* INTO #TempAccounting_InvoiceDetails_InvProd                        
 FROM Accounting_InvoiceDetails AID                        
 INNER JOIN #TempBasePolicyDetails_InvProd TBPD ON AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1 AND AID.Client_Id = @Client_Id AND (@LOB_Id IS NOT NULL OR @LOB_Carrier_Grouping_Id IS NOT NULL)                        
                        
 -- PerfTest V4: clustered on Invoice_Id so the per-invoice lookups (final SELECT, fees, LOB, LOB filter) seek instead of scanning the whole staging table
 CREATE CLUSTERED INDEX CIX_TempAccounting_InvoiceDetails_InvProd ON #TempAccounting_InvoiceDetails_InvProd (Invoice_Id)
 CREATE NONCLUSTERED INDEX NC_TempAccounting_InvoiceDetails_InvProd ON #TempAccounting_InvoiceDetails_InvProd (Id, Invoice_Id,BillingType_LineItems_Directions_Id)                           
 -- #TempAccounting_InvoiceDetails_InvProd End                        
                        
 -- #TempJournalEntries_InvProd Start                            
 -- (journal entries moved below: built only for the page invoices; PerfTest V3)                        
 -- #TempJournalEntries_InvProd End                        
                        
 -- #TempLOB_InvProd Start                        
 --SELECT AID.Invoice_Id,AL.Name AS LOBName,ROW_NUMBER() OVER (PARTITION BY AID.Invoice_Id ORDER BY AID.Id) AS RN                        
 --INTO #TempLOB_InvProd                        
 --FROM #TempAccounting_InvoiceDetails_InvProd AID                        
 --INNER JOIN Accounting_LU_LOBs AL ON AL.LOB_Id = AID.LOB_Id                        
 --WHERE AID.Status = 1;                        
                        
 --CREATE NONCLUSTERED INDEX NC_TempLOB_InvProd ON #TempLOB_InvProd (Invoice_Id) WHERE RN = 1;               
 -- #TempLOB_InvProd Start            
-- (LOB aggregation moved below: built only for the page invoices; PerfTest V3)            
-- #TempLOB_InvProd End            
 -- #TempLOB_InvProd End                        
                                                                          
  INSERT INTO #TotalFilteredInvoices (PolicyId, InvoiceId, SortDate)                                                                    
 SELECT TBPD.PolicyDetails_Id,TBPD.Invoice_Id,TBPD.EffectiveDate                      
 FROM #TempBasePolicyDetails_InvProd TBPD                                                                   
 WHERE                                                                 
    -- AND (@AgentDetail_Id IS NULL OR TBPD.AgentDetail_Id = @AgentDetail_Id)                                
    (@AgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InvProd))                                 
   AND (@WholesalerDetail_Id IS NULL OR TBPD.WholesalerDetail_Id = @WholesalerDetail_Id)                                                                
 --AND (@InvoiceNumber IS NULL OR TBPD.InvoiceNo=@InvoiceNumber)                                       
   AND (@InvoiceNumber IS NULL OR TBPD.InvoiceNo = @InvoiceNumber OR TBPD.InvoiceNo LIKE '%' + @InvoiceNumber + '%'   OR TRY_CAST(REPLACE(TBPD.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))                                      
 --AND (@CarrierDetail_Id IS NULL OR TBPD.CarrierDetail_Id = @CarrierDetail_Id)
 AND (NULLIF(LTRIM(RTRIM(@CarrierDetail_Id)), '') IS NULL OR TBPD.CarrierDetail_Id IN (SELECT TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT)
        FROM STRING_SPLIT(@CarrierDetail_Id, ',') WHERE TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT) IS NOT NULL))
    AND (                                                                    
          @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                                                           
          OR (@InvoiceDate = 'InvoiceEffectiveDate' AND TBPD.EffectiveDate >= @CompareDateFrom AND TBPD.EffectiveDate <= @CompareDateTo)
          OR (@InvoiceDate = 'InvoiceCreatedDate' AND TBPD.InvoicesCreatedOn >= @CompareDateFrom AND TBPD.InvoicesCreatedOn <= @CompareDateTo) 
		  OR (@InvoiceDate = 'TransactionEffective'  AND
          CASE
              WHEN TBPD.TransactionType = 'END'
                  THEN TBPD.EndorsementEffectiveDate
              WHEN TBPD.TransactionType = 'CAN'
                  THEN TBPD.CancelDate
              ELSE TBPD.EffectiveDate
          END >= @CompareDateFrom
          AND
          CASE
              WHEN TBPD.TransactionType = 'END'
                  THEN TBPD.EndorsementEffectiveDate
              WHEN TBPD.TransactionType = 'CAN'
                  THEN TBPD.CancelDate
              ELSE TBPD.EffectiveDate
          END < @CompareDateTo
      )
    )                                                                    
    AND (@LOB_Id IS NULL OR EXISTS (                                                           
            SELECT 1                                                                     
            FROM #TempAccounting_InvoiceDetails_InvProd AID                                                                     
            WHERE AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id                              
    ))
 AND (@InsuredName IS NULL OR EXISTS (SELECT 1 FROM #TempAccounting_InsuredDetails_InvProd ID WHERE ID.InsuredDetail_Id = TBPD.InsuredDetail_Id AND ID.Name LIKE '%' + LTRIM(RTRIM(@InsuredName)) + '%'))
 AND (@ParentAgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InvProd))                
 AND (     @LOB_Carrier_Grouping_Id IS NULL     OR EXISTS (         SELECT 1         FROM Accounting_LOB_Carrier_Grouping LCG             
 JOIN Accounting_Carrier_GroupingDetails CG              ON LCG.LOB_Carrier_Grouping_Id = CG.LOB_Carrier_Grouping_Id AND CG.Status = 1             
 LEFT JOIN Accounting_LOB_GroupingDetails LG              ON LG.LOB_Carrier_Grouping_Id = LCG.LOB_Carrier_Grouping_Id AND LG.Status = 1             
 INNER JOIN #TempAccounting_InvoiceDetails_InvProd AID                        
ON AID.Invoice_Id = TBPD.Invoice_Id              AND AID.LOB_Id = LG.LOB_Id         WHERE              LCG.Status = 1             AND LCG.Client_Id = @Client_Id                 
AND LCG.LOB_Carrier_Grouping_Id = @LOB_Carrier_Grouping_Id     ) )                
                        
    --OPTION (RECOMPILE)      
  
  ---- EIH-1298     
 Select TBPD.PolicyNumber,Max(TBPD.Submission_Id) As LatestCancelSubmissionId into #LatestCancelSubmissions from #TempBasePolicyDetails_InvProd TBPD where TBPD.TransactionType = 'CAN' group by TBPD.PolicyNumber;    
                    
  SELECT @TotalRecords = COUNT(1) FROM #TotalFilteredInvoices                  
                  
 INSERT INTO #FilteredInvoices (PolicyId, InvoiceId, SortDate)                                                                    
    SELECT PolicyId,InvoiceId,SortDate                                                                    
    FROM #TotalFilteredInvoices                   
    ORDER BY SortDate DESC                                            
    OFFSET @Offset ROWS FETCH NEXT @NextFetch ROWS ONLY                                                     
    OPTION (RECOMPILE)

-- PerfTest V3: staging tables that only the returned page needs are filled now, restricted to the page invoices.
IF @LOB_Id IS NULL AND @LOB_Carrier_Grouping_Id IS NULL
    INSERT INTO #TempAccounting_InvoiceDetails_InvProd
    SELECT AID.* FROM Accounting_InvoiceDetails AID
    INNER JOIN #FilteredInvoices FI ON AID.Invoice_Id = FI.InvoiceId AND AID.Status = 1 AND AID.Client_Id = @Client_Id;
IF @InsuredName IS NULL
    INSERT INTO #TempAccounting_InsuredDetails_InvProd
    SELECT AINSD.* FROM Accounting_InsuredDetails AINSD
    INNER JOIN (SELECT DISTINCT TB.InsuredDetail_Id FROM #TempBasePolicyDetails_InvProd TB INNER JOIN #FilteredInvoices FI ON FI.InvoiceId = TB.Invoice_Id) X ON AINSD.InsuredDetail_Id = X.InsuredDetail_Id
    WHERE AINSD.Client_Id = @Client_Id AND AINSD.Status = 1;
SELECT AJE.Source_Id AS Invoice_Id,MIN(AJE.CreatedOn) AS CreatedOn                         
 INTO #TempJournalEntries_InvProd                        
 FROM Accounting_JournalEntries AJE                        
 INNER JOIN #FilteredInvoices TBPD ON AJE.Source_Id = TBPD.InvoiceId                        
 WHERE AJE.Status = 1 AND AJE.SourceType_Id = 2                        
 GROUP BY AJE.Source_Id;                        
                        
 CREATE NONCLUSTERED INDEX NC_TempJournalEntries_InvProd ON #TempJournalEntries_InvProd (Invoice_Id);

SELECT             
    AID.Invoice_Id,            
    STUFF((            
        SELECT DISTINCT ', ' + AL2.Name            
        FROM #TempAccounting_InvoiceDetails_InvProd AID2            
        INNER JOIN Accounting_LU_LOBs AL2 ON AL2.LOB_Id = AID2.LOB_Id            
        WHERE AID2.Invoice_Id = AID.Invoice_Id AND AID2.Status = 1            
        FOR XML PATH(''), TYPE            
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBName            
INTO #TempLOB_InvProd            
FROM #TempAccounting_InvoiceDetails_InvProd AID            
WHERE AID.Status = 1 AND AID.Invoice_Id IN (SELECT InvoiceId FROM #FilteredInvoices)
GROUP BY AID.Invoice_Id;            
            
CREATE NONCLUSTERED INDEX NC_TempLOB_InvProd ON #TempLOB_InvProd (Invoice_Id);
                                                                    
                                                                    
 SELECT PolicyDetails_Id,                                                                        
    CAST(MAX(HouseCommission - AgentCommission) AS DECIMAL(18,2)) AS HouseCommissionPct,                                                                     CAST(MAX(AgentCommission) AS DECIMAL(18,2)) AS AgentCommissionPct,                               
   
                                        
    CAST(MAX(HouseCommission) AS DECIMAL(18,2)) AS TotalCommissionPct                                                                         
    INTO #PDL_Agg                                                      
    FROM Accounting_PolicyDetailsLineItems PDL                                                                     
    INNER JOIN #FilteredInvoices FI on PDL.PolicyDetails_Id = FI.PolicyId     
 WHERE PDL.Client_Id = @Client_Id    
    GROUP BY PolicyDetails_Id                                                                
 OPTION (RECOMPILE)                                      
                         
 SELECT TBPD.Invoice_Id AS InvoiceId,                                                          
    CAST(SUM(CASE WHEN TL.Is_Passthrough = 1 and TL.Direction_Id=1 THEN ID.Amount ELSE 0 END) AS DECIMAL(18,2)) AS PassthroughFees,                                                          
    CAST(SUM(CASE WHEN TL.Is_Passthrough = 0 THEN ID.Amount ELSE 0 END) AS DECIMAL(18,2)) AS RetainedFees                          
 INTO #TempInvoiceFeesCTE_InvProd                        
    FROM #TempAccounting_InvoiceDetails_InvProd ID                                                          
 INNER JOIN #TempBasePolicyDetails_InvProd TBPD ON TBPD.Invoice_Id = ID.Invoice_Id
 INNER JOIN #FilteredInvoices FIX ON FIX.InvoiceId = TBPD.Invoice_Id                         
 INNER JOIN #TempLookup_InvProd TL ON TL.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id                         
    WHERE TL.LineItemType_Id = 4                                                    
    GROUP BY TBPD.Invoice_Id                                
                        
 SELECT SUM(PDLI.Premium) AS TotalPremium,TBPD.PolicyDetails_Id                        
 INTO #TempTotalPremiumCTE_InvProd                        
 FROM Accounting_PolicyDetailsLineItems PDLI                            
 JOIN (SELECT DISTINCT PolicyId AS PolicyDetails_Id FROM #FilteredInvoices)TBPD ON PDLI.PolicyDetails_Id= TBPD.PolicyDetails_Id AND PDLI.Status=1    
 AND PDLI.Client_Id = @Client_Id    
 WHERE PDLI.LineType='PREMIUM' GROUP BY TBPD.PolicyDetails_Id                        
                        
 CREATE NONCLUSTERED INDEX NC_TempTotalPremiumCTE_InvProd ON #TempTotalPremiumCTE_InvProd (PolicyDetails_Id)            
       
    
   
      
                                                           
    SELECT                                                                    
        --FI.SortDate,                                                                    
        AC.CompanyName AS Company,                                                                    
        AC.CompanyName AS Branch,                                                                 
        ID.Name AS [Insured Name],                                               
        AD.Name AS [Agent Broker Name], 
		CASE
    WHEN @ShowReferralSource = 1 THEN ID.ReferralSource
    ELSE NULL
END AS [Referral Source],
        TBPD.RiskState AS [Primary State],                                                                    
        --CD.Name AS [Issuing Company], 
		CASE
    WHEN @ShowCarrierCode = 1
        THEN CONCAT(CD.Name, ' - ', CD.CarrierReference_Id)
    ELSE CD.Name
END AS [Issuing Company],
        CASE WHEN TBPD.WholesalerDetail_Id IS NULL THEN CD.Name ELSE WD.Name END AS [Billing Company],                    
  TBPD.FinanceCompany as [Finance Company],                    
        TBPD.PolicyNumber AS [Policy Number],                                                                              
        --TBPD.TransactionType AS [Transaction Type],           
  -- EndorsementNumber added on 05/13/2026 by Mallika          
  CASE          
          WHEN TBPD.TransactionType = 'END' AND TBPD.EndorsementNumber IS NOT NULL THEN CONCAT(TBPD.TransactionType, ' #', TBPD.EndorsementNumber)          
          ELSE TBPD.TransactionType          
        END AS [Transaction Type],          
   CASE     WHEN TBPD.TransactionType = 'CAN'         THEN 'Cancelled'     WHEN LCS.LatestCancelSubmissionId IS NOT NULL          AND TBPD.Submission_Id < LCS.LatestCancelSubmissionId         THEN 'Cancelled'     WHEN GETDATE() < TBPD.ExpirationDate  
         THEN 'Active'     ELSE 'Expired' END AS [Policy Status], -- EIH-1298                                                              
  CD.CarrierStatus as [Carrier Status],                                                              
        LOB.LOBName AS [LOB Name],                                                                    
        TBPD.Submission_Id AS [SID #],                                                                    
        TBPD.InvoiceNo AS [Invoice No #],                                                      
  CONVERT(VARCHAR(10), TBPD.InvoicesCreatedOn, 101) AS [Invoice Created],                                                  
  --CONVERT(VARCHAR(10), AJE.CreatedOn, 101) AS [Invoice Booked],
  CONVERT(VARCHAR(10),
    CASE
        WHEN @UseEffectiveDateForBookedDate = 1 THEN
            CASE
                WHEN TBPD.TransactionType IN ('CAN', 'END')
                    THEN TBPD.EndorsementEffectiveDate
                ELSE TBPD.EffectiveDate
            END
        ELSE
            TBPD.InvoicesCreatedOn
    END,
    101
) AS [Invoice Booked],
  CONVERT(VARCHAR(10), TBPD.EffectiveDate, 101) AS [Policy Effective],                                                            
  --CASE WHEN PD.TransactionType = 'END' THEN CONVERT(VARCHAR(10), PD.EndorsementEffectiveDate, 101) ELSE CONVERT(VARCHAR(10), PD.EffectiveDate, 101) END AS [Effective Date],                                                      
  --CONVERT(VARCHAR(10), PD.EffectiveDate, 101) AS [Invoice Booked],                                                          
  CONVERT(VARCHAR(10), TBPD.ExpirationDate, 101) AS [Policy Expiration],                                                      
  /*CONVERT(VARCHAR(10),                                                      
  CASE                                                       
      WHEN TBPD.TransactionType IN ('NEW','REN') THEN TBPD.EffectiveDate                                                      
      WHEN TBPD.TransactionType = 'END' THEN TBPD.EndorsementEffectiveDate                                                      
      WHEN TBPD.TransactionType = 'CAN' THEN TBPD.CancelDate                                                      
  END,                                                       
  101) AS [Transaction Effective Date],  */
  CONVERT(VARCHAR(10),
  CASE
      WHEN TBPD.TransactionType = 'END' THEN TBPD.EndorsementEffectiveDate
      WHEN TBPD.TransactionType = 'CAN' THEN TBPD.CancelDate
      ELSE TBPD.EffectiveDate
  END,
  101) AS [Transaction Effective Date],
  --CASE WHEN PD.TransactionType = 'CAN' THEN CONVERT(VARCHAR(10), PD.CancelDate, 101) ELSE CONVERT(VARCHAR(10), PD.ExpirationDate, 101) END AS [Expiration Date],                                                            
        CASE WHEN ISNULL(LTRIM(RTRIM(TBPD.PaymentPlan)), '') = '' THEN 'Term Policy' ELSE TBPD.PaymentPlan END AS [Payment Plan],                                                                    
        TBPD.BillType AS [Bill Type],                                                    
  CASE WHEN ISNULL(TBPD.PaymentPlan, '') <> '' THEN TBPD.InvoicesAmount - (Details.TotalFees + Details.TotalTaxes) ELSE TP.TotalPremium END AS [Total Premium],                       
        --Details.TotalPremium + Details.TotalHouseCommission + Details.TotalAgentBrokerCommission AS [Total Premium],      -- + Details.TotalHouseCommission + Details.TotalAgentBrokerCommission                                                             
 
        Details.TotalFees AS [Total Fees],                                                                                           
  --Details.TotalHouseFee AS [Total House Fee],                                                                             
  --Details.TotalPayableFee AS [Total Payable Fee],                                                                
  --Details.TotalCarrierFees AS [Total Carrier Fees],                                                          
  CAST(ISNULL(IFC.PassthroughFees, 0.00) AS DECIMAL(18,2)) AS [Passthrough Fees],                                                         
  CAST(ISNULL(IFC.RetainedFees, 0.00) AS DECIMAL(18,2))  AS [Retained Fees],                                                          
  Details.TotalTaxes AS [Total Taxes],                                                                                                                          
  ROUND(CASE WHEN ISNULL(TBPD.PaymentPlan, '') <> '' THEN TBPD.InvoicesAmount ELSE TP.TotalPremium + Details.TotalFees + Details.TotalTaxes END ,2) AS [Total Policy Billed Amount],              --removed EIH-1478  + Details.TotalHouseCommission + Details.TotalAgentBrokerCommission                                                       
  CAST(PDL.TotalCommissionPct   AS DECIMAL(18,2)) As [Total Commission %]  ,                                                                                      
  Details.TotalAgentBrokerCommission + Details.TotalHouseCommission AS [Total Commission],                                                  
  CAST((PDL.HouseCommissionPct ) AS DECIMAL(18,2))  As [House Commission %],                                                           
  Details.TotalHouseCommission AS [Total House Commission],                                                                              
  CAST(PDL.AgentCommissionPct  AS DECIMAL(18,2))  As [Agency/Broker Commission %],                                                                      
   Details.TotalAgentBrokerCommission AS [Total Agent Broker Commission],                                                                                           
   Details.TotalARInvoiced AS [Total AR Invoiced],		
   Details.SurplusLinesTax AS [Surplus Lines Tax],
   Details.StampingFee AS [Stamping Fee],   
   CASE
    WHEN TBPD.WholesalerDetail_Id IS NOT NULL THEN WD.Name
    ELSE CONCAT(CD.Name, '-', CD.CarrierReference_Id)
    END AS [Issuing Company/Billing Company],
  CONVERT(VARCHAR(10),TBPD.InvoicesCreatedOn, 101) AS [Invoice Created/Invoice Booked],
  CONVERT(VARCHAR(10), TBPD.InvoicesDueDate, 101) AS [Due Date],                                                                                                  
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
  Details.APOver90 AS [AP_Over_90],                                                                        
  UW.Name AS Underwriter,        
  AD.DBA        
    FROM #FilteredInvoices AS FI                                                                        
    INNER JOIN #TempBasePolicyDetails_InvProd TBPD ON TBPD.Invoice_Id = FI.InvoiceId                                             
 Inner Join #TempTotalPremiumCTE_InvProd TP ON TP.PolicyDetails_Id=TBPD.PolicyDetails_Id                        
    INNER JOIN Accounting_Clients AC ON AC.Client_Id = TBPD.Client_Id                                                           
    INNER JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = TBPD.CarrierDetail_Id                                                                    
    INNER JOIN #TempAccounting_InsuredDetails_InvProd ID ON ID.InsuredDetail_Id = TBPD.InsuredDetail_Id                                                                    
    INNER JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id = TBPD.AgentDetail_Id                                                                    
    INNER JOIN Accounting_UnderWriterDetails UW ON UW.UnderWriterDetail_Id = TBPD.UnderWriterDetail_Id AND UW.Status = 1                                                      
    LEFT JOIN #TempJournalEntries_InvProd AJE ON AJE.Invoice_Id = TBPD.Invoice_Id                         
    LEFT JOIN #PDL_Agg PDL on PDL.PolicyDetails_Id = FI.PolicyId                                                                    
    LEFT JOIN Accounting_WholesalerDetails WD ON WD.WholesalerDetail_Id = TBPD.WholesalerDetail_Id                                                           
   -- LEFT JOIN #TempLOB_InvProd LOB ON LOB.Invoice_Id = TBPD.Invoice_Id AND LOB.RN = 1              
   LEFT JOIN #TempLOB_InvProd LOB ON LOB.Invoice_Id = TBPD.Invoice_Id    
   LEFT JOIN #LatestCancelSubmissions LCS ON LCS.Policynumber =  TBPD.PolicyNumber    
    OUTER APPLY (                                                                    
        SELECT                                                
            --CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND ((LUD_Name = 'Accounts Receivable') OR (BillingType = 'Direct Bill'                                          
            --         AND LUD_Name = 'Internal'))THEN Amount ELSE 0 END ) AS DECIMAL(18,2)) AS TotalPremium,                                            
  CAST(ISNULL(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalFees,                                                                        
  CAST(ISNULL(SUM(CASE WHEN IsHouseFee = 1 THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalHouseFee,                                                                          
  CAST(ISNULL(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalPayableFee,                                                       
  CAST(ISNULL(SUM(CASE WHEN LIT_Name = 'FEE' AND LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalCarrierFees,                                                                          
  CAST(ISNULL(SUM(CASE WHEN LIT_Name = 'TAX' AND LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalTaxes,                                                                          
  CAST(ISNULL(SUM(CASE WHEN LIT_Name = 'Commission' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalCommission,                                                                          
  CAST(ISNULL(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalHouseCommission,                                                                         
  CAST(ISNULL(SUM(CASE WHEN LI_Code = 'TotalCommissionPercentage' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalCommissionPercentage,                                                                           
  CAST(ISNULL(SUM(CASE WHEN DetailView.LI_Name = 'Agency/Broker Commission' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalARInvoiced,        
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN Amount - RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS ARAmountPaid,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS ARBalanceDue,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN Amount ELSE 0 END),0) AS DECIMAL(18,2)) AS TotalAPInvoiced,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN Amount - RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS APAmountPaid,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS APBalanceDue,                                                                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS ARCurrent,                                              
  CAST(ISNULL(SUM(CASE WHEN DetailView.LI_Code = 'Surplus Lines' AND DetailView.LUD_Name = 'Accounts Receivable' THEN DetailView.Amount ELSE 0 END), 0) AS DECIMAL(18,2)) AS SurplusLinesTax,
  CAST(ISNULL(SUM(CASE WHEN DetailView.LI_Code = 'StampingFee' AND DetailView.LUD_Name = 'Accounts Receivable' THEN DetailView.Amount ELSE 0 END), 0) AS DECIMAL(18,2)) AS StampingFee, 
      
        
                    
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AR1_30,                                      
  
    
      
        
                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AR31_60,                                          
  
    
      
        
                    
  CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AR61_90,                                          
  
    
      
        
                    
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Receivable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AROver90,                                                
  
    
      
        
         
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) <= 0 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS APCurrent,                                                  
  
    
      
        
                  
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AP1_30,                                         
  
    
      
        
                    
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AP31_60,                                       
  
    
      
        
                    
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS AP61_90,                                       
  
    
      
        
                    
        CAST(ISNULL(SUM(CASE WHEN DetailView.LUD_Name = 'Accounts Payable' AND DATEDIFF(DAY, DetailView.DueDateLocal, @CurrentDate) > 90 THEN RemainingBalance ELSE 0 END),0) AS DECIMAL(18,2)) AS APOver90                                                    
  
    
      
        
                
        FROM (                                                                    
     SELECT                                                                    
                ID.Amount,                                                                  
                ID.RemainingBalance,                                                                    
                TBPD.InvoicesDueDate AS DueDateLocal,                                                                    
                TL.LineItemTypeName AS LIT_Name,                                                                
                TL.LineItemCode AS LI_Code,                                                                    
 TL.LineItemName AS LI_Name,                                                                    
                TL.DirectionName AS LUD_Name,                                          
    BT.Name AS BillingType,                                          
                CASE WHEN TL.LineItemTypeName = 'FEE'                                                                          
                AND TL.DirectionName = 'Accounts Receivable'                                                                      
                     AND NOT EXISTS (                                   
                        SELECT 1                                                                          
                        FROM #TempAccounting_InvoiceDetails_InvProd ID_AP                                                                          
                        INNER JOIN #TempLookup_InvProd TL1 ON TL1.BillingType_LineItems_Directions_Id = ID_AP.BillingType_LineItems_Directions_Id                        
                        WHERE ID_AP.Invoice_Id = TBPD.Invoice_Id AND TL1.LineItemTypeName = 'FEE' AND TL1.DirectionName = 'Accounts Payable'                                                                          
      AND TL1.LineItem_Id = TL.LineItem_Id                                                                          
    )                                                                          
                THEN 1 ELSE 0 END AS IsHouseFee                                           
            FROM #TempAccounting_InvoiceDetails_InvProd ID                                                                                                
            INNER JOIN #TempLookup_InvProd TL ON TL.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id                        
            INNER JOIN Accounting_LU_BillingTypes BT ON BT.BillingType_Id = TL.BillingType_Id AND BT.Status = 1                                                      
            WHERE ID.Invoice_Id = TBPD.Invoice_Id AND ID.Status = 1                                                                    
        ) AS DetailView                                                                    
 ) AS Details                                             LEFT JOIN #TempInvoiceFeesCTE_InvProd IFC on IFC.InvoiceId =TBPD.Invoice_Id                                                          
 ORDER BY FI.SortDate DESC                             
                         
                        
  DROP TABLE IF EXISTS #TempBasePolicyDetails_InvProd;                        
 DROP TABLE IF EXISTS #TempLookup_InvProd;                        
 DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails_InvProd;                        
 DROP TABLE IF EXISTS #PDL_Agg;                        
 DROP TABLE IF EXISTS #TotalFilteredInvoices;                   
 DROP TABLE IF EXISTS #FilteredInvoices;                        
 DROP TABLE IF EXISTS #TempInvoiceFeesCTE_InvProd;                        
 DROP TABLE IF EXISTS #TempTotalPremiumCTE_InvProd;              
 DROP TABLE IF EXISTS #TempAccounting_InsuredDetails_InvProd;                        
 DROP TABLE IF EXISTS #TempJournalEntries_InvProd;                        
 DROP TABLE IF EXISTS #TempLOB_InvProd;                        
DROP TABLE IF EXISTS #LatestCancelSubmissions;                          
END 


GO
