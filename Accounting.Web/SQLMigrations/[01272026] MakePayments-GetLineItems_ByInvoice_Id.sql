  
CREATE Proc USP_Accounting_GetLineItems_ByInvoice_Id          
(              
@Client_Id bigint ,  
@Invoice_Id bigint  
)              
As              
Begin              
SELECT     I.Id As[InvoiceId],     LI.LineItem_Id As [LineItemId],           AD.AgentDetail_Id,                                            AD.Name AS AgencyName,           ET.Name [EntityTypeName],           AID.EntityType_Id,           AID.Entity_Id     
                       FROM Accounting_AgentDetails AD                               LEFT JOIN Accounting_PolicyDetails PD ON AD.AgentDetail_Id = PD.AgentDetail_Id AND PD.Status = 1                       INNER JOIN Accounting_InsuredDetails ID ON PD.Insur
edDetail_Id = ID.InsuredDetail_Id AND ID.Status = 1                       INNER JOIN Accounting_Invoices I ON PD.Id = I.PolicyDetails_Id AND I.Status = 1                             INNER JOIN Accounting_InvoiceDetails AID ON I.Id = AID.Invoice_Id AND AID
.Status = 1                           INNER JOIN Accounting_LU_EntityTypes ET ON AID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1                           INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON AID.BillingType_LineItems_Di
rections_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                             INNER JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                         INN
ER JOIN Accounting_LU_Directions ALD ON ALD.Direction_Id = LD.Direction_Id AND ALD.Status=1                          INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                   
           INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1          LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 1 AND ag.Status = 1                   LEFT JO
IN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 3 AND ca.Status = 1                   LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 2 AND ins.Status =
 1                   LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = AID.Entity_Id AND AID.EntityType_Id = 6 AND wsd.Status = 1            WHERE AD.Client_Id = @Client_Id AND I.Id=@Invoice_Id                       AND AD.Status = 1
                           
end   