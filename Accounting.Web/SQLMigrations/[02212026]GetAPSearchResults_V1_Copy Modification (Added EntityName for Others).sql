CREATE PROCEDURE [dbo].[USP_Accounting_GetAPSearchResults_V1_Copy]            
(            
    @EntityType_Id tinyint = NULL,            
    @Entity_Id bigint = NULL,            
    @LineItem_Id tinyint = NULL,            
    @PolicyNumber varchar(100) = NULL,            
    @AgencyName varchar(200) = NULL,            
    @InsuredName varchar(100) = NULL,            
    @OutstandingBalances bit = NULL,            
    @InvoiceNo varchar(100) = NULL,            
    @Client_Id bigint = NULL            
)            
AS            
BEGIN            
    SET NOCOUNT ON;            
            
    DECLARE @sql NVARCHAR(MAX);            
    DECLARE @params NVARCHAR(MAX);            
            
    -- Determine if this is "initial load" (only Client_Id and optionally OutstandingBalances)            
    IF @EntityType_Id IS NULL            
       AND @Entity_Id IS NULL            
       AND @LineItem_Id IS NULL            
       AND @PolicyNumber IS NULL            
       AND @AgencyName IS NULL            
       AND @InsuredName IS NULL            
       AND @InvoiceNo IS NULL            
    BEGIN            
        -- Initial load: limit to TOP 50 invoices            
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
     ORDER BY inv.Id DESC)            
        ';            
            
                 
        SET @sql += '            
        ,ARCollectedForPassThrough AS            
        (            
            SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id            
            FROM Accounting_InvoiceDetails ID            
            INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1            
            INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1            
            INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
            INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1            
            INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1            
            WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 2 AND ID.Status = 1            
            GROUP BY ID.Id, INV.Id            
        )            
        SELECT ID.*,            
               CASE            
                   WHEN ID.EntityType_Id = 1 THEN ag.Name            
                   WHEN ID.EntityType_Id = 2 THEN ins.Name            
                   WHEN ID.EntityType_Id = 3 THEN ca.Name            
                   WHEN ID.EntityType_Id = 6 THEN wsd.Name
				   WHEN ID.EntityType_Id = 11 THEN AOED.Name
                   ELSE NULL            
               END AS EntityName,            
               ET.Name AS EntityTypeName,            
               CASE            
                   WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected            
               END AS ARCollected            
        FROM Accounting_InvoiceDetails ID            
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1            
        INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1            
        LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id = PD.AgentDetail_Id AND agd.Status = 1            
        LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id = PD.InsuredDetail_Id AND isd.Status = 1            
      LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1            
        LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1            
        LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1            
        LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1
		LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = ID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1  
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1            
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1            
        LEFT JOIN Accounting_LU_EntityTypes ET ON ID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1            
        LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = INV.Id            
        WHERE ID.Status = 1            
          AND PD.Client_Id = @Client_Id            
          AND LD.Direction_Id = 2            
          AND INV.Id IN (SELECT Id FROM LimitedInvoices)            
    AND inv.RemainingBalance <> 0            
        ';            
    END            
    ELSE            
    BEGIN            
        -- Filtered search: no TOP 50, apply all filters            
        SET @sql = N';with            
  ARCollectedForPassThrough AS            
        (            
            SELECT ID.Id, SUM(PALI.AppliedAmount) AS ArCollected, INV.Id AS Invoice_Id            
            FROM Accounting_InvoiceDetails ID            
            INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1            
            INNER JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1            
            INNER JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
            INNER JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1            
            INNER JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1            
            WHERE LIC.Is_Passthrough = 1 AND LD.Direction_Id = 2 AND ID.Status = 1            
            GROUP BY ID.Id, INV.Id            
        )            
        SELECT ID.*,            
               CASE            
                   WHEN ID.EntityType_Id = 1 THEN ag.Name            
                   WHEN ID.EntityType_Id = 2 THEN ins.Name            
                   WHEN ID.EntityType_Id = 3 THEN ca.Name            
                   WHEN ID.EntityType_Id = 6 THEN wsd.Name
				   WHEN ID.EntityType_Id = 11 THEN AOED.Name
                   ELSE NULL            
               END AS EntityName,            
               ET.Name AS EntityTypeName,            
               CASE            
                   WHEN LIC.Is_Passthrough = 1 THEN ARPT.ArCollected            
               END AS ARCollected            
        FROM Accounting_InvoiceDetails ID            
        INNER JOIN Accounting_Invoices INV ON ID.Invoice_Id = INV.Id AND INV.Status = 1            
        INNER JOIN Accounting_PolicyDetails PD ON INV.PolicyDetails_Id = PD.Id AND PD.Status = 1            
        LEFT JOIN Accounting_AgentDetails agd ON agd.AgentDetail_Id = PD.AgentDetail_Id AND agd.Status = 1            
        LEFT JOIN Accounting_InsuredDetails isd ON isd.InsuredDetail_Id = PD.InsuredDetail_Id AND isd.Status = 1            
        LEFT JOIN Accounting_AgentDetails ag ON ag.AgentDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 1 AND ag.Status = 1            
        LEFT JOIN Accounting_CarrierDetails ca ON ca.CarrierDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 3 AND ca.Status = 1            
        LEFT JOIN Accounting_InsuredDetails ins ON ins.InsuredDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 2 AND ins.Status = 1            
        LEFT JOIN Accounting_WholesalerDetails wsd ON wsd.WholesalerDetail_Id = ID.Entity_Id AND ID.EntityType_Id = 6 AND wsd.Status = 1
		LEFT JOIN Accounting_OtherEntityDetails AOED ON AOED.Id = ID.Entity_Id and AOED.EntityType_Id=11  and AOED.Status=1  
        LEFT JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON ID.BillingType_LineItems_Directions_Id = BLD.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
        LEFT JOIN Accounting_LU_LineItems_Directions LD ON BLD.LineItems_Directions_Id = LD.LineItems_Directions_Id AND LD.Status = 1            
        LEFT JOIN Accounting_LU_LineItems_Clients LIC ON LIC.LineItems_Clients_Id = LD.LineItems_Clients_Id AND LIC.Status = 1            
        LEFT JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LIC.LineItem_Id AND LI.Status = 1            
        LEFT JOIN Accounting_LU_EntityTypes ET ON ID.EntityType_Id = ET.EntityType_Id AND ET.Status = 1            
        LEFT JOIN ARCollectedForPassThrough ARPT ON ARPT.Invoice_Id = INV.Id            
        WHERE ID.Status = 1            
          AND PD.Client_Id = @Client_Id            
          AND LD.Direction_Id = 2            
        ';            
            
        -- Dynamic filters            
        IF @EntityType_Id IS NOT NULL            
            SET @sql += ' AND ID.EntityType_Id = @EntityType_Id';            
        IF @Entity_Id IS NOT NULL            
            SET @sql += ' AND ID.Entity_Id = @Entity_Id';            
        IF @LineItem_Id IS NOT NULL            
            SET @sql += ' AND LIC.LineItem_Id = @LineItem_Id';            
        IF @PolicyNumber IS NOT NULL            
            SET @sql += ' AND PD.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%''';            
        IF @AgencyName IS NOT NULL            
            SET @sql += ' AND agd.Name LIKE ''%'' + @AgencyName + ''%''';            
        IF @InsuredName IS NOT NULL            
            SET @sql += ' AND isd.Name LIKE ''%'' + @InsuredName + ''%''';            
        IF @InvoiceNo IS NOT NULL            
            SET @sql += ' AND INV.InvoiceNo LIKE ''%'' + @InvoiceNo + ''%''';            
        IF @OutstandingBalances = 1            
            SET @sql += ' AND ID.RemainingBalance <> 0';            
    END            
    -- Define parameters            
    SET @params = N'@Client_Id bigint,            
                    @EntityType_Id tinyint,            
                    @Entity_Id bigint,            
                    @LineItem_Id tinyint,            
                    @PolicyNumber varchar(100),            
                    @AgencyName varchar(200),            
                    @InsuredName varchar(100),            
                    @InvoiceNo varchar(100),            
                    @OutstandingBalances bit';            
   print    @sql      
    -- Execute dynamic SQL            
    EXEC sp_executesql @sql, @params,            
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