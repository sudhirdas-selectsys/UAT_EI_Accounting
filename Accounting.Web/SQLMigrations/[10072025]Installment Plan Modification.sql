 alter proc [dbo].[USP_Accounting_GetAllPaymentPlanDetails]          
(          
@Client_Id BIGINT =null,    
@PageNumber INT = 1,            
@PageSize INT = 15,            
@SearchValue varchar(100) = null,        
@TotalRecords BIGINT OUT          
)          
As          
begin            
        
if(@SearchValue is not null and @SearchValue<>'')                                    
begin        
SELECT @TotalRecords = COUNT(*)FROM Accounting_LU_PaymentPlans WHERE status = 1 and Name like '%' + @SearchValue + '%';            
SELECT ap.*, ac.name as 'CarrierName' FROM Accounting_LU_PaymentPlans ap inner join Accounting_CarrierDetails ac on ap.Carrier_Id=ac.CarrierDetail_Id     
WHERE ap.status = 1 and ap.Client_Id =@Client_Id and ap.Name like '%' + @SearchValue + '%'  ORDER BY SequenceNo ASC OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;          
end        
else        
begin        
SELECT @TotalRecords = COUNT(*)FROM Accounting_LU_PaymentPlans WHERE status = 1;            
SELECT ap.*, ac.name as 'CarrierName' FROM Accounting_LU_PaymentPlans ap inner join Accounting_CarrierDetails ac on ap.Carrier_Id=ac.CarrierDetail_Id        
WHERE ap.status = 1 and ap.Client_Id =@Client_Id ORDER BY SequenceNo ASC OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;          
end        
        
        
          
end				


