--Alter table Accounting_LU_LineItems_Clients
		   --Add Is_Commissionable bit 
		   USP_Accounting_InsertUpdateLineItems_Clients

--ALTER Proc USP_Accounting_InsertUpdateLineItems_Clients            
--(            
--@LineItems_Clients_Id bigint output,            
--@LineItem_Id int,            
--@Client_Id bigint ,          
--@Is_Passthrough bit = 0 ,      
--@Is_Excluded bit = 0   ,    
--@Is_Commissionable bit = 0  
--)            
--As            
--Begin          
--set nocount on;          
--if exists (select 1 from Accounting_LU_LineItems_Clients where LineItems_Clients_Id = @LineItems_Clients_Id and Status = 1)          
--begin          
--update Accounting_LU_LineItems_Clients set LineItem_Id = @LineItem_Id, Is_Passthrough = @Is_Passthrough,Is_Excluded = @Is_Excluded.Is_Commissionable=@Is_Commissionable, UpdatedOn = getdate()          
--where LineItems_Clients_Id = @LineItems_Clients_Id and Status = 1        
--end          
--else          
--begin          
--insert into Accounting_LU_LineItems_Clients (LineItem_Id, Client_Id, Is_Passthrough,Is_Excluded,Is_Commissionable)            
--values (@LineItem_Id, @Client_Id, @Is_Passthrough,@Is_Excluded,@Is_Commissionable)            
--set @LineItems_Clients_Id = scope_identity()           
--end          
--End   

USP_Accounting_GetLineItemsDirections_V1

--Alter proc USP_Accounting_GetLineItemsDirections_V1       
--(      
--@Lineitems_Clients_Id bigint     
--)      
--As      
--Begin      
--select LIC.LineItems_Clients_Id,LIC.Client_Id,LIC.LineItem_Id,LIC.Is_Passthrough,LIC.Is_Excluded,LIC.Is_Commissionable , LI.Name as LineItemName, LID.LineItems_Directions_Id,LID.EntityType_Id,BLD.BillingType_LineItems_Directions_Id,      
--LD.Direction_Id,BT.BillingType_Id,BT.Name as BillingTypeName,LD.Name as DirectionName,LIT.LineItemType_Id      
--from Accounting_LU_LineItems_Clients LIC       
--inner join Accounting_LU_LineItems LI on LI.LineItem_Id = LIC.LineItem_Id and LI.Status = 1      
--inner join Accounting_LU_LineItemTypes LIT on LIT.LineItemType_Id = LI.LineItemType_Id and LIT.Status = 1      
--left join Accounting_LU_LineItems_Directions LID on LID.LineItems_Clients_Id = LIC.LineItems_Clients_Id and LID.Status = 1       
--left join Accounting_LU_BillingType_LineItems_Directions BLD on BLD.LineItems_Directions_Id = LID.LineItems_Directions_Id and BLD.Status = 1      
--left join Accounting_LU_Directions LD on LD.Direction_Id = LID.Direction_Id and LD.Status = 1      
--left join Accounting_LU_BillingTypes BT on BT.BillingType_Id = BLD.BillingType_Id and BT.Status = 1      
--where LIC.Lineitems_Clients_Id = @Lineitems_Clients_Id and LIC.Status = 1      
--end 

USP_Accounting_GetExistingLineitemMapping

ALTER   Procedure USP_Accounting_GetExistingLineitemMapping       
(    
@Client_Id bigint,    
@BillTypeList varchar(250),    
@DirectionList varchar(250),    
@LineItem_Id tinyint,       
@EntityType_Id tinyint = null,       
@Is_Passthrough bit = null,      
@Is_Excluded bit = null,          
@Is_Commissionable bit = null          
)      
As Begin       
DECLARE @BillTypesTable TABLE (BillingType_Id NVARCHAR(100))               
DECLARE @DirectionsTable TABLE (Direction_Id NVARCHAR(100))               
INSERT INTO @BillTypesTable(BillingType_Id) SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@BillTypeList, ',');                
INSERT INTO @DirectionsTable(Direction_Id) SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@DirectionList, ',');                
With LineItemMappingDetails as       
(       
select LIC.LineItem_Id,LD.Direction_Id,LD.LineItems_Directions_Id,LD.    
EntityType_Id, BLD.BillingType_LineItems_Directions_Id, BLD.BillingType_Id ,LIC.Is_Excluded, LIC.Is_Passthrough,LIC.Is_Commissionable     
from Accounting_LU_LineItems_Clients LIC         
inner join Accounting_LU_LineItems LI on LI.LineItem_Id = LIC.LineItem_Id and LI.Status = 1        
inner join Accounting_LU_LineItems_Directions LD on  LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  and LD.Status = 1     
inner join Accounting_LU_BillingType_LineItems_Directions BLD on LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id and BLD.Status = 1      
where LIC.Status = 1 and LIC.Client_Id = @Client_Id     
)       
select * from LineItemMappingDetails        
Where LineItem_Id = @LineItem_Id       
AND BillingType_Id IN (select BillingType_Id from @BillTypesTable)      
AND Direction_Id IN (select Direction_Id from @DirectionsTable)       
AND (@EntityType_Id IS NULL OR (Direction_Id = 2 and  EntityType_Id = @EntityType_Id and Is_Passthrough = 1 ))       
AND ((@Is_Excluded IS NULL) or (@Is_Excluded = 0) OR (Is_Excluded = @Is_Excluded))  
AND (@Is_Passthrough IS NULL OR (Is_Passthrough = @Is_Passthrough))  
AND (@Is_Commissionable IS NULL OR (Is_Commissionable = @Is_Commissionable))  
End 