-- PERF TEST COPY (do not use in production). Rewrite of USP_Accounting_PaymentReport: the LEFT JOIN PolicyDetails/Invoices chain replaced by OUTER APPLY, same rows and columns.
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
IF OBJECT_ID(N'dbo.USP_Accounting_PaymentReport_PerfTest_V2') IS NOT NULL THROW 50002, 'Object already exists. Aborting.', 1;
GO
CREATE PROC [dbo].[USP_Accounting_PaymentReport_PerfTest_V2]                        
(                          
@Client_Id bigint = null,                        
@PageSize int = null,                      
@PageIndex int,                
@Range VARCHAR(200) = NULL,                
@PaymentFromDate varchar(200)=null,                
@PaymentToDate varchar(200)=null,    
@InvoiceNumber VARCHAR(200) = NULL,
@LOB_Carrier_Grouping_Id INT = NULL,
@TotalRecords int out                         
)                          
as                                
begin                   
                
    DECLARE @CompareDateFrom DATETIME;                    
    DECLARE @CompareDateTo DATETIME;                    
                     
    BEGIN                    
        SET @CompareDateFrom = TRY_CAST(@PaymentFromDate AS DATETIME);                    
        SET @CompareDateTo = DATEADD(MILLISECOND, -2, DATEADD(DAY, 1, TRY_CAST(@PaymentToDate AS DATETIME)));                    
    END                    
                    
If @PageSize is NULL or @PageSize = 0                      
begin                      
SELECT p.Payment_Id,                
ET.Name as [Entity Type],                 
coalesce(nullif(            
CASE                
    WHEN p.EntityType_Id = 1 THEN ag.Name                                
    WHEN p.EntityType_Id = 3 THEN ca.Name                                
    WHEN p.EntityType_Id = 2 THEN ins.Name                                
 WHEN p.EntityType_Id = 4 THEN AC.CompanyName                  
 WHEN p.EntityType_Id = 6 THEN WSD.Name             
 WHEN p.EntityType_Id=5 THEN fd.Name                
WHEN p.EntityType_Id=7 THEN SD.Name            
 ELSE NULL                           
END,''),'NA') AS [Entity Name],                
CONVERT(VARCHAR(10), p.PaymentDate, 101) AS [Payment Date],                    
CAST(ISNULL(p.Amount, 0) AS DECIMAL(18,2)) AS Amount,                
CAST(ISNULL(p.RemainingAmount, 0) AS DECIMAL(18,2)) AS [Remaining Amount],                
PT.Name AS [Payment Type],                
p.ReferenceNumber AS [Reference Number],                
PS.Name AS [Payment Status],                
OB.Name AS [Bank Account],                
PD.Name AS [Payment Direction]                
                
FROM Accounting_Payments p                 
Left JOIN Accounting_LU_EntityTypes ET ON ET.EntityType_Id=p.EntityType_Id                
Left JOIN Accounting_LU_PaymentTypes PT ON PT.PaymentType_Id=p.PaymentType_Id                
Left JOIN Accounting_LU_PaymentStatus PS ON PS.PaymentStatus_Id=p.PaymentStatus_Id                
Left JOIN Accounting_LU_OperatingBanks OB ON OB.OperatingBank_Id=p.OperatingBank_Id                
LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=p.PaymentDirection_Id                
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1                    
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1 AND p.Client_Id = ag.Client_Id                              
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1 AND p.Client_Id = ca.Client_Id                                
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1 AND p.Client_Id = ins.Client_Id                               
LEFT JOIN Accounting_Clients AC on AC.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND AC.Status = 1 AND p.Client_Id = AC.Client_Id                   
LEFT JOIN Accounting_WholesalerDetails WSD  ON WSD.WholesalerDetail_Id = p.Entity_Id AND p.EntityType_Id = 6 AND WSD.Status = 1 AND p.Client_Id = WSD.Client_Id            
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1 AND p.Client_Id = fd.Client_Id                             
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1 AND p.Client_Id = SD.Client_Id   
OUTER APPLY (SELECT I.InvoiceNo FROM Accounting_PolicyDetails APD LEFT JOIN Accounting_Invoices I ON I.PolicyDetails_Id = APD.Id AND I.Status=1 AND APD.Client_Id = I.Client_Id WHERE ins.InsuredDetail_Id = APD.InsuredDetail_Id AND APD.Status=1 AND ins.Client_Id = APD.Client_Id) I   
 where p.Client_Id=@Client_Id and p.Status = 1                  
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                        
        OR (PaymentDate >= @CompareDateFrom AND PaymentDate <= @CompareDateTo))    
  --AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)  
    AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber  OR I.InvoiceNo LIKE '%' + @InvoiceNumber + '%'  
    OR TRY_CAST(REPLACE(I.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))  
