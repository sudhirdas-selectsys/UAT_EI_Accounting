CREATE PROCEDURE [dbo].[USP_Accounting_GetAgencyInvoices_V4]                                              
(      
    @Client_Id BIGINT = null,                                                                                              
    @AgentDetail_Id BIGINT=null,                                                                              
    @Payee_Id BIGINT = NULL,                                                                              
    @PayeeType_Id TINYINT = NULL,                                                                              
    @EntityType_Id TINYINT = NULL,                                                                                          
    @Entity_Id BIGINT = NULL,                                                      
    @EntityName varchar(100)=null,                                                      
    @AmountDueBasedOnType_Id TINYINT = NULL,                                                                                  
    @AgencyName VARCHAR(200) = NULL,                                                                                          
    @LineItem_Id TINYINT = NULL,                                      
    @LineItemIds  VARCHAR(MAX) = NULL,                                                                                          
    @PolicyNumber VARCHAR(100) = NULL,                                                                                          
    @InsuredName VARCHAR(100) = NULL,                                                                                          
    @InvoiceNo VARCHAR(100) = NULL,                                                                                
    @Direction_Id TINYINT = NULL,                                            
    @OutstandingBalances   BIT = NULL      
)                                                                                                      
AS                                                                                                     
BEGIN                                                                                                      
    SET NOCOUNT ON;                                    
                                     
IF @EntityType_Id = 8 AND (@PayeeType_Id = 3 OR @PayeeType_Id = 6)                              
BEGIN                                       
SET @EntityType_Id = @PayeeType_Id                                   
END                                 
ELSE IF @EntityType_Id = 9  AND @PayeeType_Id IN (3)                                  
BEGIN                                       
SET @EntityType_Id = @PayeeType_Id                                  
END                                
                                
                                  
  IF @Direction_Id IS NULL                                    
    SET @Direction_Id = 2                                        
                                        
                                  
                                                                                                
    DECLARE @sql NVARCHAR(MAX) = N'';                                                                                                
    DECLARE @where NVARCHAR(MAX) = N'';                                                                                                
    DECLARE @params NVARCHAR(MAX);                                                                                                
    DECLARE @isInitialLoad BIT = 0;                         
                                                                                          
                                                                     
    IF @Payee_Id IS NULL                                                                                
       AND @PayeeType_Id IS NULL                 
       AND @EntityType_Id IS NULL                                            
    AND @Entity_Id IS NULL                                                  
       AND @EntityName IS NULL                          
       AND @AmountDueBasedOnType_Id IS NULL                                   
       AND @AgencyName IS NULL                         
       AND @LineItem_Id IS NULL                                                                                          
       AND @LineItemIds IS NULL                                                                                          
       AND @PolicyNumber IS NULL                                                                                                          
       AND @InsuredName IS NULL                                                                                                
       AND @InvoiceNo IS NULL                                                                                
       AND @OutstandingBalances IS NULL                                                                                               
    BEGIN                           
        SET @isInitialLoad = 1;                                                                                                
    END                                                                                              
    ELSE                                
    BEGIN                                                                                         
        SET @sql += N'; ';                                                                                      
    END                  
       
  DROP TABLE IF EXISTS #TempLimitedInvoices;         
  DROP TABLE IF EXISTS #TempBasePolicyDetails;      
  DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails;      
  DROP TABLE IF EXISTS #TempBillingTypeLookUp;      
  DROP TABLE IF EXISTS #TempPaymentApplicationLineItems;      
  DROP TABLE IF EXISTS #TempARCollectedForPassThrough;         
  DROP TABLE IF EXISTS #TempTotalARCollected;      
  DROP TABLE IF EXISTS #TempAgtCommissionPayableAmount;      
  DROP TABLE IF EXISTS #TempTotalCommissionReceivableAmount;      
  DROP TABLE IF EXISTS #TempAPAmountSalesRepCommission;      
  DROP TABLE IF EXISTS #TempARCollectedForTotalHouseCommission;      
  DROP TABLE IF EXISTS #TempTotalARHouseCommissionInvoiced;      
  DROP TABLE IF EXISTS #TempARCollectedForTotalCommission;      
      
  -- #TempBasePolicyDetails Start                                           
 SELECT IES.PolicyDetails_Id,IES.InvoiceNo,IES.Id AS Invoice_Id,IES.Amount AS InvoicesAmount,IES.RemainingBalance AS InvoicesRemainingBalance,              
 IES.DueDate AS InvoicesDueDate,IES.CreatedOn AS InvoicesCreatedOn,              
 APD.Client_Id,APD.PolicyNumber,APD.AgentDetail_Id,APD.BillType,APD.CarrierDetail_Id,APD.InsuredDetail_Id,APD.EffectiveDate,      
 APD.ExpirationDate,APD.WholesalerDetail_Id,APD.TransactionType,APD.CancelDate,              
 APD.EndorsementNumber,APD.CreatedOn AS PolicyDetailsCreatedOn, APD.RiskState, APD.EndorsementEffectiveDate      
 INTO #TempBasePolicyDetails              
 FROM Accounting_PolicyDetails APD              
 INNER JOIN Accounting_Invoices IES ON APD.Id = IES.PolicyDetails_Id AND IES.Status = 1                      
 WHERE APD.Status = 1 AND APD.Client_Id = @Client_Id               
      
 CREATE NONCLUSTERED INDEX NC_TempBasePolicyDetails_ClientPolicy ON #TempBasePolicyDetails (Client_Id, PolicyDetails_Id)              
 INCLUDE (InvoiceNo, Invoice_Id, InvoicesAmount, InvoicesRemainingBalance, InvoicesDueDate);        
      
 -- #TempAccounting_InvoiceDetails Start                                           
 SELECT AID.* INTO #TempAccounting_InvoiceDetails              
 FROM Accounting_InvoiceDetails AID              
 INNER JOIN #TempBasePolicyDetails TBPD ON AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1              
              
 CREATE NONCLUSTERED INDEX NC_TempAccounting_InvoiceDetails ON #TempAccounting_InvoiceDetails (Id, Invoice_Id,BillingType_LineItems_Directions_Id)                 
 -- #TempAccounting_InvoiceDetails End        
      
  SELECT           
  BLD.BillingType_LineItems_Directions_Id,      
