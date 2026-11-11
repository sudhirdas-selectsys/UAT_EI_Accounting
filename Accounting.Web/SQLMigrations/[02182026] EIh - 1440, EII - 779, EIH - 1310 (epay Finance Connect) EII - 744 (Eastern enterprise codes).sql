ALTER Procedure [dbo].[USP_Accounting_InsertStateDetails]
(
@StateDetail_Id int output,
@Client_Id bigint,
@Name varchar(250)
)
As Begin
	if not exists (Select 1 from Accounting_StateDetails where isnull(Name,'') = isnull(@Name,'') and isnull(Client_Id,'') = isnull(@Client_Id,'') and Status = 1)
	Begin 
		Insert into Accounting_StateDetails (Client_Id,Name) Values (@Client_Id,@Name) set @StateDetail_Id = Scope_Identity()
	End
	else --added
	   begin
	     set @StateDetail_Id = (Select StateDetail_Id from Accounting_StateDetails where isnull(Name,'') = isnull(@Name,'') and isnull(Client_Id,'') = isnull(@Client_Id,'') and Status = 1)
       end
End
go
CREATE or alter  proc USP_Accounting_GetPaymentsSearchResults_paging                                    
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
AND ((@Payment_Id IS NULL or @Payment_Id = 0) OR p.Payment_Id=@Payment_Id)   --added 0 condition   
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
AND ((@Payment_Id IS NULL or @Payment_Id = 0) OR p.Payment_Id=@Payment_Id)   --added 0 condition    
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
AND ((@Payment_Id IS NULL or @Payment_Id = 0) OR p.Payment_Id=@Payment_Id) --added 0 condition     
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                              
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)                  
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0)))              
end   
go
--Add new record in Accounting_ClientSettings--
--insert into Accounting_ClientSettings (Client_Id,SettingKey,SettingValue) values
--(10008,'AllowExternalAccountingID','true')

--Add New Column (EI_Accounting_Id) to Accounting_AgentDetails--
--alter table Accounting_AgentDetails
--add EI_Accounting_Id varchar(250)

--USP_Acconting_InsertUpdateAgentDetails--
--CREATE PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]        
--(        
--    @AgentDetail_Id BIGINT OUTPUT,        
--    @AgentReference_Id VARCHAR(250) = NULL,        
--    @Name VARCHAR(200) = NULL,        
--    @Address1 VARCHAR(500) = NULL,        
--    @Address2 VARCHAR(500) = NULL,        
--    @City VARCHAR(100) = NULL,        
--    @Country VARCHAR(100) = NULL,        
--    @County VARCHAR(100) = NULL,        
--    @State VARCHAR(10) = NULL,        
--    @ZipCode VARCHAR(10) = NULL,      
--    @FEIN VARCHAR(200) = NULL,      
--    @Client_Id BIGINT,        
--    @Email VARCHAR(250) = NULL,    
--    @AccountNumber VARCHAR(60) = NULL,    
--    @RoutingNumber VARCHAR(40) = NULL,  
--    @EI_Accounting_Id VARCHAR(250) = NULL  
--)        
--AS        
--BEGIN        
--    SET NOCOUNT ON;        
        
--    DECLARE @ExistingAgentDetail_Id BIGINT;        
        
--    SELECT @ExistingAgentDetail_Id = AgentDetail_Id        
--    FROM Accounting_AgentDetails        
--    WHERE AgentReference_Id = @AgentReference_Id        
--      AND Client_Id = @Client_Id        
--      AND Is_Deleted = 0        
--      AND Status = 1;        
        
--    IF @ExistingAgentDetail_Id IS NOT NULL        
--    BEGIN        
--        UPDATE Accounting_AgentDetails        
--        SET        
--            Name = @Name,        
--            Address1 = @Address1,        
--            Address2 = @Address2,        
--            City = @City,        
--            Country = @Country,        
--            County = @County,        
--            State = @State,        
--            ZipCode = @ZipCode,        
--            Email = @Email,      
--            FEIN = @FEIN,    
--   AccountNumber = @AccountNumber,    
--   RoutingNumber = @RoutingNumber,  
--   EI_Accounting_Id = @EI_Accounting_Id,  
--            UpdatedOn = GETDATE()        
--        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;        
        
--        SET @AgentDetail_Id = @ExistingAgentDetail_Id;        
--    END        
--    ELSE        
--    BEGIN        
--        INSERT INTO Accounting_AgentDetails        
--        (        
--            AgentReference_Id, Name, Address1, Address2, City,        
--            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber,EI_Accounting_Id      
--        )        
--        VALUES        
--        (        
--            @AgentReference_Id, @Name, @Address1, @Address2, @City,        
--            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber,@EI_Accounting_Id       
--        );        
        
--        SET @AgentDetail_Id = SCOPE_IDENTITY();        
--    END        
--END 

