update Accounting_LU_ReportTemplates set Name = 'Agency Billed Account Statement', Code = 'AgencyBilledAccountStatement' where ReportTemplate_Id = 1 and status = 1
update Accounting_LU_ReportTemplates set Name = 'Direct Billed Account Statement', Code = 'DirectBilledAccountStatement' where ReportTemplate_Id = 2 and status = 1
go


Alter Proc USP_Accounting_AgencyBillAccountStatementReport         
(          
@Client_Id bigint = null          
,@PolicyFromDate varchar(200) = null          
,@PolicyToDate varchar(200) = null         
,@AgentDetail_Id bigint = null        
)          
As Begin          
select * from Accounting_AgentDetails where Client_Id = @Client_Id and Status = 1          
AND (@AgentDetail_Id is null or AgentDetail_Id = @AgentDetail_Id)        
          
select * from Accounting_PolicyDetails           
where Client_Id = @Client_Id           
and Status = 1           
and BillType = 'Agency'           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE));          
          
;WITH PolicyLOBs AS (          
SELECT DISTINCT           
PD.PolicyNumber,           
PD.AgentDetail_Id,          
IND.Name AS InsuredName,          
LOB.Name AS LOBName,          
CD.Name as CarrierName          
FROM Accounting_PolicyDetails PD          
JOIN Accounting_InsuredDetails IND ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1          
JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id = PD.CarrierDetail_Id and CD.Status = 1          
JOIN Accounting_Invoices IES ON IES.PolicyDetails_Id = PD.ID AND IES.Status = 1          
JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = IES.ID AND ID.Status = 1          
JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1          
WHERE PD.Client_Id = @Client_Id AND PD.BillType = 'Agency' AND PD.Status = 1           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE))          
)          
          
SELECT           
PolicyNumber,          
InsuredName,          
AgentDetail_Id,          
CarrierName,          
STUFF((          
SELECT DISTINCT ', ' + L2.LOBName          
FROM PolicyLOBs L2          
WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName          
FOR XML PATH(''), TYPE          
).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames          
FROM PolicyLOBs L1          
GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;          
          
;WITH AggregatedPremiumCommissionInvoiceDetails as (          
select I.Id as InvoiceId, I.PolicyDetails_Id, I.InvoiceNo,          
SUM(CASE WHEN LI.Code = 'Premium' and LUD.Code = 'AR' THEN ID.Amount ELSE 0 END) AS PremiumAR,          
SUM(CASE WHEN LI.Code = 'AgencyBrokerCommission' and LUD.Code = 'C' THEN ID.Amount ELSE 0 END) AS AgentCommission,          
SUM(CASE WHEN LI.Code = 'MGACommission' and LUD.Code = 'IN' THEN ID.Amount ELSE 0 END) AS MGACommission          
from Accounting_Invoices I           
join Accounting_PolicyDetails PD on PD.ID = I.PolicyDetails_Id and PD.Client_Id = @Client_Id and PD.Status = 1 AND Pd.BillType = 'Agency'          
join accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and ID.Status = 1          
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id and BLD.Status = 1          
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1          
join Accounting_LU_LineItems LI on LI.LineItem_Id = LD.LineItem_Id and LI.Status = 1          
join Accounting_LU_Directions LUD on LUD.Direction_Id = LD.Direction_Id and LUD.Status = 1          
where I.Status = 1           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE))          
group by I.Id, I.PolicyDetails_Id, I.InvoiceNo          
),          
          