LIT.Code AS LIT_Code,              
    LIT.Name AS LIT_Name,    
 LI.LineItem_Id AS LI_LineItemId,    
    LI.Code  AS LI_Code,              
    LI.Name  AS LI_Name,      
 LUD.Code AS LUD_Code,              
    LUD.Name AS LUD_Name,              
    BT.Code  AS BT_Code,      
 LIC.Is_Passthrough,      
 LIC.LineItem_Id AS LineItem_Id,      
 LD.Direction_Id,      
 BLD.BillingType_Id AS BLD_BillingType_Id      
 INTO #TempBillingTypeLookUp              
 FROM Accounting_LU_BillingType_LineItems_Directions BLD               
 JOIN Accounting_LU_LineItems_Directions LD  ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1              
 JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1 AND LIC.Client_Id = @Client_Id              
 JOIN Accounting_LU_LineItems LI  ON LI.LineItem_Id  = LIC.LineItem_Id AND LI.Status = 1              
 JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1              
 JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1              
 JOIN Accounting_LU_BillingTypes BT  ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1      
 WHERE BLD.Status = 1      
                                                             
    IF @isInitialLoad = 1                                                                                                
    BEGIN                                                                                                
  SELECT distinct inv.Id          
        INTO #TempLimitedInvoices          
        FROM Accounting_Invoices inv          
        INNER JOIN Accounting_PolicyDetails pd ON inv.PolicyDetails_Id = pd.Id AND pd.Status = 1          
        WHERE inv.Status = 1 AND pd.Client_Id = @Client_Id;          
          
        CREATE NONCLUSTERED INDEX NC_TempLimitedInvoices_Invoice_Id ON #TempLimitedInvoices (Id);         
 END                                                                                                
                   
  SELECT APAL.InvoiceLineItem_Id, APAL.AppliedAmount       
  INTO #TempPaymentApplicationLineItems      
  FROM #TempBasePolicyDetails TBPD      
  INNER JOIN Accounting_PaymentApplications APA ON  TBPD.Invoice_Id = APA.Invoice_Id AND APA.Status = 1      
  INNER JOIN Accounting_PaymentApplicationLineItems APAL ON APA.PaymentApplication_Id = APAL.PaymentApplication_Id AND APAL.Status = 1      
         
  CREATE NONCLUSTERED INDEX NC_TempPaymentApplicationLineItems_InvoiceLineItem_Id ON #TempPaymentApplicationLineItems (InvoiceLineItem_Id);       
      
  SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, TBPD.Invoice_Id AS Invoice_Id,TBLU.LineItem_Id as  LineItem_Id       
  INTO #TempARCollectedForPassThrough      
        FROM #TempAccounting_InvoiceDetails ID                                                                                              
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                                                            
        INNER JOIN #TempPaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID                                                                                             
        INNER JOIN #TempBillingTypeLookUp TBLU ON ID.BillingType_LineItems_Directions_Id = TBLU.BillingType_LineItems_Directions_Id                                                                                                                           
  
    
                                          
        WHERE TBLU.Is_Passthrough = 1 AND TBLU.Direction_Id = 1 AND ID.Status = 1                                                       
        GROUP BY ID.Id, TBPD.Invoice_Id, TBLU.LineItem_Id       
      
  CREATE NONCLUSTERED INDEX NC_TempARCollectedForPassThrough_Invoice_Id_LineItem_Id ON #TempARCollectedForPassThrough (Invoice_Id, LineItem_Id);       
        
  SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, TBPD.Invoice_Id AS Invoice_Id,TBLU.LineItem_Id as  LineItem_Id        
  INTO #TempTotalARCollected      
  FROM #TempAccounting_InvoiceDetails ID                                          
  INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                                                                          
  INNER JOIN #TempPaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID                                                                                                         
  INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id      
        WHERE TBLU.Direction_Id = 2 AND ID.Status = 1                                                                                                          
        GROUP BY ID.Id, TBPD.Invoice_Id, TBLU.LineItem_Id       
      
  CREATE NONCLUSTERED INDEX NC_TempTotalARCollected_Invoice_Id_LineItem_Id ON #TempARCollectedForPassThrough (Invoice_Id, LineItem_Id);       
      
                                                                                  
        SELECT SUM(ID.Amount) AS OriginalAgtCommission, INV.Id AS Invoice_Id, ID.LOB_Id         
  INTO #TempAgtCommissionPayableAmount      
        FROM #TempAccounting_InvoiceDetails ID                                                               
        LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                                                  
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                                                    
  
   
       
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                    
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1              
        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                                      
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Code = 'AgencyBrokerCommission'                                        
  GROUP BY INV.Id, LIC.LineItem_Id, ID.LOB_Id        
      
  CREATE NONCLUSTERED INDEX NC_TempAgtCommissionPayableAmount_Invoice_Id_LineItem_Id ON #TempAgtCommissionPayableAmount (Invoice_Id, LOB_Id);      
        
      
  SELECT SUM(ID.Amount) AS OriginalTotalCommission, TBPD.Invoice_Id AS Invoice_Id, ID.LOB_Id      
  INTO #TempTotalCommissionReceivableAmount      
        FROM #TempAccounting_InvoiceDetails ID                                                     
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                                                             
  INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id      
        WHERE TBLU.Direction_Id = 1 AND ID.Status = 1 AND TBLU.LI_Code = 'TotalCommission'                                        
  GROUP BY ID.LOB_Id, TBPD.Invoice_Id                    
      
    CREATE NONCLUSTERED INDEX NC_TempTotalCommissionReceivableAmount_Invoice_Id_LineItem_Id ON #TempTotalCommissionReceivableAmount (Invoice_Id, LOB_Id);      
                
  SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalSalesRepCommission, TBPD.Invoice_Id AS Invoice_Id       
  INTO #TempAPAmountSalesRepCommission      
        FROM #TempAccounting_InvoiceDetails ID                                                                              
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                     
        INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id      
        WHERE TBLU.Direction_Id = 2 AND ID.Status = 1  AND TBLU.LI_Code = 'SalesRepCommission'      
                
  CREATE NONCLUSTERED INDEX NC_TempAPAmountSalesRepCommission_InvoiceLineItem_Id_LineItem_Id ON #TempAPAmountSalesRepCommission (InvoiceLineItem_Id,Invoice_Id);      
      
  SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, TBPD.Invoice_Id AS Invoice_Id      
  INTO #TempARCollectedForTotalHouseCommission      
 FROM #TempAccounting_InvoiceDetails ID                                                                              
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                       
        INNER JOIN #TempPaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID                                                                           
        INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id                                                                        
        WHERE TBLU.Direction_Id = 1 AND ID.Status = 1   AND TBLU.LUD_Code = 'HouseCommission'                                                                             
        GROUP BY ID.Id, TBPD.Invoice_Id        
            
 CREATE NONCLUSTERED INDEX NC_TempARCollectedForTotalHouseCommission_Invoice_Id_LineItem_Id ON #TempARCollectedForTotalHouseCommission(Id,Invoice_Id);      
        
        
  SELECT  SUM(ID.Amount) AS OriginalTotalHouseCommission, TBPD.Invoice_Id AS Invoice_Id, ID.LOB_Id       
     INTO #TempTotalARHouseCommissionInvoiced      
        FROM #TempAccounting_InvoiceDetails ID                                                                              
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                                            
        INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id                                                                                     
        WHERE TBLU.Direction_Id = 1 AND ID.Status = 1  AND TBLU.LI_Code = 'HouseCommission'                                         
  GROUP BY ID.LOB_Id, TBPD.Invoice_Id         
      
   CREATE NONCLUSTERED INDEX NC_TempTotalARHouseCommissionInvoiced_Invoice_Id_LOB_Id ON #TempTotalARHouseCommissionInvoiced(Invoice_Id,LOB_Id);      
      
 SELECT SUM(PALI.AppliedAmount) AS ArCollected, TBPD.Invoice_Id  AS Invoice_Id, ID.LOB_Id      
     INTO #TempARCollectedForTotalCommission      
        FROM #TempAccounting_InvoiceDetails ID                                                                                              
        INNER JOIN #TempBasePolicyDetails TBPD ON ID.Invoice_Id = TBPD.Invoice_Id                                                                                               
        INNER JOIN #TempPaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID                                                                                        
        INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id                                                                                     
        WHERE TBLU.Direction_Id = 1 AND ID.Status = 1 AND TBLU.LI_Code = 'TotalCommission'                                           
       GROUP BY TBPD.Invoice_Id , ID.LOB_Id        
      
   CREATE NONCLUSTERED INDEX NC_TempARCollectedForTotalCommission_Invoice_Id_LOB_Id ON #TempARCollectedForTotalCommission(Invoice_Id,LOB_Id);      
         
      
    SET @sql += N'       
    SELECT                                                                    
  TBPD.Invoice_Id As[Invoice_Id],                                                              
  AID.Id as [InvoiceLineItem_Id],                                
  TBLU.LineItem_Id As [LineItemId],                                                                      
        TBPD.InvoiceNo,                            
  ID.Name AS [InsuredName],                            
        TBPD.BillType,                                                           
  TBPD.PolicyNumber,                   
  CASE                  
    WHEN TBPD.TransactionType IN (''NEW'', ''REN'')                   
         THEN TBPD.EffectiveDate                  
    ELSE TBPD.EndorsementEffectiveDate                  
