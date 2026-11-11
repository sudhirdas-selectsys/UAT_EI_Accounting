
--insert into Accounting_Reports(Name) values ('Insured Billed - Commission Statement Report')
Select * from Accounting_Reports
select * from Accounting_ReportsQueries
select * from Accounting_LU_SQLQueries
--insert into Accounting_LU_SQLQueries(SQLQuery, Is_StoredProcedure, Description) values('USP_Accounting_InsuredBilledCommissionStatementReport', 1, 'Insured Billed Commission Statement Report for Insured Bill policies' )
--insert into Accounting_ReportsQueries (Report_Id, SQLQuery_Id, Sequence, Status) values (9, 14, 9,1)

--update Accounting_ReportsQueries set Sequence = 4 where Report_Id=9
--GO
--update Accounting_ReportsQueries set Sequence = 5 where Report_Id=4
--GO
--update Accounting_ReportsQueries set Sequence = 6 where Report_Id=5
--GO
--update Accounting_ReportsQueries set Sequence = 7 where Report_Id=6
--GO
--update Accounting_ReportsQueries set Sequence = 8 where Report_Id=7
--GO
--update Accounting_ReportsQueries set Sequence = 9 where Report_Id=8
--GO

Select * from Accounting_ReportsSettings

select * from Accounting_Clients

--INSERT INTO Accounting_ReportsSettings (Client_Id, Report_Id, Sequence, Status)
--VALUES
--(1, 9, 4, 1),
--(10001, 9, 4, 1),
--(10002, 9, 4, 1),
--(10003, 9, 4, 1),
--(10004, 9, 4, 1),
--(10005, 9, 4, 1),
--(10006, 9, 4, 1),
--(10007, 9, 4, 1),
--(10008, 9, 4, 1),
--(10009, 9, 4, 1),
--(10010, 9, 4, 1),
--(10011, 9, 4, 1),
--(10013, 9, 4, 1),
--(10014, 9, 4, 1),
--(10015, 9, 4, 1),
--(10016, 9, 4, 1),
--(10017, 9, 4, 1),
--(10018, 9, 4, 1),
--(10019, 9, 4, 1);



select * from Accounting_LU_SQLInputParameters where SqlQuery_id=3


--INSERT INTO Accounting_LU_SQLInputParameters (
--    SQLQuery_Id,
--    ParameterName,
--    DisplayName,
--    Is_OutputParameter,
--    DataType_Id,
--    HTMLControlType_Id,
--    PredefinedParameter_Id,
--    DataDictionaryType_Id,
--    Dependency_SQLParameter_Id,
--    RangeGroupName,
--    ApplyRangeValidation,
--    ShowInputField,
--    IsRequired,
--    MultiInputSeparator,
--    Sequence,
--    Status,
--    CreatedOn,
--    AllowFilterWith_Null,
--    AllowFilterWith_Empty
--)
--VALUES
--(
--14, --SQLQuery_Id
--'Client_Id', --ParameterName
--'Client ID', --DisplayName
--0, --Is_OutputParameter
--2, --DataType_Id
--1, --HTMLControlType_Id
--1, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--0, --ShowInputField
--1, --IsRequired
--NULL, --MultiInputSeparator
--4, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),

