


--modified in SP to get response of CustomerType as CL or PL
ALTER   Proc USP_Accounting_GetAllOpenInvoices     
(                  
@Client_Id bigint,                  
@Entity_Id bigint,                  
@EntityType_Id tinyint                  
)                  
As                  
Begin      
with MinimumPremiumData as    
(    
select I.PolicyDetails_Id, ID.Invoice_Id, Sum(ID.Amount) as MinimumPremium from Accounting_Invoices I    
join Accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and Id.Status = 1                  
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = Id.BillingType_LineItems_Directions_Id and BLD.Status = 1                  
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1       
JOIN dbo.Accounting_LU_LineItems_Clients AS LIC  ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
JOIN dbo.Accounting_LU_LineItems AS LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
where LD.Direction_Id = 1 and LI.LineItemType_Id in (1)    
group by I.PolicyDetails_Id, ID.Invoice_Id    
)    
,TotalFees as     
(    
select I.PolicyDetails_Id, ID.Invoice_Id, Sum(ID.Amount) as TotalFees from Accounting_Invoices I    
join Accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and Id.Status = 1                  
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = Id.BillingType_LineItems_Directions_Id and BLD.Status = 1                  
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1       
JOIN dbo.Accounting_LU_LineItems_Clients AS LIC  ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
JOIN dbo.Accounting_LU_LineItems AS LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
where LD.Direction_Id = 1 and LI.LineItemType_Id in (4)    
group by I.PolicyDetails_Id, ID.Invoice_Id    
)    
,TotalTaxes as     
(    
select I.PolicyDetails_Id, ID.Invoice_Id, Sum(ID.Amount) as TotalFees from Accounting_Invoices I    
join Accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and Id.Status = 1                  
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = Id.BillingType_LineItems_Directions_Id and BLD.Status = 1                  
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1       
JOIN dbo.Accounting_LU_LineItems_Clients AS LIC  ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
JOIN dbo.Accounting_LU_LineItems AS LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
where LD.Direction_Id = 1 and LI.LineItemType_Id in (3)    
group by I.PolicyDetails_Id, ID.Invoice_Id    
),CoverageAgg AS  
(  
    SELECT  
        DC.Invoice_Id,  
        STRING_AGG(LOB.Code, ', ') AS CoverageCode  
    FROM  
    (  
        SELECT DISTINCT Invoice_Id, LOB_Id  
        FROM Accounting_InvoiceDetails  
        WHERE Status = 1  
    ) DC  
    JOIN Accounting_LU_LOBs LOB  
        ON LOB.LOB_Id = DC.LOB_Id  
        AND LOB.Status = 1  
    GROUP BY DC.Invoice_Id  
)  
select PD.PolicyNumber, PD.EffectiveDate, I.Id as InvoiceId, I.InvoiceNo, PD.Id as PolicyDetails_Id, I.DueDate,                  
Sum(ID.Amount) as InvoiceAmount, Sum(ID.RemainingBalance) as RemainingBalance,                  
case when @EntityType_Id = 1 then AD.Name when @EntityType_Id = 2 then IND.Name when @EntityType_Id = 3 then CD.Name end as PayerName      
,IND.Name as InsuredName,    
Pd.ExpirationDate, CA.CoverageCode, cast(isnull(I.Is_Financed,0) as int) as FlagFinanced,     
case PD.BillType when 'Agency' then 'AB' when 'Insured' then 'IB' when 'Direct' then 'DB' else '' end as BillingType,    
MD.MinimumPremium as MinimumPremium, CASE PD.TransactionType WHEN 'NEW' THEN 'NBS' ELSE PD.TransactionType END AS [InvoiceTypeID],
case PD.CustomerType when 'Commercial Line' then 'CL' when 'Personal Line' then 'PL' else PD.CustomerType end as CustomerType,  
 C.Phone AS [ClientPhone], C.Email AS [ClientEmail], IND.DBA AS [InsuredDBA],     
 -- Insured Physical      
        IND.Physical_Address2 AS [InsuredPhysicalSuite],      
        IND.Physical_Address1 AS [InsuredPhysicalStreet],      
        IND.Physical_City     AS [InsuredPhysicalCity],      
        IND.Physical_State    AS [InsuredPhysicalState],      
        IND.Physical_ZipCode  AS [InsuredPhysicalPostalCode],      
      
        -- Insured Mailing      
        IND.Address2 AS [InsuredMailingSuite],      
        IND.Address1 AS [InsuredMailingStreet],      
        IND.City     AS [InsuredMailingCity],       
        IND.State    AS [InsuredMailingState],      
        IND.ZipCode  AS [InsuredMailingPostalCode],      
      
        -- Carrier      
        CD.Name     AS [CarrierName],      
        CD.Address2 AS [CarrierSuite],      
        CD.Address1 AS [CarrierStreet],      
        CD.City     AS [CarrierCity],      
        CD.State    AS [CarrierState],      
        CD.ZipCode  AS [CarrierPostalCode],      
      
        -- MGA Contact (Client)      
        C.CompanyName AS [MGAContactName],      
        C.Phone       AS [MGAContactPhone],      
        C.Address     AS [MGAContactStreet],      
        C.City        AS [MGAContactCity],      
        C.State       AS [MGAContactState],      
        C.ZipCode     AS [MGAContactPostalCode],      
      
        -- Retail Agent (only when Agency billed)      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Name     END AS [RetailAgentName],      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Address2 END AS [RetailAgentSuite],      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Address1 END AS [RetailAgentStreet],      
        CASE WHEN PD.BillType = 'Agency' THEN AD.City     END AS [RetailAgentCity],      
        CASE WHEN PD.BillType = 'Agency' THEN AD.State    END AS [RetailAgentState],      
        CASE WHEN PD.BillType = 'Agency' THEN AD.ZipCode  END AS [RetailAgentPostalCode]     
