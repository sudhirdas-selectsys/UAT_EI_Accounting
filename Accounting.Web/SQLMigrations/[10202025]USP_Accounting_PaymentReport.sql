ALTER PROC [dbo].[USP_Accounting_PaymentReport]                  
(                    
@Client_Id bigint = null,                  
@PageSize int = null,                
@PageIndex int,          
@Range VARCHAR(200) = NULL,          
@PaymentFromDate varchar(200)=null,          
@PaymentToDate varchar(200)=null,          
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
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                          
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                          
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                          
LEFT JOIN Accounting_Clients AC on AC.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND AC.Status = 1             
LEFT JOIN Accounting_WholesalerDetails WSD  ON WSD.WholesalerDetail_Id = p.Entity_Id AND p.EntityType_Id = 6 AND WSD.Status = 1       
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                        
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1      
 where p.Client_Id=@Client_Id and p.Status = 1            
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                  
        OR (PaymentDate >= @CompareDateFrom AND PaymentDate <= @CompareDateTo))           
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
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                          
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                          
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                          
LEFT JOIN Accounting_Clients AC on AC.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND AC.Status = 1             
LEFT JOIN Accounting_WholesalerDetails WSD  ON WSD.WholesalerDetail_Id = p.Entity_Id AND p.EntityType_Id = 6 AND WSD.Status = 1       
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                        
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1      
WHERE p.Client_Id=@Client_Id and p.Status = 1           
AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                  
        OR (p.PaymentDate >= @CompareDateFrom AND p.PaymentDate <= @CompareDateTo))           
order by p.Payment_Id desc OFFSET @PageSize * @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY;                  
end                
set @TotalRecords= (select count(*) from Accounting_Payments WITH (NOLOCK) where Client_Id = @Client_Id and status = 1          
AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL                  
        OR (PaymentDate >= @CompareDateFrom AND PaymentDate <= @CompareDateTo)));                
end       
    
    
    
    