//var PartialInvoiceDetails = PartialInvoiceDetails || [];
var currentSortOrder = "";
function UpdateSortsUI() {
    $('.sort-options').removeClass('active');
    $('.check-icon').addClass('d-none');
    if (currentSortOrder) {
        $('.sort-options[data-order="' + currentSortOrder + '"]')
            .addClass('active')
            .find('.check-icon')
            .removeClass('d-none');
    }
}
$(document)
    .off('click', '.sort-options')
    .on('click', '.sort-options', function (e) {
        e.preventDefault();
        currentSortOrder = $(this).data('order');
        UpdateSortsUI();
        $('[data-name="btnApplyFilters"]').trigger('click');
        $('[data-name="btnSearch"]').trigger('click');
    });
var isManualPayableEdit = false;
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
    $('[data-name="InvoiceNo"]').each(function () {
        EnableInvoiceNumberComplete(this);
    });
    /*LoadARGridWithFilters();*/
    BindARAPCommonEvents();
    /* LoadAPPanel();*/
    /*LoadAPWithFilters();*/
    BindAPSearchPanelEvents();
});

function EnableInvoiceNumberComplete(ctrl) {
    if ($(ctrl).attr("type") !== "text") return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var invoice = $(ctrl).val();
            $.ajax({
                url: "/api/SearchAPI/GetInvoiceNumbers",
                data: { InvoiceNumber: invoice },
                success: function (data) {
                    if (data && data.Data && data.Data.length) {
                        response($.map(data.Data, function (item) {
                            return { label: item.Name, value: item.Name };
                        }));
                    } else {
                        response([]);
                    }
                },
                error: function () {
                    response([]);
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            $(ctrl).val(ui.item.value);
        }
    })
        .autocomplete("widget")
        .addClass("highlight");

}

var isARLoading = false;
var lastScrollTopAR = 0;
var loadedPagesAR = [];   // track all loaded pages
var maxPageAR = null;     // will be set when no more data

var isAPLoading = false;
var lastScrollTopAP = 0;
var loadedPagesAP = [];   // track all loaded pages
var maxPageAP = null;     // will be set when no more data


$('[data-name="ARContainer"]').on('scroll', function () {
    var container = $(this);
    var currentScrollTop = container.scrollTop();

    // Scroll down
    if (!isARLoading && currentScrollTop > lastScrollTopAR && currentScrollTop + container.innerHeight() >= container[0].scrollHeight - 50) {

        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var nextPage = currentPage + 1;

        // Skip if already loaded OR beyond maxPageAR
        if (loadedPagesAR.indexOf(nextPage) === -1 && (!maxPageAR || nextPage <= maxPageAR)) {
            isARLoading = true;
            $('[name="pageIndex"]').val(nextPage);
            $('[name="pageSize"]').val(20);
            LoadARGridWithFiltersForScroll("down", nextPage);
        }
    }

    // Scroll up
    if (!isARLoading && currentScrollTop < lastScrollTopAR && currentScrollTop <= 5) {
        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var prevPage = currentPage - 1;

        // Stop at page 1 and skip if already loaded
        if (prevPage > 0 && loadedPagesAR.indexOf(prevPage) === -1) {
            isARLoading = true;
            $('[name="pageIndex"]').val(prevPage);
            $('[name="pageSize"]').val(20);
            LoadARGridWithFiltersForScroll("up", prevPage);
        }
    }

    lastScrollTopAR = currentScrollTop;
});

function LoadARGridWithFiltersForScroll(direction, pageIndex) {
    var ARContainer = $('[data-name="ARContainer"]');

    var oldHeight = ARContainer[0].scrollHeight;
    var oldScrollTop = ARContainer.scrollTop();
    var SortOrder = currentSortOrder;
    var queryString = appCommon.BuildQueryStringParameters({
        SortOrder: SortOrder
    });
    ShowLoaderRowForScroll(direction, "AR");
    appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
        if (response) {
            if (response.indexOf("No records found") !== -1) {
                console.log("No records found, setting maxPageAR");
                // mark this as the last page
                maxPageAR = pageIndex;
            } else {
                if (direction === "down") {
                    ARContainer.append(response);
                } else if (direction === "up") {
                    ARContainer.prepend(response);

                    // preserve scroll position after prepend
                    var newHeight = ARContainer[0].scrollHeight;
                    ARContainer.scrollTop(oldScrollTop + (newHeight - oldHeight));
                }
                PartialInvoiceDetails = [];
                UpdateSortsUI();
                BindARAPCommonEvents();
                ApplyInsuredNameSorting();
                $('[data-name="btnUploadInvoicePayments"]').removeClass('d-none');
            }

            // Mark this page as loaded
            if (loadedPagesAR.indexOf(pageIndex) === -1) {
                loadedPagesAR.push(pageIndex);
            }
        }
        RemoveLoaderRowForScroll("AR");
        isARLoading = false;
        lastScrollTopAR = ARContainer.scrollTop();
    }, "/Search/SearchAR?" + queryString, true, false);
}

function ShowLoaderRowForScroll(direction, type) {
    //var loaderRow = `<div class="loading-scroll-row" style="width:100%;display:flex;align-items:center;justify-content:center;padding:20px 0;">
    //<div style="width:40px;height:40px;border:4px solid #f3f3f3;border-top:4px solid #3498db;border-radius:50%;animation: spin 1s linear infinite;"></div>
    //<style>
    //    @keyframes spin {
    //      0% { transform: rotate(0deg); }
    //      100% { transform: rotate(360deg); }
    //    }
    //</style>
    //</div>`;

    var loaderRow = `
    <div class="accordion-item loading-scroll-row" style="padding:10px; border:1px solid #ddd; margin-bottom:10px;">
        <div style="display:flex; flex-wrap:nowrap; justify-content:flex-start; gap:12px;">
            <!-- Invoice Number / Checkbox column -->
            <div style="width:150px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:20px"></div>
            <!-- Agency Name -->
            <div style="width:150px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:60px"></div>
            <!-- Insured Name -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:20px"></div>
            <!-- Policy Number -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:40px"></div>
            <!-- Transaction Type -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:45px"></div>
            <!-- Billing Type -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:0px"></div>
            <!-- Billing Date -->
            <div style="width:100px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:5px"></div>
            <!-- Due Date -->
            <div style="width:100px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:5px"></div>
            <!-- Amount -->
            <div style="width:115px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:10px"></div>
            <!-- Balance -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:20px"></div>
            <!-- Amount -->
            <div style="width:125px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:70px"></div>
        </div>
        <style>
          @keyframes shimmer {
            0% { background-position:-200% 0; }
            100% { background-position:200% 0; }
          }
        </style>
    </div>`;

    if (type == "AR") {
        if (direction === "down") {
            $('[data-name="ARContainer"]').append(loaderRow);
        } else {
            $('[data-name="ARContainer"]').prepend(loaderRow);
        }
    } else {
        if (direction === "down") {
            $('[data-name="divAccountsPayableGrid"]').append(loaderRow);
        } else {
            $('[data-name="divAccountsPayableGrid"]').prepend(loaderRow);
        }
    }
}

function RemoveLoaderRowForScroll(type) {
    if (type == "AR") {
        $('[data-name="ARContainer"] .loading-scroll-row').remove();
    } else {
        $('[data-name="divAccountsPayableGrid"] .loading-scroll-row').remove();
    }
}


function ClearloadedPagesAR() {
    $('[name="pageIndex"]').val(1);
    loadedPagesAR = [];
    loadedPagesAR.push(1);
}

