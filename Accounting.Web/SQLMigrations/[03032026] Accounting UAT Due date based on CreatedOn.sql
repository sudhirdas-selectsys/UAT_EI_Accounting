 select * from Accounting_Payments where Payment_id in (21322, 21321 ) 
 select Client_Id, CompanyName  from Accounting_Clients
 USP_Accounting_GetARSearchResults_V2
CREATE or alter Proc [dbo].[USP_Accounting_InsertUpdateInvoices]                  
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
,@Schedule_Id bigint =null      
,@ClientSpecificDueDateRule varchar(max) = null      
,@Is_ClientSpecificDueDate bit = null  
,@ClientSpecificInvoiceEffectiveDate datetime = null,
@Client_Id bigint = null
,@InvoiceCreatedOn datetime output                  
)                                    
As                                    
Begin 
DECLARE @DueDateBaseMode VARCHAR(50);

SELECT @DueDateBaseMode = SettingValue
FROM Accounting_ClientSettings
WHERE Client_Id = @Client_Id
AND SettingKey = 'Billing.DueDate.BaseMode'
AND Status = 1;

DECLARE @LocalDueDate DATETIME;              
SET @LocalDueDate = dateadd(day,30,isnull(@InvoiceEffectiveDate,getdate()));              
DECLARE @NextInvoiceNo VARCHAR(50) = 'INV' +                     
    RIGHT('000000' + CAST(                    
        ISNULL(                    
            (SELECT TOP 1 CAST(SUBSTRING(InvoiceNo, PATINDEX('%[0-9]%', InvoiceNo), LEN(InvoiceNo)) AS INT)                    
             FROM Accounting_Invoices                     
             WHERE Status = 1                     
             ORDER BY Id DESC), 0                    
        ) + 1 AS VARCHAR), 6);                    
                    
SELECT @NextInvoiceNo AS NewInvoiceNo;      
if(@Is_ClientSpecificDueDate = 1)      
begin      
DECLARE @RuleMode VARCHAR(50);      
DECLARE @RuleValue INT;      
IF (@ClientSpecificDueDateRule IS NOT NULL AND CHARINDEX(':', @ClientSpecificDueDateRule) > 0)      
begin      
SET @RuleMode  = LEFT(@ClientSpecificDueDateRule, CHARINDEX(':', @ClientSpecificDueDateRule) - 1);      
SET @RuleValue = TRY_CAST(SUBSTRING(@ClientSpecificDueDateRule, CHARINDEX(':', @ClientSpecificDueDateRule) + 1, 10) AS INT);      
--DECLARE @BaseDate DATETIME = ISNULL(@ClientSpecificInvoiceEffectiveDate, GETDATE());
DECLARE @BaseDate DATETIME;
IF (@DueDateBaseMode = 'CREATED')
BEGIN
    SET @BaseDate = GETDATE();
END
ELSE
BEGIN
    SET @BaseDate = ISNULL(@ClientSpecificInvoiceEffectiveDate, GETDATE());
END
IF (@RuleMode IS NOT NULL)      
BEGIN      
    IF (@RuleMode = 'ADD_DAYS')      
        SET @LocalDueDate = DATEADD(DAY, @RuleValue, @BaseDate);      
      
    ELSE IF (@RuleMode = 'INVOICE_DATE')      
        SET @LocalDueDate = GETDATE();      
      
    ELSE      
        -- fallback if rule invalid      
        SET @LocalDueDate = DATEADD(DAY, 30, @BaseDate);      
END      
end      
end      
else      
begin      
--SET @LocalDueDate = dateadd(day,30,isnull(@InvoiceEffectiveDate,getdate()));  

IF (@DueDateBaseMode = 'CREATED')
    SET @LocalDueDate = DATEADD(DAY,30,GETDATE());
ELSE
    SET @LocalDueDate = DATEADD(DAY,30,ISNULL(@InvoiceEffectiveDate,GETDATE()));
end   
if exists (select 1 from Accounting_Invoices where PolicyDetails_Id = @PolicyDetails_Id and InstallmentNo = @InstallmentNo and status = 1 AND (Schedule_Id = 0 OR Schedule_Id IS NULL))                                    
begin         
declare @UpdateDueDate datetime                                
set @UpdateDueDate = dateadd(day,30,isnull(@InvoiceEffectiveDate,getdate()))           
update Accounting_Invoices set                                    
InvoiceNo = @InvoiceNo                                    
,DueDate = @LocalDueDate                                    
,InvoiceNote = @InvoiceNote                               
,Amount = @Amount                              
,RemainingBalance = @RemainingBalance                              
,UpdatedOn = Getdate()                                    
where PolicyDetails_Id = @PolicyDetails_Id and InstallmentNo = @InstallmentNo and status = 1;                               
end                                    
else                                    
begin                                              
insert into Accounting_Invoices                                    
(                                    
PolicyDetails_Id                                    
,InvoiceNo                                    
,InstallmentNo                                    
,DueDate                                    
,InvoiceNote                               
,Amount                              
,RemainingBalance               
,Schedule_Id              
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
,@Schedule_Id              
)                                    
set @Id = SCOPE_IDENTITY()                  
set @InvoiceNo = @NextInvoiceNo                  
end                  
select @InvoiceCreatedOn = CreatedOn from Accounting_Invoices where Id = @Id and Status = 1                  
end 