Alter Procedure USP_Accounting_GetARAPSearchInvoicesPaymentDetails    
(            
@InvoiceorPolicyNo varchar(100) = null,          
@Direction_Id tinyint = null,         
@Client_Id bigint = null        
)                      
As                      
Begin                      
select  ID.*,  
CASE   
 WHEN ID.EntityType_Id = 1 THEN ag.Name                        
 WHEN ID.EntityType_Id = 2 THEN Ins.Name                        
 WHEN ID.EntityType_Id = 3 THEN ca.Name      
 WHEN ID.EntityType_Id = 6 THEN wsd.Name       
 ELSE NULL                        
 END AS EntityName, ET.Name as EntityTypeName from Accounting_InvoiceDetails ID                    
LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id=INV.Id  AND INV.Status = 1                                
INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id=PD.Id  AND PD.Status = 1                 
LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id=pd.AgentDetail_Id AND agd.Status = 1                
LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id=pd.InsuredDetail_Id AND isd.Status = 1              
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1                        
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1                        
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1       
LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1      
left join Accounting_LU_BillingType_LineItems_Directions BLD on ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id and BLD.Status = 1                      
left join Accounting_LU_LineItems_Directions LD on BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id and LD.Status = 1                      
left join Accounting_LU_EntityTypes ET on ID.EntityType_Id = ET.EntityType_Id and ET.Status = 1                      
WHERE (INV.InvoiceNo=@InvoiceorPolicyNo or PD.PolicyNumber=@InvoiceorPolicyNo) and LD.Direction_Id = @Direction_Id and PD.Client_Id = @Client_Id and ID.Status = 1  
End