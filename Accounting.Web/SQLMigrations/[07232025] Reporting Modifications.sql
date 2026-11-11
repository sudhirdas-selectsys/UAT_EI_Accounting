--ALTER Proc [dbo].[USP_Accounting_DirectBillCommissionStatementReport]        
--(                    
-- @Client_Id bigint = null  ,                   
-- @AgentDetail_Id BIGINT = NULL,                    
-- @InvoiceDate VARCHAR(100) = NULL,                    
-- @InvoiceCreatedFromDate VARCHAR(200) = NULL,                    
-- @InvoiceCreatedToDate VARCHAR(200) = NULL,                    
-- @InvoiceEffectiveFromDate VARCHAR(200) = NULL,                  
-- @InvoiceEffectiveToDate VARCHAR(200) = NULL,                    
-- @Range VARCHAR(200) = NULL ,                   
-- @WholesalerDetail_Id BIGINT = NULL,                  
-- @CarrierDetail_Id BIGINT = NULL,                
-- @LOB_Id INT = NULL                     
--  )                    
--  as                    
--  begin                    
          
--  DECLARE @CompareDateFrom DATETIME;                    
--    DECLARE @CompareDateTo DATETIME;                    
                    
--    IF @InvoiceDate = 'InvoiceCreatedDate'                    
--    BEGIN                    
--        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedFromDate, 101));                    
--        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedToDate, 101))));                    
--    END                    
--    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'                    
--    BEGIN                    
--        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveFromDate, 101));                    
--        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveToDate, 101))));                    
--    END                    
                    
--    SELECT *                     
--    FROM Accounting_AgentDetails                     
--    WHERE Client_Id = @Client_Id AND Status = 1                    
--      AND (@AgentDetail_Id IS NULL OR AgentDetail_Id = @AgentDetail_Id);                    
                    
--  SELECT  *           
--  FROM Accounting_PolicyDetails PD    
--  WHERE PD.Client_Id = @Client_Id       
--  AND PD.Status = 1       
--  AND PD.BillType = 'Direct'      
--  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
--   OR (@InvoiceDate = 'InvoiceEffectiveDate'        
--    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
--    AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo)        
--   OR (@InvoiceDate = 'InvoiceCreatedDate'        
--    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
--    AND PD.CreatedOn > @CompareDateFrom AND PD.CreatedOn <= @CompareDateTo))        
--  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)        
--  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)        
--  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id) 
  