END AS TransactionEffectiveDate,                     
  TBPD.EffectiveDate,                    
  TBPD.TransactionType,                  
        CONCAT(LOB.CODE,'' - '',TBLU.LI_Name) AS [LineTypeName],                                                                                             
    CASE                                     
    WHEN @Direction_Id = 2                               
 THEN CONVERT(DECIMAL(18, 2), AID.Amount)                               
    ELSE CONVERT(DECIMAL(18, 2), 0)                                   
    END AS [APAmount],                                    
                                    
    CASE                              
    WHEN @Direction_Id = 1  AND  AID.Amount < 0                             
        THEN CONVERT(DECIMAL(18, 2), AID.Amount)                              
    ELSE CONVERT(DECIMAL(18, 2), 0)                              
END AS [ARAmount],                                    
                                        
       ISNULL(            
    CASE            
        WHEN TBLU.Is_Passthrough = 1             
            THEN ISNULL(ARPT.ArCollected, 0)            
            
        WHEN TBLU.LI_Code = ''AgencyBrokerCommission''            
            THEN ISNULL(            
                    (AGPA.OriginalAgtCommission             
                     / NULLIF(TCRA.OriginalTotalCommission, 0))            
                    * ISNULL(ARTC.ArCollected, 0),            
                    0            
                 )            
            
        WHEN TBLU.LI_Code = ''SalesRepCommission''             
             AND BT.Name = ''Direct Bill''            
            THEN ISNULL(            
                    (ISNULL(ARTC.ArCollected, 0)            
                     / NULLIF(TCRA.OriginalTotalCommission, 0))            
                    * ISNULL(APSRC.OriginalSalesRepCommission, 0),            
                    0            
                 )            
            
        WHEN TBLU.LI_Code = ''SalesRepCommission''             
             AND BT.Name = ''Agency Bill''            
            THEN ISNULL(            
                    (ISNULL(ARHC.ArCollected, 0)            
                     / NULLIF(ARHCI.OriginalTotalHouseCommission, 0))            
                    * ISNULL(APSRC.OriginalSalesRepCommission, 0),            
                    0            
                 )            
            
        ELSE ISNULL(TARC.ArCollected, 0)            
    END,            
    0            
) AS [AmountCollected],                                     
                                           
  CASE            
    WHEN @Direction_Id = 1 THEN            
        CAST(            
            (            
                CASE            
                    WHEN TBLU.Is_Passthrough = 1             
                        THEN ISNULL(ARPT.ArCollected, 0)            
            
                    WHEN TBLU.LI_Code = ''AgencyBrokerCommission''            
                        THEN ISNULL(            
                                (            
                                    AGPA.OriginalAgtCommission             
                                  / NULLIF(TCRA.OriginalTotalCommission, 0)            
                                ) * ISNULL(ARTC.ArCollected, 0),            
                                0            
                             )            
            
                    ELSE 0            
             END            
                - CAST(            
                      ISNULL(AID.Amount, 0.00)            
                     - (AID.Amount - AID.RemainingBalance)             
                  AS DECIMAL(18,2))            
            )            
        AS DECIMAL(18,2))            
    ELSE 0            