----------------------------------------------------------------------------
--Create or alter proc USP_Accounting_GetExternalAccountInformationByAccountNumber_V1    
--(    
--@AccountNumber varchar(250)  ,  
--@Client_Id bigint   
--)    
--as    
--Begin 
--declare @AllowExternalAccountingID varchar(100) = (select SettingValue from Accounting_ClientSettings where Client_id = @Client_Id and SettingKey = 'AllowExternalAccountingID' and status = 1);
--if (@AllowExternalAccountingID is not null and @AllowExternalAccountingID <> '')
--begin
--select cast(1 as tinyint) as EntityType_Id, AgentDetail_Id as Entity_Id,EI_Accounting_Id as AccountNumber,Client_Id   from Accounting_AgentDetails where EI_Accounting_Id = @AccountNumber and status = 1 and Client_id = @Client_Id
--end
--else
--begin
--select * from Accounting_ExternalAccountInformation where AccountNumber = @AccountNumber and Client_Id = @Client_Id and Status = 1    
--end
--end


--update Accounting_AgentDetails set EI_Accounting_Id = 'GA1963', UpdatedOn = Getdate()	where AgentReference_Id = 23 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'TN2125', UpdatedOn = Getdate()	where AgentReference_Id = 98 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'MO2966', UpdatedOn = Getdate()	where AgentReference_Id = 421 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'CA3078', UpdatedOn = Getdate()	where AgentReference_Id = 720 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'GA1063', UpdatedOn = Getdate()	where AgentReference_Id = 1263 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'GA1384', UpdatedOn = Getdate()	where AgentReference_Id = 1341 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'AR1174', UpdatedOn = Getdate()	where AgentReference_Id = 1351 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'NC1440', UpdatedOn = Getdate()	where AgentReference_Id = 1352 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'GA2173', UpdatedOn = Getdate()	where AgentReference_Id = 2088 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'GA1697', UpdatedOn = Getdate()	where AgentReference_Id = 2360 and Client_id = 10008 and status = 1
--update Accounting_AgentDetails set EI_Accounting_Id = 'CA2680', UpdatedOn = Getdate()	where AgentReference_Id = 2726 and Client_id = 10008 and status = 1



-- Deleted Procedure USP_Accounting_GetAllInvoicesBasedOnLineItemId optimize it in proc USP_Accounting_GetAllOpenInvoices

DROP proc USP_Accounting_GetAllInvoicesBasedOnLineItemId

-- modified proc and optimized it USP_Accounting_GetAllOpenInvoices


 
go
Create Procedure USP_Accounting_GetAgentDetailsByAccountNumber(  
@EI_Accounting_Id varchar(250),  
@Client_Id bigint  
)  
As Begin  
select * from Accounting_AgentDetails where Client_Id = @Client_Id and EI_Accounting_Id = @EI_Accounting_Id and Status = 1  
End
go
drop procedure  USP_Accounting_GetAgentDetailsByAccountNumber
go
Alter table Accounting_InsuredDetails
Add Physical_Address1 varchar(250)
Alter table Accounting_InsuredDetails
Add Physical_Address2 varchar(250)
Alter table Accounting_InsuredDetails
Add Physical_City varchar(250)
Alter table Accounting_InsuredDetails
Add Physical_State varchar(250)
Alter table Accounting_InsuredDetails
Add Physical_ZipCode varchar(250)
Alter table Accounting_InsuredDetails
Add Physical_Country varchar(250)

go

CREATE or alter proc [dbo].[USP_Accounting_InsertUpdateInsuredDetails]              
(                  
@InsuredDetail_Id bigint output                  
,@InsuredReference_Id varchar(250) = null                          
,@Name varchar(200) = null                          
,@Address1 varchar(500) = null                          
,@Address2 varchar(500) = null                          
,@City varchar(100) = null                          
,@Country varchar(100) = null                          
,@County varchar(100) = null                          
,@State varchar(10) = null                          
,@ZipCode varchar(10) = null                      
,@Client_Id bigint = null    
,@FEIN VARCHAR(200) = NULL    
,@DBA VARCHAR(200) = NULL  
,@Physical_Address1 varchar(250) = null  
,@Physical_Address2 varchar(250) = null  
,@Physical_City varchar(250) = null  
,@Physical_State varchar(250) = null  
,@Physical_ZipCode varchar(250) = null  
,@Physical_Country varchar(250) = null  
)                          
As                          
Begin               
SET NOCOUNT ON;            
            
DECLARE @ExistingInsuredDetail_Id BIGINT;            
            
SELECT @ExistingInsuredDetail_Id = InsuredDetail_Id             
FROM Accounting_InsuredDetails             
WHERE InsuredReference_Id = @InsuredReference_Id             
AND Client_Id = @Client_Id             
AND Is_Deleted = 0             
AND Status = 1;            
            
