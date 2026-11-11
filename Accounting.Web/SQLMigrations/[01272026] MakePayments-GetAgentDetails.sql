CREATE PROC [dbo].[USP_Accounting_GetEntityDetails]           
(    
    @EntityType_Id TINYINT,          
    @Entity_Id BIGINT,     
    @Client_Id BIGINT = NULL    
)          
AS          
BEGIN    
          
    SELECT    
        CASE @EntityType_Id    
            WHEN 1 THEN ag.Name    
            WHEN 2 THEN ins.Name    
            WHEN 3 THEN ca.Name    
            WHEN 6 THEN wh.Name  
            ELSE NULL    
        END AS EntityName,    
    
        CASE @EntityType_Id    
            WHEN 1 THEN ag.Address1    
            WHEN 2 THEN ins.Address1    
            WHEN 3 THEN ca.Address1    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityAddress,    
    
        CASE @EntityType_Id    
            WHEN 1 THEN ag.Email    
            WHEN 2 THEN ''    
            WHEN 3 THEN ''    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityEmail,    
    
  CASE @EntityType_Id    
            WHEN 1 THEN ag.City    
            WHEN 2 THEN ins.City    
            WHEN 3 THEN ca.City    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityCity,    
      
  CASE @EntityType_Id    
            WHEN 1 THEN ag.State    
            WHEN 2 THEN ins.State    
            WHEN 3 THEN ca.State    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityState,    
    
  CASE @EntityType_Id    
            WHEN 1 THEN ag.ZipCode    
            WHEN 2 THEN ins.ZipCode    
            WHEN 3 THEN ca.ZipCode    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityZipCode,    
  CASE @EntityType_Id    
            WHEN 1 THEN ag.City    
            WHEN 2 THEN ins.City    
            WHEN 3 THEN ca.City    
            WHEN 6 THEN ''  
            ELSE NULL    
        END AS EntityCity,    
        ET.Name AS EntityTypeName    
           
    FROM Accounting_LU_EntityTypes ET    
    LEFT JOIN Accounting_AgentDetails ag     
        ON @EntityType_Id = 1     
       AND ag.AgentDetail_Id = @Entity_Id     
       AND ag.Status = 1    
          
    LEFT JOIN Accounting_InsuredDetails ins     
        ON @EntityType_Id = 2     
       AND ins.InsuredDetail_Id = @Entity_Id     
       AND ins.Status = 1          
    
    LEFT JOIN Accounting_CarrierDetails ca     
        ON @EntityType_Id = 3     
       AND ca.CarrierDetail_Id = @Entity_Id     
       AND ca.Status = 1          
    
    LEFT JOIN Accounting_WholesalerDetails wh     
        ON @EntityType_Id = 6     
       AND wh.WholesalerDetail_Id = @Entity_Id     
       AND wh.Status = 1   
           
    WHERE ET.EntityType_Id = @EntityType_Id    
      AND ET.Status = 1;    
END  
  
  