END AS [NetOutgoingPaymentDue],                                    
                                    
       AID.Amount-AID.RemainingBalance AS [AmountPaid],                                                        
    CAST(                                                      
    (PDLI.BrokerCommissionAmount / NULLIF(PDLI.Premium, 0)) * 100                                                      
    AS DECIMAL(18, 2)                                                      
) AS AgentCommissionByPremiumAmount,                                                      
                                                      
CAST(                                                      
    (PDLI.BrokerCommissionAmount / NULLIF(PDLI.CommissionAmount, 0)) * 100                                                      
    AS DECIMAL(18, 2)                 
) AS AgentCommissionByTotalCommission,                                                      
    CASE                                     
    WHEN @Direction_Id = 2 THEN Net.NetDue                                   
    ELSE NULL                                    
    END AS NetDue ,                                           
    AID.RemainingBalance,                                                            
 AID.Entity_Id                                                                        
                                   
    FROM #TempBasePolicyDetails TBPD                                                          
    --INNER JOIN Accounting_PolicyDetailsLineItems PDLI ON PDLI.PolicyDetails_Id=TBPD.PolicyDetails_Id AND PDLI.Status=1                                                        
    CROSS APPLY (SELECT SUM(BrokerCommissionAmount) AS BrokerCommissionAmount, SUM(Premium) AS Premium, SUM(CommissionAmount) AS CommissionAmount FROM Accounting_PolicyDetailsLineItems                                                        
    WHERE PolicyDetails_Id = TBPD.PolicyDetails_Id AND Status = 1) PDLI                                                        
    INNER JOIN Accounting_AgentDetails AD ON TBPD.AgentDetail_Id = AD.AgentDetail_Id AND AD.Status = 1                                                                                                        
    INNER JOIN Accounting_InsuredDetails ID ON TBPD.InsuredDetail_Id = ID.InsuredDetail_Id AND ID.Status = 1                                                                
 JOIN #TempAccounting_InvoiceDetails AID ON TBPD.Invoice_Id = AID.Invoice_Id AND AID.Status=1                                                     
 join Accounting_LU_LOBS LOB on LOB.LOB_Id = AID.LOB_Id and LOB.Status = 1                                                    
    INNER JOIN Accounting_LU_EntityTypes ET ON AID.EntityType_Id = ET.EntityType_Id AND ET.Status=1        
 INNER JOIN #TempBillingTypeLookUp TBLU ON TBLU.BillingType_LineItems_Directions_Id= AID.BillingType_LineItems_Directions_Id      
    LEFT JOIN #TempARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = TBPD.Invoice_Id  and ARPT.LineItem_Id = TBLU.LineItem_Id                                                                                                        
    --LEFT JOIN #TempAgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = TBPD.Invoice_Id      
    --LEFT JOIN #TempTotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = TBPD.Invoice_Id      
    --LEFT JOIN #TempARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = TBPD.Invoice_Id                   
                                                     
    LEFT JOIN #TempAgtCommissionPayableAmount AGPA  ON AGPA.Invoice_Id = TBPD.Invoice_Id and AID.LOB_ID = AGPA.LOB_Id             
    LEFT JOIN #TempTotalCommissionReceivableAmount TCRA  ON TCRA.Invoice_Id = TBPD.Invoice_Id  and AID.LOB_ID = TCRA.LOB_Id                             
 LEFT JOIN #TempAPAmountSalesRepCommission APSRC ON APSRC.Invoice_Id = TBPD.Invoice_Id      
 LEFT JOIN #TempARCollectedForTotalHouseCommission ARHC ON ARHC.Invoice_Id = TBPD.Invoice_Id                                                               
 LEFT JOIN #TempTotalARHouseCommissionInvoiced ARHCI ON ARHCI.Invoice_Id = TBPD.Invoice_Id and AID.LOB_ID = ARHCI.LOB_Id                                        
    LEFT JOIN #TempARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = TBPD.Invoice_Id and AID.LOB_ID = ARTC.LOB_Id                                    
    LEFT JOIN #TempTotalARCollected TARC on TARC.Invoice_Id = TBPD.Invoice_Id and TARC.LineItem_Id = TBLU.LineItem_Id                                    
 INNER JOIN Accounting_LU_BillingTypes BT ON BT.BillingType_Id = TBLU.BLD_BillingType_Id AND BT.Status = 1                                       
 CROSS APPLY (                                                                               
    SELECT                                                                                        
        CASE                              
            WHEN @AmountDueBasedOnType_Id = 1 THEN                                                                                         
                (CASE                                 
                    WHEN TBLU.Is_Passthrough = 1 THEN ISNULL(ARPT.ArCollected, 0)                                                                        
                    WHEN TBLU.LI_Code = ''AgencyBrokerCommission'' THEN ISNULL((AGPA.OriginalAgtCommission / NULLIF(TCRA.OriginalTotalCommission, 0)) * ARTC.ArCollected, 0)                                        
     WHEN TBLU.LI_Code = ''SalesRepCommission'' AND BT.Name = ''Direct Bill'' THEN ((ARTC.ArCollected / NULLIF(TCRA.OriginalTotalCommission, 0)) * APSRC.OriginalSalesRepCommission)                                                                 
                    WHEN TBLU.LI_Code = ''SalesRepCommission'' AND BT.Name = ''Agency Bill'' THEN ((ARHC.ArCollected / NULLIF(ARHCI.OriginalTotalHouseCommission, 0)) * APSRC.OriginalSalesRepCommission)                                                     
  
     
                                      
          END) - (AID.Amount - AID.RemainingBalance)                                                                                        
                                                                                        
   WHEN @AmountDueBasedOnType_Id = 2 THEN                                                                                         
             --   AID.RemainingBalance                                                          
      ISNULL(AID.Amount, 0) - ISNULL((AID.Amount - AID.RemainingBalance), 0)                             
        END AS NetDue                                                        
) Net (NetDue)                                                        
                                                                                      
                                                                                        
    OUTER APPLY (                                         
    SELECT                                                                                                                      
        CAST(SUM(CASE WHEN LIT_Name = ''PREMIUM'' AND LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,                                                                                                            
  
    
     
        
           
        CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalFees,                                                                                                
        CAST(SUM(CASE WHEN IsHouseFee = 1 THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseFee,                                                                                                                      
        CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPayableFee,                                                     
      
        CAST(SUM(CASE WHEN LIT_Name = ''FEE'' AND LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCarrierFees,                                                                                              
        CAST(SUM(CASE WHEN LIT_Name = ''TAX'' AND LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalTaxes,                                                                                                                 
  
     
      CAST(SUM(CASE WHEN LIT_Name = ''Commission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,                                                          
CAST(SUM(CASE WHEN LI_Code = ''MGACommission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,                                                                                                                     
        CAST(SUM(CASE WHEN LI_Code = ''TotalCommissionPercentage'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,                                                                                                                     
        CAST(SUM(CASE WHEN LI_Name = ''Agency/Broker Commission'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission,                                                                             
      CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalARInvoiced,                                                                                                                      
        CAST(                                                                                
             (SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' THEN Amount ELSE 0 END)                                                        
              - SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' THEN RemainingBalance ELSE 0 END))                                                                                                                      
          AS DECIMAL(18,2)                                                                                                                      
         ) AS ARAmountPaid,                                                                              
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARBalanceDue,                                                              
      CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAPInvoiced,                                                                 
        CAST(SUM(CASE WHEN LI_Name = ''Total Commission'' AND LUD_Name = ''Accounts Receivable'' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,                                      
                                                             
        CAST(                                                                                                                      
             (SUM(CASE WHEN LUD_Name = ''Accounts Payable'' THEN Amount ELSE 0 END)                                                   
              - SUM(CASE WHEN LUD_Name = ''Accounts Payable'' THEN RemainingBalance ELSE 0 END))                                                            
             AS DECIMAL(18,2)                                                                                                
        ) AS APAmountPaid,                
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APBalanceDue,                                                                             
                                      
        -- AR Aging Buckets                                      
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS ARCurrent,                                                                               
  
     
      
       
          
                    
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR1_30,                                                 
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR31_60,                         
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AR61_90,                                                     
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Receivable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AROver90,                                                                                 
  
   
       
        
          
            
                                                                                                     
        -- AP Aging Buckets                                                                     
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) <= 0 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APCurrent,                                                                                   
  
    
      
        
          
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 1 AND 30 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP1_30,                                                                          
  
    
      
        
          
                  
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 31 AND 60 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP31_60,                                                                        
  
    
      
        
          
                   
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) BETWEEN 61 AND 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS AP61_90,                                                                        
  
    
      
        
          
               
        CAST(SUM(CASE WHEN LUD_Name = ''Accounts Payable'' AND DATEDIFF(DAY, DueDateLocal, GETDATE()) > 90 THEN RemainingBalance ELSE 0 END) AS DECIMAL(18,2)) AS APOver90                                            
                                                                                    
    FROM (                                                                                     
        SELECT                                                                                                          
            ID.Amount,                                                                                                          
            ID.RemainingBalance,                                                       
            TBPD.InvoicesDueDate AS DueDateLocal,                              
            TBLU_2.LIT_Name AS LIT_Name,                                                                                                          
            TBLU_2.LineItem_Id,                                                                                   
            TBLU_2.LI_Code AS LI_Code,                                                                             
            TBLU_2.LI_Name AS LI_Name,                             
            TBLU_2.LUD_Name AS LUD_Name,                                                                                 
   (ID.Amount - ID.RemainingBalance) AS TotalReceivable,                                                                                                    
   CASE                                                                                          
        WHEN TBLU_2.LIT_Name = ''FEE''                                                                                                          
                     AND TBLU_2.LUD_Name = ''Accounts Receivable''                                                  
                     AND NOT EXISTS (                                                                                                          
         SELECT 1                                                                                                          
                        FROM #TempAccounting_InvoiceDetails ID_AP                                                                                                
                        INNER JOIN #TempBillingTypeLookUp TBLU_AP ON TBLU_AP.BillingType_LineItems_Directions_Id= ID_AP.BillingType_LineItems_Directions_Id                                                                                                   
   
   
       
                        WHERE ID_AP.Invoice_Id = TBPD.Invoice_Id                                                                                              
                          AND ID_AP.Status = 1                                                                                                          
                         AND TBLU_AP.LIT_Name = ''FEE''                                                                                                          
                          AND TBLU_AP.LUD_Name = ''Accounts Payable''                                                                                                          
                          AND TBLU_AP.LineItem_Id = TBLU_2.LineItem_Id                                                                                                      
        )                                                        
                THEN 1 ELSE 0                                                                                                           
    END AS IsHouseFee                                                                                                          
        FROM #TempAccounting_InvoiceDetails ID                    
  INNER JOIN #TempBillingTypeLookUp TBLU_2 ON TBLU_2.BillingType_LineItems_Directions_Id= ID.BillingType_LineItems_Directions_Id                                                                                                  
        WHERE ID.Invoice_Id = TBPD.Invoice_Id                                                                                              
          AND ID.Status = 1                      
    ) AS DetailView                                                                                                          
) AS Details                                                     
                                                    
    WHERE                                                     
   (                                                    
    (@PayeeType_Id = 7 AND TBPD.RiskState = @EntityName )                                                    
    OR                                                    
  (@PayeeType_Id <> 7  AND AID.Entity_Id = @Payee_Id)                                                    
    )AND AD.Client_Id = @Client_Id AND AID.EntityType_Id = @PayeeType_Id AND                                     
    (     
        (@Direction_Id = 1 AND TBLU.LUD_Code = ''AR'')                                    
     OR (@Direction_Id = 2 AND TBLU.LUD_Code = ''AP'')                                    
    )               
 AND (              
        @Direction_Id <> 1               
     OR (AID.Amount < 0)              
    )              
    ';                                                                                                            
                                                          
                                                                 
                                        
    IF @EntityType_Id IS NOT NULL                                                                                          
        SET @where += N' AND ET.EntityType_Id = @EntityType_Id';                               
    IF @Entity_Id IS NOT NULL                                                                                                
        SET @where += N' AND AID.Entity_Id = @Entity_Id';                                                                                              
                          
    IF @AgencyName IS NOT NULL                                                                                                
        SET @where += N' AND AD.Name LIKE ''%'' + @AgencyName + ''%''';                                           
                                                        
    IF @LineItem_Id IS NOT NULL                                                              
        SET @where += N' AND LI.LineItem_Id = @LineItem_Id';                                      
                                      
 --IF @LineItemIds IS NOT NULL                                                              
 --       SET @where += N' AND LI.LineItem_Id IN (SELECT value FROM STRING_SPLIT(@LineItemIds, '',''))';       
 IF @LineItemIds IS NOT NULL                                                              
        SET @where += N' AND TBLU.LI_LineItemId IN (SELECT value FROM STRING_SPLIT(@LineItemIds, '',''))';         
                                                                                          
    IF @PolicyNumber IS NOT NULL                                                                                               
        SET @where += N' AND TBPD.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';                                                                                                       
                                                                                          
    IF @InsuredName IS NOT NULL                                                                                                
        SET @where += N' AND ID.Name LIKE ''%'' + @InsuredName + ''%''';                                                                               
                                                                                          
    IF @InvoiceNo IS NOT NULL                                      
       SET @where += N' AND TBPD.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                                                                                   
        
 IF @isInitialLoad = 1          
    BEGIN          
  SET @where += N' AND TBPD.Invoice_Id IN (SELECT Id FROM #TempLimitedInvoices)';          
 END          
                                                                                  
--IF @OutstandingBalances = 1 AND @Direction_Id = 1            
--    SET @where += N' AND NetOutgoingPaymentDue <> 0';            
--ELSE IF @OutstandingBalances = 1 AND @Direction_Id = 2            
--    SET @where += N' AND NetDue <> 0';            
--ELSE IF ISNULL(@OutstandingBalances,0) = 0 AND @Direction_Id = 1            
--    SET @where += N' AND NetOutgoingPaymentDue = 0';            
--ELSE IF ISNULL(@OutstandingBalances,0) = 0 AND @Direction_Id = 2            
--    SET @where += N' AND NetDue = 0';            
            
              
           
  IF @OutstandingBalances = 1                                      
        SET @where += N' AND AID.RemainingBalance <> 0';             
                                                                                                  
  --  SET @sql += @where + N' ORDER BY AID.Entity_Id;';     
SET @sql += @where + N' ORDER BY TBPD.InvoiceNo DESC';      
  
    SET @params = N'                                                                                          
        @Client_Id BIGINT,                                                                                              
        @AgentDetail_Id BIGINT,                                                                              
        @Payee_Id BIGINT,                                                                  
        @PayeeType_Id TINYINT ,                                                                              
        @EntityType_Id TINYINT,                                            
        @Entity_Id BIGINT,                                                      
        @EntityName varchar(100),                                                      
        @AmountDueBasedOnType_Id TINYINT,                                                         
        @AgencyName VARCHAR(200),                                                                                          
      @LineItem_Id TINYINT,                                      
        @LineItemIds VARCHAR(MAX),                                      
        @PolicyNumber VARCHAR(100),                                
        @InsuredName VARCHAR(100),                                                                                          
        @InvoiceNo VARCHAR(100),                                                                                        
        @Direction_Id TINYINT,                                                   
        @OutstandingBalances BIT                                                                                            
    ';                                                           
                                                                                              
    EXEC sp_executesql                                                    
        @sql,                                                                                                
        @params,                                                                                          
        @Client_Id = @Client_Id,                                                                                          
        @AgentDetail_Id = @AgentDetail_Id,                                                                
        @Payee_Id  = @Payee_Id,                                                                              
        @PayeeType_Id  = @PayeeType_Id,                                                                              
        @EntityType_Id = @EntityType_Id,                                                     
        @Entity_Id = @Entity_Id,                                                      
        @EntityName =@EntityName,                                                      
        @AmountDueBasedOnType_Id =@AmountDueBasedOnType_Id,                                                                                  
        @AgencyName = @AgencyName,                                                                                                
        @LineItem_Id = @LineItem_Id,                                 
        @LineItemIds = @LineItemIds,                                      
        @PolicyNumber = @PolicyNumber,                                                                                                
        @InsuredName = @InsuredName,                                                          
        @InvoiceNo = @InvoiceNo,                                                                                
        @Direction_Id = @Direction_Id,                                    
        @OutstandingBalances = @OutstandingBalances;         
        
 DROP TABLE IF EXISTS #TempLimitedInvoices;         
  DROP TABLE IF EXISTS #TempBasePolicyDetails;      
  DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails;      
  DROP TABLE IF EXISTS #TempBillingTypeLookUp;      
  DROP TABLE IF EXISTS #TempPaymentApplicationLineItems;      
  DROP TABLE IF EXISTS #TempARCollectedForPassThrough;         
  DROP TABLE IF EXISTS #TempTotalARCollected;      
  DROP TABLE IF EXISTS #TempAgtCommissionPayableAmount;      
  DROP TABLE IF EXISTS #TempTotalCommissionReceivableAmount;      
  DROP TABLE IF EXISTS #TempAPAmountSalesRepCommission;      
  DROP TABLE IF EXISTS #TempARCollectedForTotalHouseCommission;      
  DROP TABLE IF EXISTS #TempTotalARHouseCommissionInvoiced;      
  DROP TABLE IF EXISTS #TempARCollectedForTotalCommission;      
END 