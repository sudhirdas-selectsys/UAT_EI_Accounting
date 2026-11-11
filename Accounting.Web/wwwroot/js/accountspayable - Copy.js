$(function () {
    $('[data-name="EntityName"]').each(function () {
        EnableNameComplete(this);
    });
    $('[data-name="PolicyNumber"]').each(function () {
        EnablePolicyNumberComplete(this);
    });
    $('[data-name="AgencyName"]').each(function () {
        EnableAgencyNameComplete(this);
    });
    $('[data-name="InsuredName"]').each(function () {
        EnableInsuredNameComplete(this);
    });
    BindAPSearchPanelEvents();
    LoadAPPanel();
});
function EnableNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;

    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var selectedEntity = $(ctrl).closest('[data-name="FilterSection"]').find('[data-name="EntityType_Id"]').val();
            var Name = $(ctrl).val();
            if (typeof selectedEntity == "undefined" || selectedEntity == '') {
                appCommon.ShowMessageModal("Select a Payor/Payee Type to proceed!", "Alert");
                return;
            }
            $.ajax({
                url: "/api/SearchAPI/GetNames",
                data: { EntityType_Id: selectedEntity, Name: Name, Client_Id: $('[data-name="Client_Id"]').val() },
                success: function (data) {
                    if (data != 'undefined' && data != null && data != '') {
                        response($.map(data.Data, function (item, key) {
                            return {
                                label: item.Name,
                                value: item.Name,
                                EntityID: item.Value
                            }
                        }));
                    }
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            $(ctrl).closest('[data-name="FilterSection"]').find('[data-name="EntityName"]').val(ui.item.value);
            $(ctrl).parent().find('[data-name="Entity_Id"]').val(ui.item.EntityID);
        },

    });
}
function EnablePolicyNumberComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var PolicyNumber = $(ctrl).val();
            $.ajax({
                url: "/TransactionHistory/GetPolicyNumbers",
                data: { PolicyNumber: PolicyNumber },
                success: function (data) {
                    if (data != 'undefined' && data != null && data != '') {
                        response($.map(data.Data, function (item, key) {
                            return {
                                label: item.Name,
                                value: item.Name
                            }
                        }));
                    }
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            $(ctrl).val(ui.item.value);
        },
    });
}
function EnableInsuredNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var InsuredName = $(ctrl).val();
            $.ajax({
                url: "/api/SearchAPI/GetNames",
                data: { EntityType_Id: MasterLookup._EntityTypes.Insured, Name: InsuredName, Client_Id: $('[data-name="Client_Id"]').val() },
                success: function (data) {
                    if (data != 'undefined' && data != null && data != '') {
                        response($.map(data.Data, function (item, key) {
                            return {
                                label: item.Name,
                                value: item.Name
                            }
                        }));
                    }
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            $(ctrl).val(ui.item.value);
        },
    });
}
function EnableAgencyNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var AgencyName = $(ctrl).val();
            $.ajax({
                url: "/api/SearchAPI/GetNames",
                data: { EntityType_Id: MasterLookup._EntityTypes.Agency, Name: AgencyName, Client_Id: $('[data-name="Client_Id"]').val() },
                success: function (data) {
                    if (data != 'undefined' && data != null && data != '') {
                        response($.map(data.Data, function (item, key) {
                            return {
                                label: item.Name,
                                value: item.Name
                            }
                        }));
                    }
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            $(ctrl).val(ui.item.value);
        },
    });
}
function BindAPSearchPanelEvents() {
    $('[data-name="btnApplyFilters"]').off('click');
    $('[data-name="btnSelectPayment"]').off('click');

    $('[data-name="CollapseAPRows"]').off('click');
    $('[data-name="CollapseAPRows"]').on('click', function () {
        $('.accordion-collapse').collapse('hide');
    });

    $('[data-name="ExpandAPRows"]').off('click');
    $('[data-name="ExpandAPRows"]').on('click', function () {
        $('.accordion-collapse').collapse('show');
    });

    $('[data-name="btnApplyFilters"]').on('click', function () {
        var APContainer = $('[data-name="divAccountsPayableGrid"]');
        appCommon.ShowPartialPanelLoader(APContainer);
        appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
            if (response != undefined && response != null) {
                $(APContainer).html(response);
                BindAPSearchPanelEvents();
                BindAPGridPanelEvents();
            }
        }, "/Search/SearchAP", true, false);
    });
    $('[data-name="btnSelectPayment"]').on('click', function () {
        appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
            if (response != undefined && response != null) {
                appCommon.GenericModalFunction("Select Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                appCommon.HideLoader();
                BindAPSearchPanelEvents();
                BindAPGridPanelEvents();
                UpdatePaymentButtons();
            }
        }, "/Search/PaymentsSearchForSelection", false, true);
    });
    $('[data-name="btnEditSelectedPayment"]').off('click');
    $('[data-name="btnEditSelectedPayment"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
            if (response != undefined && response != null) {
                appCommon.GenericModalFunction("Select Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                $('[data-name="payment_' + Payment_Id + '"]').addClass('selected-payment');
            }
        }, "/Search/PaymentsSearchForSelection", true, false);
    });
}
function BindAPGridPanelEvents() {
    $('[data-name="btnApplyPayment"]').hide();
    $('[data-name="divAmountApplied"]').hide();
    $('[data-name="btnUnApplyFullPayment"]').hide();
    $('[data-name="chkInvoiceDetails"]').off('click').on('click', function () {
        UpdatePaymentButtons();
        calculateTotalAppliedAmount();
    });

    $('input[type="checkbox"][data-name="chkInvoice"]').change(function () {
        var invoiceId = $(this).attr('data-invoice-id');
        var isChecked = $(this).is(':checked');
        $('input[type="checkbox"][data-name="chkInvoiceDetails"][data-invoice-id="' + invoiceId + '"]').prop('checked', isChecked);
        UpdatePaymentButtons();
        calculateTotalAppliedAmount();
    });
    $('[data-name="btnApplyPayment"]').off('click');
    $('[data-name="btnApplyPayment"]').on('click', function () {
        var selectedInvoices = [];
        var selectedInvoiceDetails = [];
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        $('input[type="checkbox"][data-name="chkInvoice"]:checked').each(function () {
            var invoiceId = $(this).attr('data-invoice-id');
            selectedInvoices.push(invoiceId);
        });
        $('input[type="checkbox"][data-name="chkInvoiceDetails"]:checked').each(function () {
            var $checkbox = $(this);

            if (!$checkbox.closest('tr').hasClass('selected')) {
                var invoiceId = $(this).attr('data-invoice-id');
                var detailId = $(this).attr('data-invoicedetail-id');
                var amount = $(this).attr('data-amount');
                selectedInvoiceDetails.push({
                    Invoice_Id: invoiceId,
                    InvoiceDetail_Id: detailId,
                    Amount: amount
                });
            }

        });
        if (selectedInvoiceDetails.length === 0) {
            appCommon.ShowMessageModal("Please select at least one invoice detail to apply payment.");
            return;
        }
        appCommon.ExecuteAjaxPostMethod("/Payments/ApplyAPPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadAPPanel();
                    if (Payment_Id != null && Payment_Id != undefined) {
                        LoadPaymentPanel(Payment_Id);
                    }
                    appCommon.ShowMessageModal("Successfully Applied payments!", "Sucess");
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message);
                    }
                    else {
                        appCommon.ShowMessageModal("Unable to process the request.")
                    }
                }
            }

        }, true)
    });
    $('[data-name="btnUnApplyFullPayment"]').off('click');
    $('[data-name="btnUnApplyFullPayment"]').on('click', function () {
        var selectedInvoices = [];
        var selectedInvoiceDetails = [];
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        $('input[type="checkbox"][data-name="chkInvoice"]:checked').each(function () {
            var invoiceId = $(this).attr('data-invoice-id');
            selectedInvoices.push(invoiceId);
        });
        $('input[type="checkbox"][data-name="chkInvoiceDetails"]:checked').each(function () {
            var $checkbox = $(this);

            if (!$checkbox.closest('tr').hasClass('selected')) {
                var invoiceId = $(this).attr('data-invoice-id');
                var detailId = $(this).attr('data-invoicedetail-id');
                var amount = $(this).attr('data-amount');
                selectedInvoiceDetails.push({
                    Invoice_Id: invoiceId,
                    InvoiceDetail_Id: detailId,
                    Amount: amount
                });
            }

        });

        appCommon.ExecuteAjaxPostMethod("/Payments/UnApplyAPPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadAPPanel();
                    if (Payment_Id != null && Payment_Id != undefined) {
                        LoadPaymentPanel(Payment_Id);
                    }
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Sucess");
                    }
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message);
                    }
                    else {
                        appCommon.ShowMessageModal("Unable to process the request.")
                    }
                }
            }

        }, true)
    });
    $('[data-name="ViewInvoice"]').off('click');
    $('[data-name="ViewInvoice"]').on('click', function () {
        var PolicyDetails_Id = $(this).attr('data-policydetails-id');
        appCommon.ExecuteAjaxGetMethod("/Invoices/GenerateInvoice/" + PolicyDetails_Id, function (Response) {
            if (Response != undefined && Response != '') {
                appCommon.GenericModalFunction("Invoice", Response);
                $('[data-name="ModalGeneral"]').find('.modal-dialog');
                BindGenerateInvoiceEvents(PolicyDetails_Id);
            }
        }, true);
    });

    $('[data-name="ViewPayments"]').off('click');
    $('[data-name="ViewPayments"]').on('click', function () {
        var Invoice_Id = $(this).attr('data-invoice-id');
        var Invoice_No = $(this).attr('data-invoice-number');
        appCommon.ExecuteAjaxGetMethod("/Payments/InvoicePaymentDetailsInfo?Invoice_Id=" + Invoice_Id, function (Response) {
            if (Response != undefined && Response != '') {
                appCommon.GenericModalFunction("Payment Details For Invoice No: " + Invoice_No, Response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
            }
        }, true);
    });
}
function BindGenerateInvoiceEvents(PolicyDetails_Id) {
    $('[data-name="downloadPdf"]').on("click", function (event) {
        appCommon.ExecuteAjaxGetMethod("/Invoices/DownloadInvoice/" + PolicyDetails_Id, function (response) {
            if (typeof response != "undefined" && response != null) {
                appCommon.DownloadBase64File(response.Data);
            }
        }, true);
    });
}
function calculateTotalAppliedAmount() {
    var TotalApplied_Amount = 0;
    $('input[type="checkbox"][data-name="chkInvoiceDetails"]:checked').each(function () {
        //var Checked_Amount = parseFloat($(this).data('remaining-amount').replace(/[^0-9.-]+/g, '')) || 0;
        var Checked_Amount = parseFloat($(this).attr('data-remaining-amount')) || 0;
        TotalApplied_Amount += Checked_Amount;
    });

    $('[data-name="TotalAppliedAmount"]').text(appCommon.ApplyCurrencyFormatWithFractionDigits(TotalApplied_Amount, 2));
}
function LoadAPPanel() {
    appCommon.RenderHTMLPanel("/AccountsPayable/LoadGrid", $('[data-name="divAccountsPayableGrid"]'), function () { BindAPSearchPanelEvents(); BindAPGridPanelEvents(); }, true, null);
}
function LoadPaymentPanel(Payment_Id) {
    appCommon.ExecuteAjaxGetMethod("/Payments/SelectedPaymentsTile?Payment_Id=" + Payment_Id + "&Direction_Id=" + MasterLookup._Directions.AP, function (response) {
        if (response != null && response != undefined) {
            $('[data-name="divSelectedPayments"]').html(response);
            $('[data-name="GenericModal"]').modal('hide');
            BindAPGridPanelEvents();
        }
    }, true);
}

function UpdatePaymentButtons() {
    var checkedRows = $('[data-name="chkInvoiceDetails"]:checked');
    var hasPaid = false;
    var hasUnpaid = false;

    checkedRows.each(function () {
        var isInvoicePaid = ($(this).attr('data-invoice-paid') || "").toLowerCase().trim();
        if (isInvoicePaid === "true") {
            hasPaid = true;
        } else if (isInvoicePaid === "false") {
            hasUnpaid = true;
        }
    });

    if (checkedRows.length > 0) {
        $('[data-name="btnUnApplyFullPayment"]').toggle(hasPaid);
        $('[data-name="btnApplyPayment"]').toggle(hasUnpaid);
        $('[data-name="divAmountApplied"]').toggle(hasUnpaid);
    } else {
        $('[data-name="btnUnApplyFullPayment"]').hide();
        $('[data-name="divAmountApplied"]').hide();
        $('[data-name="btnApplyPayment"]').hide();
    }
}