--  ;WITH PolicyLOBs AS (                
--        SELECT DISTINCT                 
--            PD.PolicyNumber,                
--            PD.AgentDetail_Id,                
--            IND.Name AS InsuredName,                
--            LOB.Name AS LOBName,                
--            CD.Name AS CarrierName                
--        FROM Accounting_PolicyDetails PD                
--         JOIN Accounting_InsuredDetails IND ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1                
--         JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1                
--         JOIN Accounting_Invoices IES ON IES.PolicyDetails_Id = PD.ID AND IES.Status = 1                
--         JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = IES.ID AND ID.Status = 1                
--         JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                
--        WHERE PD.Client_Id = @Client_Id 
--		AND PD.BillType = 'Direct'
--		AND PD.Status = 1              
--  AND            
--  (            
--        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL            
--        OR            
--        (            
--            @InvoiceDate = 'InvoiceEffectiveDate'            
--            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL            
--            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo            
--        )            
--        OR            
--        (            
--            @InvoiceDate = 'InvoiceCreatedDate'            
--            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL            
--            AND IES.CreatedOn > @CompareDateFrom AND IES.CreatedOn <= @CompareDateTo            
--        ))             
-- AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)          
-- AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)      
-- AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
-- AND (@LOB_Id IS NULL OR EXISTS (      
--    SELECT 1      
--    FROM Accounting_InvoiceDetails AID      
--    WHERE AID.Invoice_Id = IES.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))      
--    )                
--    SELECT                 
--        PolicyNumber,                
--        InsuredName,           
--        AgentDetail_Id,                
--        CarrierName,                
--        STUFF((                
--            SELECT DISTINCT ', ' + L2.LOBName                
--            FROM PolicyLOBs L2                
--            WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName                
--            FOR XML PATH(''), TYPE                
--        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames                
--    FROM PolicyLOBs L1                
--    GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;                
     
	 
--	 ;WITH AggregatedPremiumCommissionInvoiceDetails AS (
--    SELECT
--        I.Id AS InvoiceId,
--        I.PolicyDetails_Id,
--        I.InvoiceNo,
--        SUM(CASE WHEN LI.Code = 'Premium' AND LUD.Code = 'AR' THEN ID.Amount ELSE 0 END)         AS PremiumAR,
--        SUM(CASE WHEN LI.Code = 'AgencyBrokerCommission' AND LUD.Code = 'C' THEN ID.Amount ELSE 0 END) AS AgentCommission,
--        SUM(CASE WHEN LI.Code = 'MGACommission' AND LUD.Code = 'IN' THEN ID.Amount ELSE 0 END)     AS MGACommission
--     FROM Accounting_Invoices I      
--     JOIN Accounting_PolicyDetails PD       
--        ON PD.ID = I.PolicyDetails_Id       
--        AND PD.Client_Id = @Client_Id       
--        AND PD.Status = 1       
--        AND PD.BillType = 'Direct'      
      
--     JOIN Accounting_InvoiceDetails ID       
--        ON ID.Invoice_Id = I.Id       
--        AND ID.Status = 1      
      
--     JOIN Accounting_LU_BillingType_LineItems_Directions BLD       
--        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id       
--        AND BLD.Status = 1      
      
--     JOIN Accounting_LU_LineItems_Directions LD       
--        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id       
--        AND LD.Status = 1      
      
--     JOIN Accounting_LU_LineItems LI       
--        ON LI.LineItem_Id = LD.LineItem_Id       
--        AND LI.Status = 1      
		 
--    JOIN Accounting_LU_Directions LUD       
--       ON LUD.Direction_Id = LD.Direction_Id       
--        AND LUD.Status = 1  
		
--    GROUP BY I.Id, I.PolicyDetails_Id, I.InvoiceNo
--),
--FeeData AS (
--   SELECT                     
--        I.PolicyDetails_Id,      
--        I.Id AS Invoice_Id,                    
--        ID.Id AS InvoiceDetail_Id, 
--		ABD.AgentDetail_Id,
--		ABD.Name AS [Agency Name],
--		CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],
--		CASE 
--		WHEN LI.Name = 'Total Premium' THEN CONCAT(PD.TransactionType, ' ','Premium')
--		ELSE CONCAT(PD.TransactionType, ' ', LI.Name)
--		END AS LineItemDescription,                   
--        I.InvoiceNo AS IdNo,                    
--        PD.EffectiveDate,                    
--        PD.ExpirationDate,                    
--        ID.Amount AS GrossAmount,     
--        ID.Amount * (PDL.AgentCommission / 100.0) AS AgentCommissionAmt,
--		CASE WHEN PDL.AgentCommission IS Null THen 0 ELSE PDL.AgentCommission
--		END AS AgentCommPercentage,     
--	--	CASE   
-- --   WHEN Details.TotalCommission IS NOT NULL   
-- --        AND Details.TotalCommissionPercentage IS NOT NULL   
-- --        AND Details.TotalCommissionPercentage <> 0  
-- --   THEN 
-- --       CAST(Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0) AS FLOAT)  
-- --   ELSE 
-- --       CAST(0.00 AS FLOAT)  
--	--END AS NetAmount,

--		Details.TotalPremium AS NetAmount,
--        I.DueDate,        
--        PD.PolicyNumber,        
--        ID.RemainingBalance AS RemainingBalance, 
--  --      CAST(     
--		--CASE          
--		--WHEN Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0      
--		--THEN Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0)         
--		--ELSE 0.00      
--		--END  AS FLOAT) 
--		--AS InferredPremiumCollected, 
--		ISNULL((
--    SELECT SUM(PALI.AppliedAmount)
--    FROM Accounting_PaymentApplicationLineItems PALI
--    JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
--    JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1
--    WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
--), 0.00) AS TotalCommissionReceived,

---- Inferred Premium Collected
--	CAST(
--  CASE 
--    WHEN Details.TotalCommissionPercentage IS NOT NULL 
--         AND Details.TotalCommissionPercentage <> 0 
--    THEN Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)
--    ELSE 0.00
--  END AS FLOAT
--) AS InferredPremiumCollected,
--		CASE     
--	  WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0    
--	  THEN (Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0))    
--	  ELSE 0.00    
--	  END AS TotalPremium,

