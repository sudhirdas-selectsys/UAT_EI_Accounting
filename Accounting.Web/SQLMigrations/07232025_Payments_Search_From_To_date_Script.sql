Alter proc USP_Accounting_GetPaymentsSearchResults_paging                          
(                          
@EntityType_Id tinyint = null                          
,@Entity_Id bigint = null 
--@PaymentDate  datetime = null 
,@PaymentFromDate datetime = null 
,@PaymentToDate datetime = null 
,@Amount float = null                          
,@PaymentType_Id tinyint = null                          
,@ReferenceNumber varchar(100) = null                          
,@PaymentStatus_Id tinyint = null                          
,@OperatingBank_Id tinyint = null                          
,@Notes varchar(500) = null                     
,@PaymentDirection_Id tinyint = null          
,@Client_Id bigint = null        
,@OutstandingBalances bit = null    
,@PageSize int = null
,@PageNumber int,@TotalRecords int out    
)                          
As                          
Begin     
If @PageSize is NULL or @PageSize = 0         begin    
SELECT * FROM Accounting_Payments p  
LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1  
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                        
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                        
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                     
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                        
                  
WHERE  p.Client_Id=@Client_Id and p.Status = 1                         
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                        
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                        
AND (@Amount   IS NULL OR p.Amount = @Amount)                        
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                        
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                        
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)                        
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                    
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)        
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0))         
order by p.Payment_Id desc;     
end    
else    
begin    
SELECT     
p.*,JE.ExternalJournalEntryIdentifier,                        
CASE                         
WHEN p.EntityType_Id = 1 THEN ag.Name                        
WHEN p.EntityType_Id = 3 THEN ca.Name                        
WHEN p.EntityType_Id = 2 THEN ins.Name                    
WHEN p.EntityType_Id = 4 THEN CL.CompanyName                    
ELSE NULL END AS EntityName                        
FROM Accounting_Payments p    
LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1   
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                        
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                        
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                     
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                        
                  
WHERE  p.Client_Id=@Client_Id and p.Status = 1                         
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                        
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)                        
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                        
AND (@Amount   IS NULL OR p.Amount = @Amount)                        
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                        
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                        
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)                        
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                    
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)        
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0))         
order by p.Payment_Id desc OFFSET @PageSize * @PageNumber ROWS FETCH NEXT @PageSize ROWS ONLY;    
end    
set @TotalRecords= (SELECT COUNT(*) FROM Accounting_Payments p  
LEFT JOIN Accounting_JournalEntries JE ON JE.Source_Id=P.Payment_Id AND JE.SourceType_Id = 3 AND JE.Status = 1  
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = p.Entity_Id AND p.EntityType_Id = 1 AND ag.Status = 1                        
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = p.Entity_Id AND p.EntityType_Id = 3 AND ca.Status = 1                        
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = p.Entity_Id AND p.EntityType_Id = 2 AND ins.Status = 1                     
LEFT JOIN Accounting_Clients CL ON CL.Client_Id = p.Entity_Id AND p.EntityType_Id = 4 AND CL.Status = 1                        
                  
WHERE  p.Client_Id=@Client_Id and p.Status = 1                         
AND (@EntityType_Id IS NULL OR p.EntityType_Id = @EntityType_Id)                        
AND (@Entity_Id  IS NULL OR p.Entity_Id = @Entity_Id)                        
AND (@PaymentFromDate IS NULL OR @PaymentToDate IS NULL OR CAST(p.PaymentDate AS DATE) BETWEEN CAST(@PaymentFromDate AS DATE) AND CAST(@PaymentToDate AS DATE))
--AND (@PaymentDate IS NULL OR CAST(p.PaymentDate AS DATE) = CAST(@PaymentDate AS DATE))                        
AND (@Amount   IS NULL OR p.Amount = @Amount)                        
AND (@PaymentType_Id IS NULL OR p.PaymentType_Id = @PaymentType_Id)                        
AND (@ReferenceNumber IS NULL OR p.ReferenceNumber LIKE '%' + @ReferenceNumber + '%')                        
AND (@PaymentStatus_Id IS NULL OR p.PaymentStatus_Id = @PaymentStatus_Id)                        
AND (@OperatingBank_Id IS NULL OR p.OperatingBank_Id = @OperatingBank_Id)                        
AND (@Notes IS NULL OR p.Notes LIKE '%' + @Notes + '%')                    
AND (@PaymentDirection_Id IS NULL OR p.PaymentDirection_Id = @PaymentDirection_Id)        
AND (@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND p.RemainingAmount != 0)))    
end
go


