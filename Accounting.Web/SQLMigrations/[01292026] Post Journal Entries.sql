

--ALTER TABLE Accounting_Payments
--ADD JournalIndex INT NULL;
select * from Accounting_Payments where Payment_Id = 20963


  
--ALTER proc [dbo].[USP_Accounting_InsertUpdatePayments]                    
--(                    
--@Payment_Id bigint output                    
--,@EntityType_Id tinyint = null                    
--,@Entity_Id bigint = null                     
--,@PaymentDate datetime = null                    
--,@Amount float = null                    
--,@PaymentType_Id tinyint = null                    
--,@ReferenceNumber varchar(100) = null                    
--,@PaymentStatus_Id tinyint = null                    
--,@OperatingBank_Id tinyint = null                    
--,@Notes varchar(500) = null                  
--,@RemainingAmount float = null                 
--,@PaymentDirection_Id tinyint = null          
--,@Client_Id bigint = null     
--)                    
--As                    
--Begin                    
--if exists (select 1 from Accounting_Payments where Payment_Id = @Payment_Id and Status = 1)                    
--begin                    
--update Accounting_Payments set                    
--EntityType_Id = @EntityType_Id                    
--,Entity_Id = @Entity_Id                    
--,PaymentDate = @PaymentDate                    
--,Amount = @Amount                    
--,PaymentType_Id = @PaymentType_Id                    
--,ReferenceNumber = @ReferenceNumber                    
--,PaymentStatus_Id = @PaymentStatus_Id                    
--,OperatingBank_Id = @OperatingBank_Id                    
--,Notes = @Notes                   
--,RemainingAmount = @RemainingAmount                 
--,PaymentDirection_Id = @PaymentDirection_Id        
--,Client_Id=@Client_Id    
--,UpdatedOn = GETDATE()                    
--where Payment_Id = @Payment_Id and Status = 1                    
--end                    
--else                    
--begin                    
--insert into Accounting_Payments                    
--(                    
--EntityType_Id                    
--,Entity_Id                    
--,PaymentDate                    
--,Amount                    
--,PaymentType_Id                    
--,ReferenceNumber                    
--,PaymentStatus_Id                    
--,OperatingBank_Id                    
--,Notes                    
--,RemainingAmount                 
--,PaymentDirection_Id           
--,Client_Id      
--)                    
--Values                    
--(                    
--@EntityType_Id                    
--,@Entity_Id                    
--,@PaymentDate                    
--,@Amount                    
--,@PaymentType_Id                    
--,@ReferenceNumber                    
--,@PaymentStatus_Id                    
--,@OperatingBank_Id                    
--,@Notes                    
--,@RemainingAmount                 
--,@PaymentDirection_Id          
--,@Client_Id        
--)                    
--set @Payment_Id = SCOPE_IDENTITY()                    
--end     
--end 
--go



--ALTER PROC USP_Accounting_InsertJournalEntries    
--(  
--	@JournalEntry_Id bigint output ,  
--	@SourceType_Id tinyint,  
--	@Source_Id bigint,  
--	@ExternalJournalEntryIdentifier varchar(250),
--	@JournalIndex int =null,
--	@JournalEntryName varchar(100) =null
--)    
--As  
--begin   
--Insert into Accounting_JournalEntries      
--(      
--SourceType_Id      
--,Source_Id      
--,ExternalJournalEntryIdentifier  
--,JournalIndex
--,JournalEntryName
--)      
--Values      
--(      
--@SourceType_Id      
--,@Source_Id      
--,@ExternalJournalEntryIdentifier
--,@JournalIndex
--,@JournalEntryName

--)  
--set @JournalEntry_Id = SCOPE_Identity()  
--end


--ALTER TABLE Accounting_JournalEntries
--ADD JournalIndex INT NULL;

--ALTER TABLE Accounting_JournalEntries
--ADD JournalEntryName VARCHAR(100) NULL;


--CREATE TABLE Accounting_PaymentJournalEntries (
--    Id bigint IDENTITY(1,1) NOT NULL,
--    Payment_Id bigint not null constraint FK_Accounting_PaymentJournalEntries_Payment_Id references Accounting_Payments(Payment_Id),
--    Client_Id bigint not null constraint FK_Accounting_PaymentJournalEntries_Client_Id references Accounting_Clients(Client_Id),
--    JournalIndex INT NOT NULL,
--    JournalEntryName VARCHAR(100) NOT NULL,
--    ExternalJournalEntryId VARCHAR(50) NOT NULL,
--    Amount float NULL,
--    Status bit not null  constraint DF_PaymentJournalEntries_Status default(1)
--	,CreatedOn datetime not null constraint DF_PaymentJournalEntries_CreatedOn default(Getdate())
--	,CreatedBy bigint not null constraint FK_PaymentJournalEntries_CreatedBy references Accounting_Users(User_Id)
--	,UpdatedOn datetime
--	,UpdatedBy bigint null constraint FK_PaymentJournalEntries_UpdatedBy references Accounting_Users(User_Id)
--);


