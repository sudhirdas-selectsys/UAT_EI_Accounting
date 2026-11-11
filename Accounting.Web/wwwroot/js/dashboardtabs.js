var defaultPageSize = MasterLookup._DefaultPageSize;
$(function () {
    BindDashboardTabsClickEvents(defaultPageSize, 0);
    CallPreviousTabClickEvents();
});

//$(document).on('click', '[data-name="btnPrevious"]', function () {
//    var tableName = $(this).attr('data-table-name');
//    var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
//    var PageNumber = parseInt($(this).attr('data-page-index'), 10) - 1;
//    if (tableName == "DashboardDetails") {
//        LoadDashboard(PageSize, PageNumber)
//    }
//});

//$(document).on('click', '[data-name="btnNext"]', function () {
//    var tableName = $(this).attr('data-table-name');
//    var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
//    var PageNumber = parseInt($(this).attr('data-page-index'), 10) + 1;
//    if (tableName == "DashboardDetails") {
//        LoadDashboard(PageSize, PageNumber)
//    }
//});
//$(document).on('change', '[data-name="PageSize"]', function () {
//    var tableName = $(this).attr('data-table-name');
//    var PageSize = $(this).val();
//    var PageNumber = 0;
//    if (tableName == "DashboardDetails") {
//        LoadDashboard(PageSize, PageNumber);
//    }
//});
function LoadDashboard(PageSize, PageNumber) {
    var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
    appCommon.RenderHTMLPanel("/Home/Dashboard" + "?" + queryString, $('[data-name="DashboardTabsContainer"]'), function () {       
    }, true);
}
function BindDashboardTabsClickEvents(PageSize, PageNumber) {
    $('[data-name="btnDashboardTabs"]').off('click');
    $('[data-name="btnDashboardTabs"]').on('click', function () {
        var _tabName = $(this).attr('data-tab-name');
        sessionStorage.setItem('selectedDashboardTab', _tabName);
        var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
        var tabPaths = {
            Dashboard: "/Home/Dashboard" + "?" + queryString,
            AccountsReceivable: "/AccountsReceivable/Index",
            AccountsPayable: "/AccountsPayable/Index",
            TransactionHistory: "/TransactionHistory/Index" + "?" + queryString,
            Payments: "/Payments/Index" + "?" + queryString,
            MakePayments: "/MakePayments/Index",
            PrintChecks:"/PrintChecks/Index"
        };
        if (tabPaths[_tabName]) {
            var url = tabPaths[_tabName];
            appCommon.RenderHTMLPanel(url, $('[data-name="DashboardTabsContainer"]'), function () {  }, true, null);
        }
    });
}

function CallPreviousTabClickEvents() {
    var PreviousTab = sessionStorage.getItem('selectedDashboardTab') || 'Dashboard';
    const PreviousTabButton = $(`[data-name="btnDashboardTabs"][data-tab-name="${PreviousTab}"]`);

    if (PreviousTabButton.length) {
        PreviousTabButton.trigger('click');
    } else {
        $('[data-name="btnDashboardTabs"]').first().trigger('click');
    }
}
