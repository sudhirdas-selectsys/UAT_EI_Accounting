using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.ePay;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_ClientSettings
    {
        public Enum_ClientSettings()
        {
            InvoicePaymentFooter = "Invoice.PaymentFooter";
            BillingDueDateRuleAgencyBill = "Billing.DueDateRule.AgencyBill";
            BillingDueDateRuleInsuredBill = "Billing.DueDateRule.InsuredBill";
            BillingDueDateRuleDirectBill = "Billing.DueDateRule.DirectBill";
            InvoicePaymentURLTemplate = "Invoice.PaymentURLTemplate";
            InvoiceHideAgencyCommissionInfo = "Invoice.HideAgencyCommissionInfo";
            InvoicePaymentAccountInfo = "Invoice.PaymentAccountInfo";
            InvoiceDetachAndRemitInfo = "Invoice.DetachAndRemitInfo";
            AllowExternalAccountingID = "AllowExternalAccountingID";
            Is_AllowCarrierGrouping = "Is_AllowCarrierGrouping";
            IsSingleInvoiceCreated = "IsSingleInvoiceCreated";
            AllowMinimumPremium = "AllowMinimumPremium";
            MinimumPremiumMessage = "MinimumPremiumMessage";
            RetailerMessage = "RetailerMessage";
            WholesalerMessage = "WholesalerMessage";
            DisplayAccountingId = "DisplayAccountingId";
            InvoiceShowTransactionEffectiveDate = "Invoice.ShowTransactionEffectiveDate";
            InvoiceHidePaymentDue = "Invoice.HidePaymentDue";
            InvoicePaymentDueOn = "Invoice.PaymentDueOn";
            InvoicePaymenGuidelines = "Invoice.PaymentGuidelines";
            InvoiceFormatAddress = "Invoice.FormatAddress";
            CommissionRule = "UW.CommissionRule";
            DefaultCommissionRule = "default.UnderwriterCommission";
            Paymentinstructions_MailCheck = "Paymentinstructions_MailCheck";
            Paymentinstructions_ExpressMail = "Paymentinstructions_ExpressMail";
            InvoiceHasDefaultTemplate = "Invoice.HasDefaultTemplate";
            SchedulerJobs = "SchedulerJobs";
            ReportsInvoiceDueDate = "Reports.InvoiceDueDate";
            RTWHasDefaultTemplate = "RWT.HasDefaultTemplate";
            InvoiceFooter = "Invoice.Footer";
            ePayAllowPartialPayment = "ePay.AllowPartialPayment";
            PaymentTypeVisibility = "PaymentTypeVisibility";
            AgencyBillReportFinalWarning = "AgencyBillReportFinalWarning";
            DisableInvoiceAccordionAutoExpand = "DisableInvoiceAccordionAutoExpand";
            EnableDashBoardGridSorting = "EnableDashBoardGridSorting";
            EnablePolicyDetailsInvoicesGridSorting = "EnablePolicyDetailsInvoicesGridSorting";
            InvoiceShowInsuredDetails = "Invoice.ShowInsuredDetails";
            CommissionFieldSelectionAR = "CommissionFieldSelectionMode";
            HasDefaultPaymentTypes = "HasDefaultPaymentTypes";
            ShowDownPaymentDue = "Invoice.ShowDownPaymentDue";
            UseEffectiveDateForQBO = "QBO.UseEffectiveDate";
            ShowAgencyBillEmailAndPhone = "Invoice.AgencyBill.EmailAndPhone";
            ShowInsuredBillEmailAndPhone = "Invoice.InsuredBill.EmailAndPhone";
            EnableInvoiceProductionReportTransactionEffectiveDate = "InvoiceProductionReportDateRangeType";
            EnableCoverPayPaymentLink = "EnableCoverPay_PaymentLink";
            EnableCoverPayStripePaymentLink = "EnableCoverPayStripePaymentLink";
            CoverPayExternalClient_Id = "CoverPayExternalClient_Id";
            CoverPayExternalPaymentLinkURL = "CoverPayExternalPaymentLinkURL";
            EasternDirectBillCommissionStatementPaymentBased = "DirectBillCommissionStatementPaymentBased";
            InvoiceShowCoverageType = "Invoice.ShowCoverageType";
            InvoiceShowEmail = "Invoice.ShowEmail";
            EntityTypeVisibility = "EntityTypeVisibility";
            AllowIsExcludeLineItemMapping = "AllowIsExcludeLineItemMapping";
            EnableOutstandingInvoicesForARReportAllTrans = "EnableOutstandingInvoicesForARReport";
            EnableMultiDropdownListForBDXReportGuardian = "EnableMultiDropdownListForBDXReport";
            UseLobNameForInvoiceDescription = "UseLobNameForInvoiceDescription";
            HasSpecificClassId = "sage.HasSpecificClassId";
            InvoiceDueDateChangeEnabled = "InvoiceDueDateChangeEnabled";
            InvoiceInstallmentDueDateTermDays = "InvoiceInstallmentDueDateTermDays";
            InvoiceInstallmentDueDateUseMaxAnchorDate = "InvoiceInstallmentDueDateUseMaxAnchorDate";
            InvoiceInstallmentDueDateEffectiveFrom = "InvoiceInstallmentDueDateEffectiveFrom";
            InvoiceShowRemainingInstallments = "Invoice.ShowRemainingInstallments";
            AllowOpenInvoicesAPI = "AllowOpenInvoicesAPI";
            EnableInsuredNameFilterinInvoiceProduction = "EnableInsuredNameFilterinInvoiceProductionReport";
            EnableMultiDropdownForCarrierInInvoiceProduction = "EnableMultiDropdownForCarrier.InvoiceProductionReport";
            ShowReferralSourceColumn = "Reports.InvoiceProductionReport.ShowReferralSource";
            EnableAdditionalInvoiceProductionColumns = "EnableAdditionalColumnsInInvoiceProductionReport";
            RTWHasDownPayments = "RTWHasDownPayments";
            HasUniqueCarrierNames = "HasUniqueCarrierNames";
            AsteriskVisability = "AsteriskVisability";
            ExcludeTemplateFromCheckKeeperRequest = "ExcludeTemplateFromCheckKeeperRequest";
        }
        public string InvoicePaymentFooter { get; set; }
        public string BillingDueDateRuleAgencyBill { get; set; }
        public string BillingDueDateRuleInsuredBill { get; set; }
        public string BillingDueDateRuleDirectBill { get; set; }
        public string InvoicePaymentURLTemplate { get; set; }
        public string InvoiceHideAgencyCommissionInfo { get; set; }
        public string InvoicePaymentAccountInfo { get; set; }
        public string InvoiceDetachAndRemitInfo { get; set; }
        public string AllowExternalAccountingID { get; set; }
        public string Is_AllowCarrierGrouping { get; set; }
        public string IsSingleInvoiceCreated { get; set; }
        public string AllowMinimumPremium { get; set; }
        public string MinimumPremiumMessage { get; set; }
        public string RetailerMessage { get; set; }
        public string WholesalerMessage { get; set; }
        public string DisplayAccountingId { get; set; }
        public string InvoiceShowTransactionEffectiveDate { get; set; }
        public string InvoiceHidePaymentDue { get; set; }
        public string InvoicePaymentDueOn { get; set; }
        public string InvoicePaymenGuidelines { get; set; }
        public string InvoiceFormatAddress { get; set; }
        public string CommissionRule { get; set; }
        public string DefaultCommissionRule { get; set; }
        public string Paymentinstructions_MailCheck { get; set; }
        public string Paymentinstructions_ExpressMail { get; set; }
        public string InvoiceHasDefaultTemplate { get; set; }
        public string SchedulerJobs { get; set; }
        public string ReportsInvoiceDueDate { get; set; }
        public string RTWHasDefaultTemplate { get; set; }
        public string InvoiceFooter { get; set; }
        public string ePayAllowPartialPayment { get; set; }
        public string PaymentTypeVisibility { get; set; }
        public string AgencyBillReportFinalWarning { get; set; }
        public string DisableInvoiceAccordionAutoExpand { get; set; }
        public string EnableDashBoardGridSorting { get; set; }
        public string EnablePolicyDetailsInvoicesGridSorting { get; set; }
        public string EnableCoverPayPaymentLink { get; set; }
        public string InvoiceShowInsuredDetails { get; set; }
        public string CommissionFieldSelectionAR { get; set; }
        public string HasDefaultPaymentTypes { get; set; }
        public string ShowDownPaymentDue { get; set; }
        public string UseEffectiveDateForQBO { get; set; }
        public string ShowAgencyBillEmailAndPhone { get; set; }
        public string ShowInsuredBillEmailAndPhone { get; set; }
        public string EnableInvoiceProductionReportTransactionEffectiveDate { get; set; }
        public string EnableCoverPayStripePaymentLink { get; set; }
        public string CoverPayExternalClient_Id { get; set; }
        public string CoverPayExternalPaymentLinkURL { get; set; }
        public string EasternDirectBillCommissionStatementPaymentBased { get; set; }
        public string InvoiceShowCoverageType { get; set; }
        public string InvoiceShowEmail { get; set; }
        public string EntityTypeVisibility { get; set; }
        public string AllowIsExcludeLineItemMapping { get; set; }
        public string EnableOutstandingInvoicesForARReportAllTrans { get; set; }
        public string EnableMultiDropdownListForBDXReportGuardian { get; set; }
        public string UseLobNameForInvoiceDescription { get; set; }
        public string HasSpecificClassId { get; set; }
        public string InvoiceDueDateChangeEnabled { get; set; }
        public string InvoiceInstallmentDueDateTermDays { get; set; }
        public string InvoiceInstallmentDueDateUseMaxAnchorDate { get; set; }
        public string InvoiceInstallmentDueDateEffectiveFrom { get; set; }
        public string InvoiceShowRemainingInstallments { get; set; }
        public string AllowOpenInvoicesAPI { get; set; }
        public string EnableInsuredNameFilterinInvoiceProduction { get; set; }
        public string EnableMultiDropdownForCarrierInInvoiceProduction { get; set; }
        public string ShowReferralSourceColumn { get; set; }
        public string EnableAdditionalInvoiceProductionColumns { get; set; }
        public string RTWHasDownPayments { get; set; }
        public string HasUniqueCarrierNames { get; set; }
        public string AsteriskVisability { get; set; }
        public string ExcludeTemplateFromCheckKeeperRequest { get; set; }
    }
}
