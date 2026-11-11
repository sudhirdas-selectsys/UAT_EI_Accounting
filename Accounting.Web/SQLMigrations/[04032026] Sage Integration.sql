
--Insert into Accounting_LU_ServiceLevelParameters(Service_Id,Parameter_Id,DataType_Id,DefaultValue)
--values (6,1,1,'https://api.intacct.com/ia/xml/xmlgw.phtml')

--insert into Accounting_LU_ServiceProviders(ServiceProviderType_ID,Name) 
--values(1,'Sage');

--Insert into Accounting_LU_Services(Name,ServiceProvider_Id,ServiceProviderType_Id)
--values ('Sage',5,1)


--1 BaseURL
--2 Company_Id


--Insert into Accounting_LU_Parameters(Name,DataType_Id)
--values ('Sender_Id',1),('Sender_Password',1),('User_Id',1),('User_Password',1)
--GO


--Insert into Accounting_LU_ClientLevelParameters(Client_Id,Service_Id,Parameter_Id,Value)
--values(10011,6,1,'https://api.intacct.com/ia/xml/xmlgw.phtml'),
--	  (10011,6,2,'canngeninsurance-DEV'),
--	  (10011,6,9,'canngeninsurance'),
--	  (10011,6,10,'sT5y7>i8Ag;,'),
--	  (10011,6,11,'cg_dev_webservices'),
--	  (10011,6,12,'C07cuiYJj3@')
--GO

--update Accounting_Clients set AccountingService_Id=6 ,AccountingType_Id=2 where Client_id=10011
--Go


--Alter proc USP_Accounting_GetOperatingBanks_ChartOfAccounts      
--(            
--@Service_Id tinyint            
--,@Client_Id bigint            
--)            
--as               
--begin              
--select OB.Name as BankName,OBC.ChartOfAccounts_Id,OBC.Client_Id,OBC.Service_Id,OB.* from Accounting_LU_OperatingBanks OB      
--left join Accounting_LU_OperatingBanks_ChartOfAccounts OBC on OBC.OperatingBank_Id=OB.OperatingBank_Id and OBC.Status=1 and OBC.Client_Id=10011 and OBC.Service_Id=@Service_Id
--Where OB.status=1 and OB.Client_Id = @Client_Id;      
--end

--Alter procedure USP_Accounting_Insert_LU_OperatingBanks_ChartOfAccounts    
--(    
--@Client_Id bigint    
--,@Service_Id tinyint    
--,@OperatingBank_Id int    
--,@ChartOfAccounts_Id int    
--)    
--as    
--begin    
--if exists(select 1 from Accounting_LU_OperatingBanks_ChartOfAccounts where  Client_Id=@Client_Id and OperatingBank_Id=@OperatingBank_Id and Service_Id=@Service_Id  and status=1)      
-- begin update Accounting_LU_OperatingBanks_ChartOfAccounts set ChartOfAccounts_Id=@ChartOfAccounts_Id, UpdatedOn=GETDATE() where  Client_Id=@Client_Id and OperatingBank_Id=@OperatingBank_Id    
--  and Service_Id=@Service_Id and status=1    
-- end    
--else    
--begin    
--  insert into Accounting_LU_OperatingBanks_ChartOfAccounts(    
--  Client_Id    
--  ,Service_Id    
--  ,OperatingBank_Id    
--  ,ChartOfAccounts_Id    
--  )    
--  values(    
--  @Client_Id    
--  ,@Service_Id    
--  ,@OperatingBank_Id    
--  ,@ChartOfAccounts_Id    
--  )    
--end    
--end
--GO

--CREATE PROCEDURE USP_Accounting_UpdateJournalEntryExternalId
--(
--    @JournalEntry_Id INT,
--    @ExternalJournalEntryIdentifier NVARCHAR(100)
--)
--AS
--BEGIN
--    UPDATE Accounting_JournalEntries
--    SET ExternalJournalEntryIdentifier = @ExternalJournalEntryIdentifier
--    WHERE JournalEntry_Id = @JournalEntry_Id
--END
--GO

--CREATE PROCEDURE USP_Accounting_GetPaymentDirectionCOA
--(
--    @Service_Id INT,
--    @Client_Id INT,
--    @PaymentDirection_Id INT
--)
--AS
--BEGIN
--    SET NOCOUNT ON;

--    SELECT 
--        PDCOA.PaymentDirection_Id,

--        -- Standard COA
--        STD_COA.ChartOfAccounts_Id AS StandardCOA_Id,
--        STD_COA.Name AS StandardCOAName,

