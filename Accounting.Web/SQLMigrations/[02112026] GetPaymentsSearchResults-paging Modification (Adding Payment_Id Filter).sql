alter proc USP_Accounting_GetPaymentsSearchResults_paging                                
(                                
@EntityType_Id tinyint = null                                
,@Entity_Id bigint = null       
--@PaymentDate  datetime = null       
,@PaymentFromDate datetime = null       
,@PaymentToDate datetime = null       
,@Amount float = null                                
,@PaymentType_Id tinyint = null                                
,@ReferenceNumber varchar(100) = null                                
,@PaymentStatus_Id tinyint = null                                
,@OperatingBank_Id tinyint = null                                
,@Notes varchar(500) = null                           
,@PaymentDirection_Id tinyint = null                
,@Client_Id bigint = null  
,@Payment_Id bigint = null  
,@OutstandingBalances bit = null          
,@PageSize int = null      
,@PageNumber int,@TotalRecords int out          
)                                
As                                
Begin           
If @PageSize is NULL or @PageSize = 0         begin          
SELECT * FROM Accounting_Payments p        
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1
OUTER APPLY (SELECT TOP 1 * FROM Accounting_JournalEntries JE WHERE JE.Source_Id = P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1 ORDER BY JE.JournalEntry_Id DESC) JE
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                              
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                              
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                           
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                              
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                      
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1    
                        
WHERE  p.Client_Id=@Client_Id and p.Status = 1                               
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                              
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)      
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))      
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                              
AND (@Amount   IS NULL OR p.Amount = @Amount)                              
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                              
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                              
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                              
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)                              
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                          
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)  
AND (@Payment_Id IS NULL OR p.Payment_Id=@Payment_Id)  
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0))               
order by p.Payment_Id desc;           
end          
else          
begin          
SELECT           
p.*,JE.ExternalJournalEntryIdentifier,          
coalesce(nullif(    
CASE                               
WHEN p.EntityType_Id = 1 THEN ag.Name                              
WHEN p.EntityType_Id = 3 THEN ca.Name                              
WHEN p.EntityType_Id = 2 THEN ins.Name                          
WHEN p.EntityType_Id = 4 THEN CL.CompanyName     
WHEN p.EntityType_Id=5 THEN fd.Name        
WHEN p.EntityType_Id=7 THEN SD.Name    
ELSE NULL END,''),'NA') AS EntityName                              
FROM Accounting_Payments p          
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1 
OUTER APPLY (SELECT TOP 1 * FROM Accounting_JournalEntries JE WHERE JE.Source_Id = P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1 ORDER BY JE.JournalEntry_Id DESC) JE
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                              
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                              
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1           
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                              
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                      
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1    
                        
WHERE  p.Client_Id=@Client_Id and p.Status = 1                               
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                              
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)                              
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))      
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                              
AND (@Amount   IS NULL OR p.Amount = @Amount)                              
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                              
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                              
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                              
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)  
AND (@Payment_Id IS NULL OR p.Payment_Id=@Payment_Id)  
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                          
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)              
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0))               
order by p.Payment_Id desc OFFSET @PageSize * @PageNumber ROWS FETCH NEXT @PageSize ROWS ONLY;          
end 
set @TotalRecords= (SELECT COUNT(*) FROM Accounting_Payments p
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1
OUTER APPLY (SELECT TOP 1 * FROM Accounting_JournalEntries JE WHERE JE.Source_Id = P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1 ORDER BY JE.JournalEntry_Id DESC) JE
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                              
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                              
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                           
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                              
LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                      
LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1    
                        
WHERE  p.Client_Id=@Client_Id and p.Status = 1                               
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                              
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)                              
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))      
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                              
AND (@Amount   IS NULL OR p.Amount = @Amount)                              
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                              
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                              
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                              
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)  
AND (@Payment_Id IS NULL OR p.Payment_Id=@Payment_Id)  
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                          
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)              
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0)))          
end 