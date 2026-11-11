
--Alter Table Accounting_AgentDetails
--Add SalesRep varchar(250) null

USP_Acconting_InsertUpdateAgentDetails

--ALTER PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]        
--(        
--    @AgentDetail_Id BIGINT OUTPUT,        
--    @AgentReference_Id VARCHAR(250) = NULL,        
--    @Name VARCHAR(200) = NULL,        
--    @Address1 VARCHAR(500) = NULL,        
--    @Address2 VARCHAR(500) = NULL,        
--    @City VARCHAR(100) = NULL,        
--    @Country VARCHAR(100) = NULL,        
--    @County VARCHAR(100) = NULL,        
--    @State VARCHAR(10) = NULL,        
--    @ZipCode VARCHAR(10) = NULL,      
--    @FEIN VARCHAR(200) = NULL,      
--    @Client_Id BIGINT,        
--    @Email VARCHAR(250) = NULL,    
--    @AccountNumber VARCHAR(60) = NULL,    
--    @RoutingNumber VARCHAR(40) = NULL,  
--    @EI_Accounting_Id VARCHAR(250) = NULL  ,
--	@SalesRep VARCHAR(250) = NULL 
--)        
--AS        
--BEGIN        
--    SET NOCOUNT ON;        
        
--    DECLARE @ExistingAgentDetail_Id BIGINT;        
        
--    SELECT @ExistingAgentDetail_Id = AgentDetail_Id        
--    FROM Accounting_AgentDetails        
--    WHERE AgentReference_Id = @AgentReference_Id        
--      AND Client_Id = @Client_Id        
--      AND Is_Deleted = 0        
--      AND Status = 1;        
        
--    IF @ExistingAgentDetail_Id IS NOT NULL        
--    BEGIN        
--        UPDATE Accounting_AgentDetails        
--        SET        
--            Name = @Name,        
--            Address1 = @Address1,        
--            Address2 = @Address2,        
--            City = @City,        
--            Country = @Country,        
--            County = @County,        
--            State = @State,        
--            ZipCode = @ZipCode,        
--            Email = @Email,      
--            FEIN = @FEIN,    
--   AccountNumber = @AccountNumber,    
--   RoutingNumber = @RoutingNumber,  
--   EI_Accounting_Id = @EI_Accounting_Id,  
--   SalesRep=@SalesRep,
--            UpdatedOn = GETDATE()        
--        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;        
        
--        SET @AgentDetail_Id = @ExistingAgentDetail_Id;        
--    END        
--    ELSE        
--    BEGIN        
--        INSERT INTO Accounting_AgentDetails        
--        (        
--            AgentReference_Id, Name, Address1, Address2, City,        
--            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber,EI_Accounting_Id,SalesRep   
--        )        
--        VALUES        
--        (        
--            @AgentReference_Id, @Name, @Address1, @Address2, @City,        
--            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber,@EI_Accounting_Id,@SalesRep     
--        );        
        
--        SET @AgentDetail_Id = SCOPE_IDENTITY();        
--    END        
--END 
--GO

--CREATE TABLE Accounting_SalesRepDetails
--(
--    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT UK_Accounting_SalesRepDetails_ID UNIQUE,
--	  Name varchar(50),
--    Status BIT NOT NULL CONSTRAINT DF_Accounting_SalesRepDetails_Status DEFAULT (1),
--    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_SalesRepDetails_CreatedOn DEFAULT (GETDATE()),
--    UpdatedOn DATETIME
--);


--Alter Table Accounting_AgentDetails
--Add AssignedSalesRep_Id BIGINT null CONSTRAINT FK_Accounting_AgentDetails_AssignedSalesRep_Id REFERENCES Accounting_SalesRepDetails (Id)


--Alter Table Accounting_SalesRepDetails
--Add Client_Id BIGINT null CONSTRAINT FK_Accounting_SalesRepDetails_AssignedSalesRep_Id REFERENCES Accounting_Clients(Client_Id)


