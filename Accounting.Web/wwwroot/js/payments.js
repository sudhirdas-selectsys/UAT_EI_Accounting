var defaultPageSize = MasterLookup._DefaultPageSize;
var currentSortOrder = "";
function UpdateSortUI() {
    $('.sort-option').removeClass('active');
    $('.check-icon').addClass('d-none');
    if (currentSortOrder) {
        $('.sort-option[data-order="' + currentSortOrder + '"]')
            .addClass('active')
            .find('.check-icon')
            .removeClass('d-none');
    }
}
$(function () {
    BindPaymentEvents();
    LoadPaymentsGrid(defaultPageSize, 0);

});
function BindPaginationEvents() {
    $('[data-name="btnPrevious"]').click(function () {
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) - 1;
        if (tableName == "PaymentDetails") {
            LoadPaymentsGrid(PageSize, PageNumber)
        }
    });
    $('[data-name="btnNext"]').click(function () { 
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) + 1;
        if (tableName == "PaymentDetails") {
            LoadPaymentsGrid(PageSize, PageNumber)
        }
    });
    $('[data-name="PageSize"]').change(function () {
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).val();
        var PageNumber = 0;
        if (tableName == "PaymentDetails") {
            LoadPaymentsGrid(PageSize, PageNumber)
        }
    });
}
function BindPaymentEvents() {
    InitializeFlatpickr();
    $('[data-name="btnViewPaymentDetails"]').off('click');
    $('[data-name="btnEditPaymentDetails"]').off('click');
    $('[data-name="btnDeletePaymentDetails"]').off('click');
    $('[data-name="btnViewJournalEntryDetails"]').off('click');

    $('[data-name="btnResetPayments"]').off('click');
    $('[data-name="btnSearchPayments"]').off('click');
    $('[data-name="btnAddPaymentDetails"]').off('click');

    $('[data-name="btnResetPayments"]').on('click', function () {
        var PaymentsSectionPanel = $('[data-name="frmPaymentSearch"]')
        $('[data-name="Entity_Id"]').val('');
        appCommon.ResetFieldsInPanel(PaymentsSectionPanel);
        LoadPaymentsGrid(defaultPageSize, 0);
    });

   

    $(document)
        .off('click', '.sort-option')
        .on('click', '.sort-option', function (e) {
            e.preventDefault();
            currentSortOrder = $(this).data('order');
            UpdateSortUI();
            $('[data-name="btnSearchPayments"]').trigger('click');
        });


    $('[data-name="btnSearchPayments"]').on('click', function () {
        var PaymentsGridContainer = $('[data-name="PaymentsGridContainer"]');
        var SortOrder = currentSortOrder;
        appCommon.ShowPartialPanelLoader(PaymentsGridContainer);
        var queryString = appCommon.BuildQueryStringParameters({
            PageSize: defaultPageSize,
            PageIndex: 0,
            SortOrder: SortOrder,
        });
        appCommon.SubmitForm(
            $('[data-name="frmPaymentSearch"]'),
            function (response) {
                if (response != undefined && response != null) {
                    $(PaymentsGridContainer).html(response);
                    UpdateSortUI();
                    BindPaginationEvents();
                    BindPaymentEvents();
                }
            },
            "/Search/PaymentsSearch?" + queryString,
            true,
            false
        );
    });


    //$('[data-name="btnSearchPayments"]').on('click', function () {
    //    var PaymentsGridContainer = $('[data-name="PaymentsGridContainer"]');
    //    var SortOrder = $('.sort-option.active').data('order');
    //    console.log(SortOrder);
    //    console.log(SortByColumn);
    //    var SortByColumn = $('.sort-option.active').data('column');
    //    appCommon.ShowPartialPanelLoader(PaymentsGridContainer);
    //    var queryString = appCommon.BuildQueryStringParameters({ PageSize: defaultPageSize, PageIndex: 0, SortOrder: SortOrder, SortByColumn: SortByColumn });
    //    appCommon.SubmitForm($('[data-name="frmPaymentSearch"]'), function (response) {
    //        if (response != undefined && response != null) {
    //            $(PaymentsGridContainer).html(response);
    //            BindPaginationEvents();
    //            BindPaymentEvents();
    //        }
    //    }, "/Search/PaymentsSearch" + "?" + queryString, true, false);
    //});
    $('[data-name="btnAddPaymentDetails"]').on('click', function () {
        _AppMainPanel.LoadPaymentDetails(null, function () {
            BindPaymentEvents();
        });
    });
    $('[data-name="btnViewDeletedPayments"]').on('click', function () {

        appCommon.ExecuteAjaxGetMethod("/Payments/DeletedPayments", function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Deleted Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1500px");

            }
        }, true)
    });

    $('[data-name="btnViewPaymentDetails"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        appCommon.ExecuteAjaxGetMethod("/Payments/PaymentDetailsViewOnly?Payment_Id=" + Payment_Id, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Payment Info", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
                BindPaymentEvents();
            }

        }, true);
    });


    $('[data-name="btnGeneratePaymentReceipt"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');

        appCommon.ExecuteAjaxGetMethod("/Payments/PaymentReceipt?Payment_Id=" + Payment_Id, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Payment Receipt", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
                BindPaymentEvents();
            }
        }, true);
    });

    $('[data-name="btnExportPaymentReceipt"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        var FileType_Id = $(this).attr('data-file-type-id');
        appCommon.ExecuteAjaxPostMethod("/Payments/DownloadPaymentReceipt?Payment_Id=" + Payment_Id + "&FileType_Id=" + FileType_Id, null, function (response) {
            if (response != null) {
                if (typeof response != "undefined" && response != null && response.Status) {
                    appCommon.DownloadBase64File(response.Data);
                }
            }
        }, true);

    });

    $('[data-name="btnViewJournalEntryDetails"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        appCommon.ExecuteAjaxGetMethod("/Payments/JournalEntryDetails?Payment_Id=" + Payment_Id, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Journal Entry Details - " + Payment_Id, response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                BindPaymentEvents();
            }

        }, true);
    });
    $('[data-name="btnEditPaymentDetails"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        _AppMainPanel.LoadPaymentDetails(Payment_Id, function () {
            BindPaymentEvents();
        });
    });
    $('[data-name="btnDeletePaymentDetails"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        appCommon.ShowConfirmationModal("Are you sure you want to delete this payment?", "Alert", function () {
            appCommon.ExecuteAjaxPostMethod("/Payments/DeletePaymentDetails?Payment_Id=" + Payment_Id, null, function (response) {
                if (response != null) {
                    if (response.Status) {
                        LoadPaymentsGrid(defaultPageSize, 0);
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
            }, true);
        }, function () { });
    });
    $('[data-name="EntityName"]').each(function () {
        EnableEntityNameComplete(this);
    });
    $('[data-name="btnUnapplyPayment"]').off('click').on('click', function () {
        var PaymentId = $(this).data('payment-id');
        appCommon.ShowConfirmationModal(`Are you sure you want to UnApply payment to All the Associated Invoices?`, 'Alert', function () {
            appCommon.ExecuteAjaxPostMethod('/Payments/UnApplyPaymentsByPaymentId?paymentId=' + PaymentId, null,
                function (response) {
                    if (response != null && response.Status) {
                        appCommon.ShowMessageModal(response.Message, 'Success');
                    }
                    else {
                        appCommon.ShowMessageModal(response.Message, 'Failure');
                    }
                    var PaymentsGridContainer = $('[data-name="PaymentsGridContainer"]');
                    appCommon.ShowPartialPanelLoader(PaymentsGridContainer);
                    var queryString = appCommon.BuildQueryStringParameters({ PageSize: defaultPageSize, PageIndex: 0 });
                    appCommon.SubmitForm($('[data-name="frmPaymentSearch"]'), function (response) {
                        if (response != undefined && response != null) {
                            $(PaymentsGridContainer).html(response);
                            UpdateSortUI();
                            BindPaginationEvents();
                            BindPaymentEvents();
                        }
                    }, "/Search/PaymentsSearch" + "?" + queryString, true, false);
                }, true);
        }, function () { });

    });
    $('[data-name="btnBouncePayment"]').off('click').on('click', function () {
        var PaymentId = $(this).data('payment-id');
        appCommon.ShowConfirmationModal(`Are you sure you want to bounce Payment ID ${PaymentId}? This action is unreversible.`, 'Alert', function () {
            appCommon.ExecuteAjaxPostMethod('/Payments/MaskAsBounced?paymentId=' + PaymentId, null,
                function (response) {
                    if (response != null && response.Status) {
                        appCommon.ShowMessageModal(response.Message, 'Success');
                    }
                    else {
                        appCommon.ShowMessageModal(response.Message, 'Failure');
                    }
                    LoadPaymentsGrid(defaultPageSize, 0);
                }, true);
        }, function () { });
    });
    $('[data-name="btnSavePaymentWriteoffAmount"]').off('click');
    $('[data-name="btnSavePaymentWriteoffAmount"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentWriteoff"]');
        var clonedTR = $currentRow.clone();
        var noteVal = $currentRow.find('[name="Note"]').val();
        var amountVal = parseFloat($currentRow.find('[name="Amount"]').val());
        if (isNaN(amountVal) || amountVal <= 0) {
            appCommon.ShowMessageModal("Write-off amount should be greater than zero.", "Alert");
            return;
        }
        if (!noteVal || noteVal.trim() === "") {
            appCommon.ShowMessageModal("Write-off note should not be empty.", "Alert");
            return;
        }
        var $singleRowForm = $('<form>').append(clonedTR);
        appCommon.SubmitForm($singleRowForm, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadPaymentsGrid(defaultPageSize, 0);
                    $currentRow.find('[data-name="btnCancelPaymentWriteoffAmount"]').trigger('click');
                    appCommon.ShowMessageModal(response.Message, "Message");
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        appCommon.ShowMessageModal("An error occurred while adding the payment write-off.", "Alert")
                    }
                }
            }
            appCommon.HideLoader();
        }, "/Payments/ApplyPaymentWriteoff", false, true);
    });

    $('[data-name="btnEditPaymentWriteoffAmount"]').off('click');
    $('[data-name="btnEditPaymentWriteoffAmount"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentWriteoff"]');

        $currentRow.find('[data-name="btnEditPaymentWriteoffAmount"]').hide();
        $currentRow.find('[data-name="btnSavePaymentWriteoffAmount"]').hide();
        $currentRow.find('[data-name="btnUpdatePaymentWriteoffAmount"]').show();
        $currentRow.find('[data-name="btnDeletePaymentWriteoffAmount"]').show();
        $currentRow.find('[data-name="btnCancelPaymentWriteoffAmount"]').show();

        $currentRow.find('[data-name="PaymentWriteoffAmount"]').prop('disabled', false);
        $currentRow.find('[data-name="Writeoffnote"]').prop('disabled', false);
    });

    $('[data-name="btnCancelPaymentWriteoffAmount"]').off('click');
    $('[data-name="btnCancelPaymentWriteoffAmount"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentWriteoff"]');

        $currentRow.find('[data-name="btnEditPaymentWriteoffAmount"]').show();
        $currentRow.find('[data-name="btnSavePaymentWriteoffAmount"]').hide();
        $currentRow.find('[data-name="btnUpdatePaymentWriteoffAmount"]').hide();
        $currentRow.find('[data-name="btnDeletePaymentWriteoffAmount"]').hide();
        $currentRow.find('[data-name="btnCancelPaymentWriteoffAmount"]').hide();

        $currentRow.find('[data-name="PaymentWriteoffAmount"]').prop('disabled', true);
        $currentRow.find('[data-name="Writeoffnote"]').prop('disabled', true);
    });

    $('[data-name="btnUpdatePaymentWriteoffAmount"]').off('click');
    $('[data-name="btnUpdatePaymentWriteoffAmount"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentWriteoff"]');
        var clonedTR = $currentRow.clone();
        var noteVal = $currentRow.find('[name="Note"]').val();
        var amountVal = parseFloat($currentRow.find('[name="Amount"]').val());
        if (isNaN(amountVal) || amountVal <= 0) {
            appCommon.ShowMessageModal("Write-off amount should be greater than zero.", "Alert");
            return;
        }
        if (!noteVal || noteVal.trim() === "") {
            appCommon.ShowMessageModal("Write-off note should not be empty.", "Alert");
            return;
        }
        var $singleRowForm = $('<form>').append(clonedTR);
        appCommon.SubmitForm($singleRowForm, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    LoadPaymentsGrid(defaultPageSize, 0);
                    $currentRow.find('[data-name="btnCancelPaymentWriteoffAmount"]').trigger('click');
                    appCommon.ShowMessageModal(response.Message, "Message");
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        appCommon.ShowMessageModal("An error occurred while updating the payment write-off.", "Alert")
                    }
                }
            }
            appCommon.HideLoader();
        }, "/Payments/UnApplyUpdatePaymentWriteoff", false, true);
    });

    $('[data-name="btnDeletePaymentWriteoffAmount"]').off('click');
    $('[data-name="btnDeletePaymentWriteoffAmount"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentWriteoff"]');
        var Payment_Id = $currentRow.find('input[name="Payment_Id"]').val();
        appCommon.ExecuteAjaxPostMethod("/Payments/DeletePaymentWriteoffDetails?Payment_Id=" + Payment_Id, null, function (response) {
            if (response != null) {
                if (response.Status) {
                    LoadPaymentsGrid(defaultPageSize, 0);
                    DeletePaymentWriteoff($currentRow);
                    appCommon.ShowMessageModal(response.Message, "Message");
                } else {
                    appCommon.ShowMessageModal(response.Message || "Unable to process the request.");
                }
            }
        }, true);
    });

    $('[data-name="btnDownloadBulkSample"]').off('click');
    $('[data-name="btnDownloadBulkSample"]').on('click', function () {
        appCommon.ExecuteAjaxGetMethod("/Payments/DownloadSampleFileTemplate", function (response) {
            if (response.Status == false) {
                appCommon.ShowMessageModal(response.Message);
            } else {
                window.location.href = "/Payments/DownloadSampleFileTemplate";
            }
        }, true)
    });

    $('[data-name="btnImportPayments"]').off('click');
    $('[data-name="btnImportPayments"]').on('click', function () {
        appCommon.ExecuteAjaxGetMethod("/Payments/UploadPayments", function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Upload Excel File", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "600px");
                BindPaymentEvents();
            }
        }, true);
    });

    $('[data-name="btnUploadedPayments"]').off('click');
    $('[data-name="btnUploadedPayments"]').on('click', function () {
        const fileInput = $('#paymentFile')[0];
        const file = fileInput.files[0];
        if (!file) {
            appCommon.ShowMessageModal("Please select an Excel file.", "Alert");
            return;
        }

        const fileName = file.name;
        const fileextention = fileName.split('.').pop().toLowerCase();
        const allowedExtensions = ['xlsx'];
        if (!allowedExtensions.includes(fileextention)) {
            appCommon.ShowMessageModal("Invalid file type. Please upload a .xlsx file.", "Alert");
            fileInput.value = '';
            return;
        }

        const formData = new FormData();
        formData.append("file", fileInput.files[0]);
        appCommon.ExecuteAjaxPostMethod("/Payments/UploadBulkPayments", formData, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    appCommon.ShowMessageModal("Payments uploaded successfully.", "Success");
                    LoadPaymentsGrid(defaultPageSize, 0);
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Warning");
                    }
                    else {
                        appCommon.ShowMessageModal("An error occurred while uploading the payments.", "Alert")
                    }
                }
                fileInput.value = '';
                $('[data-name="GenericModal"]').modal('hide');
            }
        }, true, true);
    });
}
function DeletePaymentWriteoff($currentRow) {
    $currentRow.find('[data-name="PaymentWriteoffAmount"]').val('').prop('disabled', false);
    $currentRow.find('[data-name="Writeoffnote"]').val('').prop('disabled', false);
    $currentRow.find('[data-name="btnSavePaymentWriteoffAmount"]').show();
    $currentRow.find('[data-name="btnEditPaymentWriteoffAmount"]').hide();
    $currentRow.find('[data-name="btnUpdatePaymentWriteoffAmount"]').hide();
    $currentRow.find('[data-name="btnCancelPaymentWriteoffAmount"]').hide();
    $currentRow.find('[data-name="btnDeletePaymentWriteoffAmount"]').hide();
}


