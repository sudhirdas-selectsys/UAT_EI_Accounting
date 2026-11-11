Alter Proc [dbo].[USP_Accounting_InsertUpdateInvoices]
(                  
@Id bigint output                  
,@PolicyDetails_Id bigint                  
,@InvoiceNo varchar(100) output                  
,@InstallmentNo int = null                  
,@DueDate datetime = null                  
,@InvoiceNote varchar(500) = null                  
,@Paid bit = null            
,@Amount float = null            
,@RemainingBalance  float = null            
,@InvoiceEffectiveDate datetime = null
,@InvoiceCreatedOn datetime output
)                  
As                  
Begin                  
if exists (select 1 from Accounting_Invoices where PolicyDetails_Id = @PolicyDetails_Id and InstallmentNo = @InstallmentNo and status = 1)                  
begin                  
update Accounting_Invoices set                  
InvoiceNo = @InvoiceNo                  
,DueDate = @DueDate                  
,InvoiceNote = @InvoiceNote             
,Amount = @Amount            
,RemainingBalance = @RemainingBalance            
,UpdatedOn = Getdate()                  
where PolicyDetails_Id = @PolicyDetails_Id and InstallmentNo = @InstallmentNo and status = 1                  
end                  
else                  
begin    
DECLARE @LocalDueDate DATETIME;    
SET @LocalDueDate = ISNULL(@DueDate, DATEADD(DAY, 30, ISNULL(@InvoiceEffectiveDate, GETDATE())));    
DECLARE @NextInvoiceNo VARCHAR(50) = 'INV' +           
    RIGHT('000000' + CAST(          
        ISNULL(          
            (SELECT TOP 1 CAST(SUBSTRING(InvoiceNo, PATINDEX('%[0-9]%', InvoiceNo), LEN(InvoiceNo)) AS INT)          
             FROM Accounting_Invoices           
             WHERE Status = 1           
             ORDER BY Id DESC), 0          
        ) + 1 AS VARCHAR), 6);          
          
SELECT @NextInvoiceNo AS NewInvoiceNo;          
insert into Accounting_Invoices                  
(                  
PolicyDetails_Id                  
,InvoiceNo                  
,InstallmentNo                  
,DueDate                  
,InvoiceNote             
,Amount            
,RemainingBalance            
)                  
Values                  
(                  
@PolicyDetails_Id                  
,@NextInvoiceNo                  
,@InstallmentNo                  
,@LocalDueDate                  
,@InvoiceNote            
,@Amount            
,@RemainingBalance            
)                  
set @Id = SCOPE_IDENTITY()
set @InvoiceNo = @NextInvoiceNo
select @InvoiceCreatedOn = CreatedOn from Accounting_Invoices where Id = @Id and Status = 1
end
end
go

insert into Accounting_LU_LineItems(Name,LineItemType_Id,Description,Code)
values
('Total Premium',1,'Total premium billed for carrier in case of direct bill','TotalPremium'),
('Total Commission Percentage',2,'Total commission percentage received from the carrier.','TotalCommissionPercentage');
go

insert into Accounting_LU_LineItems_Directions(LineItem_Id,Direction_Id)
values
(13,3),
(14,3);
go

insert into Accounting_LU_BillingType_LineItems_Directions(BillingType_Id,LineItems_Directions_Id)
values
(3,17),
(3,18);
go