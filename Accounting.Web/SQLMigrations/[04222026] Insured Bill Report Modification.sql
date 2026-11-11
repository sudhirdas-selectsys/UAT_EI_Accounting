--alter PROC [dbo].[USP_Accounting_InsuredBilledCommissionStatementReport]         
--(                                                                                                          
-- @Client_Id bigint = null,                                                                                                         
-- --@AgentDetail_Id BIGINT = NULL,            
-- @AgentDetail_Id NVARCHAR(MAX) = NULL,            
-- @InvoiceDate VARCHAR(100) = NULL,                                                                                                          
-- @InvoiceCreatedFromDate VARCHAR(200) = NULL,                                                                                                          
-- @InvoiceCreatedToDate VARCHAR(200) = NULL,                                                                                                          
-- @InvoiceEffectiveFromDate VARCHAR(200) = NULL,                                                                                                        
-- @InvoiceEffectiveToDate VARCHAR(200) = NULL,                                                                                                          
-- @Range VARCHAR(200) = NULL ,                                                                                                         
-- @WholesalerDetail_Id BIGINT = NULL,                                      
-- @InvoiceNumber VARCHAR(200) = NULL,                                           
-- @CarrierDetail_Id BIGINT = NULL,                                                                                                      
-- @LOB_Id INT = NULL   ,                                                                                      
-- @OutstandingBalances bit = null   ,                                                               
-- @As_Of_Date VARCHAR(200) = NULL,          
-- @ParentAgentDetail_Id BIGINT = NULL,                                                      
-- @Simplified bit = null,  
-- @LOB_Carrier_Grouping_Id int = NULL  
--  )                                                                                                          
--  as                                                                                                          
--BEGIN                                                                                                          
                                                                                                
--    DECLARE @CompareDateFrom DATETIME;                                                                                                          
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
       
-- -- Cache the Data at Once Start      
-- DECLARE @TempAgencyHierarchy_InsuredBill TABLE (AgentDetail_Id BIGINT);      
-- DECLARE @TempAgentIds_InsuredBill TABLE (AgentDetail_Id BIGINT);      
      
-- DROP TABLE IF EXISTS #TempBasePolicyDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempAccounting_InsuredDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempBasePayments_InsuredBill;      
-- DROP TABLE IF EXISTS #TempValidPaymentIds_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPaymentTotals_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPolicyLOBs_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPDL_Agg_InsuredBill;      
-- DROP TABLE IF EXISTS #TempTotalPremiumCTE_InsuredBill;      
-- DROP TABLE IF EXISTS #TempInvoiceDetailView_InsuredBill;      
-- DROP TABLE IF EXISTS #TempFeeData_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPaymentApplication_InsuredBill;
-- DROP TABLE IF EXISTS #TempLOBCarrierGroupingInvoices_InsuredBill;
      
--    CREATE TABLE #TempLOBCarrierGroupingInvoices_InsuredBill (Invoice_Id BIGINT);
      
-- -- #TempAgencyHierarchy       
-- IF @ParentAgentDetail_Id IS NOT NULL      
-- BEGIN      
--     INSERT INTO @TempAgencyHierarchy_InsuredBill (AgentDetail_Id)      
--     SELECT AgentDetail_Id FROM dbo.GetAgencyHierarchy(@ParentAgentDetail_Id);      
-- END      
      
-- -- #TempAgentIds       
-- IF @AgentDetail_Id IS NOT NULL      
-- BEGIN      
--     INSERT INTO @TempAgentIds_InsuredBill (AgentDetail_Id)      
--     SELECT TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT) FROM STRING_SPLIT(@AgentDetail_Id, ',')      
--     WHERE TRY_CAST(LTRIM(RTRIM(value)) AS BIGINT) IS NOT NULL;      
-- END      
      
-- -- #TempBasePolicyDetails_InsuredBill Start                                   
-- SELECT IES.PolicyDetails_Id,IES.InvoiceNo,IES.Id AS Invoice_Id,IES.Amount AS InvoicesAmount,IES.RemainingBalance AS InvoicesRemainingBalance,      
-- IES.DueDate AS InvoicesDueDate,IES.CreatedOn AS InvoicesCreatedOn,      
-- APD.Client_Id,APD.PolicyNumber,APD.AgentDetail_Id,APD.BillType,APD.CarrierDetail_Id,APD.InsuredDetail_Id,APD.EffectiveDate,APD.ExpirationDate,APD.WholesalerDetail_Id,APD.TransactionType,      
-- APD.EndorsementNumber,APD.CreatedOn AS PolicyDetailsCreatedOn,      
-- CD.Name AS CarrierName      
-- INTO #TempBasePolicyDetails_InsuredBill      
-- FROM Accounting_PolicyDetails APD      
-- INNER JOIN Accounting_Invoices IES ON APD.Id = IES.PolicyDetails_Id AND IES.Status = 1      
-- LEFT JOIN Accounting_CarrierDetails CD ON APD.CarrierDetail_Id = CD.CarrierDetail_Id AND CD.Status = 1      
-- WHERE APD.Status = 1 AND APD.Client_Id = @Client_Id AND APD.BillType = 'Insured'      
      