--(
--14, --SQLQuery_Id
--'InvoiceDate', --ParameterName
--'Invoice Date', --DisplayName
--0, --Is_OutputParameter
--NULL, --DataType_Id
--3, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--1, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceCreatedDate', --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--4, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),

--(
--14, --SQLQuery_Id
--'InvoiceCreatedFromDate', --ParameterName
--'Created From Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceCreatedDate', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--12, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),

--(
--14, --SQLQuery_Id
--'InvoiceCreatedToDate', --ParameterName
--'Created To Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceCreatedDate', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--14, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'InvoiceEffectiveFromDate', --ParameterName
--'Effective From Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceEffectiveDate', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--12, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),

--(
--14, --SQLQuery_Id
--'InvoiceEffectiveToDate', --ParameterName
--'Effective To Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceEffectiveDate', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--14, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'Range', --ParameterName
--'Range', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--5, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--3, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--10, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'AgentDetail_Id', --ParameterName
--'Agent/Broker', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--5, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--5, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--9, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'WholesalerDetail_Id', --ParameterName
--'Wholesaler/MGA', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--5, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--6, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--15, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'CarrierDetail_Id', --ParameterName
--'Carrier', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--5, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--7, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--13, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'LOB_Id', --ParameterName
--'LOBs', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--5, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--8, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--11, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'OutstandingBalances', --ParameterName
--'OutstandingBalances', --DisplayName
--0, --Is_OutputParameter
--4, --DataType_Id
--4, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--9, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--17, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'As_Of_Date', --ParameterName
--'As Of Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--16, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'Simplified', --ParameterName
--'Simplified', --DisplayName
--0, --Is_OutputParameter
--4, --DataType_Id
--4, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--10, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--18, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'InvoiceNumber', --ParameterName
--'Invoice Number', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--1, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--19, --Sequence
--1, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'PolicyNumber', --ParameterName
--'Policy Number', --DisplayName
--0, --Is_OutputParameter
--1, --DataType_Id
--1, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--NULL, --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--3, --Sequence
--0, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'PolicyFromDate', --ParameterName
--'From Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'Date', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--1, --Sequence
--0, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'PolicyToDate', --ParameterName
--'To Date', --DisplayName
--0, --Is_OutputParameter
--6, --DataType_Id
--6, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--NULL, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'Date', --RangeGroupName
--1, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--2, --Sequence
--0, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--),
--(
--14, --SQLQuery_Id
--'InvoiceEffectiveDate', --ParameterName
--'Invoice Effective Date', --DisplayName
--0, --Is_OutputParameter
--NULL, --DataType_Id
--3, --HTMLControlType_Id
--NULL, --PredefinedParameter_Id
--2, --DataDictionaryType_Id
--NULL, --Dependency_SQLParameter_Id
--'InvoiceEffectiveDate', --RangeGroupName
--0, --ApplyRangeValidation
--1, --ShowInputField
--0, --IsRequired
--NULL, --MultiInputSeparator
--10, --Sequence
--0, --Status
--GETDATE(), --CreatedOn
--0, --AllowFilterWith_Null
--0 --AllowFilterWith_Empty
--)


-- newly created TaxFilingReport similar to Invoice Production Report except at the end, have three columns for every tax: <Tax Name>-Invoiced, <Tax Name>-Collected, <Tax Name>-Paid
  
select * from Accounting_LU_SQLQueries
select * from Accounting_PolicyDetails where BillType='Insured' and Status=1
USP_Accounting_InsuredBilledCommissionStatementReport
@Client_Id=10017

Alter Proc [USP_Accounting_InsuredBilledCommissionStatementReport]                                                                          
(                                                                                      
 @Client_Id bigint = null,                                                                                     
 @AgentDetail_Id BIGINT = NULL,                                                                                      
 @InvoiceDate VARCHAR(100) = NULL,                                                                                      
 @InvoiceCreatedFromDate VARCHAR(200) = NULL,                                                                                      
 @InvoiceCreatedToDate VARCHAR(200) = NULL,                                                                                      
 @InvoiceEffectiveFromDate VARCHAR(200) = NULL,                                                                                    
 @InvoiceEffectiveToDate VARCHAR(200) = NULL,                                                                                      
 @Range VARCHAR(200) = NULL ,                                                                                     
 @WholesalerDetail_Id BIGINT = NULL,                  
 @InvoiceNumber VARCHAR(200) = NULL,                       
 @CarrierDetail_Id BIGINT = NULL,                                                                                  
 @LOB_Id INT = NULL   ,                                                                  
 @OutstandingBalances bit = null   ,                                           
 @As_Of_Date VARCHAR(200) = NULL,                                        
 @Simplified bit = null                                         
  )                                                                                      
  as                                                                                      
  begin                                                                                      
                                                                            
    DECLARE @CompareDateFrom DATETIME;                                                                                      
    DECLARE @CompareDateTo DATETIME;                                                                                      
                                                                                      
    IF @InvoiceDate = 'InvoiceCreatedDate'                                                                                      
    BEGIN                                                                                      
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedFromDate, 101));                                                                                      
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedToDate, 101))));                                                                                      
    END                                                                                      
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'                                                                                      
    BEGIN                                                                                      
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveFromDate, 101));                                                                                      
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveToDate, 101))));                                                                                      
    END                                                                                      
                     -- Table 0                                                                 
    SELECT *                                                                   
    FROM Accounting_AgentDetails                                        
    WHERE Client_Id = @Client_Id AND Status = 1                     
      AND (@AgentDetail_Id IS NULL OR AgentDetail_Id = @AgentDetail_Id);                                          
                                     
       -- Table 1                  
 SELECT  PD.*                                                                             
 FROM Accounting_PolicyDetails PD                
 WHERE PD.Client_Id = @Client_Id                                            
 AND PD.Status = 1                           
 AND PD.BillType IN ('Insured')                                                                      
 AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)                                                                         
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)                            
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id) 

  ;WITH PolicyLOBs AS (                                                                   
  SELECT DISTINCT                                                                                   
  PD.PolicyNumber,                                                                            
  PD.AgentDetail_Id,                                                                                  
  IND.Name AS InsuredName,                                                                        
  LOB.Name AS LOBName,                                                                                  
  CD.Name AS CarrierName                                                           
  FROM Accounting_PolicyDetails PD                                                                                  
  JOIN Accounting_InsuredDetails IND ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1                                                                                  
  JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1                                                                                  
  JOIN Accounting_Invoices IES ON IES.PolicyDetails_Id = PD.ID AND IES.Status = 1                                                                                  
  JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = IES.ID AND ID.Status = 1                                                                                  
  JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1                                                                                  
  WHERE PD.Client_Id = @Client_Id                                                                   
  AND PD.BillType IN ('Insured')                                                                  
  AND PD.Status = 1                                 
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)                                                                            
  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)                  
  AND (@InvoiceNumber IS NULL OR IES.InvoiceNo=@InvoiceNumber)                           
  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)                                        
  --AND (@As_Of_Date IS NULL OR PD.EffectiveDate <= @As_Of_Date)                                        
  AND (@LOB_Id IS NULL OR EXISTS (                                           
  SELECT 1                                                                        
  FROM Accounting_InvoiceDetails AID                                                        
  WHERE AID.Invoice_Id = IES.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id))   
  )                                                           
    --  Table 2                                                   
   SELECT                                                                               
   PolicyNumber,                                                                                  
   InsuredName,                                                                             
   AgentDetail_Id,                                                                                  
   CarrierName,                                                        
   STUFF((                                                                                  
   SELECT DISTINCT ', ' + L2.LOBName                                                                                  
   FROM PolicyLOBs L2                                                                                  
   WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName                                                                                  
   FOR XML PATH(''), TYPE                                                                                  
   ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames                                                    
   FROM PolicyLOBs L1                  
   GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;                                                                                  
           
 ;WITH PDL_Agg AS (                        
    SELECT                         
        PolicyDetails_Id,                        
        MAX(AgentCommission) AS AgentCommission                        
    FROM Accounting_PolicyDetailsLineItems                         
 WHERE Status = 1                        
      AND AgentCommission > 0             
    GROUP BY PolicyDetails_Id                        
),AggregatedPremiumCommissionInvoiceDetails AS (                                                                  
   SELECT                                                                  
   I.Id AS InvoiceId,                                                        
   I.PolicyDetails_Id,                                                                  
   I.InvoiceNo,                                                                  
   SUM(CASE WHEN LI.Code = 'Premium' AND LUD.Code = 'AR' THEN ID.Amount ELSE 0 END)         AS PremiumAR,                                                                  
   SUM(CASE WHEN LI.Code = 'AgencyBrokerCommission' AND LUD.Code = 'C' THEN ID.Amount ELSE 0 END) AS AgentCommission,                                                                  
   SUM(CASE WHEN LI.Code = 'MGACommission' AND LUD.Code = 'IN' THEN ID.Amount ELSE 0 END)     AS MGACommission                                                                  
   FROM Accounting_Invoices I                                                                    
   JOIN Accounting_PolicyDetails PD                     
   ON PD.ID = I.PolicyDetails_Id                                                                         
   AND PD.Client_Id = @Client_Id                                                                         
   AND PD.Status = 1                           
   AND PD.BillType IN ('Insured')                                                                             
                                                                        
   JOIN Accounting_InvoiceDetails ID                                   
   ON ID.Invoice_Id = I.Id                                                                         
   AND ID.Status = 1                                                                        
                                                                        
   JOIN Accounting_LU_BillingType_LineItems_Directions BLD                                                                         
   ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id                                                                         
   AND BLD.Status = 1                                                                        
                                                                        
   JOIN Accounting_LU_LineItems_Directions LD                                                                         
   ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id                                                                     
   AND LD.Status = 1                                                                
                                                              
   JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1                                                            
                                                                        
   JOIN Accounting_LU_LineItems LI                                                                         
   ON LI.LineItem_Id = LIC.LineItem_Id                                                                         
   AND LI.Status = 1                                                                        
                                                
   JOIN Accounting_LU_Directions LUD                                                                         
   ON LUD.Direction_Id = LD.Direction_Id                                                                         
   AND LUD.Status = 1                                                                    
                                                                    
   GROUP BY I.Id, I.PolicyDetails_Id, I.InvoiceNo                                                                  
   ),                                                                  
  FeeData AS (                                                                  
    SELECT                                          
    I.PolicyDetails_Id,                                                                        
    I.Id AS Invoice_Id,                                
    ID.Id AS InvoiceDetail_Id,                                                                   
    ABD.AgentDetail_Id,                                                                  
    ABD.Name AS [Agency Name],                                                                  
    CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],                                                                  
    CASE                                                                   
    WHEN LI.Name = 'Total Premium' THEN CONCAT(PD.TransactionType, ' ','Premium')                                                                  
    ELSE CONCAT(PD.TransactionType, ' ', LI.Name)                                                                  
    END AS LineItemDescription,                                                                                     
    I.InvoiceNo AS IdNo,                                                                                      
    PD.EffectiveDate,                                                                                      
    PD.ExpirationDate,                                                                                      
    ID.Amount AS GrossAmount,                                                                       
    ISNULL(ROUND(ID.Amount * (PDL.AgentCommission / 100.0), 2), 0.00) AS AgentCommissionAmt,                                                
    CASE WHEN PDL.AgentCommission IS Null THen 0 ELSE PDL.AgentCommission                                                                  
    END AS AgentCommPercentage,        
      CASE           
	  WHEN ISNULL(Details.TotalPremium, 0) <> 0 THEN Details.TotalPremium           
	  WHEN ISNULL(Details.TotalInsuredPremium, 0) <> 0 THEN Details.TotalInsuredPremium           
	  ELSE 0       
	  END AS NetAmount,                 
 I.DueDate,                      
    PD.PolicyNumber,                                                                          
    ID.RemainingBalance AS RemainingBalance,                                                                
    ISNULL((                                                                  
    SELECT SUM(PALI.AppliedAmount)                                                                  
    FROM Accounting_PaymentApplicationLineItems PALI                         
    JOIN Accounting_PaymentApplications PA ON PALI.PaymentApplication_Id = PA.PaymentApplication_Id AND PA.Status = 1                                                                  
    JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1                                                               
    WHERE PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1                                                                  
    ), 0.00) AS TotalCommissionReceived,                                                          
                                                        
    -- Inferred Premium Collected                                                                  
     CAST(                        
      CASE                                                                   
     WHEN Details.TotalCommissionPercentage IS NOT NULL                                                                   
       AND Details.TotalCommissionPercentage <> 0                                                                   
     THEN ISNULL(ROUND(Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0), 2), 0.00)                                                                  
     ELSE 0.00                                                                  
      END AS FLOAT                                                                  
    ) AS InferredPremiumCollected,                                                         
                                                         
      CASE                                                                       
       WHEN Details.TotalCommission IS NOT NULL AND Details.TotalCommissionPercentage IS NOT NULL AND Details.TotalCommissionPercentage <> 0                                                                      
       THEN ISNULL(ROUND((Details.TotalCommission / (Details.TotalCommissionPercentage / 100.0)),2),0.00)                                                                       
       ELSE 0.00                                                                      
       END AS TotalPremium,                                       
      CASE                                                                     
       WHEN Details.TotalCommission IS NOT NULL                                                                     
       AND Details.TotalCommission <> 0                                                                     
       AND Details.TotalCommissionPercentage IS NOT NULL                                                                     
       AND Details.TotalCommissionPercentage <> 0                                                                    
       AND PDL.AgentCommission IS NOT NULL                                                                     
       AND PDL.AgentCommission <> 0                                                                    
      THEN                                                                
      ISNULL(ROUND((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))                                                                     
      * (PDL.AgentCommission / 100.0),2),0.00)                       
      ELSE 0.00                                                                          
      END AS [Comm.],                                                               
      ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],                                                                  
      AID.Name As [Insured Name],                                                                  
      PD.PolicyNumber AS [Policy Number],                                                          
   CD.CarrierDetail_Id,                                                                    
      CD.Name AS [Carrier],                                                                  
      LOB.Name AS [LOB],                                                                  
      PD.TransactionType AS [Transaction Type]  ,                                                               
      CASE                              
     WHEN Details.TotalCommission IS NOT NULL   
  AND Details.TotalCommission <> 0                                                                  
     AND Details.TotalCommissionPercentage IS NOT NULL   
  AND Details.TotalCommissionPercentage <> 0                                                                
     AND PDL.AgentCommission IS NOT NULL   
  AND PDL.AgentCommission <> 0                                                                  
     THEN           
     ISNULL(ROUND((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))                                                                
       * (PDL.AgentCommission / 100.0) ,2),0.00)                                                               
                                                                   
     ELSE 0.00                                                          
     END AS CommsNetDue                                                               
     , CASE                                                                     
       WHEN Details.TotalCommissionReceived IS NOT NULL   
    AND Details.TotalCommissionReceived <> 0                                          
       AND Details.TotalCommissionPercentage IS NOT NULL   
    AND Details.TotalCommissionPercentage <> 0                                 
       AND PDL.AgentCommission IS NOT NULL   
    AND PDL.AgentCommission <> 0                              
      THEN                                                   
   ISNULL(ROUND(                                                
      (((Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))                                                                     
      * (PDL.AgentCommission / 100.0)) - (Details.CommPaidToDate))                                                
   ,2),0.00)                                                
      ELSE 0.00                                                                   
   END AS [Comm. Due],    
   PD.EndorsementNumber    
    FROM Accounting_Invoices I                                                                     
                                                                   
    LEFT JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id                                                 
        AND PD.Status = 1                             
 LEFT JOIN PDL_Agg PDL ON PDL.PolicyDetails_Id = PD.Id                         
 LEFT JOIN Accounting_InvoiceDetails ID                                                                       
        ON ID.Invoice_Id = I.Id                                                                  
        AND ID.Status = 1                        
                          
    LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD                                        
        ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id                                                                       
        AND BLD.Status = 1                                                                                      
                                                                      
    LEFT JOIN Accounting_LU_LineItems_Directions LD                                                                       
        ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id                                                                       
        AND LD.Status = 1                                                                                      
 LEFT JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1                                                            
                 
    LEFT JOIN Accounting_LU_LineItems LI                                                                       
        ON LI.LineItem_Id = LIC.LineItem_Id                                                                       
        AND LI.Status = 1                                                                                              
                                                                      
    LEFT JOIN Accounting_LU_LineItemTypes LIT                                                                       
        ON LIT.LineItemType_Id = LI.LineItemType_Id                                     
        AND LIT.Status = 1                                                                 
                                                                      
    LEFT JOIN Accounting_LU_Directions LUD                                                                       
        ON LUD.Direction_Id = LD.Direction_Id                                                                       
  AND LUD.Status = 1                             
                                                                      
    LEFT JOIN Accounting_LU_LOBs LOB                                                                       
        ON LOB.LOB_Id = ID.LOB_Id                                                    
        AND LOB.Status = 1                                                                      
                                                                   
    LEFT JOIN Accounting_InsuredDetails AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1                                                                  
    LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1                                                                  
 LEFT JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =PD.AgentDetail_Id AND ABD.Status=1                                                                  
                                                              
 OUTER APPLY (                                                                          
    SELECT                             
		CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalInsuredPremium,                      
        CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,                                                                       
        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,                                                                      
        CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,                                                                  
		CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name = 'Accounts Payable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS CommPaidToDate,                                                                   
        CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,                                                                       
        CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,                                                                          
        CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission                                                                      
  FROM (                                       
        SELECT                                                                           
            ID.Amount,                                                                          
            ID.RemainingBalance,                                                      
            I.DueDate AS DueDateLocal,                                                                          
            LIT.Name AS LIT_Name,                                                                          
            LI.Code AS LI_Code,                                                                          
            LI.Name AS LI_Name,                                                                          
            LUD.Name AS LUD_Name  ,                                                                  
   ID.LOB_Id,                                          
   (ID.Amount - ID.RemainingBalance) AS TotalReceivable                                             
        FROM Accounting_InvoiceDetails ID                                                                          
         JOIN Accounting_LU_BillingType_LineItems_Directions BLD                                                                           
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                                          
         JOIN Accounting_LU_LineItems_Directions LD                                                                           
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                            
      JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1                                                            
         JOIN Accounting_LU_LineItems LI                                                                           
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                                          
         JOIN Accounting_LU_LineItemTypes LIT                                                                           
            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1                                                                   
     JOIN Accounting_LU_Directions LUD                                       
            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1                                                                   
         JOIN Accounting_LU_BillingTypes BT                                                                           
            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1 and BT.Code IN ('Insured')                                                                     
        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1                                                                           
    ) AS DetailView                                                                          
) AS Details                                                                   
                                                                  
    WHERE I.Status = 1 AND PD.BillType IN ('Insured') 
  AND  LUD.Code IN ('IN','AR')                                                                    
  AND              
