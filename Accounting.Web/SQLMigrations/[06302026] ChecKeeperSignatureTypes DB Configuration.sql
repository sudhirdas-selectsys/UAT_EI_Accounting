select * from Accounting_LU_OperatingBanks


--ALTER TABLE Accounting_LU_OperatingBanks
--ADD AccountNumber VARCHAR(100) NULL,
--    RoutingNumber VARCHAR(100) NULL,
--    SignerValue VARCHAR(100) NULL;

--ALTER TABLE Accounting_LU_OperatingBanks ADD SignerType VARCHAR(10) NULL;

--ALTER TABLE Accounting_LU_OperatingBanks alter column SignerValue NVARCHAR(MAX) NULL


--ALTER PROCEDURE [dbo].[USP_Accounting_InsertUpdateOperatingBanks]   
--(  
--@OperatingBank_Id INT OUTPUT,  
--@Name VARCHAR(100)=NULL,  
--@Code VARCHAR(100)=NULL,  
--@Client_Id BIGINT=NULL,
--@AccountNumber VARCHAR(100)=NULL,
--@RoutingNumber VARCHAR(100)=NULL,
--@SignerValue NVARCHAR(MAX)=NULL,
--@SignerType VARCHAR(10) =NULL
--)      
--AS      
--BEGIN      
-- IF EXISTS (SELECT 1 FROM Accounting_LU_OperatingBanks WHERE OperatingBank_Id = @OperatingBank_Id AND Client_Id=@Client_Id AND Status=1)      
-- BEGIN      
-- UPDATE Accounting_LU_OperatingBanks SET Name=@Name,Code=@Code,UpdatedOn=GETDATE(),AccountNumber=@AccountNumber,RoutingNumber=@RoutingNumber,SignerValue=@SignerValue,SignerType=@SignerType WHERE OperatingBank_Id=@OperatingBank_Id AND Client_Id=@Client_Id AND Status=1;      
-- END  
-- ELSE  
-- BEGIN    
--  INSERT INTO Accounting_LU_OperatingBanks(Name,Code,Client_Id,AccountNumber,RoutingNumber,SignerValue,SignerType) VALUES (@Name,@Code,@Client_Id,@AccountNumber,@RoutingNumber,@SignerValue,@SignerType);      
--  SET @OperatingBank_Id=SCOPE_IDENTITY();    
-- END  
--END


CREATE TABLE Accounting_LU_ChecKeeperSignatureTypes
(
    SignatureType_Id TINYINT IDENTITY(1,1) CONSTRAINT UK_Accounting_LU_CheckKeeperSignatureTypes_SignatureType_Id Unique NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Status BIT NOT NULL CONSTRAINT DF_Accounting_LU_CheckKeeperSignatureTypes_Status DEFAULT (1),
    CreatedOn DATETIME NOT NULL CONSTRAINT DF_Accounting_LU_CheckKeeperSignatureTypes_CreatedOn DEFAULT (GETDATE()),
    UpdatedOn DATETIME NULL
);

Create Clustered Index CLU_Accounting_LU_ChecKeeperSignatureTypes_Status_SignatureType_Id
on Accounting_LU_ChecKeeperSignatureTypes(Status desc,SignatureType_Id asc)


--insert into Accounting_LU_ChecKeeperSignatureTypes (Name) values('Text'),('Image')


select * from Accounting_LU_ChecKeeperSignatureTypes



CREATE Proc USP_Accounting_GetAllChecKeeperSignatureTypes
as    
Begin    
select * from Accounting_LU_ChecKeeperSignatureTypes where status = 1    
end



