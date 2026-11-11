alter PROCEDURE USP_Accounting_GetInvoicedAgency                                          
(                                              
    @Client_Id BIGINT,                                          
    @EntityType_Id TINYINT = NULL,                                 
    @Entity_Id BIGINT = NULL,                                
    @AmountDueBasedOnType_Id TINYINT = NULL,                                
    @AgencyName VARCHAR(200) = NULL,                                          
    @LineItem_Id TINYINT = NULL,                                          
    @PolicyNumber VARCHAR(100) = NULL,                                          
    @InsuredName VARCHAR(100) = NULL,                                          
    @InvoiceNo VARCHAR(100) = NULL,     
    @EntityName      VARCHAR(100) = NULL, -- Added By Rohith      
   @OutstandingBalances   BIT = NULL                              
)                                              
AS                                              
BEGIN                                              
    SET NOCOUNT ON;                                                 
                                                
    DECLARE @sql NVARCHAR(MAX) = N'';                                                
    DECLARE @where NVARCHAR(MAX) = N'';                                                
    DECLARE @params NVARCHAR(MAX);                                                
    DECLARE @isInitialLoad BIT = 0;               
             
    IF @EntityType_Id IS NULL                                                
       AND @Entity_Id IS NULL                                
    AND @AmountDueBasedOnType_Id IS NULL                                
       AND @AgencyName IS NULL                                          
       AND @LineItem_Id IS NULL                                          
       AND @PolicyNumber IS NULL                                                          
       AND @InsuredName IS NULL                                                
       AND @InvoiceNo IS NULL     
     AND @EntityName IS NULL -- Added     
    AND @OutstandingBalances IS NULL                              
    BEGIN                                                
        SET @isInitialLoad = 1;                                                
    END                             
             
    SET @sql = N';WITH ';               
             
    IF @isInitialLoad = 1                                    
    BEGIN                                    
        SET @sql += N'                                    
        LimitedInvoices AS (                                    
            SELECT TOP (50) AD.AgentDetail_Id                                    
            FROM Accounting_AgentDetails AD                                    
            INNER JOIN Accounting_PolicyDetails PD                                    
                ON AD.AgentDetail_Id = PD.AgentDetail_Id                                    
               AND PD.Status = 1                                    
            WHERE AD.Status = 1                                    
              AND PD.Client_Id = @Client_Id                                    
            ORDER BY AD.AgentDetail_Id DESC                                    
        ),';                                    
    END               
             
                  
    SET @sql += N'                                    
  ARCollectedForPassThrough AS (                                                
        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id, LIC.LineItem_Id                                    
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
        I.Id As[InvoiceId],                      
        LI.LineItem_Id As [LineItemId],                      
        AD.AgentDetail_Id,                                                       
        AD.Name AS AgencyName,                            
        ET.Name [EntityTypeName],                            
        AID.EntityType_Id,                    
        AID.Entity_Id,                
  CASE                                   
            WHEN AID.EntityType_Id = 1 THEN ag.Name                                  
            WHEN AID.EntityType_Id = 2 THEN ins.Name                                  
            WHEN AID.EntityType_Id = 3 THEN ca.Name                                  
            WHEN AID.EntityType_Id = 6 THEN wsd.Name                       
            --WHEN AID.EntityType_Id = 6 THEN wsd.Name                    
   WHEN AID.EntityType_Id = 7 THEN concat(''State of '',PD.RiskState)--added by navyatha
    WHEN AID.EntityType_Id = 11 THEN AOED.Name  
            ELSE NULL                                  
        END AS EntityName,        
  I.InvoiceNo,        
        CASE                           
    WHEN @AmountDueBasedOnType_Id = 1 THEN                           
        ROUND(            
            ISNULL(                          
                CASE                          
                    WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected                          
                    WHEN LI.Code = ''AgencyBrokerCommission'' THEN ((AGPA.OriginalAgtCommission / TCRA.OriginalTotalCommission) * ARTC.ArCollected)                          
                END, 0                          
            )                          
            - ISNULL(AID.Amount, 0) + ISNULL(AID.RemainingBalance, 0),             
            2            
        )                          
            
    WHEN @AmountDueBasedOnType_Id = 2 THEN                           
        ROUND(            
            ISNULL(AID.Amount, 0) - (ISNULL(AID.Amount, 0) - ISNULL(AID.RemainingBalance, 0)),             
            2            
        )            
END AS [NetDue]  
            
    FROM Accounting_AgentDetails AD                                              
    LEFT JOIN Accounting_PolicyDetails PD ON AD.AgentDetail_Id = PD.AgentDetail_Id AND PD.Status = 1                                      
    INNER JOIN Accounting_InsuredDetails ID ON PD.InsuredDetail_Id = ID.InsuredDetail_Id AND ID.Status = 1                                      
    INNER JOIN Accounting_Invoices I ON PD.Id = I.PolicyDetails_Id AND I.Status = 1                                            
    INNER JOIN Accounting_InvoiceDetails AID ON I.Id = AID.Invoice_Id AND AID.Status = 1                                          
    INNER JOIN Accounting_LU_EntityTypes ET ON AID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1                                          
    INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON AID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                            
    INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                                        
    INNER JOIN Accounting_LU_Directions ALD ON ALD.Direction_Id = LD.Direction_Id AND ALD.Status=1                                         
    INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                             
    INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                
    LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 1 AND ag.Status = 1                                  
    LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 3 AND ca.Status = 1                      
    LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 2 AND ins.Status = 1                                  
    LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 6 AND wsd.Status = 1                            
    LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = I.Id AND ARPT.LineItem_Id = LIC.LineItem_Id                                              
    LEFT JOIN AgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = I.Id                 
    LEFT JOIN TotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = I.Id                            
    LEFT JOIN ARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = I.Id
	 LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = AID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1  
    WHERE AD.Client_Id = @Client_Id                                    
      AND AD.Status = 1                                 
      AND ALD.Code = ''AP''      
         
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
                                    
    IF @InvoiceNo IS NOT NULL                  --Add                  
        SET @where += N' AND I.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';      
  IF @EntityName IS NOT NULL       
  BEGIN       
  IF @EntityType_Id = 1  SET @where += N' AND ag.Name LIKE ''%'' + @EntityName + ''%''';       
  ELSE IF @EntityType_Id = 2 SET @where += N' AND ins.Name LIKE ''%''+ @EntityName + ''%''';       
  ELSE IF @EntityType_Id = 3 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';       
  ELSE IF @EntityType_Id = 6 SET @where += N' AND wsd.Name LIKE ''%'' + @EntityName + ''%''';       
  ELSE IF @EntityType_Id = 7 SET @where += N' AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%''';
  ELSE IF @EntityType_Id = 11 SET @where += N' AND AOED.Name LIKE ''%'' + @EntityName + ''%'''; 
  END     
                                    
    IF @isInitialLoad = 1                                    
        SET @where += N' AND AD.AgentDetail_Id IN (SELECT AgentDetail_Id FROM LimitedInvoices)';                              
                                
 IF @OutstandingBalances = 1                
        SET @where += N' AND AID.RemainingBalance <> 0';                              
                                    
    SET @sql += @where + N' ORDER BY I.Id DESC;';                
            
    SET @params = N'                                    
        @Client_Id BIGINT,                                    
        @EntityType_Id TINYINT,      
        @Entity_Id BIGINT,                                
        @AmountDueBasedOnType_Id TINYINT,                                
        @AgencyName VARCHAR(200),                                    
        @LineItem_Id TINYINT,                                    
        @PolicyNumber VARCHAR(100),                                    
        @InsuredName VARCHAR(100),                                    
        @InvoiceNo VARCHAR(100),       
   @EntityName VARCHAR(100),      
  @OutstandingBalances BIT                              
    ';                                    
                                    
    EXEC sp_executesql                                    
        @sql,                                    
        @params,                                    
        @Client_Id = @Client_Id,                         
        @EntityType_Id = @EntityType_Id,                                    
        @Entity_Id = @Entity_Id,                                
        @AmountDueBasedOnType_Id =@AmountDueBasedOnType_Id,                                
        @AgencyName = @AgencyName,                                    
        @LineItem_Id = @LineItem_Id,                                    
        @PolicyNumber = @PolicyNumber,                                    
        @InsuredName = @InsuredName,                                    
        @InvoiceNo = @InvoiceNo,       
  @EntityName = @EntityName, -- Added    
        @OutstandingBalances = @OutstandingBalances;                              
END 