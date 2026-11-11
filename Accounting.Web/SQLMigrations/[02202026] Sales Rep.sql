--Alter Table Accounting_AgentDetails
--Add SalesRep varchar(250) null


ALTER PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]        
(        
    @AgentDetail_Id BIGINT OUTPUT,        
    @AgentReference_Id VARCHAR(250) = NULL,        
    @Name VARCHAR(200) = NULL,        
    @Address1 VARCHAR(500) = NULL,        
    @Address2 VARCHAR(500) = NULL,        
    @City VARCHAR(100) = NULL,        
    @Country VARCHAR(100) = NULL,        
    @County VARCHAR(100) = NULL,        
    @State VARCHAR(10) = NULL,        
    @ZipCode VARCHAR(10) = NULL,      
    @FEIN VARCHAR(200) = NULL,      
    @Client_Id BIGINT,        
    @Email VARCHAR(250) = NULL,    
    @AccountNumber VARCHAR(60) = NULL,    
    @RoutingNumber VARCHAR(40) = NULL,  
    @EI_Accounting_Id VARCHAR(250) = NULL  ,
	@SalesRep VARCHAR(250) = NULL 
)        
AS        
BEGIN        
    SET NOCOUNT ON;        
        
    DECLARE @ExistingAgentDetail_Id BIGINT;        
        
    SELECT @ExistingAgentDetail_Id = AgentDetail_Id        
    FROM Accounting_AgentDetails        
    WHERE AgentReference_Id = @AgentReference_Id        
      AND Client_Id = @Client_Id        
      AND Is_Deleted = 0        
      AND Status = 1;        
        
    IF @ExistingAgentDetail_Id IS NOT NULL        
    BEGIN        
        UPDATE Accounting_AgentDetails        
        SET        
            Name = @Name,        
            Address1 = @Address1,        
            Address2 = @Address2,        
            City = @City,        
            Country = @Country,        
            County = @County,        
            State = @State,        
            ZipCode = @ZipCode,        
            Email = @Email,      
            FEIN = @FEIN,    
   AccountNumber = @AccountNumber,    
   RoutingNumber = @RoutingNumber,  
   EI_Accounting_Id = @EI_Accounting_Id,  
   SalesRep=@SalesRep,
            UpdatedOn = GETDATE()        
        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;        
        
        SET @AgentDetail_Id = @ExistingAgentDetail_Id;        
    END        
    ELSE        
    BEGIN        
        INSERT INTO Accounting_AgentDetails        
        (        
            AgentReference_Id, Name, Address1, Address2, City,        
            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber,EI_Accounting_Id,SalesRep   
        )        
        VALUES        
        (        
            @AgentReference_Id, @Name, @Address1, @Address2, @City,        
            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber,@EI_Accounting_Id,@SalesRep     
        );        
        
        SET @AgentDetail_Id = SCOPE_IDENTITY();        
    END        
END 
GO


--CREATE TABLE Accounting_SalesRepDetails
--(
--    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT UK_Accounting_SalesRepDetails_ID UNIQUE,
--	Name varchar(50),
--    Status BIT NOT NULL CONSTRAINT DF_Accounting_SalesRepDetails_Status DEFAULT (1),
--    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_SalesRepDetails_CreatedOn DEFAULT (GETDATE()),
--    UpdatedOn DATETIME
--);



--Alter Table Accounting_AgentDetails
--Add AssignedSalesRep_Id BIGINT null CONSTRAINT FK_Accounting_AgentDetails_AssignedSalesRep_Id REFERENCES Accounting_SalesRepDetails (Id)


--Alter Table Accounting_SalesRepDetails
--Add Client_Id BIGINT null CONSTRAINT FK_Accounting_SalesRepDetails_AssignedSalesRep_Id REFERENCES Accounting_Clients(Client_Id)


