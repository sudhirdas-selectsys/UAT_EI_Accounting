CREATE PROCEDURE USP_Accounting_InsertUpdateBillLineItems    
(    
    @BillLineItems_Id BIGINT OUTPUT,    
    @Bill_Id BIGINT,    
    @Invoice_Id BIGINT,    
    @LineItem_Id TINYINT,  
 @InvoiceLineItem_Id  BIGINT,  
    @AppliedAmount FLOAT  
)    
AS    
BEGIN    
    SET NOCOUNT ON;    
    
    -- UPDATE    
    IF EXISTS    
    (    
        SELECT 1    
        FROM Accounting_BillLineItems    
        WHERE BillLineItems_Id = @BillLineItems_Id    
          AND Status = 1    
    )    
    BEGIN    
        UPDATE Accounting_BillLineItems    
        SET    
            Bill_Id = @Bill_Id,    
            Invoice_Id = @Invoice_Id,    
            LineItem_Id = @LineItem_Id,  
   InvoiceLineItem_Id=@InvoiceLineItem_Id,  
            AppliedAmount = @AppliedAmount ,             
            UpdatedOn = GETDATE()    
        WHERE BillLineItems_Id = @BillLineItems_Id    
          AND Status = 1;    
    END    
    ELSE    
    BEGIN    
        -- INSERT    
        INSERT INTO Accounting_BillLineItems    
        (    
            Bill_Id,    
            Invoice_Id,    
            LineItem_Id,   
   InvoiceLineItem_Id,  
            AppliedAmount  
        )    
        VALUES    
        (    
            @Bill_Id,    
            @Invoice_Id,    
            @LineItem_Id,  
   @InvoiceLineItem_Id,  
            @AppliedAmount    
        );    
    
        SET @BillLineItems_Id = SCOPE_IDENTITY();    
    END    
END 