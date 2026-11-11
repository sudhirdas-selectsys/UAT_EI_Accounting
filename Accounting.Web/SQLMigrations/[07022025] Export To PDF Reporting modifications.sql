update Accounting_LU_FileTypes set Status = 1 where FileType_Id = 3
go
update Accounting_LU_ReportTemplates set Template = '<!DOCTYPE html>  <html lang="en">  <head>      <meta charset="UTF-8" />   {{fonts}}           <style>          body, td, table, h1, h2 {              font-family: "Poppins", sans-serif;          }          .agency-billed-table { width: 100%; }          .fw-semibold { font-weight: 600; }          .fw-medium { font-weight: 500; }          .fs-13 { font-size: 13px; }          .fs-12 { font-size: 12px; }          .lh-20 { line-height: 20px; }          .lh-18 { line-height: 18px; }          .bg-dark { background-color: #063756; }          .ac-header {              background-color: #063756;              font-size: 13px;              color: #fff;              font-weight: 500;              padding: 5px 0;              line-height: 20px;              border: 1px dotted white;              box-sizing: border-box;          }          .due-date-alert {              border: 1px dotted #FF0000;              color: #FF0000;              font-weight: 600;              text-transform: uppercase;              padding: 4px 0;          }          .agency-billed-table .t-head td { font-weight: 600; }          .td-bg-light { background-color: #F0F0F0; }          .mb-0 { margin-bottom: 0px; }          .mt-0 { margin-top: 0px; }          .px-8 { padding-left: 8px; padding-right: 8px; }          .border-top-1 { border-top: 1px solid #CCCCCC; }          .border-right-1 { border-right: 1px solid #CCCCCC; }          .border-1 { border: 1px solid #ccc; }          .border-right-0 { border-right: 1px solid #ccc; }          .pt-0 { padding-top: 0 !important; }          .pd-table td { padding: 12px 8px 12px 8px; color: #000; }          .ps-35 { padding-left: 30px; }          .fw-400 { font-weight: 400; }          .border-top-dark { border-top: 1px solid #000000; }          .text-dark { color: #000; }          .ps-50 { padding-left: 50px; }          .sepearator {              border-bottom: 3px solid #063756;              margin: 0;              width: 100%;          }          .pt-8 { padding-top: 12px; }          .pb-8 { padding-bottom: 12px; }          .page-break {              page-break-after: always;              break-after: page;          }          .fs-14 { font-size: 14px; }          .pb-0 { padding-bottom: 0; }      </style>  </head>  <body>      <table width="100%" cellpadding="0" cellspacing="0" border="0">          <tr>              <td style="padding: 20px 0; font-family: ''Poppins'', sans-serif; color: #333; font-size: 15px; line-height: 23px;">                  <table border="0" cellspacing="0" cellpadding="0" width="1050" align="center" style="border-color:rgba(0,0,0,.09);">                      <tr>                          <td>                              <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="20" class="p-0"></td></tr>                              </table>                                <table class="agency-billed-table">                                  <tr>                                      <td>{{clientLogo}}</td>                                      <td align="right" class="fs-13 lh-20">                                          <span class="fw-semibold">{{clientCompanyName}}</span><br />                                          {{clientAddress}},<br />                                          {{clientCityStateZip}}<br />                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="20" class="p-0"></td></tr>                              </table>                                <table class="agency-billed-table">                                  <tr>                                      <td align="center">                                          <div class="bg-dark ac-header">Account Statement</div>                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="20" class="p-0"></td></tr>                              </table>                                {{dynamicAgentloopingLogic}}                                <div class="page-break"></div>                                <table class="agency-billed-table" cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr>                                      <td align="center">                                          <div class="due-date-alert">Audits are due upon Receipt</div>                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="10" class="p-0"></td></tr>                              </table>                                <table class="agency-billed-table fs-12 lh-18 pd-table" cellpadding="0" border="0" cellspacing="0">                                  <tbody>                                      <tr>                                          <td rowspan="2" style="width:200px;">                                              <div><span class="fw-semibold fs-14">Account Total</span></div>                                          </td>                                          <td colspan="6"></td>                                          <td class="fw-semibold fs-14" align="right">NET DUE</td>                                          <td></td>                                      </tr>                                      <tr>                                          <td class="fw-400" colspan="6"></td>                                          <td class="fw-semibold fs-14 pt-0 pb-0" align="right">                                              <div class="pt-8 pb-8" style="border-top:4px solid #000; border-bottom:4px solid #000; width:100px;">                                                  {{TotalNetDue}}                                              </div>                                          </td>                                          <td align="right"></td>                                      </tr>                                  </tbody>                              </table>                                <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="20" class="p-0"></td></tr>                              </table>                                <table class="agency-billed-table" cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr>                                      <td align="center">                                          <div class="due-date-alert">                                              ITEMS NOT PAID BY DUE DATE MAY BE CANCELLED BY THE CARRIER WITHOUT FURTHER WARNING                                          </div>                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" border="0" cellspacing="0" width="100%">                                  <tr><td height="10" class="p-0"></td></tr>                              </table>                          </td>                      </tr>                  </table>              </td>          </tr>      </table>  </body>  </html>  ' where ReportTemplate_Id = 1 and Status = 1
go
update Accounting_LU_ReportTemplates set Template = '<!DOCTYPE html>  <html lang="en">  <head>      <meta charset="UTF-8" />   {{fonts}}         <style type="text/css">          body, td, table, h1, h2 {              font-family: "Poppins", sans-serif;          }          body {              font-size: 16px;              width: 100%;              margin: 0;              padding: 0;              color: #333;          }          table {              border-collapse: collapse;          }          td {              vertical-align: middle;          }          .body-table {              width: 1050px;              margin: 30px auto;          }          .main-table td {              padding: 5px;              border: 1px solid #393939;          }          .table-bg {              background: #005681;          }          .table-bg td {              padding: 5px;              text-align: center;              color: #fff;          }          .table-data {              border: 1px solid #e2e2e2;          }          .table-data td {              padding: 10px;              border-bottom: 1px solid #e2e2e2;          }          .table-data tr:first-child td {              line-height: 16px;              font-size: 13px;          }      </style>  </head>  <body>      <table width="100%" cellpadding="0" cellspacing="0" border="0">          <tr>              <td style="padding: 20px 0; color: #333; font-size: 15px; line-height:23px;">                  <table border="0" cellspacing="0" cellpadding="0" width="1050" align="center" style="border-color:rgba(0,0,0,.09);">                      <tr>                          <td>                              <table cellpadding="0" cellspacing="0" class="body-table">                                  <tr>                                      <td>                                          <table cellpadding="0" cellspacing="0" width="100%">                                              <tr>                                                  <td style="text-align:left;" width="80%">                                                      <div style="font-weight: bold; font-size: 13px;">{{clientCompanyName}}</div>                                                      <div style="font-size: 13px;">                                                          {{clientAddress}}<br />                                                          {{clientCityStateZip}}<br />                                                      </div>                                                  </td>                                                  <td align="right">                                                      {{clientLogo}}                                                  </td>                                              </tr>                                          </table>                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" cellspacing="0" style="width:100%;">                                  <tr>                                      <td style="font-weight:500; padding:6px 0; text-align:center; font-size: 22px; color:#289ed6; border-top:2px solid #005681; border-bottom: 2px solid #005681;">                                          Monthly Commission Statements as of {{statementDate}}                                      </td>                                  </tr>                              </table>                                <table cellpadding="0" cellspacing="0" class="body-table">                                  {{carrierBlocks}}                              </table>                          </td>                      </tr>                  </table>              </td>          </tr>      </table>  </body>  </html>  ' where  ReportTemplate_Id = 2 and Status = 1
go