FeeData as (          
select I.PolicyDetails_Id, I.Id as Invoice_Id, ID.Id as InvoiceDetail_Id,          
concat(PD.TransactionType,' ',LI.Name) as LineItemDescription, I.InvoiceNo as IdNo,          
PD.EffectiveDate, PD.ExpirationDate, ID.Amount as GrossAmount, 0 as AgentCommPercentage,          
0 as AgentCommission, ID.Amount as NetAmount, I.DueDate, PD.PolicyNumber, ID.RemainingBalance          
from Accounting_Invoices I       
join Accounting_PolicyDetails PD on PD.ID = I.PolicyDetails_Id and PD.Client_Id = @Client_Id and PD.Status = 1 AND Pd.BillType = 'Agency'          
join accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and ID.Status = 1          
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id and BLD.Status = 1          
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1          
join Accounting_LU_LineItems LI on LI.LineItem_Id = LD.LineItem_Id and LI.Status = 1          
join Accounting_LU_LineItemTypes LIT on LIT.LineItemType_Id = LI.LineItemType_Id and LIT.Status = 1          
join Accounting_LU_Directions LUD on LUD.Direction_Id = LD.Direction_Id and LUD.Status = 1          
join Accounting_LU_LOBs LOB on LOb.LOB_Id = ID.LOB_Id and LOB.Status = 1          
where I.Status = 1 and (LIT.Code = 'Fee' or LIT.Code = 'Tax') and LUD.Code = 'AR'           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE))          
),    
PaymentApplication as     
(    
select I.PolicyDetails_Id, I.Id as Invoice_Id, ID.Id as InvoiceDetail_Id,          
'Cash Receipt' as LineItemDescription, concat('Payment #',' ',p.Payment_Id) as IdNo,          
cast(p.CreatedOn as Date) as EffectiveDate, PD.ExpirationDate, 0 as GrossAmount, 0 as AgentCommPercentage,          
0 as AgentCommission, case when P.PaymentDirection_Id = 1 then -PALI.AppliedAmount else Abs(PALI.AppliedAmount) end as NetAmount, I.DueDate, PD.PolicyNumber, 0 as RemainingBalance          
from Accounting_Invoices I       
join Accounting_PolicyDetails PD on PD.ID = I.PolicyDetails_Id and PD.Client_Id = @Client_Id and PD.Status = 1 AND Pd.BillType = 'Agency'          
join accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and ID.Status = 1          
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id and BLD.Status = 1          
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1          
join Accounting_LU_LineItems LI on LI.LineItem_Id = LD.LineItem_Id and LI.Status = 1          
join Accounting_LU_LineItemTypes LIT on LIT.LineItemType_Id = LI.LineItemType_Id and LIT.Status = 1          
join Accounting_LU_Directions LUD on LUD.Direction_Id = LD.Direction_Id and LUD.Status = 1          
join Accounting_LU_LOBs LOB on LOb.LOB_Id = ID.LOB_Id and LOB.Status = 1    
join Accounting_PaymentApplicationLineItems PALI on PALI.InvoiceLineItem_Id = ID.Id and PALI.Status = 1    
join Accounting_PaymentApplications PA on PA.PaymentApplication_Id = PALI.PaymentApplication_Id and PA.Status = 1    
join Accounting_Payments P on P.Payment_Id = PA.Payment_Id and P.Status = 1    
where I.Status = 1 and LUD.Code = 'AR'           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE))          
    
)    
          
select I.PolicyDetails_Id, I.Id as Invoice_Id, ID.Id as InvoiceDetail_Id,          
concat(PD.TransactionType,' ',LI.Name) as LineItemDescription, I.InvoiceNo as IdNo,          
PD.EffectiveDate, PD.ExpirationDate, APCID.AgentCommission + PremiumAR as GrossAmount,          
((APCID.AgentCommission)/(NullIf(APCID.AgentCommission + PremiumAR, 0))*100) as AgentCommPercentage,          
APCID.AgentCommission, APCID.PremiumAR as NetAmount, I.DueDate, PD.PolicyNumber ,ID.RemainingBalance          
from Accounting_Invoices I           
join Accounting_PolicyDetails PD on PD.ID = I.PolicyDetails_Id and PD.Client_Id = @Client_Id and PD.Status = 1 AND Pd.BillType = 'Agency'          
join accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and ID.Status = 1          
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id and BLD.Status = 1          
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1          
join Accounting_LU_LineItems LI on LI.LineItem_Id = LD.LineItem_Id and LI.Status = 1          
join Accounting_LU_Directions LUD on LUD.Direction_Id = LD.Direction_Id and LUD.Status = 1          
join AggregatedPremiumCommissionInvoiceDetails APCID on APCID.InvoiceId = I.Id           
join Accounting_LU_LOBs LOB on LOb.LOB_Id = ID.LOB_Id and LOB.Status = 1          
where I.Status = 1 and LI.Code = 'Premium' and LUD.Code = 'AR'           
AND (@PolicyFromDate IS NULL OR CAST(EffectiveDate AS DATE) >= CAST(@PolicyFromDate AS DATE))          
AND (@PolicyToDate IS NULL OR CAST(EffectiveDate AS DATE) <= CAST(@PolicyToDate AS DATE))          
          
Union all          
          
select * from FeeData       
union all     
select * from PaymentApplication    
End  go

Create Clustered Index CLU_Accounting_LU_SQLInputParameters_Status_SQLQuery_Id on Accounting_LU_SQLInputParameters
(
Status desc,
SQLQuery_Id
)
go

update Accounting_LU_SQLInputParameters set Status =0 where SQLParameter_Id = 12 and Status = 1
go

Insert into Accounting_LU_PredefinedParameters(Name, Sequence)
Values ('Client_Id',1)
go

update Accounting_LU_SQLInputParameters set PredefinedParameter_Id = 1 where  SQLParameter_Id in (9,13) and Status = 1
go