--ALTER PROC [dbo].[USP_Accounting_InsertPaymentJournalEntries]  
--(  
--@PaymentJournalEntries_Id  bigint output ,  
--@Payments_Id bigint , 
--@Client_Id bigint , 
--@JournalIndex int  ,
--@JournalEntryName VARCHAR(100),
--@ExternalJournalEntryId VARCHAR(50),
--@Amount float null
--)  
--as  
--begin  
--	INSERT INTO Accounting_PaymentJournalEntries
--	(Payment_Id,Client_Id, JournalIndex, JournalEntryName, ExternalJournalEntryId, Amount)
--	VALUES
--	(@Payments_Id, @Client_Id,@JournalIndex,@JournalEntryName , @ExternalJournalEntryId, @Amount);

--	Set @PaymentJournalEntries_Id= SCOPE_IDENTITY() ;
--end



--CREATE proc [dbo].[USP_Accounting_DeletePaymentJournalEntries]  
--(  
--@Payments_Id bigint
--)  
--as  
--begin  
--	update Accounting_PaymentJournalEntries set Status=0 where Payment_Id=@Payments_Id;
--end


--CREATE PROC [dbo].[USP_Accounting_GetAllPaymentJournalEntries]  
--(  
--@Payments_Id bigint
--)  
--as  
--begin  
--	Select Payment_Id,Client_Id,JournalIndex,JournalEntryName,ExternalJournalEntryId,Amount from Accounting_PaymentJournalEntries 
--	where Payment_Id=@Payments_Id;
--end


-- updated USP_Accounting_GetAllPayments
--USP_Accounting_GetJournalEntriesLogByJournalEntry_Id


--ALTER Proc USP_Accounting_GetJournalEntriesLogByJournalEntry_Id  
--(          
--@JournalEntry_Id bigint          
--)          
--As          
--Begin          

--select JEL.*,JES.Name as JournalEntryStatusName, Concat(U.FirstName, ' ', U.LastName) as CreatedByName, DebitCOA.Name as DebitCOAName, CreditCOA.Name as CreditCOAName  ,JE.JournalEntryName As ApplicationNumber     
--from Accounting_JournalEntries JE 
--join Accounting_JournalEntriesLog JEL  on JE.JournalEntry_Id=JEL.JournalEntry_Id               
--join Accounting_LU_JournalEntryStatus JES on JES.JournalEntryStatus_Id = JEL.JournalEntryStatus_Id and JES.Status = 1          
--join Accounting_Users U on U.User_Id = JEL.CreatedBy and U.status = 1  
--left join Accounting_ChartOFAccounts DebitCOA on DebitCOA.ChartOfAccounts_Id = JEL.DebitChartOfAccount_Id and DebitCOA.Status = 1        
--left join Accounting_ChartOFAccounts CreditCOA on CreditCOA.ChartOfAccounts_Id = JEL.CreditChartOfAccount_Id and CreditCOA.Status = 1        
--where JEL.JournalEntry_Id = @JournalEntry_Id and JEL.status = 1          
--end
--go

  
--CREATE proc [dbo].[USP_Accounting_GetAllPayments]              
--(                
--@Client_Id bigint = null,              
--@PageSize int = null,            
--@PageNumber int,            
--@TotalRecords int out               
--)                
--as                      
--begin               
--If @PageSize is NULL or @PageSize = 0            
--begin            
--select * from Accounting_Payments WITH (NOLOCK) where Client_Id=@Client_Id and Status = 1 order by Payment_Id desc;            
--end            
--else              
--begin              
--SELECT p.*,JE.ExternalJournalEntryIdentifier,     
--coalesce(nullif( CASE                       
--        WHEN p.EntityType_Id = 1 THEN ag.Name                      
--        WHEN p.EntityType_Id = 3 THEN ca.Name                      
--        WHEN p.EntityType_Id = 2 THEN ins.Name                      
--  WHEN p.EntityType_Id = 4 THEN AC.CompanyName        
--  WHEN p.EntityType_Id = 6 THEN WSD.Name      
--   WHEN p.EntityType_Id = 5 THEN fd.Name      
--    WHEN p.EntityType_Id = 7 THEN SD.Name      
--        ELSE NULL                      
--    END,''),'NA') AS EntityName                      
--FROM Accounting_Payments p           
--LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id = P.Payment_Id AND JE.SourceType_Id = 3 AND (JE.JournalIndex = 0 OR JE.JournalIndex IS NULL) AND JE.Status = 1  
--LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                      
--LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                      
--LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1        
--LEFT JOIN Accounting_FinanceDetails fd ON fd.FinanceDetail_Id = p.Entity_Id AND p.EntityType_Id = 5 AND fd.Status = 1                      
--LEFT JOIN Accounting_StateDetails SD on SD.StateDetail_Id = p.Entity_Id AND p.EntityType_Id = 7 and SD.Status = 1    
--LEFT JOIN Accounting_Clients AC on AC.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND AC.Status = 1         
--LEFT JOIN Accounting_WholesalerDetails WSD  ON WSD.WholesalerDetail_Id = p.Entity_Id AND p.EntityType_Id = 6 AND WSD.Status = 1         
--WHERE p.Client_Id=@Client_Id and p.Status = 1 order by p.Payment_Id desc OFFSET @PageSize * @PageNumber ROWS FETCH NEXT @PageSize ROWS ONLY;              
--end            
--set @TotalRecords= (select count(*) from Accounting_Payments WITH (NOLOCK) where Client_Id = @Client_Id and status = 1);            
--end 
--go

