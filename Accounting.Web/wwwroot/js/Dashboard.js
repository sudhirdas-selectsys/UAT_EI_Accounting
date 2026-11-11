var defaultPageSize = MasterLookup._DefaultPageSize;
$(function () {
    BindDashboardSearchEvents();
    LoadDashboardGrid(defaultPageSize, 0);
});
function BindPaginationEvents() {

    $('[data-name="btnPrevious"]').on('click', function () {
        var DashboardGridContainer = $('[data-name="DashboardGridContainer"]');
        appCommon.ShowPartialPanelLoader(DashboardGridContainer);
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) - 1;
        if (tableName == "DashboardDetails") {
            //LoadDashboardGrid(PageSize, PageNumber)
            SearchDashboard(PageSize, PageNumber)
        }
    });
    $('[data-name="btnNext"]').on('click', function () {
        var DashboardGridContainer = $('[data-name="DashboardGridContainer"]');
        appCommon.ShowPartialPanelLoader(DashboardGridContainer);
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).closest('.table-pagination').find('select[data-name="PageSize"]').val();
        var PageNumber = parseInt($(this).attr('data-page-index'), 10) + 1;
        if (tableName == "DashboardDetails") {
            // LoadDashboardGrid(PageSize, PageNumber)
            SearchDashboard(PageSize, PageNumber)
        }
    });
    $('[data-name="PageSize"]').change(function () {
        var DashboardGridContainer = $('[data-name="DashboardGridContainer"]');
        appCommon.ShowPartialPanelLoader(DashboardGridContainer);
        var tableName = $(this).attr('data-table-name');
        var PageSize = $(this).val();
        var PageNumber = 0;
        if (tableName == "DashboardDetails") {
            //LoadDashboardGrid(PageSize, PageNumber)
            SearchDashboard(PageSize, PageNumber)
        }
    });
}
function BindDashboardSearchEvents() {
    $('[data-name="btnResetDashboard"]').off('click');
    $('[data-name="btnResetDashboard"]').on('click', function () {

        var DashboardSectionPanel = $('[data-name="frmDashboardSearch"]')
        appCommon.ResetFieldsInPanel(DashboardSectionPanel);
        LoadDashboardGrid(defaultPageSize, 0);
    });

    $('[data-name="btnSearchDashboard"]').off('click');
    $('[data-name="btnSearchDashboard"]').on('click', function () {
        var DashboardGridContainer = $('[data-name="DashboardGridContainer"]');
        appCommon.ShowPartialPanelLoader(DashboardGridContainer);
        var queryString = appCommon.BuildQueryStringParameters({ PageSize: defaultPageSize, PageIndex: 0 });
        appCommon.SubmitForm($('[data-name="frmDashboardSearch"]'), function (response) {
            if (response != undefined && response != null) {
                $(DashboardGridContainer).html(response);
                BindDashboardSearchEvents();
                BindPaginationEvents();
            }
        }, "/Search/DashboardSearch" + "?" + queryString, true, false);
    });
    $('[data-name="InsuredName"]').each(function () {
        EnableInsuredNameComplete(this);
    });

    $('[data-name="PolicyNumber"]').each(function () {
        EnablePolicyNumberComplete(this);
    });
    $('[data-name="AgencyName"]').each(function () {
        EnableAgencyNameComplete(this);
    });

};
function LoadDashboardGrid(PageSize, PageNumber) {
    var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageNumber });
    appCommon.RenderHTMLPanel("/Home/DashboardGrid" + "?" + queryString, $('[data-name ="DashboardGridContainer"]'), function () {
        BindDashboardSearchEvents();
        BindPaginationEvents();
    }, true, null);
}
function SearchDashboard(PageSize, PageIndex) {
    var DashboardGridContainer = $('[data-name="DashboardGridContainer"]');
    var queryString = appCommon.BuildQueryStringParameters({ PageSize: PageSize, PageIndex: PageIndex });
    appCommon.SubmitForm($('[data-name="frmDashboardSearch"]'), function (response) {
        $(DashboardGridContainer).html(response);
        BindDashboardSearchEvents();
        BindPaginationEvents();
    }, "/Search/DashboardSearch?" + queryString, true, false
    );
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