--		CASE   
--			WHEN Details.TotalCommission IS NOT NULL   
--         AND Details.TotalCommission > 0   
--         AND Details.TotalCommissionPercentage IS NOT NULL   
--         AND Details.TotalCommissionPercentage > 0  
--         AND PDL.AgentCommission IS NOT NULL   
--         AND PDL.AgentCommission > 0  
--		THEN   
--        (Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))   
--        * (PDL.AgentCommission / 100.0)
--		ELSE 0.00  
--		END AS [Comm.],  

-- -- Comm. Paid To Date from PaymentApplications (summing applied commission payments)
--    --ISNULL((
--    --    SELECT SUM(PALI.AppliedAmount)
--    --    FROM Accounting_PaymentApplicationLineItems PALI
--    --     JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
--    --     JOIN Accounting_Payments P1 ON P1.Payment_Id = PA.Payment_Id AND P1.Status = 1
--    --    WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
--    --), 0.00) AS [Comm. Paid to Date],
--	ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],
--		AID.Name As [Insured Name],
--		PD.PolicyNumber AS [Policy Number],
--		CD.Name AS [Carrier],
--		LOB.Name AS [LOB],
--		PD.TransactionType AS [Transaction Type]  , 
    
--		--CASE
--		--WHEN Details.TotalAgentBrokerCommission IS NOT NULL
--		--AND Details.TotalAgentBrokerCommission > 0
--		--AND Details.TotalCommissionPercentage IS NOT NULL
--		--AND Details.TotalCommissionPercentage > 0
--		--THEN ROUND(
--		--(Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) 
--		--* (Details.TotalAgentBrokerCommission / 100.0),
--		--2
--		--)
--		--ELSE 0.00
--		--END AS CommsNetDue 
			
--				CASE
--		WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommission > 0
--			 AND Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage > 0
--			 AND PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission > 0
--		THEN 
--			((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))
--			  * (PDL.AgentCommission / 100.0))
--			- (Details.CommPaidToDate)
--		ELSE 0.00
--	END AS CommsNetDue



 
--    FROM Accounting_Invoices I   
	
--	 JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id     
--        AND PD.Status = 1     
--        AND PD.BillType = 'Direct'                    
    
--     JOIN Accounting_InvoiceDetails ID     
--        ON ID.Invoice_Id = I.Id     
--        AND ID.Status = 1                    
    
--     JOIN Accounting_LU_BillingType_LineItems_Directions BLD     
--        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id     
--        AND BLD.Status = 1                    
    
--     JOIN Accounting_LU_LineItems_Directions LD     
--        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id     
--        AND LD.Status = 1                    
    
--     JOIN Accounting_LU_LineItems LI     
--        ON LI.LineItem_Id = LD.LineItem_Id     
--        AND LI.Status = 1                            
    
--     JOIN Accounting_LU_LineItemTypes LIT     
--        ON LIT.LineItemType_Id = LI.LineItemType_Id     
--        AND LIT.Status = 1                    
    
--     JOIN Accounting_LU_Directions LUD     
--        ON LUD.Direction_Id = LD.Direction_Id     
--	AND LUD.Status = 1                    
    
--     JOIN Accounting_LU_LOBs LOB     
--        ON LOB.LOB_Id = ID.LOB_Id     
--        AND LOB.Status = 1    
	
--    Left JOIN Accounting_InsuredDetails AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1
--    LEft JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1
--	LEft JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =PD.AgentDetail_Id AND ABD.Status=1
--	--OUTER APPLY (
--	--	SELECT       
--	--		MAX(HouseCommission) AS HouseCommission,      
--	--		MAX(AgentCommission) AS AgentCommission      
--	--	FROM Accounting_PolicyDetailsLineItems      
--	--	WHERE Status = 1 AND PolicyDetails_Id = I.PolicyDetails_Id
--	--) PDL 
--		JOIN (SELECT PolicyDetails_Id,MAX(HouseCommission) AS HouseCommission,MAX(AgentCommission) AS AgentCommission      
--		FROM Accounting_PolicyDetailsLineItems WHERE Status = 1 GROUP BY PolicyDetails_Id       
--		) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id  

