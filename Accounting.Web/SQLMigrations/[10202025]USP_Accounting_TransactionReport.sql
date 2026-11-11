  
ALTER Procedure [dbo].[USP_Accounting_TransactionReport]                        
(                                             
@TransactionFromDate varchar(200)=NULL,      
@TransactionToDate varchar(200)=NULL,      
@Client_Id bigint = NULL,    
@Range VARCHAR(200) = NULL,         
@PageSize int = null,        
@PageIndex int,        
@TotalRecords int out           
)                          
as                            
begin        
    
    DECLARE @CompareDateFrom DATETIME;        
    DECLARE @CompareDateTo DATETIME;        
         
    BEGIN        
        SET @CompareDateFrom = TRY_CAST(@TransactionFromDate AS DATETIME);        
        SET @CompareDateTo = DATEADD(MILLISECOND, -2, DATEADD(DAY, 1, TRY_CAST(@TransactionToDate AS DATETIME)));        
    END        
        
If @PageSize is NULL or @PageSize = 0        
begin          
 select APALI.PaymentApplicationLineItem_Id AS [Transaction_ID],    
  APD.PolicyNumber AS [Policy Number],                                 
  AINV.InvoiceNo AS [Invoice No],           
CONVERT(VARCHAR(10), APALI.CreatedOn, 101) AS [Transaction Date],                    
  CASE                       
  WHEN AP.EntityType_Id=1 THEN ADZ.Name                      
  WHEN AP.EntityType_Id=2 THEN IDZ.Name                       
  WHEN AP.EntityType_Id=3 THEN CDZ.Name                       
  WHEN AP.EntityType_Id=4 THEN CZ.CompanyName       
  WHEN AP.EntityType_Id=6 THEN WSD.Name       
  ELSE NULL END AS [Payer Or Payee],                              
  AAD.Name AS [Agency Name],                  
  ALUD.Name AS [Transaction Type],                              
  ALUPT.Name AS [Payment Method],                             
  AP.ReferenceNumber AS [Reference Number],                         
  ALUOB.Name AS [Bank Account],                       
  ALUPS.Name AS [Payment Status],                     
   CAST(ISNULL(APALI.AppliedAmount, 0) AS DECIMAL(18,2)) AS [Amount Paid Or Received],    
  PD.Name AS [Payment Direction]    
    
  FROM Accounting_PaymentApplicationLineItems APALI                            
 INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                          
 INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1       
 LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=AP.PaymentDirection_Id  and AP.Status = 1                       
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
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL            
    OR (APALI.CreatedOn >= @CompareDateFrom AND APALI.CreatedOn <= @CompareDateTo))     
     
ORDER BY APALI.CreatedOn DESC;          
end          
else          
begin          
SELECT APALI.PaymentApplicationLineItem_Id AS [Transaction_ID],    
  APD.PolicyNumber AS [Policy Number],                                 
  AINV.InvoiceNo AS [Invoice No],           
  CONVERT(VARCHAR(10), APALI.CreatedOn, 101) AS [Transaction Date],                    
  CASE                       
  WHEN AP.EntityType_Id=1 THEN ADZ.Name                      
  WHEN AP.EntityType_Id=2 THEN IDZ.Name                       
  WHEN AP.EntityType_Id=3 THEN CDZ.Name                       
  WHEN AP.EntityType_Id=4 THEN CZ.CompanyName       
  WHEN AP.EntityType_Id=6 THEN WSD.Name       
  ELSE NULL END AS [Payer Or Payee],                              
  AAD.Name AS [Agency Name],                  
  ALUD.Name AS [Transaction Type],                              
  ALUPT.Name [Payment Method],                             
  AP.ReferenceNumber AS [Reference Number],                         
  ALUOB.Name AS [Bank Account],                       
  ALUPS.Name AS [Payment Status],                     
  CAST(ISNULL(APALI.AppliedAmount, 0) AS DECIMAL(18,2)) AS [Amount Paid Or Received],    
  PD.Name AS [Payment Direction]    
    
  FROM Accounting_PaymentApplicationLineItems APALI                            
  INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                          
  INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1    
  LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=AP.PaymentDirection_Id  and AP.Status = 1    
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
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL            
    OR (APALI.CreatedOn >= @CompareDateFrom AND APALI.CreatedOn <= @CompareDateTo))     
    
  ORDER BY APALI.CreatedOn DESC  OFFSET @PageSize * @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY;          
  end       
      
  set @TotalRecords= (select count(*) FROM Accounting_PaymentApplicationLineItems APALI                            
  INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id  and APA.Status = 1                          
  INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id  and AP.Status = 1        
  LEFT JOIN Accounting_LU_PaymentDirections PD On PD.PaymentDirection_Id=AP.PaymentDirection_Id  and AP.Status = 1    
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
  AND (@CompareDateFrom IS NULL OR @CompareDateTo IS NULL            
    OR (APALI.CreatedOn >= @CompareDateFrom AND APALI.CreatedOn <= @CompareDateTo))     
);          
end