function LoadPaymentsGrid(PageSize, PageNumber) {
    var PaymentsGridContainer = $('[data-name="PaymentsGridContainer"]');
    var SortOrder = currentSortOrder;
    appCommon.ShowPartialPanelLoader(PaymentsGridContainer);
    var queryString = appCommon.BuildQueryStringParameters({
        PageSize: PageSize,
        PageIndex: PageNumber,
        SortOrder: SortOrder
    });
    appCommon.SubmitForm(
        $('[data-name="frmPaymentSearch"]'),
        function (response) {
            if (response != undefined && response != null) {
                $(PaymentsGridContainer).html(response);
                UpdateSortUI();
                BindPaginationEvents();
                BindPaymentEvents();
            }
        },
        "/Search/PaymentsSearch?" + queryString,
        true,
        false
    );
}


//function LoadPaymentsGrid(PageSize, PageNumber) {
//    //var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
//    //appCommon.RenderHTMLPanel("/Payments/PaymentsGrid" + "?" + queryString, $('[data-name="PaymentsGridContainer"]'), function () {
//    //    BindPaymentEvents();
//    //    BindPaginationEvents();
//    //}, true, null);

//    var PaymentsGridContainer = $('[data-name="PaymentsGridContainer"]');
//    appCommon.ShowPartialPanelLoader(PaymentsGridContainer);
//    var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
//    appCommon.SubmitForm($('[data-name="frmPaymentSearch"]'), function (response) {
//        if (response != undefined && response != null) {
//            $(PaymentsGridContainer).html(response);
//            BindPaginationEvents();
//            BindPaymentEvents();
//        }
//    }, "/Search/PaymentsSearch" + "?" + queryString, true, false);
//}
function EnableEntityNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {

            var selectedEntity = $(ctrl).closest('[data-name="PaymentsSection"]').find('[data-name="EntityType_Id"]').val();
            var Name = $(ctrl).val();
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

            $(ctrl).val(ui.item.value);
            $(ctrl).parent().find('[data-name="Entity_Id"]').val(ui.item.EntityID);
        },

    });
}
$(document).off('click', '[data-name="btnPrintCheck"]')
    .on('click', '[data-name="btnPrintCheck"]', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        var AccountNumber = $(this).attr('data-account-number');
        var RoutingNumber = $(this).attr('data-routing-number');
        var SignerValue = $(this).attr('data-singer-value');
        var SignerType = $(this).attr('data-singer-type');

        appCommon.ExecuteAjaxPostMethod("/Payments/PrintCheck",
            {
                Payment_Id: Payment_Id,
                AccountNumber: AccountNumber,
                RoutingNumber: RoutingNumber,
                SignerValue: SignerValue,
                SignerType: SignerType
            },
            function (response) {
                if (response != null && response != undefined) {
                    appCommon.GenericModalFunction("Print Check", response);
                    $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "500px");
                }
            }, true);
    });

