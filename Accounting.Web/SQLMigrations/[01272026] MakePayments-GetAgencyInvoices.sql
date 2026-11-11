CREATE PROCEDURE USP_Accounting_GetAgencyInvoices          
(                                                      
    @Client_Id BIGINT,                                              
    @AgentDetail_Id BIGINT=null,                              
    @Payee_Id BIGINT = NULL,                              
    @PayeeType_Id TINYINT = NULL,                              
    @EntityType_Id TINYINT = NULL,                                          
    @Entity_Id BIGINT = NULL,      
 @EntityName varchar(100)=null,      
    @AmountDueBasedOnType_Id TINYINT = NULL,                                  
    @AgencyName VARCHAR(200) = NULL,                                          
    @LineItem_Id TINYINT = NULL,                                          
    @PolicyNumber VARCHAR(100) = NULL,                                          
    @InsuredName VARCHAR(100) = NULL,                                          
    @InvoiceNo VARCHAR(100) = NULL,                                
    @OutstandingBalances   BIT = NULL                                
)                                                      
AS                                                      
BEGIN                                                      
    SET NOCOUNT ON;                                                 
                                                
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
       AND @PolicyNumber IS NULL                                                          
       AND @InsuredName IS NULL                                                
       AND @InvoiceNo IS NULL                                
    AND @OutstandingBalances IS NULL                                
    BEGIN                                                
        SET @isInitialLoad = 1;                                                
     END                                              
    ELSE                                              
    BEGIN                                              
        SET @sql += N';WITH ';                                              
    END               
             
    IF @isInitialLoad = 1                                                
    BEGIN                                                
        SET @sql += N'                                          
        ;WITH LimitedInvoices AS (                                          
            SELECT TOP (50) I.Id                                          
            FROM Accounting_Invoices I                                               
            INNER JOIN Accounting_PolicyDetails PD ON I.PolicyDetails_Id = PD.Id AND PD.Status = 1                                          
            WHERE I.Status = 1                                                
              AND PD.Client_Id = @Client_Id                                                
            ORDER BY I.Id DESC                                          
        ), ';                                          
    END                                                
                     
               
    SET @sql += N'                                        
 ARCollectedForPassThrough AS (                                       
 SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id,LIC.LineItem_Id as  LineItem_Id                          
        FROM Accounting_InvoiceDetails ID                                              
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                              
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                              
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                              
        WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 1 AND ID.Status = 1                                              
        GROUP BY ID.Id, INV.Id, LIC.LineItem_Id                                   
    ),                                              
    AgtCommissionPayableAmount AS (                                              
        SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalAgtCommission, INV.Id AS Invoice_Id                                              
        FROM Accounting_InvoiceDetails ID                                              
        LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                              
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                              
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                              
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                              
        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                              
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Code = ''AgencyBrokerCommission''                                              
    ),                                              
    TotalCommissionReceivableAmount AS (                                              
        SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalTotalCommission, INV.Id AS Invoice_Id                                              
        FROM Accounting_InvoiceDetails ID                                              
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                              
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                              
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Code = ''TotalCommission''                                              
    ),                
    ARCollectedForTotalCommission AS (                                              
        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id                                              
        FROM Accounting_InvoiceDetails ID                                              
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                              
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                            
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                              
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                              
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Code = ''TotalCommission''                                              
        GROUP BY ID.Id, INV.Id                                              
    )                                         
    SELECT                    
  I.Id As[Invoice_Id],              
  AID.Id as [InvoiceLineItem_Id],              
  LI.LineItem_Id As [LineItemId],                      
        I.InvoiceNo,                                                      
        PD.BillType,           
  PD.PolicyNumber,          
        LI.Name AS [LineTypeName],                                 
        AID.Amount AS [APAmount],             
  CASE                                               
       WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected                                              
       WHEN LI.Code = ''AgencyBrokerCommission'' THEN ((AGPA.OriginalAgtCommission / TCRA.OriginalTotalCommission) * ARTC.ArCollected)                                              
       END AS [AmountCollected],                
       AID.Amount-AID.RemainingBalance AS [AmountPaid],                 
    Net.NetDue AS [NetDue] ,            
    AID.RemainingBalance          
              
                                  
    FROM Accounting_Invoices I                                                      
    INNER JOIN Accounting_PolicyDetails PD ON I.PolicyDetails_Id = PD.Id AND PD.Status = 1                                                      
    INNER JOIN Accounting_AgentDetails AD ON PD.AgentDetail_Id = AD.AgentDetail_Id AND AD.Status = 1                                            
    INNER JOIN Accounting_InsuredDetails ID ON PD.InsuredDetail_Id = ID.InsuredDetail_Id AND ID.Status = 1                                            
    --INNER JOIN Accounting_InvoiceDetails AID ON I.Id = AID.Invoice_Id AND AID.Status=1       
 left JOIN Accounting_InvoiceDetails AID ON I.Id = AID.Invoice_Id AND AID.Status=1      
    INNER JOIN Accounting_LU_EntityTypes ET ON AID.EntityType_Id = ET.EntityType_Id AND ET.Status=1                                          
    INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON AID.BillingType_LineItems_Directions_Id=BLD.BillingType_LineItems_Directions_Id AND BLD.Status=1                                            
    INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                                        
    INNER JOIN Accounting_LU_Directions ALD ON ALD.Direction_Id = LD.Direction_Id AND ALD.Status=1                                        
    INNER JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1                                             
    INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                        
    INNER JOIN Accounting_LU_LineItemTypes LIT ON LI.LineItemType_Id = LIT.LineItemType_Id AND LIT.Status=1                  
    LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = I.Id  and ARPT.LineItem_Id = LIC.LineItem_Id                                            
    LEFT JOIN AgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = I.Id                           
    LEFT JOIN TotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = I.Id                                              
    LEFT JOIN ARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = I.Id                            
                             
 CROSS APPLY (                   
    SELECT                            
        CASE                             
            WHEN @AmountDueBasedOnType_Id = 1 THEN                             
                (CASE                                               
                    WHEN LIC.Is_Passthrough = 1 THEN ISNULL(ARPT.ArCollected, 0)                            
                    WHEN LI.Code = ''AgencyBrokerCommission''                             
                        THEN ISNULL(                            
                            (AGPA.OriginalAgtCommission / NULLIF(TCRA.OriginalTotalCommission, 0))                             
                            * ARTC.ArCollected,                             
                        0)                            
          END) - (AID.Amount - AID.RemainingBalance)                            
                            
            WHEN @AmountDueBasedOnType_Id = 2 THEN                             
                AID.RemainingBalance                            
        END                            
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
            I.DueDate AS DueDateLocal,                                                          
            LIT.Name AS LIT_Name,                                                          
            LI.LineItem_Id,                                                       
            LI.Code AS LI_Code,                             
            LI.Name AS LI_Name,                                                          
            LUD.Name AS LUD_Name,                                                     
   (ID.Amount - ID.RemainingBalance) AS TotalReceivable,                                                    
   CASE                                          
        WHEN LIT.Name = ''FEE''                                                          
                     AND LUD.Name = ''Accounts Receivable''                                                          
                     AND NOT EXISTS (                                                          
         SELECT 1                                                          
                        FROM Accounting_InvoiceDetails ID_AP                                                          
                        JOIN Accounting_LU_BillingType_LineItems_Directions BLD_AP                                                          
                          ON BLD_AP.BillingType_LineItems_Directions_Id = ID_AP.BillingType_LineItems_Directions_Id                                                          
                       AND BLD_AP.Status = 1                                                          
                        JOIN Accounting_LU_LineItems_Directions LD_AP                                                          
                          ON LD_AP.LineItems_Directions_Id = BLD_AP.LineItems_Directions_Id                                                          
              AND LD_AP.Status = 1                                                          
                        JOIN Accounting_LU_LineItems_Clients LIC_AP ON LIC_AP.LineItems_Clients_Id = LD_AP.LineItems_Clients_Id                                                          
                AND LIC_AP.Status = 1                                                          
                       JOIN Accounting_LU_LineItems LI_AP                                                          
                          ON LI_AP.LineItem_Id = LIC_AP.LineItem_Id                                                          
                          AND LI_AP.Status = 1                                                          
                        JOIN Accounting_LU_LineItemTypes LIT_AP                                                          
                          ON LIT_AP.LineItemType_Id = LI_AP.LineItemType_Id                                                          
                          AND LIT_AP.Status = 1                                                          
                        JOIN Accounting_LU_Directions LUD_AP               
                          ON LUD_AP.Direction_Id = LD_AP.Direction_Id                                                          
    AND LUD_AP.Status = 1                                                          
                        WHERE ID_AP.Invoice_Id = I.Id                                                          
                          AND ID_AP.Status = 1                                                          
                         AND LIT_AP.Name = ''FEE''                                                          
                          AND LUD_AP.Name = ''Accounts Payable''                                                          
                          AND LI_AP.LineItem_Id = LI.LineItem_Id                                                          
                     )        
                THEN 1 ELSE 0                                                           
    END AS IsHouseFee                                                          
        FROM Accounting_InvoiceDetails ID                                                          
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD                                                          
          ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id                                                          
          AND BLD.Status = 1                                                          
        JOIN Accounting_LU_LineItems_Directions LD                        
          ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id                                                          
          AND LD.Status = 1                                                          
        JOIN Accounting_LU_LineItems_Clients LIC                                                          
          ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id                                                          
          AND LIC.Status = 1                                                          
        JOIN Accounting_LU_LineItems LI                                                          
          ON LI.LineItem_Id = LIC.LineItem_Id                                                          
          AND LI.Status = 1                                                          
        JOIN Accounting_LU_LineItemTypes LIT                                                          
          ON LIT.LineItemType_Id = LI.LineItemType_Id                                                          
          AND LIT.Status = 1                        
        JOIN Accounting_LU_Directions LUD                                                          
          ON LUD.Direction_Id = LD.Direction_Id                                                          
          AND LUD.Status = 1                                                          
        JOIN Accounting_LU_BillingTypes BT                                                          
          ON BT.BillingType_Id = BLD.BillingType_Id                                                          
          AND BT.Status = 1                                                          
        WHERE ID.Invoice_Id = I.Id                                              
          AND ID.Status = 1                                                          
    ) AS DetailView                                                          
) AS Details     
    
    WHERE     
   (    
    (@PayeeType_Id = 7 AND PD.RiskState = @EntityName )    
    OR    
    (@PayeeType_Id <> 7  AND AID.Entity_Id = @Payee_Id)    
    )AND AD.Client_Id = @Client_Id AND AID.EntityType_Id = @PayeeType_Id AND I.Status = 1 AND ALD.Code = ''AP''    
    ';                                          
                         
                
    IF @EntityType_Id IS NOT NULL                                          
        SET @where += N' AND ET.EntityType_Id = @EntityType_Id';                                                
                        
    IF @Entity_Id IS NOT NULL                                                
        SET @where += N' AND AID.Entity_Id = @Entity_Id';                                              
                                          
    IF @AgencyName IS NOT NULL                                                
        SET @where += N' AND AD.Name LIKE ''%'' + @AgencyName + ''%''';                                           
                                          
    IF @LineItem_Id IS NOT NULL              
        SET @where += N' AND LI.LineItem_Id = @LineItem_Id';                                  
                                          
    IF @PolicyNumber IS NOT NULL                                                
        SET @where += N' AND PD.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';                                                       
                                          
    IF @InsuredName IS NOT NULL                                                
        SET @where += N' AND ID.Name LIKE ''%'' + @InsuredName + ''%''';                               
                                          
    IF @InvoiceNo IS NOT NULL                                 
        SET @where += N' AND I.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                                  
                                          
    IF @isInitialLoad = 1                                                
        SET @where += N' AND I.Id IN (SELECT Id FROM LimitedInvoices)';                                
                                  
 --IF @OutstandingBalances = 1                                      
 --       SET @where += N' AND AID.RemainingBalance <> 0';                             
 IF @OutstandingBalances = 1                            
    SET @where += N' AND ISNULL(Net.NetDue, 0) <> 0';                            
                            
                                                
    SET @sql += @where + N' ORDER BY AID.Entity_Id;';                                                
                                       
                     
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
        @PolicyNumber VARCHAR(100),                                          
        @InsuredName VARCHAR(100),                                          
        @InvoiceNo VARCHAR(100),                                
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
     @PolicyNumber = @PolicyNumber,                                                
        @InsuredName = @InsuredName,                                                
        @InvoiceNo = @InvoiceNo,                                
        @OutstandingBalances = @OutstandingBalances;                                
END 