(              
      LIT.Code = 'Premium'              
   --OR (PD.BillType = 'Insured' AND LI.Code = 'AgencyBrokerCommission')              
)                       
  --LIT.Code = 'Commission'              
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)                                                                              
  AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)                  
  AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)                  
  AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)                                         
  AND (@LOB_Id IS NULL OR EXISTS (                                                                            
    SELECT 1                                
    FROM Accounting_InvoiceDetails AID                                                                 
    WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id)                                                                  
 )                                       
                                       
  AND (@As_Of_Date IS NULL OR PD.EffectiveDate <= @As_Of_Date)                                                           
 )                                                                       
                                                    
   , PaymentApplication AS (                                   
  SELECT                                                           
            I.PolicyDetails_Id,                                                                                      
            I.Id AS Invoice_Id,                                                                         
            ID.Id AS InvoiceDetail_Id,                                                                    
      ABD.AgentDetail_Id,                                                                  
   ABD.Name AS [Agency Name],                                                                  
   CONCAT(ABD.Address1, ', ', ABD.City, ', ', ABD.State, ', ', ABD.ZipCode) AS [Agency Address],                                                                                    
            'Comm. Paid' AS LineItemDescription,                                                          
            CONCAT('PMT', P.Payment_Id) AS IdNo,                                                   
            CAST(P.PaymentDate AS DATE) AS EffectiveDate,                                                    
            PD.ExpirationDate,                                                                                                  0 AS GrossAmount,                                                                    
   0 AS AgentCommPercentage,                                                                    
            CASE                       
    WHEN ISNULL(Details.TotalPremium, 0) > 0 THEN Details.TotalPremium                      
    WHEN ISNULL(Details.TotalInsuredPremium, 0) > 0 THEN Details.TotalInsuredPremium                      
    ELSE 0                       
END AS NetAmount,                                                                
            I.DueDate,                                                                               
            PD.PolicyNumber,                                                                         
            0 AS RemainingBalance,                                                                        
   0 AS AgentCommissionAmt,                                                        
   0 AS InferredPremiumCollected,                        
  CASE                                                                     
   WHEN Details.TotalCommission IS NOT NULL                                                         
         AND Details.TotalCommission <> 0                                                                     
     AND Details.TotalCommissionPercentage IS NOT NULL                                                                     
         AND Details.TotalCommissionPercentage <> 0                                                                    
         AND PDL.AgentCommission IS NOT NULL                                                                     
   AND PDL.AgentCommission <> 0                                                                    
  THEN                                                        
        (Details.TotalCommissionReceived / (Details.TotalCommissionPercentage / 100.0))                                                                     
        * (PDL.AgentCommission / 100.0)                                                                  
  ELSE 0.00                                                           
  END AS [Comm.] ,                                                                    
  ISNULL((Details.CommPaidToDate), 0.00) AS [Comm. Paid to Date],                                                                  
  AID.Name As [Insured Name],                                                                  
  PD.PolicyNumber AS [Policy Number],                                                           
  CD.CarrierDetail_Id ,                                                            
  CD.Name AS [Carrier],                                                                  
  LOB.Name AS [LOB],                         
  PD.TransactionType AS [Transaction Type]  ,                                                                 
  CASE WHEN P.PaymentDirection_Id = 2 THEN                                                                  
     ROUND(-PALI.AppliedAmount,2) ELSE ROUND(ABS(PALI.AppliedAmount),2) END AS CommsNetDue,                                                                  
  0 AS [Comm. Due],          
  0 AS TotalPremium,                                                              
  ISNULL((                                                                  
  SELECT SUM(PALI2.AppliedAmount)                                                                  
  FROM Accounting_PaymentApplicationLineItems PALI2                                                                  
  JOIN Accounting_PaymentApplications PA2 ON PALI2.PaymentApplication_Id = PA2.PaymentApplication_Id AND PA2.Status = 1                                                                  
  JOIN Accounting_Payments P2 ON P2.Payment_Id = PA2.Payment_Id AND P2.Status = 1                                                                  
  WHERE PALI2.InvoiceLineItem_Id = ID.Id AND PALI2.Status = 1                                                                  
  ), 0.00) AS TotalCommissionReceived,    
  PD.EndorsementNumber    
        FROM Accounting_Invoices I                                                                        
         JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType IN ('Insured') 
         JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1 
         JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1                             
         JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                                                               
		 JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1 
         JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1     
         JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1
         JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1   
         JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1    
         JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1 
         JOIN Accounting_PaymentApplications PA ON PA.PaymentApplication_Id = PALI.PaymentApplication_Id AND PA.Status = 1  
         JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1  
		 AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)  
         AND P.Payment_Id IN (      
            SELECT DISTINCT P2.Payment_Id FROM Accounting_Payments P2         
            JOIN Accounting_PaymentApplications PA2 ON P2.Payment_Id = PA2.Payment_Id AND PA2.Status = 1    
            JOIN Accounting_PaymentApplicationLineItems PALI2 ON PA2.PaymentApplication_Id = PALI2.PaymentApplication_Id AND PALI2.Status = 1 
            JOIN Accounting_InvoiceDetails ID2 ON PALI2.InvoiceLineItem_Id = ID2.Id AND ID2.Status = 1 
            JOIN Accounting_Invoices I2 ON ID2.Invoice_Id = I2.Id AND I2.Status = 1    
            JOIN Accounting_PolicyDetails PD2 ON PD2.ID = I2.PolicyDetails_Id AND PD2.Status = 1 
            WHERE (@AgentDetail_Id IS NULL OR PD2.AgentDetail_Id = @AgentDetail_Id)                                    
        )                                                            
		LEFT JOIN Accounting_InsuredDetails AID on AID.InsuredDetail_Id =PD.InsuredDetail_Id AND AID.Status=1                                                                  
		LEFT JOIN Accounting_CarrierDetails CD on CD.CarrierDetail_Id =PD.CarrierDetail_Id AND CD.Status=1                                                                  
		LEft JOIN Accounting_AgentDetails ABD on ABD.AgentDetail_Id =PD.AgentDetail_Id AND ABD.Status=1   
		OUTER APPLY (                                                                    
		SELECT                                
		CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalInsuredPremium,                                                     
		      CAST(SUM(CASE WHEN LIT_Name = 'PREMIUM' AND  LUD_Name='Internal' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalPremium,                                                                        
		      CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name='Accounts Receivable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommission,                                                                              
			CAST(SUM(CASE WHEN LI_Name = 'Total Commission' AND LUD_Name = 'Accounts Receivable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionReceived,                                                                  
		      CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name = 'Accounts Payable' THEN TotalReceivable ELSE 0 END) AS DECIMAL(18,2)) AS CommPaidToDate,                                                                      
		      CAST(SUM(CASE WHEN LI_Name = 'Total Commission Percentage'  AND LUD_Name='Internal'  THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalCommissionPercentage,                                                                       
		      CAST(SUM(CASE WHEN LI_Code = 'MGACommission' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalHouseCommission,                            
		   CAST(SUM(CASE WHEN LI_Name = 'Agency/Broker Commission' AND LUD_Name='Accounts Payable' THEN Amount ELSE 0 END) AS DECIMAL(18,2)) AS TotalAgentBrokerCommission                                                                      
  FROM (                                                           
       SELECT                                                                           
            ID.Amount,                                                                          
            ID.RemainingBalance,                                                                          
            I.DueDate AS DueDateLocal,                                                                          
            LIT.Name AS LIT_Name,                                       
            LI.Code AS LI_Code,                                                                          
            LI.Name AS LI_Name,                                                                          
            LUD.Name AS LUD_Name  ,                                                                  
   ID.LOB_Id,                                                         
   (ID.Amount - ID.RemainingBalance) AS TotalReceivable                            
        FROM Accounting_InvoiceDetails ID                                                                          
         JOIN Accounting_LU_BillingType_LineItems_Directions BLD                                                  
            ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1                                                                          
         JOIN Accounting_LU_LineItems_Directions LD                       
            ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1                                                                
   JOIN Accounting_LU_LineItems_Clients LIC on LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id and LIC.Status = 1                                                            
         JOIN Accounting_LU_LineItems LI                                                
            ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1                                                             
         JOIN Accounting_LU_LineItemTypes LIT                                                                           
            ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1                                                                          
         JOIN Accounting_LU_Directions LUD                          
            ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1                                                                          
         JOIN Accounting_LU_BillingTypes BT                                                                           
            ON BT.BillingType_Id = BLD.BillingType_Id AND BT.Status = 1 -- and BT.Code='Direct'                                                                     
        WHERE ID.Invoice_Id = I.Id AND ID.Status = 1   ) AS DetailView                                
) AS Details                                                                   
   JOIN (SELECT PolicyDetails_Id,MAX(HouseCommission) AS HouseCommission,MAX(AgentCommission) AS AgentCommission                                                                        
  FROM Accounting_PolicyDetailsLineItems WHERE Status = 1 GROUP BY PolicyDetails_Id                                                                         
  ) PDL ON PDL.PolicyDetails_Id = I.PolicyDetails_Id                                                                        
       WHERE                                                                         
  I.Status = 1                                                     
  AND (                                                              
  @CompareDateFrom IS NULL OR @CompareDateTo IS NULL                                                          
  OR (                            
  @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL                                    
                AND P.PaymentDate > @CompareDateFrom AND P.PaymentDate <= @CompareDateTo                                    
            )                                    
        )                                    
        AND (@As_Of_Date IS NULL OR P.PaymentDate <= @As_Of_Date)                                    
        AND LUD.Code = 'AP'                                    
        AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)                  
  AND (@InvoiceNumber IS NULL OR I.InvoiceNo=@InvoiceNumber)                  
        AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)                                    
        AND (@LOB_Id IS NULL OR EXISTS (                                    
            SELECT 1                 
            FROM Accounting_InvoiceDetails AID                                    
            WHERE AID.Invoice_Id = I.Id AND AID.Status = 1 AND AID.LOB_Id = @LOB_Id                                    
        ))                                
)                                                                  
    -- Table 3                   
SELECT                                                                   
    PolicyDetails_Id,                                                     
    Invoice_Id,                                    
    InvoiceDetail_Id,                                                                  
    LineItemDescription,                                                                  
    IdNo,                                                                  
    EffectiveDate,                                                                  
    ExpirationDate,                                                   
  GrossAmount,                                                                  
    AgentCommissionAmt,                                                                  
    AgentCommPercentage ,                                                                  
    CAST(ISNULL(NetAmount, 0.00) AS FLOAT) AS NetAmount,                                                                  
    DueDate,                                                                  
    PolicyNumber,                                                                  
    RemainingBalance,                                            
    InferredPremiumCollected,                                    
    [Comm.],                                                                  
    [Comm. Paid to Date],                                                                  
    [Insured Name],                                                                  
    [Policy Number],                                                                  
    [Carrier],                                                        
    CarrierDetail_Id,                                                          
    [LOB],     
    [Transaction Type],                                                                  
    CommsNetDue,                                                                  
 [Comm. Due],                                                                
    TotalPremium,                                                                  
 TotalCommissionReceived,                                        
 AgentDetail_Id,                                                                  
 [Agency Name],                                                                  
 [Agency Address],    
 EndorsementNumber    
FROM FeeData                               
                                                                  
UNION ALL                                                                  
                                             
SELECT                                                                   
    PolicyDetails_Id,                                                                  
    Invoice_Id,                                                                  
    InvoiceDetail_Id,                                                                  
    LineItemDescription,                                    
    IdNo,                                                                  
    EffectiveDate,                                                                  
    ExpirationDate,                                                                  
    GrossAmount,                                                                  
    AgentCommissionAmt,                                                                  
    0 AS AgentCommPercentage,                                             
    CAST(ISNULL(0, 0.00) AS FLOAT) AS NetAmount,                                                                  
    DueDate,                                                                  
    PolicyNumber,                                                                  
    RemainingBalance,                                                                  
    0 AS InferredPremiumCollected,                                                                  
    [Comm.],                                                                  
    [Comm. Paid to Date],                                       
    [Insured Name],                                                                  
    [Policy Number],                                                                  
    [Carrier],                                                        
    CarrierDetail_Id,                                                                    
    [LOB],                                                                  
    [Transaction Type],                                                                  
    CommsNetDue,                                                                
  [Comm. Due],                                                                  
  TotalCommissionReceived,                                                                  
  0 AS TotalPremium,                                                                  
  AgentDetail_Id,                                                                  
  [Agency Name],                                                                  
  [Agency Address],    
  EndorsementNumber    
 FROM PaymentApplication                                                                  
                          
-- Table 4                                               
  Select Logo AS ClientLogo from Accounting_Clients where Client_Id=@Client_Id AND Status = 1;                                                                  
                                                                  
 END   
GO

Select * from Accounting_ReportFileDownloadSettings
select * from  Accounting_LU_FileTypes
select * from Accounting_ReportsSettings order by Sequence asc , Client_Id asc

--UPDATE Accounting_ReportsSettings
--SET Sequence = Sequence + 1
--WHERE Report_Id BETWEEN 4 AND 8
--AND Client_Id IN (1,10001,10002,10003,10004,10005,10006,10007,10008,10009,
--                  10010,10011,10012,10013,10014,10015,10016,10017,10018,10019);

--UPDATE Accounting_ReportsSettings
--SET Sequence = 4
--WHERE Report_Id = 9
--AND Client_Id IN (1,10001,10002,10003,10004,10005,10006,10007,10008,10009,
--                  10010,10011,10012,10013,10014,10015,10016,10017,10018,10019);
-- Download Excel option provided for Tax Filing Report
	--insert into Accounting_ReportFileDownloadSettings(Report_Id, FileType_Id) values (9, 1),(9, 3),(9, 5)

	
--	Insert Into Accounting_LU_ReportTemplates (Report_Id,Client_Id,Name,Code,Template)
--	values (9,10001,'Insured Billed Account Statement','InsuredBilledAccountStatement',
--	'<!DOCTYPE html>  
--<html lang="en">
--   <head>
--      <meta charset="UTF-8" />
--      {{fonts}}      
--      <style>          
--	  .border-top-1 {              border-top: 1px solid #CCCCCC;          }            
--	  .border-bottom-1 {              border-bottom: 1px solid #CCCCCC;          }            
--	  body {              
--	  font-family: "Poppins", 
--	  sans-serif;              
--	  font-size: 13px;              
--	  color: #333;              
--	  font-weight: 400;              
--	  line-height: normal;          
--	  }            
--	  .mt-0 {              
--	  margin-top: 0;          
--	  }            
--	  .mb-0 {              
--	  margin-bottom: 0;          
--	  }            
--	  .pb-10 {              
--	  padding-bottom: 10px;          
--	  }            
--	  .statement-date {              
--	  color: #289FD7;              
--	  font-weight: 600;              
--	  font-size: 24px;              
--	  padding: 8px 0;              
--	  text-align: center;              
--	  border-top: solid 1px #085284;              
--	  border-bottom: solid 1px #085284;          
--	  }            
--	  .agency-name {              
--	  background-color: #085284;              
--	  padding: 6px 12px;              
--	  color: #FFF;              
--	  font-size: 16px;          
--	  }            
--	  .pc-left {              
--	  background-color: #F0F0F0;              
--	  padding: 10px;              
--	  width: 240px;              
--	  color: #085284;             
--	  vertical-align: top;          
--	  }            
--	  .pc-right {              
--	  padding: 0 10px;              
--	  border: solid 1px #CCCCCC;              
--	  border-left: none;              
--	  position: relative;              
--	  vertical-align: top;          
--	  }            
--	  .pc-right table thead th,          
--	  .pc-right table tbody td {              padding: 12px 4px;          }            
--	  .pc-right table tfoot td {              padding: 12px 4px;          }            
--	  .pc-right table tbody {              vertical-align: top;          }            
--	  .page-break {              page-break-after: always;              break-after: page;          }      
--	  </style>
--   </head>
--   <body>
--      <table width="100%" cellpadding="0" cellspacing="0" border="0">
--         <tr>
--            <td style="padding: 20px 0; color: #333; font-size: 15px; line-height: 23px;">
--               <table border="0" cellspacing="0" cellpadding="0" width="1050" align="center" style="border-color: rgba(0, 0, 0, .09); margin-bottom: 100px; min-width: 99%;">
--                  <tr>
--                     <td>
--                        <!-- Header Section -->                              
--                        <table cellpadding="0" cellspacing="0" width="100%">
--                           <tr>
--                              <td>
--                                 <table cellpadding="0" cellspacing="0" width="100%">
--                                    <tr>
--                                       <td align="left"> {{clientLogo}} </td>
--                                       <td style="text-align: right;" width="80%">
--                                          <div style="font-weight: bold; font-size: 13px;"> {{clientCompanyName}} </div>
--                                          <div style="font-size: 13px;"> {{clientAddress}}<br /> {{clientCityStateZip}}<br /> </div>
--                                       </td>
--                                    </tr>
--                                 </table>
--                              </td>
--                           </tr>
--                        </table>
--                        <!-- Statement Date -->                              
--                        <table cellpadding="0" cellspacing="0" style="width: 100%;">
--                           <tr>
--                              <td class="statement-date"> Commission Statements as of {{statementDate}} </td>
--                           </tr>
--                        </table>
--                        <!-- Carrier Blocks Section -->                              
--                        <table cellpadding="0" cellspacing="0" class="body-table"> {{carrierBlocks}} </table>
--                     </td>
--                  </tr>
--               </table>
--            </td>
--         </tr>
--      </table>
--   </body>
--</html>')