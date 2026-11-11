var defaultPageSize = MasterLookup._DefaultPageSize;
$(document).ready(function () {
    MainPanel = $('#maincontent');

    var stepsbar = $('.stepsbar');

    if ($(window).width() < 1023) {
        $('.main-content .container-fluid').first().before(stepsbar);
    }
    else {
        $('.page-header .user-info').first().before(stepsbar);
    }

    $('body').on('click', '.backdrop', function () {
        $('.backdrop').hide();
        $('.policy-menu').hide();
    })

    $(window).on('resize', function () {
        if ($(window).width() > 1023) {
            $('.policy-menu').show()
        } else {
            $('.policy-menu').hide()
            $('.backdrop').remove();
            $('.policy-menu').removeClass('d-none');
            if ($('.policy-menu').hasClass('menu-lean')) {
                $('.policy-menu').removeClass('menu-lean')
            }
        }
    })

    $('.policy-menu-btn').on('click', function () {
        $('.card .policy-menu').show()
        $('body').append('<div class="backdrop"></div>').show();
    });

    PartialPanelLoader = $('[data-name="PartialPanelLoader"]');
    if (typeof PartialPanelLoader != 'undefined' && PartialPanelLoader != null) {
        PartialPanelLoader.removeClass("invisible");
        var copyOfPartialPanelLoader = $(PartialPanelLoader).clone(false);
        $(PartialPanelLoader).remove();
        PartialPanelLoader = copyOfPartialPanelLoader;
    }
    InitializeFlatpickr = function () {
        flatpickr(".flatpickr", {
            dateFormat: "m/d/Y",
            disableMobile: true,
           // maxDate: "today"
        });

        flatpickr(".flatpickrRange", {
            mode: "range",
            minDate: "today",
            dateFormat: "Y-m-d"
        });

        toDatePicker = flatpickr("input[name='PaymentToDate']", {
            dateFormat: "m-d-Y",
            disableMobile: true,
            maxDate: "today"
        });

        fromDatePicker = flatpickr("input[name='PaymentFromDate']", {
            dateFormat: "m-d-Y",
            disableMobile: true,
            maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];

                    toDatePicker.set("minDate", fromDate);
                    var toDateVal = toDatePicker.input.value;
                    if (toDateVal) {
                        var toDate = toDatePicker.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            toDatePicker.clear();
                        }
                    }
                }
            }
        });

        //Invoice Production Report Created Date radio button
        invoiceCreatedToDate = flatpickr("input[data-name='InvoiceCreatedToDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            //maxDate: "today"
        });

        invoiceCreatedFromDate = flatpickr("input[data-name='InvoiceCreatedFromDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
           // maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];
                    invoiceCreatedToDate.set("minDate", fromDate);
                    var toDateVal = invoiceCreatedToDate.input.value;
                    if (toDateVal) {
                        var toDate = invoiceCreatedToDate.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            invoiceCreatedToDate.clear();
                        }
                    }
                }
            }
        });

        // Invoice Production Report Effective Date radio button
        InvoiceEffectiveToDate = flatpickr("input[data-name='InvoiceEffectiveToDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today"
        });

        InvoiceEffectiveFromDate = flatpickr("input[data-name='InvoiceEffectiveFromDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];
                    InvoiceEffectiveToDate.set("minDate", fromDate);
                    var toDateVal = InvoiceEffectiveToDate.input.value;
                    if (toDateVal) {
                        var toDate = InvoiceEffectiveToDate.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            InvoiceEffectiveToDate.clear();
                        }
                    }
                }
            }
        });
        // Invoice Production Report Transaction Effective Date radio button
        TransactionEffectiveToDate = flatpickr("input[data-name='TransactionEffectiveToDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            //maxDate: "today"
        });

        TransactionEffectiveFromDate = flatpickr("input[data-name='TransactionEffectiveFromDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            //maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];
                    InvoiceEffectiveToDate.set("minDate", fromDate);
                    var toDateVal = InvoiceEffectiveToDate.input.value;
                    if (toDateVal) {
                        var toDate = InvoiceEffectiveToDate.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            InvoiceEffectiveToDate.clear();
                        }
                    }
                }
            }
        });
        // Payment Report
        PaymentToDate = flatpickr("input[data-name='PaymentToDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today"
        });

        PaymentFromDate = flatpickr("input[data-name='PaymentFromDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];
                    PaymentToDate.set("minDate", fromDate);
                    var toDateVal = PaymentToDate.input.value;
                    if (toDateVal) {
                        var toDate = PaymentToDate.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            PaymentToDate.clear();
                        }
                    }
                }
            }
        });

        // Transaction Report
        TransactionToDate = flatpickr("input[data-name='TransactionToDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today"
        });

        TransactionFromDate = flatpickr("input[data-name='TransactionFromDate']", {
            dateFormat: "m/d/Y",
            disableMobile: true,
            maxDate: "today",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length > 0) {
                    var fromDate = selectedDates[0];
                    TransactionToDate.set("minDate", fromDate);
                    var toDateVal = TransactionToDate.input.value;
                    if (toDateVal) {
                        var toDate = TransactionToDate.parseDate(toDateVal, "m-d-Y");
                        if (toDate < fromDate) {
                            TransactionToDate.clear();
                        }
                    }
                }
            }
        });
        //Enable future date option for the Effective Date radio button in the Report section.
        $(document).on("change", "input[type='radio']", function () {
            var activeReportName = $("[data-name='btnLoadReportForm'].active").find("span").text();
            var selectedText = $(this).closest(".form-check").find("label").attr("data-name");
            var EffectiveFromDate = $("input[data-name='InvoiceEffectiveFromDate']")[0]._flatpickr;
            var EffectiveToDate = $("input[data-name='InvoiceEffectiveToDate']")[0]._flatpickr;

            if (activeReportName == "Invoice Production Report" && selectedText == "Effective Date") {
                EffectiveFromDate.set("maxDate", null);
                EffectiveToDate.set("maxDate", null);
            }
            else {
                EffectiveFromDate.clear();
                EffectiveToDate.clear();
            }
        });

        ["DueFromDate", "DueToDate", "InvoiceCreatedFromDate", "InvoiceCreatedToDate", "InvoiceEffectiveFromDate", "InvoiceEffectiveToDate", "As_Of_Date", "TransactionEffectiveFromDate", "TransactionEffectiveToDate", "ReportingDateFrom", "ReportingDateTo"].forEach(function (name) {
            var DateField = $("input[data-name='" + name + "']");
            if (DateField.length && DateField.data("DateTimePicker")) {
                DateField.datetimepicker("destroy");
            }
        });
        DueToDate = flatpickr("input[data-name='DueToDate']", {
            dateFormat: "m/d/Y"
        });
        DueFromDate = flatpickr("input[data-name='DueFromDate']", {
            dateFormat: "m/d/Y"
        });
    };
    $('[data-name="btnAddPaymentDetails"]').on('click', function () {
        _AppMainPanel.LoadPaymentDetails(null, function () {

        });
    });

    var path = window.location.pathname.toLowerCase();
    if (path.includes('/systemconfiguration/index')) {
        $('[data-name="SelectCompanyUserSettings"]').hide();
    } else {
        $('[data-name="SelectCompanyUserSettings"]').show();
    }
});
var MainPanel = null;
var PartialPanelLoader = null;
AppMainPanel = function () {
    return {
        LoadPaymentDetails: function (Payment_Id, CallbackFun) {
            if (typeof Payment_Id == "undefined" || Payment_Id == "") {
                Payment_Id = null;
            }
            appCommon.ExecuteAjaxGetMethod("/Payments/PaymentDetails" + (Payment_Id != null ? ("?Payment_Id=" + Payment_Id) : ""), function (response) {
                if (response != null && response != undefined) {
                    var OffCanvasTitle = Payment_Id != null ? "Edit Payment Details" : "Add Payment Details";
                    appCommon.OffcanvasGeneralFunction(OffCanvasTitle, response);
                    if (typeof CallbackFun != "undefined" && CallbackFun != null) {
                        CallbackFun();
                        BindPaymentClickEvents();
                        InitializeFlatpickr();
                    }
                }
            }, true);
        },
        LoadPolicyDetails: function (PolicyDetails_Id) {
            if (typeof PolicyDetails_Id == "undefined" || PolicyDetails_Id == "")
                PolicyDetails_Id = null;
            appCommon.RenderHTMLPanel("/PolicyDetails/Index" + (PolicyDetails_Id != null ? ("/" + PolicyDetails_Id) : ""), MainPanel, function () { }, true, null, true);
        },
        LoadInvoices: function (PolicyDetails_Id) {
            if (typeof PolicyDetails_Id == "undefined" || PolicyDetails_Id == "")
                PolicyDetails_Id = null;
            appCommon.RenderHTMLPanel("/Invoices/Index" + (PolicyDetails_Id != null ? ("/" + PolicyDetails_Id) : ""), MainPanel, function () { }, true, null, true);
        },
        TableHeaderSearchandButtonPanel: function (header, placeholder, buttonHtml, selectors) {
            if (!selectors || !selectors.header || !selectors.search || !selectors.button) {
                return;
            }

            $(selectors.header).html(header || "");
            $(selectors.search).attr("placeholder", placeholder || "");
            $(selectors.button).html(buttonHtml || "");
        }
        //TableHeaderSearchandButtonPanel: function (header, placeholder, buttontext) {
        //    $('[data-name="lblPageName"]').html("").html(header);
        //    $('[data-name="txtSearch"]').attr("placeholder", placeholder);
        //    $('[data-name="btnAddBank"]').html("").html(buttontext);
        //}
    }
}
function BindPaymentClickEvents() {
    $('[data-name="btnSavePaymentDetails"]').off('click');
    $('[data-name="btnSavePaymentDetails"]').on('click', function () {
        var PaymentDetailsForm = $('[data-name="frmPaymentDetails"]');
        if (appCommon.IsValidForm(PaymentDetailsForm)) {
            appCommon.RemoveCurrencyFormatForPanel(PaymentDetailsForm);
            appCommon.SubmitForm(PaymentDetailsForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        var isEdit = $('[data-name="frmPaymentDetails"]').find('[name="IsEdit"]').val();
                        if (isEdit === "True" || isEdit === "true") {
                            appCommon.ShowMessageModal("Payment updated successfully.", "Success");
                        } else {
                            appCommon.ShowMessageModal("Payment created successfully.", "Success");
                        }
                        $('[data-name="OffCanvasRightGeneral"]').offcanvas('hide');
                        if (typeof LoadPaymentsGrid === 'function') {
                            LoadPaymentsGrid(defaultPageSize, 0);
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
                appCommon.HideLoader();
            }, "/Payments/PaymentDetails", false, true);
        }
    });
    $('[data-name="EntityName"]').each(function () {
        EnableEntityNameComplete(this);
    });
    $('[data-name="EntityType_Id"]').off('change').on('change', function () {
        var entityNameInput = $(this).closest('[data-name="PaymentsSection"]').find('[data-name="EntityName"]');
        entityNameInput.val("");
        entityNameInput.parent().find('[data-name="Entity_Id"]').val("");
    })
}

