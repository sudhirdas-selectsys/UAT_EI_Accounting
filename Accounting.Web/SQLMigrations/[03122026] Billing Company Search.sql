CREATE    PROCEDURE USP_Accounting_GetInvoicedAgency_V1                                        
(                                                                     
    @Client_Id BIGINT,                                                               
    @EntityType_Id TINYINT = NULL,                                                         
    @Entity_Id BIGINT = NULL,                                                        
    @AmountDueBasedOnType_Id TINYINT = NULL,                                                        
    @AgencyName VARCHAR(200) = NULL,                                                                
    @LineItem_Id TINYINT = NULL,                      
    @LineItemIds  VARCHAR(MAX) = NULL,                          
    @PolicyNumber VARCHAR(100) = NULL,                                                                
    @InsuredName VARCHAR(100) = NULL,                                                                
    @InvoiceNo VARCHAR(100) = NULL,                            
    @EntityName      VARCHAR(100) = NULL,               
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
       AND @LineItemIds IS NULL                     
       AND @PolicyNumber IS NULL                                                                                
       AND @InsuredName IS NULL                                                                        
       AND @InvoiceNo IS NULL                            
       AND @EntityName IS NULL               
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
        SELECT SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id, LIC.LineItem_Id, ID.LOB_Id                                                            
        FROM Accounting_InvoiceDetails ID                                        
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                 
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                            
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                            
        WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 1 AND ID.Status = 1                                                            
        GROUP BY ID.Id, INV.Id, LIC.LineItem_Id,ID.LOB_Id                                                            
),                                      
    AgtCommissionPayableAmount AS (                                                            
        SELECT ID.Amount AS OriginalAgtCommission, INV.Id AS Invoice_Id,ID.LOB_Id                                                            
        FROM Accounting_InvoiceDetails ID                                                            
        LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                            
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                            
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                            
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                            
        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                            
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Code = ''AgencyBrokerCommission''                                                            
    ),                                            
    TotalCommissionReceivableAmount AS (                                            
        SELECT SUM(ID.Amount) AS OriginalTotalCommission, INV.Id AS Invoice_Id,ID.LOB_Id                                                            
        FROM Accounting_InvoiceDetails ID                                                            
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                            
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                       
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                            
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Code = ''TotalCommission''      
  group by ID.LOB_Id,INV.Id    
    ),                                            
    ARCollectedForTotalCommission AS (                                                                
        SELECT SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id,ID.LOB_Id                                                            
        FROM Accounting_InvoiceDetails ID                                     
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                            
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                            
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                            
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                              
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Code = ''TotalCommission''                                                            
        GROUP BY INV.Id,ID.LOB_Id                                  
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
     WHEN AID.EntityType_Id = 7 THEN concat(''State of '',PD.RiskState)                  
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
    LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = I.Id AND ARPT.LineItem_Id = LIC.LineItem_Id and ARPT.LOB_Id = AID.LOB_Id                                                                                              
    LEFT JOIN AgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = I.Id and AGPA.LOB_Id = AID.LOB_Id                                       
    LEFT JOIN TotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = I.Id and TCRA.LOB_Id = AID.LOB_Id                                                   
    LEFT JOIN ARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = I.Id  and ARTC.LOB_Id = AID.LOB_Id                        
    LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = AID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1                         
    WHERE AD.Client_Id = @Client_Id                                                            
      AND AD.Status = 1                                                                         
      AND ALD.Code = ''AP''                             
    ';                                                            
                                    
    IF @EntityType_Id IS NOT NULL          
 BEGIN        
  IF @EntityType_Id = 8        
   SET @where += N' AND AID.EntityType_Id IN (3, 6)';        
  ELSE IF @EntityType_Id = 9        
   SET @where += N' AND AID.EntityType_Id = 3';        
  ELSE        
   SET @where += N' AND AID.EntityType_Id = @EntityType_Id';        
 END           
                                                          
    IF @Entity_Id IS NOT NULL                              
        SET @where += N' AND AID.Entity_Id = @Entity_Id';                                                                        
                                                      
    IF @AgencyName IS NOT NULL                                                                        
        SET @where += N' AND AD.Name LIKE ''%'' + @AgencyName + ''%''';                                                                                
                                                          
    IF @LineItem_Id IS NOT NULL                                                                        
        SET @where += N' AND LI.LineItem_Id = @LineItem_Id';                      
            
    IF @LineItemIds IS NOT NULL                                                                        
        SET @where += N' AND LI.LineItem_Id IN (SELECT value FROM STRING_SPLIT(@LineItemIds, '',''))';                    
                      
    IF @PolicyNumber IS NOT NULL                                                                        
        SET @where += N' AND PD.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';                                                                                              
                                                    
    IF @InsuredName IS NOT NULL                                                                        
        SET @where += N' AND ID.Name LIKE ''%'' + @InsuredName + ''%''';                                                                        
                                                          
    IF @InvoiceNo IS NOT NULL                             
        SET @where += N' AND I.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                              
              
    IF @EntityName IS NOT NULL                              
    BEGIN                              
        IF @EntityType_Id = 1  SET @where += N' AND ag.Name LIKE ''%'' + @EntityName + ''%''';                              
        ELSE IF @EntityType_Id = 2 SET @where += N' AND ins.Name LIKE ''%''+ @EntityName + ''%''';       
        ELSE IF @EntityType_Id = 3 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';                              
        ELSE IF @EntityType_Id = 6 SET @where += N' AND wsd.Name LIKE ''%'' + @EntityName + ''%''';                              
        ELSE IF @EntityType_Id = 7 SET @where += N' AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%''';                
        ELSE IF @EntityType_Id = 11 SET @where += N' AND AOED.Name LIKE ''%'' + @EntityName + ''%''';           
  ELSE IF @EntityType_Id = 8 SET @where += N' AND (wsd.Name LIKE ''%'' + @EntityName + ''%'' OR ca.Name LIKE ''%'' + @EntityName + ''%'')';         ELSE IF @EntityType_Id = 9 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';      
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
        @LineItemIds VARCHAR(MAX),                    
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
        @AmountDueBasedOnType_Id = @AmountDueBasedOnType_Id,                                                        
        @AgencyName = @AgencyName,                                                            
        @LineItem_Id = @LineItem_Id,                                                          
        @LineItemIds = @LineItemIds,                                                            
        @PolicyNumber = @PolicyNumber,                                                            
        @InsuredName = @InsuredName,                                                            
        @InvoiceNo = @InvoiceNo,                              
        @EntityName = @EntityName,                          
        @OutstandingBalances = @OutstandingBalances;                            
END
go
CREATE    PROCEDURE [dbo].[USP_Accounting_GetARSearchResults_V4]                                            
(                                            
    @EntityType_Id TINYINT = NULL,                                            
    @Entity_Id BIGINT = NULL,                                
    @Entity_Name VARCHAR(100)=NULL,                 
    @LineItemIds VARCHAR(MAX) = NULL,           
 @LineItem_Id TINYINT = NULL,          
    @PolicyNumber VARCHAR(100) = NULL,                                            
    @AgencyName VARCHAR(200) = NULL,                                            
    @InsuredName VARCHAR(100) = NULL,                                            
    @OutstandingBalances BIT = NULL,                                            
    @InvoiceNo VARCHAR(100) = NULL,                                            
    @Client_Id BIGINT = NULL                                            
)                                            
AS                                            
BEGIN                                            
    SET NOCOUNT ON;                     
                                            
    DECLARE @sql NVARCHAR(MAX) = N'';                                             
    DECLARE @params NVARCHAR(MAX) = N'';                                            
                                         
                                        
 DECLARE @condition BIT = CASE                                         
        WHEN @EntityType_Id IS NULL                              
        AND @Entity_Name IS NULL                              
         AND @Entity_Id IS NULL                       
        AND @LineItemIds IS NULL                                         
         AND @PolicyNumber IS NULL                                         
         AND @AgencyName IS NULL                                         
         AND @InsuredName IS NULL                                         
         AND @InvoiceNo IS NULL                                         
        THEN '1' ELSE '0' END;                                        
                                            
    -- Check if this is "initial load" (only @Client_Id and optionally @OutstandingBalances)                                            
      SET @sql = N'                                        
    ;WITH LimitedInvoices AS                                        
    (                                        
        SELECT TOP (25) inv.Id                                        
        FROM Accounting_Invoices inv                                        
        INNER JOIN Accounting_PolicyDetails pd                                        
            ON inv.PolicyDetails_Id = pd.Id                                        
           AND pd.Status = 1                                        
        WHERE inv.Status = 1                                        
          AND pd.Client_Id = @Client_Id                                     
    ORDER BY inv.Id DESC                                  
                                            
    )                                            
                                                    
        SELECT id.Id,id.Invoice_Id,id.Amount,id.RemainingBalance,id.Entity_Id,id.EntityType_Id,id.BillingType_LineItems_Directions_Id,id.PaidOn,id.Paid,id.LOB_Id,id.Status,id.CreatedOn,id.UpdatedOn,                                
               CASE id.EntityType_Id                                           
              WHEN 1 THEN ag.Name                                            
                   WHEN 2 THEN ins.Name                                            
                   WHEN 3 THEN ca.Name                        
       WHEN 4 THEN cl.CompanyName                        
                   WHEN 6 THEN wsd.Name                                 
       WHEN 7 THEN concat(''State of '',Pd.RiskState)                        
    WHEN  10 THEN srd.Name                          
    WHEN 11 THEN AOED.Name                          
       ELSE NULL                                            
               END AS EntityName,                             
               et.Name AS EntityTypeName,                                            
               ISNULL(pps.DueDate, inv.CreatedOn) AS InvoiceDueDate,                                        
     LIM.Name AS LineItemName                             
  ,Lic.Is_Commissionable                            
        FROM Accounting_InvoiceDetails id                                            
        INNER JOIN Accounting_Invoices inv                                            
            ON id.Invoice_Id = inv.Id                                            
            AND inv.Status = 1                                            
        INNER JOIN Accounting_PolicyDetails pd                                            
            ON inv.PolicyDetails_Id = pd.Id                                            
            AND pd.Status = 1                                              
        INNER JOIN Accounting_AgentDetails agd                                            
            ON agd.AgentDetail_Id = pd.AgentDetail_Id                                            
            AND agd.Status = 1                                             
        INNER JOIN Accounting_InsuredDetails isd                                            
            ON isd.InsuredDetail_Id = pd.InsuredDetail_Id                                            
            AND isd.Status = 1                                              
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions bld                                     
            ON id.BillingType_LineItems_Directions_Id = bld.BillingType_LineItems_Directions_Id                                            
            AND bld.Status = 1                                            
        INNER JOIN Accounting_LU_LineItems_Directions ld                                            
            ON bld.LineItems_Directions_Id = ld.LineItems_Directions_Id                                            
            AND ld.Status = 1                                            
        INNER JOIN Accounting_LU_LineItems_Clients lic                                            
            ON lic.LineItems_Clients_Id = ld.LineItems_Clients_Id                             
   AND lic.Status = 1                                            
  INNER JOIN Accounting_LU_LineItems LIM                                         
   ON LIM.LineItem_Id = Lic.LineItem_Id                                        
   AND LIM.Status = 1                                        
        INNER JOIN Accounting_LU_EntityTypes et                                            
            ON id.EntityType_Id = et.EntityType_Id                                            
            AND et.Status = 1                                            
                                                  
        LEFT JOIN Accounting_PolicyPaymentSchedule pps                                            
            ON inv.Schedule_Id = pps.PolicyPaymentSchedule_Id                                         
        LEFT JOIN Accounting_AgentDetails ag                                            
            ON ag.AgentDetail_Id = id.Entity_Id                                            
            AND id.EntityType_Id = 1                                            
            AND ag.Status = 1                                            
        LEFT JOIN Accounting_CarrierDetails ca                                            
            ON ca.CarrierDetail_Id = id.Entity_Id                                            
            AND id.EntityType_Id = 3                 
            AND ca.Status = 1                        
   LEFT JOIN Accounting_Clients cl ON cl.Client_Id = pd.Client_Id AND cl.Status = 1                        
        LEFT JOIN Accounting_InsuredDetails ins                                            
            ON ins.InsuredDetail_Id = id.Entity_Id                                         
            AND id.EntityType_Id = 2                    
            AND ins.Status = 1                                            
        LEFT JOIN Accounting_WholesalerDetails wsd                                            
            ON wsd.WholesalerDetail_Id = id.Entity_Id                                            
            AND id.EntityType_Id = 6                                            
      AND wsd.Status = 1                        
   LEFT JOIN Accounting_SalesRepDetails srd ON srd.Id = id.Entity_Id AND id.EntityType_Id = 10 AND srd.Status = 1                        
  LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = ID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1                           
   '                          
                             
   SET @sql +=  N' '+ CASE WHEN @condition = 1 THEN ' INNER JOIN LimitedInvoices li ON li.Id = inv.Id  ' ELSE '' END + ''                                  
   set @sql +=N' ' +                                  
        'WHERE id.Status = 1                                           
          AND pd.Client_Id = @Client_Id                                            
          AND ld.Direction_Id = 1   '                                        
                                        
                                            
        -- Add dynamic filters only if parameters are provided                                            
       IF @EntityType_Id IS NOT NULL         
    BEGIN           
    IF @EntityType_Id = 8           
    BEGIN            
    SET @sql += N' AND id.EntityType_Id IN (6,3)';      
    END           
    ELSE IF @EntityType_Id = 9           
    BEGIN            
    SET @sql += N' AND id.EntityType_Id = 3';           
    END           
    ELSE           
    BEGIN             
    SET @sql += N' AND id.EntityType_Id = @EntityType_Id';           
    END          
    END                                              
        IF @Entity_Id IS NOT NULL                                            
            SET @sql += ' AND id.Entity_Id = @Entity_Id';                               
    IF @Entity_Name IS NOT NULL                               
  BEGIN                                
   IF @EntityType_Id = 1                                
    SET @sql += ' AND ag.Name LIKE ''%' + @Entity_Name + '%''';                                
   ELSE IF @EntityType_Id = 2                 
    SET @sql += ' AND ins.Name LIKE ''%' + @Entity_Name + '%''';                                
   ELSE IF @EntityType_Id = 3                 
    SET @sql += ' AND ca.Name LIKE ''%' + @Entity_Name + '%''';                         
   ELSE IF @EntityType_Id = 4                 
    SET @sql += ' AND cl.CompanyName LIKE ''%' + @Entity_Name + '%''';                         
   ELSE IF @EntityType_Id = 6                 
    SET @sql += ' AND wsd.Name LIKE ''%' + @Entity_Name + '%''';                                
   ELSE IF @EntityType_Id = 7                 
    SET @sql += ' AND Pd.RiskState LIKE ''%' + @Entity_Name + '%''';      
 ELSE IF @EntityType_Id = 8         SET @sql += ' AND (wsd.Name LIKE ''%' + @Entity_Name + '%'' OR ca.Name LIKE ''%' + @Entity_Name + '%'')';     ELSE IF @EntityType_Id = 9         SET @sql += ' AND ca.Name LIKE ''%' + @Entity_Name + '%''';      
    ELSE IF @EntityType_Id = 11                 
    SET @sql += ' AND AOED.Name LIKE ''%' + @Entity_Name + '%''';                            
  END              
  IF @LineItemIds IS NOT NULL                                                      
   SET @sql += ' AND Lic.LineItem_Id IN (SELECT value FROM STRING_SPLIT(@LineItemIds, '',''))';                    
        IF @PolicyNumber IS NOT NULL                      
        SET @sql += '                      
        AND REPLACE(REPLACE(REPLACE(REPLACE(PD.PolicyNumber,'' '',''''),''-'',''''),''"'',''''),''.'','''')                      
        =                      
        REPLACE(REPLACE(REPLACE(REPLACE(@PolicyNumber,'' '',''''),''-'',''''),''"'',''''),''.'','''')';                      
        IF @AgencyName IS NOT NULL                                            
            SET @sql += ' AND agd.Name LIKE ''%'' + @AgencyName + ''%''';                                            
        IF @InsuredName IS NOT NULL                                            
   SET @sql += ' AND isd.Name LIKE ''%'' + @InsuredName + ''%''';                                            
        IF @InvoiceNo IS NOT NULL                                            
            SET @sql += ' AND inv.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                                            
        IF @OutstandingBalances = 1                                          
            SET @sql += ' AND inv.RemainingBalance <> 0';                                            
                                            
    -- Define parameters for sp_executesql                                            
    SET @params = N'@Client_Id BIGINT,                                
          @Entity_Name VARCHAR(100),                               
                    @EntityType_Id TINYINT,                                            
                    @Entity_Id BIGINT,                                                   
                    @LineItemIds VARCHAR(Max),                
                    @PolicyNumber VARCHAR(100),                                            
                    @AgencyName VARCHAR(200),                                            
                    @InsuredName VARCHAR(100),                                            
                    @InvoiceNo VARCHAR(100),                                            
                    @OutstandingBalances BIT';                                            
    print @sql                       
    -- Execute the query                                            
    EXEC SP_EXECUTESQL @sql, @params,                                            
        @Client_Id = @Client_Id,                                            
        @EntityType_Id = @EntityType_Id,                               
        @Entity_Name=@Entity_Name,                              
        @Entity_Id = @Entity_Id,                                             
        @LineItemIds = @LineItemIds,                                             
        @PolicyNumber = @PolicyNumber,                                            
        @AgencyName = @AgencyName,                                            
        @InsuredName = @InsuredName,                                            
        @InvoiceNo = @InvoiceNo,                                            
        @OutstandingBalances = @OutstandingBalances;                                            
END 
go
CREATE   PROCEDURE USP_Accounting_GetAPSearchResults_V3                                      
(                                                    
    @EntityType_Id         TINYINT = NULL,                                                    
    @Entity_Id             BIGINT = NULL,                  
    @LineItemIds     VARCHAR(MAX) = NULL,          
 @LineItem_Id         TINYINT = NULL,         
    @PolicyNumber          VARCHAR(100) = NULL,                                                    
    @AgencyName            VARCHAR(200) = NULL,                                                    
    @InsuredName           VARCHAR(100) = NULL,                                                    
    @OutstandingBalances   BIT = NULL,                                                    
    @InvoiceNo             VARCHAR(100) = NULL,                                                    
    @EntityName      VARCHAR(100) = NULL, -- Added By Rohith                                            
    @Client_Id             BIGINT = NULL                                                    
)                                                    
AS                                                    
BEGIN                                                    
    SET NOCOUNT ON;                                                    
                                                    
    DECLARE @sql NVARCHAR(MAX) = N'';                                                    
    DECLARE @where NVARCHAR(MAX) = N'';                                                    
    DECLARE @params NVARCHAR(MAX);                                                    
    DECLARE @isInitialLoad BIT = 0;                                                    
                                                    
    ---------------------------------------------------------------------------------                                                    
    -- Step 1: Detect initial load (no filters)                                                    
    ---------------------------------------------------------------------------------                                                    
    IF @EntityType_Id IS NULL                                                    
       AND @Entity_Id IS NULL                     
    AND @LineItemIds IS NULL                                                   
       AND @PolicyNumber IS NULL                                                    
       AND @AgencyName IS NULL                                                    
       AND @InsuredName IS NULL                                                    
       AND @InvoiceNo IS NULL                                            
    AND @EntityName IS NULL -- Added by Rohith                                            
    BEGIN                                                    
        SET @isInitialLoad = 1;                                                    
    END                                                    
                                                    
    ---------------------------------------------------------------------------------                                                    
    -- Step 2: Begin query                                                    
    ---------------------------------------------------------------------------------                                                    
    IF @isInitialLoad = 1                                                    
    BEGIN                                                    
        SET @sql += N'                                                    
        ;WITH LimitedInvoices AS (                                                    
            SELECT TOP (25) inv.Id                                                    
            FROM Accounting_Invoices inv                                                    
            INNER JOIN Accounting_PolicyDetails pd                                                    
                ON inv.PolicyDetails_Id = pd.Id                                                  
                AND pd.Status = 1                                   
            WHERE inv.Status = 1                 
              AND pd.Client_Id = @Client_Id                                                    
           ORDER BY inv.Id DESC                                                    
),                                                    
        ';                                                  
    END                                          
    ELSE                                                    
    BEGIN                       
        SET @sql += N';WITH ';                                                    
    END                                                    
                                                  
    ---------------------------------------------------------------------------------                                                    
   -- Step 3: Add the supporting CTEs                                                    
    ---------------------------------------------------------------------------------                                                    
    SET @sql += N'                                                    
    ARCollectedForPassThrough AS (                                                    
        SELECT SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id,LIC.LineItem_Id as  LineItem_Id, ID.LOB_Id                                                    
        FROM Accounting_InvoiceDetails ID                                                    
     INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 1 AND ID.Status = 1                                  
  GROUP BY INV.Id, LIC.LineItem_Id, ID.LOB_Id                                                    
    ),                                                    
    AgtCommissionPayableAmount AS (                                                    
        SELECT SUM(ID.Amount) AS OriginalAgtCommission, INV.Id AS Invoice_Id, ID.LOB_Id                                                    
        FROM Accounting_InvoiceDetails ID                                                    
        LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                    
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Status = 1 AND LI.Code = ''AgencyBrokerCommission''                        
  GROUP BY ID.LOB_Id, INV.Id                        
    ),              
    TotalCommissionReceivableAmount AS (                                                    
        select SUM(ID.Amount) AS OriginalTotalCommission, INV.Id AS Invoice_Id, ID.LOB_Id                                                    
        FROM Accounting_InvoiceDetails ID              
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                    
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1 AND LI.Code = ''TotalCommission''                         
  GROUP BY ID.LOB_Id, INV.Id                        
    ),                                                    
    ARCollectedForTotalCommission AS (                                                    
        SELECT SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id, ID.LOB_Id                                                  
        FROM Accounting_InvoiceDetails ID                                                    
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                  
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''TotalCommission''                                                   
         GROUP BY INV.Id, ID.LOB_Id                                                   
    ),                                                   
    ARCollectedForTotalHouseCommission AS (                                                    
        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id                                                   
        FROM Accounting_InvoiceDetails ID                                                    
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                  
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                   
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''HouseCommission''                                                   
        GROUP BY ID.Id, INV.Id                                                   
    ),                          
 TotalARHouseCommissionInvoiced AS (                                         
  SELECT  SUM(ID.Amount) AS OriginalTotalHouseCommission, INV.Id AS Invoice_Id, ID.LOB_Id                                                      
        FROM Accounting_InvoiceDetails ID                                                    
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                    
        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1 AND LI.Code = ''HouseCommission''               
  GROUP BY ID.LOB_Id, INV.Id                
    ),                          
    ARCollectedForSalesRepCommission AS (                                                    
        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id                                                    
        FROM Accounting_InvoiceDetails ID                                                    
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                                    
        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                                                    
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''SalesRepCommission''                                                
        GROUP BY ID.Id, INV.Id                          
    ),                            
    APAmountSalesRepCommission AS (                                                    
        SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalSalesRepCommission, INV.Id AS Invoice_Id                                                    
        FROM Accounting_InvoiceDetails ID                                                    
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                                         
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                 
        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                    
        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''SalesRepCommission''                           
    ),                            
 TotalARInvoiced AS (                              
     SELECT                 
    INV.Id AS Invoice_Id,                              
    SUM(ID.Amount) AS OriginalARAmount                                             
   FROM Accounting_InvoiceDetails ID                                
   INNER JOIN Accounting_Invoices INV                               
    ON ID.Invoice_Id = INV.Id                               
    AND INV.Status = 1                                     
   INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD                               
    ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id                               
    AND BLD.Status = 1                                                    
   INNER JOIN Accounting_LU_LineItems_Directions LD                               
    ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id                               
    AND LD.Status = 1                                
   INNER JOIN Accounting_LU_LineItems_Clients LIC                               
    ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id                               
    AND LIC.Status = 1                                                   
   INNER JOIN Accounting_LU_LineItems LI                               
    ON LI.LineItem_Id = LIC.LineItem_Id                               
    AND LI.Status = 1                                   
   WHERE                               
    LD.Direction_Id = 1                               
    AND ID.Status = 1                               
   GROUP BY                               
    INV.Id                              
                              
 ),                              
 TotalAPInvoiced AS (                      
     SELECT                               
    INV.Id AS Invoice_Id,                              
    SUM(ID.Amount) AS OriginalAPAmount                              
   FROM Accounting_InvoiceDetails ID                                
   INNER JOIN Accounting_Invoices INV                               
    ON ID.Invoice_Id = INV.Id                               
    AND INV.Status = 1                                                    
   INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD                               
    ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id                               
    AND BLD.Status = 1                                                    
   INNER JOIN Accounting_LU_LineItems_Directions LD                               
    ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id                               
    AND LD.Status = 1                                
   INNER JOIN Accounting_LU_LineItems_Clients LIC                               
    ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id                        
    AND LIC.Status = 1                                                    
   INNER JOIN Accounting_LU_LineItems LI                               
    ON LI.LineItem_Id = LIC.LineItem_Id                               
    AND LI.Status = 1                                   
   WHERE                               
    LD.Direction_Id = 2                               
    AND ID.Status = 1                               
   GROUP BY                               
    INV.Id                              
 )                              
    SELECT                                                    
        ID.*,                                                    
        CASE                                                     
            WHEN ID.EntityType_Id = 1 THEN ag.Name                                                    
            WHEN ID.EntityType_Id = 2 THEN ins.Name                                                    
            WHEN ID.EntityType_Id = 3 THEN ca.Name                      
   WHEN ID.EntityType_Id = 4  THEN cl.CompanyName                      
     WHEN ID.EntityType_Id = 6 THEN wsd.Name                                              
   WHEN ID.EntityType_Id = 7 THEN concat(''State of '', Pd.riskstate)                                  
   WHEN ID.EntityType_Id = 11 THEN AOED.Name                             
   WHEN ID.EntityType_Id = 10 THEN srd.Name                            
            ELSE NULL                                                    
        END AS EntityName,                                                    
 ET.Name AS EntityTypeName,                                                    
        CASE                                                     
            WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected                                                    
            WHEN LI.Code = ''AgencyBrokerCommission'' THEN ((AGPA.OriginalAgtCommission / TCRA.OriginalTotalCommission) * ARTC.ArCollected)                              
   WHEN LI.Code = ''SalesRepCommission'' AND BT.Name = ''Direct Bill'' THEN ((ARTC.ArCollected / NULLIF(TCRA.OriginalTotalCommission, 0)) * APSRC.OriginalSalesRepCommission)                           
 WHEN LI.Code = ''SalesRepCommission'' AND BT.Name = ''Agency Bill'' THEN ((ARHC.ArCollected / NULLIF(ARHCI.OriginalTotalHouseCommission, 0)) * APSRC.OriginalSalesRepCommission)                                
 ELSE 0                              
        END AS ARCollected,                                    
  LIC.Is_Commissionable,                                  
  LI.LineItem_Id                                  
    FROM Accounting_InvoiceDetails ID                                                    
    LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                      
    INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1                                                    
    LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id = pd.AgentDetail_Id AND agd.Status = 1                                                    
    LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id = pd.InsuredDetail_Id AND isd.Status = 1                                                    
    LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1                                                    
    LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1                      
 LEFT JOIN Accounting_Clients cl ON cl.Client_Id = PD.Client_Id AND cl.Status = 1                       
    LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1                                                    
    LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1                             
    LEFT JOIN Accounting_SalesRepDetails srd ON srd.Id = ID.Entity_Id AND ID.EntityType_Id = 10 AND srd.Status = 1                             
    LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                    
    LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                                                    
 LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                                                    
    LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                    
    LEFT JOIN Accounting_LU_EntityTypes ET ON ID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1                                                    
  LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = INV.Id  and ARPT.LineItem_Id = LIC.LineItem_Id and ID.LOB_Id = ARPT.LOB_Id                                                 
    LEFT JOIN AgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = INV.Id and ID.LOB_ID = AGPA.LOB_Id                                                   
    LEFT JOIN TotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = INV.Id and ID.LOB_ID = TCRA.LOB_Id                                                   
    LEFT JOIN ARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = INV.Id  and ID.LOB_ID = ARTC.LOB_Id                           
 LEFT JOIN ARCollectedForSalesRepCommission ARSC ON ARSC.Invoice_Id = INV.Id                             
 LEFT JOIN ARCollectedForTotalHouseCommission ARHC ON ARHC.Invoice_Id = INV.Id                           
 LEFT JOIN TotalARHouseCommissionInvoiced ARHCI ON ARHCI.Invoice_Id = INV.Id and ID.LOB_ID = ARHCI.LOB_Id                          
 LEFT JOIN APAmountSalesRepCommission APSRC ON APSRC.Invoice_Id = INV.Id                           
 LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = ID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1                                  
 LEFT JOIN TotalARInvoiced TARI ON TARI.Invoice_Id = INV.Id                                  
 LEFT JOIN TotalAPInvoiced TAPI ON TAPI.Invoice_Id = INV.Id                           
 INNER JOIN Accounting_LU_BillingTypes BT ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1                                  
   WHERE LD.Direction_Id =2                                          
  AND PD.Client_Id = @Client_Id                                                    
  AND ID.Status = 1                                          
  AND LI.Status = 1                                        
  AND LIC.Status = 1                                        
    ';                                                    
                                                    
    ---------------------------------------------------------------------------------                                                    
    -- Step 4: Build dynamic WHERE filters                                                    
    ---------------------------------------------------------------------------------                                 
          IF @EntityType_Id IS NOT NULL        
    BEGIN         
    IF @EntityType_Id = 8         
    BEGIN          
    SET @sql += N' AND id.EntityType_Id IN (6,3)';         
    END         
    ELSE IF @EntityType_Id = 9         
    BEGIN          
    SET @sql += N' AND id.EntityType_Id = 3';         
    END         
    ELSE         
    BEGIN           
    SET @sql += N' AND id.EntityType_Id = @EntityType_Id';        
    END        
    END                                                        
    IF @Entity_Id IS NOT NULL                                                    
        SET @where += N' AND ID.Entity_Id = @Entity_Id';                                                  
    IF @LineItemIds IS NOT NULL                                                    
        SET @where += N' AND LIC.LineItem_Id IN (SELECT value FROM STRING_SPLIT(@LineItemIds, '',''))';                                                   
    IF @PolicyNumber IS NOT NULL                    
        SET @where += '                    
        AND REPLACE(REPLACE(REPLACE(REPLACE(PD.PolicyNumber,'' '',''''),''-'',''''),''"'',''''),''.'','''')                    
      =                    
        REPLACE(REPLACE(REPLACE(REPLACE(@PolicyNumber,'' '',''''),''-'',''''),''"'',''''),''.'','''')';                                                
    IF @AgencyName IS NOT NULL                                                    
        SET @where += N' AND agd.Name LIKE ''%'' + @AgencyName + ''%''';                                                    
    IF @InsuredName IS NOT NULL                                                    
        SET @where += N' AND isd.Name LIKE ''%'' + @InsuredName + ''%''';                       
    IF @InvoiceNo IS NOT NULL                                                    
        SET @where += N' AND INV.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                                            
 IF @EntityName IS NOT NULL                                             
  BEGIN                                            
  IF @EntityType_Id = 1  SET @where += N' AND ag.Name LIKE ''%'' + @EntityName + ''%''';                                             
  ELSE IF @EntityType_Id = 2 SET @where += N' AND ins.Name LIKE ''%''+ @EntityName + ''%''';                                             
  ELSE IF @EntityType_Id = 3 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';                       
  ELSE IF @EntityType_Id = 4 SET @where += N' AND cl.CompanyName LIKE ''%'' + @EntityName + ''%''';                       
  ELSE IF @EntityType_Id = 6 SET @where += N' AND wsd.Name LIKE ''%'' + @EntityName + ''%''';                                             
  ELSE IF @EntityType_Id = 7 SET @where += N' AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%''';        
  ELSE IF @EntityType_Id = 8 SET @where += N' AND (wsd.Name LIKE ''%'' + @EntityName + ''%'' OR ca.Name LIKE ''%'' + @EntityName + ''%'' )';   ELSE IF @EntityType_Id = 9 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';                     
  
          
  END                                           
                                          
 --IF @EntityName IS NOT NULL                                            
 -- SET @where += N'                                            
 -- AND (                                            
 --  (ID.EntityType_Id = 1 AND ag.Name LIKE ''%'' + @EntityName + ''%'')                                            
 -- OR (ID.EntityType_Id = 2 AND ins.Name LIKE ''%'' + @EntityName + ''%'')                                            
 -- OR (ID.EntityType_Id = 3 AND ca.Name LIKE ''%'' + @EntityName + ''%'')                                            
 -- OR (ID.EntityType_Id = 6 AND wsd.Name LIKE ''%'' + @EntityName + ''%'')                                            
 -- OR (ID.EntityType_Id = 7 AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%'')                                            
 --   )';   -- Added By Rohith                                            
                                           
    IF @OutstandingBalances = 1                                                    
        SET @where += N' AND ID.RemainingBalance <> 0';                                                    
                                                    
    -- Add LimitedInvoices only for initial load                                        
    IF @isInitialLoad = 1                                                    
        SET @where += N' AND INV.Id IN (SELECT Id FROM LimitedInvoices)';                                                    
                                         
    SET @sql += @where + N' ORDER BY INV.Id DESC;';                                                    
                                                    
    ---------------------------------------------------------------------------------                                                    
    -- Step 5: Execute dynamic SQL                                                    
    ---------------------------------------------------------------------------------                                                    
    SET @params = N'                             
        @EntityType_Id TINYINT,                                                    
        @Entity_Id BIGINT,             
        @LineItemIds VARCHAR(Max),                                                    
        @PolicyNumber VARCHAR(100),                                                    
        @AgencyName VARCHAR(200),                                                            
  @InsuredName VARCHAR(100),                                                    
        @OutstandingBalances BIT,                                                    
        @InvoiceNo VARCHAR(100),                                            
  @EntityName VARCHAR(100),                                             
        @Client_Id BIGINT                                                    
    ';                                                    
    EXEC sp_executesql                                                    
        @sql,                                                    
        @params,                                                    
        @EntityType_Id = @EntityType_Id,                                                    
        @Entity_Id = @Entity_Id,             
  @LineItemIds=@LineItemIds,                  
        @PolicyNumber = @PolicyNumber,                                                    
        @AgencyName = @AgencyName,                                 
        @InsuredName = @InsuredName,                                                    
        @OutstandingBalances = @OutstandingBalances,                                                    
        @InvoiceNo = @InvoiceNo,                                            
  @EntityName = @EntityName, -- Added by Rohith                                            
        @Client_Id = @Client_Id;                                                    
END 
go