function ClearloadedPagesAP() {
    $('[name="pageIndex"]').val(1);
    loadedPagesAP = [];
    loadedPagesAP.push(1);
}
function LoadARGridWithFilters() {
    var ARContainer = $('[data-name="ARContainer"]');
    var SortOrder = currentSortOrder;
    appCommon.ShowPartialPanelLoader(ARContainer);
    var queryString = appCommon.BuildQueryStringParameters({
        SortOrder: SortOrder
    });
    appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
        if (response != undefined && response != null) {
            $(ARContainer).html(response);
            UpdateSortsUI();
            BindARAPCommonEvents();
            ApplyInsuredNameSorting();
            $('[data-name="btnUploadInvoicePayments"]').removeClass('d-none');
            loadedPagesAR.push(1);
        }
    }, "/Search/SearchAR?" + queryString, true, false);
}
function BindARAPCommonEvents() {
    $('[data-name="btnResetAR"]').off('click');
    $('[data-name="btnReset"]').off('click');
    $('[data-name="btnResetAR"]').on('click', function () {
        var ARSectionPanel = $('[data-name="frmSearch"]')
        $('[data-name="Entity_Id"]').val('');
        appCommon.ResetFieldsInPanel(ARSectionPanel);
        var multiSelect = ARSectionPanel.find('.custom-multi-selection');

        multiSelect.find('input[type="checkbox"][data-name="lineitem-options"]').prop('checked', false);
        multiSelect.find('.chip').remove();
        multiSelect.find('.default-text').show();
        multiSelect.find('input[name="LineItemList"]').val('');
        ClearloadedPagesAR();
        LoadARGridWithFilters();
        $('[data-name="chkAllInvoices"]').addClass("d-none");
    });
    $('[name="OutstandingBalances"]').off('change');
    $('[name="OutstandingBalances"]').on('change', function () {
        var Direction_Id = $('[data-name="Direction_Id"]').val();
        if (Direction_Id == MasterLookup._Directions.AR) {
            ClearloadedPagesAR();
            LoadARGridWithFilters();
        }
        else if (Direction_Id == MasterLookup._Directions.AP) {
            ClearloadedPagesAP();
            LoadAPWithFilters();
        }
    });
    $('[data-name="btnReset"]').on('click', function () {
        var APSectionPanel = $('[data-name="frmAdditionalFilters"]')
        $('[data-name="Entity_Id"]').val('');
        appCommon.ResetFieldsInPanel(APSectionPanel);
        var multiSelect = APSectionPanel.find('.custom-multi-selection');

        multiSelect.find('input[type="checkbox"][data-name="lineitem-options"]').prop('checked', false);
        multiSelect.find('.chip').remove();
        multiSelect.find('.default-text').show();
        multiSelect.find('input[name="LineItemList"]').val('');
        ClearloadedPagesAP();
        LoadAPWithFilters();
        $('[data-name="chkAllInvoices"]').addClass("d-none");
    });

    $('[data-name="btnSearch"]').off('click');
    $('[data-name="btnSearch"]').on('click', function () {
        ClearloadedPagesAR();
        var ARContainer = $('[data-name="ARContainer"]');
        var SortOrder = currentSortOrder;
        appCommon.ShowPartialPanelLoader(ARContainer);
        var queryString = appCommon.BuildQueryStringParameters({
            SortOrder: SortOrder
        });
        appCommon.SubmitForm($('[data-name="frmSearch"]'), function (response) {
            if (response != undefined && response != null) {
                $(ARContainer).html(response);
                UpdateSortsUI();
                BindARAPCommonEvents();
                ApplyInsuredNameSorting();
                $('[data-name="chkAllInvoices"]').removeClass("d-none");
            }
        }, "/Search/SearchAR?" + queryString, true, false);
    });
    $('[data-name="btnApplyPayment"]').hide();
    $('[data-name="divAmountApplied"]').hide();
    $('[data-name="btnUnApplyFullPayment"]').hide();
    $('[data-name="ViewInvoice"]').off('click');
    $('[data-name="ViewInvoice"]').on('click', function () {
        var PolicyDetails_Id = $(this).attr('data-policydetails-id');
        var InstallmentNo = $(this).attr('data-installmentNo');
        appCommon.ExecuteAjaxGetMethod("/Invoices/GenerateInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (Response) {
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
        var PolicyDetails_Id = $(this).attr('data-policydetails-id');
        appCommon.ExecuteAjaxGetMethod("/Payments/InvoicePaymentDetailsInfo?Invoice_Id=" + Invoice_Id + "&PolicyDetails_Id=" + PolicyDetails_Id, function (Response) {
            if (Response != undefined && Response != '') {
                appCommon.GenericModalFunction("Payment Details For Invoice No: " + Invoice_No, Response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
            }
        }, true);
    });
    $('[data-name="MarkAsFinanced"]').off('click');
    $('[data-name="MarkAsFinanced"]').on('click', function () {
        var $this = $(this);
        var Invoice_Id = $(this).closest('ul').attr('data-invoice-id');
        var Is_Financed = $(this).attr('data-is-financed');
        var title = "Confirm Mark this Invoice as Financed"
        var message = "Are you sure you want to Mark this as Financed?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod('/AccountsReceivable/MarkFinanced?Invoice_Id=' + Invoice_Id + '&Is_Financed=' + Is_Financed, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Successfull Marked this Invoice as Financed.', title);
                    $this.closest('ul').find('[data-name="UnMarkAsFinanced"]').removeClass('d-none');
                    $this.addClass('d-none');
                    $this.closest('.accordion-item').find('[data-name="FinanceBadge"]').removeClass('d-none');
                }
                else {
                    appCommon.ShowMessageModal("Unable to Mark it as Financed. Please try again later");
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="PaymentReminder"]').off('click');
    $('[data-name="PaymentReminder"]').on('click', function () {
        var $this = $(this);
        var schedulerClientJobId = $(this).attr('data-scheduler-client-job-id');
        var Invoice_Id = $(this).closest('ul').attr('data-invoice-id');
        var title = $this.html();
        var message = "Are you sure you want to proceed to send the reminder?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod('/AccountsReceivable/SendPaymentReminder?Invoice_Id=' + Invoice_Id + '&Job_Id=' + schedulerClientJobId, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Sucessfully sent the payment reminder', title);
                }
                else {
                    appCommon.ShowMessageModal("Unable to send payment reminder. Please try again later", title);
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="UnMarkAsFinanced"]').off('click');
    $('[data-name="UnMarkAsFinanced"]').on('click', function () {
        var $this = $(this);
        var Invoice_Id = $(this).closest('ul').attr('data-invoice-id');
        var Is_Financed = $(this).attr('data-is-financed');
        var title = "Confirm Unmark this Invoice as Financed"
        var message = "Are you sure you want to Unmark this as Financed?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod('/AccountsReceivable/MarkFinanced?Invoice_Id=' + Invoice_Id + '&Is_Financed=' + Is_Financed, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Successfull Unmarked this Invoice as Financed.', title);
                    $this.closest('ul').find('[data-name="MarkAsFinanced"]').removeClass('d-none');
                    $this.addClass('d-none');
                    $this.closest('.accordion-item').find('[data-name="FinanceBadge"]').addClass('d-none');
                }
                else {
                    appCommon.ShowMessageModal("Unable to make Unmark it as Financed. Please try again later");
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="SubmissionDocuments"]').off('click');
    $('[data-name="SubmissionDocuments"]').on('click', function () {
        var $this = $(this);
        var FileType_Id = $(this).attr('data-file-type-id');
        var PolicyNumber = $(this).attr('data-policy-number');
        var SubmissionId = $(this).attr('data-submission-id');
        var TransactionType = $(this).attr('data-transaction-type');
        appCommon.ExecuteAjaxPostMethod('/AccountsReceivable/SubmissionDocuments?Submission_Id=' + SubmissionId + '&TransactionType=' + TransactionType, null, function (response) {
            if (response) {
                appCommon.GenericModalFunction("Submission Documents", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
            }
            else {
                appCommon.ShowMessageModal("Unable load the View. Please try again later");
            }
        }, true, true)
    });
    $(document).off("click", '[data-name="DownLoadFileDataPDF"]')
        .on("click", '[data-name="DownLoadFileDataPDF"]', function () {
            var DTDDocumentId = $(this).attr('data-dtd-document-id');
            var fileName = $(this).attr('data-filename') || "Document.pdf";
            appCommon.ExecuteAjaxGetMethod("/AccountsReceivable/GetFileData?DTDDocumentId=" + DTDDocumentId, function (response) {
                if (typeof response != "undefined" && response != null) {
                    var base64Data = response.Data;
                    var DocumentFileInfo =
                    {
                        Base64String: base64Data,
                        Name: fileName + '.pdf',
                        ContentType: 'application/pdf'
                    };
                    appCommon.DownloadBase64File(DocumentFileInfo);
                }
            }, true);
        });
    $('[data-name="downloadPdf"]').on("click", function (event) {

        var PolicyDetails_Id = $(this).attr('data-policydetails-id');
        var InstallmentNo = $(this).attr('data-installmentNo');
        appCommon.ExecuteAjaxGetMethod("/Invoices/DownloadInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (response) {
            if (typeof response != "undefined" && response != null) {
                appCommon.DownloadBase64File(response.Data);
            }
        }, true);
    });
    $('[data-name="ExpandRows"]').off('click');
    $('[data-name="ExpandRows"]').on('click', function () {
        $('.accordion-collapse').collapse('show');
    });
    $('[data-name="CollapseRows"]').off('click');
    $('[data-name="CollapseRows"]').on('click', function () {
        $('.accordion-collapse').collapse('hide');
    });
    $('[data-name="btnUnApplyFullPayment"]').off('click');
    $('[data-name="btnUnApplyFullPayment"]').on('click', function () {
        var selectedInvoices = [];
        var selectedInvoiceDetails = [];
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        var Direction_Id = $('[data-name="Direction_Id"]').val();
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
                var remainingbalance = $(this).attr('data-remaining-amount');
                selectedInvoiceDetails.push({
                    Invoice_Id: invoiceId,
                    InvoiceDetail_Id: detailId,
                    Amount: amount,
                    RemainingBalance: remainingbalance
                });
            }

        });

        appCommon.ExecuteAjaxPostMethod("/Payments/UnApplyPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id, Direction_Id: Direction_Id }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    if (Direction_Id == MasterLookup._Directions.AR) {
                        LoadARGridWithFilters();
                    }
                    else if (Direction_Id == MasterLookup._Directions.AP) {
                        LoadAPWithFilters();
                    }
                    if (Payment_Id != null && Payment_Id != undefined) {
                        LoadPaymentPanel(Payment_Id, Direction_Id);
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
$('[data-name="btnSelectPayment"]').off('click');
$('[data-name="btnSelectPayment"]').on('click', function () {
    var queryString = appCommon.BuildQueryStringParameters({
        OutstandingBalances: true
    });
    appCommon.SubmitForm($('[data-name="selectPayments"]'), function (response) {
        if (response != undefined && response != null) {
            appCommon.GenericModalFunction("Select Payments", response);
            $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
            appCommon.HideLoader();
            BindPaymentsTileEvents();
        }
    }, "/Search/PaymentsSearchForSelection?" + queryString, false, true);
});

    $('[data-name="btnUploadInvoicePayments"]').off('click');
    $('[data-name="btnUploadInvoicePayments"]').on('click', function () {
        appCommon.ExecuteAjaxGetMethod("/Invoices/UploadInvoicesPayment", function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Upload CSV/Excel File", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "600px");
                BindARAPCommonEvents();
                BindAPSearchPanelEvents();
                //if (PartialInvoiceDetails.length > 0) {
                //    $('[data-name="PayableAmount"]').trigger('focusout');
                //}
                $('input[type="checkbox"][data-name="chkInvoice"]:checked').trigger('change');
            }
        }, true);
    });

    $('[data-name="btnUploadedInvoicesPayments"]').off('click');
    $('[data-name="btnUploadedInvoicesPayments"]').on('click', function () {
        const fileInput = $('#invoicepaymentFile')[0];
        const file = fileInput.files[0];

        if (!file) {
            appCommon.ShowMessageModal("Please select a file", "Alert");
            return;
        }
        if (fileInput && fileInput.files.length > 0) {
            ReadExcelFile(fileInput.files[0]);
        }
        let formData = new FormData();
        formData.append("file", file);
        formData.append("Direction_Id", $('[data-name="Direction_Id"]').val());

        appCommon.ExecuteAjaxPostMethod('/Payments/UploadInvoicePaymentFile', formData, function (html) {
            appCommon.GenericModalFunction("Map Columns", html);

        }, true, true)
    })
    var excelJsonData = [];
    function ReadExcelFile(file) {

        isExcelLoaded = false;

        const reader = new FileReader();

        reader.onload = function (e) {

            const data = new Uint8Array(e.target.result);
            const workbook = XLSX.read(data, { type: 'array' });
            const sheet = workbook.Sheets[workbook.SheetNames[0]];

            excelJsonData = XLSX.utils.sheet_to_json(sheet, { defval: "" });
            isExcelLoaded = true; // ✅ IMPORTANT
        };

        reader.readAsArrayBuffer(file);
    }
    $(document).off('click', '[data-name="submitcolmapping"]');
    $(document).on('click', '[data-name="submitcolmapping"]', function () {
        if (!isExcelLoaded || excelJsonData.length === 0) {
            appCommon.ShowMessageModal("Excel file is still loading. Please wait.", "Alert");
            return;
        }
        let mapping = {};
        let used = {};

        $('.system-field-dd').each(function () {

            const systemField = $(this).val();
            const excelColumn = $(this)
                .closest('tr')
                .find('input[name="ExcelColumns[]"]').val();

            if (!systemField) return;

            if (used[systemField]) {
                appCommon.ShowMessageModal(`${systemField} mapped more than once`, "Alert");
                throw "Duplicate mapping";
            }

            used[systemField] = true;

            if (systemField === "Invoice #") mapping.InvoiceNumber = excelColumn;
            if (systemField === "Policy #") mapping.PolicyNumber = excelColumn;
            if (systemField === "Payment Amount") mapping.PaymentAmount = excelColumn;
        });

        if (!mapping.PaymentAmount) {
            appCommon.ShowMessageModal("Payment Amount is required", "Alert");
            return;
        }

        if (!mapping.InvoiceNumber && !mapping.PolicyNumber) {
            appCommon.ShowMessageModal("Invoice # or Policy # is required", "Alert");
            return;
        }
        const $form = $('#columnMappingForm');
        const tempFileId = $form.find('input[name="TempFileId"]').val();
        const directionId = $form.find('input[name="Direction_Id"]').val();
        const Excel_Id = $form.find('input[name="Excel_Id"]').val();

        if (!tempFileId || !directionId) {
            appCommon.ShowMessageModal("Session expired. Please re-upload the file.", "Alert");
            return;
        }
        const payload = {
            TempFileId: tempFileId,
            Direction_Id: directionId,
            ColumnMapping: mapping,
            Excel_Id: Excel_Id
        };
        appCommon.ExecuteAjaxPostMethod(
            "/Payments/SearchInvoicesPaymentDetailsFromExcel",
            payload,
            function (Jsonresponse) {

                if (!Jsonresponse) {
                    appCommon.ShowMessageModal("Unable to process request");
                    return;
                }
                else if (Jsonresponse.uploadedInvoicesPayment != null && Jsonresponse.Direction_Id != null) {
                    appCommon.ExecuteAjaxPostMethod("/Search/SearchInvoicesPaymentDetails", { uploadedInvoicesPayment: Jsonresponse.uploadedInvoicesPayment, Direction_Id: Jsonresponse.Direction_Id }, function (response) {
                        if (response) {
                            if (Jsonresponse.Direction_Id == MasterLookup._Directions.AR) {
                                var ARContainer = $('[data-name="ARContainer"]');
                                $(ARContainer).html(response);
                                BindARAPCommonEvents();
                                BindAPSearchPanelEvents();
                                FilteredUploadedInvoicespayment(excelJsonData, mapping);
                            }
                            else if (Jsonresponse.Direction_Id == MasterLookup._Directions.AP) {
                                var APContainer = $('[data-name="divAccountsPayableGrid"]');
                                $(APContainer).html(response);
                                BindARAPCommonEvents();
                                BindAPSearchPanelEvents();
                                FilteredUploadedInvoicespayment(excelJsonData, mapping);
                            }
                            $('[data-name="PayableAmount"]').trigger('change');

                        }
                        else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }

                    }, true);
                }
            }, true);
        $(this).closest('.modal').modal('hide');
    });

    $('[data-name="chkInvoice"]').on('change', function () {
        isManualPayableEdit = false;
        let invoiceId = $(this).data('invoice-id');
        let isChecked = $(this).is(':checked');
        let remainingAmt = Number(Number($(this).data('remaining-amount')).toFixed(2));
        let payableInput = $(`input[data-name="PayableAmount"][data-invoice-id="${invoiceId}"]`);

        if (isChecked) {
            // Set PayableAmount to remaining amount & disable input
            payableInput.val(remainingAmt.toFixed(2)).prop('disabled', true);
            // check all line items under this invoice
            $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]`).prop('checked', true);
        } else {
            // Reset PayableAmount and enable input
            payableInput.val('0').prop('disabled', false);
            // Uncheck all line items under this invoice
            $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]`).prop('checked', false);
        }
        UpdatePaymentButtons();
        UpdateTotalAppliedAmount();
    });
    // Line-item checkbox click handler
    $('[data-name="chkInvoiceDetails"]').off('change').on('change', function () {
        let invoiceId = $(this).data('invoice-id');
        let isChecked = $(this).is(':checked');
        let isAccountsPayable = $(this).data('accounts-payable').toString() === 'true';
        let remainingAmt = Number(Number($(this).data('remaining-amount')).toFixed(2));
        let payableInput = $(`input[data-name="PayableAmount"][data-invoice-id="${invoiceId}"]`);
        let lobId = $(this).attr('data-lob-id');
        let lineItemName = $(this).attr('data-lineitem-name');
        const isIndependentSelection = $('[data-name="CommissionSelectionModeToggle"]').is(':checked');
        const billingType = ($(this).attr('data-billing-type') || '').trim().toLowerCase();
        const getPairedCheckbox = () => {
            let pairedName = null;
            if (billingType === 'insured') {
                pairedName = lineItemName === MasterLookup._CustomLineItems.CarrierPremium
                    ? MasterLookup._CustomLineItems.TotalCommission
                    : MasterLookup._CustomLineItems.CarrierPremium;
            }
            else {
                 pairedName = lineItemName === MasterLookup._CustomLineItems.CarrierPremium
                    ? MasterLookup._CustomLineItems.HouseCommission
                    : MasterLookup._CustomLineItems.CarrierPremium;
            }
            return $(`input[data-name="chkInvoiceDetails"][data-lineitem-name="${pairedName}"][data-invoice-id="${invoiceId}"][data-lob-id="${lobId}"]`);
        };

        if (isChecked) {
            // If Accounts Payable, allow only one line item checked per invoice
            //if (isAccountsPayable) {
            //    let checkedCount = $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`).length;
            //    if (checkedCount > 1) {
            //        $(this).prop('checked', false);
            //        appCommon.ShowMessageModal("Please select only one line item per invoice for partial payment application.");
            //        return;
            //    }
            //}
            if (!isIndependentSelection && (lineItemName === MasterLookup._CustomLineItems.CarrierPremium || lineItemName === MasterLookup._CustomLineItems.HouseCommission || (billingType === 'insured' && lineItemName === MasterLookup._CustomLineItems.TotalCommission))) {
                const $pairedCheckbox = getPairedCheckbox();
                if (!$pairedCheckbox.is(':checked')) {
                    $pairedCheckbox.prop('checked', true);
                    const pairedAmount = Number(Number($pairedCheckbox.attr("data-remaining-amount")).toFixed(2)) || 0;
                    remainingAmt += pairedAmount;
                }
            }

            //// Add line item's remaining amount to PayableAmount input value
            //let currentVal = parseFloat(payableInput.val()) || 0;
            //let newVal = currentVal + remainingAmt;
            //payableInput.val(newVal.toFixed(2)).prop('disabled', false);
            if (!isManualPayableEdit) {
                let currentVal = Number(Number(payableInput.val()).toFixed(2)) || 0;
                let newVal = currentVal + remainingAmt;
                payableInput.val(newVal.toFixed(2)).prop('disabled', false);
            }

        }
        else {
            if (!isIndependentSelection && (lineItemName === MasterLookup._CustomLineItems.CarrierPremium || lineItemName === MasterLookup._CustomLineItems.HouseCommission || (billingType === 'insured' && lineItemName === MasterLookup._CustomLineItems.TotalCommission))) {
                const $pairedCheckbox = getPairedCheckbox();
                if ($pairedCheckbox.is(':checked')) {
                    $pairedCheckbox.prop('checked', false);
                    const pairedAmount = Number(Number($pairedCheckbox.attr("data-remaining-amount")).toFixed(2)) || 0;
                    remainingAmt += pairedAmount;
                }
            }
            if (!isManualPayableEdit) {
                let currentVal = Number(Number(payableInput.val()).toFixed(2)) || 0;
                let newVal = currentVal - remainingAmt;
                if (newVal < 0) newVal = 0;
                payableInput.val(newVal.toFixed(2)).prop('disabled', false);
            }
            //// When unchecked, subtract this line item's amount from PayableAmount
            //let currentVal = parseFloat(payableInput.val()) || 0;
            //let newVal = currentVal - remainingAmt;
            //if (newVal < 0) newVal = 0;
            //payableInput.val(newVal.toFixed(2)).prop('disabled', false);
        }
        UpdatePaymentButtons();
        UpdateTotalAppliedAmount();
    });
    //$('[data-name="PayableAmount"]').on('blur', function () {
    //    var invoiceId = $(this).data('invoice-id');
    //    let isAccountsPayable = $(this).attr('data-accounts-payable').toString() === 'true';
    //    var value = $(this).val().trim();
    //    if (value !== "" && !isAccountsPayable) {
    //         $('[data-name="chkInvoiceDetails"][data-invoice-id="' + invoiceId + '"]').prop('checked', true);
    //         $(this).prop("checked", true).trigger("change");
    //    }
    //});
    $('[data-name="PayableAmount"]').off('change');
    $('[data-name="PayableAmount"]').on('change', function () {
        isManualPayableEdit = true;
        let invoiceId = $(this).data('invoice-id');
        let amountStr = $(this).val();
        let amount = Number(Number(amountStr).toFixed(2)) || 0;
        let remainingAmt = Number(Number($(this).data('remaining-amount')).toFixed(2)) || 0;
        let isAccountsPayable = $(this).attr('data-accounts-payable').toString() === 'true';
        var isInvoicePaid = ($(this).attr('data-invoice-paid') || "").toLowerCase().trim();
        // Sign validation: amount and remaining must have same sign
        if (amount !== 0 && remainingAmt !== 0 && Math.sign(amount) !== Math.sign(remainingAmt)) {
            appCommon.ShowMessageModal(
                "Payable amount must have the same sign as the remaining balance."
            );
            UpdateTotalAppliedAmount();
            return;
        }
        //if (amount > remainingAmt) {
        //    appCommon.ShowMessageModal("Payable amount cannot exceed the remaining balance.");
        //    let checkedSum = 0;
        //    let checkedDetails = $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`);
        //    if (checkedDetails.length > 0) {
        //        checkedDetails.each(function () {
        //            checkedSum += Number(Number($(this).attr('data-remaining-amount')).toFixed(2)) || 0;
        //        });
        //        $(this).val(checkedSum.toFixed(2)); // reset to the sum of checked invoice details remaing amount
        //    }
        //    else {
        //        $(this).val(checkedSum);
        //    }
        //    UpdateTotalAppliedAmount();
        //    return;
        //}
        if (amount != 0) {
            $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]`).each(function () {
                let lineRemaining = Number(Number($(this).data('remaining-amount')).toFixed(2)) || 0;
                if (lineRemaining != 0 && !$(this).is(':checked')) {
                    $(this).prop('checked', true);
                }
                if (isInvoicePaid === "true" && lineRemaining == 0) {
                    $(this).prop('checked', true);
                }
            });
        }
        else {
            $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]`).each(function () {
                let lineRemaining = parseFloat($(this).data('remaining-amount')) || 0;
                if (lineRemaining != 0 && $(this).is(':checked')) {
                    $(this).prop('checked', false);
                }
            });
        }

        // Calculate sum of checked line items' remaining amount
        //let checkedSum = 0;
        //$(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`).each(function () {
        //    checkedSum += parseFloat($(this).data('remaining-amount')) || 0;
        //});

        //if (checkedSum > 0 && amount > checkedSum) {
        //    appCommon.ShowMessageModal("Payable amount cannot exceed the sum of selected line items' remaining balance.");
        //    $(this).val(checkedSum.toFixed(2)); // reset to sum of checked line items
        //    UpdateTotalAppliedAmount();
        //    return;
        //}
        UpdatePaymentButtons();
        UpdateTotalAppliedAmount();
    });
    $('[data-name="btnApplyPayment"]').off('click');
    $('[data-name="btnApplyPayment"]').on('click', function () {

        let selectedInvoiceDetails = [];
        let invalidSelection = false;
        var Payment_Id = $('[data-name="Payment_Id"]').val();
        var Direction_Id = $('[data-name="Direction_Id"]').val();
        const PayableAmountElements = $('[data-name="PayableAmount"]').filter(function () {

            const $el = $(this);
            const invoiceId = $el.attr('data-invoice-id');

            const amount = Number($el.val()) || 0;

            // get checked line items
            const checkedItems = $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`);

            let hasValidLineItem = false;

            checkedItems.each(function () {
                const remaining = Number($(this).attr('data-remaining-amount')) || 0;
                var isInvoicePaid = ($(this).attr('data-invoice-paid') || "").toLowerCase().trim();
                if (remaining !== 0) {
                    hasValidLineItem = true;
                    return false; // break
                }
                if (isInvoicePaid === "true" && remaining === 0) {
                    hasValidLineItem = true;
                    return false; // break
                }
            });

            // include if:
            // - textbox has value OR
            // - user selected valid line items
            return amount !== 0 || hasValidLineItem;
        });
        for (const el of PayableAmountElements) {

            const $el = $(el);
            let invoiceId = $el.attr('data-invoice-id');
            let amountStr = $el.val();
            let amount = Number(Number(amountStr).toFixed(2)) || 0;
            let remainingAmt = Number(Number($el.attr('data-remaining-amount')).toFixed(2)) || 0;
            let isAccountsPayable = $el.attr('data-accounts-payable').toString() === 'true';
            let invoiceNo = $el.attr("data-invoice-number");
            let HasIndividualNonZeroRemainingBalance = false;
            let IndividualLineItems = $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`);
            var isInvoicePaid = ($el.attr('data-invoice-paid') || "").toLowerCase().trim();
            if (amount == 0 && remainingAmt == 0) {
                IndividualLineItems.each(function () {

                    HasIndividualNonZeroRemainingBalance = (Number(Number($(this).attr('data-remaining-amount')).toFixed(2)) || 0) != 0 ? true : false;
                    if (HasIndividualNonZeroRemainingBalance) {
                        return false; // break loop if found any line item with non-zero remaining balance
                    }
                });
            }

            if (amount != 0 || (amount == 0 && HasIndividualNonZeroRemainingBalance)) {

                //if (Math.abs(amount) > Math.abs(remainingAmt) && !(HasIndividualNonZeroRemainingBalance && Math.abs(remainingAmt) === 0)) {
                //    appCommon.ShowMessageModal("Payable amount cannot exceed the remaining balance for invoice " + invoiceNo + ".");
                //    invalidSelection = true;
                //    break;
                //}
                if (amount !== 0 && remainingAmt !== 0 && Math.sign(amount) !== Math.sign(remainingAmt)) {
                    appCommon.ShowMessageModal("Payable amount must have the same sign as the remaining balance." + invoiceNo + ".");
                    invalidSelection = true;
                    break;
                }
                // Sum of checked line items' remaining amounts
                let checkedSum = 0;
                let checkedDetails = $(`input[data-name="chkInvoiceDetails"][data-invoice-id="${invoiceId}"]:checked`);
                checkedDetails.each(function () {
                    checkedSum += Number(Number($(this).attr('data-remaining-amount')).toFixed(2)) || 0;
                });
                checkedSum = Number(checkedSum.toFixed(2));
                // Validation: If checked line items exist, amount must not exceed their sum
                if (checkedSum == 0 && !(amount == 0 && remainingAmt == 0 && HasIndividualNonZeroRemainingBalance)) {
                    if (isInvoicePaid === "false") {
                        appCommon.ShowMessageModal("Payable amount cannot be 0 for" + invoiceNo + ".");
                        invalidSelection = true;
                        break;
                    }
                }
                let invoiceDetailIds = [];
                checkedDetails.each(function () {
                    invoiceDetailIds.push($(this).attr('data-invoicedetail-id'));
                });
                if (checkedDetails.length === 0) {
                    // No line items selected
                    if (amount != 0) {
                        selectedInvoiceDetails.push({
                            Invoice_Id: invoiceId,
                            InvoiceDetail_Id: null,
                            Amount: amount,
                            RemainingBalance: remainingAmt,
                            InvoiceDetail_Ids: null,
                            IsPartial: true
                        });
                    }
                } else {
                    if (amount === remainingAmt) {
                        // PayableAmount matches the all item's sum of checked, pay each line item fully
                        //checkedDetails.each(function () {
                        //    let detailId = $(this).attr('data-invoicedetail-id');
                        //    let detailAmt = Number(Number($(this).attr('data-remaining-amount')).toFixed(2)) || 0;
                        //    selectedInvoiceDetails.push({
                        //        Invoice_Id: invoiceId,
                        //        InvoiceDetail_Id: detailId,
                        //        Amount: detailAmt,
                        //        RemainingBalance: detailAmt,
                        //        InvoiceDetail_Ids: null,
                        //        IsPartial: false
                        //    });
                        //});
                        selectedInvoiceDetails.push({
                            Invoice_Id: invoiceId,
                            InvoiceDetail_Id: null,
                            Amount: amount,
                            RemainingBalance: remainingAmt,
                            InvoiceDetail_Ids: invoiceDetailIds,
                            IsPartial: false
                        });
                    }
                    else {
                        // checkedDetails present
                        if (amount === checkedSum) {
                            selectedInvoiceDetails.push({
                                Invoice_Id: invoiceId,
                                InvoiceDetail_Id: null,
                                Amount: amount,
                                RemainingBalance: remainingAmt,
                                InvoiceDetail_Ids: invoiceDetailIds,
                                IsPartial: false
                            });
                            //checkedDetails.each(function () {
                            //    let detailId = $(this).attr('data-invoicedetail-id');
                            //    let detailAmt = Number(Number($(this).attr('data-remaining-amount')).toFixed(2)) || 0;
                            //    selectedInvoiceDetails.push({
                            //        Invoice_Id: invoiceId,
                            //        InvoiceDetail_Id: detailId,
                            //        Amount: detailAmt,
                            //        RemainingBalance: detailAmt,
                            //        InvoiceDetail_Ids: null,
                            //        IsPartial: false
                            //    });
                            //});
                        }
                        else {
                            // PayableAmount is not equal to checked sum, partial payment on selected line items

                            var partialContext = true;
                            if ((amount > checkedSum) && (amount > 0 && remainingAmt > 0)) {
                                partialContext = false;
                            }
                            else if ((checkedSum > amount) && (amount < 0 && remainingAmt < 0)) {

                                partialContext = false;

                            }
                            else if (checkedSum == 0 && amount > 0 && remainingAmt == 0) {
                                partialContext = false;
                            }

                            selectedInvoiceDetails.push({
                                Invoice_Id: invoiceId,
                                InvoiceDetail_Id: null,
                                Amount: amount,
                                RemainingBalance: remainingAmt,
                                InvoiceDetail_Ids: invoiceDetailIds,
                                IsPartial: partialContext
                            });

                        }
                    }
                }
            } 
        }

        if (invalidSelection) return false;

        if (selectedInvoiceDetails.length === 0) {
            appCommon.ShowMessageModal("Please select at least one full or partial invoice to apply payment.");
            return false;
        }
        //appCommon.ExecuteAjaxPostMethod("/Payments/ApplyPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id, Direction_Id: Direction_Id }, function (response) {
        //    if (response != null && typeof response != 'undefined') {
        //        if (response.Status) {
        //            if (Direction_Id == MasterLookup._Directions.AR) {
        //                LoadARGridWithFilters();
        //            }
        //            else if (Direction_Id == MasterLookup._Directions.AP) {
        //                LoadAPWithFilters();
        //            }
        //            if (Payment_Id != null && Payment_Id != undefined) {
        //                LoadPaymentPanel(Payment_Id, Direction_Id);
        //            }
        //            appCommon.ShowMessageModal("Successfully Applied payments!", "Sucess");
        //        }
        //        else {
        //            if (response.Data != null && typeof response.Data != undefined) {
        //                if (response.Data.EntityMismatch) {
        //                    appCommon.ShowConfirmationModal(response.Message + "<br><br>Would you like to override the invoice entities? Please confirm to proceed.", "Alert", function () {
        //                        appCommon.ExecuteAjaxPostMethod("/Payments/ApplyPayments", { checkedInvoiceDetails: selectedInvoiceDetails, Payment_Id: Payment_Id, Direction_Id: Direction_Id, IsOverride: true }, function (response) {
        //                            if (response != null && typeof response != 'undefined') {
        //                                if (response.Status) {
        //                                    if (Direction_Id == MasterLookup._Directions.AR) {
        //                                        LoadARGridWithFilters();
        //                                    }
        //                                    else if (Direction_Id == MasterLookup._Directions.AP) {
        //                                        LoadAPWithFilters();
        //                                    }
        //                                    if (Payment_Id != null && Payment_Id != undefined) {
        //                                        LoadPaymentPanel(Payment_Id, Direction_Id);
        //                                    }
        //                                    appCommon.ShowMessageModal("Successfully Applied payments!", "Sucess");
        //                                }
        //                                else {
        //                                    if (response.Message != null && response.Message != '') {
        //                                        appCommon.ShowMessageModal(response.Message);
        //                                    }
        //                                    else {
        //                                        appCommon.ShowMessageModal("Unable to process the request.")
        //                                    }
        //                                }
        //                            }
        //                        }, true);
        //                    });
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
        //    }

        //}, true)
        ApplyPayments(selectedInvoiceDetails, Payment_Id, Direction_Id, false, false, false);
    });

}
function ApplyPayments(selectedInvoiceDetails, Payment_Id, Direction_Id, isOverride, allowCredit, allowInvoiceCredit) {

    appCommon.ExecuteAjaxPostMethod(
        "/Payments/ApplyPayments",
        {
            checkedInvoiceDetails: selectedInvoiceDetails,
            Payment_Id: Payment_Id,
            Direction_Id: Direction_Id,
            IsOverride: isOverride,
            AllowCredit: allowCredit,
            AllowInvoiceCredit: allowInvoiceCredit
        },
        function (response) {

            if (!response) {
                appCommon.ShowMessageModal("Unable to process request.");
                return;
            }

            // SUCCESS
            if (response.Status) {

                if (Direction_Id == MasterLookup._Directions.AR) {
                    LoadARGridWithFilters();
                }
                else if (Direction_Id == MasterLookup._Directions.AP) {
                    LoadAPWithFilters();
                }

                if (Payment_Id) {
                    LoadPaymentPanel(Payment_Id, Direction_Id);
                }

                appCommon.ShowMessageModal(
                    "Successfully Applied payments!",
                    "Success"
                );

                return;
            }

            // ENTITY MISMATCH
            if (response.Data && response.Data.EntityMismatch) {

                appCommon.ShowConfirmationModal(
                    response.Message +
                    "<br><br>Would you like to override the invoice entities?",
                    "Alert",
                    function () {

                        ApplyPayments(selectedInvoiceDetails, Payment_Id, Direction_Id, true, allowCredit, allowInvoiceCredit);
                    });

                return;
            }

            // ALLOW CREDIT
            if (response.Data && response.Data.AllowCredit) {

                appCommon.ShowConfirmationModal(
                    response.Message,
                    "Alert",
                    function () {

                        ApplyPayments(selectedInvoiceDetails, Payment_Id, Direction_Id, isOverride, true, allowInvoiceCredit);
                    });

                return;
            }
            // ALLOW INVOICE CREDIT
            if (response.Data && response.Data.AllowInvoiceCredit) {

                appCommon.ShowConfirmationModal(
                    response.Message,
                    "Warning",
                    function () {

                        ApplyPayments(selectedInvoiceDetails, Payment_Id, Direction_Id,
                            isOverride,
                            allowCredit,
                            true
                        );
                    });

                return;
            }

            // GENERAL ERROR
            appCommon.ShowMessageModal(
                response.Message || "Unable to process the request."
            );
        },
        true
    );
}
function FilteredUploadedInvoicespayment(jsonData, mapping) {

    let updatedCount = 0;

    // Helper function to normalize policy number
    const normalizePolicy = (value) => {
        return value
            ? value.toString().trim()
                .replace(/[^a-zA-Z0-9]/g, '')
                .toUpperCase()
            : null;
    };

    jsonData.forEach(item => {

        const invoiceNo = mapping.InvoiceNumber
            ? item[mapping.InvoiceNumber]?.toString().trim()
            : null;

        // Normalize Excel policy number
        const policyNo = mapping.PolicyNumber
            ? normalizePolicy(item[mapping.PolicyNumber])
            : null;

        const amountRaw = mapping.PaymentAmount
            ? item[mapping.PaymentAmount]
            : null;

        const isRemaining =
            amountRaw?.toString().trim().toUpperCase() === "X";

        const amount = Number(amountRaw);

        let matched = false;

        $('[data-name="PayableAmount"]').each(function () {

            const currentInvoiceNo =
                $(this).data('invoice-number')?.toString().trim();

            // Normalize UI policy number also
            const currentPolicyNo =
                normalizePolicy($(this).data('policy-number'));

            if (
                (invoiceNo && currentInvoiceNo === invoiceNo) ||
                (policyNo && currentPolicyNo === policyNo)
            ) {

                const remainingRaw =
                    $(this).attr("data-remaining-amount");

                const remaining = Number(remainingRaw);

                // Apply the restriction only during Excel upload.
                if (remainingRaw != null && remainingRaw.trim() !== ""
                    && Number.isFinite(remaining)) {

                    let paymentAmount;

                    if (isRemaining) {

                        // X means apply the full remaining balance.
                        paymentAmount = remaining;

                    } else if (Number.isFinite(amount)) {

                        // Cap the uploaded amount using absolute values.
                        const cappedAmount = Math.min(
                            Math.abs(amount),
                            Math.abs(remaining)
                        );

                        // Preserve the sign of the uploaded amount.
                        paymentAmount =
                            amount < 0 ? -cappedAmount : cappedAmount;

                    } else {
                        // Do not overwrite the field with an invalid amount.
                        return false;
                }

                    $(this).val(
                        appCommon.RemoveCurrencyFormat(
                            paymentAmount.toFixed(2)
                        )
                    );

                updatedCount++;
                matched = true;
                return false;
            }
          }
        });

        item.status = matched ? "" : "Discrepancy";
    });

    $('[data-name="PayableAmount"]').trigger('focusout');

    if (jsonData.some(r => r.status === "Discrepancy")) {

        const discrepancyOnly = jsonData
            .filter(r => r.status === "Discrepancy")
            .map(r => {
                const copy = { ...r };
                delete copy.status;
                return copy;
            });

        const ws = XLSX.utils.json_to_sheet(discrepancyOnly);
        const wb = XLSX.utils.book_new();

        XLSX.utils.book_append_sheet(
            wb, ws, "Invoice Mismatch Report"
        );

        XLSX.writeFile(wb, "Invoice Mismatch Report.xlsx");

        appCommon.ShowMessageModal(
            "Invoice mismatch report downloaded successfully.",
            "Success"
        );
    }
}



