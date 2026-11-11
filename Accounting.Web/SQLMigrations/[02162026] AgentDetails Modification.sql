


select * from Accounting_AgentDetails


-- ALTER TABLE Accounting_AgentDetails ADD RoutingNumber VARCHAR(40) NULL
-- ALTER TABLE Accounting_AgentDetails ADD AccountNumber VARCHAR(60) NULL



USP_Acconting_InsertUpdateAgentDetails

alter PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]    
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
	@RoutingNumber VARCHAR(40) = NULL
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
            UpdatedOn = GETDATE()    
        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;    
    
        SET @AgentDetail_Id = @ExistingAgentDetail_Id;    
    END    
    ELSE    
    BEGIN    
        INSERT INTO Accounting_AgentDetails    
        (    
            AgentReference_Id, Name, Address1, Address2, City,    
            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber  
        )    
        VALUES    
        (    
            @AgentReference_Id, @Name, @Address1, @Address2, @City,    
            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber   
        );    
    
        SET @AgentDetail_Id = SCOPE_IDENTITY();    
    END    
END    