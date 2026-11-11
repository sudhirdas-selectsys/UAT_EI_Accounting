alter PROCEDURE USP_Accounting_GetDefaultEntityByInvoiceIdAndEntityTypeId    
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
                WHEN @EntityType_Id = 8  THEN
                CASE 
                    WHEN PD.WholesalerDetail_Id IS NOT NULL 
                        THEN PD.WholesalerDetail_Id
                    ELSE PD.CarrierDetail_Id
                END
                WHEN @EntityType_Id = 9  THEN PD.CarrierDetail_Id     
                WHEN @EntityType_Id = 10  THEN ag.AssignedSalesRep_Id  
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
                WHEN @EntityType_Id = 8 
           THEN CASE 
              WHEN wsd.Name IS NOT NULL THEN wsd.Name
              ELSE ca.Name
          END    
                WHEN @EntityType_Id = 9  THEN ca.Name    
                WHEN @EntityType_Id = 10  THEN srd.Name   
                ELSE NULL    
            END
			,PayorType_Id = 
			 CASE    
                WHEN @EntityType_Id = 1  THEN CAST(1 AS TINYINT)  
                WHEN @EntityType_Id = 2  THEN CAST(2 AS TINYINT)  
                WHEN @EntityType_Id = 3  THEN CAST(3 AS TINYINT)
                WHEN @EntityType_Id = 4  THEN CAST(4 AS TINYINT) 
                WHEN @EntityType_Id = 5  THEN CAST(5 AS TINYINT)  
                WHEN @EntityType_Id = 6  THEN CAST(6 AS TINYINT)    
                WHEN @EntityType_Id = 7  THEN CAST(7 AS TINYINT)  
                  WHEN @EntityType_Id = 8 THEN CAST(
        CASE 
            WHEN wsd.Name IS NOT NULL THEN 6
            ELSE 8
        END AS TINYINT
    )
				WHEN @EntityType_Id = 9  THEN CAST(3 AS TINYINT)
                WHEN @EntityType_Id = 10 THEN CAST(10 AS TINYINT)
                ELSE NULL    
            END
    FROM Accounting_Invoices INV
	--LEFT JOIN Accounting_InvoiceDetails ID ON ID.EntityType_Id = @EntityType_Id AND ID.Invoice_Id = INV.Id AND ID.Status=1
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


select * from Accounting_Clients


USP_Acconting_InsertUpdateSalesRepDetails
CREATE PROC [dbo].[USP_Acconting_InsertUpdateSalesRepDetails]              
(              
    @SalesRepDetails_Id BIGINT OUTPUT,                       
    @Name VARCHAR(200) = NULL,  
 @EntityType_Id TINYINT = NULL,  
 @Client_Id BIGINT =NULL    
)              
AS              
BEGIN              
    SET NOCOUNT ON;              
              
    DECLARE @ExistingSalesRepDetails_Id BIGINT;              
              
    SELECT @ExistingSalesRepDetails_Id = Id              
    FROM Accounting_SalesRepDetails              
    WHERE Id = @SalesRepDetails_Id              
      AND Client_Id = @Client_Id      
      AND Status = 1;              
              
    IF @ExistingSalesRepDetails_Id IS NOT NULL              
    BEGIN              
        UPDATE Accounting_SalesRepDetails              
        SET              
            Name = @Name,  
   EntityType_Id =@EntityType_Id,  
            UpdatedOn = GETDATE()              
        WHERE Id = @ExistingSalesRepDetails_Id;              
              
        SET @SalesRepDetails_Id = @ExistingSalesRepDetails_Id;              
    END              
    ELSE              
    BEGIN              
        INSERT INTO Accounting_SalesRepDetails              
        (           Name, Client_Id,EntityType_Id         
        )              
        VALUES              
        (          @Name, @Client_Id,@EntityType_Id          
        );              
              
        SET @SalesRepDetails_Id = SCOPE_IDENTITY();              
    END              
END