--        -- Writeoff COA
--        WR_COA.ChartOfAccounts_Id AS WriteoffCOA_Id,
--        WR_COA.Name AS WriteoffCOAName

--    FROM PaymentDirectionsChartOfAccounts PDCOA

--    LEFT JOIN ChartOfAccounts STD_COA 
--        ON STD_COA.ChartOfAccounts_Id = PDCOA.StandardCOA_Id

--    LEFT JOIN ChartOfAccounts WR_COA 
--        ON WR_COA.ChartOfAccounts_Id = PDCOA.WriteoffCOA_Id

--    WHERE 
--        PDCOA.Service_Id = @Service_Id
--        AND PDCOA.Client_Id = @Client_Id
--        AND PDCOA.PaymentDirection_Id = @PaymentDirection_Id
--END
--Go

--CREATE OR ALTER proc USP_Accounting_GetPaymentDirections_SageChartOfAccounts  
--(            
--@Service_Id tinyint            
--,@Client_Id bigint,
--@PaymentDirection_Id tinyint
--)            
--as               
--begin              
--SELECT  
--    PD.Name AS Direction,
--    PD.PaymentDirection_Id,

--    -- IDs
--    MAX(CASE WHEN PDST.Code = 'Standard' THEN PDC.ChartOfAccounts_Id END) AS StandardCOA_Id,
--    MAX(CASE WHEN PDST.Code = 'Writeoff' THEN PDC.ChartOfAccounts_Id END) AS WriteoffCOA_Id,
--    -- Standard COA Details
--    MAX(CASE WHEN PDST.Code = 'Standard' THEN COA_STD.Name END) AS StandardCOAName,
--    -- Writeoff COA Details
--    MAX(CASE WHEN PDST.Code = 'Writeoff' THEN COA_WR.Name END) AS WriteoffCOAName
--FROM Accounting_LU_PaymentDirections PD          

--LEFT JOIN Accounting_LU_PaymentDirections_ChartOfAccounts PDC 
--    ON PDC.PaymentDirection_Id = PD.PaymentDirection_Id 
      
--    AND PDC.Status = 1     

--LEFT JOIN Accounting_LU_PaymentDirectionsSubTypes PDST 
--    ON PDST.PaymentDirectionsSubType_Id = PDC.PaymentDirectionsSubType_Id 
--    AND PDST.Status = 1  

--LEFT JOIN Accounting_ChartOfAccounts COA_STD 
--    ON COA_STD.ChartOfAccounts_Id = PDC.ChartOfAccounts_Id 
--    AND PDST.Code = 'Standard' and COA_STD.Status=1

--LEFT JOIN Accounting_ChartOfAccounts COA_WR 
--    ON COA_WR.ChartOfAccounts_Id = PDC.ChartOfAccounts_Id 
--    AND PDST.Code = 'Writeoff' and COA_WR.Status=1

--WHERE 
--    PD.Status = 1 AND PDC.Client_Id = @Client_Id 
--    AND PDC.Service_Id = @Service_Id  
--    AND PDC.PaymentDirection_Id = @PaymentDirection_Id

--GROUP BY 
--    PD.Name,
--    PD.PaymentDirection_Id
--end
--GO


--CREATE OR ALTER PROC USP_Accounting_GetChartOfAccountDetails  
--(            
--@ChartOfAccount_Id int            
--,@Client_Id bigint            
--)            
--as               
--begin              
--select ChartOfAccounts_Id,Name
--,Code
--,AccountType
--,AccountSubType
--,Classicfication
--,ReferenceID
--,Client_Id
--,Service_Id 
--from Accounting_ChartOfAccounts where ChartOfAccounts_Id=@ChartOfAccount_Id and Client_Id=@Client_Id and Status=1
--end
--GO


--Insert into Accounting_LU_Clients_AccountingTypes_BillingTypes (Client_Id
--,BillingType_Id
--,AccountingType_Id) values (10011,1,)
--GO


