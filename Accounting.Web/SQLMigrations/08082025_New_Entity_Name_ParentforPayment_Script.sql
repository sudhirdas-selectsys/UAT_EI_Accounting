Alter proc [dbo].[USP_Acconting_InsertUpdateAgentDetails]
(    
@AgentDetail_Id bigint output    
,@AgentReference_Id varchar(250) = null            
,@Name varchar(200) = null            
,@Address1 varchar(500) = null            
,@Address2 varchar(500) = null            
,@City varchar(100) = null            
,@Country varchar(100) = null            
,@County varchar(100) = null            
,@State varchar(10) = null            
,@ZipCode varchar(10) = null        
,@Client_Id bigint        
)            
As            
Begin
if not exists (select 1 from Accounting_AgentDetails where ISNULL(AgentReference_Id, '') = ISNULL(@AgentReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)            
begin            
update Accounting_AgentDetails set Status = 0 where AgentReference_Id = @AgentReference_Id and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id           
insert into Accounting_AgentDetails            
(            
AgentReference_Id            
,Name            
,Address1            
,Address2            
,City            
,Country            
,County            
,State            
,ZipCode      
,Client_Id      
)            
Values            
(            
@AgentReference_Id            
,@Name            
,@Address1            
,@Address2            
,@City            
,@Country            
,@County            
,@State            
,@ZipCode      
,@Client_Id      
)      
set @AgentDetail_Id = SCOPE_IDENTITY()    
end            
else    
begin    
set @AgentDetail_Id = (select AgentDetail_Id from Accounting_AgentDetails where ISNULL(AgentReference_Id, '') = ISNULL(@AgentReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)    
end    
end
go

Alter proc [dbo].[USP_Acconting_InsertUpdateCarrierDetails]
(      
@CarrierDetail_Id bigint output    
,@CarrierReference_Id varchar(250) = null            
,@Name varchar(200) = null            
,@Address1 varchar(500) = null            
,@Address2 varchar(500) = null            
,@City varchar(100) = null            
,@Country varchar(100) = null            
,@County varchar(100) = null            
,@State varchar(10) = null            
,@ZipCode varchar(10) = null         
,@Client_Id bigint        
)            
As            
Begin            
if not exists (select 1 from Accounting_CarrierDetails where ISNULL(CarrierReference_Id, '') = ISNULL(@CarrierReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)            
begin            
update Accounting_CarrierDetails set Status = 0 where CarrierReference_Id = @CarrierReference_Id and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id           
insert into Accounting_CarrierDetails            
(            
CarrierReference_Id            
,Name            
,Address1            
,Address2            
,City            
,Country            
,County            
,State            
,ZipCode          
,Client_Id      
)            
Values            
(            
@CarrierReference_Id            
,@Name            
,@Address1            
,@Address2            
,@City            
,@Country            
,@County            
,@State            
,@ZipCode      
,@Client_Id      
)     
set @CarrierDetail_Id = SCOPE_IDENTITY()    
end            
else    
begin    
set @CarrierDetail_Id = (select CarrierDetail_Id from Accounting_CarrierDetails where ISNULL(CarrierReference_Id, '') = ISNULL(@CarrierReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)    
end    
end
go

Alter proc [dbo].[USP_Accounting_InsertUpdateInsuredDetails]
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
)      
As      
Begin      
if not exists (select 1 from Accounting_InsuredDetails where ISNULL(InsuredReference_Id, '') = ISNULL(@InsuredReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)              
begin              
update Accounting_InsuredDetails set Status = 0 where InsuredReference_Id = @InsuredReference_Id and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id             
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
)        
set @InsuredDetail_Id = SCOPE_IDENTITY()      
end              
else      
begin      
set @InsuredDetail_Id = (select InsuredDetail_Id from Accounting_InsuredDetails where ISNULL(InsuredReference_Id, '') = ISNULL(@InsuredReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)      
end      
end
go

Alter proc [dbo].[USP_Acconting_InsertUpdateFinanceDetails]
(    
@FinanceDetail_Id bigint output    
,@FinanceReference_Id varchar(250) = null            
,@Name varchar(200) = null            
,@Address1 varchar(500) = null            
,@Address2 varchar(500) = null            
,@City varchar(100) = null            
,@Country varchar(100) = null            
,@County varchar(100) = null            
,@State varchar(10) = null            
,@ZipCode varchar(10) = null          
,@Client_Id bigint        
)            
As            
Begin            
if not exists (select 1 from Accounting_FinanceDetails where ISNULL(FinanceReference_Id, '') = ISNULL(@FinanceReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)            
begin            
update Accounting_FinanceDetails set Status = 0 where FinanceReference_Id = @FinanceReference_Id and Status = 1 and Is_Deleted = 0   and Client_Id = @Client_Id         
insert into Accounting_FinanceDetails            
(            
FinanceReference_Id            
,Name            
,Address1            
,Address2            
,City            
,Country            
,County            
,State            
,ZipCode          
,Client_Id      
)            
Values            
(            
@FinanceReference_Id            
,@Name            
,@Address1            
,@Address2            
,@City            
,@Country            
,@County            
,@State            
,@ZipCode      
,@Client_Id      
)      
set @FinanceDetail_Id = SCOPE_IDENTITY()    
end            
else    
begin    
set @FinanceDetail_Id = (select FinanceDetail_Id from Accounting_FinanceDetails where ISNULL(FinanceReference_Id, '') = ISNULL(@FinanceReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(Address1, '') = ISNULL(@Address1, '') and ISNULL(Address2, '') = ISNULL(@Address2, '') and ISNULL(City, '') = ISNULL(@City, '') and ISNULL(Country, '') = ISNULL(@Country, '') and ISNULL(County, '') = ISNULL(@County, '') and ISNULL(State, '') = ISNULL(@State, '') and ISNULL(ZipCode, '') = ISNULL(@ZipCode, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)    
end    
end
go

Alter proc [dbo].[USP_Acconting_InsertUpdateWholesalerDetails]
(    
@WholesalerDetail_Id bigint output  
,@WholesalerReference_Id varchar(250) = null        
,@Client_Id bigint      
,@Name varchar(200) = null            
,@DisplayName varchar(500) = null            
,@ParentID int = null            
,@IsActive bit = null            
,@AlternateID varchar(100) = null            
)            
As            
Begin            
if not exists (select 1 from Accounting_WholesalerDetails where ISNULL(WholesalerReference_Id, '') = ISNULL(@WholesalerReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(DisplayName, '') = ISNULL(@DisplayName, '') and ISNULL(ParentID, 0) = ISNULL(@ParentID, 0) and ISNULL(IsActive, 0) = ISNULL(@IsActive, 0) and ISNULL(AlternateID, '') = ISNULL(@AlternateID, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)            
begin            
update Accounting_WholesalerDetails set Status = 0, UpdatedOn = Getdate() where WholesalerReference_Id = @WholesalerReference_Id and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id           
insert into Accounting_WholesalerDetails            
(            
WholesalerReference_Id            
,Name            
,DisplayName            
,ParentID            
,IsActive            
,AlternateID    
,Client_Id    
)            
Values            
(            
@WholesalerReference_Id            
,@Name            
,@DisplayName            
,@ParentID            
,@IsActive            
,@AlternateID        
,@Client_Id    
)    
set @WholesalerDetail_Id = SCOPE_IDENTITY()  
end            
else  
begin  
set @WholesalerDetail_Id = (select WholesalerDetail_Id from Accounting_WholesalerDetails where ISNULL(WholesalerReference_Id, '') = ISNULL(@WholesalerReference_Id, '') and ISNULL(Name, '') = ISNULL(@Name, '') and ISNULL(DisplayName, '') = ISNULL(@DisplayName, '') and ISNULL(ParentID, 0) = ISNULL(@ParentID, 0) and ISNULL(ParentID, 0) = ISNULL(@ParentID, 0) and ISNULL(IsActive, 0) = ISNULL(@IsActive, 0) and ISNULL(AlternateID, '') = ISNULL(@AlternateID, '') and Status = 1 and Is_Deleted = 0 and Client_Id = @Client_Id)  
end  
end
go