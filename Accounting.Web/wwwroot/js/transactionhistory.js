var defaultPageSize = MasterLookup._DefaultPageSize;
$(function () {    
    BindTransactionHistoryEvents();
    LoadTransactionHistoryGrid(defaultPageSize, 0);
});
function BindPaginationEvents() {
    $('[data-name="btnPrevious"]').click(function () {
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) - 1;
        if (tableName == "TransactionHistoryDetails") {
            LoadTransactionHistoryGrid(PageSize, PageNumber)
        }
    });
    $('[data-name="btnNext"]').click(function () {
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) + 1;
        if (tableName == "TransactionHistoryDetails") {
            LoadTransactionHistoryGrid(PageSize, PageNumber)
        }
    });
    $('[data-name="PageSize"]').change(function () {
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).val();
        var PageNumber = 0;
        if (tableName == "TransactionHistoryDetails") {
            LoadTransactionHistoryGrid(PageSize, PageNumber)
        }
    });
}
function BindTransactionHistoryEvents() {
    flatpickr(".flatpickr", {
        dateFormat: "m-d-Y",
        disableMobile: true
    });

    flatpickr(".flatpickrRange", {
        mode: "range",
        minDate: "today",
        dateFormat: "Y-m-d"
    });

    $('[data-name="btnViewTransactionHistoryDetails"]').off('click');
    $('[data-name="btnEditTransactionHistoryDetails"]').off('click');
    $('[data-name="btnDeleteTransactionHistoryDetails"]').off('click');

    $('[data-name="btnResetTransactionHistory"]').on('click', function () {
        var TransactionHistorySectionPanel = $('[data-name="frmTransactionHistorySearch"]')
        appCommon.ResetFieldsInPanel(TransactionHistorySectionPanel);
        LoadTransactionHistoryGrid(defaultPageSize, 0);
    });

    $('[data-name="btnSearchTransactionHistory"]').off('click');
    $('[data-name="btnSearchTransactionHistory"]').on('click', function () {
        var TransactionHistoryGridContainer = $('[data-name="TransactionHistoryGridContainer"]');
        appCommon.ShowPartialPanelLoader(TransactionHistoryGridContainer);
        var queryString = appCommon.BuildQueryStringParameters({ PageSize: defaultPageSize, PageIndex: 0 });
        appCommon.SubmitForm($('[data-name="frmTransactionHistorySearch"]'), function (response) {
            if (response != undefined && response != null) {
                $(TransactionHistoryGridContainer).html(response);
                BindTransactionHistoryEvents();
                BindPaginationEvents();
            }
        }, "/TransactionHistory/TransactionHistorySearch" + "?" + queryString, true, false);
    });

    $('[data-name="btnViewTransactionHistoryDetails"]').on('click', function () {
        var PaymentApplicationLineItem_Id = $(this).attr('data-PaymentApplicationLineItem-Id');
        appCommon.ExecuteAjaxGetMethod("/TransactionHistory/TransactionHistoryDetailsViewOnly?PaymentApplicationLineItem_Id=" + PaymentApplicationLineItem_Id, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Transaction History Info", response);
                BindTransactionHistoryEvents();
            }
        }, true);
    });

    $('[data-name="PayorOrPayee"]').each(function () {
        EnableTransactionEntityNameComplete(this);
    });

    $('[data-name="PolicyNumber"]').each(function () {
        EnablePolicyNumberComplete(this);
    });

    $('[data-name="InvoiceNumber"]').each(function () {
        EnableInvoiceNumberComplete(this);
    });

}

//$(document).on('click', '[data-name="btnViewTransactionHistoryDetails"]', function () {
//    var PaymentApplicationLineItem_Id = $(this).attr('data-PaymentApplicationLineItem-Id');
//    appCommon.ExecuteAjaxGetMethod("/TransactionHistory/TransactionHistoryDetailsViewOnly?PaymentApplicationLineItem_Id=" + PaymentApplicationLineItem_Id, function (response) {
//        if (response != null && response != undefined) {
//            appCommon.ModalGeneralFunction("Transaction History Info", response);
//            BindTransactionHistoryEvents();
//        }
//    }, true);
//});

function LoadTransactionHistoryGrid(PageSize, PageNumber) {
    var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
    appCommon.RenderHTMLPanel("/TransactionHistory/TransactionHistoryGrid" + "?" + queryString, $('[data-name="TransactionHistoryGridContainer"]'), function () {
        BindTransactionHistoryEvents();
        BindPaginationEvents();
    }, true, null);
}

function EnableTransactionEntityNameComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var Name = $(ctrl).val();
            $.ajax({
                url: "/TransactionHistory/GetPayorOrPayeeNames",
                data: { Name: Name },
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

function EnableInvoiceNumberComplete(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;
    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var InvoiceNumber = $(ctrl).val();
            $.ajax({
                url: "/api/SearchAPI/GetInvoiceNumbers",
                data: { InvoiceNumber: InvoiceNumber },
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