-- CREATE NONCLUSTERED INDEX NC_TempBasePolicyDetails_InsuredBill_ClientPolicy ON #TempBasePolicyDetails_InsuredBill (Client_Id, PolicyDetails_Id)      
-- INCLUDE (InvoiceNo, Invoice_Id, InvoicesAmount, InvoicesRemainingBalance, InvoicesDueDate);      
      
-- --#TempBasePolicyDetails_InsuredBill End      
      
-- -- #TempAccounting_InsuredDetails_InsuredBill Start                                   
-- SELECT AINSD.* INTO #TempAccounting_InsuredDetails_InsuredBill      
-- FROM Accounting_InsuredDetails AINSD      
-- INNER JOIN (SELECT DISTINCT InsuredDetail_Id FROM #TempBasePolicyDetails_InsuredBill) TBPD ON AINSD.InsuredDetail_Id = TBPD.InsuredDetail_Id      
-- WHERE AINSD.Client_Id = @Client_Id AND AINSD.Status = 1      
      
-- CREATE NONCLUSTERED INDEX NC_TempAccounting_InsuredDetails_InsuredBill ON #TempAccounting_InsuredDetails_InsuredBill (Client_Id, InsuredDetail_Id)      
-- -- #TempAccounting_InsuredDetails_InsuredBill End      
      
-- -- #TempAccounting_InvoiceDetails_InsuredBill Start                                   
-- SELECT AID.* INTO #TempAccounting_InvoiceDetails_InsuredBill      
-- FROM Accounting_InvoiceDetails AID      
-- INNER JOIN #TempBasePolicyDetails_InsuredBill TBPD ON AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1      
      
-- CREATE NONCLUSTERED INDEX NC_TempAccounting_InvoiceDetails_InsuredBill ON #TempAccounting_InvoiceDetails_InsuredBill (Id, Invoice_Id,BillingType_LineItems_Directions_Id)         
-- -- #TempAccounting_InvoiceDetails_InsuredBill End      
      
-- --#TempBasePayments_InsuredBill Start      
-- SELECT P.Payment_Id,P.PaymentDate,P.Status AS Payment_Status,P.PaymentDirection_Id,PA.PaymentApplication_Id,      
--    PA.Status AS PA_Status,PALI.PaymentApplicationLineItem_Id AS PaymentApplicationLineItem_Id,PALI.InvoiceLineItem_Id,PALI.AppliedAmount,PALI.Status AS PALI_Status,      
--    TID.Invoice_Id AS Invoice_Id,TID.Id AS InvoiceDetail_Id      
-- INTO #TempBasePayments_InsuredBill      
-- FROM Accounting_Payments P      
-- JOIN Accounting_PaymentApplications PA ON PA.Payment_Id = P.Payment_Id AND PA.Status = 1      
-- JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PALI.Status = 1      
-- JOIN #TempAccounting_InvoiceDetails_InsuredBill TID ON TID.Id= PALI.InvoiceLineItem_Id      
-- WHERE P.Status = 1;      
      
-- CREATE NONCLUSTERED INDEX NC_TempBasePayments_InsuredBill ON #TempBasePayments_InsuredBill (Invoice_Id, Payment_Id) INCLUDE (PaymentDate, PaymentDirection_Id, AppliedAmount, InvoiceLineItem_Id);      
-- --#TempBasePayments_InsuredBill End      
      
-- --#TempPaymentTotals_InsuredBill Start      
-- SELECT InvoiceLineItem_Id,SUM(AppliedAmount) AS TotalAppliedAmount      
-- INTO #TempPaymentTotals_InsuredBill      
-- FROM #TempBasePayments_InsuredBill      
-- GROUP BY InvoiceLineItem_Id;      
      
-- CREATE NONCLUSTERED INDEX NC_TempPaymentTotals ON #TempPaymentTotals_InsuredBill (InvoiceLineItem_Id);      
-- --#TempPaymentTotals_InsuredBill End      
      
-- --#TempValidPaymentIds_InsuredBill Start      
-- SELECT DISTINCT TBP1.Payment_Id      
-- INTO #TempValidPaymentIds_InsuredBill      
-- FROM #TempBasePayments_InsuredBill TBP1      
-- JOIN #TempBasePolicyDetails_InsuredBill TBPD2 ON TBP1.Invoice_Id = TBPD2.Invoice_Id      
-- WHERE (@AgentDetail_Id IS NULL OR TBPD2.AgentDetail_Id IN (SELECT AgentDetail_Id FROM @TempAgentIds_InsuredBill))            
-- AND (@ParentAgentDetail_Id IS NULL OR TBPD2.AgentDetail_Id IN (SELECT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill));      
       
-- CREATE NONCLUSTERED INDEX NC_TempValidPaymentIds_InsuredBill ON #TempValidPaymentIds_InsuredBill (Payment_Id);      
-- --#TempValidPaymentIds_InsuredBill End     
 
-- -- #TempLOBCarrierGroupingInvoices_InsuredBill Start
--    IF @LOB_Carrier_Grouping_Id IS NOT NULL
--    BEGIN
--        INSERT INTO #TempLOBCarrierGroupingInvoices_InsuredBill (Invoice_Id)
--        SELECT DISTINCT TBPD.Invoice_Id
--        FROM #TempBasePolicyDetails_InsuredBill TBPD
--        JOIN #TempAccounting_InvoiceDetails_InsuredBill TID ON TID.Invoice_Id = TBPD.Invoice_Id
--        JOIN Accounting_LOB_Carrier_Grouping LCG 
--            ON LCG.LOB_Carrier_Grouping_Id = @LOB_Carrier_Grouping_Id 
--            AND LCG.Status = 1 
--            AND LCG.Client_Id = @Client_Id
--        JOIN Accounting_Carrier_GroupingDetails CG 
--            ON LCG.LOB_Carrier_Grouping_Id = CG.LOB_Carrier_Grouping_Id 
--            AND CG.Status = 1
--        LEFT JOIN Accounting_LOB_GroupingDetails LG 
--            ON LG.LOB_Carrier_Grouping_Id = LCG.LOB_Carrier_Grouping_Id 
--            AND LG.Status = 1
--        WHERE (LG.LOB_Id IS NULL OR TID.LOB_Id = LG.LOB_Id);
--    END
--    ELSE
--    BEGIN
--        INSERT INTO #TempLOBCarrierGroupingInvoices_InsuredBill (Invoice_Id)
--        SELECT DISTINCT Invoice_Id FROM #TempBasePolicyDetails_InsuredBill;
--    END

--    CREATE NONCLUSTERED INDEX NC_TempLOBCarrierGroupingInvoices_InsuredBill 
--        ON #TempLOBCarrierGroupingInvoices_InsuredBill (Invoice_Id);
--    -- #TempLOBCarrierGroupingInvoices_InsuredBill End
                                                       
--    SELECT * FROM Accounting_AgentDetails                                                            
--    WHERE Client_Id = @Client_Id AND Status = 1                                         
-- -- AND (@AgentDetail_Id IS NULL OR AgentDetail_Id = @AgentDetail_Id);             
--    AND (@AgentDetail_Id IS NULL OR AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InsuredBill))            
--    AND (@ParentAgentDetail_Id IS NULL OR AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill))      
        
                                   
-- SELECT  APD.*                                                                                                 
-- FROM Accounting_PolicyDetails APD       
-- INNER JOIN (SELECT DISTINCT PolicyDetails_Id FROM #TempBasePolicyDetails_InsuredBill) TBPD ON APD.Id = TBPD.PolicyDetails_Id AND APD.Client_Id = @Client_Id AND APD.Status = 1      
-- WHERE APD.BillType ='Insured'      
-- --AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)            
--   AND (@AgentDetail_Id IS NULL OR APD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InsuredBill))          
-- AND (@ParentAgentDetail_Id IS NULL OR APD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill))         
-- AND (@WholesalerDetail_Id IS NULL OR APD.WholesalerDetail_Id = @WholesalerDetail_Id)                                                
-- AND (@CarrierDetail_Id IS NULL OR APD.CarrierDetail_Id = @CarrierDetail_Id)          
       