-- OUTER APPLY (        
--    SELECT   
	
--        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,      
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,    
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,
--		CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name = 'Accounts Payable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS CommPaidToDate, 
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,     
--        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,        
--        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission    
--  FROM (        
--        SELECT         
--            ID.Amount,        
--            ID.RemainingBalance,        
--            I.DueDate AS DueDateLocal,        
--            LIT.Name AS LIT_Name,        
--            LI.Code AS LI_Code,        
--            LI.Name AS LI_Name,        
--            LUD.Name AS LUD_Name  ,
--			ID.LOB_Id,
--			(ID.Amount - ID.RemainingBalance) AS TotalReceivable
--        FROM Accounting_InvoiceDetails ID        
--         JOIN Accounting_LU_BillingType_LineItems_Directions BLD         
--            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1        
--         JOIN Accounting_LU_LineItems_Directions LD         
--            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1        
--         JOIN Accounting_LU_LineItems LI         
--            ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1        
--         JOIN Accounting_LU_LineItemTypes LIT         
--            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1        
--         JOIN Accounting_LU_Directions LUD         
--            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1        
--         JOIN Accounting_LU_BillingTypes BT         
--            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1 and BT.Code='Direct'   
--        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1         
--    ) AS DetailView        
--) AS Details 

--    WHERE I.Status = 1                   
--      AND                
--	(                
--        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                
--        OR                
--        (                
--            @InvoiceDate = 'InvoiceEffectiveDate'                
--            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
--            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo                
--        )                
--        OR                
--        (                
--            @InvoiceDate = 'InvoiceCreatedDate'                
--            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                
--            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo                
--        ))                 
--  --AND (LIT.Code = 'Fee' OR LIT.Code = 'Tax')  
--  --AND LUD.Code = 'AR' 
--  AND  LUD.Code = 'IN' 
--  AND LIT.Code='Premium'           
--  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)              
--  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)           
--  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)            
--  AND (@LOB_Id IS NULL OR EXISTS (          
--    SELECT 1          
--    FROM Accounting_InvoiceDetails AID          
--    WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id)
--	)     
--	)     

--   , PaymentApplication AS (      
--    --SELECT *      
--    --FROM (      
--        SELECT       
--            I.PolicyDetails_Id,                    
--            I.Id AS Invoice_Id,       
--            ID.Id AS InvoiceDetail_Id,  
--			ABD.AgentDetail_Id,
--			ABD.Name AS [Agency Name],
--			CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],                  
--            'Comm. Paid' AS LineItemDescription,       
--            CONCAT('PMT', P.Payment_Id) AS IdNo,                   
--            CAST(P.CreatedOn AS DATE) AS EffectiveDate,                    
--            PD.ExpirationDate,                    
--            0 AS GrossAmount,  
--			0 AS AgentCommPercentage,        
--			--CASE   
--			--WHEN Details.TotalCommission IS NOT NULL   
--			--		AND Details.TotalCommissionPercentage IS NOT NULL   
--			--		AND Details.TotalCommissionPercentage <> 0  
--			--THEN (Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0))  
--			--ELSE 0.00  
--			--END AS NetAmount,   
--			--CAST(
--			--CASE 
--			--	WHEN P.PaymentDirection_Id = 1 
--			--	THEN -PALI.AppliedAmount 
--			--	ELSE ABS(PALI.AppliedAmount) 
--			--END 
--			--AS FLOAT) AS NetAmount,
     
--			Details.TotalPremium AS NetAmount,
--            I.DueDate,                    
--            PD.PolicyNumber,       
--            0 AS RemainingBalance,      
--			0 AS AgentCommissionAmt,   
--  --      CAST(     
--		--CASE          
--		--WHEN Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0      
--		--THEN Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0)         
--		--ELSE 0.00      
--		--END  AS FLOAT) 
--		--AS InferredPremiumCollected,  
--		0 AS InferredPremiumCollected,
--		CASE   
--			WHEN Details.TotalCommission IS NOT NULL   
--         AND Details.TotalCommission > 0   
--         AND Details.TotalCommissionPercentage IS NOT NULL   
--         AND Details.TotalCommissionPercentage > 0  
--         AND PDL.AgentCommission IS NOT NULL   
--         AND PDL.AgentCommission > 0  
--		THEN   
--        (Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))   
--        * (PDL.AgentCommission / 100.0)
--		ELSE 0.00  
--		END AS [Comm.] ,