$(document).off('click', '[data-name="DownloadCheck"]')
    .on('click', '[data-name="DownloadCheck"]', function () {
        var form = $('[data-name="frmPrintCheckform"]');
        if (!appCommon.IsValidForm(form)) {
            return;
        }
        var PaymentId = $(this).attr('data-payment-Id');
        var AccountNumber = $(this).attr('data-account-number');
        var RoutingNumber = $(this).attr('data-routing-number');
        var SignerType = $(this).attr('data-singer-type');
        var SignerValue = $(this).attr('data-signer-value');
        var CheckNumber = $('[data-name="checkNumber"]').val();
        if (!AccountNumber?.trim() || !RoutingNumber?.trim() || !SignerType?.trim() || !SignerValue?.trim()) {
            appCommon.ShowMessageModal("Bank details are not found. Please update your bank details for printing checks.");
            return;
        }
        if (AccountNumber === null || AccountNumber === '' || !AccountNumber) {
            appCommon.ShowMessageModal("Please add Bank Account Number.");
            return;
        }
        if (AccountNumber.length > 22) {
            appCommon.ShowMessageModal("Account number should not be greater than 22 characters.");
            return;
        }
        
        if (RoutingNumber === null || RoutingNumber === '' || !RoutingNumber) {
            appCommon.ShowMessageModal("Please add Routing Number.");
            return;
        }
        if (RoutingNumber.length != 9) {
            appCommon.ShowMessageModal("Routing Number must be 9 digits");
            return;
        }
        
        if (SignerType === null || SignerType === '') {
            appCommon.ShowMessageModal("Please Select Signature Type.");
            return;
        }
        
        if (SignerValue === null || SignerValue === '') {
            appCommon.ShowMessageModal("Please add Signature value.");
            return;
        }
        
       
        appCommon.ShowConfirmationModal(`Are you sure you want to Print Check PaymentId: ${PaymentId}.`, 'Alert', function () {
            $('[data-name="GenericModal"]').modal('hide');
            appCommon.ExecuteAjaxPostMethod("/Payments/DownloadCheck",
                {
                    Payment_Id: PaymentId,
                    AccountNumber: AccountNumber,
                    RoutingNumber: RoutingNumber,
                    SignerValue: SignerValue,
                    CheckNumber: CheckNumber,
                    SignerType: SignerType
                },
                function (response) {
                    $(document).off('click', '[data-name="ModalMessageOk"]');
                    if (response != null && response.Status) {
                        var pdf = response.Data;
                        if (pdf != null) {
                            var DocumentFileInfo =
                            {
                                Base64String: pdf,
                                Name: 'Check_' + PaymentId + '.pdf',
                                ContentType: 'application/pdf'
                            };
                            appCommon.DownloadBase64File(DocumentFileInfo);
                            appCommon.ShowMessageModal('Check Printed Successfully', 'Success');
                            $(document).on('click', '[data-name="ModalMessageOk"]', function () {
                                LoadPaymentsGrid(defaultPageSize, 0);
                            });
                        }
                        else {
                            if (response.Message != null && response.Message != '') {
                                appCommon.ShowMessageModal(response.Message, 'Failure');
                            }
                            else {
                                appCommon.ShowMessageModal('Unable to process the request.', 'Failure');
                            }
                        }

                    }
                    else {
                        appCommon.ShowMessageModal(response.Message, 'Failure');
                    }
                }, true);
        }, function () { });
    });