-- SELECT DISTINCT TBPD.PolicyNumber,TBPD.AgentDetail_Id,IND.Name AS InsuredName,LOB.Name AS LOBName, CD.Name AS CarrierName                  
-- INTO #TempPolicyLOBs_InsuredBill      
-- FROM #TempBasePolicyDetails_InsuredBill TBPD      
-- JOIN #TempAccounting_InsuredDetails_InsuredBill IND ON IND.InsuredDetail_Id = TBPD.InsuredDetail_Id AND IND.Status = 1                                                                                                      
-- JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = TBPD.CarrierDetail_Id AND CD.Status = 1                                          
-- JOIN #TempAccounting_InvoiceDetails_InsuredBill ID ON ID.Invoice_Id = TBPD.Invoice_Id AND ID.Status = 1                                                                                                      
-- JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                                                                                                      
-- WHERE       
-- --AND (@AgentDetail_Id IS NULL OR TBPD.AgentDetail_Id = @AgentDetail_Id)            
-- (@AgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InsuredBill))             
-- AND (@ParentAgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill))         
-- AND (@WholesalerDetail_Id IS NULL OR TBPD.WholesalerDetail_Id = @WholesalerDetail_Id)                                      
-- -- AND (@InvoiceNumber IS NULL OR TBPD.InvoiceNo=@InvoiceNumber)              
--     AND (@InvoiceNumber IS NULL OR TBPD.InvoiceNo = @InvoiceNumber OR TBPD.InvoiceNo LIKE '%' + @InvoiceNumber + '%'              
--    OR TRY_CAST(REPLACE(TBPD.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))              
-- AND (@CarrierDetail_Id IS NULL OR TBPD.CarrierDetail_Id = @CarrierDetail_Id)                                        
-- --AND (@As_Of_Date IS NULL OR TBPD.EffectiveDate <= @As_Of_Date)                                                            
-- AND (@LOB_Id IS NULL OR EXISTS (SELECT 1 FROM Accounting_InvoiceDetails AID                                        
-- WHERE AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))        
      
-- CREATE NONCLUSTERED INDEX NC_TempPolicyLOBs_InsuredBill ON #TempPolicyLOBs_InsuredBill (PolicyNumber, InsuredName)      
                                                                   
-- SELECT PolicyNumber,InsuredName,AgentDetail_Id,CarrierName,                                                                            
-- STUFF((SELECT DISTINCT ', ' + L2.LOBName FROM #TempPolicyLOBs_InsuredBill L2 WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName                                                                                                     
 
-- FOR XML PATH(''), TYPE                                                                                                      
-- ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames                                                                        
-- FROM #TempPolicyLOBs_InsuredBill L1                                      
-- GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;              
       
      
-- SELECT PDLI.PolicyDetails_Id,MAX(PDLI.AgentCommission) AS AgentCommission,MAX(PDLI.HouseCommission) AS HouseCommission      
-- INTO #TempPDL_Agg_InsuredBill      
-- FROM Accounting_PolicyDetailsLineItems PDLI      
-- INNER JOIN #TempBasePolicyDetails_InsuredBill TBPD ON PDLI.PolicyDetails_Id = TBPD.PolicyDetails_Id      
-- WHERE Status = 1 AND AgentCommission > 0                                 
--    GROUP BY PDLI.PolicyDetails_Id          
      
-- CREATE NONCLUSTERED INDEX NC_TempPDL_Agg_InsuredBill ON #TempPDL_Agg_InsuredBill (PolicyDetails_Id)      
      
-- SELECT SUM(PDLI.Premium) AS TotalPremium,TBPD.PolicyDetails_Id      
-- INTO #TempTotalPremiumCTE_InsuredBill      
-- FROM Accounting_PolicyDetailsLineItems PDLI          
-- JOIN (SELECT DISTINCT PolicyDetails_Id FROM #TempBasePolicyDetails_InsuredBill)TBPD ON PDLI.PolicyDetails_Id= TBPD.PolicyDetails_Id AND PDLI.Status=1                
-- WHERE PDLI.LineType='PREMIUM' GROUP BY TBPD.PolicyDetails_Id      
      
-- CREATE NONCLUSTERED INDEX NC_TempTotalPremiumCTE_InsuredBill ON #TempTotalPremiumCTE_InsuredBill (PolicyDetails_Id)      
      
-- SELECT      
--    ID.Id AS InvoiceDetail_Id,      
--    ID.Invoice_Id,      
--    ID.Amount,      
--    ID.RemainingBalance,      
--    ID.LOB_Id,      
--    (ID.Amount - ID.RemainingBalance) AS TotalReceivable,      
-- LIT.Code AS LIT_Code,      
--    LIT.Name AS LIT_Name,      
--    LI.Code  AS LI_Code,      
--    LI.Name  AS LI_Name,      
-- LUD.Code AS LUD_Code,      
--    LUD.Name AS LUD_Name,      
--    BT.Code  AS BT_Code      
-- INTO #TempInvoiceDetailView_InsuredBill      
-- FROM #TempAccounting_InvoiceDetails_InsuredBill ID      
-- JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
-- JOIN Accounting_LU_LineItems_Directions LD  ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
-- JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1 AND LIC.Client_Id = @Client_Id      
-- JOIN Accounting_LU_LineItems LI  ON LI.LineItem_Id  = LIC.LineItem_Id AND LI.Status = 1      
-- JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1      
-- JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1      
-- JOIN Accounting_LU_BillingTypes BT  ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1 AND BT.Code = 'Insured';      
      
-- CREATE NONCLUSTERED INDEX NC_TempInvoiceDetailView_Invoice ON #TempInvoiceDetailView_InsuredBill (Invoice_Id, InvoiceDetail_Id)       
-- INCLUDE (Amount, RemainingBalance, TotalReceivable, LIT_Name, LI_Code,LIT_Code, LI_Name, LUD_Name, BT_Code,LUD_Code);      
      
      
-- SELECT I.PolicyDetails_Id,I.Id AS Invoice_Id,ID.Id AS InvoiceDetail_Id,ABD.AgentDetail_Id,                                                                                      
--    ABD.Name AS [Agency Name],                                                                                      
--    CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],                    
--    CASE WHEN DV.LI_Name = 'Carrier Premium' THEN CONCAT(PD.TransactionType, ' ','Premium')                                                                                      
--    ELSE CONCAT(PD.TransactionType, ' ', DV.LI_Name)                                                                                      
--    END AS LineItemDescription,                                                                                                         
--    I.InvoiceNo AS IdNo,                                                                            
--    PD.EffectiveDate,                                                                                                          
--    PD.ExpirationDate,                                                                                                          
--    ID.Amount AS GrossAmount,                                                                                           
--    ISNULL(ROUND(ID.Amount * (PDL.AgentCommission / 100.0), 2), 0.00) AS AgentCommissionAmt,                                                                    
--    CASE WHEN PDL.AgentCommission IS Null THen 0 ELSE PDL.AgentCommission                                                                                      
--    END AS AgentCommPercentage,                            
--    --CASE                        
--    --  WHEN ISNULL(Details.TotalPremium, 0) > 0 THEN Details.TotalPremium                        
--    --  WHEN ISNULL(Details.TotalInsuredPremium, 0) > 0 THEN Details.TotalInsuredPremium                        
--    --  ELSE 0                        
--    --END AS NetAmount,          
           
-- --CASE  WHEN ISNULL(Details.TotalPremium, 0) <> 0 THEN Details.TotalPremium             
-- --WHEN ISNULL(Details.TotalInsuredPremium, 0) <> 0 THEN Details.TotalInsuredPremium              
-- --ELSE 0 END AS NetAmount,          
-- TPCTE.TotalPremium AS NetAmount,          
--    --CASE WHEN Details.TotalPremium IS Null THen 0 ELSE Details.TotalPremium END AS NetAmount,                                      
-- I.DueDate,                                          
--    PD.PolicyNumber,                                                                                              
--    ID.RemainingBalance AS RemainingBalance,                                                                                    
--    ISNULL((                                                                                      
--     SELECT SUM(TBP1.AppliedAmount)      
--  FROM #TempBasePayments_InsuredBill TBP1      
--  WHERE TBP1.InvoiceLineItem_Id = ID.Id                                                                             
--    ), 0.00) AS TotalCommissionReceived,                                                                              
                                                                            
--    -- Inferred Premium Collected                                                                                      
-- CAST(CASE WHEN Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0                                                                                       
--  THEN ISNULL(ROUND(Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0), 2), 0.00)                                                                                      
--  ELSE 0.00 END AS FLOAT ) AS InferredPremiumCollected,                                                                                                                                                
-- CASE WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0                                                                                          
--  THEN ISNULL(ROUND((Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0)),2),0.00) ELSE 0.00 END AS TotalPremium,                   
-- CASE WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommission <> 0 AND Details.TotalCommissionPercentage IS NOT NULL                                                                                         
--  AND Details.TotalCommissionPercentage <> 0 AND PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission <> 0                                                                                        
--  THEN ISNULL(ROUND((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) * (PDL.AgentCommission / 100.0),2),0.00)                                           
--  ELSE 0.00 END AS [Comm.],                                                                                   
-- ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],                                                                                      
--    AID.Name As [Insured Name],                                                                                      
--    PD.PolicyNumber AS [Policy Number],                                                                              
-- CD.CarrierDetail_Id,                                                                                        
--    CD.Name AS [Carrier],                                                                                      
--    LOB.Name AS [LOB],                                                                                      
--    PD.TransactionType AS [Transaction Type]  ,                                                                                   
--    CASE WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommission <> 0 AND Details.TotalCommissionPercentage IS NOT NULL                       
--  AND Details.TotalCommissionPercentage <> 0 AND PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission <> 0                     
--  THEN ISNULL(ROUND((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) * (PDL.AgentCommission / 100.0) ,2),0.00)                                                                                                                  
 
    
                                                   