Create Table Accounting_LOB_Mapping
(
LOB_Mapping_Id int not null constraint UK_Accounting_LOB_Mapping_LOB_Mapping_Id unique identity(1,1)
,LOB_Id tinyint constraint FK_Accounting_LOB_Mapping_LOB_Id references Accounting_LU_LOBs (LOB_Id)
,Client_Id bigint constraint FK_Accounting_LOB_Mapping_Client_Id references Accounting_Clients (Client_Id)
,CreatedBy bigint constraint FK_Accounting_LOB_Mapping_CreatedBy references Accounting_Users (User_Id)
,Status bit not null constraint DF_Accounting_LOB_Mapping_Status default (1)
,CreatedOn datetime not null constraint DF_Accounting_LOB_Mapping_CreatedOn default (Getdate())
,UpdatedOn datetime
)
go
Create Clustered Index CLU_Accounting_LOB_Mapping_Status_LOB_Mapping_Id on Accounting_LOB_Mapping
(
Status desc,
LOB_Mapping_Id
)
go

Create Proc USP_Accounting_GetAll_LOB_Mapping_Details  
(  
@Client_Id bigint=null 
)  
as  
begin
select LM.LOB_Mapping_Id,LM.LOB_Id,Lobs.Name,Lobs.Code from Accounting_LOB_Mapping LM 
inner join Accounting_LU_LOBs Lobs ON Lobs.LOB_Id = LM.LOB_Id and Lobs.Status=1
where LM.Client_Id=@Client_Id and LM.Status=1
end
go

Create procedure USP_Accounting_InsertUpdateLOBMapping  
(  
@LOB_Mapping_Id int output,
@Client_Id bigint=null,
@LOB_Id tinyint=null,
@CreatedBy bigint=null
)  
as  
begin  
if exists(select 1 from Accounting_LOB_Mapping where LOB_Id = @LOB_Id and Client_Id = @Client_Id and status = 1)  
begin  
update Accounting_LOB_Mapping set UpdatedOn=GETDATE(),CreatedBy=@CreatedBy where LOB_Id = @LOB_Id and Client_Id = @Client_Id and status = 1  
end  
else  
begin  
insert into Accounting_LOB_Mapping  
(  
Client_Id,
LOB_Id,  
CreatedBy  
)  
values  
(  
@Client_Id,
@LOB_Id,  
@CreatedBy  
)  
set @LOB_Mapping_Id = SCOPE_IDENTITY()   
end  
end
go

CREATE procedure USP_Accounting_DeleteLOBMapping
(  
@LOB_Mapping_Id int =null  
)  
as  
begin  
update Accounting_LOB_Mapping set Status=0,UpdatedOn=GETDATE() where LOB_Mapping_Id=@LOB_Mapping_Id and Status=1; 
end
go


Create Proc USP_Accounting_GetAllClientLOBs
(  
@Client_Id bigint=null 
)  
as  
begin
select LM.LOB_Id,Lobs.Name,Lobs.Code from Accounting_LOB_Mapping LM 
inner join Accounting_LU_LOBs Lobs ON Lobs.LOB_Id = LM.LOB_Id and Lobs.Status=1
where LM.Client_Id=@Client_Id and LM.Status=1
end
go