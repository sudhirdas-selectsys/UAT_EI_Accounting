
--alter table Accounting_SalesrepDetails Add EntityType_Id tinyint null

  
--alter PROC [dbo].[USP_Acconting_InsertUpdateSalesRepDetails]            
--(            
--    @SalesRepDetails_Id BIGINT OUTPUT,                     
--    @Name VARCHAR(200) = NULL,
--	@EntityType_Id TINYINT = NULL,
-- @Client_Id BIGINT =NULL  
--)            
--AS            
--BEGIN            
--    SET NOCOUNT ON;            
            
--    DECLARE @ExistingSalesRepDetails_Id BIGINT;            
            
--    SELECT @ExistingSalesRepDetails_Id = Id            
--    FROM Accounting_SalesRepDetails            
--    WHERE Id = @SalesRepDetails_Id            
--      AND Client_Id = @Client_Id    
--      AND Status = 1;            
            
--    IF @ExistingSalesRepDetails_Id IS NOT NULL            
--    BEGIN            
--        UPDATE Accounting_SalesRepDetails            
--        SET            
--            Name = @Name,
--			EntityType_Id =@EntityType_Id,
--            UpdatedOn = GETDATE()            
--        WHERE Id = @ExistingSalesRepDetails_Id;            
            
--        SET @SalesRepDetails_Id = @ExistingSalesRepDetails_Id;            
--    END            
--    ELSE            
--    BEGIN            
--        INSERT INTO Accounting_SalesRepDetails            
--        (           Name, Client_Id,EntityType_Id       
--        )            
--        VALUES            
--        (          @Name, @Client_Id,@EntityType_Id        
--        );            
            
--        SET @SalesRepDetails_Id = SCOPE_IDENTITY();            
--    END            
--END   