--IF EXISTS (
--    SELECT 1 
--    FROM sys.foreign_keys 
--    WHERE name = 'FK_Accounting_AgentDetails_AssignedSalesRep_Id'
--)
--BEGIN
--    ALTER TABLE Accounting_AgentDetails
--    DROP CONSTRAINT FK_Accounting_AgentDetails_AssignedSalesRep_Id;
--END


USP_Acconting_InsertUpdateSalesRepDetails
USP_Acconting_InsertUpdateAgentDetails
USP_Accounting_GetAPSearchResults_V1
  select * from Accounting_SalesRepDetails

--CREATE PROC [dbo].[USP_Acconting_InsertUpdateSalesRepDetails]          
--(          
--    @SalesRepDetails_Id BIGINT OUTPUT,                   
--    @Name VARCHAR(200) = NULL,
--	@Client_Id BIGINT =NULL
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
--            UpdatedOn = GETDATE()          
--        WHERE Id = @ExistingSalesRepDetails_Id;          
          
--        SET @SalesRepDetails_Id = @ExistingSalesRepDetails_Id;          
--    END          
--    ELSE          
--    BEGIN          
--        INSERT INTO Accounting_SalesRepDetails          
--        (           Name, Client_Id     
--        )          
--        VALUES          
--        (          @Name, @Client_Id       
--        );          
          
--        SET @SalesRepDetails_Id = SCOPE_IDENTITY();          
--    END          
--END 
--GO

USP_Accounting_GetAPSearchResults_V1
@Client_Id=10007,
@InvoiceNo='INV002169'
  
--ALTER PROCEDURE USP_Accounting_GetAPSearchResults_V1          
--(                        
--    @EntityType_Id         TINYINT = NULL,                        
--    @Entity_Id             BIGINT = NULL,                        
--    @LineItem_Id           TINYINT = NULL,                        
--    @PolicyNumber          VARCHAR(100) = NULL,                        
--    @AgencyName            VARCHAR(200) = NULL,                        
--    @InsuredName           VARCHAR(100) = NULL,                        
--    @OutstandingBalances   BIT = NULL,                        
--    @InvoiceNo             VARCHAR(100) = NULL,                        
--    @EntityName      VARCHAR(100) = NULL, -- Added By Rohith                
--    @Client_Id             BIGINT = NULL                        
--)                        
--AS                        
--BEGIN                        
--    SET NOCOUNT ON;                        
                        
--    DECLARE @sql NVARCHAR(MAX) = N'';                        
--    DECLARE @where NVARCHAR(MAX) = N'';                        
--    DECLARE @params NVARCHAR(MAX);                        
--    DECLARE @isInitialLoad BIT = 0;                        
                        
--    ---------------------------------------------------------------------------------                        
--    -- Step 1: Detect initial load (no filters)                        
--    ---------------------------------------------------------------------------------                        
--    IF @EntityType_Id IS NULL                        
--       AND @Entity_Id IS NULL                        
--       AND @LineItem_Id IS NULL                        
--       AND @PolicyNumber IS NULL                        
--       AND @AgencyName IS NULL                        
--       AND @InsuredName IS NULL                        
--       AND @InvoiceNo IS NULL                
--    AND @EntityName IS NULL -- Added by Rohith                
--    BEGIN                        
--        SET @isInitialLoad = 1;                        
--    END                        
                        
--    ---------------------------------------------------------------------------------                        
--    -- Step 2: Begin query                        
--    ---------------------------------------------------------------------------------                        
--    IF @isInitialLoad = 1                        
--    BEGIN                        
--        SET @sql += N'                        
--        ;WITH LimitedInvoices AS (                        
--            SELECT TOP (50) inv.Id                        
--            FROM Accounting_Invoices inv                        
--            INNER JOIN Accounting_PolicyDetails pd                        
--                ON inv.PolicyDetails_Id = pd.Id                        
--                AND pd.Status = 1                        
--            WHERE inv.Status = 1                        
--              AND pd.Client_Id = @Client_Id                        
--            ORDER BY inv.Id DESC                        
--        ),                        
--        ';                        
--    END                        
--    ELSE                        
--    BEGIN                        
--        SET @sql += N';WITH ';                        
--    END                        
                        
--    ---------------------------------------------------------------------------------                        
--    -- Step 3: Add the supporting CTEs                        
--    ---------------------------------------------------------------------------------                        
--    SET @sql += N'                        
--    ARCollectedForPassThrough AS (                        
--        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id,LIC.LineItem_Id as  LineItem_Id                        
--        FROM Accounting_InvoiceDetails ID                        
--        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                        
--        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--        WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 1 AND ID.Status = 1                      GROUP BY ID.Id, INV.Id, LIC.LineItem_Id                        
--    ),                        
--    AgtCommissionPayableAmount AS (                        
--        SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalAgtCommission, INV.Id AS Invoice_Id                        
--        FROM Accounting_InvoiceDetails ID                        
--        LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                        
--        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Status = 1 AND LI.Code = ''AgencyBrokerCommission''                        
--    ),                        
--    TotalCommissionReceivableAmount AS (                        
--        SELECT ID.Id AS InvoiceLineItem_Id, ID.Amount AS OriginalTotalCommission, INV.Id AS Invoice_Id                        
--        FROM Accounting_InvoiceDetails ID                        
--        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                        
--        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1 AND LI.Code = ''TotalCommission''                        
--    ),                        
--    ARCollectedForTotalCommission AS (                        
--        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id                        
--        FROM Accounting_InvoiceDetails ID                        
--        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                        
--        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                    
--        WHERE LD.Direction_Id = 1 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''TotalCommission''                       
--        GROUP BY ID.Id, INV.Id                        
--    ),                        
--    ARCollectedForSalesRepCommission AS (                        
--        SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id                        
--        FROM Accounting_InvoiceDetails ID                        
--        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--        INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.ID AND PALI.Status = 1                        
--        INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--        INNER JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                    
--        WHERE LD.Direction_Id = 2 AND ID.Status = 1 AND LI.Status = 1  AND LI.Code = ''SalesRepCommission''                    
--        GROUP BY ID.Id, INV.Id                        
--    ),   
-- TotalARInvoiced AS (  
--     SELECT   
--    INV.Id AS Invoice_Id,  
--    SUM(ID.Amount) AS OriginalARAmount                 
--   FROM Accounting_InvoiceDetails ID    
--   INNER JOIN Accounting_Invoices INV   
--    ON ID.Invoice_Id = INV.Id   
--    AND INV.Status = 1                        
--   INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD   
--    ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id   
--    AND BLD.Status = 1                        
--   INNER JOIN Accounting_LU_LineItems_Directions LD   
--    ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id   
--    AND LD.Status = 1    
--   INNER JOIN Accounting_LU_LineItems_Clients LIC   
--    ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id   
--    AND LIC.Status = 1                        
--   INNER JOIN Accounting_LU_LineItems LI   
--    ON LI.LineItem_Id = LIC.LineItem_Id   
--    AND LI.Status = 1       
--   WHERE   
--    LD.Direction_Id = 1   
--    AND ID.Status = 1   
--   GROUP BY   
--    INV.Id  
  
-- ),  
-- TotalAPInvoiced AS (  
--     SELECT   
--    INV.Id AS Invoice_Id,  
--    SUM(ID.Amount) AS OriginalAPAmount  
--   FROM Accounting_InvoiceDetails ID    
--   INNER JOIN Accounting_Invoices INV   
--    ON ID.Invoice_Id = INV.Id   
--    AND INV.Status = 1                        
--   INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD   
--    ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id   
--    AND BLD.Status = 1                        
--   INNER JOIN Accounting_LU_LineItems_Directions LD   
--    ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id   
--    AND LD.Status = 1    
--   INNER JOIN Accounting_LU_LineItems_Clients LIC   
--    ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id   
--    AND LIC.Status = 1                        
--   INNER JOIN Accounting_LU_LineItems LI   
--    ON LI.LineItem_Id = LIC.LineItem_Id   
--    AND LI.Status = 1       
--   WHERE   
--    LD.Direction_Id = 2   
--    AND ID.Status = 1   
--   GROUP BY   
--    INV.Id  
-- )  
--    SELECT                        
--        ID.*,                        
--        CASE                         
--            WHEN ID.EntityType_Id = 1 THEN ag.Name                        
--            WHEN ID.EntityType_Id = 2 THEN ins.Name                        
--            WHEN ID.EntityType_Id = 3 THEN ca.Name                        
--            WHEN ID.EntityType_Id = 6 THEN wsd.Name                  
--   WHEN ID.EntityType_Id = 7 THEN concat(''State of '', Pd.riskstate)      
--   WHEN ID.EntityType_Id = 11 THEN AOED.Name 
--   WHEN ID.EntityType_Id = 10 THEN srd.Name
--            ELSE NULL                        
--        END AS EntityName,                        
--        ET.Name AS EntityTypeName,                        
--        CASE                         
--            WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected                        
--            WHEN LI.Code = ''AgencyBrokerCommission'' THEN ((AGPA.OriginalAgtCommission / TCRA.OriginalTotalCommission) * ARTC.ArCollected)  
--			WHEN LI.Code = ''SalesRepCommission'' THEN ((ARSC.ArCollected / NULLIF(TARI.OriginalARAmount, 0)) * TAPI.OriginalAPAmount)    
--        END AS ARCollected,        
--  LIC.Is_Commissionable,      
--  LI.LineItem_Id      
--    FROM Accounting_InvoiceDetails ID                        
--    LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1                        
--    INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1                        
--    LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id = pd.AgentDetail_Id AND agd.Status = 1                        
--    LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id = pd.InsuredDetail_Id AND isd.Status = 1                        
--    LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1                        
--    LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1                        
--    LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1                        
--    LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1 
--    LEFT JOIN Accounting_SalesRepDetails srd ON srd.Id = ID.Entity_Id AND ID.EntityType_Id = 10 AND srd.Status = 1 
--    LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1                        
--    LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1                        
--	LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1                        
--    LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                        
--    LEFT JOIN Accounting_LU_EntityTypes ET ON ID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1                        
--    LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = INV.Id  and ARPT.LineItem_Id = LIC.LineItem_Id                      
--    LEFT JOIN AgtCommissionPayableAmount AGPA ON AGPA.Invoice_Id = INV.Id                        
--    LEFT JOIN TotalCommissionReceivableAmount TCRA ON TCRA.Invoice_Id = INV.Id                        
--    LEFT JOIN ARCollectedForTotalCommission ARTC ON ARTC.Invoice_Id = INV.Id 
--	LEFT JOIN ARCollectedForSalesRepCommission ARSC ON ARSC.Invoice_Id = INV.Id      
--	LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = ID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1      
--	LEFT JOIN TotalARInvoiced TARI ON TARI.Invoice_Id = INV.Id      
--	LEFT JOIN TotalAPInvoiced TAPI ON TAPI.Invoice_Id = INV.Id      
--   WHERE LD.Direction_Id =2              
--  AND PD.Client_Id = @Client_Id                        
--  AND ID.Status = 1              
--  AND LI.Status = 1            
--  AND LIC.Status = 1            
--    ';                        
                        
--    ---------------------------------------------------------------------------------                        
--    -- Step 4: Build dynamic WHERE filters                        
--    ---------------------------------------------------------------------------------                        
--    IF @EntityType_Id IS NOT NULL                        
--        SET @where += N' AND ID.EntityType_Id = @EntityType_Id';                        
--    IF @Entity_Id IS NOT NULL                        
--        SET @where += N' AND ID.Entity_Id = @Entity_Id';                        
--    IF @LineItem_Id IS NOT NULL                        
--        SET @where += N' AND LIC.LineItem_Id = @LineItem_Id';                        
--    IF @PolicyNumber IS NOT NULL                        
--        SET @where += N' AND PD.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';                        
--    IF @AgencyName IS NOT NULL                        
--        SET @where += N' AND agd.Name LIKE ''%'' + @AgencyName + ''%''';                        
--    IF @InsuredName IS NOT NULL                        
--        SET @where += N' AND isd.Name LIKE ''%'' + @InsuredName + ''%''';                        
--    IF @InvoiceNo IS NOT NULL                        
--        SET @where += N' AND INV.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';                
-- IF @EntityName IS NOT NULL                 
--  BEGIN                
--  IF @EntityType_Id = 1  SET @where += N' AND ag.Name LIKE ''%'' + @EntityName + ''%''';                 
--  ELSE IF @EntityType_Id = 2 SET @where += N' AND ins.Name LIKE ''%''+ @EntityName + ''%''';                 
--  ELSE IF @EntityType_Id = 3 SET @where += N' AND ca.Name LIKE ''%'' + @EntityName + ''%''';                 
--  ELSE IF @EntityType_Id = 6 SET @where += N' AND wsd.Name LIKE ''%'' + @EntityName + ''%''';                 
--  ELSE IF @EntityType_Id = 7 SET @where += N' AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%''';    
--  ELSE IF @EntityType_Id = 11 SET @where += N' AND AOED.Name LIKE ''%'' + @EntityName + ''%''';    
--  END               
              
-- --IF @EntityName IS NOT NULL                
-- -- SET @where += N'                
-- -- AND (                
-- --  (ID.EntityType_Id = 1 AND ag.Name LIKE ''%'' + @EntityName + ''%'')                
-- -- OR (ID.EntityType_Id = 2 AND ins.Name LIKE ''%'' + @EntityName + ''%'')                
-- -- OR (ID.EntityType_Id = 3 AND ca.Name LIKE ''%'' + @EntityName + ''%'')                
-- -- OR (ID.EntityType_Id = 6 AND wsd.Name LIKE ''%'' + @EntityName + ''%'')                
-- -- OR (ID.EntityType_Id = 7 AND CONCAT(''State of '', PD.RiskState) LIKE ''%'' + @EntityName + ''%'')                
-- --   )';   -- Added By Rohith                
               
--    IF @OutstandingBalances = 1                        
--        SET @where += N' AND ID.RemainingBalance <> 0';                        
                        
--    -- Add LimitedInvoices only for initial load                        
--    IF @isInitialLoad = 1                        
--        SET @where += N' AND INV.Id IN (SELECT Id FROM LimitedInvoices)';                        
                        
--    SET @sql += @where + N' ORDER BY INV.Id DESC;';                        
                        
--    ---------------------------------------------------------------------------------                        
--    -- Step 5: Execute dynamic SQL                        
--    ---------------------------------------------------------------------------------                        
--    SET @params = N'                        
--        @EntityType_Id TINYINT,                        
--        @Entity_Id BIGINT,                        
--        @LineItem_Id TINYINT,                        
--        @PolicyNumber VARCHAR(100),                        
--        @AgencyName VARCHAR(200),                        
--        @InsuredName VARCHAR(100),                        
--        @OutstandingBalances BIT,                        
--        @InvoiceNo VARCHAR(100),                
--  @EntityName VARCHAR(100),                 
--        @Client_Id BIGINT                        
--    ';                        
--    EXEC sp_executesql                        
--        @sql,                        
--        @params,                        
--        @EntityType_Id = @EntityType_Id,                        
--        @Entity_Id = @Entity_Id,                        
--        @LineItem_Id = @LineItem_Id,                        
--        @PolicyNumber = @PolicyNumber,                        
--        @AgencyName = @AgencyName,                        
--        @InsuredName = @InsuredName,                        
--        @OutstandingBalances = @OutstandingBalances,                        
--        @InvoiceNo = @InvoiceNo,                
--  @EntityName = @EntityName, -- Added by Rohith                
--        @Client_Id = @Client_Id;                        
--END     
--GO




		Select * from Accounting_AgentDetails

		USP_Acconting_InsertUpdateAgentDetails

		  
--ALTER PROC [dbo].[USP_Acconting_InsertUpdateAgentDetails]          
--(          
--    @AgentDetail_Id BIGINT OUTPUT,          
--    @AgentReference_Id VARCHAR(250) = NULL,          
--    @Name VARCHAR(200) = NULL,          
--    @Address1 VARCHAR(500) = NULL,          
--    @Address2 VARCHAR(500) = NULL,          
--    @City VARCHAR(100) = NULL,          
--    @Country VARCHAR(100) = NULL,          
--    @County VARCHAR(100) = NULL,          
--    @State VARCHAR(10) = NULL,          
--    @ZipCode VARCHAR(10) = NULL,        
--    @FEIN VARCHAR(200) = NULL,        
--    @Client_Id BIGINT,          
--    @Email VARCHAR(250) = NULL,      
--    @AccountNumber VARCHAR(60) = NULL,      
--    @RoutingNumber VARCHAR(40) = NULL,    
--    @EI_Accounting_Id VARCHAR(250) = NULL  ,  
-- @SalesRep VARCHAR(250) = NULL ,
-- @AssignedSalesRep_Id BIGINT=null
--)          
--AS          
--BEGIN          
--    SET NOCOUNT ON;          
          
--    DECLARE @ExistingAgentDetail_Id BIGINT;          
          
--    SELECT @ExistingAgentDetail_Id = AgentDetail_Id          
--    FROM Accounting_AgentDetails          
--    WHERE AgentReference_Id = @AgentReference_Id          
--      AND Client_Id = @Client_Id          
--      AND Is_Deleted = 0          
--      AND Status = 1;          
          
--    IF @ExistingAgentDetail_Id IS NOT NULL          
--    BEGIN          
--        UPDATE Accounting_AgentDetails          
--        SET          
--            Name = @Name,          
--            Address1 = @Address1,          
--            Address2 = @Address2,          
--            City = @City,          
--            Country = @Country,          
--            County = @County,          
--            State = @State,          
--            ZipCode = @ZipCode,          
--            Email = @Email,        
--            FEIN = @FEIN,      
--   AccountNumber = @AccountNumber,      
--   RoutingNumber = @RoutingNumber,    
--   EI_Accounting_Id = @EI_Accounting_Id,    
--   SalesRep=@SalesRep,  
--   AssignedSalesRep_Id=@AssignedSalesRep_Id,
--            UpdatedOn = GETDATE()          
--        WHERE AgentDetail_Id = @ExistingAgentDetail_Id;          
          
--        SET @AgentDetail_Id = @ExistingAgentDetail_Id;          
--    END          
--    ELSE          
--    BEGIN          
--        INSERT INTO Accounting_AgentDetails          
--        (          
--            AgentReference_Id, Name, Address1, Address2, City,          
--            Country, County, State, ZipCode, Client_Id, Email, FEIN,AccountNumber,RoutingNumber,EI_Accounting_Id,SalesRep,AssignedSalesRep_Id    
--        )          
--        VALUES          
--        (          
--            @AgentReference_Id, @Name, @Address1, @Address2, @City,          
--            @Country, @County, @State, @ZipCode, @Client_Id, @Email, @FEIN,@AccountNumber,@RoutingNumber,@EI_Accounting_Id,@SalesRep ,@AssignedSalesRep_Id     
--        );          
          
--        SET @AgentDetail_Id = SCOPE_IDENTITY();          
--    END          
--END 
--GO