insert into Accounting_ReportFileDownloadSettings (Report_Id,FileType_Id)
Values (1,1),
(1,2)
go
create Clustered Index CLU_Accounting_ReportFileDownloadSettings_Status_Report_Id_FileType_Id on Accounting_ReportFileDownloadSettings
(
Status desc,
Report_Id,
FileType_Id
)
go
  
 Alter PROC USP_Accounting_AgencyBillAccountStatementReport            
(            
    @Client_Id BIGINT = NULL,            
    @AgentDetail_Id BIGINT = NULL,            
    @InvoiceDate VARCHAR(100) = NULL,            
    @InvoiceCreatedFromDate VARCHAR(200) = NULL,            
    @InvoiceCreatedToDate VARCHAR(200) = NULL,            
    @InvoiceEffectiveFromDate VARCHAR(200) = NULL,            
    @InvoiceEffectiveToDate VARCHAR(200) = NULL,            
    @Range VARCHAR(200) = NULL ,           
 @WholesalerDetail_Id BIGINT = NULL,          
 @CarrierDetail_Id BIGINT = NULL,        
 @LOB_Id INT = NULL                
)            
AS            
BEGIN            
    DECLARE @CompareDateFrom DATETIME;            
    DECLARE @CompareDateTo DATETIME;            
            
    IF @InvoiceDate = 'InvoiceCreatedDate'            
    BEGIN            
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedFromDate, 101));            
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceCreatedToDate, 101))));            
    END            
    ELSE IF @InvoiceDate = 'InvoiceEffectiveDate'            
    BEGIN            
        SET @CompareDateFrom = CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveFromDate, 101));            
        SET @CompareDateTo = DATEADD(ms, -2, DATEADD(d, 1, CONVERT(DATETIME, CONVERT(VARCHAR(11), @InvoiceEffectiveToDate, 101))));            
    END            
            
    SELECT *             
    FROM Accounting_AgentDetails             
    WHERE Client_Id = @Client_Id AND Status = 1            
      AND (@AgentDetail_Id IS NULL OR AgentDetail_Id = @AgentDetail_Id);            
            
    SELECT *             
    FROM Accounting_PolicyDetails PD            
    WHERE Client_Id = @Client_Id             
      AND Status = 1             
      AND BillType = 'Agency'            
   AND        
    (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.CreatedOn > @CompareDateFrom AND PD.CreatedOn <= @CompareDateTo        
        )        
    )    
 AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)  
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)      
        
    ;WITH PolicyLOBs AS (            
        SELECT DISTINCT             
            PD.PolicyNumber,            
            PD.AgentDetail_Id,            
            IND.Name AS InsuredName,            
            LOB.Name AS LOBName,            
            CD.Name AS CarrierName            
        FROM Accounting_PolicyDetails PD            
        JOIN Accounting_InsuredDetails IND ON IND.InsuredDetail_Id = PD.InsuredDetail_Id AND IND.Status = 1            
        JOIN Accounting_CarrierDetails CD ON CD.CarrierDetail_Id = PD.CarrierDetail_Id AND CD.Status = 1            
        JOIN Accounting_Invoices IES ON IES.PolicyDetails_Id = PD.ID AND IES.Status = 1            
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = IES.ID AND ID.Status = 1            
        JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1            
        WHERE PD.Client_Id = @Client_Id AND PD.BillType = 'Agency' AND PD.Status = 1          
  AND        
  (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND IES.CreatedOn > @CompareDateFrom AND IES.CreatedOn <= @CompareDateTo        
        ))         
 AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)  
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)          
    )            
    SELECT             
        PolicyNumber,            
        InsuredName,       
        AgentDetail_Id,            
        CarrierName,            
        STUFF((            
            SELECT DISTINCT ', ' + L2.LOBName            
            FROM PolicyLOBs L2            
            WHERE L2.PolicyNumber = L1.PolicyNumber AND L2.InsuredName = L1.InsuredName            
            FOR XML PATH(''), TYPE            
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LOBNames            
    FROM PolicyLOBs L1            
    GROUP BY PolicyNumber, InsuredName, AgentDetail_Id, CarrierName;            
            
    ;WITH AggregatedPremiumCommissionInvoiceDetails AS (            
        SELECT             
            I.Id AS InvoiceId,            
            I.PolicyDetails_Id,            
            I.InvoiceNo,            
            SUM(CASE WHEN LI.Code = 'Premium' AND LUD.Code = 'AR' THEN ID.Amount ELSE 0 END) AS PremiumAR,            
            SUM(CASE WHEN LI.Code = 'AgencyBrokerCommission' AND LUD.Code = 'C' THEN ID.Amount ELSE 0 END) AS AgentCommission,            
            SUM(CASE WHEN LI.Code = 'MGACommission' AND LUD.Code = 'IN' THEN ID.Amount ELSE 0 END) AS MGACommission            
        FROM Accounting_Invoices I            
        JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType = 'Agency'            
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1            
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
        JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1            
        JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1            
        JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1            
        WHERE I.Status = 1            
  AND        
  (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo        
        ))        
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)  
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)          
        GROUP BY I.Id, I.PolicyDetails_Id, I.InvoiceNo            
    ),            
    FeeData AS (            
        SELECT             
            I.PolicyDetails_Id,            
            I.Id AS Invoice_Id,            
            ID.Id AS InvoiceDetail_Id,            
        CONCAT(PD.TransactionType, ' ', LI.Name) AS LineItemDescription,            
            I.InvoiceNo AS IdNo,            
            PD.EffectiveDate,            
            PD.ExpirationDate,            
            ID.Amount AS GrossAmount,            
            0 AS AgentCommPercentage,            
            0 AS AgentCommission,            
            ID.Amount AS NetAmount,            
            I.DueDate,            
            PD.PolicyNumber,            
            ID.RemainingBalance            
        FROM Accounting_Invoices I            
        JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType = 'Agency'            
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1            
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
        JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1            
        JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1            
        JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1            
        JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1            
        JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1            
        WHERE I.Status = 1            
      AND        
  (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo        
        ))         
  AND (LIT.Code = 'Fee' OR LIT.Code = 'Tax') AND LUD.Code = 'AR'      
  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)      
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)   
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)         
    ),            
    PaymentApplication AS (            
        SELECT             
            I.PolicyDetails_Id,            
            I.Id AS Invoice_Id,            
            ID.Id AS InvoiceDetail_Id,            
            'Cash Receipt' AS LineItemDescription,            
            CONCAT('Payment #', ' ', P.Payment_Id) AS IdNo,            
            CAST(P.CreatedOn AS DATE) AS EffectiveDate,            
            PD.ExpirationDate,            
            0 AS GrossAmount,            
            0 AS AgentCommPercentage,            
            0 AS AgentCommission,            
            CASE WHEN P.PaymentDirection_Id = 1 THEN -PALI.AppliedAmount ELSE ABS(PALI.AppliedAmount) END AS NetAmount,            
            I.DueDate,            
            PD.PolicyNumber,            
            0 AS RemainingBalance            
        FROM Accounting_Invoices I            
        JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType = 'Agency'            
        JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1            
        JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
        JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1            
        JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1            
        JOIN Accounting_LU_LineItemTypes LIT ON LIT.LineItemType_Id = LI.LineItemType_Id AND LIT.Status = 1            
        JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1            
        JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1            
        JOIN Accounting_PaymentApplicationLineItems PALI ON PALI.InvoiceLineItem_Id = ID.Id AND PALI.Status = 1            
        JOIN Accounting_PaymentApplications PA ON PA.PaymentApplication_Id = PALI.PaymentApplication_Id AND PA.Status = 1            
        JOIN Accounting_Payments P ON P.Payment_Id = PA.Payment_Id AND P.Status = 1            
        WHERE I.Status = 1            
  AND        
  (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo        
        ))        
  AND LUD.Code = 'AR' AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)    
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)   
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)                
    )            
            
    SELECT             
        I.PolicyDetails_Id,       
        I.Id AS Invoice_Id,            
        ID.Id AS InvoiceDetail_Id,            
        CONCAT(PD.TransactionType, ' ', LI.Name) AS LineItemDescription,            
        I.InvoiceNo AS IdNo,            
        PD.EffectiveDate,            
        PD.ExpirationDate,            
        APCID.AgentCommission + PremiumAR AS GrossAmount,            
        ((APCID.AgentCommission) / (NULLIF(APCID.AgentCommission + PremiumAR, 0)) * 100) AS AgentCommPercentage,            
        APCID.AgentCommission,            
        APCID.PremiumAR AS NetAmount,            
        I.DueDate,            
        PD.PolicyNumber,            
        ID.RemainingBalance            
    FROM Accounting_Invoices I            
    JOIN Accounting_PolicyDetails PD ON PD.ID = I.PolicyDetails_Id AND PD.Client_Id = @Client_Id AND PD.Status = 1 AND PD.BillType = 'Agency'            
    JOIN Accounting_InvoiceDetails ID ON ID.Invoice_Id = I.Id AND ID.Status = 1            
    JOIN Accounting_LU_BillingType_LineItems_Directions BLD ON BLD.BillingType_LineItems_Directions_Id = ID.BillingType_LineItems_Directions_Id AND BLD.Status = 1            
    JOIN Accounting_LU_LineItems_Directions LD ON LD.LineItems_Directions_Id = BLD.LineItems_Directions_Id AND LD.Status = 1            
    JOIN Accounting_LU_LineItems LI ON LI.LineItem_Id = LD.LineItem_Id AND LI.Status = 1            
    JOIN Accounting_LU_Directions LUD ON LUD.Direction_Id = LD.Direction_Id AND LUD.Status = 1            
    JOIN AggregatedPremiumCommissionInvoiceDetails APCID ON APCID.InvoiceId = I.Id            
    JOIN Accounting_LU_LOBs LOB ON LOB.LOB_Id = ID.LOB_Id AND LOB.Status = 1            
    WHERE I.Status = 1             
  AND        
  (        
        @CompareDateFrom IS NULL OR @CompareDateTo IS NULL        
        OR        
        (        
            @InvoiceDate = 'InvoiceEffectiveDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND PD.EffectiveDate > @CompareDateFrom AND PD.EffectiveDate <= @CompareDateTo        
        )        
        OR        
        (        
            @InvoiceDate = 'InvoiceCreatedDate'        
            AND @CompareDateFrom IS NOT NULL AND @CompareDateTo IS NOT NULL        
            AND I.CreatedOn > @CompareDateFrom AND I.CreatedOn <= @CompareDateTo        
        ))            
  AND LI.Code = 'Premium' AND LUD.Code = 'AR'  AND (@AgentDetail_Id IS NULL OR PD.AgentDetail_Id = @AgentDetail_Id)     
 AND (@WholesalerDetail_Id IS NULL OR PD.WholesalerDetail_Id = @WholesalerDetail_Id)  
 AND (@CarrierDetail_Id IS NULL OR PD.CarrierDetail_Id = @CarrierDetail_Id)               
            
    UNION ALL            
            
    SELECT * FROM FeeData            
            
    UNION ALL            
            
    SELECT * FROM PaymentApplication            
END   
go
update Accounting_Clients set City = 'Garden City', State = 'NY', Zipcode = '11530' where Client_Id = 10001
go