$(document).on('change', '#chkAllInvoices', function () {
    var isChecked = $(this).is(':checked');
    $('input[type="checkbox"][data-name="chkInvoice"]').prop('checked', isChecked).trigger('change');
    $('input[type="checkbox"][data-name="chkInvoiceDetails"]').prop('checked', isChecked);
});

$(document).on('change', 'input[type="checkbox"][data-name= "chkInvoice"]', function () {
    var invoiceId = $(this).data('invoice-id');
    var isChecked = $(this).is(':checked');
    $('input[type="checkbox"][data-name="chkInvoiceDetails"][data-invoice-id="' + invoiceId + '"]').prop('checked', isChecked);
});
function BindPaymentsTileEvents() {
    $('[data-name="btnEditSelectedPayment"]').off('click');
    $('[data-name="btnEditSelectedPayment"]').on('click', function () {
        var Payment_Id = $(this).attr('data-payment-id');
        appCommon.SubmitForm($('[data-name="selectPayments"]'), function (response) {
            if (response != undefined && response != null) {
                appCommon.GenericModalFunction("Select Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                $('[data-name="payment_' + Payment_Id + '"]').addClass('selected-payment');
            }
        }, "/Search/PaymentsSearchForSelection", true, false);
    });
}

//function LoadAPPanel() {
//    appCommon.RenderHTMLPanel("/AccountsPayable/LoadGrid", $('[data-name="divAccountsPayableGrid"]'), function () { BindAPSearchPanelEvents(); BindARAPCommonEvents(); }, true, null);
//}
function LoadPartialPaymentDetails(PolicyDetail_Id, Invoice_Id, Direction_Id) {
    appCommon.ExecuteAjaxGetMethod("/Payments/PartialPayment?policyDetail_Id=" + PolicyDetail_Id + "&Invoice_Id=" + Invoice_Id + "&Direction_Id=" + Direction_Id, function (response) {
        if (response != null && response != undefined) {
            appCommon.GenericModalFunction("Information", response);
            BindARAPCommonEvents();
            ApplyInsuredNameSorting();
        }
    }, true);
}


$('[data-name="divAccountsPayableGrid"]').on('scroll', function () {
    var container = $(this);
    var currentScrollTop = container.scrollTop();

    // Scroll down
    if (!isAPLoading && currentScrollTop > lastScrollTopAP && currentScrollTop + container.innerHeight() >= container[0].scrollHeight - 50) {

        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var nextPage = currentPage + 1;

        // Skip if already loaded OR beyond maxPageAP
        if (loadedPagesAP.indexOf(nextPage) === -1 && (!maxPageAP || nextPage <= maxPageAP)) {
            isAPLoading = true;
            $('[name="pageIndex"]').val(nextPage);
            $('[name="pageSize"]').val(20);
            LoadAPGridWithFiltersForScroll("down", nextPage);
        }
    }

    // Scroll up
    if (!isAPLoading && currentScrollTop < lastScrollTopAP && currentScrollTop <= 5) {
        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var prevPage = currentPage - 1;

        // Stop at page 1 and skip if already loaded
        if (prevPage > 0 && loadedPagesAP.indexOf(prevPage) === -1) {
            isAPLoading = true;
            $('[name="pageIndex"]').val(prevPage);
            $('[name="pageSize"]').val(20);
            LoadAPGridWithFiltersForScroll("up", prevPage);
        }
    }

    lastScrollTopAP = currentScrollTop;
});

function LoadAPGridWithFiltersForScroll(direction, pageIndex) {
    var APContainer = $('[data-name="divAccountsPayableGrid"]');

    var oldHeight = APContainer[0].scrollHeight;
    var oldScrollTop = APContainer.scrollTop();
    var SortOrder = currentSortOrder;
    var queryString = appCommon.BuildQueryStringParameters({
        SortOrder: SortOrder
    });
    ShowLoaderRowForScroll(direction, "AP");
    appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
        if (response) {
            if (response.indexOf("No records found") !== -1) {
                console.log("No records found, setting maxPageAP");
                // mark this as the last page
                maxPageAP = pageIndex;
            } else {
                if (direction === "down") {
                    APContainer.append(response);
                } else if (direction === "up") {
                    APContainer.prepend(response);

                    // preserve scroll position after prepend
                    var newHeight = APContainer[0].scrollHeight;
                    APContainer.scrollTop(oldScrollTop + (newHeight - oldHeight));
                }
                PartialInvoiceDetails = [];
                UpdateSortsUI();
                BindARAPCommonEvents();
                ApplyInsuredNameSorting();
            }

            // Mark this page as loaded
            if (loadedPagesAP.indexOf(pageIndex) === -1) {
                loadedPagesAP.push(pageIndex);
            }
        }
        RemoveLoaderRowForScroll("AP");
        isAPLoading = false;
        lastScrollTopAP = APContainer.scrollTop();
    }, "/Search/SearchAP?" + queryString, true, false);
}