--		  -- Comm. Paid To Date from PaymentApplications (summing applied commission payments)
--		      --ISNULL((
--		      --    SELECT SUM(PALI.AppliedAmount)
--		      --    FROM Accounting_PaymentApplicationLineItems PALI
--		      --    JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
--		      --    JOIN Accounting_Payments P1 ON P1.Payment_Id = PA.Payment_Id AND P1.Status = 1
--		      --    WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
--		      --), 0.00) AS [Comm. Paid to Date],
		  
	
--	ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],
--		AID.Name As [Insured Name],
--		PD.PolicyNumber AS [Policy Number],
--		CD.Name AS [Carrier],
--		LOB.Name AS [LOB],
--		PD.TransactionType AS [Transaction Type]  , 
--		--CAST(  CASE
--		--WHEN Details.TotalAgentBrokerCommission IS NOT NULL
--		--AND Details.TotalAgentBrokerCommission > 0
--		--AND Details.TotalCommissionPercentage IS NOT NULL
--		--AND Details.TotalCommissionPercentage > 0
--		--THEN ROUND(
--		--(Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) 
--		--* (Details.TotalAgentBrokerCommission / 100.0),
--		--2
--		--)
--		--ELSE 0.00
--		--END AS FLOAT) AS CommsNetDue,
--		CASE WHEN P.PaymentDirection_Id = 1 THEN -PALI.AppliedAmount ELSE ABS(PALI.AppliedAmount) END AS CommsNetDue,

--		0 AS TotalPremium,
--		ISNULL((
--		SELECT SUM(PALI.AppliedAmount)
--		FROM Accounting_PaymentApplicationLineItems PALI
--		JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
--		JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1
--		WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
--		), 0.00) AS TotalCommissionReceived
--        FROM Accounting_Invoices I      
--         JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 
--		 AND PD.BillType = 'Direct' 
		
--         JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1    
		
--         JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        
--		 JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1  
		
--         JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1      

--         JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1    
		
--         JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1 
		
--         JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1      

--         JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1  
		
--         JOIN Accounting_PaymentApplications PA ON PA.PaymentApplication_Id = PALI.PaymentApplication_Id AND PA.Status = 1  
		
--         JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1    
		 
--		LEFT JOIN Accounting_InsuredDetails AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1
--		LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1
--		LEft JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =PD.AgentDetail_Id AND ABD.Status=1
		 
		
--		OUTER APPLY (        
--		SELECT   
	
--        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,      
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,    
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,
--		CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name = 'Accounts Payable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS CommPaidToDate,    
--        CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,     
--        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,        
--        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission    
--		FROM (        
--        SELECT         
--            ID.Amount,        
--            ID.RemainingBalance,        
--            I.DueDate AS DueDateLocal,        
--            LIT.Name AS LIT_Name,        
--            LI.Code AS LI_Code,        
--            LI.Name AS LI_Name,        
--            LUD.Name AS LUD_Name  ,
--			ID.LOB_Id,
--			(ID.Amount - ID.RemainingBalance) AS TotalReceivable
--        FROM Accounting_InvoiceDetails ID        
--         JOIN Accounting_LU_BillingType_LineItems_Directions BLD         
--            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1        
--         JOIN Accounting_LU_LineItems_Directions LD         
--            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1        
--         JOIN Accounting_LU_LineItems LI         
--            ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1        
--         JOIN Accounting_LU_LineItemTypes LIT         
--            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1        
--         JOIN Accounting_LU_Directions LUD         
--            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1        
--         JOIN Accounting_LU_BillingTypes BT         
--            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1 -- and BT.Code='Direct'   
--        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1         
--    ) AS DetailView        
--) AS Details 
--		 JOIN (SELECT PolicyDetails_Id,MAX(HouseCommission) AS HouseCommission,MAX(AgentCommission) AS AgentCommission      
--		FROM Accounting_PolicyDetailsLineItems WHERE Status = 1 GROUP BY PolicyDetails_Id       
--		) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id      
--        WHERE       
--            I.Status = 1           
--            AND (      
--                @CompareDateFrom IS NULL OR @CompareDateTo IS NULL      
--                OR (      
--                    @InvoiceDate = 'InvoiceEffectiveDate'      
--                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
--                    AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo      
--                )      
--                OR (      
--                    @InvoiceDate = 'InvoiceCreatedDate'      
--                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
--                    AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo      
--                )      
--            )   
			
