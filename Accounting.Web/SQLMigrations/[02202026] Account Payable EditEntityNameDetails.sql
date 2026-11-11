--CREATE PROCEDURE USP_Accounting_UpdatePayeerAndEntityByInvoiceDetailId
--   (
--    @InvoiceDetail_Id BIGINT = NULL,
--    @Entity_Id BIGINT = NULL,
--    @EntityType_Id TINYINT = NULL
--   )
--AS
--BEGIN
--    SET NOCOUNT ON;
--    IF EXISTS (SELECT 1 FROM Accounting_InvoiceDetails WHERE Id = @InvoiceDetail_Id AND Status = 1)
--    BEGIN
--        UPDATE Accounting_InvoiceDetails SET Entity_Id = @Entity_Id, EntityType_Id = @EntityType_Id
--        WHERE Id = @InvoiceDetail_Id;
--    END
--END


--INSERT INTO Accounting_LU_EntityTypes (Name, Code) VALUES ('Others', 'Others');


--CREATE TABLE Accounting_OtherEntityDetails
--(
--    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT UK_Accounting_OtherEntityDetails_ID UNIQUE,
--	Name varchar(200) null,
--	Description varchar(200) null,
--    Entity_Id BIGINT,
--    EntityType_Id TINYINT CONSTRAINT FK_Accounting_OtherEntityDetails_EntityType_Id REFERENCES Accounting_LU_EntityTypes (EntityType_Id),
--    Status BIT NOT NULL CONSTRAINT DF_Accounting_OtherEntityDetails_Status DEFAULT (1),
--    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_OtherEntityDetails_CreatedOn DEFAULT (GETDATE()),
--    UpdatedOn DATETIME
--);

--CREATE CLUSTERED INDEX CLU_Accounting_OtherEntityDetails_Status_Id ON Accounting_OtherEntityDetails (Status DESC, ID ASC); 
 

 --select * from Accounting_OtherEntityDetails
 
--CREATE PROCEDURE USP_Accounting_InsertUpdateOtherEntityDetails 
--(  
--    @Id BIGINT OUTPUT,
--    @Name VARCHAR(200) = NULL,
--    @Description VARCHAR(250) = NULL,
--    @Entity_Id BIGINT = NULL,
--    @EntityType_Id TINYINT	=NULL
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
--            UpdatedOn = GETDATE()  
--        WHERE Id = @Id  
--          AND Status = 1;  
--    END  
--    ELSE  
--    BEGIN  
--        INSERT INTO Accounting_OtherEntityDetails
--            (Name, Description, Entity_Id, EntityType_Id)
--        VALUES 
--            (@Name, @Description, @Entity_Id, @EntityType_Id);  

--        SET @Id = SCOPE_IDENTITY();  
--    END  
--END
--GO