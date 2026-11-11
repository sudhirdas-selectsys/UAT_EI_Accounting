
--alter PROCEDURE USP_Accounting_InsertUpdateOtherEntityDetails   
--(    
--    @Id BIGINT OUTPUT,  
--    @Name VARCHAR(200) = NULL,  
--    @Description VARCHAR(250) = NULL,  
--    @Entity_Id BIGINT = NULL,  
--    @EntityType_Id TINYINT =NULL,
--	@Client_Id BIGINT=NULL
--)    
--AS    
--BEGIN    
--    SET NOCOUNT ON;    
  
--    IF EXISTS (SELECT 1 FROM Accounting_OtherEntityDetails WHERE Id = @Id AND Status = 1)  
--    BEGIN    
--        UPDATE Accounting_OtherEntityDetails   
--        SET Name = @Name,  
--            Description = @Description,   
--            Entity_Id = @Entity_Id,   
--            EntityType_Id = @EntityType_Id,
--			Client_Id =@Client_Id, 
--            UpdatedOn = GETDATE()    
--        WHERE Id = @Id    
--          AND Status = 1;    
--    END    
--    ELSE    
--    BEGIN    
--        INSERT INTO Accounting_OtherEntityDetails  
--            (Name, Description, Entity_Id, EntityType_Id,Client_Id)  
--        VALUES   
--            (@Name, @Description, @Entity_Id, @EntityType_Id,@Client_Id);    
  
--        SET @Id = SCOPE_IDENTITY();    
--    END    
--END  


--ALTER TABLE Accounting_OtherEntityDetails ADD Client_Id BIGINT NULL;


  

--CREATE Proc USP_Accounting_GetAllOtherEntityDetailsByName
--(
--@Name varchar(200) = null,      
--@Client_Id bigint  null 
--)
--As                  
--begin                  
--select Id as Value, Name as Name from Accounting_OtherEntityDetails where Name like '%' + @Name + '%' and Client_Id = @Client_Id and  Status = 1                   
--end 


