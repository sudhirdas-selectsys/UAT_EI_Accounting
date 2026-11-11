alter   Procedure [dbo].[USP_Accounting_GetTransactionHistorySearchResults]                            
(                              
@Direction_Id tinyint =NULL,                              
@EntityName varchar(500)=NULL,                              
@TransactionDate datetime=NULL,                              
@PaymentType_Id tinyint=NULL,                              
@Amount_From float=NULL,                              
@Amount_To float=NULL,                              
@PaymentStatus_Id tinyint =NULL,                        
@PolicyNumber varchar(100)=NULL,                    
@InvoiceNumber varchar(100)=NULL,                
@Client_Id bigint = NULL,
@Payment_Id bigint= NULL,
@PageSize int = null,            
@PageNumber int,            
@TotalRecords int out               
)                              
as                                
begin               
If @PageSize is NULL or @PageSize = 0            
begin              
select * FROM Accounting_PaymentApplicationLineItems APALI                                
INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                              
INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1                              
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1                             
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1                             
INNER JOIN Accounting_PolicyDetails APD ON AINV.PolicyDetails_Id=APD.Id and APD.Status = 1                               
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1                          
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1                               
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1                              
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1                               
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1                               
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1                                            
                           
LEFT JOIN Accounting_LU_BillingType_LineItems_Directions ALUBTLI ON AINVD.BillingType_LineItems_Directions_Id=ALUBTLI.BillingType_LineItems_Directions_Id and ALUBTLI.Status = 1                               
LEFT JOIN Accounting_LU_LineItems_Directions ALULID ON ALUBTLI.LineItems_Directions_Id=ALULID.LineItems_Directions_Id   and ALULID.Status = 1                             
LEFT JOIN Accounting_LU_Directions ALUD ON ALULID.Direction_Id=ALUD.Direction_Id  and ALUD.Status = 1                              
LEFT JOIN Accounting_LU_PaymentTypes ALUPT ON AP.PaymentType_Id=ALUPT.PaymentType_Id and ALUPT.Status = 1                               
LEFT JOIN Accounting_LU_OperatingBanks ALUOB ON AP.OperatingBank_Id=ALUOB.OperatingBank_Id  and ALUOB.Status = 1                              
LEFT JOIN Accounting_LU_PaymentStatus ALUPS ON AP.PaymentStatus_Id=ALUPS.PaymentStatus_Id and ALUPS.Status = 1                               
WHERE APD.Client_Id=@Client_Id AND APALI.Status=1                              
AND (@Direction_Id IS NULL OR ALUD.Direction_Id = @Direction_Id)                                  
AND (@EntityName  IS NULL OR (ADZ.Name LIKE '%' + @EntityName + '%' OR IDZ.Name LIKE '%' + @EntityName + '%' OR CDZ.Name LIKE '%' + @EntityName + '%' OR CZ.CompanyName LIKE '%' + @EntityName + '%'))                                  
AND (@TransactionDate IS NULL OR CAST(APALI.CreatedOn AS DATE) = CAST(@TransactionDate AS DATE))        
AND (@PaymentType_Id IS NULL OR ALUPT.PaymentType_Id = @PaymentType_Id)                       
AND (@Amount_From IS NULL OR @Amount_To IS NULL OR APALI.AppliedAmount BETWEEN @Amount_From AND @Amount_To)                              
AND (@PaymentStatus_Id IS NULL OR ALUPS.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@PolicyNumber IS NULL OR APD.PolicyNumber LIKE '%' + @PolicyNumber + '%')                    
AND (@InvoiceNumber IS NULL OR AINV.InvoiceNo LIKE '%' + @InvoiceNumber + '%')
AND (@Payment_Id IS NULL OR AP.Payment_Id = @Payment_Id) 
ORDER BY APALI.CreatedOn DESC;              
end              
else              
begin              
SELECT APD.PolicyNumber,              
APALI.PaymentApplicationLineItem_Id,                                
APALI.InvoiceLineItem_Id AS InvoiceDetails_Id,                                
AINV.InvoiceNo,                                
APALI.CreatedOn AS TransactionDate,                                
CASE                           
WHEN AP.EntityType_Id=1 THEN ADZ.Name                          
WHEN AP.EntityType_Id=2 THEN IDZ.Name                           
WHEN AP.EntityType_Id=3 THEN CDZ.Name                           
WHEN AP.EntityType_Id=4 THEN CZ.CompanyName           
WHEN AP.EntityType_Id=6 THEN WSD.Name        
WHEN AP.EntityType_Id=7 THEN concat('State of ', APD.RiskState)      
ELSE NULL END AS EntityName,                              
AAD.Name AS AgentName,                                
AAD.AgentDetail_Id,                                
ALUD.Name AS TransactionType,                                
ALUD.Direction_Id,                                
ALUPT.Name PaymentMethod,                                
ALUPT.PaymentType_Id,                                
AP.ReferenceNumber,                                
APALI.AppliedAmount,                                
ALUOB.Name AS BankAccount,                                
ALUOB.OperatingBank_Id,                                
ALUPS.Name AS PaymentStatus,                                
ALUPS.PaymentStatus_Id,                    
AP.PaymentDirection_Id  ,    
AP.Payment_Id    
FROM Accounting_PaymentApplicationLineItems APALI                                
INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                              
INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1                              
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1                             
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1                             
INNER JOIN Accounting_PolicyDetails APD ON AINV.PolicyDetails_Id=APD.Id and APD.Status = 1                               
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1                          
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1                               
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1                              
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1                               
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1                               
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1                                            
LEFT JOIN Accounting_LU_BillingType_LineItems_Directions ALUBTLI ON AINVD.BillingType_LineItems_Directions_Id=ALUBTLI.BillingType_LineItems_Directions_Id and ALUBTLI.Status = 1                               
LEFT JOIN Accounting_LU_LineItems_Directions ALULID ON ALUBTLI.LineItems_Directions_Id=ALULID.LineItems_Directions_Id   and ALULID.Status = 1                             
LEFT JOIN Accounting_LU_Directions ALUD ON ALULID.Direction_Id=ALUD.Direction_Id  and ALUD.Status = 1                              
LEFT JOIN Accounting_LU_PaymentTypes ALUPT ON AP.PaymentType_Id=ALUPT.PaymentType_Id and ALUPT.Status = 1                               
LEFT JOIN Accounting_LU_OperatingBanks ALUOB ON AP.OperatingBank_Id=ALUOB.OperatingBank_Id  and ALUOB.Status = 1                              
LEFT JOIN Accounting_LU_PaymentStatus ALUPS ON AP.PaymentStatus_Id=ALUPS.PaymentStatus_Id and ALUPS.Status = 1                               
WHERE APD.Client_Id=@Client_Id AND APALI.Status=1                        
AND (@Direction_Id IS NULL OR ALUD.Direction_Id = @Direction_Id)                                  
AND (@EntityName  IS NULL OR (ADZ.Name LIKE '%' + @EntityName + '%' OR IDZ.Name LIKE '%' + @EntityName + '%' OR CDZ.Name LIKE '%' + @EntityName + '%' OR CZ.CompanyName LIKE '%' + @EntityName + '%'))                                  
AND (@TransactionDate IS NULL OR CAST(APALI.CreatedOn AS DATE) = CAST(@TransactionDate AS DATE))                              
AND (@PaymentType_Id IS NULL OR ALUPT.PaymentType_Id = @PaymentType_Id)                               
AND (@Amount_From IS NULL OR @Amount_To IS NULL OR APALI.AppliedAmount BETWEEN @Amount_From AND @Amount_To)                              
AND (@PaymentStatus_Id IS NULL OR ALUPS.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@PolicyNumber IS NULL OR APD.PolicyNumber LIKE '%' + @PolicyNumber + '%')                    
AND (@InvoiceNumber IS NULL OR AINV.InvoiceNo LIKE '%' + @InvoiceNumber + '%')
AND (@Payment_Id IS NULL OR AP.Payment_Id = @Payment_Id) 
ORDER BY APALI.CreatedOn DESC  OFFSET @PageSize * @PageNumber ROWS FETCH NEXT @PageSize ROWS ONLY;              
end              
set @TotalRecords= (select count(*) FROM Accounting_PaymentApplicationLineItems APALI                                
INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                              
INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1                              
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1                             
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1                             
INNER JOIN Accounting_PolicyDetails APD ON AINV.PolicyDetails_Id=APD.Id and APD.Status = 1                               
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1                          
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1                               
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1                              
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1                               
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1                               
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1                          
LEFT JOIN Accounting_LU_BillingType_LineItems_Directions ALUBTLI ON AINVD.BillingType_LineItems_Directions_Id=ALUBTLI.BillingType_LineItems_Directions_Id and ALUBTLI.Status = 1                               
LEFT JOIN Accounting_LU_LineItems_Directions ALULID ON ALUBTLI.LineItems_Directions_Id=ALULID.LineItems_Directions_Id   and ALULID.Status = 1                             
LEFT JOIN Accounting_LU_Directions ALUD ON ALULID.Direction_Id=ALUD.Direction_Id  and ALUD.Status = 1                              
LEFT JOIN Accounting_LU_PaymentTypes ALUPT ON AP.PaymentType_Id=ALUPT.PaymentType_Id and ALUPT.Status = 1                    
LEFT JOIN Accounting_LU_OperatingBanks ALUOB ON AP.OperatingBank_Id=ALUOB.OperatingBank_Id  and ALUOB.Status = 1                              
LEFT JOIN Accounting_LU_PaymentStatus ALUPS ON AP.PaymentStatus_Id=ALUPS.PaymentStatus_Id and ALUPS.Status = 1                               
WHERE APD.Client_Id=@Client_Id AND APALI.Status=1                              
AND (@Direction_Id IS NULL OR ALUD.Direction_Id = @Direction_Id)                                  
AND (@EntityName  IS NULL OR (ADZ.Name LIKE '%' + @EntityName + '%' OR IDZ.Name LIKE '%' + @EntityName + '%' OR CDZ.Name LIKE '%' + @EntityName + '%' OR CZ.CompanyName LIKE '%' + @EntityName + '%'))                                  
AND (@TransactionDate IS NULL OR CAST(APALI.CreatedOn AS DATE) = CAST(@TransactionDate AS DATE))                
AND (@PaymentType_Id IS NULL OR ALUPT.PaymentType_Id = @PaymentType_Id)                               
AND (@Amount_From IS NULL OR @Amount_To IS NULL OR APALI.AppliedAmount BETWEEN @Amount_From AND @Amount_To)                              
AND (@PaymentStatus_Id IS NULL OR ALUPS.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@PolicyNumber IS NULL OR APD.PolicyNumber LIKE '%' + @PolicyNumber + '%')                    
AND (@InvoiceNumber IS NULL OR AINV.InvoiceNo LIKE '%' + @InvoiceNumber + '%')
AND (@Payment_Id IS NULL OR AP.Payment_Id = @Payment_Id) 
);              
end 