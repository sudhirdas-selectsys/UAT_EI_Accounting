Create Table Accounting_LU_PaymentDirectionsSubTypes
(
PaymentDirectionsSubType_Id tinyint not null constraint UK_Accounting_LU_PaymentDirectionsSubTypes_PaymentDirectionsSubType_Id unique identity(1,1)
,Name varchar(500)
,Code varchar(500)
,SequenceNo tinyint
,Status bit constraint DF_Accounting_LU_PaymentDirectionsSubTypes_Status default(1) not null
,CreatedOn datetime constraint DF_Accounting_LU_PaymentDirectionsSubTypes_CreatedOn  default(getdate()) not null 
,UpdatedOn datetime
)
go
Create Clustered Index CLU_Accounting_LU_PaymentDirectionsSubTypes_Status_PaymentDirectionsSubType_Id on Accounting_LU_PaymentDirectionsSubTypes
(
Status desc,
PaymentDirectionsSubType_Id
)
go


insert into Accounting_LU_PaymentDirectionsSubTypes(Name,Code)
values ('Standard','Standard'),
('Write off','Writeoff');
go

CREATE Proc USP_Accounting_GetAllPaymentDirectionsSubTypes  
as    
Begin    
select * from Accounting_LU_PaymentDirectionsSubTypes where status = 1    
end
go

Alter Table Accounting_LU_PaymentDirections_ChartOfAccounts
Add PaymentDirectionsSubType_Id tinyint Constraint FK_Accounting_LU_PaymentDirections_ChartOfAccounts_PaymentDirectionsSubType_Id references Accounting_LU_PaymentDirectionsSubTypes(PaymentDirectionsSubType_Id)
go


Alter proc USP_Accounting_GetPaymentDirections_ChartOfAccounts
(          
@Service_Id tinyint          
,@Client_Id bigint          
)          
as             
begin            
select  PD.Name AS Direction,PD.PaymentDirection_Id,
MAX(CASE WHEN PDST.Code = 'Standard' THEN PDC.ChartOfAccounts_Id END) AS StandardCOA_Id,
MAX(CASE WHEN PDST.Code = 'Writeoff' THEN PDC.ChartOfAccounts_Id END) AS WriteoffCOA_Id,
MAX(PDC.Client_Id) AS Client_Id,
MAX(PDC.Service_Id) AS Service_Id from Accounting_LU_PaymentDirections PD        
left join Accounting_LU_PaymentDirections_ChartOfAccounts PDC ON PDC.PaymentDirection_Id = PD.PaymentDirection_Id and PDC.Client_Id=@Client_Id and PDC.Service_Id=@Service_Id  and PDC.Status = 1   
left join Accounting_LU_PaymentDirectionsSubTypes PDST ON PDST.PaymentDirectionsSubType_Id = PDC.PaymentDirectionsSubType_Id and PDST.Status = 1
where PD.Status = 1 group by PD.Name,PD.PaymentDirection_Id  
end
go

Update Accounting_LU_PaymentDirections_ChartOfAccounts set PaymentDirectionsSubType_Id=1 where status=1
--Update Accounting_LU_PaymentDirections_ChartOfAccounts set PaymentDirectionsSubType_Id=1 where Client_Id=10001 and Service_Id=1 and PaymentDirection_Id=1 and status=1
--Update Accounting_LU_PaymentDirections_ChartOfAccounts set PaymentDirectionsSubType_Id=1 where Client_Id=10001 and Service_Id=1 and PaymentDirection_Id=2 and status=1
go

Alter procedure USP_Accounting_Insert_LU_PaymentDirections_ChartOfAccounts  
(  
@Client_Id bigint  
,@Service_Id bit  
,@PaymentDirection_Id int  
,@ChartOfAccounts_Id int
,@PaymentDirectionsSubType_Id tinyint = null
)  
as  
begin  
if exists(select 1 from Accounting_LU_PaymentDirections_ChartOfAccounts where  Client_Id=@Client_Id and PaymentDirection_Id=@PaymentDirection_Id and Service_Id=@Service_Id and PaymentDirectionsSubType_Id=@PaymentDirectionsSubType_Id and status=1)    
begin 
update Accounting_LU_PaymentDirections_ChartOfAccounts set ChartOfAccounts_Id=@ChartOfAccounts_Id,UpdatedOn = GETDATE()
where Client_Id=@Client_Id and PaymentDirection_Id=@PaymentDirection_Id and Service_Id=@Service_Id and PaymentDirectionsSubType_Id=@PaymentDirectionsSubType_Id and status=1  
end  
else  
begin  
insert into Accounting_LU_PaymentDirections_ChartOfAccounts(Client_Id,Service_Id,PaymentDirection_Id,ChartOfAccounts_Id,PaymentDirectionsSubType_Id)  
values(@Client_Id,@Service_Id,@PaymentDirection_Id,@ChartOfAccounts_Id,@PaymentDirectionsSubType_Id)  
end  
end
go