--Alter procedure USP_Accounting_Insert_LU_LineItems_ChartOfAccounts      
--(      
--@Client_Id bigint      
--,@Service_Id tinyint      
--,@BillingType_LineItems_Directions_Id int      
--,@ChartOfAccount_Id int   
--,@LOB_Carrier_Grouping_Id int = null  
--,@ChartOfAccountsClassificationType_Id tinyint = null  
--)      
--as      
--begin      
--if exists(select 1 from Accounting_LU_LineItems_ChartOfAccounts where  Client_Id=@Client_Id and 
--BillingType_LineItems_Directions_Id=@BillingType_LineItems_Directions_Id and Service_Id=@Service_Id  
--and status=1 and LOB_Carrier_Grouping_Id = @LOB_Carrier_Grouping_Id 
--and ChartOfAccountsClassificationType_Id = @ChartOfAccountsClassificationType_Id)        
-- begin update Accounting_LU_LineItems_ChartOfAccounts set ChartOfAccount_Id=@ChartOfAccount_Id, UpdatedOn = GETDATE() where  Client_Id=@Client_Id and BillingType_LineItems_Directions_Id=@BillingType_LineItems_Directions_Id      
--  and Service_Id=@Service_Id and status=1  and LOB_Carrier_Grouping_Id = @LOB_Carrier_Grouping_Id and ChartOfAccountsClassificationType_Id = @ChartOfAccountsClassificationType_Id    
-- end      
--else      
--begin      
--  insert into Accounting_LU_LineItems_ChartOfAccounts(      
--  Client_Id      
--  ,Service_Id      
--  ,BillingType_LineItems_Directions_Id      
--  ,ChartOfAccount_Id  
--  ,LOB_Carrier_Grouping_Id  
--  ,ChartOfAccountsClassificationType_Id  
--  )      
--  values(      
--  @Client_Id      
--  ,@Service_Id      
--  ,@BillingType_LineItems_Directions_Id      
--  ,@ChartOfAccount_Id   
--  ,@LOB_Carrier_Grouping_Id  
--  ,@ChartOfAccountsClassificationType_Id  
--  )      
--end      
      
--end
go
CREATE PROCEDURE [dbo].[USP_Accounting_GetOperatingBank]    
(        
@OperatingBank_Id INT,      
@Client_Id BIGINT      
)          
AS          
BEGIN      
SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,  
case when OBCOA.Client_Id is null then C.Client_Id else OBCOA.Client_Id end as Client_Id,case when OBCOA.Service_Id is null then C.AccountingService_Id else OBCOA.Service_Id  end as Service_Id   
FROM Accounting_LU_OperatingBanks OB   
JOIN Accounting_Clients C on C.Client_Id = OB.Client_Id     
LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1 AND OBCOA.Service_Id = C.AccountingService_Id      
WHERE OB.OperatingBank_Id = @OperatingBank_Id AND OB.Client_Id=@Client_Id AND OB.Status = 1;      
END 
go
CREATE PROCEDURE [dbo].[USP_Accounting_GetAllOperatingBanks]           
(            
@PageNumber INT=1,            
@PageSize INT=10,            
@SearchValue VARCHAR(100)=NULL,            
@Client_Id BIGINT=NULL,            
@TotalRecords BIGINT OUT            
)              
AS              
BEGIN              
 IF(@SearchValue IS NOT NULL AND @SearchValue<>'')              
  BEGIN              
   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_OperatingBanks WHERE Client_Id=@Client_Id AND Status=1 AND Name LIKE '%' +@SearchValue+ '%';              
   SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,OBCOA.Client_Id,OBCOA.Service_Id, Cast(Case when csdb.OperatingBank_Id is not null then 1 else 0 end as bit) as DefaultPaymentOperatingBank FROM Accounting_LU_OperatingBanks OB          
   LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1 AND OBCOA.Service_Id = (  
        SELECT AccountingService_Id   
        FROM Accounting_Clients   
        WHERE Client_Id = @Client_Id  
    )      
   left join Accounting_ClientServiceDefaultOperatingBank csdb on csdb.OperatingBank_Id = OB.OperatingBank_Id and  csdb.ServiceProviderType_Id = 2 and csdb.Status = 1  
   WHERE OB.Client_Id=@Client_Id AND OB.Status=1 AND OB.Name LIKE '%' +@SearchValue+ '%'            
   ORDER BY OB.OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;              
  END              
              
 ELSE              
  BEGIN              
   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_OperatingBanks  WHERE Status=1;              
   SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,OBCOA.Client_Id,OBCOA.Service_Id, Cast(Case when csdb.OperatingBank_Id is not null then 1 else 0 end as bit) as DefaultPaymentOperatingBank FROM Accounting_LU_OperatingBanks OB          
   LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1  AND OBCOA.Service_Id = (  
        SELECT AccountingService_Id   
        FROM Accounting_Clients   
        WHERE Client_Id = @Client_Id  
    )     
   left join Accounting_ClientServiceDefaultOperatingBank csdb on csdb.OperatingBank_Id = OB.OperatingBank_Id and  csdb.ServiceProviderType_Id = 2 and csdb.Status = 1  
   WHERE OB.Client_Id=@Client_Id AND OB.Status=1              
   ORDER BY OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;              
  END              
END 