--            AND LUD.Code = 'AR' 
--            AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
--            AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)      
--            AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
--            AND (@LOB_Id IS NULL OR EXISTS (      
--                SELECT 1      
--                FROM Accounting_InvoiceDetails AID      
--                WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id      
--            ))   
--)

----SELECT Distinct
----    PD.Id AS PolicyDetails_Id,
----    I.Id                        AS Invoice_Id,
----    ID.Id                       AS InvoiceDetail_Id,
----    CONCAT(PD.TransactionType, ' ', LI.Name) AS LineItemDescription,
----    I.InvoiceNo                 AS IdNo,
----    PD.EffectiveDate,
----    PD.ExpirationDate,
----    ID.Amount AS GrossAmount,
----    CASE WHEN PDL.AgentCommission IS Null THen 0 ELSE PDL.AgentCommission
----		END AS AgentCommPercentage,

----    APCID.AgentCommission       AS AgentCommissionAmt, -- matches FeeData/PaymentApp
----    CASE   
----    WHEN Details.TotalCommission IS NOT NULL   
----         AND Details.TotalCommissionPercentage IS NOT NULL   
----         AND Details.TotalCommissionPercentage <> 0  
----    THEN 
----        CAST(Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0) AS FLOAT)  
----    ELSE 
----        CAST(0.00 AS FLOAT)  
----	END AS NetAmount,
----    I.DueDate,
----    PD.PolicyNumber,
----    ID.RemainingBalance,
----	CAST(     
----		CASE          
----		WHEN Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0      
----		THEN Details.TotalCommissionReceived  / (Details.TotalCommissionPercentage / 100.0)         
----		ELSE 0.00      
----		END  AS FLOAT) 
----		AS InferredPremiumCollected, 
----    APCID.AgentCommission       AS [Comm.],
----     ISNULL((
----		          SELECT SUM(PALI.AppliedAmount)
----		          FROM Accounting_PaymentApplicationLineItems PALI
----		          JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
----		          JOIN Accounting_Payments P1 ON P1.Payment_Id = PA.Payment_Id AND P1.Status = 1
----		          WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
----		      ), 0.00) AS [Comm. Paid to Date],

----    AID.Name                          AS [Insured Name],
----    PD.PolicyNumber             AS [Policy Number],
----    CD.Name                          AS [Carrier],
----    LOB.Name                        AS [LOB],
----    PD.TransactionType          AS [Transaction Type],
----	CAST(  CASE
----	WHEN Details.TotalAgentBrokerCommission IS NOT NULL
----	AND Details.TotalAgentBrokerCommission > 0
----	AND Details.TotalCommissionPercentage IS NOT NULL
----	AND Details.TotalCommissionPercentage > 0
----	THEN ROUND(
----	(Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) 
----	* (Details.TotalAgentBrokerCommission / 100.0),
----	2
----	)
----	ELSE 0.00
----	END AS FLOAT) AS CommsNetDue,
----    Details.TotalCommission + Details.TotalPremium AS TotalPremium,
----	ISNULL((
----    SELECT SUM(PALI.AppliedAmount)
----    FROM Accounting_PaymentApplicationLineItems PALI
----    JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1
----    JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1
----    WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1
----), 0.00) AS TotalCommissionReceived

----FROM Accounting_Invoices I      
----         JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 
----		 AND PD.BillType = 'Direct' 
		
----         JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1    
		
----         JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        
----		 JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1  
		
----         JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1      

----         JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1    
		
----         JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1 
		
----         JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1      

----         JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1  
		
----         JOIN Accounting_PaymentApplications PA ON PA.PaymentApplication_Id = PALI.PaymentApplication_Id AND PA.Status = 1  
		
----         JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1    
		 
----		LEFT JOIN Accounting_InsuredDetails AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1
----		LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1
		
----		 JOIN (SELECT PolicyDetails_Id,MAX(HouseCommission) AS HouseCommission,MAX(AgentCommission) AS AgentCommission      
----		FROM Accounting_PolicyDetailsLineItems WHERE Status = 1 GROUP BY PolicyDetails_Id       
----		) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id      
       
