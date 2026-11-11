$(function () {
    BindSearchPanelClickEvents();
    BindGridEvents();
    LoadARGridWithFilters();
    $('[data-name="PolicyNumber"]').each(function () {
        EnablePolicyNumberComplete(this);
    });
    $('[data-name="AgencyName"]').each(function () {
        EnableAgencyNameComplete(this);
    });
    $('[data-name="InsuredName"]').each(function () {
        EnableInsuredNameComplete(this);
    });
});
function BindGridEvents() {
    $('[data-name="ARTable"]').off('dblclick', 'tbody tr').on('dblclick', 'tbody tr', function () {
        var Invoice_Id = $(this).attr("data-invoice-id");
        var PolicyDetail_Id = $(this).attr("data-policydetail-id");
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        appCommon.ExecuteAjaxGetMethod("/AccountsReceivable/Details?policyDetail_Id=" + PolicyDetail_Id + "&Invoice_Id=" + Invoice_Id + "&Payment_Id=" + Payment_Id + "&Direction_Id=" + MasterLookup._Directions.AR, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Information", response);
                BindGridEvents();
            }
        }, true);
    });

    $('[data-name="btnApplyFullPaymentGrid"]').hide();
    $('[data-name="divAmountApplied"]').hide();
    $('[data-name="btnUnApplyFullPayment"]').hide();
    $('[data-name="chkARInvoiceDetails"]').off('click').on('click', function () {
        UpdateARPaymentButtons();
        calculateTotalAppliedAmount();
    });

    $('input[type="checkbox"][data-name="chkARInvoice"]').change(function () {
        var invoiceId = $(this).attr('data-invoice-id');
        var isChecked = $(this).is(':checked');
        $('input[type="checkbox"][data-name="chkARInvoiceDetails"][data-invoice-id="' + invoiceId + '"]').prop('checked', isChecked);
        UpdateARPaymentButtons();
        calculateTotalAppliedAmount();
    });


    $('[data-name="btnApplyFullPayment"]').off('click');
    $('[data-name="btnBackPaymentScreen"]').off('click');
    $('[data-name="btnSplitPayment"]').off('click');
    $('[data-name="btnSavePartialPayment"]').off('click');

    $('[data-name="btnApplyFullPayment"]').on('click', function () {
        var invoiceId = $(this).attr('data-invoice-id');
        var paymentId = $('[data-name="Payment_Id"]').val();
        appCommon.ExecuteAjaxPostMethod("/Payments/ApplyFullPaymentForSingleInvoice?Invoice_Id=" + invoiceId + "&Payment_Id=" + paymentId, null, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    $('[data-name="GenericModal"]').modal('hide');
                    appCommon.ShowMessageModal("Successfully Applied payments!", "Sucess");
                    LoadARGridWithFilters();
                    if (paymentId != null && paymentId != undefined) {
                        LoadPaymentPanel(paymentId);
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

    //$('[data-name="btnApplyFullPaymentGrid"]').off('click');
    //$('[data-name="btnApplyFullPaymentGrid"]').on('click', function () {
    //    var form = $('<form></form>');
    //    var table = $('[data-name="ARTable"]');
    //    var index = 0;
    //    var paymentId = $('[data-name="Payment_Id"]').val();
    //    table.find('.form-check-input:checked').each(function (i) {

    //        var row = $(this).closest('tr');
    //        if (!row.hasClass('selected')) {
    //            var invoiceId = row.attr('data-invoice-id');
    //            var invoiceNumber = row.attr('data-invoice-no');
    //            var Direction_Id = row.attr('data-direction-id');
    //            form.append(`<input type="hidden" name="List[${index}].Invoice_Id" value="${invoiceId}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].Payment_Id" value="${paymentId}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].InvoiceNumber" value="${invoiceNumber}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].Direction_Id" value="${Direction_Id}" />`);
    //            index++;
    //        }
    //    });

    //    if (form.children('input').length === 0) {
    //        appCommon.ShowMessageModal("Please select at least one invoice to apply payment.");
    //        return;
    //    }
    //    appCommon.SubmitForm(form, function (response) {
    //        if (response != null) {
    //            if (response.Status) {
    //                appCommon.ShowMessageModal("Successfully Applied payments!", "Sucess");
    //                LoadPaymentPanel(paymentId);
    //                LoadARGridWithFilters();
    //            }
    //            else {
    //                if (response.Message != null && response.Message != '') {
    //                    appCommon.ShowMessageModal(response.Message);
    //                }
    //                else {
    //                    appCommon.ShowMessageModal("Unable to process the request.")
    //                }
    //            }
    //        }
    //        appCommon.HideLoader();
    //    }, "/Payments/ApplyFullPaymentsByGrid", false, true);
    //});

    $('[data-name="btnApplyFullPaymentGrid"]').off('click');
    $('[data-name="btnApplyFullPaymentGrid"]').on('click', function () {
        var selectedInvoices = [];
        var selectedInvoiceDetails = [];
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        $('input[type="checkbox"][data-name="chkARInvoice"]:checked').each(function () {
            var invoiceId = $(this).attr('data-invoice-id');
            selectedInvoices.push(invoiceId);
        });
        $('input[type="checkbox"][data-name="chkARInvoiceDetails"]:checked').each(function () {
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
        appCommon.ExecuteAjaxPostMethod("/Payments/ApplyARPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadARGridWithFilters();
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

    //$('[data-name="btnUnApplyFullPayment"]').off('click');
    //$('[data-name="btnUnApplyFullPayment"]').on('click', function () {
    //    var form = $('<form></form>');
    //    var table = $('[data-name="ARTable"]');
    //    var index = 0;
    //    var paymentId = $('[data-name="Payment_Id"]').val();
    //    table.find('.form-check-input:checked').each(function (i) {

    //        var row = $(this).closest('tr');
    //        if (!row.hasClass('selected')) {
    //            var invoiceId = row.attr('data-invoice-id');
    //            var invoiceNumber = row.attr('data-invoice-no');
    //            var Direction_Id = row.attr('data-direction-id');
    //            form.append(`<input type="hidden" name="List[${index}].Invoice_Id" value="${invoiceId}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].Payment_Id" value="${paymentId}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].InvoiceNumber" value="${invoiceNumber}" />`);
    //            form.append(`<input type="hidden" name="List[${index}].Direction_Id" value="${Direction_Id}" />`);
    //            index++;
    //        }
    //    });

    //    if (form.children('input').length === 0) {
    //        alert('Please select at least one Invoice.');
    //        return;
    //    }
    //    appCommon.SubmitForm(form, function (response) {
    //        if (response != null) {
    //            if (response.Status) {
    //                if (response.Message != null && response.Message != '') {
    //                    appCommon.ShowMessageModal(response.Message, "Sucess");
    //                }
    //                LoadARGridWithFilters();
    //                if (paymentId != null && paymentId != undefined) {
    //                    LoadPaymentPanel(paymentId);
    //                }
    //            }
    //            else {
    //                if (response.Message != null && response.Message != '') {
    //                    appCommon.ShowMessageModal(response.Message);
    //                }
    //                else {
    //                    appCommon.ShowMessageModal("Unable to process the request.")
    //                }
    //            }
    //        }
    //        appCommon.HideLoader();
    //    }, "/Payments/UnApplyARPayments", false, true);
    //})

    $('[data-name="btnUnApplyFullPayment"]').off('click');
    $('[data-name="btnUnApplyFullPayment"]').on('click', function () {
        var selectedInvoices = [];
        var selectedInvoiceDetails = [];
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        $('input[type="checkbox"][data-name="chkARInvoice"]:checked').each(function () {
            var invoiceId = $(this).attr('data-invoice-id');
            selectedInvoices.push(invoiceId);
        });
        $('input[type="checkbox"][data-name="chkARInvoiceDetails"]:checked').each(function () {
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

        appCommon.ExecuteAjaxPostMethod("/Payments/UnApplyARPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadARGridWithFilters();
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

    $('[data-name="btnBackPaymentScreen"]').on('click', function () {
        $(this).closest('[data-name="GenericModal"]').find(".modal-title").text('Information');
        $('[data-name="btnClosePaymentWindow"]').removeClass('d-none');
        $('[data-name="btnApplyFullPayment"]').removeClass('d-none');
        $(this).removeClass('d-none');
        $('[data-name="AppyPartialAmount"]').addClass('d-none');
        $('[data-name="AppyPartialAmount"]').attr('data-disable', 'true');
        $('[data-name="SplitPaymentContainer"]').addClass('d-none');
        $('[data-name="SplitPaymentContainer"]').attr('data-disable', 'true');
        $('[data-name="btnBackPaymentScreen"]').addClass('d-none');
        $('[data-name="btnSplitPayment"]').removeClass('d-none');
        $('[data-name="btnSavePartialPayment"]').addClass('d-none');
        $('[data-name="BalanceDue"]').val($('[data-name="RemainingBalance"]').val());
    });
    $('[data-name="btnSplitPayment"]').on('click', function () {
        $(this).closest('[data-name="GenericModal"]').find(".modal-title").text('Partial Payment');
        $('[data-name="btnClosePaymentWindow"]').addClass('d-none');
        $('[data-name="btnApplyFullPayment"]').addClass('d-none');
        $(this).addClass('d-none');
        $('[data-name="AppyPartialAmount"]').addClass('d-none');
        $('[data-name="AppyPartialAmount"]').attr('data-disable', 'true');
        $('[data-name="SplitPaymentContainer"]').removeClass('d-none');
        $('[data-name="SplitPaymentContainer"]').removeAttr('data-disable');
        $('[data-name="btnBackPaymentScreen"]').removeClass('d-none');
        $('[data-name="btnSplitPayment"]').removeClass('d-none');
        $('[data-name="btnSavePartialPayment"]').removeClass('d-none');
    });
    $('[data-name="btnSavePartialPayment"]').on('click', function () {
        var paymentId = $('[data-name="Payment_Id"]').val();
        var form = $('[data-name="frmInvoicePaymentInfo"]');
        if (appCommon.IsValidForm(form)) {
            appCommon.RemoveCurrencyFormatForPanel(form);
            appCommon.SubmitForm(form, function (response) {
                if (response != null) {
                    if (response.Status) {
                        $('[data-name="GenericModal"]').modal('hide');
                        LoadPaymentPanel(paymentId);
                        LoadARGridWithFilters();
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
                appCommon.HideLoader();
            }, "/Payments/SavePartialPayment", false, true);
        }
    });
    $('[data-name="SplitPaymentContainer"] input[type="checkbox"]').change(function () {
        if ($(this).prop('checked')) {
            $(this).closest('td').find('[data-name="IsInvliceLineItemPaid"]').val('True');
        } else {
            $(this).closest('td').find('[data-name="IsInvliceLineItemPaid"]').val('False');
        }
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
    $('input[type="checkbox"][data-name="chkARInvoiceDetails"]:checked').each(function () {
        var Checked_Amount = parseFloat($(this).attr('data-remaining-amount')) || 0;
        TotalApplied_Amount += Checked_Amount;
    });
    $('[data-name="TotalAppliedAmount"]').text(appCommon.ApplyCurrencyFormatWithFractionDigits(TotalApplied_Amount, 2));
}
function BindSearchPanelClickEvents() {
    $('[data-name="EntityName"]').each(function () {
        EnableNameComplete(this);
    });

    $('[data-name="CollapseARRows"]').off('click');
    $('[data-name="CollapseARRows"]').on('click', function () {
        $('.accordion-collapse').collapse('hide');
    });

    $('[data-name="ExpandARRows"]').off('click');
    $('[data-name="ExpandARRows"]').on('click', function () {
        $('.accordion-collapse').collapse('show');
    });

    $('[data-name="btnSearch"]').off('click');
    $('[data-name="btnSearch"]').on('click', function () {
        var ARContainer = $('[data-name="ARContainer"]');
        appCommon.ShowPartialPanelLoader(ARContainer);
        appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
            if (response != undefined && response != null) {
                $(ARContainer).html(response);
                BindGridEvents();
            }
        }, "/Search/SearchAR", true, false);
    });

    $('[data-name="btnSelectPayment"]').off('click');
    $('[data-name="btnSelectPayment"]').on('click', function () {
        appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
            if (response != undefined && response != null) {
                //LoadARGridWithFilters();
                appCommon.GenericModalFunction("Select Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                appCommon.HideLoader();
            }
        }, "/Search/PaymentsSearchForSelection", false, true);

    });
}
function EnableNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;

    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var selectedEntity = $('[data-name="EntityType_Id"]').val();
            var Name = $('[data-name="EntityName"]').val();
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
            $('[data-name="EntityName"]').val(ui.item.value);
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
function LoadARGridWithFilters() {
    var ARContainer = $('[data-name="ARContainer"]');
    appCommon.ShowPartialPanelLoader(ARContainer);
    appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
        if (response != undefined && response != null) {
            $(ARContainer).html(response);
            BindGridEvents();
        }
    }, "/Search/SearchAR", true, false);
}
function LoadPaymentPanel(Payment_Id) {
    appCommon.ExecuteAjaxGetMethod("/Payments/SelectedPaymentsTile?Payment_Id=" + Payment_Id + "&Direction_Id=" + MasterLookup._Directions.AR, function (response) {
        if (response != null && response != undefined) {
            $('[data-name="divSelectedPayments"]').html(response);
            $('[data-name="GenericModal"]').modal('hide');
            BindGridEvents();
        }
    }, true);
}
function UpdateARPaymentButtons() {
    var checkedRows = $('[data-name="chkARInvoiceDetails"]:checked');
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
        $('[data-name="divAmountApplied"]').toggle(hasUnpaid);
        $('[data-name="btnApplyFullPaymentGrid"]').toggle(hasUnpaid);
    } else {
        $('[data-name="btnUnApplyFullPayment"]').hide();
        $('[data-name="divAmountApplied"]').hide();
        $('[data-name="btnApplyFullPaymentGrid"]').hide();
    }
}