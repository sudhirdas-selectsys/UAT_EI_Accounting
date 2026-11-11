
--added column for storing NAIC which is pulled from EI
alter table Accounting_CarrierDetails add NAIC varchar(6);


--fetched NAIC data from EI is storing in Accounting_CarrierDetails table
  ALTER proc [dbo].[USP_Acconting_InsertUpdateCarrierDetails]                      
(                  
@CarrierDetail_Id bigint output                  
,@CarrierReference_Id varchar(250) = null                          
,@Name varchar(200) = null                          
,@Address1 varchar(500) = null                          
,@Address2 varchar(500) = null                          
,@City varchar(100) = null                          
,@Country varchar(100) = null                          
,@County varchar(100) = null    
,@State varchar(10) = null                          
,@ZipCode varchar(10) = null    
,@CarrierStatus varchar(30)=null   
,@NAIC varchar(6) = null  
,@Client_Id bigint                      
)                          
As                          
Begin               
SET NOCOUNT ON;            
            
DECLARE @ExistingCarrierDetail_Id BIGINT;            
            
SELECT @ExistingCarrierDetail_Id = CarrierDetail_Id             
FROM Accounting_CarrierDetails             
WHERE CarrierReference_Id = @CarrierReference_Id             
AND Client_Id = @Client_Id             
AND Is_Deleted = 0             
AND Status = 1;            
            
IF @ExistingCarrierDetail_Id IS NOT NULL            
BEGIN            
IF EXISTS (            
SELECT 1             
FROM Accounting_CarrierDetails             
WHERE CarrierDetail_Id = @ExistingCarrierDetail_Id            
AND (            
ISNULL(Name, '') <> ISNULL(@Name, '')            
OR ISNULL(Address1, '') <> ISNULL(@Address1, '')            
OR ISNULL(Address2, '') <> ISNULL(@Address2, '')            
OR ISNULL(City, '') <> ISNULL(@City, '')            
OR ISNULL(Country, '') <> ISNULL(@Country, '')            
OR ISNULL(County, '') <> ISNULL(@County, '')    
OR ISNULL(State, '') <> ISNULL(@State, '')            
OR ISNULL(ZipCode, '') <> ISNULL(@ZipCode, '')    
OR ISNULL(CarrierStatus, '') <> ISNULL(@CarrierStatus, '')   
OR ISNULL(NAIC, '') <> ISNULL(@NAIC, '')  
)            
)            
BEGIN            
UPDATE Accounting_CarrierDetails             
SET             
Name = @Name,            
Address1 = @Address1,            
Address2 = @Address2,            
City = @City,            
Country = @Country,            
County = @County,    
State = @State,            
ZipCode = @ZipCode,    
CarrierStatus = @CarrierStatus,  
NAIC = @NAIC,  
UpdatedOn = Getdate()            
WHERE CarrierDetail_Id = @ExistingCarrierDetail_Id            
END            
            
SET @CarrierDetail_Id = @ExistingCarrierDetail_Id;            
END            
ELSE            
BEGIN            
insert into Accounting_CarrierDetails                          
(                          
CarrierReference_Id                          
,Name                          
,Address1                          
,Address2                          
,City                          
,Country                          
,County   
,State                          
,ZipCode    
,CarrierStatus   
,NAIC  
,Client_Id                    
)                          
Values                          
(                          
@CarrierReference_Id                          
,@Name                          
,@Address1                          
,@Address2                          
,@City                          
,@Country                          
,@County   
,@State                          
,@ZipCode    
,@CarrierStatus   
,@NAIC  
,@Client_Id                    
);            
                    
set @CarrierDetail_Id = SCOPE_IDENTITY();            
END            
END 


