    
ALTER Proc [dbo].[USP_Accounting_DirectBillCommissionStatementReport]        
(                    
 @Client_Id bigint = null  ,                   
 @AgentDetail_Id BIGINT = NULL,                    
 @InvoiceDate VARCHAR(100) = NULL,                    
 @InvoiceCreatedFromDate VARCHAR(200) = NULL,                    
 @InvoiceCreatedToDate VARCHAR(200) = NULL,                    
 @InvoiceEffectiveFromDate VARCHAR(200) = NULL,                  
 @InvoiceEffectiveToDate VARCHAR(200) = NULL,                    
 @Range VARCHAR(200) = NULL ,                   
 @WholesalerDetail_Id BIGINT = NULL,                  
 @CarrierDetail_Id BIGINT = NULL,                
 @LOB_Id INT = NULL                     
  )                    
  as                    
  begin                    
          
  DECLARE @CompareDateFrom DATETIME;                    
    DECLARE @CompareDateTo DATETIME;                    
                    
    IF @InvoiceDate = 'InvoiceCreatedDate'                    
    BEGIN                    
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedFromDate, 101));                    
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedToDate, 101))));                    
    END                    
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'                    
    BEGIN                    
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveFromDate, 101));                    
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveToDate, 101))));                    
    END                    
                    
    SELECT *                     
    FROM Accounting_AgentDetails                     
    WHERE Client_Id = @Client_Id AND Status = 1                    
      AND (@AgentDetail_Id IS NULL OR AgentDetail_Id = @AgentDetail_Id);                    
                    
  SELECT       
  *       
  --I.InvoiceNo      
  FROM Accounting_PolicyDetails PD      
  --JOIN Accounting_Invoices I       
  --ON I.PolicyDetails_Id = PD.ID AND    
  --Status = 1      
  WHERE PD.Client_Id = @Client_Id       
  AND PD.Status = 1       
  AND PD.BillType = 'Direct'      
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
   OR (@InvoiceDate = 'InvoiceEffectiveDate'        
    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
    AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo)        
   OR (@InvoiceDate = 'InvoiceCreatedDate'        
    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
    AND PD.CreatedOn > @CompareDateFrom AND PD.CreatedOn <= @CompareDateTo))        
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)        
  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)        
  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
  ;WITH PolicyLOBs AS (                
        SELECT DISTINCT                 
            PD.PolicyNumber,                
            PD.AgentDetail_Id,                
            IND.Name AS InsuredName,                
            LOB.Name AS LOBName,                
            CD.Name AS CarrierName                
        FROM Accounting_PolicyDetails PD                
        JOIN Accounting_InsuredDetails IND ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1                
        JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1                
        JOIN Accounting_Invoices IES ON IES.PolicyDetails_Id = PD.ID AND IES.Status = 1                
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = IES.ID AND ID.Status = 1                
        JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                
        WHERE PD.Client_Id = @Client_Id AND PD.BillType = 'Direct' AND PD.Status = 1              
  AND            
  (            
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL            
        OR            
        (            
            @InvoiceDate = 'InvoiceEffectiveDate'            
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL            
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo            
        )            
        OR            
        (            
            @InvoiceDate = 'InvoiceCreatedDate'            
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL            
            AND IES.CreatedOn > @CompareDateFrom AND IES.CreatedOn <= @CompareDateTo            
        ))             
 AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)          
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)      
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
 AND (@LOB_Id IS NULL OR EXISTS (      
    SELECT 1      
    FROM Accounting_InvoiceDetails AID      
    WHERE AID.Invoice_Id = IES.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))      
    )                
    SELECT                 
        PolicyNumber,                
        InsuredName,           
        AgentDetail_Id,                
        CarrierName,                
        STUFF((                
            SELECT DISTINCT ', ' + L2.LOBName                
            FROM PolicyLOBs L2                
            WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName                
            FOR XML PATH(''), TYPE                
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames                
    FROM PolicyLOBs L1                
    GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;                
            
  ;WITH AggregatedPremiumCommissionInvoiceDetails  AS (      
    SELECT Distinct      
        I.Id AS InvoiceId,      
        I.PolicyDetails_Id,      
        CAST(I.InvoiceNo AS VARCHAR(50)) AS InvoiceNo,      
      
        -- Premium AR: Premium + AR      
        SUM(CASE       
            WHEN LI.Name = 'Total Premium' AND LUD.Name = 'Accounts Receivable'       
            THEN ID.Amount ELSE 0       
        END) AS PremiumAR,      
      
        -- Agent Commission: Agency/Broker Commission + Accounts Payable      
        SUM(CASE       
            WHEN LI.Name = 'Agency/Broker Commission' AND LUD.Name = 'Accounts Payable'       
            THEN ID.Amount ELSE 0       
        END) AS AgentCommission,      
    ID.Amount AS Premium,      
        -- MGA Commission: MGA Commission + Internal      
        
        PDL.HouseCommission - PDL.AgentCommission  AS MGACommission,      
      
  -- Agent Commission Percentage = (AgentCommission / PremiumAR) * 100      
  PDL.AgentCommission AS AgentCommissionPercentage ,  
  CASE   
    WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommission <> 0   
    THEN (Details.TotalAgentBrokerCommission / Details.TotalCommission) * 100   
    ELSE 0.00   
END AS AgentCommPercentage,  
 CASE   
    WHEN Details.TotalCommission IS NOT NULL   
         AND Details.TotalCommissionPercentage IS NOT NULL   
         AND Details.TotalCommissionPercentage <> 0  
    THEN (Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0))  
    ELSE 0.00  
END AS TotalPremium,      
            Details.TotalCommission,        
            Details.TotalHouseCommission,        
            Details.TotalAgentBrokerCommission ,  
   Details.TotalCommissionPercentage  
    FROM Accounting_Invoices I      
    JOIN Accounting_PolicyDetails PD       
        ON PD.ID = I.PolicyDetails_Id       
        AND PD.Client_Id = @Client_Id       
        AND PD.Status = 1       
        AND PD.BillType = 'Direct'      
      
    JOIN Accounting_InvoiceDetails ID       
        ON ID.Invoice_Id = I.Id       
        AND ID.Status = 1      
      
    JOIN Accounting_LU_BillingType_LineItems_Directions BLD       
        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id       
        AND BLD.Status = 1      
      
    JOIN Accounting_LU_LineItems_Directions LD       
        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id       
        AND LD.Status = 1      
      
    JOIN Accounting_LU_LineItems LI       
        ON LI.LineItem_Id = LD.LineItem_Id       
        AND LI.Status = 1      
      
    LEFT JOIN (      
    SELECT       
        PolicyDetails_Id,      
        MAX(HouseCommission) AS HouseCommission,      
        MAX(AgentCommission) AS AgentCommission      
    FROM Accounting_PolicyDetailsLineItems      
    WHERE Status = 1      
    GROUP BY PolicyDetails_Id      
) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id      
      
   JOIN Accounting_LU_Directions LUD       
       ON LUD.Direction_Id = LD.Direction_Id       
        AND LUD.Status = 1      
 OUTER APPLY (        
    SELECT     
        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,      
        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,      
        CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,     
        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,        
        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission    
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
        JOIN Accounting_LU_LineItems LI         
            ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1        
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
        I.Status = 1               
  AND                
  (                
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                
        OR                
        (                
            @InvoiceDate = 'InvoiceEffectiveDate'                
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo                
        )                
        OR                
        (                
            @InvoiceDate = 'InvoiceCreatedDate'                
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo                
        ))                
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)              
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)          
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)           
 AND (@LOB_Id IS NULL OR EXISTS (          
    SELECT 1          
    FROM Accounting_InvoiceDetails AID          
    WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))          
        GROUP BY I.Id, I.PolicyDetails_Id, I.InvoiceNo,      
  PDL.HouseCommission, PDL.AgentCommission,                  
    ID.Amount,Details.TotalPremium,       
            Details.TotalCommission,        
            Details.TotalHouseCommission,        
            Details.TotalAgentBrokerCommission,  
   Details.TotalCommissionPercentage),    
     
 FeeData AS  (                    
    SELECT                     
        I.PolicyDetails_Id,      
        I.Id AS Invoice_Id,                    
        ID.Id AS InvoiceDetail_Id,                    
        CONCAT(PD.TransactionType, ' ', LI.Name) AS LineItemDescription,                    
        I.InvoiceNo AS IdNo,                    
        PD.EffectiveDate,                    
        PD.ExpirationDate,                    
        ID.Amount AS GrossAmount,         
        -- Agent Commission Percentage    
        APCID.AgentCommissionPercentage AS AgentCommPercentage,      
        -- Net Amount = GrossAmount    
        --ID.Amount AS NetAmount,    
  APCID.TotalPremium AS NetAmount,    
        -- Due Date    
        I.DueDate,                    
    
        -- Policy Info    
        PD.PolicyNumber,        
        ID.RemainingBalance AS RemainingBalance,      
    
        -- AgentCommissionAmt = Gross * Commission %    
        ID.Amount * (PDL.AgentCommission / 100.0) AS AgentCommissionAmt,    
    
        -- PremiumCollected = (AgentCommissionAmt * 100) / AgentCommission%    
        CAST(     CASE          WHEN APCID.TotalCommissionPercentage IS NOT NULL AND APCID.TotalCommissionPercentage <> 0          THEN APCID.TotalCommission / (APCID.TotalCommissionPercentage / 100.0)         
		ELSE 0.00      END  AS FLOAT) AS PremiumCollected,  
  CASE   
    WHEN APCID.TotalCommission IS NOT NULL   
         AND APCID.TotalCommission > 0   
         AND APCID.TotalCommissionPercentage IS NOT NULL   
         AND APCID.TotalCommissionPercentage > 0  
         AND APCID.AgentCommissionPercentage IS NOT NULL   
         AND APCID.AgentCommissionPercentage > 0  
    THEN ROUND(  
        (APCID.TotalCommission / (APCID.TotalCommissionPercentage / 100.0))   
        * (APCID.AgentCommissionPercentage / 100.0),  
        2  
    )  
    ELSE 0.00  
END AS [Comm.],  
          -- CommsNetDue = Gross - MGA Commission    
--      CASE   
--    WHEN PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission <> 0   
--    THEN ROUND(ID.Amount * (PDL.AgentCommission / 100.0), 2)   
--    ELSE 0.00   
--END AS CommsNetDue,    
I.RemainingBalance AS CommsNetDue,  
   APCID.TotalPremium    
    
    
    FROM Accounting_Invoices I   JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id     
        AND PD.Status = 1     
        AND PD.BillType = 'Direct'                    
    
    JOIN Accounting_InvoiceDetails ID     
        ON ID.Invoice_Id = I.Id     
        AND ID.Status = 1                    
    
    JOIN Accounting_LU_BillingType_LineItems_Directions BLD     
        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id     
        AND BLD.Status = 1                    
    
    JOIN Accounting_LU_LineItems_Directions LD     
        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id     
        AND LD.Status = 1                    
    
    JOIN Accounting_LU_LineItems LI     
        ON LI.LineItem_Id = LD.LineItem_Id     
        AND LI.Status = 1                            
    
    JOIN Accounting_LU_LineItemTypes LIT     
        ON LIT.LineItemType_Id = LI.LineItemType_Id     
        AND LIT.Status = 1                    
    
    JOIN Accounting_LU_Directions LUD     
        ON LUD.Direction_Id = LD.Direction_Id     
 AND LUD.Status = 1                    
    
    JOIN Accounting_LU_LOBs LOB     
        ON LOB.LOB_Id = ID.LOB_Id     
        AND LOB.Status = 1                  
    
 Left join AggregatedPremiumCommissionInvoiceDetails APCID      
 ON APCID.PolicyDetails_Id=PD.ID      
 AND PD.Status=1    
    
    LEFT JOIN (    
        SELECT     
            PolicyDetails_Id,    
            MAX(HouseCommission) AS HouseCommission,    
            MAX(AgentCommission) AS AgentCommission      
        FROM Accounting_PolicyDetailsLineItems     
        WHERE Status = 1     
        GROUP BY PolicyDetails_Id       
    ) PDL     
        ON PDL.PolicyDetails_Id = I.PolicyDetails_Id      
    
    WHERE I.Status = 1                   
      AND                
  (                
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                
        OR                
        (                
            @InvoiceDate = 'InvoiceEffectiveDate'                
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo                
        )                
        OR                
        (                
            @InvoiceDate = 'InvoiceCreatedDate'                
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo                
        ))                 
  AND (LIT.Code = 'Fee' OR LIT.Code = 'Tax') AND LUD.Code = 'AP'              
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)              
  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)           
  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)            
  AND (@LOB_Id IS NULL OR EXISTS (          
    SELECT 1          
    FROM Accounting_InvoiceDetails AID          
    WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))               
    )                    
   , PaymentApplication AS (      
    SELECT *      
    FROM (      
        SELECT       
            I.PolicyDetails_Id,                    
            I.Id AS Invoice_Id,       
            ID.Id AS InvoiceDetail_Id,                    
            'Comm. Paid' AS LineItemDescription,       
            I.InvoiceNo AS IdNo,                    
            CAST(P.CreatedOn AS DATE) AS EffectiveDate,                    
            PD.ExpirationDate,                    
            ID.Amount AS GrossAmount,   
        APCID.AgentCommissionPercentage AS AgentCommPercentage,     
   Cast(APCID.TotalPremium AS FLOAT) AS NetAmount,    
            --CASE WHEN P.PaymentDirection_Id = 2 THEN -PALI.AppliedAmount ELSE ABS(PALI.AppliedAmount) END AS NetAmount,      
            I.DueDate,                    
            PD.PolicyNumber,       
            0 AS RemainingBalance,      
   ID.Amount * (PDL.AgentCommission / 100.0) AS AgentCommissionAmt,   
         CAST(     CASE          WHEN APCID.TotalCommissionPercentage IS NOT NULL AND APCID.TotalCommissionPercentage <> 0          THEN APCID.TotalCommission / (APCID.TotalCommissionPercentage / 100.0)          
		 ELSE 0.00      END  AS FLOAT) AS PremiumCollected,  
  CASE   
    WHEN APCID.TotalCommission IS NOT NULL   
         AND APCID.TotalCommission > 0   
         AND APCID.TotalCommissionPercentage IS NOT NULL   
         AND APCID.TotalCommissionPercentage > 0  
         AND APCID.AgentCommissionPercentage IS NOT NULL   
         AND APCID.AgentCommissionPercentage > 0  
    THEN ROUND(  
        (APCID.TotalCommission / (APCID.TotalCommissionPercentage / 100.0))   
        * (APCID.AgentCommissionPercentage / 100.0),  
        2  
    )  
    ELSE 0.00  
END AS [Comm.],  
--      CASE   
--    WHEN PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission <> 0   
--    THEN ROUND(ID.Amount * (PDL.AgentCommission / 100.0), 2)   
--    ELSE 0.00   
--END AS CommsNetDue,   
I.RemainingBalance AS CommsNetDue,  
   APCID.TotalPremium,    
    
            ROW_NUMBER() OVER (PARTITION BY I.Id ORDER BY P.CreatedOn DESC, PALI.AppliedAmount DESC) AS rn      
        FROM Accounting_Invoices I      
        JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType = 'Direct'      
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1      
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
        JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1      
        JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1      
        JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1      
        JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1      
        JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1      
        JOIN Accounting_PaymentApplications PA ON PA.PaymentApplication_Id = PALI.PaymentApplication_Id AND PA.Status = 1      
        JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1      
  Left join AggregatedPremiumCommissionInvoiceDetails APCID  ON APCID.PolicyDetails_Id=PD.ID  AND PD.Status=1    
  LEFT JOIN (SELECT PolicyDetails_Id,MAX(HouseCommission) AS HouseCommission,MAX(AgentCommission) AS AgentCommission      
   FROM Accounting_PolicyDetailsLineItems WHERE Status = 1 GROUP BY PolicyDetails_Id       
  ) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id      
        WHERE       
            I.Status = 1      
            AND LUD.Code = 'AP'      
            AND (      
                @CompareDateFrom IS NULL OR @CompareDateTo IS NULL      
                OR (      
                    @InvoiceDate = 'InvoiceEffectiveDate'      
                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
                    AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo      
                )      
                OR (      
                    @InvoiceDate = 'InvoiceCreatedDate'      
                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
                    AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo      
                )      
            )      
            AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
            AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)      
            AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
            AND (@LOB_Id IS NULL OR EXISTS (      
                SELECT 1      
                FROM Accounting_InvoiceDetails AID      
                WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id      
            ))      
    ) Ranked      
    WHERE rn = 1      
)      
      
SELECT       
    PolicyDetails_Id,      
    Invoice_Id,      
    InvoiceDetail_Id,      
    LineItemDescription,      
    IdNo,      
    EffectiveDate,      
    ExpirationDate,      
    GrossAmount,      
    AgentCommPercentage,      
    NetAmount,      
    DueDate,      
    PolicyNumber,      
    RemainingBalance,      
    PremiumCollected,    
 [Comm.] AS [Comm.],     
 CommsNetDue ,    
 TotalPremium      
FROM FeeData      
      
UNION ALL      
      
SELECT       
    PolicyDetails_Id,      
    Invoice_Id,      
    InvoiceDetail_Id,      
    LineItemDescription,      
    IdNo,      
    EffectiveDate,      
    ExpirationDate,      
    GrossAmount,      
    AgentCommPercentage,      
    NetAmount,      
    DueDate,      
    PolicyNumber,      
    RemainingBalance,      
    PremiumCollected,     
 [Comm.] AS [Comm.],  
 CommsNetDue ,    
 TotalPremium    
FROM PaymentApplication      
      
  end