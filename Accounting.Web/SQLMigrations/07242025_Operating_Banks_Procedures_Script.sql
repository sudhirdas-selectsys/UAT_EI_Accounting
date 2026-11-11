--Old Procedure
--CREATE PROCEDURE [dbo].[USP_Accounting_GetAllOperatingBanks]  
--(  
--@PageNumber INT=1,  
--@PageSize INT=10,  
--@SearchValue VARCHAR(100)=NULL,  
--@Client_Id BIGINT=NULL,  
--@TotalRecords BIGINT OUT  
--)    
--AS    
--BEGIN    
-- IF(@SearchValue IS NOT NULL AND @SearchValue<>'')    
--  BEGIN    
--   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_OperatingBanks WHERE Client_Id=@Client_Id AND Status=1 AND Name LIKE '%' +@SearchValue+ '%';    
--   SELECT * FROM Accounting_LU_OperatingBanks WHERE Client_Id=@Client_Id AND Status=1 AND Name LIKE '%' +@SearchValue+ '%'    
--   ORDER BY OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;    
--  END    
--    
-- ELSE    
--  BEGIN    
--   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_OperatingBanks  WHERE Status=1;    
--   SELECT * FROM Accounting_LU_OperatingBanks WHERE Client_Id=@Client_Id AND Status=1    
--   ORDER BY OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;    
--  END    
--END

Alter PROCEDURE [dbo].[USP_Accounting_GetAllOperatingBanks]    
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
   SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,OBCOA.Client_Id,OBCOA.Service_Id FROM Accounting_LU_OperatingBanks OB  
   LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1  
   WHERE OB.Client_Id=@Client_Id AND OB.Status=1 AND OB.Name LIKE '%' +@SearchValue+ '%'      
   ORDER BY OB.OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;      
  END      
      
 ELSE      
  BEGIN      
   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_OperatingBanks  WHERE Status=1;      
   SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,OBCOA.Client_Id,OBCOA.Service_Id FROM Accounting_LU_OperatingBanks OB  
   LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1  
   WHERE OB.Client_Id=@Client_Id AND OB.Status=1      
   ORDER BY OperatingBank_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;      
  END      
END
go

--Old Procedure
--CREATE PROCEDURE [dbo].[USP_Accounting_GetOperatingBank]  
--(  
--@OperatingBank_Id INT  
--)    
--AS    
--BEGIN    
--SELECT * FROM Accounting_LU_OperatingBanks WHERE OperatingBank_Id = @OperatingBank_Id AND Status = 1;    
--END

Alter PROCEDURE [dbo].[USP_Accounting_GetOperatingBank]  
(  
@OperatingBank_Id INT,
@Client_Id BIGINT
)    
AS    
BEGIN
SELECT OB.*,OBCOA.ChartOfAccounts_Id AS OperatingBankCOA_Id,OBCOA.Client_Id,OBCOA.Service_Id FROM Accounting_LU_OperatingBanks OB
LEFT JOIN Accounting_LU_OperatingBanks_ChartOfAccounts OBCOA ON OBCOA.OperatingBank_Id=OB.OperatingBank_Id AND OBCOA.Client_Id=@Client_Id AND OBCOA.Status=1
WHERE OB.OperatingBank_Id = @OperatingBank_Id AND OB.Client_Id=@Client_Id AND OB.Status = 1;
END
go

--Old Procedure
--CREATE PROCEDURE [dbo].[USP_Accounting_DeleteOperatingBank](@OperatingBank_Id INT)    
--AS    
--BEGIN    
--UPDATE Accounting_LU_OperatingBanks SET Status = 0, UpdatedOn = GETDATE() WHERE OperatingBank_Id = @OperatingBank_Id AND Status = 1;    
--END


Alter PROCEDURE USP_Accounting_DeleteOperatingBank
(
@OperatingBank_Id INT,
@Client_Id BIGINT
)
AS
BEGIN
UPDATE Accounting_LU_OperatingBanks SET Status = 0, UpdatedOn = GETDATE() WHERE OperatingBank_Id = @OperatingBank_Id AND Client_Id = @Client_Id AND Status = 1;
UPDATE Accounting_LU_OperatingBanks_ChartOfAccounts SET Status = 0, UpdatedOn = GETDATE() WHERE OperatingBank_Id = @OperatingBank_Id AND Client_Id = @Client_Id AND Status = 1;
END
go