from Accounting_PolicyDetails PD                  
left join Accounting_AgentDetails AD on AD.AgentDetail_Id = PD.AgentDetail_Id and AD.Status = 1                  
left join Accounting_InsuredDetails IND on IND.InsuredDetail_Id = Pd.InsuredDetail_Id and IND.Status = 1                  
left join Accounting_CarrierDetails CD on CD.CarrierDetail_Id = PD.CarrierDetail_Id and CD.Status = 1                  
join Accounting_Invoices I on PD.ID = I.PolicyDetails_Id and I.Status = 1                  
join Accounting_InvoiceDetails ID on ID.Invoice_Id = I.Id and Id.Status = 1                  
join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.BillingType_LineItems_Directions_Id = Id.BillingType_LineItems_Directions_Id and BLD.Status = 1                  
join Accounting_LU_LineItems_Directions LD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and LD.Status = 1        
join Accounting_Clients C on C.Client_Id = PD.Client_Id and C.Status = 1    
left join MinimumPremiumData MD on MD.Invoice_id = I.Id    
left join TotalFees TF on TF.Invoice_id = I.Id    
left join TotalTaxes TA on TA.Invoice_id = I.Id  
left join CoverageAgg CA on CA.Invoice_Id = I.Id  
where                   
PD.Client_Id = @Client_Id                   
and PD.Status = 1                   
and LD.Direction_Id = 1                   
and I.CreatedOn <= Getdate()                   
and (                  
(@EntityType_Id = 1 and AD.AgentDetail_Id = @Entity_Id and PD.BillType = 'Agency') or                  
(@EntityType_Id = 2 and IND.InsuredDetail_Id = @Entity_Id and PD.BillType = 'Insured') or                  
(@EntityType_Id = 3 and CD.CarrierDetail_Id = @Entity_Id and PD.BillType = 'Direct')                  
)                  
and I.Remainingbalance <> 0                  
group by                   
I.Id, PD.PolicyNumber, PD.EffectiveDate, I.InvoiceNo,                   
PD.Id, I.DueDate, I.Amount, I.RemainingBalance,                  
AD.Name, IND.Name, CD.Name,IND.Name,Pd.ExpirationDate,I.Is_Financed, PD.BillType,MD.MinimumPremium, PD.TransactionType, PD.CustomerType    
,C.Phone, C.Email, IND.DBA,IND.Physical_Address2,      
IND.Physical_Address1,      
IND.Physical_City     ,      
IND.Physical_State,      
IND.Physical_ZipCode,      
IND.Address2,      
IND.Address1,      
IND.City,       
IND.State,      
IND.ZipCode,      
CD.Name,      
CD.Address2,      
CD.Address1,      
CD.City,      
CD.State,      
CD.ZipCode,      
C.CompanyName,      
C.Phone,      
C.Address,      
C.City,      
C.State,      
C.ZipCode,  AD.Name,AD.Address2,AD.Address1,AD.City,AD.State,AD.ZipCode,CA.CoverageCode    
order by I.Id Desc              
End 