function LoadAPWithFilters() {
    var APContainer = $('[data-name="divAccountsPayableGrid"]');
    var SortOrder = currentSortOrder;
    appCommon.ShowPartialPanelLoader(APContainer);
    var queryString = appCommon.BuildQueryStringParameters({
        SortOrder: SortOrder
    });
    appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
        if (response != undefined && response != null) {
            $(APContainer).html(response);
            UpdateSortsUI();
            ApplyInsuredNameSorting();
            BindAPSearchPanelEvents();
            BindARAPCommonEvents();
            loadedPagesAP.push(1);
        }
    }, "/Search/SearchAP?" + queryString, true, false);
}
function BindAPSearchPanelEvents() {
    $('[data-name="btnApplyFilters"]').off('click');
    $('[data-name="btnApplyFilters"]').on('click', function () {
        ClearloadedPagesAP();
        var APContainer = $('[data-name="divAccountsPayableGrid"]');
        var SortOrder = currentSortOrder;
        appCommon.ShowPartialPanelLoader(APContainer);
        var queryString = appCommon.BuildQueryStringParameters({
            SortOrder: SortOrder
        });
        appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
            if (response != undefined && response != null) {
                $(APContainer).html(response);
                UpdateSortsUI();
                ApplyInsuredNameSorting();
                BindAPSearchPanelEvents();
                BindARAPCommonEvents();
                $('[data-name="chkAllInvoices"]').removeClass("d-none");
            }
        }, "/Search/SearchAP?" + queryString, true, false);
    });

    $('[data-name="btnSelectPayment"]').off('click');
    $('[data-name="btnSelectPayment"]').on('click', function () {
        appCommon.SubmitForm($('[data-name="selectPayments"]'), function (response) {
            if (response != undefined && response != null) {
                appCommon.GenericModalFunction("Select Payments", response);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1400px");
                appCommon.HideLoader();
                BindAPSearchPanelEvents();
                BindARAPCommonEvents();
                UpdatePaymentButtons();
            }
        }, "/Search/PaymentsSearchForSelection", false, true);
    });


}
$('[data-name="lineitem-options"]').on("change", function () {
    updateLineItemList($(this));
});
function updateLineItemList(ctrl) {
    //
    $this = $(ctrl)
    var selectedCode = $this.attr('data-lineitem-code');
    var isChecked = $this.is(':checked');

    var pairedCode = null;

    if (selectedCode === "CarrierPremium") {
        pairedCode = "HouseCommission";
    }
    else if (selectedCode === "HouseCommission") {
        pairedCode = "CarrierPremium";
    }

    if (pairedCode) {

        var $pairedCheckbox = $('input[data-name="lineitem-options"][data-lineitem-code="' + pairedCode + '"]');

        // Prevent infinite trigger loop
        if ($pairedCheckbox.length) {
            $pairedCheckbox.prop('checked', isChecked);
        }
    }
    var selected = [];
    $('input[data-name="lineitem-options"]:checked').each(function () {
        selected.push($(this).data("lineitem-id"));
    });
    $('input[name="LineItemList"]').val(selected.join(","));
}
function LoadPaymentPanel(Payment_Id, Direction_Id) {
    appCommon.ExecuteAjaxGetMethod("/Payments/SelectedPaymentsTile?Payment_Id=" + Payment_Id + "&Direction_Id=" + Direction_Id, function (response) {
        if (response != null && response != undefined) {
            $('[data-name="divSelectedPayments"]').html(response);
            $('[data-name="GenericModal"]').modal('hide');
            BindPaymentsTileEvents();
        }
    }, true);
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
function UpdatePaymentButtons() {
    var checkedRows = $('[data-name="chkInvoiceDetails"]:checked');
    var hasPaid = false;
    var hasUnpaid = false;

    checkedRows.each(function () {
        var isInvoicePaid = ($(this).attr('data-invoice-paid') || "").toLowerCase().trim();
        var remainingAmount = Number($(this).attr('data-remaining-amount')) || 0;
        if (isInvoicePaid === "true") {
            hasPaid = true;
        } else if (isInvoicePaid === "false") {
            hasUnpaid = true;
        }

        if (isInvoicePaid === "true" && remainingAmount != 0) {
            hasUnpaid = true;
        } else if (isInvoicePaid === "true" && remainingAmount == 0) {
            hasUnpaid = true;
        }
    });
    const PayableAmountElements = $('[data-name="PayableAmount"]').filter(function () {
        return Number($(this).val()) !== 0;
    });

    if (checkedRows.length > 0) {
        $('[data-name="btnUnApplyFullPayment"]').show();
        $('[data-name="btnApplyPayment"]').toggle(hasUnpaid);
        $('[data-name="divAmountApplied"]').toggle(hasUnpaid);
    }
    else if (PayableAmountElements.length > 0) {
        $('[data-name="btnUnApplyFullPayment"]').show();
        $('[data-name="btnApplyPayment"]').show();
        $('[data-name="divAmountApplied"]').show();
    }

    else {
        $('[data-name="btnUnApplyFullPayment"]').hide();
        $('[data-name="divAmountApplied"]').hide();
        $('[data-name="btnApplyPayment"]').hide();
    }
}
function BindGenerateInvoiceEvents(PolicyDetails_Id) {
    $('[data-name="downloadPdf"]').on("click", function (event) {

        var InstallmentNo = $(this).attr('data-installmentNo');
        appCommon.ExecuteAjaxGetMethod("/Invoices/DownloadInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (response) {
            if (typeof response != "undefined" && response != null) {
                appCommon.DownloadBase64File(response.Data);
            }
        }, true);
    });
}
function UpdateTotalAppliedAmount() {
    let total = 0;
    $('input[data-name="PayableAmount"]').each(function () {
        let val = Number(Number($(this).val()).toFixed(2));
        if (!isNaN(val)) total += val;
    });
    $('[data-name="TotalAppliedAmount"]').text(appCommon.ApplyCurrencyFormatWithFractionDigits(total, 2));
    $('[data-name="divAmountApplied"]').show();
}
// Utility to build a normalized header map from a JSON row (e.g., after reading Excel)
function BuildNormalizedHeaderMap(row) {
    const normalizedColumnMap = {};
    Object.keys(row || {}).forEach(col => {
        normalizedColumnMap[col.trim().toLowerCase()] = col;
    });
    return normalizedColumnMap;
}