CREATE PROC [dbo].[USP_Acconting_InsertUpdateSalesRepDetails]          
(          
    @SalesRepDetails_Id BIGINT OUTPUT,                   
    @Name VARCHAR(200) = NULL,
	@Client_Id BIGINT =NULL
)          
AS          
BEGIN          
    SET NOCOUNT ON;          
          
    DECLARE @ExistingSalesRepDetails_Id BIGINT;          
          
    SELECT @ExistingSalesRepDetails_Id = Id          
    FROM Accounting_SalesRepDetails          
    WHERE Id = @SalesRepDetails_Id          
      AND Client_Id = @Client_Id  
      AND Status = 1;          
          
    IF @ExistingSalesRepDetails_Id IS NOT NULL          
    BEGIN          
        UPDATE Accounting_SalesRepDetails          
        SET          
            Name = @Name,     
            UpdatedOn = GETDATE()          
        WHERE Id = @ExistingSalesRepDetails_Id;          
          
        SET @SalesRepDetails_Id = @ExistingSalesRepDetails_Id;          
    END          
    ELSE          
    BEGIN          
        INSERT INTO Accounting_SalesRepDetails          
        (           Name, Client_Id     
        )          
        VALUES          
        (          @Name, @Client_Id       
        );          
          
        SET @SalesRepDetails_Id = SCOPE_IDENTITY();          
    END          
END 
GO


	  
ALTER PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]          
(          
    @AgentDetail_Id BIGINT OUTPUT,          
    @AgentReference_Id VARCHAR(250) = NULL,          
    @Name VARCHAR(200) = NULL,          
    @Address1 VARCHAR(500) = NULL,          
    @Address2 VARCHAR(500) = NULL,          
    @City VARCHAR(100) = NULL,          
    @Country VARCHAR(100) = NULL,          
    @County VARCHAR(100) = NULL,          
    @State VARCHAR(10) = NULL,          
    @ZipCode VARCHAR(10) = NULL,        
    @FEIN VARCHAR(200) = NULL,        
    @Client_Id BIGINT,          
    @Email VARCHAR(250) = NULL,      
    @AccountNumber VARCHAR(60) = NULL,      
    @RoutingNumber VARCHAR(40) = NULL,    
    @EI_Accounting_Id VARCHAR(250) = NULL  ,  
 @SalesRep VARCHAR(250) = NULL ,
 @AssignedSalesRep_Id BIGINT=null
)          
AS          
BEGIN          
    SET NOCOUNT ON;          
          
    DECLARE @ExistingAgentDetail_Id BIGINT;          
          
    SELECT @ExistingAgentDetail_Id = AgentDetail_Id          
    FROM Accounting_AgentDetails          
    WHERE AgentReference_Id = @AgentReference_Id          
      AND Client_Id = @Client_Id          
      AND Is_Deleted = 0          
      AND Status = 1;          
          
    IF @ExistingAgentDetail_Id IS NOT NULL          
    BEGIN          
        UPDATE Accounting_AgentDetails          
        SET          
            Name = @Name,          
            Address1 = @Address1,          
            Address2 = @Address2,          
            City = @City,          
            Country = @Country,          
            County = @County,          
            State = @State,          
            ZipCode = @ZipCode,          
            Email = @Email,        
            FEIN = @FEIN,      
   AccountNumber = @AccountNumber,      
   RoutingNumber = @RoutingNumber,    
   EI_Accounting_Id = @EI_Accounting_Id,    
   SalesRep=@SalesRep,  
   AssignedSalesRep_Id=@AssignedSalesRep_Id,
            UpdatedOn = GETDATE()          
        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;          
          
        SET @AgentDetail_Id = @ExistingAgentDetail_Id;          
    END          
    ELSE          
    BEGIN          
        INSERT INTO Accounting_AgentDetails          
        (          
            AgentReference_Id, Name, Address1, Address2, City,          
            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber,EI_Accounting_Id,SalesRep,AssignedSalesRep_Id    
        )          
        VALUES          
        (          
            @AgentReference_Id, @Name, @Address1, @Address2, @City,          
            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber,@EI_Accounting_Id,@SalesRep ,@AssignedSalesRep_Id     
        );          
          
        SET @AgentDetail_Id = SCOPE_IDENTITY();          
    END          
END 

