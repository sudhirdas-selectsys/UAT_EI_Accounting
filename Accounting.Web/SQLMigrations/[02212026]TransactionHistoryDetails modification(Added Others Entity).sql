alter   proc [dbo].[USP_Accounting_GetTransactionHistoryDetails]
(@PaymentApplicationLineItem_Id bigint)           
as          
begin          
SELECT APD.PolicyNumber,      
APALI.PaymentApplicationLineItem_Id,          
APALI.InvoiceLineItem_Id AS InvoiceDetails_Id,          
AINV.InvoiceNo,          
APALI.CreatedOn AS TransactionDate,          
CASE         
WHEN AP.EntityType_Id=1 THEN AD.Name         
WHEN AP.EntityType_Id=2 THEN ID.Name         
WHEN AP.EntityType_Id=3 THEN CD.Name         
WHEN AP.EntityType_Id=4 THEN C.CompanyName       
WHEN AP.EntityType_Id=6 THEN WSD.Name      
WHEN AP.EntityType_Id=7 THEN concat('State of ', APD.riskstate)
WHEN AINVD.EntityType_Id = 11 THEN AOED.Name
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
AP.PaymentDirection_Id      
FROM Accounting_PaymentApplicationLineItems APALI          
INNER JOIN Accounting_PaymentApplications APA ON APALI.PaymentApplication_Id=APA.PaymentApplication_Id          
INNER JOIN Accounting_Payments AP ON APA.Payment_Id = AP.Payment_Id          
INNER JOIN Accounting_InvoiceDetails AINVD ON APALI.InvoiceLineItem_Id=AINVD.Id          
LEFT JOIN Accounting_Invoices AINV ON AINVD.Invoice_Id=AINV.Id          
INNER JOIN Accounting_PolicyDetails APD ON AINV.PolicyDetails_Id=APD.Id          
LEFT JOIN Accounting_AgentDetails AAD ON APD.AgentDetail_Id=AAD.AgentDetail_Id          
--LEFT JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id=AINVD.Entity_Id AND AINVD.EntityType_Id=1          
--LEFT JOIN Accounting_InsuredDetails ID ON ID.InsuredDetail_Id=AINVD.Entity_Id AND AINVD.EntityType_Id=2          
--LEFT JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id=AINVD.Entity_Id AND AINVD.EntityType_Id=3          
--LEFT JOIN Accounting_Clients C ON C.Client_Id=AINVD.Entity_Id AND AINVD.EntityType_Id=6        
LEFT JOIN Accounting_AgentDetails AD ON AD.AgentDetail_Id=AP.Entity_Id AND AP.EntityType_Id=1 and AD.status=1            
LEFT JOIN Accounting_InsuredDetails ID ON ID.InsuredDetail_Id=AP.Entity_Id AND AP.EntityType_Id=2 and ID.status=1             
LEFT JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id=AP.Entity_Id AND AP.EntityType_Id=3 and CD.status=1             
LEFT JOIN Accounting_Clients C ON C.Client_Id=AP.Entity_Id AND AP.EntityType_Id=4 and C.status=1
LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = AINVD.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1  
LEFT JOIN Accounting_WholesalerDetails WSD ON WSD.WholesalerDetail_Id=AP.Entity_Id AND AP.EntityType_Id=6 and WSD.status=1          
LEFT JOIN Accounting_LU_BillingType_LineItems_Directions ALUBTLI ON AINVD.BillingType_LineItems_Directions_Id=ALUBTLI.BillingType_LineItems_Directions_Id          
LEFT JOIN Accounting_LU_LineItems_Directions ALULID ON ALUBTLI.LineItems_Directions_Id=ALULID.LineItems_Directions_Id          
LEFT JOIN Accounting_LU_Directions ALUD ON ALULID.Direction_Id=ALUD.Direction_Id          
LEFT JOIN Accounting_LU_PaymentTypes ALUPT ON AP.PaymentType_Id=ALUPT.PaymentType_Id          
LEFT JOIN Accounting_LU_OperatingBanks ALUOB ON AP.OperatingBank_Id=ALUOB.OperatingBank_Id          
LEFT JOIN Accounting_LU_PaymentStatus ALUPS ON AP.PaymentStatus_Id=ALUPS.PaymentStatus_Id          
WHERE APALI.PaymentApplicationLineItem_Id=@PaymentApplicationLineItem_Id AND APALI.Status=1          
end     