// Reusable value extractor
function GetValue(row, header, normalizedColumnMap) {
    const realHeader = normalizedColumnMap[header.trim().toLowerCase()];
    return realHeader ? (row[realHeader] != null ? row[realHeader].toString().trim() : "") : "";
}
//Sorting Functionlaity for both AR/AP Tab
var insuredNameSortOrder = "";
$(document).off("click.sortInsuredName")
    .on("click.sortInsuredName", '[data-name="sortInsuredName"]', function () {
        insuredNameSortOrder = ($(this).attr("data-sort-order") || "").trim().toUpperCase();
        ApplyInsuredNameSorting();
        if (insuredNameSortOrder === "ASC") {
            $('[data-sort-order="ASC"]').addClass("d-none");
            $('[data-sort-order="DESC"]').removeClass("d-none");
        }
        else {
            $('[data-sort-order="DESC"]').addClass("d-none");
            $('[data-sort-order="ASC"]').removeClass("d-none");
        }
    });

function ApplyInsuredNameSorting() {

    if (!insuredNameSortOrder)
        return;

    var isAscending = insuredNameSortOrder === "ASC";
    var gridContainer = $('[data-name="divAccountsPayableGrid"]');
    var accordionContainer = $('#accountsPayableStayOpen');

    if (accordionContainer.length === 0) {
        gridContainer = $('[data-name="ARContainer"]');
        accordionContainer = $('#accountsReceivableStayOpen');
    }

    if (accordionContainer.length === 0)
        return;

    var allItems = gridContainer.find(".accordion-item").get();

    allItems.sort(function (a, b) {
        var nameA = ($(a).attr("data-insured-name") || "").trim().toLowerCase();
        var nameB = ($(b).attr("data-insured-name") || "").trim().toLowerCase();

        return isAscending ? nameA.localeCompare(nameB) : nameB.localeCompare(nameA);
    });

    gridContainer.find(".accordion-item").remove();
    accordionContainer.append(allItems);
    BindARAPCommonEvents();
}
