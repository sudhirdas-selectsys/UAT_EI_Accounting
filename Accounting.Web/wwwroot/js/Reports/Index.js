var ReportForm = null;
var ReportView = null;
var ExistingForm = null;
var defaultPageSize = MasterLookup._DefaultPageSize;
var Report_Id = null;
$(document).ready(function () {
    ReportForm = $('[data-name="ReportForm"]');
    ReportView = $('[data-name="ReportView"]');
    if (typeof DefaultReport_Id != 'undefined' && DefaultReport_Id > 0) {
        $(`[data-report-id="${DefaultReport_Id}"]`).addClass('active');
        _ReportsPanel.LoadReportForm(DefaultReport_Id, defaultPageSize, 0);
    }

    if ($('[data-name="btnLoadReportForm"]').length > 0)
        $('[data-name="btnLoadReportForm"]').click(function () {
            Report_Id = $(this).attr("data-report-id");
            $(this).closest('ul').find('a').removeClass('active');
            $(this).addClass('active');
            $('[data-report-name]').text($(this).text());
            ClearReportView();
            _ReportsPanel.LoadReportForm(Report_Id, defaultPageSize, 0);
            waitForElement('input[type="radio"][data-name="InvoiceDate"][value="InvoiceCreatedDate"]', function () {
                $('input[type="radio"][data-name="InvoiceDate"][value="InvoiceCreatedDate"]').prop('checked', true).trigger('change');
            });
            return false;
        });

    $(document).on('change', '[data-name="PageSize"]', function () {
        var tableName = $(this).attr('data-table-name');
        Report_Id = $(this).attr("data-report-id");
        var PageSize = $(this).val();
        var PageNumber = 0;
        if (tableName == "ReportDetails") {
            if (typeof window.LoadReportFromForm === "function") {
                window.LoadReportFromForm(PageSize, PageNumber, Report_Id);
            }
        }
    });

    $(document).on('click', '[data-name="btnPrevious"]', function () {
        var tableName = $(this).attr('data-table-name');
        Report_Id = $(this).attr("data-report-id");
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) - 1;
        if (tableName == "ReportDetails") {
            if (typeof window.LoadReportFromForm === "function") {
                window.LoadReportFromForm(PageSize, PageNumber, Report_Id);
            }
        }
    });

    $(document).on('click', '[data-name="btnNext"]', function () {
        var tableName = $(this).attr('data-table-name');
        Report_Id = $(this).attr("data-report-id");
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) + 1;
        if (tableName == "ReportDetails") {
            if (typeof window.LoadReportFromForm === "function") {
                window.LoadReportFromForm(PageSize, PageNumber, Report_Id);
            }
        }
    });

    $(document).on('change', 'input[type="radio"][data-name="InvoiceDate"]', function () {
        $('select[data-name="Range"]').prop('selectedIndex', 0);
        $('input[data-val-type="date"]').each(function () {
            if (!$(this).closest('[data-form-group-item]').hasClass('d-none')) {
                const label = $(this).attr('placeholder')?.toLowerCase();
                if (label.includes("from")) {
                    $(this).val('');
                } else if (label.includes("to")) {
                    $(this).val('');
                }
            }
        });
        var selectedValue = $(this).val();

        $(`[data-parent-group="InvoiceCreatedDate"], [data-parent-group="InvoiceEffectiveDate"]`).each(function () {
            $(this).closest('[data-form-group-item]').addClass('d-none');
        });

        $('[data-parent-group="' + selectedValue + '"]').each(function () {
            $(this).closest('[data-form-group-item]').removeClass('d-none');
        });
    });
    waitForElement('input[type="radio"][data-name="InvoiceDate"][value="InvoiceCreatedDate"]', function () {
        $('input[type="radio"][data-name="InvoiceDate"][value="InvoiceCreatedDate"]').prop('checked', true).trigger('change');
    });


    $(document).on('change', 'select[data-name="Range"]', function () {
        const selectedRange = $(this).val();
        const today = new Date();
        let fromDate = '', toDate = '';

        const formatDate = (date) => {
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            const yyyy = date.getFullYear();
            return `${mm}/${dd}/${yyyy}`;
        };

        switch (selectedRange) {
            case 'Year To Date':
                fromDate = `01/01/${today.getFullYear()}`;
                toDate = formatDate(today);
                break;
            case 'This Month':
                fromDate = `${String(today.getMonth() + 1).padStart(2, '0')}/01/${today.getFullYear()}`;
                toDate = formatDate(today);
                break;
            case 'Last Month':
                const firstOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                const lastMonthEnd = new Date(firstOfMonth - 1);
                const lastMonthStart = new Date(lastMonthEnd.getFullYear(), lastMonthEnd.getMonth(), 1);
                fromDate = formatDate(lastMonthStart);
                toDate = formatDate(lastMonthEnd);
                break;
            case 'This Week':
                const today2 = new Date();
                const dayOfWeek = today2.getDay(); // 0 (Sun) to 6 (Sat)
                const startOfWeek = new Date(today2);
                startOfWeek.setDate(today2.getDate() - ((dayOfWeek + 1) % 7));
                const endOfWeek = new Date(startOfWeek);
                endOfWeek.setDate(startOfWeek.getDate() + 6);
                fromDate = formatDate(startOfWeek);
                toDate = formatDate(endOfWeek);
                break;
            case 'Last Week':
                const endLastWeek = new Date(today);
                endLastWeek.setDate(today.getDate() - today.getDay()); // Last Sunday
                const startLastWeek = new Date(endLastWeek);
                startLastWeek.setDate(endLastWeek.getDate() - 6); // Previous Monday
                fromDate = formatDate(startLastWeek);
                toDate = formatDate(endLastWeek);
                break;
            case 'Last Year':
                fromDate = `01/01/${today.getFullYear() - 1}`;
                toDate = `12/31/${today.getFullYear() - 1}`;
                break;
            default:
                fromDate = '';
                toDate = '';
        }

        $('input[data-val-type="date"]').each(function () {
            if (!$(this).closest('[data-form-group-item]').hasClass('d-none')) {
                const label = $(this).attr('placeholder')?.toLowerCase();
                if (label.includes("from")) {
                    $(this).val(fromDate);
                } else if (label.includes("to")) {
                    $(this).val(toDate);
                }
            }
        });
    });
});
ReportsPanel = function (Report_Id, PageSize, PageNumber) {
    return {
        LoadReportForm: function (Report_Id, PageSize, PageNumber) {
            appCommon.UpdateBrowserURLHistory("/Reports", null, { Report_Id: Report_Id });
            var queryString = appCommon.BuildQueryStringParameters({ Report_Id: Report_Id, PageSize: PageSize, PageIndex: PageNumber });
            appCommon.RenderHTMLPanel("/Reports/ReportForm?" + queryString, ReportForm, function () { $('[data-name="ReportForm"]').css('min-height', '0px'); }, true);
        },
        LoadReport: function (Report_Id, PageSize, PageNumber) {
            var queryString = appCommon.BuildQueryStringParameters({ Report_Id: Report_Id, PageSize: PageSize, PageIndex: PageNumber });
            appCommon.RenderHTMLPanel("/Reports/Report?" + queryString, ReportView, function () { }, true);
        }
    }
}
_ReportsPanel = new ReportsPanel();

//$(window).on('load', function () {
//    $('input[type="radio"][data-name="InvoiceDate"][value="InvoiceCreatedDate"]').prop('checked', true).trigger('change');
//});
function ClearReportView() {    
    $('[data-name="ReportView"]').html("");
}
function ClearReportFormView() {
    $('[data-name="ReportForm"]').html("");
}
function waitForElement(selector, callback, interval = 200, timeout = 10000) {
    const startTime = Date.now();
    const timer = setInterval(function () {
        if ($(selector).length) {
            clearInterval(timer);
            callback();
        } else if (Date.now() - startTime > timeout) {
            clearInterval(timer);
            console.warn('Element not found within timeout:', selector);
        }
    }, interval);
}