var ExistingForm = null;
var defaultPageSize = MasterLookup._DefaultPageSize;
$(document).ready(function () {
    ClearReportView();
    if ($('[data-name="btnLoadReport"]').length > 0) {
        //  MultiSelection For Agent/Broker
        $(document).on('change', '.custom-multi-selection input[type="checkbox"]', function () {
            var container = $(this).closest('.custom-multi-selection');
            var selectedValues = [];
            container.find('input[type="checkbox"]:checked').each(function () {
                selectedValues.push($(this).val());
            });
            container.find('input[type="hidden"]').val(selectedValues.join(','));
        });
       

        $('[data-name="btnLoadReport"]').click(function () {
            let Report_Id = $(this).attr("data-reportid");

            // Reset the aria-expanded attribute on dropdown toggle buttons for accessibility
            $('.custom-multi-selection .dropdown-menu.show').removeClass('show');            
            $('.custom-multi-selection .dropdown-toggle').attr('aria-expanded', 'false')

            if ($('[name="ReportForm"]').find('.dropdown-toggle').hasClass("show")) {
                $('[name="ReportForm"]').find('.dropdown-toggle').dropdown('toggle');
            }
            if (appCommon.IsValidForm($('[name="ReportForm"]'))) {
                ExistingForm = $('[name="ReportForm"]').clone(false);
                $(ExistingForm).find('select').each(function (s, sitem) {
                    $(sitem).val($('[name="ReportForm"]').find('[name="' + $(sitem).attr("name") + '"]').val());
                });
                SubmitReportForm(defaultPageSize, 0, ExistingForm, Report_Id);
            }
            return false;
        });
    }
    else {
        SubmitReportForm(defaultPageSize, 0, null, 0);
    }

    $('[data-name="imgToggleIcon"]').click(function () {
        $(this).parent().find('.dropdown-toggle').dropdown('toggle');
        return false;
    });

    if ($('[data-name="btnResetReport"]').length > 0) {

        $('[data-name="btnResetReport"]').click(function () {
            var $form = $('form[name="ReportForm"]');
            let Report_Id = $(this).attr("data-reportid");
            $form[0].reset();
            // Reset Default selection (1099 Report)
            var range =$form.find('select[data-name="Range"]');
            if (range.length > 0 && Number(Report_Id) === window.ReportConstants.Report1099)
            {                
              range.val('Year To Date');
              range.trigger('change');
            }
            //  Reset Multi Select
            $('.custom-multi-selection').each(function () {
                var container = $(this);
                container.find('input[type="checkbox"]').prop('checked', false);
                container.find('input[type="hidden"]').val('');
                container.find('button').html('<span class="default-text">--Select--</span>');
            });            
            var $createdDateRadio = $('input[type="radio"][value="InvoiceCreatedDate"]');
            if ($createdDateRadio.length) {
                $createdDateRadio.prop('checked', true).trigger('change');
            }
            ClearReportView();
        });
    }
    else {
        SubmitReportForm(defaultPageSize, 0, null, 0);
    }
});


function SubmitReportForm(PageSize, PageNumber, formToSubmit, Report_Id) {
    ClearReportView();
    var ReportForm = $('[name="ReportForm"]');
    var ReportView = $('[data-name="ReportView"]');
    appCommon.ReArrangeFormInputsGroupSequence($(formToSubmit).find('[data-form-group-item]'));
    appCommon.ShowPartialPanelLoader(ReportView);
    $(ReportView).addClass("vcSpinner");
    appCommon.AdjustWindowMinHeight(ReportView);
    var queryString = appCommon.BuildQueryStringParameters({ Report_Id: Report_Id, PageSize: PageSize, PageIndex: PageNumber });
    appCommon.SubmitForm(formToSubmit, function (response) {
        $(ReportView).removeClass("vcSpinner");
        appCommon.AdjustWindowMinHeight(ReportView);
        $(ReportView).html(response);
    }, "/Reports/Report?" + queryString, false, false);
}
function LoadReportFromForm(PageSize, PageNumber, Report_Id) {
    var $form = $('[name="ReportForm"]');

    if ($form.find('.dropdown-toggle').hasClass("show")) {
        $form.find('.dropdown-toggle').dropdown('toggle');
    }

    if (appCommon.IsValidForm($form)) {
        ExistingForm = $form.clone(false);

        $(ExistingForm).find('select').each(function (s, sitem) {
            $(sitem).val($form.find('[name="' + $(sitem).attr("name") + '"]').val());
        });

        SubmitReportForm(PageSize, PageNumber, ExistingForm, Report_Id);
    }
}

function BindReportFormInputChange(Panel, SQLQuery_Id, SQLParameter_Id) {
    $(Panel).find('input,select,textarea').change(function () {
        var panelToLoadDependencyParameters = $("[data-dependency-sqlparameter-panel='" + SQLParameter_Id + "']");
        HandleAllReferenceChilds(panelToLoadDependencyParameters);
        var value = $(this).val().trim();
        if (value != "") {
            var FormGroups = $('[name="ReportForm"]').find('[data-form-group-item]').length;
            var queryString = appCommon.BuildQueryStringParameters({ SQLParameter_Id: SQLParameter_Id, SQLQuery_Id: SQLQuery_Id, InputValue: value, FormGroups: FormGroups });
            appCommon.RenderHTMLPanel("/Reports/RenderSQLDependencyInputs?" + queryString, panelToLoadDependencyParameters, function () {
                $(panelToLoadDependencyParameters).css("min-height", "0px");
            }, false, function () {
                $(panelToLoadDependencyParameters).html("");
                $(panelToLoadDependencyParameters).css("display", "block");
                if (typeof PartialPanelLoader != 'undefined' && PartialPanelLoader != null) {
                    $(panelToLoadDependencyParameters).append($(PartialPanelLoader));
                    $(panelToLoadDependencyParameters).find('[data-name="PartialPanelLoader"]').addClass("vcSpinner");
                }
            });
        }
    });
}

function HandleAllReferenceChilds(panelToLoadDependencyParameters) {
    var referenceChild = $(panelToLoadDependencyParameters).find("input[type='hidden']").attr("data-name");
    $(panelToLoadDependencyParameters).html("");
    $(panelToLoadDependencyParameters).css("display", "none");
    while (typeof referenceChild != 'undefined' && referenceChild != null && referenceChild != "" && referenceChild > 0) {
        var panelToLoadDependencyParameters = $("[data-dependency-sqlparameter-panel='" + referenceChild + "']");
        var referenceChild = $(panelToLoadDependencyParameters).find("input[type='hidden']").attr("data-name");
        $(panelToLoadDependencyParameters).html("");
        $(panelToLoadDependencyParameters).css("display", "none");
    };
}


$(function () {
    $('[data-name]').each(function () {
        const name = $(this).data('name');
        if (name === "InvoiceNumber")
        {
            EnableInvoiceNumberComplete(this);
        }
        if (name === "InsuredName") {
            EnableInsuredNameComplete(this);
        }
        if (name === "PolicyNumber") {
            EnableInsuredNameComplete(this);
        }
    });
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