Alter table Accounting_PaymentApplicationLineItems add SubType_Id tinyint NULL Constraint FK_Accounting_PaymentApplicationLineItems_SubType_Id
references Accounting_LU_PaymentDirectionsSubTypes(PaymentDirectionsSubType_Id);
go

Alter table Accounting_PaymentApplicationLineItems alter column InvoiceLineItem_Id bigint NULL;
go

Alter table Accounting_PaymentApplications alter column Invoice_Id bigint NULL;
go

Alter proc [dbo].[USP_Accounting_InsertUpdatePaymentApplications]  
(  
@PaymentApplication_Id  bigint output  
,@Payment_Id bigint  
,@Invoice_Id bigint = null  
,@AppliedDate date = null  
,@TotalAppliedAmount float = null  
,@Notes  varchar(500) = null  
)  
As  
Begin  
if exists (select 1 from Accounting_PaymentApplications where PaymentApplication_Id = @PaymentApplication_Id and Payment_Id = @Payment_Id and Invoice_Id  = @Invoice_Id and status = 1)  
begin  
update Accounting_PaymentApplications set   
AppliedDate = @AppliedDate  
,TotalAppliedAmount = @TotalAppliedAmount  
,Notes = @Notes  
,UpdatedOn = Getdate()  
where PaymentApplication_Id = @PaymentApplication_Id and Payment_Id = @Payment_Id and Invoice_Id  = @Invoice_Id and status = 1  
end  
else  
begin  
insert into Accounting_PaymentApplications   
(  
Payment_Id  
,Invoice_Id  
,AppliedDate  
,TotalAppliedAmount  
,Notes  
)  
Values   
(  
@Payment_Id  
,@Invoice_Id  
,@AppliedDate  
,@TotalAppliedAmount  
,@Notes  
)  
set @PaymentApplication_Id = SCOPE_IDENTITY()  
end  
end
go

Alter proc [dbo].[USP_Accounting_InsertUpdatePaymentApplicationLineItems]    
(    
@PaymentApplicationLineItem_Id bigint output     
,@PaymentApplication_Id bigint    
,@InvoiceLineItem_Id bigint = null    
,@AppliedAmount float = null   
,@Is_SplitPayment bit = null 
,@SubType_Id tinyint = null
)    
As    
Begin    
if exists (select 1 from Accounting_PaymentApplicationLineItems where PaymentApplicationLineItem_Id = @PaymentApplicationLineItem_Id and PaymentApplication_Id = @PaymentApplication_Id and status = 1)    
begin    
update Accounting_PaymentApplicationLineItems set    
InvoiceLineItem_Id = @InvoiceLineItem_Id    
,AppliedAmount = @AppliedAmount    
,Is_SplitPayment = @Is_SplitPayment 
,SubType_Id = @SubType_Id
,UpdatedOn = GETDATE()    
where PaymentApplicationLineItem_Id = @PaymentApplicationLineItem_Id and PaymentApplication_Id = @PaymentApplication_Id and status = 1    
end    
else    
begin    
insert into Accounting_PaymentApplicationLineItems    
(    
PaymentApplication_Id    
,InvoiceLineItem_Id    
,AppliedAmount    
,Is_SplitPayment
,SubType_Id
)    
Values    
(    
@PaymentApplication_Id    
,@InvoiceLineItem_Id    
,@AppliedAmount   
,@Is_SplitPayment
,@SubType_Id
)    
set @PaymentApplicationLineItem_Id = SCOPE_IDENTITY()    
end    
end 
go

CREATE Proc USP_Accounting_GetWriteoffPaymentApplicationLineItemsByPayment_Id
(      
@Payment_Id bigint       
)      
As      
Begin      
select PALT.*,PA.Notes
from Accounting_PaymentApplicationLineItems PALT      
join Accounting_PaymentApplications PA on PA.PaymentApplication_Id = PALT.PaymentApplication_Id and PA.Status = 1      
join Accounting_Payments P on P.Payment_Id = PA.Payment_Id and P.Status = 1  
join Accounting_LU_PaymentDirectionsSubTypes PST on PST.PaymentDirectionsSubType_Id = PALT.SubType_Id and PST.status=1
where P.Payment_Id = @Payment_Id and PST.Code='Writeoff' and PALT.Status = 1      
End
go