-- For CarrierContact NAIC column is binded, based on CoverageCode pulled CoverageCodeDescription and displaying in api response
ALTER PROC USP_Accounting_GetAllOpenInvoices      
(      
    @Client_Id     BIGINT,      
    @Entity_Id     BIGINT,      
    @EntityType_Id TINYINT      
)      
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    ----------------------------------------------------------------------      
    -- Minimum Premium      
    ----------------------------------------------------------------------      
    WITH MinimumPremiumData AS      
    (      
        SELECT I.PolicyDetails_Id, ID.Invoice_Id, SUM(ID.Amount) AS MinimumPremium      
        FROM Accounting_Invoices I      
        JOIN Accounting_InvoiceDetails ID      
            ON ID.Invoice_Id = I.Id AND ID.Status = 1      
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD      
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        JOIN Accounting_LU_LineItems_Directions LD      
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
        JOIN Accounting_LU_LineItems_Clients LIC      
            ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
        JOIN Accounting_LU_LineItems LI      
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
        WHERE LD.Direction_Id = 1 AND LI.LineItemType_Id IN (1)      
        GROUP BY I.PolicyDetails_Id, ID.Invoice_Id      
    ),      
      
    ----------------------------------------------------------------------      
    -- Total Fees      
    ----------------------------------------------------------------------      
    TotalFees AS      
    (      
        SELECT I.PolicyDetails_Id, ID.Invoice_Id, SUM(ID.Amount) AS TotalFees      
        FROM Accounting_Invoices I      
        JOIN Accounting_InvoiceDetails ID      
            ON ID.Invoice_Id = I.Id AND ID.Status = 1      
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD      
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        JOIN Accounting_LU_LineItems_Directions LD      
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
        JOIN Accounting_LU_LineItems_Clients LIC      
            ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
        JOIN Accounting_LU_LineItems LI      
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
        WHERE LD.Direction_Id = 1 AND LI.LineItemType_Id IN (4)      
        GROUP BY I.PolicyDetails_Id, ID.Invoice_Id      
    ),      
      
    ----------------------------------------------------------------------      
    -- Total Taxes       
    ----------------------------------------------------------------------      
    TotalTaxes AS      
    (      
        SELECT I.PolicyDetails_Id, ID.Invoice_Id, SUM(ID.Amount) AS TotalTaxes      
        FROM Accounting_Invoices I      
        JOIN Accounting_InvoiceDetails ID      
            ON ID.Invoice_Id = I.Id AND ID.Status = 1      
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD      
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
        JOIN Accounting_LU_LineItems_Directions LD      
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
        JOIN Accounting_LU_LineItems_Clients LIC      
            ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id  AND LIC.Status = 1      
        JOIN Accounting_LU_LineItems LI      
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1      
        WHERE LD.Direction_Id = 1 AND LI.LineItemType_Id IN (3)      
        GROUP BY I.PolicyDetails_Id, ID.Invoice_Id      
    ),      
      
    ----------------------------------------------------------------------      
    -- Coverage Aggregation      
    ----------------------------------------------------------------------      
   CoverageAgg AS      
    (      
        SELECT      
            DC.Invoice_Id,      
            STRING_AGG(LOB.Code, ', ') AS CoverageCode,  
   STRING_AGG(LOB.Name, ', ') AS CoverageCodeDescription  
        FROM      
        (      
            SELECT DISTINCT Invoice_Id, LOB_Id      
            FROM Accounting_InvoiceDetails      
            WHERE Status = 1      
        ) DC      
        JOIN Accounting_LU_LOBs LOB      
            ON LOB.LOB_Id = DC.LOB_Id AND LOB.Status = 1      
        GROUP BY DC.Invoice_Id      
    ),      
      
    ----------------------------------------------------------------------      
    -- CustomerType normalization and aggregation      
    -- Include invoices only if all tokens for a PolicyDetails_Id are either all 'CL' or all 'PL'      
    ----------------------------------------------------------------------      
    LineItemCT AS      
    (      
        SELECT      
            PDLI.PolicyDetails_Id,      
            UPPER(LTRIM(RTRIM(s.value))) AS CTVal      
        FROM Accounting_PolicyDetailsLineItems PDLI      
        CROSS APPLY STRING_SPLIT(PDLI.CustomerType, ',') AS s      
        WHERE PDLI.CustomerType IS NOT NULL      
    ),      
    LineItemCTAgg AS      
    (      
        SELECT      
            PolicyDetails_Id,      
            COUNT(DISTINCT CASE WHEN CTVal IN ('CL','PL') THEN CTVal END) AS DistinctCTCount,      
            MAX(CASE WHEN CTVal IN ('CL','PL') THEN CTVal END)            AS SingleCT,      
            SUM(CASE WHEN CTVal NOT IN ('CL','PL') THEN 1 ELSE 0 END)     AS NonCLPLCount      
        FROM LineItemCT      
        GROUP BY PolicyDetails_Id      
    )      
      
    ----------------------------------------------------------------------      
    ----------------------------------------------------------------------      
    SELECT      
        PD.PolicyNumber,      
        PD.EffectiveDate,      
        I.Id AS InvoiceId,      
        I.InvoiceNo,      
        PD.Id AS PolicyDetails_Id,      
        I.DueDate,      
      
        SUM(ID.Amount) AS InvoiceAmount,      
        SUM(ID.RemainingBalance) AS RemainingBalance,      
      
        CASE      
            WHEN @EntityType_Id = 1 THEN AD.Name      
            WHEN @EntityType_Id = 2 THEN IND.Name      
            WHEN @EntityType_Id = 3 THEN CD.Name      
        END AS PayerName,      
      
        IND.Name AS InsuredName,      
        PD.ExpirationDate,      
        CA.CoverageCode,      
        CA.CoverageCodeDescription,  
        CAST(ISNULL(I.Is_Financed,0) AS INT) AS FlagFinanced,      
      
        CASE PD.BillType      
            WHEN 'Agency' THEN 'AB'      
            WHEN 'Insured' THEN 'IB'      
            WHEN 'Direct' THEN 'DB'      
            ELSE ''      
        END AS BillingType,      
      
        MD.MinimumPremium,      
      
        CASE PD.TransactionType WHEN 'NEW' THEN 'NBS' ELSE PD.TransactionType END AS InvoiceTypeID,      
      
        MAX(LCTA.SingleCT) AS CustomerType,      
      
        C.Phone AS ClientPhone,      
        C.Email AS ClientEmail,      
        IND.DBA AS DBAName,      
      
        IND.Physical_Address2 AS InsuredPhysicalSuite,      
        IND.Physical_Address1 AS InsuredPhysicalStreet,      
        IND.Physical_City     AS InsuredPhysicalCity,      
        IND.Physical_State    AS InsuredPhysicalState,      
        IND.Physical_ZipCode  AS InsuredPhysicalPostalCode,      
      
        IND.Address2 AS InsuredMailingSuite,      
        IND.Address1 AS InsuredMailingStreet,      
        IND.City     AS InsuredMailingCity,      
        IND.State    AS InsuredMailingState,      
        IND.ZipCode  AS InsuredMailingPostalCode,      
      
        CD.Name      AS CarrierName,      
        CD.Address2  AS CarrierSuite,      
        CD.Address1  AS CarrierStreet,      
        CD.City      AS CarrierCity,      
        CD.State     AS CarrierState,    
  CD.NAIC      AS CarrierNAIC,  
        CD.ZipCode   AS CarrierPostalCode,      
      
        C.CompanyName AS MGAContactName,      
        C.Phone       AS MGAContactPhone,      
        C.Address     AS MGAContactStreet,      
        C.City        AS MGAContactCity,      
        C.State       AS MGAContactState,      
        C.ZipCode     AS MGAContactPostalCode,      
      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Name     END AS RetailAgentName,      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Address2 END AS RetailAgentSuite,      
        CASE WHEN PD.BillType = 'Agency' THEN AD.Address1 END AS RetailAgentStreet,      
        CASE WHEN PD.BillType = 'Agency' THEN AD.City     END AS RetailAgentCity,      
        CASE WHEN PD.BillType = 'Agency' THEN AD.State    END AS RetailAgentState,      
        CASE WHEN PD.BillType = 'Agency' THEN AD.ZipCode  END AS RetailAgentPostalCode      
      
    FROM Accounting_PolicyDetails PD      
    LEFT JOIN Accounting_AgentDetails AD      
       ON AD.AgentDetail_Id = PD.AgentDetail_Id AND AD.Status = 1      
    LEFT JOIN Accounting_InsuredDetails IND      
        ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1      
    LEFT JOIN Accounting_CarrierDetails CD      
        ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1      
    JOIN Accounting_Invoices I      
        ON PD.Id = I.PolicyDetails_Id AND I.Status = 1      
    JOIN Accounting_InvoiceDetails ID      
        ON ID.Invoice_Id = I.Id AND ID.Status = 1      
    JOIN Accounting_LU_BillingType_LineItems_Directions BLD      
        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1      
    JOIN Accounting_LU_LineItems_Directions LD      
        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1      
    JOIN Accounting_Clients C      
        ON C.Client_Id = PD.Client_Id AND C.Status = 1      
      
    LEFT JOIN MinimumPremiumData MD ON MD.Invoice_Id = I.Id      
    LEFT JOIN TotalFees          TF ON TF.Invoice_Id = I.Id      
    LEFT JOIN TotalTaxes         TA ON TA.Invoice_Id = I.Id      
    LEFT JOIN CoverageAgg        CA ON CA.Invoice_Id = I.Id      
    LEFT JOIN LineItemCTAgg   LCTA ON LCTA.PolicyDetails_Id = PD.Id      
      
    WHERE      
        PD.Client_Id = @Client_Id      
        AND PD.Status = 1      
        AND LD.Direction_Id = 1      
        AND I.CreatedOn <= GETDATE()      
        AND (      
            (@EntityType_Id = 1 AND AD.AgentDetail_Id = @Entity_Id AND PD.BillType = 'Agency')      
         OR (@EntityType_Id = 2 AND IND.InsuredDetail_Id = @Entity_Id AND PD.BillType = 'Insured')      
         OR (@EntityType_Id = 3 AND CD.CarrierDetail_Id  = @Entity_Id AND PD.BillType = 'Direct')      
        )      
        AND I.RemainingBalance <> 0      
      
              
        AND LCTA.DistinctCTCount = 1      
        AND ISNULL(LCTA.NonCLPLCount, 0) = 0      
      
    GROUP BY      
        I.Id, PD.PolicyNumber, PD.EffectiveDate, I.InvoiceNo,      
        PD.Id, I.DueDate, I.RemainingBalance,      
        AD.Name, IND.Name, CD.Name, IND.Name, PD.ExpirationDate,      
        I.Is_Financed, PD.BillType, MD.MinimumPremium, PD.TransactionType,      
        C.Phone, C.Email, IND.DBA,      
        IND.Physical_Address2, IND.Physical_Address1, IND.Physical_City,      
        IND.Physical_State, IND.Physical_ZipCode,      
        IND.Address2, IND.Address1, IND.City, IND.State, IND.ZipCode,      
        CD.Name, CD.Address2, CD.Address1, CD.City, CD.State, CD.NAIC, CD.ZipCode,      
        C.CompanyName, C.Phone, C.Address, C.City, C.State, C.ZipCode,      
        AD.Name, AD.Address2, AD.Address1, AD.City, AD.State, AD.ZipCode,      
        CA.CoverageCode, CA.CoverageCodeDescription      
      
    ORDER BY I.Id DESC;      
END   