--  ELSE 0.00 END AS CommsNetDue,       
-- CASE WHEN Details.TotalCommissionReceived IS NOT NULL AND Details.TotalCommissionReceived <> 0                                                              
--  AND Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0 AND PDL.AgentCommission IS NOT NULL                       
--  AND PDL.AgentCommission <> 0 THEN                                                                       
--  ISNULL(ROUND((((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) * (PDL.AgentCommission / 100.0)) - (Details.CommPaidToDate)),2),0.00)                                                                    
--  ELSE 0.00 END AS [Comm. Due],                        
-- PD.EndorsementNumber                        
-- INTO #TempFeeData_InsuredBill      
--    FROM Accounting_Invoices I                                                                                                                                           
--    LEFT JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1           
-- LEFT JOIN #TempTotalPremiumCTE_InsuredBill TPCTE on TPCTE.PolicyDetails_Id = PD.Id            
-- LEFT JOIN #TempPDL_Agg_InsuredBill PDL ON PDL.PolicyDetails_Id = PD.Id                                             
-- LEFT JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1                                            
-- LEFT JOIN #TempInvoiceDetailView_InsuredBill DV ON DV.InvoiceDetail_Id = ID.Id                                                       
--    LEFT JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                                                                                                            
--    LEFT JOIN #TempAccounting_InsuredDetails_InsuredBill AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1                                                                                      
--    LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1                                                                                      
-- LEFT JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =PD.AgentDetail_Id AND ABD.Status=1                                                                                                                                                              
-- OUTER APPLY (                                
--  SELECT                                                 
--   CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalInsuredPremium,                                          
--   CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,                                                                                           
--   CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,                                          
--   CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,                                                                                      
--   CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name = 'Accounts Payable' THEN ISNULL(PaidAmount,0) ELSE 0 END ) AS DECIMAL(18,2)) AS CommPaidToDate,                            
--   CAST(CASE WHEN (SUM(CASE WHEN LIT_Name = 'PREMIUM' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) + SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='AccountReceivable'           
--   THEN Amount ELSE 0 END)) = 0 THEN 0 ELSE SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) /           
--   (SUM(CASE WHEN LIT_Name = 'PREMIUM' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) + SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable'                    
--   THEN Amount ELSE 0 END)) * 100 END AS DECIMAL(18,2)) AS TotalCommissionPercentage  ,                                                                                        
--   CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,                                                  
--   CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission                                                            
--  FROM (                                                           
--   SELECT                                                          
--            DV2.Amount,                                                                                              
--            DV2.RemainingBalance,                                                                          
--            I.DueDate AS DueDateLocal,                                                                                              
--            DV2.LIT_Name AS LIT_Name,                                                                                              
--            DV2.LI_Code AS LI_Code,                                                                                              
--            DV2.LI_Name AS LI_Name,                                                                                              
--            DV2.LUD_Name AS LUD_Name  ,                               
--   DV2.LOB_Id,                                                              
--   DV2.TotalReceivable ,                  
--   CASE WHEN @As_Of_Date IS NULL THEN DV2.TotalReceivable ELSE ISNULL(P.PaidAmount,0) END AS PaidAmount                                                                  
--   FROM #TempInvoiceDetailView_InsuredBill DV2                  
--   OUTER APPLY (                 
--    SELECT SUM(TBP1.AppliedAmount) AS PaidAmount                 
--    FROM #TempBasePayments_InsuredBill TBP1      
--    WHERE TBP1.InvoiceLineItem_Id = DV2.InvoiceDetail_Id AND (@As_Of_Date IS NULL OR TBP1.PaymentDate <= @As_Of_Date)                
--   ) P                
--   WHERE DV2.Invoice_Id = I.Id AND ID.Status = 1                                                                                               
--  ) AS DetailView                                                                                              
-- ) AS Details                                                                                                                            
--    WHERE I.Status = 1 AND PD.BillType='Insured'      
-- AND  DV.LUD_Code IN ('IN','AR')                                                                                        
-- AND                                  
-- (                                  
--       DV.LIT_Code = 'Premium'                                  
--    --OR (PD.BillType = 'Insured' AND LI.Code = 'AgencyBrokerCommission')                                  
-- )                                           
-- --DV.LIT_Code = 'Commission'                                  
-- --AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)            
-- AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InsuredBill))        
-- AND (@ParentAgentDetail_Id IS NULL OR PD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill))         
-- AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)                                      
-- --AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)              
--    AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber OR I.InvoiceNo LIKE '%' + @InvoiceNumber + '%'              
--    OR TRY_CAST(REPLACE(I.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))              
-- AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)                                                             
-- AND (@LOB_Id IS NULL OR EXISTS (SELECT 1 FROM Accounting_InvoiceDetails AID WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))                                                                                                              
-- AND (@As_Of_Date IS NULL OR PD.EffectiveDate <= @As_Of_Date)         
-- AND EXISTS(SELECT 1 FROM #TempLOBCarrierGroupingInvoices_InsuredBill LCGI WHERE LCGI.Invoice_Id = I.Id);         
      
      
-- SELECT  TBPD.PolicyDetails_Id,TBPD.Invoice_Id AS Invoice_Id,ID.Id AS InvoiceDetail_Id,ABD.AgentDetail_Id,                                                                                      
-- ABD.Name AS [Agency Name],                                                                                      
-- CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],                                                                                                        
--    'Comm. Paid' AS LineItemDescription,                                                                              
--    CONCAT('PMT', TBP.Payment_Id) AS IdNo,                                                                       
--    CAST(TBP.PaymentDate AS DATE) AS EffectiveDate,                                                                        
--    TBPD.ExpirationDate,          
-- 0 AS GrossAmount,                          
--    0 AS AgentCommPercentage,                                                                                        
-- --            CASE                                           
-- --    WHEN ISNULL(Details.TotalPremium, 0) > 0 THEN Details.TotalPremium                                          
-- --    WHEN ISNULL(Details.TotalInsuredPremium, 0) > 0 THEN Details.TotalInsuredPremium                    
-- --    ELSE 0                                           
-- --END AS NetAmount,          
-- TPCTE.TotalPremium AS NetAmount,           
--    TBPD.InvoicesDueDate AS DueDate,                                                                                                   
--    TBPD.PolicyNumber,                                                                                             
--    0 AS RemainingBalance,                                                                                            
-- 0 AS AgentCommissionAmt,                                                                            
-- 0 AS InferredPremiumCollected,                                            
-- CASE WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommission <> 0 AND Details.TotalCommissionPercentage IS NOT NULL                      
--  AND Details.TotalCommissionPercentage <> 0 AND PDL.AgentCommission IS NOT NULL AND PDL.AgentCommission <> 0                                                                                        
--  THEN (Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0)) * (PDL.AgentCommission / 100.0)                                                                                      
--  ELSE 0.00 END AS [Comm.] ,                                                                                        
-- ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],                                                     
-- AID.Name As [Insured Name],                                                                                      
-- TBPD.PolicyNumber AS [Policy Number],                                                                               
-- CD.CarrierDetail_Id ,                                                             
-- CD.Name AS [Carrier],                                                                                      
-- LOB.Name AS [LOB],                                             
-- TBPD.TransactionType AS [Transaction Type]  ,                                                            
-- CASE WHEN TBP.PaymentDirection_Id = 2 THEN ROUND(-TBP.AppliedAmount,2) ELSE ROUND(ABS(TBP.AppliedAmount),2) END AS CommsNetDue,                                                                                      
-- 0 AS [Comm. Due],                              
-- 0 AS TotalPremium,                                                                                  
-- ISNULL(PT.TotalAppliedAmount, 0.00) AS TotalCommissionReceived,                        
-- TBPD.EndorsementNumber                     
-- INTO #TempPaymentApplication_InsuredBill      
--    FROM #TempBasePolicyDetails_InsuredBill TBPD      
-- JOIN #TempTotalPremiumCTE_InsuredBill TPCTE on TPCTE.PolicyDetails_Id = TBPD.PolicyDetails_Id      
--    JOIN #TempAccounting_InvoiceDetails_InsuredBill ID ON ID.Invoice_Id = TBPD.Invoice_Id AND ID.Status = 1        
-- JOIN #TempInvoiceDetailView_InsuredBill DV ON DV.InvoiceDetail_Id = ID.Id           
--    JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                        
--    JOIN #TempBasePayments_InsuredBill TBP ON TBP.InvoiceLineItem_Id = ID.Id      
-- JOIN #TempValidPaymentIds_InsuredBill VPI ON VPI.Payment_Id = TBP.Payment_Id      
-- LEFT JOIN #TempPaymentTotals_InsuredBill PT ON PT.InvoiceLineItem_Id = ID.Id      
-- --AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)            
--    AND (@AgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgentIds_InsuredBill))         
-- AND (@ParentAgentDetail_Id IS NULL OR TBPD.AgentDetail_Id IN (SELECT DISTINCT AgentDetail_Id FROM @TempAgencyHierarchy_InsuredBill))                                                                              
-- LEFT JOIN #TempAccounting_InsuredDetails_InsuredBill AID on AID.InsuredDetail_Id =TBPD.InsuredDetail_Id AND AID.Status=1                                                                                      
-- LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =TBPD.CarrierDetail_Id AND CD.Status=1                                                                                      
-- LEft JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =TBPD.AgentDetail_Id AND ABD.Status=1                       
-- OUTER APPLY (                                                                                        
--  SELECT                                                    
--  CAST(SUM(CASE WHEN DV2.LIT_Name = 'PREMIUM' AND DV2.LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalInsuredPremium,                                                                         
--        CAST(SUM(CASE WHEN DV2.LIT_Name = 'PREMIUM' AND  DV2.LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,                                                                                            
--        CAST(SUM(CASE WHEN DV2.LI_Name = 'Total Commission' AND DV2.LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,                                                                                              
   
   
--  CAST(SUM(CASE WHEN DV2.LI_Name = 'Total Commission' AND DV2.LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,                                                                                  
  
   
     
--        CAST(SUM(CASE WHEN DV2.LI_Name = 'Agency/Broker Commission' AND DV2.LUD_Name = 'Accounts Payable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS CommPaidToDate,                                                                                
  
    
          
--        CAST(SUM(CASE WHEN DV2.LI_Name = 'Total Commission Percentage'  AND DV2.LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,                                                                                   
  
    
        
--        CAST(SUM(CASE WHEN DV2.LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,                                                
--  CAST(SUM(CASE WHEN DV2.LI_Name = 'Agency/Broker Commission' AND DV2.LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission                                                                                      
  
    
--  FROM #TempInvoiceDetailView_InsuredBill DV2         
--  WHERE DV2.Invoice_Id = TBPD.Invoice_Id                                                     
-- ) AS Details                                                                    
-- JOIN #TempPDL_Agg_InsuredBill PDL ON PDL.PolicyDetails_Id = TBPD.PolicyDetails_Id                                                                                          
--    WHERE      
-- (                                                                                  
--   @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                                                                              
--   OR (@CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL AND TBP.PaymentDate > @CompareDateFrom AND TBP.PaymentDate <= @CompareDateTo)                                                        
-- )                                                        
-- AND (@As_Of_Date IS NULL OR TBP.PaymentDate <= @As_Of_Date)                                                        
--    AND DV.LUD_Code = 'AP'                                                        
--    AND (@WholesalerDetail_Id IS NULL OR TBPD.WholesalerDetail_Id = @WholesalerDetail_Id)                  
-- -- AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)              
--    AND (@InvoiceNumber IS NULL OR TBPD.InvoiceNo = @InvoiceNumber OR TBPD.InvoiceNo LIKE '%' + @InvoiceNumber + '%'              
--    OR TRY_CAST(REPLACE(TBPD.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))              
--    AND (@CarrierDetail_Id IS NULL OR TBPD.CarrierDetail_Id = @CarrierDetail_Id)                                                        
--    AND (@LOB_Id IS NULL OR EXISTS (SELECT 1 FROM Accounting_InvoiceDetails AID WHERE AID.Invoice_Id = TBPD.Invoice_Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))                                                    
--     AND EXISTS (SELECT 1 FROM #TempLOBCarrierGroupingInvoices_InsuredBill LCGI WHERE LCGI.Invoice_Id = TBPD.Invoice_Id); 
                                     
-- SELECT                                                                                       
--  PolicyDetails_Id,                                                                         
--  Invoice_Id,                                                        
--  InvoiceDetail_Id,                                                                                      
--  LineItemDescription,                                                                                      
--  IdNo,                                                                                      
--  EffectiveDate,                                                             
--  ExpirationDate,                                                                       
--  GrossAmount,                                                                                      
--  AgentCommissionAmt,                                                                                      
--  AgentCommPercentage ,                                                                                      
--  CAST(ISNULL(NetAmount, 0.00) AS FLOAT) AS NetAmount,                                                                                      
--  DueDate,                                          
--  PolicyNumber,                                                                                      
--  RemainingBalance,                                                                
--  InferredPremiumCollected,                                       
--  [Comm.],                                                                                      
--  [Comm. Paid to Date],                              
--  [Insured Name],                                                                                      
--  [Policy Number],                                                                                      
--  [Carrier],                                                                            
--  CarrierDetail_Id,                                                                              
--  [LOB],                         
--  [Transaction Type],                                                                                      
--  CommsNetDue,                                                                                      
--  [Comm. Due],                                             
--  TotalPremium,                                                  
--  TotalCommissionReceived,                                                            
--  AgentDetail_Id,                                                                                      
--  [Agency Name],                                                                                      
--  [Agency Address],                        
--  EndorsementNumber                        
--  FROM #TempFeeData_InsuredBill                                                   
                                                                                      
-- UNION ALL                                                                                      
                                                                 
-- SELECT                                                            
--  PolicyDetails_Id,                                                                                      
--  Invoice_Id,                                                                                      
--  InvoiceDetail_Id,                                                                                      
--  LineItemDescription,                                              
--  IdNo,                                                                                      
--  EffectiveDate,                                                                                      
--  ExpirationDate,                                                                                      
--  GrossAmount,                                                                                      
--  AgentCommissionAmt,                                                                                      
--  0 AS AgentCommPercentage,                                                                 
--  CAST(ISNULL(0, 0.00) AS FLOAT) AS NetAmount,                                                                                      
--  DueDate,                          
--  PolicyNumber,                                                                                      
--  RemainingBalance,                                                                                      
--  0 AS InferredPremiumCollected,                                                                                      
--  [Comm.],                                                                                      
--  [Comm. Paid to Date],               
--  [Insured Name],                                                                                      
--  [Policy Number],                                                                                      
--  [Carrier],                                                                            
--  CarrierDetail_Id,                                                                                        
--  [LOB],                                                                                      
--  [Transaction Type],                                                                                      
--  CommsNetDue,                  
--  [Comm. Due],                                                                                      
--  TotalCommissionReceived,                                                                                      
--  0 AS TotalPremium,                                                                                      
--  AgentDetail_Id,                                                                                      
--  [Agency Name],                                                                                      
--  [Agency Address],                        
--  EndorsementNumber                        
--  FROM #TempPaymentApplication_InsuredBill                                                                                      
                                   
       
-- SELECT Logo AS ClientLogo FROM Accounting_Clients WHERE Client_Id=@Client_Id AND Status = 1;                                                  
               
-- DROP TABLE IF EXISTS #TempBasePolicyDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempAccounting_InsuredDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempAccounting_InvoiceDetails_InsuredBill;      
-- DROP TABLE IF EXISTS #TempBasePayments_InsuredBill;      
-- DROP TABLE IF EXISTS #TempValidPaymentIds_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPaymentTotals_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPolicyLOBs_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPDL_Agg_InsuredBill;      
-- DROP TABLE IF EXISTS #TempTotalPremiumCTE_InsuredBill;      
-- DROP TABLE IF EXISTS #TempInvoiceDetailView_InsuredBill;      
-- DROP TABLE IF EXISTS #TempFeeData_InsuredBill;      
-- DROP TABLE IF EXISTS #TempPaymentApplication_InsuredBill;
-- DROP TABLE IF EXISTS #TempLOBCarrierGroupingInvoices_InsuredBill;
-- END 