$(document).off('click', '[data-name="btnBatchBillRetry"]')
$(document).on('click', '[data-name="btnBatchBillRetry"]', function () {
    var payment_Id = $(this).data('payment-id');
    var transactionDate = $(this).data('transaction-date');
    var targetURL = "/Payments/BillBatchRetry?payment_Id=" + payment_Id + "&date=" + transactionDate;
    appCommon.ShowConfirmationModal("Do you want to Retry the Bill?", "Confirmation", function () {
        appCommon.ExecuteAjaxPostMethod(targetURL, null, function (response) {
            if (response && response.Status) {
                appCommon.ShowMessageModal(response.Message, "Success");
            }
            else {
                appCommon.ShowMessageModal(response?.Message || "Unable to process request.");
            }
        }, true)
    });
})

$(document).off('click', '[data-name="btnBatchBillCancel"]')
$(document).on('click', '[data-name="btnBatchBillCancel"]', function () {
    var Payment_Id = $(this).attr('data-payment-id');
    appCommon.ShowConfirmationModal("Are you sure you want to cancel the Bill?", "Alert", function () {
        appCommon.ExecuteAjaxPostMethod("/Payments/DeletePaymentDetails?Payment_Id=" + Payment_Id+"&IsCancel=true", null, function (response) {
            if (response != null) {
                if (response.Status) {
                    LoadPaymentsGrid(defaultPageSize, 0);
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
        }, true);
    }, function () { });
});