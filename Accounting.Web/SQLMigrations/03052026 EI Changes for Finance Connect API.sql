-- pulled NAIC from tblCarriers table in EI
ALTER PROCEDURE [dbo].[eisp_Accounting_SilverFinancials_GetPolicyTransferJSON_V1]      
(      
    @SubmissionID INT      
)      
AS      
BEGIN      
    SET NOCOUNT ON;      
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;      
      
    DECLARE @PostAsShell BIT = 0;      
    SELECT @PostAsShell = CASE WHEN FieldValue = '1' THEN 1 ELSE 0 END      
    FROM tblSubmissions_AdditionalInfo      
    WHERE FieldName = '~PostAsShell'      
      AND SubmissionID = @SubmissionID;      
      
    DECLARE @json NVARCHAR(MAX) = N'';      
      
    SET @json = (      
        SELECT      
              CONVERT(VARCHAR(6), s.SubmissionID) AS SubmissionId      
            , CONVERT(VARCHAR(6), s.OriginalSubmissionID) AS OriginalSubmissionID      
            , CONVERT(VARCHAR(6), s.PreviousSubmissionID) AS PreviousSubmissionID      
            , q.PolicyNumber      
            , CONVERT(VARCHAR(6), s.UserID) AS AssignUser_Id      
            , u.FullName AS AssignUserName      
            , CONVERT(VARCHAR(6), q.QuoteId) AS QuoteId      
            , '' AS PolicyInstallmentCode      
            , tc.CompanyName AS Corporation      
            , '01' AS [Location]      
            , q.PrimaryState AS Risk      
            , CONVERT(VARCHAR(6), s.BrokerID) AS Agent      
            , '' AS Producer      
            , CONVERT(VARCHAR(6), COALESCE(q.CarrierID, '')) AS Carrier      
            , CONVERT(VARCHAR(6), COALESCE(q.WholesalerID, '')) AS Wholesaler      
            , qai.SilverFinancialsInstallmentPlan AS PaymentPlan      
            , CASE qai.SilverFinancialsBillType      
                WHEN '0'   THEN 'NoBill'      
                WHEN '1'   THEN 'Agency'      
                WHEN '2'   THEN 'Insured'      
                WHEN '3'   THEN 'Direct'      
                WHEN '255' THEN 'DoNotBill'      
                ELSE qai.SilverFinancialsBillTypeAlias      
              END AS BillType      
            , FORMAT(q.EffectiveDate, 'MM/dd/yyyy') AS EffectiveDate      
            , FORMAT(qe.EndorsementDate, 'MM/dd/yyyy') AS EndorsementEffectiveDate      
            , FORMAT(q.ExpirationDate, 'MM/dd/yyyy') AS ExpirationDate      
            , FORMAT(CASE WHEN s.SubmissionTypeID = 4 THEN qe.EndorsementDate ELSE q.ExpirationDate END, 'MM/dd/yyyy') AS CancelDate      
            , FORMAT(can.CancelOnDate,'MM/dd/yyyy') AS PolicyCancellationDate    
   ,qe.EndorsementNumber    
            , CASE      
                WHEN s.SubmissionTypeID IN (2, 8) THEN      
                    CASE WHEN s.CancellationEndorsement = 1 THEN 'Void' ELSE 'Revised' END      
                WHEN s.SubmissionTypeID = 4  THEN 'Void'      
                WHEN s.SubmissionTypeID = 10 THEN 'Audit'      
                ELSE 'New Line'      
              END AS PolicyAction      
            , CASE WHEN PATINDEX('%,%', q.LOB) > 0 THEN 'XX' ELSE q.LOB END AS LOBOverride      
            , CASE WHEN s.PackageGUID <> '' AND q.IsPackage = 1 THEN 'Package' ELSE '' END AS PolicyType      
            , st.Description AS TransactionType      
            , i.Name AS PolicyDescription      
            , COALESCE(InvoiceNotes.Note,'') AS InvoiceNote      
      
            , b.Name AS AgentName      
            , COALESCE(b.Email,'') AS AgentEmail      
            , COALESCE(b.FEIN,'') AS AgentFEIN      
            , COALESCE(b.Address1,'') AS AgentAddress1      
            , COALESCE(b.Address2,'') AS AgentAddress2      
            , COALESCE(b.City,'') AS AgentCity      
            , COALESCE(b.Country,'') AS AgentCountry      
            , '' AS AgentCounty      
            , COALESCE(b.State,'') AS AgentState      
            , COALESCE(b.Zip,'') AS AgentZipCode      
            , COALESCE(ABI.AccountNumber, '') AS AgentAccountNumber      
            , COALESCE(ABI.RoutingNumber, '') AS AgentRoutingNumber      
      
            , COALESCE(c.CompanyName,'') AS CarrierName      
            , COALESCE(c.Address1,'') AS CarrierAddress1      
            , COALESCE(c.Address2,'') AS CarrierAddress2      
            , COALESCE(c.City,'') AS CarrierCity      
            , COALESCE(c.Country,'') AS CarrierCountry  
   , COALESCE(c.NAIC, '') AS CarrierNAIC  
            , CONVERT(VARCHAR(100), COALESCE(b.AccountingID, '')) AS AgentAccountingID      
            , '' AS CarrierCounty      
            , COALESCE(c.State,'') AS CarrierState      
            , COALESCE(c.PostalCode,'') AS CarrierZipCode      
            , CASE WHEN CL.NonAdmitted = 1 THEN 'NonAdmitted' ELSE 'Admitted' END AS CarrierStatus      
      
            , COALESCE(w.CompanyName,'') AS WholesalerName      
            , COALESCE(w.DisplayName,'') AS WholesalerDisplayName      
            , CONVERT(VARCHAR(6), COALESCE(w.ParentID,'')) AS WholesalerParentID      
            , CONVERT(VARCHAR(6), COALESCE(w.Active,'')) AS WholesalerIsActive      
            , COALESCE(w.AlternateID,'') AS WholesalerAlternateID      
      
            , CONVERT(VARCHAR(6), i.InsuredID) AS Insured      
            , i.Name AS InsuredName      
            , COALESCE(i.Address1,'') AS InsuredAddress1      
            , COALESCE(i.Address2,'') AS InsuredAddress2      
            , COALESCE(i.City,'') AS InsuredCity      
            , COALESCE(i.Country,'') AS InsuredCountry      
            , '' AS InsuredCounty      
            , COALESCE(i.State,'') AS InsuredState      
            , COALESCE(i.PostalCode,'') AS InsuredZipCode      
            , COALESCE(il.Address1,'') AS InsuredPhysicalAddress1      
            , COALESCE(il.Address2,'') AS InsuredPhysicalAddress2      
            , COALESCE(il.City,'') AS InsuredPhysicalCity      
            , COALESCE(il.Country,'') AS InsuredPhysicalCountry      
            , COALESCE(il.State,'') AS InsuredPhysicalState      
            , COALESCE(il.PostalCode,'') AS InsuredPhysicalZipCode      
            , COALESCE(i.FEIN,'') AS InsuredFEIN      
            , COALESCE(i.DBA,'') AS InsuredDBA      
      
            , COALESCE(qai.FinanceCompanyValue,'') AS Finance      
            , COALESCE(qai.FinanceCompanyName,'') AS FinanceName      
            , '' AS FinanceAddress1      
            , '' AS FinanceAddress2      
            , '' AS FinanceCity      
            , '' AS FinanceCountry      
            , '' AS FinanceCounty      
            , '' AS FinanceState      
            , '' AS FinanceZipCode      
      
            , CONVERT(VARCHAR(6), und.UserID) AS UW      
            , und.FullName AS UWName      
            , cb.Address1 AS UWAddress1      
            , cb.Address2 AS UWAddress2      
            , cb.City AS UWCity      
            , 'United States' AS UWCountry      
            , '' AS UWCounty      
            , cb.State AS UWState      
            , cb.PostalCode AS UWZipCode      
      
            , '' AS ConsolidatedGuid      
            , CONVERT(VARCHAR(6), q.CarrierID) AS WrittingCarrier      
            , COALESCE(c.CompanyName,'') AS WrittingName      
      
            , '' AS ProducerName      
            , '' AS ProducerAddress1      
            , '' AS ProducerAddress2      
            , '' AS ProducerCity      
            , '' AS ProducerCountry      
            , '' AS ProducerCounty      
            , '' AS ProducerState      
            , '' AS ProducerZipCode      
      
            , CONVERT(VARCHAR(6), COALESCE(qi.TypeID,0)) AS InterestTypeID      
            , '' AS Interest      
            , qi.Name AS InterestName      
            , qi.Address1 AS InterestAddress1      
            , qi.Address2 AS InterestAddress2      
            , qi.City AS InterestCity      
            , qi.Country AS InterestCountry      
            , qi.County AS InterestCounty      
            , qi.State AS InterestState      
            , qi.PostalCode AS InterestZipCode      
      
            , '' AS MasterPolicy      
            , '' AS LoanNum      
            , CASE WHEN @PostAsShell = 1 THEN '1' ELSE '0' END AS PostAsShell      
      
                  
          --  , COALESCE(lc.Name, '') AS CustomerType      
      
            , (      
                SELECT *      
                FROM (      
                    /* ===================== PREMIUM LINES ===================== */      
                    SELECT      
                          CONVERT(VARCHAR(6), q.SubmissionID) AS SubmissionId      
                        , '' AS CommissionId      
                        , 'PREMIUM' AS LineType      
                        , FORMAT(lp1.CommissionPercent, 'N3') AS HouseCommission      
                        , FORMAT(lp1.BrokerCommissionPercent, 'N3') AS AgentCommission      
                        , REPLACE(      
                            CASE      
                                WHEN s.SubmissionTypeID IN (1,3)      
                                    THEN FORMAT(lp1.Commission, 'N2')      
                                ELSE FORMAT(CAST((lp1.AdditionalPrem * lp1.CommissionPercent) / 100 AS DECIMAL(18,2)), 'N2')      
                            END, ',', ''      
                          ) AS CommissionAmount      
                        , REPLACE(      
                            CASE      
                                WHEN s.SubmissionTypeID IN (1,3)      
                                    THEN FORMAT(lp1.BrokerCommission, 'N2')      
                                ELSE FORMAT(CAST((lp1.AdditionalPrem * lp1.BrokerCommissionPercent) / 100 AS DECIMAL(18,2)), 'N2')      
                            END, ',', ''      
                          ) AS BrokerCommissionAmount      
                        , tl.Name AS Coverage      
                        , tl.Name AS PolicyDetailDescription      
                        , '0.00' AS FeeTaxAmount      
                        , REPLACE(      
                            CASE      
                                WHEN s.SubmissionTypeID IN (1,3)      
                                    THEN FORMAT((lp1.PurePremium + lp1.TerrorismPremium), 'N2')      
                                ELSE FORMAT(lp1.AdditionalPrem, 'N2')      
                            END, ',', ''      
                          ) AS Premium      
                        , 0 AS FeeTaxable      
                        , '0' AS Is_Commissionable      
                        , FORMAT(COALESCE(qe.EndorsementDate, q.EffectiveDate), 'MM/dd/yyyy', 'en-US') AS EffectiveDate      
                        , lp1.[State] AS [State]      
      
                        --  CustomerType       
                        , CASE      
                            WHEN UPPER(tlc.Name) LIKE '%Commercial Line%' THEN 'CL'      
                            WHEN UPPER(tlc.Name) LIKE '%Personal Line%'   THEN 'PL'      
                            ELSE COALESCE(tlc.Name, '')      
                          END AS CustomerType      
      
                    FROM tblQuotes_LOB_Premiums lp1      
                    INNER JOIN tblTypes_LOBs tl      
                        ON tl.Code = lp1.LOB      
                    LEFT JOIN tblTypes_LOBClassificationTypes tlc      
                        ON tlc.LOBClassificationID = tl.LOBClassificationID      
                    WHERE lp1.QuoteID = q.QuoteID      
      
                    UNION ALL      
      
                    /* ===================== FEE / TAX LINES ===================== */      
                    SELECT      
                          CONVERT(VARCHAR(6), q.SubmissionID) AS SubmissionId      
                        , '' AS CommissionId      
                        , CASE WHEN csf.TypeID = 0 THEN 'TAX' ELSE 'FEE' END AS LineType      
                        , '0.00' AS HouseCommission      
                        , '0.00' AS AgentCommission      
                        , '0.00' AS CommissionAmount      
                        , '0.00' AS BrokerCommissionAmount      
                        , tl.Name AS Coverage      
                        , qf.OtherDescription AS PolicyDetailDescription      
                        , REPLACE(FORMAT(CASE WHEN s.SubmissionTypeID IN (1,3) THEN qf.Amount ELSE qf.EndAmount END, 'N2'), ',', '') AS FeeTaxAmount      
                        , FORMAT(CASE WHEN s.SubmissionTypeID IN (1,3)      
                                      THEN (lp1.PurePremium + lp1.TerrorismPremium)      
                                      ELSE lp1.AdditionalPrem      
                                 END, 'N2') AS Premium      
                        , 0 AS FeeTaxable      
                        , CONVERT(VARCHAR(6), qf.Commissionable) AS Is_Commissionable      
                        , FORMAT(COALESCE(qe.EndorsementDate, q.EffectiveDate), 'MM/dd/yyyy', 'en-US') AS EffectiveDate      
                        , qf.[State] AS [State]      
      
                        --  CustomerType       
                        , CASE      
                            WHEN UPPER(tlc.Name) LIKE '%Commercial Line%' THEN 'CL'      
                            WHEN UPPER(tlc.Name) LIKE '%Personal Line%'   THEN 'PL'      
                            ELSE COALESCE(tlc.Name, '')      
                          END AS CustomerType      
      
    FROM tblQuotes_LOB_Premiums lp1      
                    INNER JOIN tblQuotes_Fees qf      
                        ON qf.QuoteID = lp1.QuoteID      
                       AND qf.LOB     = lp1.LOB      
                    INNER JOIN tblTypes_LOBs tl      
                        ON tl.Code = lp1.LOB      
                    LEFT JOIN tblTypes_LOBClassificationTypes tlc      
                        ON tlc.LOBClassificationID = tl.LOBClassificationID      
                    INNER JOIN tblCorporation_States_Fees csf      
                        ON csf.FeeID = qf.FeeID      
                    WHERE lp1.QuoteID = q.QuoteID      
                ) d      
                FOR JSON PATH      
            ) AS PolicyDetailTransfer      
      
        FROM tblSubmissions s      
        INNER JOIN tblTypes_SubmissionTypes st ON st.SubmissionTypeID = s.SubmissionTypeID      
        INNER JOIN tblTypes_SubmissionStatusCodes sc ON sc.StatusCode = s.Status      
        INNER JOIN tblCorporation_Users u ON u.UserID = s.UserID      
        INNER JOIN tblCorporation_Users und ON und.UserID = s.OwnerUserID      
        INNER JOIN tblCorporation_Branches cb ON cb.BranchID = und.BranchID      
        INNER JOIN tblCorporation tc ON tc.CorporationId = und.CorporationId      
        INNER JOIN tblBrokers b ON b.BrokerID = s.BrokerID      
        LEFT JOIN tblBrokers_AgentBankInfo ABI ON ABI.BrokerID = b.BrokerID AND ABI.Is_Default = 1      
        INNER JOIN tblInsureds i ON i.InsuredID = s.InsuredID      
        LEFT JOIN tblInsureds_Locations il ON il.InsuredID = i.InsuredID AND il.LocationTypeId = 0      
        INNER JOIN tblQuotes q ON q.QuoteID = s.BoundQuoteID      
        LEFT JOIN tblSubmissions_Cancellations can ON can.SubmissionID = s.SubmissionID      
        LEFT OUTER JOIN tblQuotes_EndorsementInfo qe ON qe.QuoteID = q.QuoteID      
        INNER JOIN tblCarriers c ON c.CarrierID = q.CarrierID      
        OUTER APPLY (SELECT TOP 1 CL.NonAdmitted FROM tblCarriers_LOBs CL WHERE CL.CarrierID = c.CarrierID) CL      
        LEFT OUTER JOIN tblWholesalers w ON w.WholesalerID = q.WholesalerID      
        OUTER APPLY (      
            SELECT      
                  MAX(CASE WHEN FieldName = 'SilverFinancialsFinanceCompany' THEN FieldValue END) AS SilverFinancialsFinanceCompany      
                , MAX(CASE WHEN FieldName = 'SilverFinancialsInstallmentPlan' THEN FieldValue END) AS SilverFinancialsInstallmentPlan      
                , MAX(CASE WHEN FieldName = 'FinanceCompany' THEN FieldAlias END) AS FinanceCompanyName      
                , MAX(CASE WHEN FieldName = 'FinanceCompany' THEN FieldValue END) AS FinanceCompanyValue      
                , MAX(CASE WHEN FieldName = 'SilverFinancialsBillType' THEN FieldValue END) AS SilverFinancialsBillType      
                , MAX(CASE WHEN FieldName = 'SilverFinancialsBillType' THEN FieldAlias END) AS SilverFinancialsBillTypeAlias      
            FROM tblQuotes_AdditionalInfo AS qx WITH (NOLOCK)      
            WHERE qx.FieldName IN ('SilverFinancialsFinanceCompany','SilverFinancialsInstallmentPlan','SilverFinancialsBillType','FinanceCompany')      
              AND qx.QuoteID = q.QuoteID      
            GROUP BY QuoteID      
        ) AS qai      
        OUTER APPLY (SELECT TOP 1 * FROM tblQuotes_Interests WHERE QuoteID = q.QuoteID) AS qi      
        OUTER APPLY (SELECT TOP 1 Note FROM tblQuotes_Notes WHERE NoteTypeID = 8 AND QuoteID = q.QuoteID) AS InvoiceNotes      
        LEFT JOIN tblTypes_LOBs l WITH (NOLOCK) ON l.Code = s.LOB      
        LEFT JOIN tblTypes_LOBClassificationTypes lc WITH (NOLOCK) ON lc.LOBClassificationID = l.LOBClassificationID AND l.Active = 1      
        WHERE s.SubmissionID = @SubmissionID      
        FOR JSON PATH, INCLUDE_NULL_VALUES      
    );      
      
          
    SELECT REPLACE(REPLACE(@json,'NULL','""'), '’', '''') AS d;      
END 