order by Payment_Id desc;                      
end                      
else                        
begin                        
SELECT p.Payment_Id,                
ET.Name as [Entity Type],                 
coalesce(nullif(            
CASE                
    WHEN p.EntityType_Id = 1 THEN ag.Name                                
    WHEN p.EntityType_Id = 3 THEN ca.Name                     
    WHEN p.EntityType_Id = 2 THEN ins.Name                                
    WHEN p.EntityType_Id = 4 THEN AC.CompanyName                  
    WHEN p.EntityType_Id = 6 THEN WSD.Name             
    WHEN p.EntityType_Id=5 THEN fd.Name                
    WHEN p.EntityType_Id=7 THEN SD.Name
    WHEN p.EntityType_Id=15 THEN MD.Name
 ELSE NULL                                
END,''),'NA') AS [Entity Name],                
CONVERT(VARCHAR(10), p.PaymentDate, 101) AS [Payment Date],                    
CAST(ISNULL(p.Amount, 0) AS DECIMAL(18,2)) AS Amount,                
CAST(ISNULL(p.RemainingAmount, 0) AS DECIMAL(18,2)) AS [Remaining Amount],                
PT.Name AS [Payment Type],                
p.ReferenceNumber AS [Reference Number],                
PS.Name AS [Payment Status],                
OB.Name AS [Bank Account],                
PD.Name AS [Payment Direction]                
                
FROM Accounting_Payments p                 
Left JOIN Accounting_LU_EntityTypes ET ON ET.EntityType_Id=p.EntityType_Id                
Left JOIN Accounting_LU_PaymentTypes PT ON PT.PaymentType_Id=p.PaymentType_Id                
Left JOIN Accounting_LU_PaymentStatus PS ON PS.PaymentStatus_Id=p.PaymentStatus_Id                
Left JOIN Accounting_LU_OperatingBanks OB ON OB.OperatingBank_Id=p.OperatingBank_Id                
LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=p.PaymentDirection_Id                
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1                    
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1 AND p.Client_Id = ag.Client_Id                                
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1 AND p.Client_Id = ca.Client_Id                               
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1 AND p.Client_Id = ins.Client_Id                               
LEFT JOIN Accounting_Clients AC on AC.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND AC.Status = 1 AND p.Client_Id = AC.Client_Id                 
LEFT JOIN Accounting_WholesalerDetails WSD  ON WSD.WholesalerDetail_Id = p.Entity_Id AND p.EntityType_Id = 6 AND WSD.Status = 1 AND p.Client_Id = WSD.Client_Id             
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1 AND p.Client_Id = fd.Client_Id                             
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1 AND p.Client_Id = SD.Client_Id
LEFT JOIN Accounting_MortgageeDetails MD on MD.MortgageeDetail_Id = p.Entity_Id AND p.EntityType_Id = 15 and MD.Status = 1 AND p.Client_Id = MD.Client_Id   
OUTER APPLY (SELECT I.InvoiceNo FROM Accounting_PolicyDetails APD LEFT JOIN Accounting_Invoices I ON I.PolicyDetails_Id = APD.Id AND I.Status=1 AND APD.Client_Id = I.Client_Id WHERE ins.InsuredDetail_Id = APD.InsuredDetail_Id AND APD.Status=1 AND ins.Client_Id = APD.Client_Id) I   
WHERE p.Client_Id=@Client_Id and p.Status = 1                 
AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                        
        OR (p.PaymentDate >= @CompareDateFrom AND p.PaymentDate <= @CompareDateTo))    
  --AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)  
    AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber OR I.InvoiceNo LIKE '%' + @InvoiceNumber + '%'  
    OR TRY_CAST(REPLACE(I.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))  
order by p.Payment_Id desc OFFSET @PageSize * @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY;                        
end                      
set @TotalRecords= (select count(*) from Accounting_Payments p WITH (NOLOCK)    
Left JOIN Accounting_LU_EntityTypes ET ON ET.EntityType_Id=p.EntityType_Id                
Left JOIN Accounting_LU_PaymentTypes PT ON PT.PaymentType_Id=p.PaymentType_Id                
Left JOIN Accounting_LU_PaymentStatus PS ON PS.PaymentStatus_Id=p.PaymentStatus_Id                
Left JOIN Accounting_LU_OperatingBanks OB ON OB.OperatingBank_Id=p.OperatingBank_Id                
LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=p.PaymentDirection_Id                
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1                    
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1 AND p.Client_Id = ag.Client_Id                               
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1  AND p.Client_Id = ca.Client_Id                               
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1 AND p.Client_Id = ins.Client_Id    
OUTER APPLY (SELECT I.InvoiceNo FROM Accounting_PolicyDetails APD LEFT JOIN Accounting_Invoices I ON I.PolicyDetails_Id = APD.Id AND I.Status=1 AND APD.Client_Id = I.Client_Id WHERE ins.InsuredDetail_Id = APD.InsuredDetail_Id AND APD.Status=1 AND ins.Client_Id = APD.Client_Id) I    
where p.Client_Id = @Client_Id and p.status = 1                
AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                        
        OR (PaymentDate >= @CompareDateFrom AND PaymentDate <= @CompareDateTo))    
  --AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)  
    AND (@InvoiceNumber IS NULL OR I.InvoiceNo = @InvoiceNumber OR I.InvoiceNo LIKE '%' + @InvoiceNumber + '%'  
    OR TRY_CAST(REPLACE(I.InvoiceNo, 'INV', '') AS INT) = TRY_CAST(@InvoiceNumber AS INT))  
  );                      
end             
          

