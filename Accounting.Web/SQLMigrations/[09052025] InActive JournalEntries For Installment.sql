
--CREATE Proc [dbo].[USP_Accounting_DeleteJournalEntries]    
--(    
--@Invoice_Id tinyint,
--@SourceType_Id tinyint
--)    
--As    
--Begin    
--update Accounting_JournalEntries set Status = 0 where Source_Id = @Invoice_Id and SourceType_Id=@SourceType_Id and Status = 1;    
--end
--go

--ALTER Proc [dbo].[USP_Accounting_DeleteInvoices]    
--(    
--@PolicyDetails_Id bigint
--)    
--As    
--Begin    
--update Accounting_Invoices set Status = 0 where PolicyDetails_Id = @PolicyDetails_Id and Status = 1;    
--end
--go


--CREATE PROC [dbo].[USP_Accounting_GetAllInvoicesByPolicyDetail_Id]      
--(      
--@PolicyDetails_Id bigint 
--)      
--as      
--begin      
--select * from Accounting_Invoices where PolicyDetails_Id = @PolicyDetails_Id and Status = 1 ;     
--end

--go


--CREATE PROCEDURE [dbo].[USP_Accounting_InsertInstallmentInvoices]
--(
--    @Id bigint OUTPUT,
--    @PolicyDetails_Id bigint,
--    @InvoiceNo varchar(100) OUTPUT,
--    @InstallmentNo int = NULL,
--    @DueDate datetime = NULL,
--    @InvoiceNote varchar(500) = NULL,
--    @Paid bit = NULL,
--    @Amount float = NULL,
--    @RemainingBalance float = NULL,
--    @InvoiceEffectiveDate datetime = NULL,
--    @Schedule_Id bigint = NULL,
--    @InvoiceCreatedOn datetime OUTPUT
--)
--AS
--BEGIN
--    SET NOCOUNT ON;

--    DECLARE @LocalDueDate DATETIME = ISNULL(@DueDate, DATEADD(DAY, 30, ISNULL(@InvoiceEffectiveDate, GETDATE())));

--    DECLARE @NextInvoiceNo VARCHAR(50) = 'INV' +
--        RIGHT('000000' + CAST(
--            ISNULL(
--                (SELECT TOP 1 
--                    CAST(SUBSTRING(InvoiceNo, PATINDEX('%[0-9]%', InvoiceNo), LEN(InvoiceNo)) AS INT)
--                 FROM Accounting_Invoices
--                 WHERE Status = 1
--                 ORDER BY Id DESC),
--            0) + 1 AS VARCHAR), 6);

--    SELECT @NextInvoiceNo AS NewInvoiceNo;

--    INSERT INTO Accounting_Invoices
--    (
--        PolicyDetails_Id,
--        InvoiceNo,
--        InstallmentNo,
--        DueDate,
--        InvoiceNote,
--        Amount,
--        RemainingBalance,
--        Schedule_Id
--    )
--    VALUES
--    (
--        @PolicyDetails_Id,
--        @NextInvoiceNo,
--        @InstallmentNo,
--        @LocalDueDate,
--        @InvoiceNote,
--        @Amount,
--        @RemainingBalance,
--        @Schedule_Id
--    );

--    -- Set output values
--    SET @Id = SCOPE_IDENTITY();
--    SET @InvoiceNo = @NextInvoiceNo;

--    SELECT @InvoiceCreatedOn = CreatedOn
--    FROM Accounting_Invoices
--    WHERE Id = @Id AND Status = 1;
--END