$(document).on('click', '[data-name="AddEntityName"]', function () {
    var form = $('[data-name="frmPaymentDetails"]');
    var entityTypeId = form.find('[name="EntityType_Id"]').val();
    if (!entityTypeId) {
        appCommon.ShowMessageModal("Please select an Entity Type before adding a new Entity Name.");
        return;
    }

    appCommon.ExecuteAjaxGetMethod("/Payments/ParentEntityName", function (response) {
        if (response != null && response != undefined) {
            appCommon.GenericModalFunction("Add Entity Name", response);
            $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "600px");
            $('[data-name="frmParentEntityName"] [name="EntityType_Id"]').val(entityTypeId);
        }
    }, true);
});

$(document).on('click', '[data-name="btnSaveParentEntityName"]', function () {
    var formParentEntityName = $('[data-name="frmParentEntityName"]');
    if (appCommon.IsValidForm(formParentEntityName)) {
        appCommon.SubmitForm(formParentEntityName, function (response) {
            if (response != null && typeof response != 'undefined') {
                $('[data-name="GenericModal"]').modal('hide');
                if (response.Status) {
                    appCommon.ShowMessageModal(response.Message, "Success");                    
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
        }, "/Payments/ParentEntityName", false, true);
    }
});
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
_AppMainPanel = new AppMainPanel();
