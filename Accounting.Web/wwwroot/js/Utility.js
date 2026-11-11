try {
    var regExpDate = /^(0?[1-9]|[12][0-9]|3[01])[\/\-](0?[1-9]|1[012])[\/\-]\d{4}$/;

    if (typeof ServerDateFormat === 'undefined' || ServerDateFormat == null || ServerDateFormat == "") {
        ServerDateFormat = "MM/DD/YYYY";
    }
    if (ServerDateFormat == "MM/DD/YYYY") {
        regExpDate = /^(0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])[\/\-]\d{4}$/;
    }
    var regExpEmail = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i;
    var regExpMobilePattern = new RegExp("^[0-9]{10}$");
    var regNumber = /^\d+$/;
    var regDecimal = /^\d+(\.\d{1,10})?$/;
    var regOnlyAlphabets = /^[a-zA-Z ]*$/;
    var regOnlyAlphaNumeric = /^[a-zA-Z0-9]*$/;
    var regExpZincode_In = new RegExp("^[0-9]{5}$");
    var regExpZipcode_US = new RegExp("^[0-9]{5}$");
    var regExpZipcodeExtension = new RegExp("^[0-9]{4}$");
    var regPannumber = new RegExp("^[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}$");
    var regGSTIN = new RegExp("^([0]{1}[1-9]{1}|[1-2]{1}[0-9]{1}|[3]{1}[0-7]{1})([a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}[zZ]{1}[0-9a-zA-Z]{1})+$");
    var regRepeatedCharacters = /(.)\1/;
    var regExpCurrencyUSD = /^\$?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9]{0,8})?$/;
    var regExpCurrencyINR = new RegExp('^(?:0|[1-9]\d*)(?:\.(?!.*000)\d+)?$');
    var _LookUpCurrencyFormat = { USD: 'USD', INR: 'INR' };
    var _LookUpZipcodeFormat = { US: 'US', IN: 'IN' };
    var regExpUrl = /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$/;
    var regExpSSN = /^\d{3}-?\d{2}-?\d{4}$/;
    var regExpFEIN = /^\d{2}-?\d{7}$/;
    var regExpCC = /^[0-9]{13,16}$/;
    var regExpCVV = /^[0-9]{3,4}$/;
    var regExpCCVisa = /^4[0-9]{6,}$/;
    var regExpCCMastercard = /^5[1-5][0-9]{5,}|222[1-9][0-9]{3,}|22[3-9][0-9]{4,}|2[3-6][0-9]{5,}|27[01][0-9]{4,}|2720[0-9]{3,}$/;
    var regExpCCAmericanExpress = /^3[47][0-9]{5,}$/;
    var regExpCCDiscover = /^6(?:011|5[0-9]{2})[0-9]{3,}$/;
    var regExpCCJCB = /^(?:2131|1800|35[0-9]{3})[0-9]{3,}$/;
    var regExpCCDinersClub = /^3(?:0[0-5]|[68][0-9])[0-9]{4,}$/;
    var regExpbase64Format = /[A-Za-z0-9+/=]/;
    var regExpGUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    var _BrowserType = { IE: 'IE', Chrome: 'Chrome' };
    var CurrentBrowser = null;
    var PanelMode = { Show: "Show", Hide: "Hide", Disable: "Disable" };
    var CCType = { Visa: 1, Mastercard: 2, AmericanExpress: 3, Discover: 4, JCB: 5, DinersClub: 6 };
    var InvalidFEINOrSSNList =
        [
            "111111111",
            "222222222",
            "333333333",
            "444444444",
            "555555555",
            "666666666",
            "777777777",
            "888888888",
            "999999999",
            "000000000",
            "123456789",
            "234567891",
            "345678912",
            "456789123",
            "567891234",
            "678912345",
            "789123456",
            "891234567",
            "912345678",
            "987654321",
            "876543219",
            "765432198",
            "654321987",
            "543219876",
            "432198765",
            "321987654",
            "219876543",
            "198765432"
        ];
    var HttpStatusCode = { OK: 200, Created: 200, Accepted: 201, NoContent: 204, PartialContent: 206, TemporaryRedirect: 307, BadRequest: 400, Unauthorized: 401, Forbid: 403, NotFound: 404, RequestTimeout: 408, UnProcessableEntity: 422 };
    const currencies = [
        { Currency: "Brazilian Real", CurrencyCode: "BRL", Locale: "pt-BR", RegularExpression: /^-?R\$\s?\d{1,3}(\.\d{3})*(,\d{2})?$/ },
        { Currency: "Canadian Dollar", CurrencyCode: "CAD", Locale: "en-CA", RegularExpression: /^-?\$\s?\d{1,3}(,\d{3})*(\.\d{2})?$/ },
        { Currency: "Euro", CurrencyCode: "EUR", Locale: "fr-FR", RegularExpression: /^-?\€?\s?((\d{1,3}([.,]\d{3})*)|(\d+))([.,]\d{2})?\s?\€?$/ },
        { Currency: "Pound Sterling", CurrencyCode: "GBP", Locale: "en-GB", RegularExpression: /^-?\£\s?\d{1,3}(,\d{3})*(\.\d{2})?$/ },
        { Currency: "Indian Rupee", CurrencyCode: "INR", Locale: "en-IN", RegularExpression: /^(?:0|[1-9]\d*)(?:\.(?!.*000)\d+)?$/ },
        { Currency: "Yen", CurrencyCode: "JPY", Locale: "ja-JP", RegularExpression: /^-?\¥\s?\d{1,3}(,\d{3})*$/ },
        { Currency: "US Dollar", CurrencyCode: "USD", Locale: "en-US", RegularExpression: /^\$?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9]{0,8})?$/ }
    ];
    try {
        var ua = window.navigator.userAgent;
        var msie = ua.indexOf("MSIE ");
        if (msie > 0) // If Internet Explorer, return version number
        {
            CurrentBrowser = _BrowserType.IE;
        }
    }
    catch (ex) { }

    try {
        //repeat function is not supporetd IE. Expliciltly adding this function for IE.
        //if (CurrentBrowser == _BrowserType.IE)
        String.prototype.repeat = function (num) {
            return new Array(num + 1).join(this);
        }
    }
    catch (ex) { }

    try {
        Date.prototype.addDays = function (days) {
            this.setDate(this.getDate() + parseInt(days));
            return this;
        };
    } catch (e) {
    }
    (function ($) {


        $.fn.U_MakeDefaultButton = function (object) {
            var returncallback = function (event) {
                try {
                    var charCode = (event.which) ? event.which : event.keyCode
                    if (charCode == 13) {
                        $(object).click();
                    }
                }
                catch (exception) {
                    // U_ShowError(exception);
                }
            };
            return this.each(function () {
                // $(this).keypress(returncallback);
            });
        }
        $(document).ready(function () {
            appCommon.InitializeValidations();
        });
        $(document).ajaxComplete(function (event, xhr, options) {
            if (typeof xhr != "undefined" && xhr != null && typeof xhr.status != "undefined") {
                if (xhr.status == HttpStatusCode.BadRequest || xhr.status == HttpStatusCode.Forbid || xhr.status == HttpStatusCode.UnProcessableEntity) {
                    var title = "";
                    if (xhr.status == HttpStatusCode.Forbid)
                        title = "Access Denied";
                    appCommon.ShowMessageModal(xhr.responseText, title);
                }
                else if (xhr.status == HttpStatusCode.Unauthorized) {
                    location.reload();
                }

            }
        });

        $.fn.U_Validate = function (form) {
            var returncallback = function () {
                $(form).find("input[type=text],input[type=email],input[type=password]").each(function (i, item) {
                    if ($(item).val().trim() == "") {
                        $("#p" + $(item).attr("id") + "_GeneratedErrorId").html("This field is required.")
                    }
                });
                try {
                }
                catch (exception) { }
                return false;
            };
            return this.each(function () {
                $(this).click(returncallback);
            });
        };
        $.fn.extend(
            {
                CustomSelect: function (data) {
                    $(this).each(function (c, CustomSelectPanel) {
                        //var CustomSelectPanel = $(this);
                        $(this).find('[data-name="option"]').each(function () {
                            $(this).click(function () {
                                if (data != null && data.items != null && data.items.length > 0) {
                                    var uid = $(this).attr('data-uid');
                                    if (uid != null && uid != 'undefined') {
                                        $(CustomSelectPanel).find('[data-selecteduid]').val(uid);
                                        if (data.uidName != null && data.uidName != 'undefined') {
                                            var selectedItem =
                                                data.items.filter(element => element[data.uidName] == uid);
                                            if (selectedItem != null && selectedItem != 'undefined' && selectedItem.length > 0) {
                                                //var optionSelectedTemplate = $(CustomSelectPanel).find('[data-name="optionSelectedTemplate"]');
                                                //if (optionSelectedTemplate != null && optionSelectedTemplate != 'undefined' && optionSelectedTemplate.length > 0)
                                                appCommon.RenderObjectData($(CustomSelectPanel), selectedItem[0]);
                                            }
                                        }
                                    }
                                }
                            });
                        });
                    });
                }
            });
    })(jQuery);
    //to format the string
    if (!String.prototype.format) {
        String.prototype.format = function () {
            var args = arguments;
            return this.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined'
                    ? args[number]
                    : match
                    ;
            });
        };
    }
    //example for to format string
    //  console.log("{0} is dead, but {1} is alive! {0} {1}".format("ASP", "ASP.NET"));

    App = function () {
        return {
            InitializeValidations: function () {
                $('form').each(function (i, form) {
                    appCommon.InitializePanelOrFormValidations(form);
                });


            },
            InitializePanelOrFormValidations: function (form) {
                if (form.tagName == "FORM") {
                    $(form).attr("novalidate", "novalidate");
                    $(form).submit(function (e) {
                        //e.preventDefault();
                        return appCommon.IsValidForm(form);
                    });
                }
                $(form).find("[data-number-panel-sum]").each(function (i, item) {
                    appCommon.CreateErrorControl(form, this);
                });
                $(form).find("[data-val='true']").each(function (i, item) {
                    appCommon.CreateErrorControl(form, this);
                    appCommon.BindValidationEvent(this, appCommon.DefaultEventTypes);
                });
                $(form).find("[data-val-type='number'],[data-val-type='phone'],[data-val-type='zipcode'],[data-val-type='zipcodeextension'],[data-val-type='alphabets'],[data-val-type='alphanumeric'],[data-val-type='pannumber'],[data-val-type='email'],[data-val-type='maskdob'],[data-val-type='url'],[data-val-type='currency'],[data-val-type='cc'],[data-val-type='cvv'],[data-val-type='fein'],[data-val-type='ssn'],[data-val-type='decimal']").each(function (i, item) {
                    if ($(this).attr("data-val") != true) {
                        appCommon.CreateErrorControl(form, this);
                        appCommon.BindValidationEvent(this, appCommon.DefaultEventTypes);
                    }
                });
                $(form).find("[data-val-type='date']").each(function (i, item) {
                    var ViewMode = "years";
                    var maxDate = new Date();
                    var month = maxDate.getMonth();
                    var day = maxDate.getDate();
                    var minDate = new Date((new Date().getFullYear() - 100), month, day);
                    try {
                        if (typeof $(item).attr("data-val-date-max-months") != "undefined") {
                            month = month + parseInt($(item).attr("data-val-date-max-months"));
                            maxDate = new Date(new Date().getFullYear(), month, day);
                        }
                    }
                    catch (exception) {
                    }
                    try {
                        if (typeof $(item).attr("data-val-date-maxyears") != "undefined") {
                            maxDate = new Date(new Date().getFullYear() + parseInt($(item).attr("data-val-date-maxyears")), month, day);
                        }
                        if (typeof $(item).attr("data-val-date-minyears") != "undefined") {
                            minDate = new Date(new Date().getFullYear() - parseInt($(item).attr("data-val-date-minyears")), month, day);
                        }
                    }
                    catch (exception) {
                    }

                    try {
                        if (typeof $(item).attr("data-date-viewmode") != "undefined") {
                            var viewmodeattr = $(item).attr("data-date-viewmode");
                            if (viewmodeattr == "years" || viewmodeattr == "months" || viewmodeattr == 'days') {
                                ViewMode = viewmodeattr;
                            }
                        }
                    }
                    catch (ex) { }

                    $(this).datetimepicker({
                        useCurrent: false,
                        format: ServerDateFormat,
                        viewMode: ViewMode,
                        minDate: minDate,
                        maxDate: maxDate,
                        widgetPositioning: {
                            horizontal: "auto",
                            //vertical: "top",
                        }
                    }).on("dp.change", function (e) {
                        if (typeof $(this).attr("data-val-date-autofill") != "undefined") {
                            var targetControl = $('[name="' + $(this).attr("data-val-date-autofill") + '"]');
                            if (typeof targetControl != 'undefined') {
                                if (typeof $(this).attr("data-val-date-autofill-mode") != "undefined" && typeof $(this).attr("data-val-date-autofill-mode-val") != "undefined") {
                                    var autofillMode = $(this).attr("data-val-date-autofill-mode");
                                    var autofillModeValue = parseInt($(this).attr("data-val-date-autofill-mode-val"));
                                    if (autofillMode == "Year") {
                                        var originalDate = new Date(e.target.value);
                                        var oneYearFromOriginalDate = originalDate.setFullYear(originalDate.getFullYear() + autofillModeValue);
                                        var oneYearFromOriginalDateFormatted = appCommon.ConvertStringDateToDateWithFormat(oneYearFromOriginalDate, ServerDateFormat);
                                        targetControl.val(oneYearFromOriginalDateFormatted);
                                    }
                                }
                            }
                        }
                    });


                    try {
                        var inputDateCtrl = $(this);
                        if ($(inputDateCtrl).parent().find(".input-group-addon").length > 0) {
                            $(inputDateCtrl).parent().find(".input-group-addon").click(function () {
                                $(inputDateCtrl).focus();
                            });
                        }
                        if ($(inputDateCtrl).parent().find(".input-group-text").length > 0) {
                            $(inputDateCtrl).parent().find(".input-group-text").click(function () {
                                $(inputDateCtrl).focus();
                            });
                        }
                        if ($(inputDateCtrl).closest(".input-group").find("[data-icon]").length > 0) {
                            $(inputDateCtrl).closest(".input-group").find("[data-icon]").click(function () {
                                $(inputDateCtrl).focus();
                            });
                        }
                    }
                    catch (ex) {

                    }
                });
                $(form).find("select").each(function (i, item) {
                    if (typeof $(this) != "undefined" && $(this).val() != null && $(this).val().indexOf("?") >= 0) {
                        $(this).val("");
                    }
                });
                //Sample Json: data-panel-dependency="MailingAddressPanel" data-panel-dependency-json='[{"RefName":"IsMailAddressSameAsBusiness","Values":[true],"ShowPanel":true}]'
                $(form).find('[data-panel-dependency]').each(function (i, item) {
                    try {
                        if (typeof $(item).attr("data-panel-dependency-json") != 'undefined' && $(item).attr("data-panel-dependency-json") != '') {
                            try {
                                var JsonDependencyInput = JSON.parse($(item).attr("data-panel-dependency-json"));
                                if (JsonDependencyInput != null) {
                                    $(JsonDependencyInput).each(function (j, jsonItem) {
                                        if (typeof jsonItem.RefName != 'undefined' && jsonItem.RefName != '' && jsonItem.RefName != null && typeof jsonItem.Values != 'undefined' && jsonItem.Values != '' && jsonItem.Values != null)
                                            if (typeof $('[name="' + jsonItem.RefName + '"]') != 'undefined' && $('[name="' + jsonItem.RefName + '"]').length > 0) {
                                                var controlType = $('[name="' + jsonItem.RefName + '"]')[0].tagName == "INPUT" ? $('[name="' + jsonItem.RefName + '"]').attr("type").toLowerCase() : $('[name="' + jsonItem.RefName + '"]')[0].tagName.toLowerCase();
                                                var DefaultDependencyEventTypes = 'focusout';
                                                if (controlType == 'radio' || controlType == 'checkbox') {
                                                    DefaultDependencyEventTypes = "click";
                                                }
                                                else if (controlType == "select") {
                                                    DefaultDependencyEventTypes = "change";
                                                }
                                                $(jsonItem.Values).each(function (v, value) {
                                                    jsonItem.Values[v] = jsonItem.Values[v].toString();
                                                });
                                                var jsonItemRef = $('[name="' + jsonItem.RefName + '"]');
                                                $(jsonItemRef).on(DefaultDependencyEventTypes, function () {
                                                    appCommon.ToggleJsonPanel(item, JsonDependencyInput, true);
                                                });
                                            }
                                    });
                                    appCommon.ToggleJsonPanel(item, JsonDependencyInput);
                                }
                            }
                            catch (ex) { }
                        }
                    }
                    catch (ex) { }
                });

                $(form).find('[data-val-required-dependency]').each(function (i, item) {
                    try {
                        if (typeof $(item).attr("data-val-required-dependency") != 'undefined' && $(item).attr("data-val-required-dependency") != '') {
                            try {
                                var JsonInput = JSON.parse($(item).attr("data-val-required-dependency"));
                                if (typeof JsonInput != 'undefined' && JsonInput != null && JsonInput != "") {
                                    $(JsonInput).each(function (j, jsonItem) {
                                        if (typeof jsonItem.Reference != 'undefined' && jsonItem.Reference != '' && jsonItem.Reference != null) {
                                            var attributeIdentifier = "name";
                                            if (typeof jsonItem.AttributeIdentifier != 'undefined' && jsonItem.AttributeIdentifier != '' && jsonItem.AttributeIdentifier != null) {
                                                attributeIdentifier = jsonItem.AttributeIdentifier;
                                            }
                                            var ctrl = $('[' + attributeIdentifier + '="' + jsonItem.Reference + '"]');
                                            if (typeof $(ctrl) != 'undefined' && $(ctrl).length > 0) {
                                                $(ctrl).blur(function () {
                                                    var attr = $(item).attr("data-val");
                                                    if ($(this).val().trim() != "") {
                                                        if (typeof attr === 'undefined')
                                                            $(item).attr("data-val", true);
                                                    }
                                                    else {
                                                        //if (typeof attr !== 'undefined')
                                                        //$(item).removeAttr("data-val", true);
                                                        appCommon.OffRequiredEvent(item);
                                                    }
                                                });
                                            }
                                        }
                                    });
                                }
                            }
                            catch (ex) { }
                        }
                    }
                    catch (ex) { }
                });

                $(form).find('[data-searchable]').each(function (ds, dsinput) {

                    //This is for to enable Search
                    var searchableMenuItem = $(this).closest("[data-searchable]").parent().find("[data-searchable-panel]");
                    if (searchableMenuItem != null && typeof searchableMenuItem != 'undefined') {
                        //Add hidden filed for selected value identification
                        var controlName = $(dsinput).attr("name");
                        if (typeof controlName == "undefined") {
                            controlName = $(dsinput).attr("id");
                        }
                        if (typeof controlName == "undefined") {
                            controlName = appCommon.GenerateGUID();
                        }
                        var inputHiddenIdentifier = document.createElement("input");
                        inputHiddenIdentifier.setAttribute("type", "hidden");
                        inputHiddenIdentifier.setAttribute('name', controlName);
                        inputHiddenIdentifier.setAttribute('selected-data-uid', controlName);

                        $(inputHiddenIdentifier).insertAfter(dsinput);
                        $(dsinput).attr("name", controlName + "_" + appCommon.GenerateGUID());

                        var triggeredDefaultLoadKeyUpEvent = true;

                        //$(dsinput).on("keyup", function (event) {
                        //    var charCode = (event.which) ? event.which : event.keyCode
                        //    if (charCode == 13) {
                        //        event.preventDefault();
                        //        return false;
                        //    }
                        //});
                        $(dsinput).click(function () {
                            $(searchableMenuItem).closest(".dropdown-menu").scrollTop(0);//set to top
                            if ($(searchableMenuItem).find("[data-searchable-item].active").length > 0) {
                                var currentOffset = $(searchableMenuItem).find("[data-searchable-item].active").offset().top;
                                //var customDDHeight = $(searchableMenuItem).height();
                                var customDDHeight = $(searchableMenuItem).find("[data-searchable-item]:first-child").offset().top;
                                $(searchableMenuItem).scrollTop((currentOffset - customDDHeight));//then set equal to the position of the selected element minus the height of scrolling div
                            }
                        });
                        $(dsinput).on("keydown", function (event) {
                            var charCode = (event.which) ? event.which : event.keyCode
                            if (charCode == 13) {
                                event.preventDefault();
                            }
                        });
                        $(dsinput).on("keyup", function (event) {
                            //event.preventDefault();
                            var charCode = (event.which) ? event.which : event.keyCode
                            if ((charCode == 13 || charCode == 38 || charCode == 40) && !$(searchableMenuItem).hasClass("show")) {
                                $(dsinput).click();
                            }
                            if (charCode == 13) {
                                $(searchableMenuItem).find("[data-searchable-item]:not(.inactive)").each(function (i) {
                                    if ($(this).hasClass("active")) {
                                        $(this).click();
                                    }
                                });
                                return false;
                            }
                            else if (charCode == 38 || charCode == 40) {
                                //$(object).click();
                                var previousActiveItemIndex = -1;
                                var listItemsCount = $(searchableMenuItem).find("[data-searchable-item]:not(.inactive)").length;
                                var previousLi = null;
                                if (listItemsCount == 0)
                                    return false;
                                $(searchableMenuItem).find("[data-searchable-item]:not(.inactive)").each(function (i) {
                                    if ($(this).hasClass("active")) {
                                        $(this).removeClass("active")
                                        previousActiveItemIndex = i;
                                        previousLi = this;
                                        return false;
                                    }
                                });

                                if ((previousActiveItemIndex == 0 && charCode == 38) || (previousActiveItemIndex + 1 == listItemsCount && charCode == 40)) {
                                    if (previousLi != null) {
                                        $(previousLi).addClass("active");
                                    }
                                    return false;
                                }
                                var currentIndexTobeActivated = previousActiveItemIndex;
                                if (charCode == 40)
                                    currentIndexTobeActivated++;
                                else
                                    currentIndexTobeActivated--;
                                $(searchableMenuItem).find("[data-searchable-item]:not(.inactive)").each(function (c) {
                                    if (c == currentIndexTobeActivated) {
                                        $(this).addClass("active");
                                        //$(this).closest("tr").focus();
                                        $(searchableMenuItem).scrollTop(0);//set to top
                                        var currentOffset = $(this).offset().top;
                                        var customDDHeight = $(searchableMenuItem).height();
                                        console.log(currentOffset + "            " + customDDHeight);
                                        $(searchableMenuItem).scrollTop((currentOffset - customDDHeight));//then set equal to the position of the selected element minus the height of scrolling div
                                        return false;
                                    }
                                });
                                return false;
                            }
                            var searchValue = $(this).val().trim();
                            if (searchValue != null && searchValue != 'undefined') {
                                $(searchableMenuItem).find("[data-searchable-item]").removeClass("active");
                                $(searchableMenuItem).find("[data-searchable-item]").each(function () {
                                    var liValue = $(this).text().trim();
                                    if (liValue != null && liValue != "" && liValue.toLowerCase().indexOf(searchValue.toLowerCase()) >= 0)
                                        $(this).removeClass("inactive");
                                    else
                                        $(this).addClass("inactive");
                                });
                            }
                            else {
                                $(searchableMenuItem).find("[data-searchable-item]").each(function () {
                                    $(this).removeClass("inactive");
                                });
                            }
                            //Show MenuItem if it's already closed.
                            if (!triggeredDefaultLoadKeyUpEvent && !$(searchableMenuItem).hasClass("show")) {
                                $(dsinput).click();
                            }
                            triggeredDefaultLoadKeyUpEvent = false;
                            return false;
                        });

                        $(searchableMenuItem).find("[data-searchable-item]").each(function () {
                            var currentliElement = $(this);
                            $(this).click(function (e) {
                                e.preventDefault();
                                $(dsinput).click();
                                var selecteduid = $(this).attr("data-uid");
                                appCommon.Set_SelectedItemOfDataSearchable(dsinput, selecteduid);
                                return false;
                            });
                        });

                        var selecteduid = $(dsinput).attr("data-uid");
                        appCommon.Set_SelectedItemOfDataSearchable(dsinput, selecteduid);
                    }
                });

                //Multi Select item
                $(form).find('[data-multiselect-dropdown-toggle]').each(function (MDT, multiSelectDT) {
                    var hasAllOption = $(this).find("[data-opt-all-items]").length > 0;
                    var multiSelectItems = $(multiSelectDT).find("[data-multiselect-item]");
                    var multiSelectInput = $(multiSelectDT).find('[data-multiselect-input]');
                    var controlName = $(multiSelectInput).attr("name");
                    var multiSelectSeparator = $(multiSelectInput).attr("data-multiSelectSeparator");
                    if (typeof multiSelectSeparator == "undefined" || multiSelectSeparator != null) {
                        multiSelectSeparator = ",";
                    }
                    //Create copy of hidden field to post the selected values in backend
                    var inputHiddenIdentifier = document.createElement("input");
                    inputHiddenIdentifier.setAttribute("type", "hidden");
                    inputHiddenIdentifier.setAttribute('name', controlName);
                    $(inputHiddenIdentifier).insertAfter(multiSelectInput);
                    $(multiSelectInput).attr("name", controlName + "_" + appCommon.GenerateGUID());

                    $(multiSelectDT).find("[data-multiselect-item]").click(function (e) {
                        var currentItem = $(this);
                        var tagName = $(e.target)[0].tagName;
                        var hasCheckBox = $(currentItem).find('input[type="checkbox"]').length > 0;
                        if (hasCheckBox) {
                            if (tagName == "LABEL" || tagName == "SPAN") {
                                if ($(currentItem).find('input[type="checkbox"]:checked').length > 0)
                                    $(currentItem).find('input[type="checkbox"]').prop("checked", false);
                                else
                                    $(currentItem).find('input[type="checkbox"]').prop("checked", true);
                            }
                            if ($(currentItem).attr("data-opt-all-items") == "true") {
                                if ($(currentItem).find('input[type="checkbox"]').prop("checked"))
                                    $(multiSelectItems).find('input[type="checkbox"]').prop("checked", true);
                                else
                                    $(multiSelectItems).find('input[type="checkbox"]').prop("checked", false);
                            }
                            else if (hasAllOption) {
                                var hasAllOptionOpted = $(multiSelectDT).find('[data-opt-all-items]').find('input[type="checkbox"]').prop("checked")
                                if (!hasAllOptionOpted) {
                                    if ($(multiSelectItems).find('input[type="checkbox"]').length - 1 == $(multiSelectItems).find('input[type="checkbox"]:checked').length)
                                        $(multiSelectItems).find('input[type="checkbox"]').prop("checked", true);
                                }
                                else {
                                    $(multiSelectDT).find('[data-opt-all-items]').find('input[type="checkbox"]').prop("checked", false);
                                }
                            }
                            //Show selected options
                            if (hasAllOption && $(multiSelectItems).find('input[type="checkbox"]').length == $(multiSelectItems).find('input[type="checkbox"]:checked').length) {
                                $(multiSelectInput).val("All");
                            }
                            else {
                                var name = "";
                                $(multiSelectItems).find('input[type="checkbox"]:checked').each(function (c, citem) {
                                    var currenNameString = $(citem).closest('[data-multiselect-item]').attr("name");
                                    if (typeof currenNameString != 'undefined' && currenNameString != null) {
                                        if (name != "")
                                            name = name + multiSelectSeparator + " ";
                                        name += currenNameString;
                                    }
                                });
                                $(multiSelectInput).val(name);
                            }
                        }
                        //Set selected value
                        var value = "";
                        $(multiSelectItems).find('input[type="checkbox"]:checked').each(function (c, citem) {
                            if ($(citem).attr("data-opt-all-items") != "true") {
                                var currenValueString = $(citem).closest('[data-multiselect-item]').attr("value");
                                if (typeof currenValueString != 'undefined' && currenValueString != null) {
                                    if (value != "")
                                        value = value + multiSelectSeparator;
                                    value += currenValueString;
                                }
                            }
                        });
                        $(inputHiddenIdentifier).val(value);

                        if (!hasCheckBox || tagName == "LABEL" || tagName == "SPAN")
                            return false;
                    });

                    //Show already selected values
                    if ($(multiSelectDT).find('[data-opt-all-items]').length > 0) {
                        var hasAllOptionOpted = $(multiSelectDT).find('[data-opt-all-items]').find('input[type="checkbox"]').prop("checked")
                        if (hasAllOptionOpted || $(multiSelectItems).find('input[type="checkbox"]').length - 1 == $(multiSelectItems).find('input[type="checkbox"]:checked').length)
                            $(multiSelectItems).find('input[type="checkbox"]').prop("checked", true);
                    }

                    var name = ""; var value = "";
                    $(multiSelectItems).find('input[type="checkbox"]:checked').each(function (c, citem) {
                        var currenNameString = $(citem).closest('[data-multiselect-item]').attr("name");
                        if (typeof currenNameString != 'undefined' && currenNameString != null) {
                            if (name != "")
                                name = name + multiSelectSeparator + " ";
                            name += currenNameString;
                        }
                        var currenValueString = $(citem).closest('[data-multiselect-item]').attr("value");
                        if (typeof currenValueString != 'undefined' && currenValueString != null) {
                            if (value != "")
                                value = value + multiSelectSeparator;
                            value += currenValueString;
                        }
                    });
                    $(multiSelectInput).val(name);
                    $(inputHiddenIdentifier).val(value);
                });

                $(form).find('[data-name="SlideNavPanel"]').each(function () {
                    $(this).find('[data-name="SlideNavPanelItem"]').each(function (i, NavPanelItem) {
                        if (i > 0)
                            $(this).css("display", "none");
                        else {
                            $(this).css("display", "block");
                            appCommon.SetFocusOnControl($(NavPanelItem));
                        }
                        $(this).find('[data-name="SlideNext"]').click(function () {
                            var validationStatus = true;
                            if ($(this).closest('[data-disable=""],[data-disable="true"]').length == 0 && !appCommon.IsValidForm($(NavPanelItem))) {
                                validationStatus = false;
                            }
                            if (validationStatus) {
                                $(NavPanelItem).css("display", "none");
                                $(NavPanelItem).next().css("display", "block");
                                appCommon.SetFocusOnControl($(NavPanelItem).next());

                            }
                        });
                        $(this).find('[data-name="SlidePrevious"]').click(function () {
                            $(NavPanelItem).css("display", "none");
                            $(NavPanelItem).prev().css("display", "block");
                            appCommon.SetFocusOnControl($(NavPanelItem).prev());
                        });

                        //Make Default submit button for the panel
                        $(this).find('input[type="text"],textarea,select').keypress(function (event) {
                            event = (event) ? event : window.event;
                            var charCode = (event.which) ? event.which : event.keyCode;
                            if (charCode == 13) {
                                if ($(NavPanelItem).find('[data-name="SlideNext"]').length > 0)
                                    $(NavPanelItem).find('[data-name="SlideNext"]').click();
                                else if ($(NavPanelItem).find('[data-default-button]').length > 0)
                                    $(NavPanelItem).find('[data-default-button]').click();
                            }
                        });

                    });
                });
                $('[data-disable]').each(function (d, disableItem) {
                    appCommon.DisableAllControls(disableItem);
                });
                $('[data-disable-with-events]').each(function (d, disableItem) {
                    appCommon.DisableAllControlsWithEvents(disableItem);
                });
                appCommon.EnablePopovers();
            },
            ///Parameters
            // 1.Pass the jquery reference of the searchable textbox 
            // 2.Pass the value that needs to be set as a selected item.
            ///Parameters
            Set_SelectedItemOfDataSearchable: function (data_searchableItem, selected_uid) {
                if (typeof data_searchableItem == "undefined" || data_searchableItem == null)
                    return;
                var searchableMenuItem = $(data_searchableItem).parent().find("[data-searchable-panel]");
                if (searchableMenuItem != "undefined" && searchableMenuItem != null && selected_uid != "undefined") {
                    $(searchableMenuItem).find("[data-searchable-item]").each(function () {
                        $(this).removeClass("active");
                    });
                    var inputHiddenIdentifier = $(searchableMenuItem).parent().find('[selected-data-uid]');
                    $(searchableMenuItem).find("[data-searchable-item]").each(function () {
                        if ($(this).attr("data-uid") == selected_uid) {
                            $(this).addClass("active");
                            var selectedText = $(this).text().trim();
                            var text = $(this).attr("data-text");
                            if (typeof text != "undefined")
                                selectedText = text;
                            var valueuid = $(this).attr("data-uid");
                            if (typeof valueuid != "undefined" && valueuid != "") {
                                $(inputHiddenIdentifier).val(valueuid);
                            }
                            else {
                                $(inputHiddenIdentifier).val(selectedText);
                            }
                            $(data_searchableItem).val(selectedText);
                        }
                        else
                            $(this).removeClass("active");
                    });
                }
            },
            SetFocusOnControl: function (Panel) {
                //Default focus
                try {
                    if ($(Panel).closest("[data-disable]").length == 0) {
                        var focusControls = $(Panel).find('input[type="text"],input[type="checkbox"],input[type="radio"],textarea,select');
                        if (typeof focusControls != 'undefined' && focusControls != null && focusControls.length > 0) {
                            $(focusControls).each(function (c, ctrl) {
                                if (appCommon.IsVisibleCtrl(ctrl)) {
                                    $(ctrl).focus();
                                    return false;
                                }
                            });

                        }
                    }
                }
                catch (ex) { }
            },
            ToggleJsonPanel: function (dependencyPanel, JsonInput, ApplyAnimation) {
                if (typeof ApplyAnimation == 'undefined')
                    ApplyAnimation = false;
                try {
                    var ShowPanel = false;
                    $(JsonInput).each(function (i, jsonItem) {
                        var jsonItemRef = $('[name="' + jsonItem.RefName + '"]');
                        var controlType = $('[name="' + jsonItem.RefName + '"]')[0].tagName == "INPUT" ? $('[name="' + jsonItem.RefName + '"]').attr("type").toLowerCase() : $('[name="' + jsonItem.RefName + '"]')[0].tagName.toLowerCase();
                        var RefValue = controlType == 'radio' || controlType == 'checkbox' ? $(('[name="' + $(jsonItemRef).attr('name') + '"]:checked')).val() : $(jsonItemRef).val();
                        //if ((controlType == "radio" || controlType == "checkbox") && $('[name=' + jsonItem.RefName + ']:checked').length == 0) {
                        //    ShowPanel == true;
                        //    return false;
                        //}
                        if (RefValue != null && RefValue != '' && ((jsonItem.ShowPanel && jsonItem.Values.indexOf(RefValue.trim()) >= 0) || (!jsonItem.ShowPanel && jsonItem.Values.indexOf(RefValue.trim()) == -1))) {
                            ShowPanel = true;
                        }
                        else {
                            ShowPanel = false;
                            return false;
                        }
                    });
                    if (ShowPanel) {
                        if (ApplyAnimation)
                            $(dependencyPanel).slideDown(500);
                        else
                            $(dependencyPanel).show();
                    }
                    else {
                        if (ApplyAnimation)
                            $(dependencyPanel).slideUp(500)
                        else
                            $(dependencyPanel).hide();
                    }
                } catch (ex) { }
            },
            IsVisibleCtrl: function isVisible(obj, IsModel, Parent) {
                if (!IsModel || ($(obj).attr("id") != $(Parent).attr("id"))) {
                    if (obj == document) {
                        return true;
                    }
                    if (!obj) {
                        return false;
                    }
                    if (!obj.parentNode) {
                        return false;
                    }
                    if (obj.style) {
                        if (obj.style.display == 'none') {
                            return false
                        }
                        if (obj.style.visibility == 'hidden') {
                            return false
                        }
                    }

                    //Try the computed style in a standard way
                    if (window.getComputedStyle) {
                        var style = window.getComputedStyle(obj, "")
                        if (style.display == 'none') {
                            return false
                        }
                        if (style.visibility == 'hidden') {
                            return false
                        }
                    }

                    //Or get the computed style using IE's silly proprietary way
                    var style = obj.currentStyle
                    if (style) {
                        if (style['display'] == 'none') {
                            return false;
                        }
                        if (style['visibility'] == 'hidden') {
                            return false;
                        }
                    }
                    return isVisible(obj.parentNode, IsModel, Parent)

                }
                return true;
            },
            IsDisabledCtrl: function isVisible(obj) {
                try {
                    if (obj != null && obj != 'undefined') {
                        if ($(obj).prop('disabled'))
                            return true;
                    }
                } catch (ex) { }
                return false;
            },
            CreateErrorControl: function (form, item) {
                var controlName = $(item).attr("name");
                if (typeof controlName == "undefined") {
                    controlName = $(item).attr("id");
                    $(item).attr("name", controlName);
                }
                if (typeof controlName == "undefined") {
                    controlName = appCommon.GenerateGUID();
                    $(item).attr("name", controlName);

                }
                if ($(form).find("[data-valmsg-for='" + controlName + "']").length == 0) {
                    var paragraph = document.createElement("p");
                    paragraph.className = "field-validation-error";
                    paragraph.setAttribute('data-valmsg-for', controlName);
                    if ($(item).parent().hasClass("input-group")) {
                        $(item).parent().after(paragraph);
                    }
                    else if ($(item).attr("type") == "radio") {
                        $(item).parent().parent().after(paragraph);
                    }
                    else {
                        $(item).after(paragraph);
                    }
                }
                else {

                }

            },
            OffRequiredEvent: function (item) {
                var controlName = $(item).attr("name");
                if (typeof controlName != "undefined" && controlName != null) {
                    $(item).removeAttr("data-val");
                    $(item).removeClass("input-validation-error");
                    if ($("[data-valmsg-for='" + controlName + "']").length > 0) {
                        $("[data-valmsg-for='" + controlName + "']").html("");
                        $("[data-valmsg-for='" + controlName + "']").removeClass("field-validation-error");
                    }
                }
            },
            ToJavaScriptDate: function (value) {
                var pattern = /Date\(([^)]+)\)/;
                var results = pattern.exec(value);
                var dt = new Date(parseFloat(results[1]));
                return (dt.getMonth() + 1) + "/" + dt.getDate() + "/" + dt.getFullYear();
            },
            GenerateGUID: function () {
                return appCommon.GenerateRandomNumber() + appCommon.GenerateRandomNumber() + '_' + appCommon.GenerateRandomNumber() + '_' + appCommon.GenerateRandomNumber() + '_' +
                    appCommon.GenerateRandomNumber() + '_' + appCommon.GenerateRandomNumber() + appCommon.GenerateRandomNumber() + appCommon.GenerateRandomNumber();
            },
            GenerateRandomNumber: function () {
                return Math.floor((1 + Math.random()) * 0x10000)
                    .toString(16)
                    .substring(1);
            },
            AddRippleEffect: function () {
                $('.btn').addClass('ripple');
                //$('a').addClass('ripple');

                var onClass = "on";
                var showClass = "show";

                $("input, select")
                    .bind("checkval", function () {
                        var label = $(this).prev("label");

                        if (this.value !== "")
                            label.addClass(showClass);

                        else
                            label.removeClass(showClass);
                    })
                    .on("keyup", function () {
                        $(this).trigger("checkval");
                    })
                    .on("focus", function () {
                        $(this).prev("label").addClass(onClass);
                    })
                    .on("blur", function () {
                        $(this).prev("label").removeClass(onClass);
                    })
                    .trigger("checkval");

                $("select")
                    .on("change", function () {
                        var $this = $(this);

                        if ($this.val() == "")
                            $this.addClass("watermark");

                        else
                            $this.removeClass("watermark");

                        $this.trigger("checkval");
                    })
                    .change();
            },
            messages: {
                required: "This field is required.",
                remote: "Please fix this field.",
                email: "Please enter a valid email address.",
                phone: "Please enter a valid phone number.",
                url: "Please enter a valid URL.",
                date: "Please enter a valid date.",
                dateISO: "Please enter a valid date (ISO).",
                number: "Please enter a valid number.",
                decimal: "Please enter a valid value.",
                currency: "Please enter a valid amount.",
                alphabets: "Please enter a valid input.",
                alphanumeric: "Please enter alphanumeric characters only.",
                zipcode: "Please enter a valid zipcode.",
                zipcodeextension: "Please enter a valid zipcode extension.",
                digits: "Please enter only digits.",
                cc: "Please enter a valid credit card number.",
                cvv: "Please enter a valid CVV.",
                equalTo: "Please enter the same value again.",
                maxlength: "Please enter no more than {0} characters.",
                minlength: "Please enter at least {0} characters.",
                rangelength: "Please enter a value between {0} and {1} characters long.",
                range: "Please enter a value between {0} and {1}.",
                max: "Please enter a value less than or equal to {0}.",
                min: "Please enter a value greater than or equal to {0}.",
                restrictedspecialchars: "Special characters are not allowed.",
                pannumber: "Please enter a valid pan number.",
                ssn: "Please enter a valid SSN number.",
                fein: "Please enter a valid FEIN number.",
                gstin: "Please enter a valid GSTIN number.",
                repeatedcharacters: "Please don't repeat the characters more than {0} times in a sequence.",
                maskdob: "Date of Birth must be MM/DD/YYYY format."
            },
            IsValidForm: function (form, IsModel) {
                console.log("Validating Form");

                var IsValid = true;
                if (IsModel == 'undefined') IsModel = false;
                var errorControlOffseTop = 0;
                try {
                    //$(form).find("[data-val='true']")
                    $(form).find("[data-val='true'],[data-val-type='pannumber'],[data-val-type='phone'],[data-val-type='gstin'],[data-val-type='alphabets'],[data-val-type='alphanumeric'],[data-val-type='email'],[data-val-type='maskdob'],[data-val-type='url'],[data-val-type='number'],[data-val-type='decimal'],[data-val-type='currency'],[data-val-type='fein'],[data-val-type='ssn'],[data-val-type='zipcode'],[data-val-type='zipcodeextension']").each(function (i, item) {

                        var controlType = $(this)[0].tagName == "INPUT" ? $(this).attr("type").toLowerCase() : $(this)[0].tagName.toLowerCase();
                        var controlName = $(item).attr("name");
                        var currentControlIsValid = true;
                        var Message = "";
                        var isVisibleMode = appCommon.IsVisibleCtrl(this, IsModel, $(form));
                        var isDisabledMode = appCommon.IsDisabledCtrl(this);
                        if (isVisibleMode && !isDisabledMode) {
                            var errorControl = $("[data-valmsg-for='" + controlName + "']");
                            if (errorControl.length == 0) {
                                appCommon.CreateErrorControl(form, this);
                                var errorControl = $("[data-valmsg-for='" + controlName + "']");
                            }
                            $(errorControl).html("");
                            if (controlType == "text" || controlType == "select" || controlType == "password" || controlType == "file" || controlType == "textarea" || controlType == "email" || controlType == "number") {
                                if (($(this).attr("data-val") == "true" && ($(this).val() == null || $(this).val().trim() == "")) || ($(this).val().indexOf('?') >= 0 && controlType == "select")) {
                                    currentControlIsValid = false;
                                    if (typeof $(this).attr("data-val-required") != "undefined") {
                                        Message = $(this).attr("data-val-required");
                                    }
                                    else {
                                        Message = appCommon.messages.required;
                                    }

                                }
                                else if ($(this).val().trim() != "") {
                                    if ($(this).attr("data-val-type") == "email" && !regExpEmail.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-email") != "undefined") {
                                            Message = $(this).attr("data-val-email");
                                        }
                                        else {
                                            Message = appCommon.messages.email;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "phone") {
                                        var value = $(this).val().trim();
                                        if ($(this).attr("data-apply-format") == "true") {
                                            value = appCommon.RemovePhoneFormat(value);
                                        }
                                        var regExpMobileByCtrl = appCommon.GetMobileRegExpByControl($(this));
                                        if (!regExpMobileByCtrl.test(value)) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-phone") != "undefined") {
                                                Message = $(this).attr("data-val-phone");
                                            }
                                            else {
                                                Message = appCommon.messages.phone;
                                            }
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "ssn") {
                                        var value = $(this).val().trim();
                                        if (!regExpSSN.test(value) || (typeof InvalidFEINOrSSNList != "undefined" && InvalidFEINOrSSNList != null && InvalidFEINOrSSNList.indexOf(appCommon.RemoveSSNFormat(value)) > -1)) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-ssn") != "undefined") {
                                                Message = $(this).attr("data-val-ssn");
                                            }
                                            else {
                                                Message = appCommon.messages.ssn;
                                            }
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "fein") {
                                        var value = $(this).val().trim();
                                        if (!regExpFEIN.test(value) || (typeof InvalidFEINOrSSNList != "undefined" && InvalidFEINOrSSNList != null && InvalidFEINOrSSNList.indexOf(appCommon.RemoveFEINFormat(value)) > -1)) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-fein") != "undefined") {
                                                Message = $(this).attr("data-val-fein");
                                            }
                                            else {
                                                Message = appCommon.messages.fein;
                                            }
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "date" && !regExpDate.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-date") != "undefined") {
                                            Message = $(this).attr("data-val-date");
                                        }
                                        else {
                                            Message = appCommon.messages.date;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "alphabets" && !regOnlyAlphabets.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-alphabets") != "undefined") {
                                            Message = $(this).attr("data-val-alphabets");
                                        }
                                        else {
                                            Message = appCommon.messages.alphabets;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "alphanumeric" && !regOnlyAlphaNumeric.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-alphanumeric") != "undefined") {
                                            Message = $(this).attr("data-val-alphanumeric");
                                        }
                                        else {
                                            Message = appCommon.messages.alphanumeric;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type-repeat") == "true" && new RegExp("(.)\\1" + ("\\1".repeat((typeof $(this).attr("data-val-repeat-maxcount") != "undefined" && $(this).attr("data-val-repeat-maxcount") != null && $(this).attr("data-val-repeat-maxcount") != '' && $(this).attr("data-val-repeat-maxcount") > 2) ? ($(this).attr("data-val-repeat-maxcount") - 2) : 0)), 'g').test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-repeatedcharacters") != "undefined") {
                                            Message = $(this).attr("data-val-repeatedcharacters");
                                        }
                                        else {
                                            Message = appCommon.messages.repeatedcharacters.format((typeof $(this).attr("data-val-repeat-maxcount") != "undefined" && $(this).attr("data-val-repeat-maxcount") != null && $(this).attr("data-val-repeat-maxcount") != '' && $(this).attr("data-val-repeat-maxcount") > 0) ? $(this).attr("data-val-repeat-maxcount") : 2);
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "pannumber" && !regPannumber.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-pannumber") != "undefined") {
                                            Message = $(this).attr("data-val-pannumber");
                                        }
                                        else {
                                            Message = appCommon.messages.pannumber;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "cc" && !regExpCC.test((appCommon.ReplaceAllOccurence($(this).val().trim(), " ", "")))) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-cc") != "undefined") {
                                            Message = $(this).attr("data-val-cc");
                                        }
                                        else {
                                            Message = appCommon.messages.cc;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "cvv" && !regExpCVV.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-cvv") != "undefined") {
                                            Message = $(this).attr("data-val-cvv");
                                        }
                                        else {
                                            Message = appCommon.messages.cvv;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "gstin" && !regGSTIN.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-gstin") != "undefined") {
                                            Message = $(this).attr("data-val-gstin");
                                        }
                                        else {
                                            Message = appCommon.messages.gstin;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "number" || $(this).attr("data-val-type") == "currency") {
                                        if ($(this).attr("data-val-type") == "currency") {
                                            if (!appCommon.IsValidCurrency(this)) {
                                                currentControlIsValid = false;
                                                if (typeof $(this).attr("data-val-currency") != "undefined") {
                                                    Message = $(this).attr("data-val-currency");
                                                }
                                                else {
                                                    Message = appCommon.messages.currency;
                                                }
                                            }
                                        }
                                        else if (!regNumber.test($(this).val().trim())) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-number") != "undefined") {
                                                Message = $(this).attr("data-val-number");
                                            }
                                            else {
                                                Message = appCommon.messages.number;
                                            }
                                        }

                                        if ($(this).val().trim() != '' && typeof $(this).attr("data-val-min-number") != "undefined" && typeof $(this).attr("data-val-max-number") != "undefined") {
                                            try {
                                                var min = parseInt($(this).attr("data-val-min-number"));
                                                var max = parseInt($(this).attr("data-val-max-number"));
                                                var value = $(this).val();
                                                if ($(this).attr("data-val-type") == "currency") {
                                                    value = parseFloat(appCommon.RemoveCurrencyFormat(value));
                                                }
                                                if (value < min || value > max) {
                                                    currentControlIsValid = false;
                                                    Message = appCommon.messages.range.format(min, max);
                                                }
                                            }
                                            catch (exception) {

                                            }
                                        }
                                        else if (typeof $(this).attr("data-val-min-number") != "undefined") {
                                            try {
                                                var min = parseInt($(this).attr("data-val-min-number"));
                                                var value = $(this).val();
                                                if ($(this).attr("data-val-type") == "currency") {
                                                    value = parseFloat(appCommon.RemoveCurrencyFormat(value));
                                                }
                                                if (value < min) {
                                                    currentControlIsValid = false;
                                                    if (typeof $(this).attr("data-val-min-message") != "undefined") {
                                                        Message = $(this).attr("data-val-min-message");
                                                    }
                                                    else {
                                                        // Message = appCommon.messages.number;
                                                        Message = appCommon.messages.min.format(min)
                                                    }
                                                }
                                                else {
                                                    if (typeof $(this).attr("data-val-min-year") != "undefined") {
                                                        var minyear = parseInt($(this).attr("data-val-min-year"));
                                                        if ($(this).val() < minyear) {
                                                            currentControlIsValid = false;
                                                            Message = "Minimum value is " + minyear;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (exception) {

                                            }
                                        }
                                        if (typeof $(this).attr("data-val-max-number") != "undefined" && currentControlIsValid == true) {
                                            try {
                                                var max = parseInt($(this).attr("data-val-max-number"));
                                                var value = $(this).val();
                                                if ($(this).attr("data-val-type") == "currency") {
                                                    value = parseFloat(appCommon.RemoveCurrencyFormat(value));
                                                }
                                                if (value > max) {
                                                    currentControlIsValid = false;
                                                    if (typeof $(this).attr("data-val-max-message") != "undefined") {
                                                        Message = $(this).attr("data-val-max-message");
                                                    }
                                                    else {
                                                        Message = appCommon.messages.max.format(max);
                                                    }
                                                }
                                            }
                                            catch (exception) {

                                            }
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "decimal") {
                                        if (!regDecimal.test($(this).val().trim())) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-decimal") != "undefined") {
                                                Message = $(this).attr("data-val-decimal");
                                            }
                                            else {
                                                Message = appCommon.messages.decimal;
                                            }
                                        }
                                    }

                                    else if ($(this).attr("data-val-type") == "zipcode") {
                                        var regZipcode = regExpZipcode_US;
                                        if ($(this).attr("data-val-zipcode-format") == _LookUpZipcodeFormat.IN) {
                                            regZipcode = regExpZincode_In;

                                        }
                                        if (!regZipcode.test($(this).val().trim())) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-zipcode") != "undefined") {
                                                Message = $(this).attr("data-val-zipcode");
                                            }
                                            else {
                                                Message = appCommon.messages.zipcode;
                                            }
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "zipcodeextension") {
                                        if (!regExpZipcodeExtension.test($(this).val().trim())) {
                                            currentControlIsValid = false;
                                            if (typeof $(this).attr("data-val-zipcodeextension") != "undefined") {
                                                Message = $(this).attr("data-val-zipcodeextension");
                                            }
                                            else {
                                                Message = appCommon.messages.zipcodeextension;
                                            }
                                        }
                                    }


                                    else if ($(this).attr("data-val-type") == "url" && !regExpUrl.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-url") != "undefined") {
                                            Message = $(this).attr("data-val-url");
                                        }
                                        else {
                                            Message = appCommon.messages.url;
                                        }
                                    }
                                    else if ($(this).attr("data-val-type") == "maskdob" && !regExpDate.test($(this).val().trim())) {
                                        currentControlIsValid = false;
                                        if (typeof $(this).attr("data-val-maskdob") != "undefined") {
                                            Message = $(this).attr("data-val-maskdob");
                                        }
                                        else {
                                            Message = appCommon.messages.maskdob;
                                        }
                                    }
                                    else if (typeof $(this).attr("data-val-minlength") != "undefined") {
                                        try {
                                            var min = parseInt($(this).attr("data-val-minlength"));
                                            if ($(this).val().length < min) {
                                                currentControlIsValid = false;
                                                if (typeof $(this).attr("data-val-minlength-message") != "undefined") {
                                                    Message = $(this).attr("data-val-minlength-message");
                                                }
                                                else {
                                                    Message = appCommon.messages.minlength.format(min);
                                                }
                                            }
                                        }
                                        catch (exception) {

                                        }
                                    }
                                }
                            }
                            else if (controlType == "radio") {
                                if ($("input[name='" + $(this).attr("name") + "']:checked").length == 0) {
                                    currentControlIsValid = false;
                                    if (typeof $(this).attr("data-val-required") != "undefined") {
                                        Message = $(this).attr("data-val-required");
                                    }
                                    else {
                                        Message = appCommon.messages.required;
                                    }
                                }
                            }
                        }
                        //try {
                        //    if (currentControlIsValid) {
                        //        if ($(this).val().trim() != "" && ($(this).val().trim().indexOf(';') != -1 || $(this).val().trim().indexOf("'") != -1)) {
                        //            currentControlIsValid = false;
                        //            Message = appCommon.messages.restrictedspecialchars;
                        //        }
                        //    }
                        //}
                        //catch (exception) { }

                        if (currentControlIsValid) {
                            $(this).removeClass("input-validation-error");
                        }
                        else {
                            $(errorControl).html(Message);
                            $(this).addClass("input-validation-error");
                            if (errorControlOffseTop == 0 || errorControlOffseTop > $(this).offset().top) {
                                errorControlOffseTop = $(this).offset().top;
                            }
                            IsValid = false;
                            if (typeof $(this).attr("data-val-touched") == "undefined") {
                                $(this).attr("data-val-touched", true);
                            }
                        }
                    });

                    try {
                        $(form).find('[data-number-panel-sum]').each(function (n, numberpanel) {
                            try {
                                if (appCommon.IsValidForm($(numberpanel)) && appCommon.IsVisibleCtrl(numberpanel, IsModel, $(form)) && !appCommon.ValidateNumberPanel($(numberpanel))) {
                                    IsValid = false;
                                    if (errorControlOffseTop == 0 || errorControlOffseTop > $(numberpanel).offset().top) {
                                        errorControlOffseTop = $(numberpanel).offset().top;
                                    }
                                }

                            }
                            catch (datanumberpanelsumex) { }
                        });


                    }
                    catch (ex) { }

                    if (!IsValid) {
                        try {
                            if ($(form).css("z-index") > 0) {
                                //                                $(form).animate({
                                //                                    scrollTop: errorControlOffseTop
                                //                                }, 500);
                            }
                            else {
                                if (!IsModel) {
                                    $('html,body, .cy-container').animate({
                                        scrollTop: errorControlOffseTop - 200
                                    }, 500);
                                }

                            }
                        }
                        catch (error) {
                        }
                    }
                }
                catch (exception) {
                    console.log(exception);
                }
                return IsValid;
            },
            GetMobileRegExpByControl: function (ctrl) {
                var regExpMobile = regExpMobilePattern;
                var datavalformat = $(ctrl).attr("data-val-format");
                var datavalformatcharacter = $(ctrl).attr("data-val-format-character");
                if (datavalformatcharacter == null || datavalformatcharacter == 'undefined' || datavalformatcharacter == "")
                    datavalformatcharacter = "#";
                if (datavalformat != null && datavalformat != "") {
                    var charactersCount = appCommon.GetCharacterRepeatCount(datavalformat, datavalformatcharacter);
                    regExpMobile = new RegExp("^[0-9]{" + charactersCount + "}$")
                }
                return regExpMobile;
            },
            GetCharacterRepeatCount(str, character) {
                var repeatCount = 0;
                if (str != null && str != "" && str != 'undefined') {
                    for (var index = 0; index < str.length; index++) {
                        if (str[index] === character) {
                            repeatCount++;
                        }
                    }
                }
                return repeatCount;
            },
            DefaultEventTypes: 'focusout keyup',
            JSDefaultNameSpace: "Default",
            WithRequiredEventTypes: 'focusin.NameSpace focusout.NameSpace keyup.NameSpace',
            BindValidationEvent: function (ctrl, events) {
                var errorControl = $("[data-valmsg-for='" + $(ctrl).attr("name") + "']");
                events = events + "." + $(ctrl).attr("name");
                if (typeof errorControl != "undefined") {
                    var data_val_type = $(ctrl).attr("data-val-type");
                    if (data_val_type == "number" || data_val_type == "decimal" || data_val_type == "currency" || data_val_type == "phone" || data_val_type == "ssn" || data_val_type == "fein" || data_val_type == "cc" || data_val_type == "zipcode" || data_val_type == "zipcodeextension" || data_val_type == "alphabets" || data_val_type == "alphanumeric") {
                        if (data_val_type == "alphabets" || data_val_type == "alphanumeric") {
                            $(ctrl).on("keypress", function (event) {
                                event = (event) ? event : window.event;
                                var charCode = (event.which) ? event.which : event.keyCode;
                                //  if (charCode == 13) return true;
                                if (($(ctrl).attr("data-val-type") == "alphanumeric" && charCode >= 48 && charCode <= 57) || (charCode >= 97 && charCode <= 122) || (charCode >= 65 && charCode <= 90) || charCode == 8 || (charCode == 32 && $(ctrl).attr("data-val-not-allow-space") == true)) {
                                    $(this).removeClass("input-validation-error");
                                    $(errorControl).html("");
                                }
                                else {
                                    $(errorControl).html($(ctrl).attr("data-val-type") == "alphanumeric" ? appCommon.messages.alphanumeric : appCommon.messages.alphabets);
                                    $(this).addClass("input-validation-error");
                                    return false;
                                }
                            });
                        }
                        else if ($(ctrl).attr("data-val-type") == "decimal") {
                            $(ctrl).on("keypress", function (event) {
                                event = (event) ? event : window.event;
                                var charCode = (event.which) ? event.which : event.keyCode;
                                if (charCode > 31 && (charCode != 46 || $(this).val().indexOf('.') != -1) && (charCode < 48 || charCode > 57)) {
                                    if (typeof $(this).attr("data-val-decimal") != "undefined") {
                                        Message = $(this).attr("data-val-decimal");
                                    }
                                    else {
                                        Message = appCommon.messages.decimal;
                                    }
                                    $(errorControl).html(Message);
                                    $(this).addClass("input-validation-error");
                                    return false;
                                }
                                else {
                                    if (!appCommon.CheckDecimalFormatRuleOnEnter(ctrl))
                                        return false;
                                    $(this).removeClass("input-validation-error");
                                    $(errorControl).html("");
                                    appCommon.CheckNumberPanel($(this));
                                }
                            });
                        }
                        //else if ($(ctrl).attr("data-val-type") == "currency") {
                        //    $(ctrl).on("keyup", function (event) {
                        //        if ($(this).attr("data-val-type") == "currency") {
                        //            if (!appCommon.IsValidCurrency(this)) {
                        //                if (typeof $(this).attr("data-val-currency") != "undefined") {
                        //                    Message = $(this).attr("data-val-currency");
                        //                }
                        //                else {
                        //                    Message = appCommon.messages.currency;
                        //                }
                        //                $(errorControl).html(Message);
                        //                $(this).addClass("input-validation-error");
                        //                return false;
                        //            }
                        //            else {
                        //                $(this).removeClass("input-validation-error");
                        //                $(errorControl).html("");
                        //                appCommon.CheckNumberPanel($(this));
                        //                appCommon.FormatToCurrencyFormat($(this));
                        //            }
                        //        }
                        //    });
                        //    try {
                        //        if ($(ctrl).val() != "")
                        //            appCommon.FormatToCurrencyFormat($(ctrl));
                        //    }
                        //    catch (ex) { }
                        //}
                        else if ($(ctrl).attr("data-val-type") == "currency") {
                            $(ctrl).on("keyup", function (event) {
                                if ($(this).attr("data-val-type") == "currency") {
                                    if (($(ctrl).attr('data-val-number') != null && $(ctrl).attr('data-val-number') != undefined)) {
                                        if (event.which === 9) {
                                            return;
                                        }
                                        var inputValue = $(this).val();
                                        if (!regNumberWithDollar.test(inputValue)) {
                                            $(errorControl).html(appCommon.messages.onlynumbers);
                                            $(this).addClass("input-validation-error");
                                            return false;
                                        }
                                        else {
                                            $(this).removeClass("input-validation-error");
                                            $(errorControl).html("");
                                            if (!($(ctrl).attr('data-blur-currency') != null && $(ctrl).attr('data-blur-currency') != undefined)) {
                                                appCommon.FormatToCurrencyFormat($(this));
                                            }
                                            if ($(this).attr("data-val-ignore") == "currency") {
                                                appCommon.FormatToCurrencyFormat($(this), true);
                                            }
                                        }
                                    }
                                    else {
                                        if (!appCommon.IsValidCurrency(this)) {
                                            if (typeof $(this).attr("data-val-currency") != "undefined") {
                                                Message = $(this).attr("data-val-currency");
                                            }
                                            else {
                                                Message = appCommon.messages.currency;
                                            }
                                            $(errorControl).html(Message);
                                            $(this).addClass("input-validation-error");
                                            return false;
                                        }
                                        else {
                                            $(this).removeClass("input-validation-error");
                                            $(errorControl).html("");
                                            appCommon.CheckNumberPanel($(this));
                                            if (!($(ctrl).attr('data-blur-currency') != null && $(ctrl).attr('data-blur-currency') != undefined)) {
                                                appCommon.FormatToCurrencyFormat($(this));
                                            }
                                            if ($(this).attr("data-val-ignore") == "currency") {
                                                appCommon.FormatToCurrencyFormat($(this), true);
                                            }
                                        }
                                    }
                                }
                            });
                            if ($(ctrl).attr('data-blur-currency') != null && $(ctrl).attr('data-blur-currency') != undefined) {
                                $(ctrl).on("blur", function (event) {
                                    if (!appCommon.IsValidCurrency(this)) {
                                        if (typeof $(this).attr("data-val-currency") != "undefined") {
                                            Message = $(this).attr("data-val-currency");
                                        }
                                        else {
                                            Message = appCommon.messages.currency;
                                        }
                                        $(errorControl).html(Message);
                                        $(this).addClass("input-validation-error");
                                        return false;
                                    }
                                    else {
                                        $(this).removeClass("input-validation-error");
                                        $(errorControl).html("");
                                        appCommon.CheckNumberPanel($(this));
                                        appCommon.FormatToCurrencyFormat($(this));
                                    }
                                });
                            }
                            try {
                                if ($(ctrl).val() != "" && !($(ctrl).attr('data-blur-currency') != null && $(ctrl).attr('data-blur-currency') != undefined)) {
                                    if ($(ctrl).attr("data-val-ignore") == "currency") {
                                        appCommon.FormatToCurrencyFormat($(ctrl), true);
                                    }
                                    else {
                                        appCommon.FormatToCurrencyFormat($(ctrl));
                                    }
                                }
                            }
                            catch (ex) { }
                        }
                        else {

                            //Apply default format if the value of the filed is empty
                            if ($(ctrl).attr("data-apply-format") == "true" && $(ctrl).attr("data-default-val-format") == "true") {
                                appCommon.ApplyDefaultPhoneFormat(ctrl);
                            }

                            $(ctrl).on("keypress", function (event) {
                                event = (event) ? event : window.event;
                                var charCode = (event.which) ? event.which : event.keyCode;
                                if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                                    $(errorControl).html("Only numbers are allowed.");
                                    $(this).addClass("input-validation-error");
                                    //if (typeof $(this).attr("data-val-touched") == "undefined") {
                                    //    $(this).attr("data-val-touched", true);
                                    //}
                                    return false;
                                }
                                else {
                                    $(this).removeClass("input-validation-error");
                                    $(errorControl).html("");
                                    appCommon.CheckNumberPanel($(this));
                                    //if ($(this).attr("data-apply-format") == "true") {
                                    //    var value = $(this).val().trim();
                                    //    if (data_val_type == "phone") {
                                    //        value = appCommon.RemovePhoneFormat(value);
                                    //        $(this).val(appCommon.ApplyPhoneFormat(value));
                                    //    }
                                    //    else if (data_val_type == "ssn") {
                                    //        value = appCommon.RemoveSSNFormat(value);
                                    //        $(this).val(appCommon.ApplySSNFormat(value));
                                    //    }
                                    //    else if (data_val_type == "fein") {
                                    //        value = appCommon.RemoveFEINFormat(value);
                                    //        $(this).val(appCommon.ApplyFEINFormat(value));
                                    //    }
                                    //}
                                }
                            });
                            if ($(ctrl).attr("data-apply-format") == "true" || data_val_type == "ssn" || data_val_type == "fein") {
                                $(ctrl).on("keyup", function (event) {
                                    var value = $(this).val().trim();
                                    var valueformat = $(this).attr("data-val-format");
                                    if (data_val_type == "phone") {
                                        value = appCommon.RemovePhoneFormat(value);
                                        $(this).val(appCommon.ApplyPhoneFormat(value, valueformat));
                                    }
                                    else if (data_val_type == "ssn") {
                                        value = appCommon.RemoveSSNFormat(value);
                                        $(this).val(appCommon.ApplySSNFormat(value));
                                    }
                                    else if (data_val_type == "fein") {
                                        value = appCommon.RemoveFEINFormat(value);
                                        $(this).val(appCommon.ApplyFEINFormat(value));
                                    }
                                    else if (data_val_type == "cc") {
                                        value = appCommon.RemoveFormat(value);
                                        $(this).val(appCommon.ApplyCCFormat(value));
                                    }
                                });
                            }
                        }
                        //if ($(this).attr("data-val-type") == "number" || $(this).attr("data-val-type") == "decimal" || $(this).attr("data-val-type") == "currency") {
                        //    try {
                        //        if (typeof $(this).closest('[data-number-panel-sum]') != 'undefined' && $(this).closest('[data-number-panel-sum]').length > 0) {
                        //            appCommon.CheckNumberPanel($(this));
                        //        }
                        //    }
                        //    catch (datanumberpanelsumex) { }
                        //}
                    }
                    $(ctrl).on(appCommon.DefaultEventTypes, function (event) {
                        if (typeof $(this).attr("data-val-touched") != "undefined" || event.type == "focusout") {
                            var IsValid = true;
                            if ($(this).attr("data-val") == "true" && ($(this).val() == null || $(this).val().trim() == "" && typeof event.namespace != appCommon.JSDefaultNameSpace)) {
                                if (typeof $(this).attr("data-val-required") != "undefined") {
                                    $(errorControl).html($(this).attr("data-val-required"));
                                }
                                else {
                                    $(errorControl).html(appCommon.messages.required);
                                }

                                IsValid = false;
                            }
                            else if ($(this).val().trim() != "") {
                                if ($(this).attr("data-val-type") == "email" && !regExpEmail.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-email") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-email"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.email);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "phone") {
                                    var value = $(this).val().trim();
                                    var valueformat = $(this).attr("data-val-format");
                                    if ($(this).attr("data-apply-format") == "true") {
                                        value = appCommon.RemovePhoneFormat(value);
                                        $(this).val(appCommon.ApplyPhoneFormat(value, valueformat));
                                    }
                                    var regExpMobileByCtrl = appCommon.GetMobileRegExpByControl($(this));
                                    if ($(this).attr("data-val-touched") != "undefined" && !regExpMobileByCtrl.test(value)) {
                                        if (typeof $(this).attr("data-val-phone") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-phone"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.phone);
                                        }
                                        IsValid = false;
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "ssn") {
                                    var value = $(this).val().trim();
                                    if ($(this).attr("data-val-touched") != "undefined" && (!regExpSSN.test(value) || (typeof InvalidFEINOrSSNList != "undefined" && InvalidFEINOrSSNList != null && InvalidFEINOrSSNList.indexOf(appCommon.RemoveSSNFormat(value)) > -1))) {
                                        if (typeof $(this).attr("data-val-ssn") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-ssn"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.ssn);
                                        }
                                        IsValid = false;
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "fein") {
                                    var value = $(this).val().trim();
                                    if ($(this).attr("data-val-touched") != "undefined" && (!regExpFEIN.test(value) || (typeof InvalidFEINOrSSNList != "undefined" && InvalidFEINOrSSNList != null && InvalidFEINOrSSNList.indexOf(appCommon.RemoveFEINFormat(value)) > -1))) {
                                        if (typeof $(this).attr("data-val-fein") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-fein"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.fein);
                                        }
                                        IsValid = false;
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "date" && !regExpDate.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-date") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-date"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.date);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "number" && !regNumber.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-number") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-number"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.number);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "number") {
                                    if (!regNumber.test($(this).val().trim())) {
                                        if (typeof $(this).attr("data-val-number") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-number"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.number);
                                        }
                                        IsValid = false;
                                    }

                                    else if (typeof $(this).attr("data-val-min-number") != "undefined") {
                                        try {
                                            var min = parseInt($(this).attr("data-val-min-number"));
                                            if ($(this).val() < min) {
                                                if (typeof $(this).attr("data-val-min-message") != "undefined") {
                                                    $(errorControl).html($(this).attr("data-val-min-message"));
                                                }
                                                else {
                                                    $(errorControl).html(appCommon.messages.min.format(min));
                                                }
                                                IsValid = false;
                                            }
                                            else {
                                                if (typeof $(this).attr("data-val-min-year") != "undefined") {
                                                    var minyear = parseInt($(this).attr("data-val-min-year"));
                                                    if ($(this).val() < minyear) {
                                                        IsValid = false;
                                                        $(errorControl).html("Minimum value is " + minyear);
                                                    }
                                                }
                                            }

                                        }
                                        catch (exception) {

                                        }
                                    }
                                    if (typeof $(this).attr("data-val-max-number") != "undefined" && IsValid == true) {
                                        try {
                                            var max = parseInt($(this).attr("data-val-max-number"));
                                            if ($(this).val() > max) {
                                                IsValid = false;
                                                if (typeof $(this).attr("data-val-max-message") != "undefined") {
                                                    $(errorControl).html($(this).attr("data-val-max-message"));
                                                }
                                                else {
                                                    $(errorControl).html(appCommon.messages.max.format(max));
                                                }
                                            }
                                        }
                                        catch (exception) {

                                        }
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "pannumber" && !regPannumber.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-pannumber") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-pannumber"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.pannumber);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "cc" && !regExpCC.test(appCommon.ReplaceAllOccurence($(this).val().trim(), " ", ""))) {
                                    if (typeof $(this).attr("data-val-cc") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-cc"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.cc);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "cvv" && !regExpCVV.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-cvv") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-cvv"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.cvv);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "alphabets" && !regOnlyAlphabets.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-alphabets") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-alphabets"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.alphabets);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "alphanumeric" && !regOnlyAlphaNumeric.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-alphanumeric") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-alphanumeric"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.alphanumeric);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type-repeat") == "true" && new RegExp("(.)\\1" + ("\\1".repeat((typeof $(this).attr("data-val-repeat-maxcount") != "undefined" && $(this).attr("data-val-repeat-maxcount") != null && $(this).attr("data-val-repeat-maxcount") != '' && $(this).attr("data-val-repeat-maxcount") > 2) ? ($(this).attr("data-val-repeat-maxcount") - 2) : 0)), 'g').test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-repeatedcharacters") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-repeatedcharacters"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.repeatedcharacters.format((typeof $(this).attr("data-val-repeat-maxcount") != "undefined" && $(this).attr("data-val-repeat-maxcount") != null && $(this).attr("data-val-repeat-maxcount") != '' && $(this).attr("data-val-repeat-maxcount") > 0) ? $(this).attr("data-val-repeat-maxcount") : 2));
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "zipcode") {
                                    var regZipcode = regExpZipcode_US;
                                    if ($(this).attr("data-val-zipcode-format") == _LookUpZipcodeFormat.IN) {
                                        regZipcode = regExpZincode_In;
                                    }
                                    if (!regZipcode.test($(this).val().trim())) {
                                        if (typeof $(this).attr("data-val-zipcode") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-zipcode"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.zipcode);
                                        }
                                        IsValid = false;
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "zipcodeextension") {
                                    if (!regExpZipcodeExtension.test($(this).val().trim())) {
                                        if (typeof $(this).attr("data-val-zipcodeextension") != "undefined") {
                                            $(errorControl).html($(this).attr("data-val-zipcodeextension"));
                                        }
                                        else {
                                            $(errorControl).html(appCommon.messages.zipcodeextension);
                                        }
                                        IsValid = false;
                                    }
                                }
                                else if ($(this).attr("data-val-type") == "url" && !regExpUrl.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-url") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-url"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.url);
                                    }
                                    IsValid = false;
                                }
                                else if ($(this).attr("data-val-type") == "maskdob" && !regExpDate.test($(this).val().trim())) {
                                    if (typeof $(this).attr("data-val-maskdob") != "undefined") {
                                        $(errorControl).html($(this).attr("data-val-maskdob"));
                                    }
                                    else {
                                        $(errorControl).html(appCommon.messages.maskdob);
                                    }
                                    IsValid = false;
                                }
                                else if (event.type == "focusout" && $(this).attr("data-val-type") == "decimal" && $(this).val() != "" && !isNaN($(this).val())) {
                                    try {
                                        if ($(this).val().trim().indexOf(".") == 0) {
                                            $(this).val(("0" + $(this).val().trim()));
                                        }
                                    }
                                    catch (ex) { }
                                    if (regDecimal.test($(this).val().trim()) && $(this).attr("data-autoapply-decimal") == "true") {
                                        if (parseInt($(this).val()) % 1 === 0) {
                                            $(this).val((parseFloat($(this).val()).toFixed(2)));
                                        }
                                    }
                                }
                                //appCommon.CheckNumberPanel($(this));

                            }
                            //else if (event.type == "focusout" && $(this).attr("data-val-type") == "phone") {
                            //    appCommon.ApplyDefaultPhoneFormat(this);
                            //}
                            if (IsValid) {
                                $(this).removeClass("input-validation-error");
                                $(errorControl).html("");
                                appCommon.CheckNumberPanel($(this));
                            }
                            else {
                                $(this).addClass("input-validation-error");
                                if (typeof $(this).attr("data-val-touched") == "undefined") {
                                    $(this).attr("data-val-touched", true);
                                }
                            }
                        }
                        else {
                            appCommon.CheckNumberPanel($(this));
                        }
                    });
                    if (data_val_type == "number" || data_val_type == "decimal" || data_val_type == "currency")
                        appCommon.CheckNumberPanel($(ctrl));
                }
            },
            CheckNumberPanel: function (ctrl) {
                if ($(ctrl).attr("data-val-type") == "number" || $(ctrl).attr("data-val-type") == "decimal" || $(ctrl).attr("data-val-type") == "currency") {
                    try {
                        if (typeof $(ctrl).closest('[data-number-panel-sum]') != 'undefined' && $(ctrl).closest('[data-number-panel-sum]').length > 0) {
                            return appCommon.ValidateNumberPanel($(ctrl).closest('[data-number-panel-sum]'));
                        }
                    }
                    catch (datanumberpanelsumex) { }
                }
            },
            ValidateNumberPanel: function (ctrl) {
                try {
                    var controlName = $(ctrl).attr("name");
                    var errorControl = $("[data-valmsg-for='" + controlName + "']");
                    var totalValue = 0;
                    var TotalValueToBe = $(ctrl).attr('data-number-panel-sum');
                    if (regNumber.test(TotalValueToBe)) {
                        $(ctrl).find('[data-val-type="number"],[data-val-type="decimal"],[data-val-type="currency"]').each(function (item) {
                            if ($(this).val() != null && $(this).val().trim() != '')
                                totalValue += parseFloat($(this).val());
                            if (typeof $(this).attr("data-val-touched") != "undefined") {
                                showValidationMessage = true;
                            }
                        });
                    }
                    if (totalValue != TotalValueToBe) {
                        $(errorControl).html("Must be equal to " + TotalValueToBe + "");
                    }
                    else {
                        $(errorControl).html("");
                    }
                    //  }
                    if ($(ctrl).find("[show-data-number-panel-total]") != "undefined" && $(ctrl).find("[show-data-number-panel-total]").length > 0) {
                        appCommon.WriteValueToControl($(ctrl).find("[show-data-number-panel-total]"), totalValue + "%");
                    }
                    return totalValue == TotalValueToBe;
                } catch (ex) {
                    return false;
                }
                return true;
            },
            WriteValueToControl: function (ctrl, value) {
                if (typeof $(ctrl) != "undefined" && $(ctrl).length > 0) {
                    if ($(ctrl).attr("type") == "text" || $(ctrl).attr("type") == "textarea") {
                        $(ctrl).val(value);
                    }
                    else {
                        $(ctrl).html(value);
                    }
                }
            },
            ReadValueFromControl: function (ctrl) {
                if (typeof $(ctrl) != "undefined" && $(ctrl).length > 0) {
                    if ($(ctrl).attr("type") == "text" || $(ctrl).attr("type") == "textarea") {
                        return $(ctrl).val();
                    }
                    else {
                        return $(ctrl).html();
                    }
                }
                return "";
            },
            IsValidEmail: function (email) {
                if (regExpEmail.test(email)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidURL: function (url) {
                if (regExpUrl.test(url)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidBase64Content: function (input) {
                if (regExpbase64Format.test(input)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidGUID: function (input) {
                if (regExpGUID.test(input)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidZipCode: function (ControlRef, zipcode) {
                var regZipcode = regExpZipcode_US;
                if ($(ControlRef).attr("data-val-zipcode-format") == _LookUpZipcodeFormat.IN) {
                    regZipcode = regExpZincode_In;
                }
                if (regZipcode.test(zipcode)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidZipCodeExtension: function (ControlRef, zipcodeextension) {
                if (regExpZipcodeExtension.test(zipcodeextension)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidMobile: function (mobileno) {
                if (regExpMobilePattern.test(mobileno)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidRegDate: function (RegDate) {
                if (regExpDate.test(RegDate)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidFEIN: function (FEIN) {
                if (regExpFEIN.test(FEIN) && (typeof InvalidFEINOrSSNList == "undefined" || InvalidFEINOrSSNList == null || InvalidFEINOrSSNList.indexOf(appCommon.RemoveFEINFormat(FEIN)) == -1)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidSSN: function (SSN) {
                if (regExpSSN.test(SSN) && (typeof InvalidFEINOrSSNList == "undefined" || InvalidFEINOrSSNList == null || InvalidFEINOrSSNList.indexOf(appCommon.RemoveSSNFormat(SSN)) == -1)) {
                    return true;
                }
                else {
                    return false;
                }
            },
            IsValidCurrency: function (ctrl) {
                var currencyCode = $(ctrl).attr("data-val-currency-code");
                var regEx = appCommon.GetRegExFromCurrency(currencyCode);
                var value = appCommon.ReadValueFromControl(ctrl);
                value = appCommon.RemoveCurrencyFormat(value);
                //if ($(ctrl).attr("data-val-currency-format") == _LookUpCurrencyFormat.INR && regExpCurrencyINR.test(value)) {
                //    return true;
                //}
                //else if (regExpCurrencyUSD.test(value)) {
                //    return true;
                //}
                if (regEx.test(value)) {
                    return true;
                }
                return false;
            },
            CheckDecimalFormatRuleOnEnter: function (ctrl) {
                try {
                    var value = appCommon.ReadValueFromControl(ctrl);
                    if (typeof value != 'undefined' && value != "" && value != null && value.indexOf('.') > 0) {
                        var valuesArray = value.split('.');
                        if (valuesArray != null && valuesArray.length > 1) {
                            if (valuesArray.length > 2)
                                return false;
                            var precisionValue = valuesArray[1].length + 1;
                            var maxPrecision = $(ctrl).attr("data-max-precision");
                            if (typeof maxPrecision == "undefined" || maxPrecision == "")
                                return true;
                            maxPrecision = parseInt(maxPrecision);
                            if (precisionValue > maxPrecision)
                                return false;
                        }
                    }
                }
                catch (ex) {
                }
                return true;
                //var regDynamicExpDecimal = new RegExp('^\\d+\\.\\d{' + minPrecision + ',' + maxPrecision + '}$');
                //return false;
            },
            MakeDefaultButton: function (item, Panel) {
                $(Panel).keypress(function (event) {
                    var charCode = (event.which) ? event.which : event.keyCode
                    if (charCode == 13) {
                        $(item).click();
                    }
                });
            },
            ConvertJsonDateToDate: function (jsonDate) {
                return moment(jsonDate).format(ServerDateFormat);
            },
            ConvertStringDateToDate: function (stringDate) {
                return moment(stringDate, ServerDateFormat);
            },
            ConvertStringDateToDateWithFormat: function (stringDate, dateformat) {
                try {
                    if (typeof dateformat == "undefined") dateformat = ServerDateFormat;
                    var date = moment(stringDate, ServerDateFormat).format(dateformat);
                    if (date == "Invalid date")
                        date = moment(stringDate).format(dateformat);
                    return date;
                } catch (ex) { }
            },
            SubmitForm: function (formdataRef, CallbackFun, Url, HideLoader, ShowLoader) {
                if (typeof Url == 'undefined' || Url == '' || Url == null) {
                    Url = $(formdataRef).prop("action");
                }
                if (typeof Url == 'undefined' || Url == '' || Url == null) {
                    return;
                }
                if (typeof HideLoader == 'undefined' || HideLoader == '' || HideLoader == null) {
                    HideLoader = false;
                }
                if (typeof ShowLoader == 'undefined' || ShowLoader == null) {
                    ShowLoader = true;
                }
                if (ShowLoader)
                    appCommon.ShowLoader();
                $.ajax({ url: Url, type: "POST", data: $(formdataRef).serialize() })
                    .done(function (response, textStatus, xhr) {
                        if (typeof CallbackFun == 'function')
                            CallbackFun(response, xhr.status);
                        console.log("Success");
                    })
                    .fail(function (response, textStatus, xhr) {
                        appCommon.TemporaryContent(response);
                        if (typeof CallbackFun == 'function')
                            CallbackFun(response, response.status);
                        console.log("Failure");
                        appCommon.HideLoader();
                    })
                    .always(function () {
                        if (HideLoader) {
                            appCommon.HideLoader();
                        }
                    });
            },
            ExecuteAjaxGetMethod: function (Url, CallbackFun, ShowLoader) {
                if (typeof Url == 'undefined' || Url == '' || Url == null || Url.trim() == '') {
                    return;
                }
                if (ShowLoader)
                    appCommon.ShowLoader();
                $.ajax({ url: Url, type: "Get" })
                    .done(function (response, textStatus, xhr) {
                        if (appCommon.HandleSessionTimeout(response)) {
                            return;
                        }

                        CallbackFun(response, xhr.status);
                    })
                    .fail(function (response, textStatus, xhr) {
                        if (appCommon.HandleSessionTimeout(response.responseText)) {
                            return;
                        }

                        CallbackFun(response, response.status);
                    })
                    .always(function () {
                        if (ShowLoader)
                            appCommon.HideLoader();
                    });
            },
            ExecuteAjaxPostMethod: function (Url, Data, CallbackFun, ShowLoader, Is_IncludedAttachements) {
                if (typeof Url == 'undefined' || Url == '' || Url == null || Url.trim() == '') {
                    return;
                }
                if (ShowLoader)
                    appCommon.ShowLoader();

                if (typeof Is_IncludedAttachements != 'undefined' && Is_IncludedAttachements) {
                        $.ajax({
                            url: Url, type: "POST", data: Data, contentType: false,
                            processData: false
                        })
                        .done(function (response, textStatus, xhr) {
                            if (appCommon.HandleSessionTimeout(response)) {
                                return;
                            }

                            CallbackFun(response, xhr.status);
                        })
                        .fail(function (response, textStatus, xhr) {
                            if (appCommon.HandleSessionTimeout(response.responseText)) {
                                return;
                            }
                            CallbackFun(response, response.status);
                        })
                        .always(function () {
                            if (ShowLoader)
                                appCommon.HideLoader();
                        });
                }
                else {
                        $.ajax({
                            url: Url, type: "POST", data: Data
                        })
                        .done(function (response) {
                            if (appCommon.HandleSessionTimeout(response)) {
                                return;
                            }
                            CallbackFun(response);
                        })
                        .fail(function (response) {
                            if (appCommon.HandleSessionTimeout(response.responseText)) {
                                return;
                            }
                            appCommon.TemporaryContent(response);
                            CallbackFun(response);
                        })
                        .always(function () {
                            if (ShowLoader)
                                appCommon.HideLoader();
                        });
                }
            },
            HandleSessionTimeout: function (response) {
                if (typeof response === 'string' && response.indexOf('window.top.location.href') > -1) {
                    document.open();
                    document.write(response);
                    document.close();
                    return true;
                }

                return false;
            },
            TemporaryContent: function (response) {
                if (response.status == HttpStatusCode.TemporaryRedirect) {
                    if (typeof $('[data-name="TemporaryContent"]') == "undefined" || $('[data-name="TemporaryContent"]').length == 0) {
                        $('body').append('<div data-name="TemporaryContent"></div>');
                    }
                    else {
                        if (typeof $('[data-name="TemporaryContent"]').find('.modal') != 'undefined' && $('[data-name="TemporaryContent"]').find('.modal').length > 0) {
                            $('[data-name="TemporaryContent"]').find('.modal').each(function (i, modal) {
                                $(modal).modal("hide");
                            });
                        }
                    }
                    $('[data-name="TemporaryContent"]').html("");
                    $('[data-name="TemporaryContent"]').append(response.responseText);
                    appCommon.InitializePanelOrFormValidations($('[data-name="TemporaryContent"]'));
                }
            },
            ClearInVisibleHTMLControlsData: function (panel, IsModel) {
                if (typeof panel != 'undefined' && panel != null) {
                    $(panel).find('input[type=text],input[type=email],input[type=phone],textarea,select').each(function (i, item) {
                        var isVisibleMode = appCommon.IsVisibleCtrl(item, IsModel, $(panel));
                        var isDisabledMode = appCommon.IsDisabledCtrl(item);
                        if (!isVisibleMode && !isDisabledMode) {
                            $(item).val("");
                        }
                    });
                }
            },
            EnableVisibleHTMLControls: function (inputPanel, IsModel) {
                if (typeof inputPanel == 'undefined' || inputPanel == null)
                    return null;

                //The selected value doesn't reserve for select control with the clone.
                $(inputPanel).find('select').each(function (i, item) {
                    $(item).find("option:selected").attr("selected", "selected");
                });
                var panel = $(inputPanel).clone();
                appCommon.ClearInVisibleHTMLControlsData(panel, IsModel);
                if (typeof panel != 'undefined' && panel != null) {
                    $(panel).find('input[type=text],input[type=email],input[type=phone],textarea,select').each(function (i, item) {
                        var isVisibleMode = appCommon.IsVisibleCtrl(item, IsModel, $(panel));
                        var isDisabledMode = appCommon.IsDisabledCtrl(item);
                        if (isVisibleMode && isDisabledMode) {
                            $(item).removeAttr("disabled");
                        }
                    });
                }
                return panel;
            },
            RenderHTMLPanel: function (Url, PanelIdentifierToLoad, CallbackFun, ShowLoader, ShowLoaderCallbackFun, IsModal) {
                if (ShowLoader) {
                    if (IsModal) {
                        appCommon.ShowLoader();
                        //$(".preloader").css("z-index", "1200");
                    }
                    else {
                        appCommon.ShowPartialPanelLoader(PanelIdentifierToLoad);
                        $(PanelIdentifierToLoad).addClass("vsSpinner");
                        appCommon.AdjustWindowMinHeight(PanelIdentifierToLoad);
                    }
                }
                if (typeof ShowLoaderCallbackFun != 'undefined' && ShowLoaderCallbackFun != null) {
                    ShowLoaderCallbackFun();
                }
                $.ajax({ url: Url, type: "Get" })
                    .done(function (response, textStatus, xhr) {
                        if (IsModal) {
                            appCommon.HideLoader();
                        }
                        else {
                            $(PanelIdentifierToLoad).removeClass("vsSpinner");
                        }
                        $(PanelIdentifierToLoad).html(response);
                        appCommon.InitializePanelOrFormValidations($(PanelIdentifierToLoad));
                        appCommon.AdjustWindowMinHeight(PanelIdentifierToLoad);
                        if (typeof CallbackFun != "undefined" && CallbackFun != null)
                            CallbackFun(response);
                    })
                    .fail(function (response, textStatus) {
                        if (IsModal) {
                            appCommon.HideLoader();
                        }
                        else if (typeof PanelIdentifierToLoad != 'undefined' && PanelIdentifierToLoad != null && $(PanelIdentifierToLoad).length > 0) {
                            $(PanelIdentifierToLoad).removeClass("vcSpinner");
                            $(PanelIdentifierToLoad).html("");
                        }
                        appCommon.TemporaryContent(response);
                        if (typeof CallbackFun != 'undefined' && CallbackFun != null && CallbackFun != "") {
                            try {
                                CallbackFun(response);
                            } catch (ex) { }
                        }
                    })
                    .always(function () {
                        /*$(PanelIdentifierToLoad).removeClass("vcSpinner");*/
                    });
            },
            FormatToCurrencyFormat: function (ctrl) {
                if (typeof $(ctrl) != 'undefined' && $(ctrl).length > 0) {
                    var type = $(ctrl).attr("type")
                    var value = appCommon.ReadValueFromControl(ctrl);
                    if (value != '' && value != null) {
                        value = appCommon.RemoveCurrencyFormat(value);
                        var fractionDigits = 0;
                        var data_currency_fractionDigits = $(ctrl).attr("data-currency-fractionDigits");
                        try {
                            if (typeof data_currency_fractionDigits != 'undefined' && data_currency_fractionDigits != null && data_currency_fractionDigits != '' && parseInt(data_currency_fractionDigits)) {
                                fractionDigits = parseInt(data_currency_fractionDigits);
                            }
                        } catch (ex) { }
                        var formattedValue = "";
                        try {
                            if ($(ctrl).attr("data-val-currency-format") == _LookUpCurrencyFormat.INR) {

                                formattedValue = new Intl.NumberFormat('en-IN', {
                                    style: 'currency',
                                    currency: 'INR',
                                    maximumFractionDigits: fractionDigits,
                                    minimumFractionDigits: fractionDigits
                                }).format(value);
                            }
                            else {
                                formattedValue = new Intl.NumberFormat('en-US', {
                                    style: 'currency',
                                    currency: 'USD',
                                    maximumFractionDigits: fractionDigits,
                                    minimumFractionDigits: fractionDigits
                                }).format(value)
                            }
                            //var currencyCode = $(ctrl).attr("data-val-currency-code");
                            //var locale = appCommon.GetLocaleFromCurrency(currencyCode);
                            //formattedValue = new Intl.NumberFormat(locale, {
                            //    style: 'currency',
                            //    currency: currencyCode,
                            //    maximumFractionDigits: fractionDigits,
                            //    minimumFractionDigits: fractionDigits
                            //}).format(value)
                            if (formattedValue != "" && formattedValue != null)
                                appCommon.WriteValueToControl(ctrl, formattedValue);
                            //appCommon.PlaceCursorAtEndOfNumber(ctrl);
                        }
                        catch (ex) { }
                    }
                }
            },
            GetCurrencyInfoByCode: function (currencyCode) {
                var currenciesFiltered = currencies.filter(c => c.CurrencyCode == currencyCode);
                if (currenciesFiltered.length == 0) {
                    currenciesFiltered = currencies.filter(c => c.CurrencyCode == 'USD');
                }
                return currenciesFiltered[0];
            },
            GetLocaleFromCurrency: function (currencyCode) {
                var currency = appCommon.GetCurrencyInfoByCode(currencyCode);
                return currency.Locale;
            },
            GetRegExFromCurrency: function (currencyCode) {
                var currency = appCommon.GetCurrencyInfoByCode(currencyCode);
                return currency.RegularExpression;
            },
            PlaceCursorAtEndOfNumber: function ($inputElement) {
                // Get the value of the input field
                let inputValue = $inputElement.val();

                // Regular expression to match the last sequence of digits in the string
                let numberMatch = inputValue.match(/\d+(\.\d+)?/g);

                // If there is a number found, place the cursor after the last number
                if (numberMatch) {
                    let lastNumber = numberMatch[numberMatch.length - 1];
                    let lastNumberIndex = inputValue.lastIndexOf(lastNumber);

                    // Set the cursor position using setSelectionRange
                    $inputElement[0].setSelectionRange(lastNumberIndex + lastNumber.length, lastNumberIndex + lastNumber.length);
                } else {
                    // If no number is found, place the cursor at the end of the input
                    $inputElement[0].setSelectionRange(inputValue.length, inputValue.length);
                }
            },
            FormatToDateTimeFormat: function (ctrl, DateTimeFormat) {
                if (typeof $(ctrl) != 'undefined' && $(ctrl).length > 0) {
                    var type = $(ctrl).attr("type")
                    var value = appCommon.ReadValueFromControl(ctrl);
                    if (value != '' && value != null) {
                        try {
                            var formattedValue = moment(value).format(DateTimeFormat);
                            if (formattedValue != "" && formattedValue != null)
                                appCommon.WriteValueToControl(ctrl, formattedValue);
                        }
                        catch (ex) { }
                    }
                }
            },
            RemoveCurrencyFormat: function (value) {
                if (value != "" && value != null) {
                    //return value.replace('$', '').replace('₹', '').replace(/,/g, '')
                    return value.replace('$', '').replace('₹', '').replace('€', '').replace('£', '').replace('¥', '').replace(/,/g, '')
                }
                return "";
            },
            RemoveCurrencyFormatForPanel: function (panel) {
                try {
                    $(panel).find('[data-val-type="currency"]').each(function (i, item) {
                        var value = appCommon.ReadValueFromControl(item);
                        value = appCommon.RemoveCurrencyFormat(value);
                        $(item).val(value);
                    });
                } catch (ex) { }
            },
            ShowLoader: function () {
                $(".classic-spinner").removeClass("invisible");
            },
            HideLoader: function () { $(".classic-spinner").addClass("invisible"); },
            ShowOffcanvasLoader: function () {
                return $('[data-name="OffcanvasLoader"]').prop('outerHTML').replace('invisible', '');
            },
            ShowPartialPanelLoader: function (PanelIdentifier) {
                if (typeof PartialPanelLoader != 'undefined' && PartialPanelLoader != null) {
                    var stepsBar = $(PanelIdentifier).find(".stepsbar");
                    $(PanelIdentifier).html("");
                    if ($(stepsBar).length > 0) {
                        $(PanelIdentifier).append($(stepsBar));
                    }
                    $(PanelIdentifier).append($(PartialPanelLoader));
                }
            },
            ShowPanelLoader: function (PanelIdentifier) {
                if (typeof PartialPanelLoader != 'undefined' && PartialPanelLoader != null) {
                    var panelLoader = $(PartialPanelLoader).clone();
                    var Left = (PanelIdentifier.width()) / 2 - ($(panelLoader).width() / 2),
                        Top = (PanelIdentifier.height()) / 2 - ($(panelLoader).height() / 2);
                    $(PanelIdentifier).append(panelLoader);
                    $(panelLoader).css({
                        'left': Left,
                        'top': Top,
                        'position': 'absolute'
                    });
                }
            },
            ApplyPhoneFormat: function (PhoneNo, valueformat) {
                if (typeof PhoneNo != 'undefined' && PhoneNo != null && PhoneNo != '' && valueformat != 'undefined' && valueformat != null && valueformat != '') {
                    //var PhoneNoParts = PhoneNo.replace(/\D/g, '').match(/(\d{0,3})(\d{0,3})(\d{0,4})/);
                    //if (PhoneNoParts != null && PhoneNoParts != '')
                    //    return !PhoneNoParts[2] ? PhoneNoParts[1] : '(' + PhoneNoParts[1] + ') ' + PhoneNoParts[2] + (PhoneNoParts[3] ? '-' + PhoneNoParts[3] : '');
                    for (var c = 0; c < PhoneNo.length; c++) {
                        valueformat = valueformat.replace(/#/, PhoneNo.charAt(c));
                    }
                    // return valueformat.replace(/#/g,"_");
                    var indexOfFormatCharater = valueformat.indexOf("#");
                    if (indexOfFormatCharater > -1)
                        return valueformat.substring(0, indexOfFormatCharater).trim();
                    return valueformat != null && valueformat != 'undefined' ? valueformat.trim() : valueformat;
                }
                return "";
            },
            RemovePhoneFormat: function (PhoneNo) {
                if (typeof PhoneNo != 'undefined' && PhoneNo != null && PhoneNo != '') {
                    return PhoneNo.replace(/[^0-9]/gi, '');
                }
                return "";
            },
            RemovePhoneFaxFormatForPanel: function (panel) {
                try {
                    $(panel).find('[data-val-type="phone"]').each(function (i, item) {
                        var value = appCommon.ReadValueFromControl(item);
                        value = appCommon.RemovePhoneFormat(value);
                        $(item).val(value);
                    });
                } catch (ex) { }
            },
            ApplyDefaultPhoneFormat: function (ctrl) {
                var value = $(ctrl).val().trim();
                value = value.replace(/[^0-9]/gi, '');
                if ($(ctrl).attr("data-apply-format") == "true" && $(ctrl).attr("data-default-val-format") == "true" && value == "") {
                    var valueformat = $(ctrl).attr("data-val-format");
                    if (valueformat != 'undefined' && valueformat != null && valueformat != '') {
                        $(ctrl).attr("placeholder", valueformat.replace(/#/g, "_"));
                    }
                }
            },
            ApplySSNFormat: function (SSNNo) {
                if (typeof SSNNo != 'undefined' && SSNNo != null && SSNNo != '') {
                    var SSNNoParts = SSNNo.replace(/\D/g, '').match(/(\d{0,3})(\d{0,2})(\d{0,4})/);
                    if (SSNNoParts != null && SSNNoParts != '')
                        return !SSNNoParts[2] ? SSNNoParts[1] : SSNNoParts[1] + '-' + SSNNoParts[2] + (SSNNoParts[3] ? '-' + SSNNoParts[3] : '');
                }
                return "";
            },
            RemoveSSNFormat: function (SSNNo) {
                if (typeof SSNNo != 'undefined' && SSNNo != null && SSNNo != '') {
                    return SSNNo.replace(/[^0-9]/gi, '');
                }
                return "";
            },
            ApplyFEINFormat: function (FEINNo) {
                if (typeof FEINNo != 'undefined' && FEINNo != null && FEINNo != '') {
                    var FEINNoParts = FEINNo.replace(/\D/g, '').match(/(\d{0,2})(\d{0,7})/);
                    if (FEINNoParts != null && FEINNoParts != '')
                        return !FEINNoParts[2] ? FEINNoParts[1] : FEINNoParts[1] + '-' + FEINNoParts[2];
                }
                return "";
            },
            RemoveFEINFormat: function (FEINNo) {
                if (typeof FEINNo != 'undefined' && FEINNo != null && FEINNo != '') {
                    return FEINNo.replace(/[^0-9]/gi, '');
                }
                return "";
            },
            ApplyCCFormat: function (CCNumber) {
                if (typeof CCNumber != 'undefined' && CCNumber != null && CCNumber != '') {
                    var CCNumberParts = CCNumber.replace(/\D/g, '').match(/(\d{0,4})(\d{0,4})(\d{0,4})(\d{0,4})/);
                    if (CCNumberParts != null && CCNumberParts != '' && CCNumberParts.length > 1 && CCNumberParts[1] != "") {
                        var FormatedValue = CCNumberParts[1];
                        if (CCNumberParts.length > 2 && CCNumberParts[2] != "") {
                            FormatedValue += " " + CCNumberParts[2];
                            if (CCNumberParts.length > 3 && CCNumberParts[3] != "") {
                                FormatedValue += " " + CCNumberParts[3];
                                if (CCNumberParts.length > 4 && CCNumberParts[4] != "") {
                                    FormatedValue += " " + CCNumberParts[4];
                                }
                            }
                        }
                        return FormatedValue;
                    }
                }
                return "";
            },
            RemoveFormat: function (value) {
                if (typeof value != 'undefined' && value != null && value != '') {
                    return value.replace(/[^0-9]/gi, '');
                }
                return "";
            },
            RenderJsonViewFormat: function (jsoninput) {
                try {
                    var displayContentJson = JSON.parse(jsoninput);
                    return JSON.stringify(displayContentJson, undefined, 4);
                }
                catch (ex) { }
                return jsoninput;
            },
            RenderXMLViewFormat: function (xmlinput) {
                const padding = ' '.repeat(2);
                const reg = /(>)(<)(\/*)/g;
                let pad = 0;
                xmlinput = xmlinput.replace(reg, '$1\r\n$2$3');
                var Nodes = [];
                $(xmlinput.split('\r\n')).each(function (index, node) {
                    let indent = 0;
                    if (node.match(/.+<\/\w[^>]*>$/)) {
                        indent = 0;
                    } else if (node.match(/^<\/\w/) && pad > 0) {
                        pad -= 1;
                    } else if (node.match(/^<\w[^>]*[^\/]>.*$/)) {
                        indent = 1;
                    } else {
                        indent = 0;
                    }

                    pad += indent;

                    Nodes.push(padding.repeat(pad - indent) + node);
                });
                xmlinput = Nodes.join('\r\n');
                return xmlinput;
            },
            DeepCloneData: function (input) {
                return JSON.parse(JSON.stringify(input));
            },
            RenderObjectData: function (template, dataItem) {
                if (typeof dataItem == "undefined" || dataItem == null || typeof template == "undefined" || template == null) {
                    return;
                }
                var properties = Object.getOwnPropertyNames(dataItem);
                var conditionalStatmentsCtrls = $(template).find('[data-if]');
                try {
                    if (typeof conditionalStatmentsCtrls != 'undefined' & conditionalStatmentsCtrls != null && conditionalStatmentsCtrls.length > 0) {
                        $(conditionalStatmentsCtrls).each(function (i, statementctrl) {
                            if (typeof $(statementctrl).attr("data-condition") != 'undefined')
                                $(statementctrl).removeAttr("data-condition");
                        });
                    }
                }
                catch (ex) { }
                $(properties).each(function (i, property) {
                    try {
                        //   $(template).html(appCommon.ReplaceAllOccurence($(template).html(), ('{{' + property + '}}'), dataItem[property]));
                    }
                    catch (ex) { }
                    var control = $(template).find('[data-bind="' + property + '"]');
                    if (typeof control != 'undefined' && control != null && control.length > 0) {
                        appCommon.WriteValueToControl(control, dataItem[property]);
                        //$(control).html(dataItem[property]);
                        if ($(control).attr('data-val-type') == 'currency') {
                            appCommon.FormatToCurrencyFormat(control);
                        }
                        else if ($(control).attr('data-val-type') == 'datetime') {
                            appCommon.FormatToDateTimeFormat(control, $(control).attr("data-datetime-format"));
                        }

                    }
                    var control = $(template).find('[data-src="' + property + '"]');
                    if (typeof control != 'undefined' && control != null && control.length > 0) {
                        if (appCommon.IsValidBase64Content(dataItem[property]))
                            $(control).attr("src", "data:image/png;base64," + dataItem[property]);
                        else
                            $(control).attr("src", dataItem[property]);
                    }

                    var control = $(template).find('[data-data-val-format="' + property + '"]');
                    if (typeof control != 'undefined' && control != null && control.length > 0) {
                        $(control).attr("data-val-format", dataItem[property]);
                        var value = $(control).val().trim();
                        value = value.replace(/[^0-9]/gi, '');
                        appCommon.ApplyDefaultPhoneFormat(control);
                        if (value != "") {
                            $(control).val(appCommon.ApplyPhoneFormat(value, dataItem[property]));
                        }
                    }



                    var control = $(template).find('[data-alt="' + property + '"]');
                    if (typeof control != 'undefined' && control != null && control.length > 0) {
                        $(control).attr("alt", dataItem[property]);
                    }
                    //Find Anchor tags
                    $(template).find('a').each(function (a, anchorItem) {
                        var href = $(anchorItem).attr("data-href");
                        if (typeof href != 'undefined' && href != null) {
                            $(anchorItem).attr("href", appCommon.ReplaceAllOccurence(href, ('{{' + property + '}}'), dataItem[property]));
                        }
                    });

                    try {
                        if (typeof conditionalStatmentsCtrls != 'undefined' & conditionalStatmentsCtrls != null && conditionalStatmentsCtrls.length > 0) {
                            $(conditionalStatmentsCtrls).each(function (i, statementctrl) {
                                var data_condition = typeof $(statementctrl).attr("data-condition") != 'undefined' ? $(statementctrl).attr("data-condition") : $(statementctrl).attr("data-if");
                                $(statementctrl).attr("data-condition", appCommon.ReplaceAllOccurenceWord(data_condition, property, dataItem[property]));
                            });

                        }
                    }
                    catch (ex) { }
                });


                try {
                    if (typeof conditionalStatmentsCtrls != 'undefined' & conditionalStatmentsCtrls != null && conditionalStatmentsCtrls.length > 0) {
                        $(conditionalStatmentsCtrls).each(function (i, statementctrl) {
                            var data_if = $(statementctrl).attr("data-condition");
                            if (typeof data_if != "undefined" && data_if != null) {
                                if (!eval(data_if)) {
                                    $(statementctrl).hide();
                                }
                                else {
                                    $(statementctrl).show();
                                }
                            }
                        });

                    }
                }
                catch (ex) { }

                return template;
            },

            ReplaceAllOccurence: function (input, match, replaceValue) {
                if (typeof input != 'undefined' && input != null)
                    return input = input.replace(new RegExp(match, "gi"), replaceValue);
            },
            ReplaceAllOccurenceWord: function (input, match, replaceValue) {
                if (typeof input != 'undefined' && input != null)
                    return input = input.replace(new RegExp('\\b' + match + '\\b', "gi"), replaceValue);
                return "";
            },
            DisableAllControls: function (panel) {
                if (typeof $(panel) != 'undefined') {
                    $(panel).find('input[type="text"],input[type="file"],input[type="checkbox"],input[type="radio"],input[type="submit"],input[type="button"],textarea,select,button,a').each(function () {
                        if ($(this).attr('data-disable') != 'false' && (typeof $(this).closest('[data-disabled]') == 'undefined' || $(this).closest('[data-disabled]').length == 0)) {
                            $(this).attr("disabled", "disabled");
                            $(this).off("click");
                            $(this).click(function () {
                                return false;
                            });
                        }
                    });
                }
            },
            DisableAllControlsWithEvents: function (panel) {
                if (typeof $(panel) != 'undefined') {
                    $(panel).find('input[type="text"],input[type="file"],input[type="checkbox"],input[type="radio"],input[type="submit"],input[type="button"],textarea,select,button,a').each(function () {
                        if ($(this).attr('data-disable-with-events') != 'false' && (typeof $(this).closest('[data-disabled]') == 'undefined' || $(this).closest('[data-disabled]').length == 0)) {
                            $(this).attr("disabled", "disabled");
                        }
                    });
                }
            },
            EnableAllControlsWithEvents: function (panel) {
                if (typeof $(panel) != 'undefined') {
                    $(panel).find('input[type="text"],input[type="file"],input[type="checkbox"],input[type="radio"],input[type="submit"],input[type="button"],textarea,select,button,a').each(function () {
                        $(this).attr("disabled", false);
                    });
                }
            },
            ResetFieldsInPanel: function (panel) {
                $(panel).find('input[type="text"],input[type="file"],input[type="checkbox"],input[type="radio"],input[type="submit"],input[type="button"],textarea,select,button,a').each(function () {
                    var controlType = $(this)[0].tagName == "INPUT" ? $(this).attr("type").toLowerCase() : $(this)[0].tagName.toLowerCase();
                    if (controlType == 'radio' || controlType == 'checkbox') {
                       // $(this).attr('checked', false);
                        $(this).prop('checked', false);
                    }
                    else if (controlType == "select" && $(this).attr("size") != null && $(this).attr("size") != undefined) {
                        appCommon.ClearSelectContrlData($(this), false);
                    }
                    else {
                        $(this).val('');
                    }
                });
            },
            GetCCType: function (input) {
                if (typeof input != "undefined" && input != "" && input != null) {
                    input = appCommon.ReplaceAllOccurence(input, " ", "");
                    if (regExpCCVisa.test(input)) {
                        return CCType.Visa;
                    }
                    else if (regExpCCAmericanExpress.test(input)) {
                        return CCType.AmericanExpress;
                    }
                    else if (regExpCCMastercard.test(input)) {
                        return CCType.Mastercard;
                    }
                    else if (regExpCCDiscover.test(input)) {
                        return CCType.Discover;
                    }
                    else if (regExpCCJCB.test(input)) {
                        return CCType.JCB;
                    }
                    else if (regExpCCDinersClub.test(input)) {
                        return CCType.DinersClub;
                    }
                }
            },
            ConvertStringToBoolean: function (input) {
                return JSON.parse(input);
            },
            ShowMessageModal: function (message, titile) {
                $('[data-name="ModalMessageTitle"]').html('');
                $('[data-name="ModalMessageBody"]').html('');
                $('[data-name="ModalMessageBody"]').html(message);
                if (typeof titile != "undefined" && titile != null && titile != '')
                    $('[data-name="ModalMessageTitle"]').html(titile);
                else
                    $('[data-name="ModalMessageTitle"]').html('Warning!');
                $('[data-name="ModalMessage"]').modal("show");
            },
            HideMessageModal: function () {
                $('[data-name="ModalMessage"]').modal("hide");
            },
            ShowConfirmationModal: function (message, titile, CallbackFun, CallbackFunCancel) {
                $('[data-name="ModalConfirmationTitle"]').html('');
                $('[data-name="ModalConfirmationMessageBody"]').html('');
                $('[data-name="ModalConfirmation"]').modal("show");
                $('[data-name="ModalConfirmationMessageBody"]').html(message);
                if (typeof titile != "undefined" && titile != null && titile != '')
                    $('[data-name="ModalConfirmationTitle"]').html(titile);
                else
                    $('[data-name="ModalConfirmationTitle"]').html("Confirmation");

                $('[data-name="ModalConfirmationOK"]').off("click");
                $('[data-name="ModalConfirmationOK"]').click(function () {
                    appCommon.HideConfirmationModal();
                    if (typeof CallbackFun != 'undefined' && CallbackFun != null)
                        CallbackFun();
                });
                $('[data-name="ModalConfirmationCancel"]').off("click");
                $('[data-name="ModalConfirmationCancel"]').click(function () {
                    if (typeof CallbackFunCancel != 'undefined' && CallbackFunCancel != null)
                        CallbackFunCancel();
                });
            },
            HideConfirmationModal: function () {
                $('[data-name="ModalConfirmation"]').modal("hide");
            },
            EnablePopovers: function () {
                try {
                    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
                    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
                        return new bootstrap.Popover(popoverTriggerEl)
                    })
                }
                catch (ex) { }
            },
            ClearSelectContrlData: function (Ctrl, AddEmptyOption) {
                if (typeof Ctrl == 'undefined' || Ctrl.length == 0)
                    return;
                if (typeof AddEmptyOption == 'undefined')
                    AddEmptyOption = false;
                $(Ctrl).find("option").remove().end();
                if (AddEmptyOption) {
                    var emptyoption = document.createElement('option');
                    emptyoption.text = "Select";
                    emptyoption.value = "";
                    $(Ctrl).append(emptyoption);
                }
            },
            BindDataToSelectContrl: function (Ctrl, dataItems, ValueField, TextField, IgnoreEmptyOption) { // ,selectDefaultValue
                if (typeof Ctrl == 'undefined' || Ctrl.length == 0)
                    return;
                if (typeof IgnoreEmptyOption == 'undefined')
                    IgnoreEmptyOption = false;
                $(Ctrl).find("option").remove().end();
                if (!IgnoreEmptyOption) {
                    var emptyoption = document.createElement('option');
                    emptyoption.text = "Select";
                    emptyoption.value = "";
                    $(Ctrl).append(emptyoption);
                }
                if (typeof dataItems != 'undefined' && dataItems != null && dataItems.length > 0) {
                    if (typeof TextField == 'undefined')
                        TextField = ValueField;
                    for (var i = 0; i < dataItems.length; i++) {
                        var option = document.createElement('option');
                        option.text = dataItems[i][TextField];
                        option.value = dataItems[i][ValueField];
                        //if (typeof selectDefaultValue != 'undefined' && selectDefaultValue !=null) {
                        //    if (option.value == selectDefaultValue)
                        //        $(option).prop('selected', true);
                        //}
                        $(Ctrl).append(option);
                    }
                }


            },
            BuildQueryStringParameters: function (ObjParameter) { //{ LineGUID: LineGUID, CompanyLocationGuid: CompanyLocationGuid }
                var QueryString = "";
                if (typeof ObjParameter != "undefined" && ObjParameter != null) {
                    $(Object.getOwnPropertyNames(ObjParameter)).each(function (O, propertyName) {
                        var value = ObjParameter[propertyName];
                        if (typeof value == 'undefined' || value === "")
                            value = null;
                        QueryString += (propertyName + "=" + value) + "&";
                    });
                    QueryString = QueryString.replace(/&+$/, '')
                }
                return QueryString;
            },
            ShoworHideQuoteInfoEditNavTabs: function (NavTabReference) {
                if (typeof NavTabReference != "undefined" && NavTabReference != null) {
                    if ($(NavTabReference).length > 0) {
                        $(NavTabReference).click();
                    }
                }
            },
            ActiveNavLinkTab: function (NavLinkTabReference) {
                if (typeof NavLinkTabReference != "undefined" && NavLinkTabReference != null) {
                    var navTabList = $(NavLinkTabReference).closest(".nav-tabs");
                    if (typeof navTabList != "undefined" && navTabList != null && navTabList.length > 0) {
                        $(navTabList).find(".nav-link").each(function (i, item) {
                            $(item).removeClass("active");
                        });
                    }
                    $(NavLinkTabReference).addClass("active");
                }
            },
            ActiveNavLinkTabContainer: function (NavTabReference) {
                if (typeof NavTabReference != "undefined" && NavTabReference != null) {
                    var navTabList = $(NavTabReference).parent().find(".tab-pane");
                    if (typeof navTabList != "undefined" && navTabList != null && navTabList.length > 0) {
                        $(navTabList).each(function (i, item) {
                            $(item).removeClass("show active");
                        });
                    }
                    $(NavTabReference).addClass("show active");
                }
            },
            UpdateBrowserURLHistory: function (URLParameter, ObjRoutingParametersArray, ObjSearchParameters) {
                var url = new URL(window.location);
                if ((URLParameter != "undefined" && URLParameter != null && URLParameter != "")) {
                    var newURL = url.protocol + "//" + url.host + (URLParameter.startsWith("/") ? "" : "/") + URLParameter;
                    if (newURL.charAt(newURL.length - 1) == '/') {
                        newURL = newURL.substr(0, newURL.length - 1);
                    }
                    url = new URL(newURL);
                }

                if (typeof ObjRoutingParametersArray != "undefined" && ObjRoutingParametersArray != null && ObjRoutingParametersArray.length > 0) {
                    var href = url.href;
                    $(ObjRoutingParametersArray).each(function (O, value) {
                        href = href + "/" + value;
                    });
                    url = new URL(href);
                }

                if (typeof ObjSearchParameters != "undefined" && ObjSearchParameters != null && typeof ObjSearchParameters === 'object') {
                    $(Object.getOwnPropertyNames(ObjSearchParameters)).each(function (O, propertyName) {
                        var value = ObjSearchParameters[propertyName];
                        //if (typeof value == 'undefined' || value == "")
                        //    value = null;
                        if (typeof value != 'undefined')
                            url.searchParams.set(propertyName, value);
                    });
                }
                window.history.pushState({}, "", url);
            },
            DownloadBase64File: function (DocumentFileInfo) {
                const linkSource = `data:${DocumentFileInfo.ContentType};base64,${DocumentFileInfo.Base64String}`;
                const downloadLink = document.createElement('a');
                document.body.appendChild(downloadLink);
                downloadLink.href = linkSource;
                downloadLink.target = '_self';
                downloadLink.download = DocumentFileInfo.Name;
                downloadLink.click();
            },
            AdjustWindowMinHeight: function (PanelIdentifierToLoad) {
                try {
                    var IssetMinHeight = true;
                    //if (typeof $(PanelIdentifierToLoad).attr("data-name") != "undefined" && $(PanelIdentifierToLoad).attr("data-name") == "QuoteCompanyLinesPanel") {
                    if (typeof $(PanelIdentifierToLoad).attr("data-name") != "undefined" && ($(PanelIdentifierToLoad).attr("data-name") == "QuoteCompanyLinesPanel" || $(PanelIdentifierToLoad).attr("data-name") == "NotesDashboardMessagesPanel" || $(PanelIdentifierToLoad).attr("data-name") == "NotesDashboardTasksPanel")) {
                        IssetMinHeight = false;
                    }
                    if (IssetMinHeight)
                        $(PanelIdentifierToLoad).css("min-height", window.innerHeight - $(PanelIdentifierToLoad).offset().top - 32);//bottom padding top 16px bottom 16px;

                }
                catch (ex) {
                    console.log("Failed to set min height");
                }
            },
            ReArrangeFormInputsGroupSequence: function (panel) {
                var rowNumber = 0;
                $(panel).each(function (t, item) {
                    $(item).find('input,textarea,select').each(function (e, element) {
                        var attrName = $(this).attr("name");
                        var InstanceName = null;
                        if (attrName != null && typeof attrName != 'undefined') {
                            if (attrName.lastIndexOf("[") > 0) {
                                InstanceName = attrName.substring(0, attrName.lastIndexOf("["));
                            }
                            if (attrName.lastIndexOf(".") > -1) {
                                attrName = attrName.substring(attrName.lastIndexOf(".") + 1);
                            }
                            if (InstanceName != null && InstanceName != 'undefined' && attrName != null && attrName != "" && attrName != 'undefined')
                                $(this).attr("name", (InstanceName + "[" + (rowNumber).toString() + "]." + attrName));
                        }
                    });
                    if ($(item).html().trim() != "" && $(item).css('display') != 'none')
                        rowNumber++;
                });
            },
            CloneForm: function (form) {
                var clonedForm = $(form).clone(false);
                $(clonedForm).find('select').each(function (s, sitem) {
                    $(sitem).val($(form).find('[name="' + $(sitem).attr("name") + '"]').val());
                });
                return clonedForm;
            },
            OpenBrowserChildWindow: function (Url, WindowCloseEventCallbackFun, ShowLoader) {
                if (typeof Url == 'undefined' || Url == '' || Url == null || Url.trim() == '') {
                    return;
                }
                if (ShowLoader)
                    appCommon.ShowLoader();
                const editChildWindow = window.open(Url, "_blank", "toolbar=no,scrollbars=yes,resizable=yes,menubar=no,location=yes,status=no")
                if (!editChildWindow) {
                    // The window wasn't allowed to open
                    // This is likely caused by built-in popup blockers.
                    // …
                    appCommon.ShowMessageModal("The popup blocker appears to be blocking the child window from launching.");
                    if (ShowLoader) {
                        appCommon.HideLoader();
                    }
                }
                var popupBlockedAttempts = 0;
                var editChildWindowTick = setInterval(function () {
                    if (typeof editChildWindow === 'undefined' || editChildWindow == null) {
                        popupBlockedAttempts++;
                        if (popupBlockedAttempts > 60)//terminate after 1 hour.
                            clearInterval(editChildWindowTick);
                    }
                    else {
                        if (editChildWindow.closed) {
                            clearInterval(editChildWindowTick);
                            if (ShowLoader) {
                                appCommon.HideLoader();
                            }
                            if (typeof WindowCloseEventCallbackFun !== 'undefined' && WindowCloseEventCallbackFun != null && WindowCloseEventCallbackFun != '')
                                WindowCloseEventCallbackFun();
                        }
                    }
                }, 1000);
            },
            MakeLinkActive: function (link) {
                $(link).closest('.sidebar-nav').find('li').find('a').removeClass('active');
                $(link).addClass('active');
            },
            GenericModalFunction(title, ModalBody) {
                $('[data-name="GenericModal"]').find(".modal-title").html("").html(title);
                $('[data-name="GenericModal"]').find('.modal-body').html("").html(ModalBody);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "600px");

                //if (headerElementHtml) {
                //    $('[data-name="GenericModal"]').find('[data-name="IsPlanActive"]').remove();
                //    $('[data-name="GenericModal"]').find(".modal-header .btn-close").before(
                //        `${headerElementHtml}`
                //    );
                //}
                $('[data-name="GenericModal"]').modal('show');
            },
            OffcanvasGeneralFunction(title, body) {
                $('[data-name="OffCanvasRightGeneral"]').find(".offcanvas-footer").html('');
                $('[data-name="OffCanvasRightGeneral"]').find(".offcanvas-title").html(title);
                $('[data-name="OffCanvasRightGeneral"]').find(".offcanvas-body").html(body);
                $('[data-name="OffCanvasRightGeneral"]').css('--bs-offcanvas-width', '400px');
                $('[data-name="OffCanvasRightGeneral"]').offcanvas('show');
            },
            ApplyCurrencyFormat: function (value) {
                return new Intl.NumberFormat('en-us', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 0
                }).format(value);
            },
            ApplyCurrencyFormatWithFractionDigits: function (value, FractionDigits) {
                return new Intl.NumberFormat('en-us', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: FractionDigits,
                    maximumFractionDigits: FractionDigits
                }).format(value);
            },
            IsValidJSONformat: function (input) {
                try {
                    JSON.parse(input);
                    return true;
                } catch (e) {
                    return false;
                }
            },
            IsValidXMLformat: function (input) {
                var parser = new DOMParser();
                var xmlDoc = parser.parseFromString(input, "text/xml");
                return xmlDoc.getElementsByTagName("parsererror").length === 0;
            },
            MakeButtonActive: function (ctrl) {
                $(ctrl).closest('.policy-buttons').find('[data-name="policybuttons"]').removeClass('active');
                $(ctrl).addClass('active');
            },
            ResponseRedirectToLogin: function (response) {
                var responseText = typeof response === "string" ? response : $(response).html();
                if (responseText.includes("Sign In") && responseText.includes("Username") && responseText.includes("Password")) {
                    window.location.href = "/Login/Index";
                    return;
                }
            },
        }
    }
    appCommon = new App();
}
catch (exceptin) {
    console.log(exceptin + "in app.js")
}



//Add CSS Class

var style = document.createElement('style');
style.type = 'text/css';
style.innerHTML = "  .validation-summary-errors {\
border: 2px solid red;\
color: red;\
font-weight: bold;\
margin: 6px;\
width: 30%;\
}\
        .field-validation-error, .input-validation-error {\
    color: #a94442;\
}\
.field-validation-error {\
    margin-top: 5px;\
margin-bottom: 0px;\
}\
.input-validation-error {\
    border: 1px solid #a94442;\
}\
";
document.getElementsByTagName('head')[0].appendChild(style);
