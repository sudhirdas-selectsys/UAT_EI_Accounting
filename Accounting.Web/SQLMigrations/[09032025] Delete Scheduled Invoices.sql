--ALTER PROCEDURE USP_Accounting_DeleteScheduledInvoicesByPolicyDetailId
--    @PolicyDetails_Id INT
--AS
--BEGIN
--    UPDATE Accounting_PolicyPaymentSchedule
--    SET Status = 0 
--    WHERE PolicyDetail_Id = @PolicyDetails_Id
--END



--Alter table Accounting_PolicyPaymentSchedule
--drop constraint UQ_EI_Accounting_PolicyPaymentSchedule_Policy_Installment


    
      
--ALTER PROCEDURE [dbo].[USP_Accounting_InsertUpdateScheduledInvoices]        
        
--    @PolicyInstallmentDetails_Id BIGINT = NULL,        
--    @PolicyDetail_Id       BIGINT,        
--    @PaymentPlan_Id        BIGINT,        
--    @InstallmentNo         INT=null,        
--    @IsDownPayment         BIT,        
--    @DueDate               DATETIME,        
--    @BillDate              DATETIME,  
--	@CreatedOn             DATETIME = null,  
--    @Amount                DECIMAL(18,2),        
--    @Paid                  BIT,        
--    @PaidOn                DATETIME = NULL,        
--    @Notes                 NVARCHAR(MAX) = NULL,        
--    @Sequence              INT ,    
--    @PolicyPaymentSchedule_Id BIGINT OUTPUT
--	,@InvoiceCreatedOn datetime output        
--AS        
--BEGIN        
--    SET NOCOUNT ON;        
        
--    IF @PolicyInstallmentDetails_Id IS NULL        
--    BEGIN        
--        -- Insert        
--        INSERT INTO dbo.Accounting_PolicyPaymentSchedule        
--        (        
--            PolicyDetail_Id,        
--            PaymentPlan_Id,        
--            InstallmentNo,        
--            IsDownPayment,        
--            DueDate,        
--            BillDate,  
--			CreatedOn,  
--            Amount,        
--            Paid,        
--            PaidOn,        
--            Notes,        
--            SequenceNo        
--        )        
--        VALUES        
--        (        
--            @PolicyDetail_Id,        
--            @PaymentPlan_Id,        
--            @InstallmentNo,        
--            @IsDownPayment,        
--            @DueDate,        
--            @BillDate,  
--			@CreatedOn,  
--            @Amount,        
--            @Paid,        
--            @PaidOn,        
--            @Notes,        
--            @Sequence        
--        );        
--  SET @PolicyPaymentSchedule_Id = SCOPE_IDENTITY();   
--  Select @InvoiceCreatedOn = CreatedOn from Accounting_PolicyPaymentSchedule where PolicyPaymentSchedule_Id = @PolicyPaymentSchedule_Id and Status = 1        
--    END 
--END 