IF @ExistingInsuredDetail_Id IS NOT NULL            
BEGIN            
IF EXISTS (            
SELECT 1             
FROM Accounting_InsuredDetails             
WHERE InsuredDetail_Id = @ExistingInsuredDetail_Id            
AND (            
ISNULL(Name, '') <> ISNULL(@Name, '')            
OR ISNULL(Address1, '') <> ISNULL(@Address1, '')            
OR ISNULL(Address2, '') <> ISNULL(@Address2, '')            
OR ISNULL(City, '') <> ISNULL(@City, '')            
OR ISNULL(Country, '') <> ISNULL(@Country, '')            
OR ISNULL(County, '') <> ISNULL(@County, '')            
OR ISNULL(State, '') <> ISNULL(@State, '')            
OR ISNULL(ZipCode, '') <> ISNULL(@ZipCode, '')    
OR ISNULL(FEIN, '') <> ISNULL(@FEIN, '')    
OR ISNULL(DBA, '') <> ISNULL(@DBA, '')    
  
OR ISNULL(Physical_Address1, '') <> ISNULL(@Physical_Address1, '')            
OR ISNULL(Physical_Address2, '') <> ISNULL(@Physical_Address2, '')            
OR ISNULL(Physical_City, '') <> ISNULL(@Physical_City, '')            
OR ISNULL(Physical_Country, '') <> ISNULL(@Physical_Country, '')            
OR ISNULL(Physical_State, '') <> ISNULL(@Physical_State, '')            
OR ISNULL(Physical_ZipCode, '') <> ISNULL(@Physical_ZipCode, '')    
  
)            
)            
BEGIN            
UPDATE Accounting_InsuredDetails             
SET             
Name = @Name,            
Address1 = @Address1,            
Address2 = @Address2,            
City = @City,            
Country = @Country,            
County = @County,            
State = @State,            
ZipCode = @ZipCode,    
FEIN = @FEIN,    
DBA = @DBA,    
Physical_Address1 = @Physical_Address1,
Physical_Address2 = @Physical_Address2,
Physical_City = @Physical_City,
Physical_State = @Physical_State,
Physical_ZipCode = @Physical_ZipCode,
Physical_Country = @Physical_Country
,UpdatedOn = Getdate()            
WHERE InsuredDetail_Id = @ExistingInsuredDetail_Id            
END            
            
SET @InsuredDetail_Id = @ExistingInsuredDetail_Id;            
END            
ELSE            
BEGIN            
insert into Accounting_InsuredDetails                          
(                          
InsuredReference_Id                          
,Name                          
,Address1                          
,Address2                          
,City                          
,Country                          
,County                          
,State                          
,ZipCode                    
,Client_Id    
,FEIN    
,DBA   
,Physical_Address1  
,Physical_Address2  
,Physical_City  
,Physical_State  
,Physical_ZipCode  
,Physical_Country  
)                          
Values                          
(                          
@InsuredReference_Id                          
,@Name                          
,@Address1                          
,@Address2                          
,@City                      
,@Country                          
,@County                          
,@State                          
,@ZipCode                    
,@Client_Id    
,@FEIN    
,@DBA   
,@Physical_Address1  
,@Physical_Address2  
,@Physical_City  
,@Physical_State  
,@Physical_ZipCode  
,@Physical_Country  
);            
                    
set @InsuredDetail_Id = SCOPE_IDENTITY();            
END            
END   
go

CREATE     Proc USP_Accounting_GetAllOpenInvoices             
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
)  
  
select PD.PolicyNumber, PD.EffectiveDate, I.Id as InvoiceId, I.InvoiceNo, PD.Id as PolicyDetails_Id, I.DueDate,                
Sum(ID.Amount) as InvoiceAmount, Sum(ID.RemainingBalance) as RemainingBalance,                
case when @EntityType_Id = 1 then AD.Name when @EntityType_Id = 2 then IND.Name when @EntityType_Id = 3 then CD.Name end as PayerName    
,IND.Name as InsuredName,  
Pd.ExpirationDate, STRING_AGG(LOB.Code, ', ') as CoverageCode, cast(isnull(I.Is_Financed,0) as int) as FlagFinanced,   
case PD.BillType when 'Agency' then 'AB' when 'Insured' then 'IB' when 'Direct' then 'DB' else '' end as BillingType,  
MD.MinimumPremium as MinimumPremium, CASE PD.TransactionType WHEN 'NEW' THEN 'NBS' ELSE PD.TransactionType END AS [InvoiceTypeID],  PD.CustomerType,  
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
join Accounting_LU_LOBs LOB on LOB.LOB_Id = ID.LOB_id and LOB.Status = 1  
join Accounting_Clients C on C.Client_Id = PD.Client_Id and C.Status = 1  
left join MinimumPremiumData MD on MD.Invoice_id = I.Id  
left join TotalFees TF on TF.Invoice_id = I.Id  
left join TotalTaxes TA on TA.Invoice_id = I.Id  
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
C.ZipCode,  AD.Name,AD.Address2,AD.Address1,AD.City,AD.State,AD.ZipCode  
order by I.Id Desc            
End    