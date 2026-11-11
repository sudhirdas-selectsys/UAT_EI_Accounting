

select * from Accounting_LU_EntityTypes
select * from  Accounting_SalesRepDetails

USP_Acconting_InsertUpdateSalesRepDetails

ALter PROC [dbo].[USP_Accounting_GetSalesRepDetails]            
(                    
    @SalesRep VARCHAR(200) = NULL,  
	@Client_Id BIGINT =NULL  
)            
AS            
BEGIN            
    SET NOCOUNT ON;            
   
   Select top 1 * from Accounting_SalesRepDetails where Client_Id=@Client_Id and [Name]=@SalesRep;
END   

USP_Accounting_GetDefaultEntityByInvoiceIdAndEntityTypeId
@Client_Id=10007,
@Invoice_Id=26193,
@EntityType_Id=10



Alter PROCEDURE USP_Accounting_GetDefaultEntityByInvoiceIdAndEntityTypeId  
(  
    @Client_Id BIGINT,  
    @Invoice_Id     BIGINT,  
    @EntityType_Id  TINYINT  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    SELECT  
        Entity_Id =  
            CASE  
                WHEN @EntityType_Id = 1  THEN PD.AgentDetail_Id  
                WHEN @EntityType_Id = 2  THEN PD.InsuredDetail_Id  
                WHEN @EntityType_Id = 3  THEN PD.CarrierDetail_Id  
                WHEN @EntityType_Id = 4  THEN PD.Client_Id  
				WHEN @EntityType_Id = 5  THEN PD.FinanceDetail_Id  
                WHEN @EntityType_Id = 6  THEN PD.WholesalerDetail_Id      
                WHEN @EntityType_Id = 8  THEN PD.WholesalerDetail_Id  
				WHEN @EntityType_Id = 9  THEN PD.CarrierDetail_Id   
				WHEN @EntityType_Id = 10 THEN ag.AssignedSalesRep_Id
                ELSE NULL  
            END,  
  
        EntityName =  
            CASE  
                WHEN @EntityType_Id = 1  THEN ag.Name  
                WHEN @EntityType_Id = 2  THEN ins.Name  
                WHEN @EntityType_Id = 3  THEN ca.Name  
                WHEN @EntityType_Id = 4  THEN cl.CompanyName  
				WHEN @EntityType_Id = 5  THEN fd.Name  
                WHEN @EntityType_Id = 6  THEN wsd.Name  
                WHEN @EntityType_Id = 7  THEN CONCAT('State of ', PD.RiskState)  
                WHEN @EntityType_Id = 8  THEN wsd.Name  
				 WHEN @EntityType_Id = 9  THEN ca.Name  
				 WHEN @EntityType_Id = 10  THEN srd.Name 
                ELSE NULL  
            END  
  
    FROM Accounting_Invoices INV   
		INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1  
        LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = PD.AgentDetail_Id AND ag.Status = 1  
		LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = PD.InsuredDetail_Id AND ins.Status = 1  
        LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = PD.CarrierDetail_Id AND ca.Status = 1  
        LEFT JOIN Accounting_Clients cl ON cl.Client_Id = PD.Client_Id AND cl.Status = 1  
		LEFT JOIN Accounting_FinanceDetails fd on fd.FinanceDetail_Id = PD.FinanceDetail_Id AND fd.Status=1  
        LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = PD.WholesalerDetail_Id AND wsd.Status = 1  
        LEFT JOIN Accounting_SalesrepDetails srd ON srd.Id = ag.AssignedSalesRep_Id AND srd.Status = 1     
        WHERE INV.Id = @Invoice_Id AND INV.Status = 1 AND PD.Client_Id = @Client_Id  
END
GO

Select * from Accounting_LU_EntityTypes
select * from Accounting_SalesrepDetails
select * from Accounting_AgentDetails where AssignedSalesRep_Id in (13,14)

USP_Accounting_GetSalesRepDetails

CREATE PROC [dbo].[USP_Accounting_GetSalesRepDetails]              
(                      
    @SalesRep VARCHAR(200) = NULL,    
 @Client_Id BIGINT =NULL    
)              
AS              
BEGIN              
    SET NOCOUNT ON;              
     
   Select top 1 * from Accounting_SalesRepDetails where Client_Id=@Client_Id and [Name]=@SalesRep;  
END 


select * from Accounting_Invoices where InvoiceNo='INV002872'