----		 JOIN AggregatedPremiumCommissionInvoiceDetails APCID ON APCID.InvoiceId = I.Id
		 
 
---- OUTER APPLY (        
----    SELECT   
	
----        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal'  AND  LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,      
----        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,    
----        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,   
----        CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,     
----        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,        
----        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission    
----  FROM (        
----        SELECT         
----            ID.Amount,        
----            ID.RemainingBalance,        
----            I.DueDate AS DueDateLocal,        
----            LIT.Name AS LIT_Name,        
----            LI.Code AS LI_Code,        
----            LI.Name AS LI_Name,        
----            LUD.Name AS LUD_Name  ,
----			ID.LOB_Id,
----			(ID.Amount - ID.RemainingBalance) AS TotalReceivable
----        FROM Accounting_InvoiceDetails ID        
----         JOIN Accounting_LU_BillingType_LineItems_Directions BLD         
----            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1        
----         JOIN Accounting_LU_LineItems_Directions LD         
----            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1        
----         JOIN Accounting_LU_LineItems LI         
----            ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1        
----         JOIN Accounting_LU_LineItemTypes LIT         
----            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1        
----         JOIN Accounting_LU_Directions LUD         
----            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1        
----         JOIN Accounting_LU_BillingTypes BT         
----            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1  --and BT.Code='Direct'   
----        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1         
----    ) AS DetailView        
----) AS Details 

----		WHERE I.Status = 1
----		AND (      
----                @CompareDateFrom IS NULL OR @CompareDateTo IS NULL      
----                OR (      
----                    @InvoiceDate = 'InvoiceEffectiveDate'      
----                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
----                    AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo      
----                )      
----                OR (      
----                    @InvoiceDate = 'InvoiceCreatedDate'      
----                    AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL      
----                    AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo      
----                )      
----            )   
			
----            AND LUD.Code = 'AR' 
----            AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
----            AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)      
----            AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
----            AND (@LOB_Id IS NULL OR EXISTS (      
----                SELECT 1      
----                FROM Accounting_InvoiceDetails AID      
----                WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id      
----            ))   

----UNION ALL

--SELECT Distinct
--    PolicyDetails_Id,
--    Invoice_Id,
--    InvoiceDetail_Id,
--    LineItemDescription,
--    IdNo,
--    EffectiveDate,
--    ExpirationDate,
--    GrossAmount,
--    AgentCommissionAmt,
--    AgentCommPercentage ,
--    CAST(ISNULL(NetAmount, 0.00) AS FLOAT) AS NetAmount,
--    DueDate,
--    PolicyNumber,
--    RemainingBalance,
--    InferredPremiumCollected,
--    [Comm.],
--    [Comm. Paid to Date],
--    [Insured Name],
--    [Policy Number],
--    [Carrier],
--    [LOB],
--    [Transaction Type],
--    CommsNetDue,
--    TotalPremium,
--	TotalCommissionReceived,
--	AgentDetail_Id,
--	[Agency Name],
--	[Agency Address]
--FROM FeeData 

--UNION ALL

--SELECT Distinct
--    PolicyDetails_Id,
--    Invoice_Id,
--    InvoiceDetail_Id,
--    LineItemDescription,
--    IdNo,
--    EffectiveDate,
--    ExpirationDate,
--    GrossAmount,
--    AgentCommissionAmt,
--    0 AS AgentCommPercentage,
--    CAST(ISNULL(0, 0.00) AS FLOAT) AS NetAmount,
--    DueDate,
--    PolicyNumber,
--    RemainingBalance,
--    0 AS InferredPremiumCollected,
--    [Comm.],
--    [Comm. Paid to Date],
--    [Insured Name],
--    [Policy Number],
--    [Carrier],
--    [LOB],
--    [Transaction Type],
--    CommsNetDue,
--	TotalCommissionReceived,
--    0 AS TotalPremium,
--	AgentDetail_Id,
--	[Agency Name],
--	[Agency Address]
--FROM PaymentApplication
--;

--	END



--INSERT INTO Accounting_LU_ClientLevelParameters (
--    Client_Id,
--    Service_Id,
--    Parameter_Id,
--    Value
--)
--VALUES 
--(10003, 1, 2, '9341455036679173'),
--(10003, 1, 4, 'AB19sbW4AU6UiaheA8jkDsbMbMtU1l4Bg7w9Sy6SWQHnHwpDZ5'),
--(10003, 1, 5, 'BhsSaMudxTgBlDJ1Ao1C0KGLGbwuVDH20AYmJ0bF'),
--(10003, 1, 7, 'RT1-196-H0-1761816025ycal6ti9atbdirbzdne5');