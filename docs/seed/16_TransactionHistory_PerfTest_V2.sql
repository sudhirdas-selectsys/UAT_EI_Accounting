-- PERF TEST COPY (do not use in production). Prototype V2 of USP_Accounting_GetTransactionHistorySearchResults for the PolicyNumber search (see docs/Iteration1_SlowQuery_Analysis.md).
IF DB_NAME() <> N'uat-accounting-perf' THROW 50001, 'Wrong database. Aborting.', 1;
IF OBJECT_ID(N'dbo.USP_Accounting_GetTransactionHistorySearchResults_PerfTest_V2') IS NOT NULL THROW 50002, 'Object already exists. Aborting.', 1;
GO
CREATE PROCEDURE [dbo].[USP_Accounting_GetTransactionHistorySearchResults_PerfTest_V2]                                
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
-- PerfTest V2: when @PolicyNumber is supplied, first stage the matching policies (same filter as before:
-- Client_Id, Status = 1, PolicyNumber LIKE '%x%'), then join from them. All other cases run the original procedure unchanged.
IF @PolicyNumber IS NULL
BEGIN
    EXEC dbo.USP_Accounting_GetTransactionHistorySearchResults @Direction_Id=@Direction_Id, @EntityName=@EntityName, @TransactionDate=@TransactionDate, @PaymentType_Id=@PaymentType_Id,
         @Amount_From=@Amount_From, @Amount_To=@Amount_To, @PaymentStatus_Id=@PaymentStatus_Id, @PolicyNumber=@PolicyNumber,
         @InvoiceNumber=@InvoiceNumber, @Client_Id=@Client_Id, @Payment_Id=@Payment_Id, @PageSize=@PageSize, @PageNumber=@PageNumber,
         @TotalRecords=@TotalRecords OUTPUT;
    RETURN;
END
SELECT * INTO #APD FROM Accounting_PolicyDetails WHERE Client_Id = @Client_Id AND Status = 1 AND PolicyNumber LIKE '%' + @PolicyNumber + '%';
CREATE UNIQUE CLUSTERED INDEX CIX_APD ON #APD (Id);                   
If @PageSize is NULL or @PageSize = 0                
begin                  
select * FROM Accounting_PaymentApplicationLineItems APALI                                    
INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                                  
INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1                                
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1 AND AINVD.Client_Id = @Client_Id                              
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1 and AINVD.Client_Id =AINV.Client_Id                                
INNER JOIN #APD APD ON AINV.PolicyDetails_Id=APD.Id and AINV.Client_Id = APD.Client_Id                                 
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1 and APD.Client_Id = AAD.Client_Id                             
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1 and AP.Client_Id = ADZ.Client_Id                                   
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1 and AP.Client_Id = IDZ.Client_Id                                  
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1 and AP.Client_Id = CDZ.Client_Id                                   
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1 and AP.Client_Id = CZ.Client_Id                                 
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1 and AP.Client_Id = WSD.Client_Id                                            
                               
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
WHEN AP.EntityType_Id=11 THEN AOED.Name
WHEN AP.EntityType_Id=15 THEN MD.Name  
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
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1 AND AINVD.Client_Id = @Client_Id                                
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1 AND AINVD.Client_Id = AINV.Client_Id                                 
INNER JOIN #APD APD ON AINV.PolicyDetails_Id=APD.Id AND AINV.Client_Id = APD.Client_Id                                  
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1 AND APD.Client_Id = AAD.Client_Id                              
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1 AND AP.Client_Id = ADZ.Client_Id                                   
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1 AND AP.Client_Id = IDZ.Client_Id                                 
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1 AND AP.Client_Id = CDZ.Client_Id                                   
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1 AND AP.Client_Id = CZ.Client_Id                                   
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1 AND AP.Client_Id = WSD.Client_Id  
LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = AP.Entity_Id and AP.EntityType_Id=11  and AOED.Status=1 AND AP.Client_Id = AOED.Client_Id
LEFT JOIN Accounting_MortgageeDetails MD ON MD.MortgageeDetail_Id = AP.Entity_Id AND AP.EntityType_Id=15 and MD.Status = 1 AND AP.Client_Id = MD.Client_Id 
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
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id   and AINVD.Status = 1 AND AINVD.Client_Id = @Client_Id                                 
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id   and AINV.Status = 1 AND AINVD.Client_Id = AINV.Client_Id                                
INNER JOIN #APD APD ON AINV.PolicyDetails_Id=APD.Id AND AINV.Client_Id = APD.Client_Id                                  
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id and AAD.Status = 1 AND APD.Client_Id = AAD.Client_Id                             
LEFT JOIN Accounting_AgentDetails ADZ ON ADZ.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and ADZ.Status = 1 AND AP.Client_Id = ADZ.Client_Id                                  
LEFT JOIN Accounting_InsuredDetails IDZ ON IDZ.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2  and IDZ.Status = 1 AND AP.Client_Id = IDZ.Client_Id                                 
LEFT JOIN Accounting_CarrierDetails CDZ ON CDZ.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CDZ.Status = 1 AND AP.Client_Id = CDZ.Client_Id                                   
LEFT JOIN Accounting_Clients CZ ON CZ.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and CZ.Status = 1 AND AP.Client_Id = CZ.Client_Id                                   
LEFT JOIN Accounting_WholesalerDetails WSD on WSD.WholesalerDetail_Id = AP.Entity_Id AND AP.EntityType_Id=6 and WSD.Status = 1 AND AP.Client_Id = WSD.Client_Id  
LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = AP.Entity_Id and AP.EntityType_Id=11  and AOED.Status=1 AND AP.Client_Id = AOED.Client_Id
LEFT JOIN Accounting_MortgageeDetails MD ON MD.MortgageeDetail_Id = AP.Entity_Id and AP.EntityType_Id=15  and MD.Status=1 AND AP.Client_Id = MD.Client_Id
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

GO
