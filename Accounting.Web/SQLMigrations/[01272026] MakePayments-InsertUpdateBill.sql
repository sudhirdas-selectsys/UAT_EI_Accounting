CREATE PROCEDURE USP_Accounting_InsertUpdateBill  
(  
    @Bill_Id BIGINT OUTPUT,  
    @EntityType_Id TINYINT,  
    @Entity_Id BIGINT,  
    @Due FLOAT = NULL,  
    @Amount_Applied FLOAT  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    -- UPDATE  
    IF EXISTS (SELECT 1 FROM Accounting_Bill WHERE Bill_Id = @Bill_Id AND Status = 1)  
    BEGIN  
        UPDATE Accounting_Bill SET  
            EntityType_Id = @EntityType_Id,  
            Entity_Id = @Entity_Id,  
            Due = @Due,  
            Amount_Applied = @Amount_Applied,  
            UpdatedOn = GETDATE()  
            WHERE Bill_Id = @Bill_Id  
          AND Status = 1;  
    END  
    ELSE  
    BEGIN  
   
        INSERT INTO Accounting_Bill(EntityType_Id, Entity_Id, Due,Amount_Applied)  
        VALUES (@EntityType_Id, @Entity_Id, @Due, @Amount_Applied);  
        SET @Bill_Id = SCOPE_IDENTITY();  
    END  
END  