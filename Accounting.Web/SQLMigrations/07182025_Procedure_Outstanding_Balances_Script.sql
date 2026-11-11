Alter Proc [dbo].[USP_Accounting_GetARSearchResults_V1]
(                
@EntityType_Id tinyint = null,                
@Entity_Id bigint = null,                
@LineItem_Id tinyint = null,            
@PolicyNumber varchar(100) = null,            
@AgencyName varchar(200) = null,            
@InsuredName varchar(100) = null,    
@OutstandingBalances bit = null,   
@Client_Id bigint = null  
)                
As                
Begin                
select  ID.*,                
CASE                   
        WHEN ID.EntityType_Id = 1 THEN ag.Name                  
        WHEN ID.EntityType_Id = 2 THEN Ins.Name                  
        WHEN ID.EntityType_Id = 3 THEN ca.Name
		WHEN ID.EntityType_Id = 6 THEN wsd.Name 
        ELSE NULL                  
    END AS EntityName,                
 ET.Name as EntityTypeName                
from Accounting_InvoiceDetails ID              
LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id=INV.Id                        
INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id=PD.Id         
LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id=pd.AgentDetail_Id AND agd.Status = 1          
LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id=pd.InsuredDetail_Id AND isd.Status = 1        
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1                  
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1                  
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1 
LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1
left join Accounting_LU_BillingType_LineItems_Directions BLD on ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id and BLD.Status = 1                
left join Accounting_LU_LineItems_Directions LD on BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id and LD.Status = 1                
left join Accounting_LU_EntityTypes ET on ID.EntityType_Id = ET.EntityType_Id and ET.Status = 1                
WHERE                                           
(@EntityType_Id IS NULL OR ID.EntityType_Id = @EntityType_Id)                                          
AND                                          
(@Entity_Id IS NULL OR (Id.Entity_Id = @Entity_Id))                
And                
(@LineItem_Id is null or (LD.LineItem_Id = @LineItem_Id))             
And                
(@PolicyNumber is null or (PD.PolicyNumber like '%' + @PolicyNumber + '%'))              
And                
(@AgencyName is null or ((agd.Name like '%' + @AgencyName + '%')))              
And                
(@InsuredName is null or (isd.Name like '%' + @InsuredName + '%'))       
And     
(@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR (@OutstandingBalances = 1 AND INV.RemainingBalance != 0 AND (ID.RemainingBalance != 0 OR ID.RemainingBalance = 0)))
And     
LD.Direction_Id = 1 and ID.Status = 1 and PD.Client_Id = @Client_Id               
End
go


Alter Proc [dbo].[USP_Accounting_GetAPSearchResults]
(              
@EntityType_Id tinyint = null,              
@Entity_Id bigint = null,              
@LineItem_Id tinyint = null,          
@PolicyNumber varchar(100) = null,          
@AgencyName varchar(200) = null,          
@InsuredName varchar(100) = null,    
@OutstandingBalances bit = null,  
@Client_Id bigint = null  
)              
As              
Begin              
select  ID.*,              
CASE                 
        WHEN ID.EntityType_Id = 1 THEN ag.Name                
        WHEN ID.EntityType_Id = 2 THEN Ins.Name                
        WHEN ID.EntityType_Id = 3 THEN ca.Name
		WHEN ID.EntityType_Id = 6 THEN wsd.Name   
        ELSE NULL                
    END AS EntityName,              
 ET.Name as EntityTypeName              
from Accounting_InvoiceDetails ID            
LEFT JOIN Accounting_Invoices INV ON ID.Invoice_Id=INV.Id                      
INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id=PD.Id       
LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id=pd.AgentDetail_Id AND agd.Status = 1        
LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id=pd.InsuredDetail_Id AND isd.Status = 1      
LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1                
LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1                
LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1 
LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1
left join Accounting_LU_BillingType_LineItems_Directions BLD on ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id and BLD.Status = 1              
left join Accounting_LU_LineItems_Directions LD on BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id and LD.Status = 1              
left join Accounting_LU_EntityTypes ET on ID.EntityType_Id = ET.EntityType_Id and ET.Status = 1                
WHERE                                         
(@EntityType_Id IS NULL OR ID.EntityType_Id = @EntityType_Id)                                        
AND                                        
(@Entity_Id IS NULL OR (Id.Entity_Id = @Entity_Id))              
And              
(@LineItem_Id is null or (LD.LineItem_Id = @LineItem_Id))           
And              
(@PolicyNumber is null or (PD.PolicyNumber like '%' + @PolicyNumber + '%'))            
And              
(@AgencyName is null or ((agd.Name like '%' + @AgencyName + '%')))            
And              
(@InsuredName is null or (isd.Name like '%' + @InsuredName + '%'))    
And     
(@OutstandingBalances IS NULL OR @OutstandingBalances = 0 OR  (@OutstandingBalances = 1 AND ID.RemainingBalance != 0))
And    
LD.Direction_Id = 2 and PD.Client_Id = @Client_Id  and ID.Status = 1              
End
go


CREATE PROCEDURE USP_Accounting_GetAllLineofBusiness  
(  
@PageNumber INT=1,  
@PageSize INT=10,  
@SearchValue VARCHAR(100)=NULL,
@TotalRecords BIGINT OUT  
)    
AS    
BEGIN    
 IF(@SearchValue IS NOT NULL AND @SearchValue<>'')    
  BEGIN    
   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_LOBs WHERE Status=1 AND Name LIKE '%' +@SearchValue+ '%' OR Code LIKE '%' +@SearchValue+ '%';    
   SELECT * FROM Accounting_LU_LOBs WHERE Status=1 AND Name LIKE '%' +@SearchValue+ '%' OR Code LIKE '%' +@SearchValue+ '%'   
   ORDER BY LOB_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;    
  END    
    
 ELSE    
  BEGIN    
   SELECT @TotalRecords=COUNT(*) FROM Accounting_LU_LOBs  WHERE Status=1;    
   SELECT * FROM Accounting_LU_LOBs WHERE Status=1    
   ORDER BY LOB_Id DESC OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY;    
  END    
END
go


CREATE PROCEDURE USP_Accounting_GetLineofBusiness 
(  
@LOB_Id tinyint  
)    
AS    
BEGIN    
SELECT * FROM Accounting_LU_LOBs WHERE LOB_Id = @LOB_Id AND Status = 1;    
END
go

CREATE PROCEDURE USP_Accounting_InsertUpdateLineofBusiness   
(  
@LOB_Id TINYINT OUTPUT,  
@Name VARCHAR(500)=NULL,  
@Code VARCHAR(500)=NULL
)      
AS      
BEGIN      
 IF EXISTS (SELECT 1 FROM Accounting_LU_LOBs WHERE LOB_Id = @LOB_Id AND Status=1)      
 BEGIN      
 UPDATE Accounting_LU_LOBs SET Name=@Name,Code=@Code,UpdatedOn=GETDATE() WHERE LOB_Id=@LOB_Id AND Status=1;      
 END  
 ELSE  
 BEGIN    
  INSERT INTO Accounting_LU_LOBs(Name,Code) VALUES (@Name,@Code);      
  SET @LOB_Id=SCOPE_IDENTITY();    
 END  
END
go


CREATE PROCEDURE USP_Accounting_DeleteLineofBusiness(@LOB_Id TINYINT)    
AS    
BEGIN    
UPDATE Accounting_LU_LOBs SET Status = 0, UpdatedOn = GETDATE() WHERE LOB_Id = @LOB_Id AND Status = 1;    
END
go