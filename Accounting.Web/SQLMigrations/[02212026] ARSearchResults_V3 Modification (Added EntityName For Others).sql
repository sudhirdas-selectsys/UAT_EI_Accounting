alter PROCEDURE [dbo].[USP_Accounting_GetARSearchResults_V3]              
(              
    @EntityType_Id TINYINT = NULL,              
    @Entity_Id BIGINT = NULL,              
    @LineItem_Id TINYINT = NULL,              
    @PolicyNumber VARCHAR(100) = NULL,              
    @AgencyName VARCHAR(200) = NULL,              
    @InsuredName VARCHAR(100) = NULL,              
    @OutstandingBalances BIT = NULL,              
    @InvoiceNo VARCHAR(100) = NULL,              
    @Client_Id BIGINT = NULL              
)              
AS              
BEGIN              
    SET NOCOUNT ON;              
              
    DECLARE @sql NVARCHAR(MAX) = N'';               
    DECLARE @params NVARCHAR(MAX) = N'';              
           
          
 DECLARE @condition BIT = CASE           
        WHEN @EntityType_Id IS NULL           
         AND @Entity_Id IS NULL           
         AND @LineItem_Id IS NULL           
         AND @PolicyNumber IS NULL           
         AND @AgencyName IS NULL           
         AND @InsuredName IS NULL           
         AND @InvoiceNo IS NULL           
        THEN '1' ELSE '0' END;          
              
    -- Check if this is "initial load" (only @Client_Id and optionally @OutstandingBalances)              
      SET @sql = N'          
    ;WITH LimitedInvoices AS          
    (          
        SELECT TOP (50) inv.Id          
        FROM Accounting_Invoices inv          
        INNER JOIN Accounting_PolicyDetails pd          
            ON inv.PolicyDetails_Id = pd.Id          
           AND pd.Status = 1          
        WHERE inv.Status = 1          
          AND pd.Client_Id = @Client_Id       
    ORDER BY inv.Id DESC    
              
    )              
                      
        SELECT id.Id,id.Invoice_Id,id.Amount,id.RemainingBalance,id.Entity_Id,id.EntityType_Id,id.BillingType_LineItems_Directions_Id,id.PaidOn,id.Paid,id.LOB_Id,id.Status,id.CreatedOn,id.UpdatedOn,          
               CASE id.EntityType_Id             
                   WHEN 1 THEN ag.Name              
                   WHEN 2 THEN ins.Name              
                   WHEN 3 THEN ca.Name              
                   WHEN 6 THEN wsd.Name  
       when 7 THEN Concat(''State of '',pd.RiskState)
	   WHEN 11 THEN AOED.Name
                   ELSE NULL              
               END AS EntityName,              
               et.Name AS EntityTypeName,              
               ISNULL(pps.DueDate, inv.CreatedOn) AS InvoiceDueDate,          
     LIM.Name AS LineItemName          
        FROM Accounting_InvoiceDetails id              
        INNER JOIN Accounting_Invoices inv              
            ON id.Invoice_Id = inv.Id              
            AND inv.Status = 1              
        INNER JOIN Accounting_PolicyDetails pd              
            ON inv.PolicyDetails_Id = pd.Id              
            AND pd.Status = 1                
        INNER JOIN Accounting_AgentDetails agd              
            ON agd.AgentDetail_Id = pd.AgentDetail_Id              
            AND agd.Status = 1               
        INNER JOIN Accounting_InsuredDetails isd              
            ON isd.InsuredDetail_Id = pd.InsuredDetail_Id              
            AND isd.Status = 1                
        INNER JOIN Accounting_LU_BillingType_LineItems_Directions bld              
            ON id.BillingType_LineItems_Directions_Id = bld.BillingType_LineItems_Directions_Id              
            AND bld.Status = 1              
        INNER JOIN Accounting_LU_LineItems_Directions ld              
            ON bld.LineItems_Directions_Id = ld.LineItems_Directions_Id              
            AND ld.Status = 1              
        INNER JOIN Accounting_LU_LineItems_Clients lic              
            ON lic.LineItems_Clients_Id = ld.LineItems_Clients_Id              
   AND lic.Status = 1              
  INNER JOIN Accounting_LU_LineItems LIM           
   ON LIM.LineItem_Id = Lic.LineItem_Id          
   AND LIM.Status = 1          
        INNER JOIN Accounting_LU_EntityTypes et              
            ON id.EntityType_Id = et.EntityType_Id              
            AND et.Status = 1              
                    
        LEFT JOIN Accounting_PolicyPaymentSchedule pps              
            ON inv.Schedule_Id = pps.PolicyPaymentSchedule_Id           
        LEFT JOIN Accounting_AgentDetails ag              
            ON ag.AgentDetail_Id = id.Entity_Id              
            AND id.EntityType_Id = 1              
            AND ag.Status = 1              
        LEFT JOIN Accounting_CarrierDetails ca              
            ON ca.CarrierDetail_Id = id.Entity_Id              
            AND id.EntityType_Id = 3              
            AND ca.Status = 1              
        LEFT JOIN Accounting_InsuredDetails ins              
            ON ins.InsuredDetail_Id = id.Entity_Id              
            AND id.EntityType_Id = 2              
            AND ins.Status = 1              
        LEFT JOIN Accounting_WholesalerDetails wsd              
            ON wsd.WholesalerDetail_Id = id.Entity_Id              
            AND id.EntityType_Id = 6              
            AND wsd.Status = 1
			LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = id.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1'     
   SET @sql +=  N' '+ CASE WHEN @condition = 1 THEN ' INNER JOIN LimitedInvoices li ON li.Id = inv.Id  ' ELSE '' END + ''    
   set @sql +=N' ' +    
        'WHERE id.Status = 1             
          AND pd.Client_Id = @Client_Id              
          AND ld.Direction_Id = 1   '          
          
              
        -- Add dynamic filters only if parameters are provided              
        IF @EntityType_Id IS NOT NULL              
            SET @sql += ' AND id.EntityType_Id = @EntityType_Id';              
        IF @Entity_Id IS NOT NULL              
            SET @sql += ' AND id.Entity_Id = @Entity_Id';              
        IF @LineItem_Id IS NOT NULL              
            SET @sql += ' AND lic.LineItem_Id = @LineItem_Id';              
        IF @PolicyNumber IS NOT NULL              
            SET @sql += ' AND pd.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';              
        IF @AgencyName IS NOT NULL              
            SET @sql += ' AND agd.Name LIKE ''%'' + @AgencyName + ''%''';              
        IF @InsuredName IS NOT NULL              
            SET @sql += ' AND isd.Name LIKE ''%'' + @InsuredName + ''%''';              
        IF @InvoiceNo IS NOT NULL              
            SET @sql += ' AND inv.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';              
        IF @OutstandingBalances = 1              
            SET @sql += ' AND inv.RemainingBalance <> 0';              
              
    -- Define parameters for sp_executesql              
    SET @params = N'@Client_Id BIGINT,              
                    @EntityType_Id TINYINT,              
                    @Entity_Id BIGINT,              
                    @LineItem_Id TINYINT,              
                    @PolicyNumber VARCHAR(100),              
                    @AgencyName VARCHAR(200),              
                    @InsuredName VARCHAR(100),              
     @InvoiceNo VARCHAR(100),              
                    @OutstandingBalances BIT';              
    print @sql          
    -- Execute the query              
    EXEC SP_EXECUTESQL @sql, @params,              
        @Client_Id = @Client_Id,              
        @EntityType_Id = @EntityType_Id,              
        @Entity_Id = @Entity_Id,              
        @LineItem_Id = @LineItem_Id,              
        @PolicyNumber = @PolicyNumber,              
        @AgencyName = @AgencyName,              
        @InsuredName = @InsuredName,              
        @InvoiceNo = @InvoiceNo,              
        @OutstandingBalances = @OutstandingBalances;              
END 