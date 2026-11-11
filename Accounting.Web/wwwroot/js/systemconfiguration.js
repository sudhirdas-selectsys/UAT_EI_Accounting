$(function () {
    BindSystemConfigurationEvents();
    BindPaginationEvents();
});
var allChartItems = [];
var validChartNamesMap = {};
var PageSize;
var PageNumber;
var TotalPages;
function BindSystemConfigurationEvents() {
    $('[data-name="btnEditPDCOA"]').off('click');
    $('[data-name="btnEditPDCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentDirectionsCOA"]');
        $currentRow.find('td .dropdown.qv-dropdown').removeClass('disabled-link');

        // Show save and cancel buttons, hide edit button
        $currentRow.find('[data-name="btnEditPDCOA"]').hide();
        $currentRow.find('[data-name="btnSavePDCOA"]').show();
        $currentRow.find('[data-name="btnCancelPDCOA"]').show();
    });
    $('[data-name="btnEditCOA"]').off('click');
    $('[data-name="btnEditCOA"]').on('click', function () {
        var tableName = $(this).attr('data-table-name');
        var trCOA = $(this).closest('[data-name="trCOA"]');
        trCOA.find('[data-name="COAText"]').hide();
        trCOA.find('[data-name="COADropdown"]').show();
        trCOA.find('[data-name="btnEditCOA"]').hide();
        if (tableName == "LineItemsCOA") {
            trCOA.find('[data-name="btnSaveLineItemCOA"]').show();
        }
        if (tableName == "OperatingBanksCOA") {
            trCOA.find('[data-name="btnSaveOperatingBankCOA"]').show();
        }
        if (tableName == "PaymentDirectionsCOA") {
            trCOA.find('[data-name="btnSavePaymentDirectionCOA"]').show();
        }
        trCOA.find('[data-name="btnCancelCOA"]').show();
    });

    $('[data-name="btnCancelPDCOA"]').off('click');
    $('[data-name="btnCancelPDCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentDirectionsCOA"]');
        $currentRow.find('td .dropdown.qv-dropdown').addClass('disabled-link');

        // Hide save and cancel buttons, show edit button
        $currentRow.find('[data-name="btnEditPDCOA"]').show();
        $currentRow.find('[data-name="btnSavePDCOA"]').hide();
        $currentRow.find('[data-name="btnCancelPDCOA"]').hide();
    });
    $('[data-name="btnCancelPDCOA"]').off('click');
    $('[data-name="btnCancelPDCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentDirectionsCOA"]');
        var $dropdown = $currentRow.find('td .dropdown.qv-dropdown');
        $dropdown.find('.dropdown-menu').removeClass('show');
        $dropdown.find('.qv-dropdown-label').removeClass('show');
        $dropdown.addClass('disabled-link');
        //$currentRow.find('td .dropdown.qv-dropdown').addClass('disabled-link');

        // Hide save and cancel buttons, show edit button
        $currentRow.find('[data-name="btnEditPDCOA"]').show();
        $currentRow.find('[data-name="btnSavePDCOA"]').hide();
        $currentRow.find('[data-name="btnCancelPDCOA"]').hide();
    });

    $('[data-name="btnCancelCOA"]').off('click');
    $('[data-name="btnCancelCOA"]').on('click', function () {
        var tableName = $(this).attr('data-table-name');
        var trCOA = $(this).closest('[data-name="trCOA"]');
        SaveCancelCOA(trCOA, tableName);
    });

    $('[data-name="btnSavePDCOA"]').off('click');
    $('[data-name="btnSavePDCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trPaymentDirectionsCOA"]');
        var clonedTR = $currentRow.clone();
        var $singleRowForm = $('<form>').append(clonedTR);
        appCommon.SubmitForm($singleRowForm, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    appCommon.ShowMessageModal(response.Message, "Message");
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        appCommon.ShowMessageModal("An error occurred while syncing the Chart of Accounts.", "Message")
                    }
                }
            }
            appCommon.HideLoader();
        }, "/SystemConfiguration/PaymentDirectionChartofAccounts", false, true);
    });

    $('[data-name="SelectCompany"]').off('click');
    $('[data-name="SelectCompany"]').on('click', function () {
        var OffCanvasTitle = "Choose Company";
        appCommon.OffcanvasGeneralFunction(OffCanvasTitle, appCommon.ShowOffcanvasLoader());
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/SelectCompany", function (response) {
            if (response != null && response != undefined) {
                appCommon.OffcanvasGeneralFunction(OffCanvasTitle, response);
                $('[data-name="OffCanvasRightGeneral"] .offcanvas-footer').html(`
                    <button type="button" class="btn btn-secondary me-1" data-bs-dismiss="offcanvas">Close</button>
                    <button type="button" class="btn btn-primary" data-name="btnSaveDefaultClientSettings" style="display:none; width:100px;">Save</button>
                    <button type="button" class="btn btn-primary ms-1" data-name="btnSelectCompany">Select</button>
                    `);
                $('[data-name="OffCanvasRightGeneral"]').css('--bs-offcanvas-width', '440px');
                BindSystemConfigurationEvents();
            }
        }, false);
    });
    $('[data-name="SelectClient"]').off('click');
    $('[data-name="SelectClient"]').on('click', function () {
        $('[data-name="ClientDropdown"] .dropdown-loader').show();
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/SelectClient", function (response) {
            if (response != null && response != undefined) {
                $('[data-name="ClientDropdown"]').html(response);
                BindSystemConfigurationEvents();
                $('[data-name="ClientDropdown"] .dropdown-loader').hide();
                var Client = $('[data-name="ClientDropdown"]');
                if (Client.hasClass('show')) {
                    $('[data-name="policy-menu"]').css('height', 'inherit');
                    $('[data-name="policy-menu"]').css('overflow', 'inherit');
                }
                else {
                    $('[data-name="policy-menu"]').css('overflow-x', 'hidden');
                    $('[data-name="policy-menu"]').css('overflow-y', 'auto');
                }
            }
        }, false);
    });
    $('[data-name="divSelectClient"]').off('click');
    $('[data-name="divSelectClient"]').on('click', function () {
        var selectedClientId = $(this).attr('data-client-id');
        appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/SelectClient", { Client_Id: selectedClientId }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    window.location.href = "/SystemConfiguration/Index";
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
    $('[data-name="btnSaveDefaultClientSettings"]').off('click');
    $('[data-name="btnSaveDefaultClientSettings"]').on('click', function () {
        var selectedClientId = $('input[data-name="rdnDefaultClient"]:checked').attr('data-client-id');
        if (!selectedClientId) {
            appCommon.ShowMessageModal("Please select a default client to proceed.", "Alert");
            return;
        }
        else {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/SaveDefaultClientSettings", { Client_Id: selectedClientId }, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        //appCommon.ShowMessageModal("Default client settings changed successfully.", "Sucess");
                        $('[data-name="OffCanvasRightGeneral"]').offcanvas('hide');
                        window.location.href = "/";
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
        }
    });
    $('[data-name="btnSelectCompany"]').off('click');
    $('[data-name="btnSelectCompany"]').on('click', function () {
        var selectedClientId = $('[data-name="divCompanyList"]').find(".dropdown-item.active-company").attr('data-client-id');
        if (selectedClientId == null || typeof selectedClientId == "undefined") {
            appCommon.ShowMessageModal("Please select a client to proceed.", "Alert");
            return;
        }
        else {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/SelectCompany", { Client_Id: selectedClientId }, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        $('[data-name="OffCanvasRightGeneral"]').offcanvas('hide');
                        window.location.href = "/";
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
        }
    });
    $('[data-name="divSelectCompany"]').off('click');
    $('[data-name="divSelectCompany"]').on('click', function () {
        $(this).closest('.role-container').find(".dropdown-item").removeClass("active-company");
        $(this).closest('.dropdown-item').addClass("active-company");
    });
    $('input[data-name="rdnDefaultClient"]').on('change', function () {
        $('[data-name="btnSaveDefaultClientSettings"]').show();
    });
    $('[data-name="btnSyncChartOfAccounts"]').off('click');
    $('[data-name="btnSyncChartOfAccounts"]').on('click', function () {
        appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/SyncChartofAccounts", null, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    if (response.Message != "") {
                        appCommon.ShowMessageModal(response.Message, "Success");
                    }
                    else {
                        appCommon.ShowMessageModal("Chart of accounts synced successfully.", "Success");
                    }
                    LoadChartOfAccounts();
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


    $('[data-name="btnSaveOperatingBankCOA"]').off('click');
    $('[data-name="btnSaveOperatingBankCOA"]').on('click', function () {
        var tableName = $(this).attr('data-table-name');
        var trCOA = $(this).closest('[data-name="trCOA"]');
        var trCOAindex = trCOA.attr('data-index');
        var clonedTR = trCOA.clone();
        clonedTR.find('input, select').each(function (i, e) {
            if (typeof $(this).attr('name') !== 'undefined') {
                var updatedName = $(this).attr('name').replace(`[${trCOAindex}]`, '');;
                $(this).attr('name', updatedName);
            }
        });
        var $singleRowForm = $('<form>').append(clonedTR);
        if (appCommon.IsValidForm($singleRowForm)) {
            appCommon.SubmitForm($singleRowForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message, "Message");
                        }
                        else {
                            appCommon.ShowMessageModal("An error occurred while inserting Chart of Accounts records for the Operating Bank.")
                        }
                    }
                }
                SaveCancelCOA(trCOA, tableName);
                appCommon.HideLoader();
                LoadOperatingBankChartOfAccounts();
            }, "/SystemConfiguration/OperatingBankChartofAccounts", false, true);
        }
    });

    $('[data-name="btnSavePaymentDirectionCOA"]').off('click');
    $('[data-name="btnSavePaymentDirectionCOA"]').on('click', function () {
        //var LineitemsForm = $('[data-name="frmLineItems"]');
        var tableName = $(this).attr('data-table-name');
        var trCOA = $(this).closest('[data-name="trCOA"]');
        var trCOAindex = trCOA.attr('data-index');
        var clonedTR = trCOA.clone();
        clonedTR.find('input, select').each(function (i, e) {
            if (typeof $(this).attr('name') !== 'undefined') {
                var updatedName = $(this).attr('name').replace(`[${trCOAindex}]`, '');;
                $(this).attr('name', updatedName);
            }
        });
        var $singleRowForm = $('<form>').append(clonedTR);
        if (appCommon.IsValidForm($singleRowForm)) {
            appCommon.SubmitForm($singleRowForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message, "Message");
                        }
                        else {
                            appCommon.ShowMessageModal("An error occurred while inserting Chart of Accounts records for the Payment Directions.", "Message")
                        }
                    }

                }
                SaveCancelCOA(trCOA, tableName);
                appCommon.HideLoader();
                LoadPaymentDirectionChartOfAccounts();
            }, "/SystemConfiguration/PaymentDirectionChartofAccounts", false, true);
        }
    });

    $('.toggle-menu-btn-lob:not(.policy-menu-heading )').off('click');
    $('.toggle-menu-btn-lob:not(.policy-menu-heading )').on('click', function () {
        $('.card .policy-menu').show()
        $('body').append('<div class="backdrop"></div>').show();
    });

    $('.toggle-menu-btn-lob.policy-menu-heading').off('click');
    $('.toggle-menu-btn-lob.policy-menu-heading').on('click', function () {
        if ($(window).width() > 1023) {
            $('.policy-menu').toggleClass('menu-lean');
            $(this).toggleClass('menu-wrap');
        }
    });
    $(document).on('focus', '[data-name="ChartofAccount"]', function () {
        const rowIndex = $(this).data('row-index');
        loadChartItems(rowIndex);
    });

    $(document).on('keyup', '[data-name="ChartofAccount"]', function () {
        const rowIndex = $(this).data('row-index');
        filterFunction(rowIndex);
    });
    $(document).on('blur', '[data-name="ChartofAccount"]', function () {
        const rowIndex = $(this).data('row-index');
        const $input = $(this);
        const $hiddenInput = $(`[data-name="ChartofAccount_Id"][data-row-index="${rowIndex}"]`);
        const validNames = validChartNamesMap[rowIndex] || [];

        setTimeout(() => {
            if (!validNames.includes($input.val().toLowerCase())) {
                $input.val('');
                $hiddenInput.val('');
            }
        }, 200);
    });
    $('[data-tab-name="btnGetChartofAccounts"]').off('click');
    $('[data-tab-name="btnGetChartofAccounts"]').on('click', function () {
        LoadChartOfAccounts();
        MakeSubMenuActive($('[data-tab-name="btnGetChartofAccounts"]'))
    });

    $('[data-tab-name="btnLineItemsBillingTypes"]').off('click');
    $('[data-tab-name="btnLineItemsBillingTypes"]').on('click', function () {
        var TabName = $(this).attr('data-tab-type');
        LoadLineItemsChartOfAccounts(TabName);
    });
    $('[data-tab-name="btnLineItemsFeeTaxes"]').off('click');
    $('[data-tab-name="btnLineItemsFeeTaxes"]').on('click', function () {
        var TabName = $(this).attr('data-tab-type');
        LoadLineItemsChartOfAccounts(TabName);
    });
    $('[data-tab-name="btnOperatingBank"]').off('click');
    $('[data-tab-name="btnOperatingBank"]').on('click', function () {
        LoadOperatingBankChartOfAccounts();
    });
    $('[data-tab-name="btnPaymentDirections"]').off('click');
    $('[data-tab-name="btnPaymentDirections"]').on('click', function () {
        LoadPaymentDirectionChartOfAccounts();
    });
    $('[data-tab-name="btnLobCarrierGrouping"]').off('click');
    $('[data-tab-name="btnLobCarrierGrouping"]').on('click', function () {
        LoadLOBCarrierGrouping();
    });
    $('[data-name="btnAddLobCarrierGroup"]').off('click');
    $('[data-name="btnAddLobCarrierGroup"]').on('click', function (event) {
        event.preventDefault();
        var lobSelected = $('[data-name="lob-option"]:checked').length > 0;
        //var carrierSelected = $('[data-name="carrier-option"]:checked').length > 0;
        if (!lobSelected) {
            MakeSubMenuActive($('[data-tab-name="btnLobCarrierGrouping"]'));
            appCommon.ShowMessageModal("Please select at least one LOB and One Carrier.", "Alert");
        }
        else {
            var frmLOBCarrierGroupingForm = $('[data-name="frmLOBCarrierGroupingForm"]');
            if (appCommon.IsValidForm(frmLOBCarrierGroupingForm)) {
                var lobValues = $('[data-name="lob-option"]:checked').map(function () { return $(this).data('lob-id'); }).get();
                var carrierValues = $('[data-name="carrier-option"]:checked').map(function () { return $(this).data('carrier-id'); }).get();
                var carrierChecked = $('#selectAllCarriers');
                var isSelectAll = carrierChecked.is(':checked');
                var description = frmLOBCarrierGroupingForm.find('input[name="Description"]').val();
                var lobCarrierGroupingform = {
                    LOB_Ids: lobValues,
                    Carrier_Ids: isSelectAll ? 0 : carrierValues,
                    Description: description
                };

                appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/LobCarrierGrouings", { lobcarriergrouping: lobCarrierGroupingform }, function (response) {
                    if (response != null && typeof response != 'undefined') {
                        if (response.Status) {
                            if (response.Message != null && response.Message != '') {
                                appCommon.ShowMessageModal(response.Message, "Sucess");
                                LoadLOBCarrierGrouping();
                            }
                        }
                        else {
                            if (response.Message != null && response.Message != '') {
                                appCommon.ShowMessageModal(response.Message);
                            }
                            else {
                                appCommon.ShowMessageModal("Unable to process the request.");
                            }
                        }
                    }
                }, true);
            }
        }
    });

    //$('[data-name="btnEditLOBCarrierGrouping"]').off('click');
    //$('[data-name="btnEditLOBCarrierGrouping"]').on('click', function () {
    //    var row = $(this).closest("tr");
    //    var lobs = row.find('input[data-name="edit-lob-option"]:checked').map(function () { return this.value; }).get();
    //    var description = row.find('input[data-name="editdescription"]').val();

    //    row.data('ELobs', lobs);
    //    row.data('EDescription', description);

    //    row.find("[data-name='viewlobgroup'], [data-name='viewcarriergroup'],[data-name='viewdescription']").hide();
    //    row.find("[data-name='editlobgroup'], [data-name='editcarriergroup'],[data-name='editdescription']").show();
    //    row.find("[data-name='btnEditLOBCarrierGrouping']").hide();
    //    row.find("[data-name='btnSaveLOBCarrierGrouping'], [data-name='btnCancelLOBCarrierGrouping']").show();
    //    updateLobChips(row.find('[data-name="edit-lob-container"]'));
    //});

    $('[data-name="btnEditLOBCarrierGrouping"]').off('click');
    $('[data-name="btnEditLOBCarrierGrouping"]').on('click', function () {

        var row = $(this).closest("tr");
        var lobContainer = row.find('[data-name="edit-lob-container"]');
        var carrierContainer = row.find('[data-name="edit-carrier-container"]');
        var description = row.find('input[data-name="editdescription"]').val();
        row.data('EDescription', description);
        if (lobContainer.find('input[data-name="edit-lob-option"]:checked').not('[data-lob-id="0"]').length === 0) {
            lobContainer.find('input[data-name="edit-lob-option"]').prop('checked', true);
        }
        if (carrierContainer.find('input[data-name="edit-carrier-option"]:checked').not('[data-carrier-id="0"]').length === 0) {
            carrierContainer.find('input[data-name="edit-carrier-option"]').prop('checked', true);
        }
        updateMultiSelectChips(lobContainer, 'input[data-name="edit-lob-option"]');
        updateMultiSelectChips(carrierContainer, 'input[data-name="edit-carrier-option"]');
        row.find("[data-name='viewlobgroup'],[data-name='viewcarriergroup'],[data-name='viewdescription']").hide();
        row.find("[data-name='editlobgroup'],[data-name='editcarriergroup'],[data-name='editdescription']").show();
        row.find("[data-name='btnEditLOBCarrierGrouping']").hide();
        row.find("[data-name='btnSaveLOBCarrierGrouping'], [data-name='btnCancelLOBCarrierGrouping']").show();
    });

    function updateMultiSelectChips(container, checkboxSelector) {

        const checkboxes = container.find(checkboxSelector).not('[data-lob-id="0"],[data-carrier-id="0"]');
        const selectAllCheckbox = container.find('[data-lob-id="0"],[data-carrier-id="0"]');
        const checked = checkboxes.filter(':checked');
        const chips = container.find('.selected-chips');
        const defaultText = container.find('.default-text');
        chips.empty();

        if (checked.length === 0) {
            defaultText.show();
        }
        else if (checked.length === checkboxes.length) {
            defaultText.hide();
            chips.append(`<div class="chip d-flex align-items-center gap-1 all-chip" data-value="all">
                <span>All</span>
                <span class="remove-chip">&times;</span>
            </div>`);
            selectAllCheckbox.prop('checked', true);
        }

        else {
            defaultText.hide();
            checked.each(function () {

                chips.append(`<div class="chip d-flex align-items-center gap-1" data-value="${this.value}">
                    <span>${this.value}</span>
                    <span class="remove-chip">&times;</span>
                </div>`);
            });
            selectAllCheckbox.prop('checked', false);
        }
    }

    $(document).on('change', 'input[data-name="edit-lob-option"]',
        function () {
            updateMultiSelectChips($(this).closest('[data-name="edit-lob-container"]'), 'input[data-name="edit-lob-option"]');
        }
    );
    $(document).on('change', 'input[data-name="edit-carrier-option"]',
        function () {
            updateMultiSelectChips($(this).closest('[data-name="edit-carrier-container"]'), 'input[data-name="edit-carrier-option"]');
        }
    );

    $('[data-name="btnCancelLOBCarrierGrouping"]').off('click');
    $('[data-name="btnCancelLOBCarrierGrouping"]').on('click', function () {
        var row = $(this).closest("tr");
        var CLobs = row.data('ELobs') || [];
        row.find('input[data-name="edit-lob-option"]').each(function () { this.checked = CLobs.includes(this.value); });

        var CDescription = row.data('EDescription') || '';
        row.find('input[data-name="editdescription"]').val(CDescription);

        row.find("[data-name='viewlobgroup'], [data-name='viewcarriergroup'],[data-name='viewdescription']").show();
        row.find("[data-name='editlobgroup'], [data-name='editcarriergroup'],[data-name='editdescription']").hide();
        row.find("[data-name='btnEditLOBCarrierGrouping']").show();
        row.find("[data-name='btnSaveLOBCarrierGrouping'], [data-name='btnCancelLOBCarrierGrouping']").hide();
        updateLobChips(row.find('[data-name="edit-lob-container"]'));
    });

    $('[data-name="btnSaveLOBCarrierGrouping"]').off('click');
    $('[data-name="btnSaveLOBCarrierGrouping"]').on('click', function (event) {

        event.preventDefault();
        const row = $(this).closest('tr');
        const lobcarriergroupId = row.data('lob-carrier-grouping-id');
        const lobChecked = row.find('input[data-name="edit-lob-option"]:checked');
        const carrierChecked = row.find('input[data-name="edit-carrier-option"]:checked');

        if (lobChecked.length === 0) {
            MakeSubMenuActive($('[data-tab-name="btnLobCarrierGrouping"]'));
            appCommon.ShowMessageModal("Please select at least one LOB and one Carrier.", "Alert");
            return;
        }

        //var isSelectAll = row.find('#selectAllCarriers_' + lobcarriergroupId).is(':checked');
        const description = row.find('input[data-name="editdescription"]').val();
        var lobCarrierGroupingform = {
            LOB_Carrier_Grouping_Id: lobcarriergroupId,
            LOB_Ids: lobChecked.map(function () { return $(this).data('lob-id'); }).get(),
            Carrier_Ids: carrierChecked.map(function () { return $(this).data('carrier-id'); }).get(),
            Description: description
        };

        appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/LobCarrierGrouings", { lobcarriergrouping: lobCarrierGroupingform }, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Sucess");
                        LoadLOBCarrierGrouping();
                    }
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message);
                    }
                    else {
                        appCommon.ShowMessageModal("Unable to process the request.");
                    }
                }
            }
        }, true);
    });


    $('[data-name="btnDeleteLOBCarrierGrouping"]').off('click');
    $('[data-name="btnDeleteLOBCarrierGrouping"]').on('click', function () {
        var LOB_Carrier_Grouping_Id = $(this).attr('data-lob-carrier-grouping-Id');
        appCommon.ShowConfirmationModal("Are you sure you want to delete this LOB_Carrier_Group?", "Alert", function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteLobcarrierGroupDetails?LOB_Carrier_Grouping_Id=" + LOB_Carrier_Grouping_Id, null, function (response) {
                if (response != null) {
                    if (response.Status) {
                        LoadLOBCarrierGrouping();
                        appCommon.ShowMessageModal(response.Message, "Sucess");
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

    $('[data-name="btnCOASearch"]').off('click');
    $('[data-name="btnCOASearch"]').on('click', function () {
        var container = $(this).closest('.dropdown-menu[data-name="DropDownContainer"]').find('[data-name="COAListContainer"]');
        var SerachTextValue = $(this).closest('.input-group').find('[data-name="COASearchText"]').val();
        if (typeof SerachTextValue == "undefined" || SerachTextValue == "") {
            SerachTextValue = null;
        }
        else {
            $(this).closest('.input-group').find('[data-name="btnCOASearch"]').hide();
            $(this).closest('.input-group').find('[data-name="btnCOASearchClear"]').show();
        }
        LoadChartOfAccountsList(container, SerachTextValue)
    });

    $('[data-name="btnCOASearchClear"]').off('click');
    $('[data-name="btnCOASearchClear"]').on('click', function () {
        var container = $(this).closest('.dropdown-menu[data-name="DropDownContainer"]').find('[data-name="COAListContainer"]');
        var SerachTextValue = $(this).closest('.input-group').find('[data-name="COASearchText"]').val("");
        $(this).closest('.input-group').find('[data-name="btnCOASearch"]').show();
        $(this).closest('.input-group').find('[data-name="btnCOASearchClear"]').hide();
        LoadChartOfAccountsList(container, SerachTextValue);
    });

    //$('.qv-dropdown').off('show.bs.dropdown');
    //$('.qv-dropdown').on('show.bs.dropdown', function () {
    //    var $dropDownContainer = $(this).find('div[data-name="DropDownContainer"]');
    //    LoadChartOfAccountsList($dropDownContainer, null);
    //});
    $('[data-name="btnEditBillingCOA"]').off('click');
    $('[data-name="btnEditBillingCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trBillingLineItems"]');
        $currentRow.find('td .dropdown.qv-dropdown').removeClass('disabled-link');

        // Show save and cancel buttons, hide edit button
        $currentRow.find('[data-name="btnEditBillingCOA"]').hide();
        $currentRow.find('[data-name="btnSaveBillingLineItemCOA"]').show();
        $currentRow.find('[data-name="btnCancelBillingCOA"]').show();
    });
    $('[data-name="btnCancelBillingCOA"]').off('click');
    $('[data-name="btnCancelBillingCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trBillingLineItems"]');
        $currentRow.find('td .dropdown.qv-dropdown').addClass('disabled-link');

        // Hide save and cancel buttons, show edit button
        $currentRow.find('[data-name="btnEditBillingCOA"]').show();
        $currentRow.find('[data-name="btnSaveBillingLineItemCOA"]').hide();
        $currentRow.find('[data-name="btnCancelBillingCOA"]').hide();
    });
    $('[data-name="btnCancelBillingCOA"]').off('click');
    $('[data-name="btnCancelBillingCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trBillingLineItems"]');
        var $dropdown = $currentRow.find('td .dropdown.qv-dropdown');
        $dropdown.find('.dropdown-menu').removeClass('show');
        $dropdown.find('.qv-dropdown-label').removeClass('show');
        $dropdown.addClass('disabled-link');
        //$currentRow.find('td .dropdown.qv-dropdown').addClass('disabled-link');

        // Hide save and cancel buttons, show edit button
        $currentRow.find('[data-name="btnEditBillingCOA"]').show();
        $currentRow.find('[data-name="btnSaveBillingLineItemCOA"]').hide();
        $currentRow.find('[data-name="btnCancelBillingCOA"]').hide();
    });
    $('[data-name="btnSaveBillingLineItemCOA"]').off('click');
    $('[data-name="btnSaveBillingLineItemCOA"]').on('click', function () {
        var $currentRow = $(this).closest('tr[data-name="trBillingLineItems"]');
        var TabName = $(this).closest('.action-btns').attr('data-tab-type');
        var clonedTR = $currentRow.clone();
        var $singleRowForm = $('<form>').append(clonedTR);
        appCommon.SubmitForm($singleRowForm, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    appCommon.ShowMessageModal(response.Message, "Message");
                }
                else {
                    if (response.Message != null && response.Message != '') {
                        appCommon.ShowMessageModal(response.Message, "Message");
                    }
                    else {
                        appCommon.ShowMessageModal("An error occurred while syncing the Chart of Accounts.", "Message")
                    }
                }
            }
            appCommon.HideLoader();
            //LoadLineItemsChartOfAccounts(TabName);
        }, "/SystemConfiguration/BillingLineItemsChartofAccounts", false, true);
    });
    $('[data-name="accLOBCarrier"]').off('click');
    $('[data-name="accLOBCarrier"]').on('click', function () {
        if (!$(this).hasClass('collapsed')) {
            var TabName = $(this).attr('data-tab-type');
            var LOB_Carrier_Group_Id = $(this).attr('data-lob-carrier-id');
            appCommon.RenderHTMLPanel("/SystemConfiguration/BillingLineItemsCOA" + "?TabName=" + TabName + "&LOB_Carrier_Grouping_Id=" + LOB_Carrier_Group_Id, $('[data-name="divLineItemstableContainer_' + LOB_Carrier_Group_Id + '"]'), function () {
                $('[data-name="divLineItemstableContainer_' + LOB_Carrier_Group_Id + '"]').css('min-height', '360px');
                BindSystemConfigurationEvents();
                BindCOADropDownEvents();
            }, true);
        }

    });
    $('[data-tab-name="btnAddOperatingBank"]').off('click');
    $('[data-tab-name="btnAddOperatingBank"]').on('click', function () {
        PageNumber = 1;
        PageSize = 10;
        LoadAddOperatingBank(PageNumber, PageSize);
    });

    $('[data-tab-name="btnLineofBusiness"]').off('click');
    $('[data-tab-name="btnLineofBusiness"]').on('click', function () {
        PageNumber = 1;
        PageSize = 10;
        LoadLineofBusiness(PageNumber, PageSize);
    });
    $('[data-tab-name="btnInstallmentPlan"]').off('click');
    $('[data-tab-name="btnInstallmentPlan"]').on('click', function () {
        PageNumber = 1;
        PageSize = 50;
        LoadInstallmentPlan(PageNumber, PageSize);
    });
    $('[data-tab-name="btnLineItems"]').off('click');
    $('[data-tab-name="btnLineItems"]').on('click', function () {
        PageNumber = MasterLookup._DefaultPageNumber;
        PageSize = MasterLookup._DefaultPageSize;
        LoadLineItemsList(PageNumber, PageSize);
    });

    $('[data-tab-name="btnUsers"]').off('click');
    $('[data-tab-name="btnUsers"]').on('click', function () {
        PageNumber = MasterLookup._DefaultPageNumber;
        PageSize = MasterLookup._DefaultPageSize;
        LoadUsersList(PageNumber, PageSize);
    });

    $('[data-tab-name="btnLobMapping"]').off('click');
    $('[data-tab-name="btnLobMapping"]').on('click', function () {
        LoadLOBMapping();
    });
    $('[data-tab-name="btnLineItemMapping"]').off('click');
    $('[data-tab-name="btnLineItemMapping"]').on('click', function () {
        LoadlineItemMappingIndex();
    });

    $('[data-name="btnAddLobMapping"]').off('click');
    $('[data-name="btnAddLobMapping"]').on('click', function (event) {
        event.preventDefault();
        var lobSelected = $('[data-name="lob-mapping-option"]:checked').length > 0;
        if (!lobSelected) {
            MakeSubMenuActive($('[data-tab-name="btnLobMapping"]'));
            appCommon.ShowMessageModal("Please select at least one LOB.", "Alert");
        }
        else {
            var frmLOBMappingForm = $('[data-name="frmLOBMappingForm"]');
            if (appCommon.IsValidForm(frmLOBMappingForm)) {
                var lobValues = $('[data-name="lob-mapping-option"]:checked').filter(function () { return $(this).data('lob-id') != 0; }).map(function () { return $(this).data('lob-id'); }).get();
                var lobMappingform = {
                    LOB_Ids: lobValues
                };

                appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/LobMapping", { lobmapping: lobMappingform }, function (response) {
                    if (response != null && typeof response != 'undefined') {
                        if (response.Status) {
                            if (response.Message != null && response.Message != '') {
                                appCommon.ShowMessageModal(response.Message, "Sucess");
                                LoadLOBMapping();
                            }
                        }
                        else {
                            if (response.Message != null && response.Message != '') {
                                appCommon.ShowMessageModal(response.Message);
                            }
                            else {
                                appCommon.ShowMessageModal("Unable to process the request.");
                            }
                        }
                    }
                }, true);
            }
        }
    });

    $('[data-name="btnDeleteLOBMapping"]').off('click');
    $('[data-name="btnDeleteLOBMapping"]').on('click', function () {
        var LOB_Mapping_Id = $(this).attr('data-lob-mapping-Id');
        appCommon.ShowConfirmationModal("Are you sure you want to delete this LOB_Mapping?", "Alert", function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteLobMapping?LOB_Mapping_Id=" + LOB_Mapping_Id, null, function (response) {
                if (response != null) {
                    if (response.Status) {
                        LoadLOBMapping();
                        appCommon.ShowMessageModal(response.Message, "Sucess");
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
    function ToggleSaveButton() {
        if ($('[data-name="chkEIUsage"][data-changed="true"]').length > 0) {
            $('#btnSaveCOAs').show();
        } else {
            $('#btnSaveCOAs').hide();
        }
    }

    $('[data-name="chkEIUsage"]').on('change', function () {
        var checkbox = $(this);
        var chartOfAccountId = checkbox.data('chartofaccount-id');
        var originallyChecked = checkbox.attr("data-original-checked") === "true";
        if (checkbox.is(':checked')) {
            // Only mark as changed if it wasn't already enabled
            if (!originallyChecked) {
                checkbox.attr("data-changed", "true");
            }
            ToggleSaveButton();
            return;
        }

        // User unchecked a newly selected COA (not saved yet)
        if (!originallyChecked) {
            checkbox.removeAttr("data-changed");
            ToggleSaveButton();
            return;
        }
        appCommon.ShowConfirmationModal(
            "Are you sure you want to disable this Chart of Account?",
            "Alert",
            function () {
                var chartOfAccounts = [{
                    ChartOfAccount_Id: chartOfAccountId,
                    Is_Checked: false
                }];
                appCommon.ExecuteAjaxPostMethod(
                    "/SystemConfiguration/ChartofAccountsUsage",
                    { chartOfAccounts: chartOfAccounts },
                    function (response) {
                        if (response != null && response.Status) {
                            checkbox.attr("data-original-checked", "false");
                            appCommon.ShowMessageModal(
                                "Chart of account saved successfully.",
                                "Success"
                            );
                            LoadChartOfAccounts();
                        }
                        else {
                            checkbox.prop("checked", true);
                            appCommon.ShowMessageModal(response.Message);
                        }
                    },
                    true
                );
            },
            function () {
                // User clicked No
                checkbox.prop("checked", true);
            });
    });


    $('#btnSaveCOAs').on('click', function () {
        var chartOfAccounts = [];
        $('[data-name="chkEIUsage"][data-changed="true"]').each(function () {
            chartOfAccounts.push({
                ChartOfAccount_Id: $(this).data('chartofaccount-id'),
                Is_Checked: true
            });
        });
        if (chartOfAccounts.length == 0) {
            appCommon.ShowMessageModal("Please select at least one Chart of Account.");
            return;
        }
        appCommon.ShowConfirmationModal(
            "Are you sure you want to enable the selected Chart(s) of Account?",
            "Alert",
            function () {
                appCommon.ExecuteAjaxPostMethod(
                    "/SystemConfiguration/ChartofAccountsUsage",
                    { chartOfAccounts: chartOfAccounts },
                    function (response) {
                        if (response != null && response.Status) {
                            $('[data-name="chkEIUsage"][data-changed="true"]').each(function () {
                                $(this)
                                    .attr("data-original-checked", "true")
                                    .removeAttr("data-changed");
                            });
                            ToggleSaveButton();
                            appCommon.ShowMessageModal(
                                "Chart of account(s) saved successfully.",
                                "Success"
                            );
                            LoadChartOfAccounts();
                        }
                        else {
                            appCommon.ShowMessageModal(response.Message);
                        }
                    },
                    true
                );
            },
            function () { }
        );
    });

    $('[data-tab-name="btnCommissionRules"]').off('click');
    $('[data-tab-name="btnCommissionRules"]').on('click', function () {
        PageNumber = MasterLookup._DefaultPageNumber;
        PageSize = MasterLookup._DefaultPageSize;
        LoadCommissionRules(PageNumber, PageSize);
    });
}
function LoadChartOfAccountsList(Container, SerachValue) {
    var SerachTextValue = null;
    if (typeof SerachValue == "undefined" || SerachValue == "" || SerachValue == null)
        SerachTextValue = null;
    else
        SerachTextValue = SerachValue;
    appCommon.ShowPartialPanelLoader($(Container));
    appCommon.ExecuteAjaxGetMethod("/api/SearchAPI/SuggestChartofAccounts" + "?SearchValue=" + SerachTextValue, function (response) {
        if (response.Status) {
            if (response.Data != null) {
                var COAListhtml = '';
                $.each(response.Data, function (index, item) {
                    COAListhtml += `
                    <div class="qv-item" data-name="COAItem" data-coa-id="${item.ChartOfAccounts_Id}"> 
                        <span>${item.Name}</span> 
                    </div>`;
                });
                $(Container).html(COAListhtml);
                BindCOADropDownEvents();
            }
            else {
                $(Container).html("No Records Found.");
            }
        }
        else {
            $(Container).html(response.Message);
        }
    }, false);
}
function loadChartItems(rowIndex) {
    if (allChartItems.length > 0) {
        filterFunction(rowIndex);
        return;
    }
    $.ajax({
        url: "/api/SearchAPI/SuggestChartofAccounts?SearchValue=null",
        type: "GET",
        success: function (res) {
            if (res?.Data?.length) {
                allChartItems = res.Data;
                filterFunction(rowIndex);
            }
        }
    });
}
function filterFunction(rowIndex) {
    const $input = $(`[data-name="ChartofAccount"][data-row-index="${rowIndex}"]`);
    const $dropdown = $(`[data-name="dropdownList"][data-row-index="${rowIndex}"]`);
    const $hiddenInput = $(`[data-name="ChartofAccount_Id"][data-row-index="${rowIndex}"]`);
    const $Client_Id = $(`[data-name="Client_Id"][data-row-index="${rowIndex}"]`);
    const $Service_Id = $(`[data-name="Service_Id"][data-row-index="${rowIndex}"]`);

    const searchTerm = $input.val().toLowerCase();
    $dropdown.empty();

    const filteredItems = allChartItems.filter(item =>
        item.Name.toLowerCase().includes(searchTerm)
    );

    validChartNamesMap[rowIndex] = filteredItems.map(x => x.Name.toLowerCase());

    if (filteredItems.length > 0) {
        filteredItems.forEach(item => {
            const $option = $(`<div class="dropdown-item" style="cursor:pointer;font-size: 12px;
				padding: 4px 8px;" data-id="${item.ChartOfAccounts_Id}">${item.Name}</div>`);
            $option.on('click', function () {
                $input.val(item.Name);
                $hiddenInput.val(item.ChartOfAccounts_Id);
                $Client_Id.val(item.Client_Id);
                $Service_Id.val(item.Service_Id);
                //$dropdown.hide();
            });
            $dropdown.append($option);
        });
        //$dropdown.show();
    } else {
        $dropdown.append(`<div class="dropdown-item text-muted">No results found</div>`).show();
    }
}
function SaveCancelCOA(trCOA, tableName) {
    trCOA.find('[data-name="COAText"]').show();
    trCOA.find('[data-name="COADropdown"]').hide();
    trCOA.find('[data-name="btnEditCOA"]').show();
    if (tableName == "LineItemsCOA") {
        trCOA.find('[data-name="btnSaveLineItemCOA"]').hide();
    }
    if (tableName == "OperatingBanksCOA") {
        trCOA.find('[data-name="btnSaveOperatingBankCOA"]').hide();
    }
    if (tableName == "PaymentDirectionsCOA") {
        trCOA.find('[data-name="btnSavePaymentDirectionCOA"]').hide();
    }
    trCOA.find('[data-name="btnCancelCOA"]').hide();
}

function LoadChartOfAccounts() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/GetChartofAccounts", $('[data-name="ConfigurationTabContainer"]'), function () { }, true);
}
function LoadLineItemsChartOfAccounts(TabName) {
    appCommon.RenderHTMLPanel("/SystemConfiguration/BillingLineItems_Grouping" + "?TabName=" + TabName, $('[data-name="ConfigurationTabContainer"]'), function () {
        if (TabName == "ARAPLineItems") {
            MakeSubMenuActive($('[data-tab-name="btnLineItemsBillingTypes"]'));
        }
        else if (TabName == "FeesTaxesLineItems") {
            MakeSubMenuActive($('[data-tab-name="btnLineItemsFeeTaxes"]'));
        }
        BindSystemConfigurationEvents()
    }, true);
}
function LoadOperatingBankChartOfAccounts() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/OperatingBankChartofAccounts", $('[data-name="ConfigurationTabContainer"]'), function () { MakeSubMenuActive($('[data-tab-name="btnOperatingBank"]')) }, true);
}
function LoadPaymentDirectionChartOfAccounts() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/PaymentDirectionChartofAccounts", $('[data-name="ConfigurationTabContainer"]'), function () { BindCOADropDownEvents(); MakeSubMenuActive($('[data-tab-name="btnPaymentDirections"]')) }, true);
}
function MakeSubMenuActive(ctrl) {
    $('.nav-link[data-name="btntab"]').removeClass("active");
    $(ctrl).addClass("active");
}
function updateLobChips(container) {
    const checkboxes = container.find('input[data-name="edit-lob-option"]');
    const checkedLobs = checkboxes.filter(':checked').map(function () { return this.value; }).get();
    const chipContainer = container.find('.selected-chips');
    chipContainer.empty();

    if (checkedLobs.length === 0) {
        chipContainer.append('<span class="text-muted">Select</span>');
    } else if (checkedLobs.length === checkboxes.length) {
        chipContainer.append(`
            <div class="chip d-flex align-items-center gap-1 all-chip" data-value="all" style="cursor:pointer;">
                <span>All</span>
                <span class="remove-chip">&times;</span>
            </div>
        `);
    } else {
        checkedLobs.forEach(lob => {
            chipContainer.append(`
                <div class="chip d-flex align-items-center gap-1" data-value="${lob}" style="cursor:pointer;">
                    <span>${lob}</span>
                    <span class="remove-chip">&times;</span>
                </div>
            `);
        });
    }
}

function BindCOADropDownEvents() {
    $('[data-name="COAItem"]').off('click');
    $('[data-name="COAItem"]').on('click', function () {
        var $clickedItem = $(this);
        var $dropdownContainer = $clickedItem.closest('[data-name="DropDownContainer"]');
        var $parentDropdown = $clickedItem.closest('.qv-dropdown');
        var $dropdownLabel = $parentDropdown.find('.qv-dropdown-label');
        var $dropdownMenu = $parentDropdown.find('.dropdown-menu');
        $dropdownContainer.find('[data-name="COAItem"]').removeClass('selected-item');
        $clickedItem.addClass('selected-item');
        var selectedText = $clickedItem.find('span').text() || $clickedItem.text();
        $dropdownLabel.find('span[data-name="SelectedCOA"]').text(selectedText.trim()).attr('title', selectedText.trim());
        var selectedCoaId = $clickedItem.data('coa-id');
        $dropdownMenu.find('input[type="hidden"][data-name="ChartOfAccounts_Id"]').val(selectedCoaId);
        $dropdownLabel.trigger('click');
    });
}
function LoadLineItemFeeTaxesChartOfAccounts() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/LineItemsChartofAccounts", $('[data-name="ConfigurationTabContainer"]'), function () { MakeSubMenuActive($('[data-tab-name="btnLineItemsFeeTaxes"]')) }, true);
}
function LoadLOBCarrierGrouping() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/LobCarrierGrouings", $('[data-name="ConfigurationTabContainer"]'), function () { MakeSubMenuActive($('[data-tab-name="btnLobCarrierGrouping"]')) }, true);
}

function LoadAddOperatingBank(PageNumber, PageSize, SearchValue) {
    //document.title = "Operating Banks";
    var queryString = appCommon.BuildQueryStringParameters({ PageNumber: PageNumber, PageSize: PageSize, SearchValue: SearchValue });
    var Url = "/SystemConfiguration/OperatingBanksIndex?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnAddOperatingBank"]'))
    }, true, null);
}

function LoadUsersList(PageNumber, PageSize, SearchValue) {
    var queryString = appCommon.BuildQueryStringParameters({ PageNumber: PageNumber, PageSize: PageSize, SearchValue: SearchValue });
    var Url = "/SystemConfiguration/UsersList?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnUsers"]'));
        BindUserEvents();
    }, true, null);
}
function LoadLineofBusiness(PageNumber, PageSize, SearchValue) {
    //document.title = "Line of Business";
    var queryString = appCommon.BuildQueryStringParameters({ PageNumber: PageNumber, PageSize: PageSize, SearchValue: SearchValue });
    var Url = "/SystemConfiguration/LineofBusinessIndex?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnLineofBusiness"]'))
    }, true, null);
}
function LoadInstallmentPlan(PageNumber, PageSize, SearchFilters) {
    //var queryString = appCommon.BuildQueryStringParameters({ PageNumber: PageNumber, PageSize: PageSize, SearchValue: SearchFilters });
    var data = {
        PageNumber: PageNumber,
        PageSize: PageSize
    };
    if (SearchFilters && typeof SearchFilters === "object") {
        Object.keys(SearchFilters).forEach(k => {
            data[k] = SearchFilters[k];
        });
    }

    var queryString = appCommon.BuildQueryStringParameters(data);
    var Url = "/SystemConfiguration/InstallmentPlanIndex?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnInstallmentPlan"]'))
    }, true, null);
}
function LoadLineItemsList(PageNumber, PageSize, SearchValue) {
    var queryString = appCommon.BuildQueryStringParameters({ PageNumber: PageNumber, PageSize: PageSize, SearchValue: SearchValue });
    var Url = "/SystemConfiguration/LineItemsList?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnLineItems"]'));
        BindLineItemsEvents();
    }, true, null);
}

function LoadLOBMapping() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/LobMapping", $('[data-name="ConfigurationTabContainer"]'), function () { MakeSubMenuActive($('[data-tab-name="btnLobMapping"]')) }, true);
}
function BindPaginationEvents() {


    $('[data-name="PageSize"]').change(function () {
        var TableName = $(this).attr('data-table-name');
        var PageSize = $(this).val();
        var PageNumber = 1;
        $('[data-name="btnPrevious"]').val(PageNumber);
        if (TableName == "OperatingBanks") {
            LoadAddOperatingBank(PageNumber, PageSize);
        }
        else if (TableName == "LineofBusiness") {
            LoadLineofBusiness(PageNumber, PageSize);
        }
        else if (TableName == "LineItems") {
            LoadLineItemsList(PageNumber, PageSize);
        }
        else if (TableName == "InstallmentPlan") {
            LoadInstallmentPlan(PageNumber, PageSize);
        }
        else if (TableName == "Users") {
            LoadUsersList(PageNumber, PageSize)
        }
        else if (TableName == "CommissionRules") {
            LoadCommissionRules(PageNumber, PageSize)
        }
    });

    $('[data-name="btnPageNumber"]').click(function (e) {
        e.preventDefault();
        var TableName = $(this).attr('data-table-name');
        PageNumber = $(this).data('page');
        value = PageNumber.replace(/''/g, '');
        var values = value.split('+');
        values = values.filter(function (val) {
            return val !== '';
        });
        PageNumber = parseInt(values[0])
        var PageSize = parseInt(values[1]);
        if (TableName == "OperatingBanks") {
            LoadAddOperatingBank(PageNumber, PageSize);
        }
        else if (TableName == "LineofBusiness") {
            LoadLineofBusiness(PageNumber, PageSize);
        }
        else if (TableName == "LineItems") {
            LoadLineItemsList(PageNumber, PageSize);
        }
        else if (TableName == "InstallmentPlan") {
            LoadInstallmentPlan(PageNumber, PageSize);
        }
        else if (TableName == "Users") {
            LoadUsersList(PageNumber, PageSize)
        }
        else if (TableName == "CommissionRules") {
            LoadCommissionRules(PageNumber, PageSize)
        }
    });

    $('[data-name="btnNext"]').click(function () {
        var TableName = $(this).attr('data-table-name');
        var value = $(this).attr('value');
        value = value.replace(/''/g, '');
        var values = value.split('+');
        values = values.filter(function (val) {
            return val !== '';
        });
        var PageSize = parseInt(values[0]);
        var PageNumber = parseInt(values[1]);
        var TotalPages = parseInt(values[2]);
        if (PageNumber < TotalPages) {
            PageNumber++;
        }
        if (PageNumber > TotalPages) {
            PageNumber = TotalPages;
        }
        if (TableName == "OperatingBanks") {
            LoadAddOperatingBank(PageNumber, PageSize);
        }
        else if (TableName == "LineofBusiness") {
            LoadLineofBusiness(PageNumber, PageSize);
        }
        else if (TableName == "LineItems") {
            LoadLineItemsList(PageNumber, PageSize);
        }
        else if (TableName == "InstallmentPlan") {
            LoadInstallmentPlan(PageNumber, PageSize);
        }
        else if (TableName == "Users") {
            LoadUsersList(PageNumber, PageSize)
        }
        else if (TableName == "CommissionRules") {
            LoadCommissionRules(PageNumber, PageSize)
        }
        UpdatePaginationButtons(PageNumber, TotalPages);
    });

    $('[data-name="btnPrevious"]').click(function () {
        var TableName = $(this).attr('data-table-name');
        var value = $(this).attr('value');
        value = value.replace(/''/g, '');
        var values = value.split('+');
        values = values.filter(function (val) {
            return val !== '';
        });
        var PageSize = parseInt(values[0]);
        var PageNumber = parseInt(values[1]);
        var TotalPages = parseInt(values[2]);
        if (PageNumber > 1) {
            PageNumber--;
        }
        if (PageNumber < 1) {
            PageNumber = 1;
        }
        if (TableName == "OperatingBanks") {
            LoadAddOperatingBank(PageNumber, PageSize);
        }
        else if (TableName == "LineofBusiness") {
            LoadLineofBusiness(PageNumber, PageSize);
        }
        else if (TableName == "LineItems") {
            LoadLineItemsList(PageNumber, PageSize);
        }
        else if (TableName == "InstallmentPlan") {
            LoadInstallmentPlan(PageNumber, PageSize);
        }
        UpdatePaginationButtons(PageNumber, TotalPages);
    });

    BindClickEventsForOperatingBank();
    BindClickEventsLineofBusiness();
    BindClickEventsInstallmentPlan();
    BindClickEventsCommissionRules();
}
function BindClickEventsForOperatingBank() {


    _AppMainPanel.TableHeaderSearchandButtonPanel(
        "Operating Banks",
        "Search with operating bank...",
        '<i class="ri-add-circle-line"></i> New',
        {
            header: '[data-name="lblPageName"]',
            search: '[data-name="txtSearch"]',
            button: '[data-name="btnAddBank"]'
        }
    );

    $('[data-name="btnAddBank"]').off('click');
    $('[data-name="btnAddBank"]').click(function () {

        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/OperatingBanks/", function (response) {
            appCommon.OffcanvasGeneralFunction("Add Operating Bank", response);
            BindCOADropDownEvents();
        }, true);
    });

    $('[data-name="btnEditOperatingBank"]').off('click');
    $('[data-name="btnEditOperatingBank"]').click(function () {
        var OperatingBank_Id = $(this).attr('data-operatingbank-id');
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/OperatingBanks?OperatingBank_Id=" + OperatingBank_Id, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit Operating Bank", response);
            BindCOADropDownEvents();
        }, true);
    });

    $(document).ready(function () {
        $('[data-name="signatureMode"]').trigger('change');
    });
    $('[data-name="signatureMode"]').change(function () {
        var type = $(this).find(':selected').data('type');

        if (type === "text") {
            $('[data-name="textSignature"]').show();
            $('[data-name="imageSignature"]').hide();
            $('[data-name="SignerType"]').val("text");
        }
        else if (type === "image") {
            $('[data-name="textSignature"]').hide();
            $('[data-name="imageSignature"]').show();
            $('[data-name="SignerType"]').val("image");
        }
    });


    $('[data-name="signatureImage"]').on('change', function () {
        var file = this.files[0];
        if (!file)
            return;
        switch (file.type) {
            case "image/jpeg":
            case "jpg":
                $('[data-name="SignerType"]').val("jpg");
                break;

            case "image/png":
                $('[data-name="SignerType"]').val("png");
                break;

            case "image/gif":
                $('[data-name="SignerType"]').val("gif");
                break;

            default:
                appCommon.ShowMessageModal("Only JPG, PNG and GIF are allowed.");
                $(this).val('');
                return;
        }
        var reader = new FileReader();
        reader.onload = function (e) {
            var base64 = e.target.result.split(',')[1];
            $('[data-name="SignerValue"]').val(base64);
        };
        reader.readAsDataURL(file);
    });

    $('[data-name="btnSaveOperatingBank"]').off('click').on('click', function () {
        var service_Id = $('[data-service-id]').val();
        if (service_Id != null && service_Id != "0") {
            var chartOfAccounts_Id = $('[data-name="ChartOfAccounts_Id"]').val();
            var AccountNumber = $('[data-name="AccountNumber"]').val();
            //if (AccountNumber.length > 22) {
            //    appCommon.ShowMessageModal("Account number should not be greater than 22 characters.");
            //    return;
            //}
            var RoutingNumber = $('[data-name="RoutingNumber"]').val();
            //if (RoutingNumber.length != 9) {
            //    appCommon.ShowMessageModal("RoutingNumber number must be 9 characters.");
            //    return;
            //}            
            var Check_BankName = $('[data-name="Check_BankName"]').val();
            var cityName = $('[data-name="cityName"]').val();
            var zipCode = $('[data-name="zipCode"]').val();
            var signatureType = $('[data-name="signatureMode"] option:selected').attr('data-type');
            if (signatureType === "text") {
                $('[data-name="SignerType"]').val("text");
                $('[data-name="SignerValue"]').val($('[data-name="textSignerValue"]').val()
                );
            }
            var SignerValue = $('[data-name="SignerValue"]').val();

            if (service_Id == MasterLookup._Services.Intuit) {
                if (!chartOfAccounts_Id || chartOfAccounts_Id.trim() === '') {
                    appCommon.ShowMessageModal("Please select a Chart of Account for the operating bank.", "Alert");
                    return;
                }
            }
        }

        var OperatingBankForm = $('[data-name="FormOperatingBank"]');
        if (appCommon.IsValidForm(OperatingBankForm)) {
            appCommon.SubmitForm(OperatingBankForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal('Operating Bank has been added/updated successfully.', 'Success');
                        LoadAddOperatingBank(1, 10);
                        $('[data-name="btnClose"]').trigger('click');
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="btnClose"]').trigger('click');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/OperatingBanks/", true, true);
        }
    });

    $('[data-name="btnDeleteOperatingBank"]').off('click');
    $('[data-name="btnDeleteOperatingBank"]').click(function () {
        var OperatingBank_Id = $(this).attr('data-operatingbank-id');
        var queryString = appCommon.BuildQueryStringParameters({ OperatingBank_Id: OperatingBank_Id });
        var title = "Confirm Operating Bank Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteOperatingBank?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Operating Bank deleted successfully.', title);
                    LoadAddOperatingBank(1, 10);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete operating bank. Please try again later");
                }
            }, true, true)

        }, function () {

        });
    });
    $('[data-name="rdnDefaultPaymentOperatingBank"]').off('change');
    $('[data-name="rdnDefaultPaymentOperatingBank"]').on('change', function () {
        var previousChecked = $('[data-name="rdnDefaultPaymentOperatingBank"][data-default="true"]');
        if ($(this).is(':checked')) {
            var OperatingBank_Id = $(this).attr('data-operatingbank-id');
            var queryString = appCommon.BuildQueryStringParameters({ OperatingBank_Id: OperatingBank_Id });
            var title = "Confirm Operating Bank"
            var message = "Are you sure you want to Mark it as Default Operating Bank?"
            appCommon.ShowConfirmationModal(message, title, function () {
                appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DefaultOperatingBank?" + queryString, null, function (response) {
                    if (response.Status) {
                        appCommon.ShowMessageModal('Marked it as Default operating Bank successfully.', title);
                        LoadAddOperatingBank(1, 10);
                    }
                    else {
                        appCommon.ShowMessageModal("Unable to marked it as Default operating Bank. Please try again later");
                        previousChecked.prop('checked', true);
                    }
                }, true, true)

            }, function () {
                previousChecked.prop('checked', true);
            });
        }
    });

    $('[data-name="btnSearch"]').off('click');
    $('[data-name="btnSearch"]').on('click', function () {
        SearchValue = $('[data-name="txtSearch"]').val().trim();
        LoadAddOperatingBank(1, 10, SearchValue);
    });
}
function BindUserEvents() {
    appCommon.InitializePanelOrFormValidations($('[data-name="frmUsers"]'));
    $('[data-name="btnAddUsers"]').off('click');
    $('[data-name="btnAddUsers"]').click(function () {
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/User/", function (response) {
            appCommon.OffcanvasGeneralFunction("Add User", response);
            BindUserEvents();

        }, true);
    });
    $('[data-name="btnEditUser"]').off('click');
    $('[data-name="btnEditUser"]').click(function () {
        var User_Id = $(this).attr('data-User-id');
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/User?User_Id=" + User_Id, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit User", response);
            BindUserEvents();
        }, true);
    });
    $('[data-name="btnSaveUsers"]').off('click').on('click', function () {
        var UsersForm = $('[data-name="frmUsers"]');
        const selectedClients = $('[data-name="client-options"]:checked').length;
        if (selectedClients === 0) {
            appCommon.ShowMessageModal("Please select at least one Client.");
            return;
        }
        var roleId = $('[data-name="Role_Id"]').val();
        var email = $('[name="Email"]').val().toLowerCase();
        const emailDomains = ["@expertinsured.com", "@selectsys.com"];
        if (roleId == 8) {
            if (emailDomains.every(domain => !email.includes(domain))) {
                appCommon.ShowMessageModal("For SuperAdmin users, only '@expertinsured.com' or '@selectsys.com' email domains are accepted.");
                return false;
            }
        }
        if (appCommon.IsValidForm(UsersForm)) {
            appCommon.SubmitForm(UsersForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal('User has been added/updated successfully.', 'Success');
                        LoadUsersList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                        $('[data-name="btnClose"]').trigger('click');
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="btnClose"]').trigger('click');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/User/", true, true);
        }
    });
    $('[data-name="btnDeleteUser"]').off('click');
    $('[data-name="btnDeleteUser"]').click(function () {

        var User_id = $(this).attr('data-User-id');
        var queryString = appCommon.BuildQueryStringParameters({ User_Id: User_id });
        var title = "Confirm on User Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteUser?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('User deleted successfully.', title);
                    LoadUsersList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete User. Please try again later");
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="btnUsersSearch"]').off('click');
    $('[data-name="btnUsersSearch"]').on('click', function () {
        SearchValue = $('[data-name="txtUsersSearch"]').val().trim();
        LoadUsersList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize, SearchValue);
    });
    $('[data-name="client-options"]').on("change", function () {
        var selected = [];
        $('input[data-name="client-options"]:checked').each(function () {
            selected.push($(this).data("client-id"));
        });
        $('input[name="ClientIdList"]').val(selected.join(","));
    });
    $('[data-name="Is_APIUser"]').on("change", function () {
        const isChecked = $(this).prop("checked");
        $('input[name="Is_APIUser"][type="hidden"]').val(isChecked ? "true" : "false");
        const Role = $('[data-name="Role_Id"]');
        if (!isChecked) {
            Role.attr('data-val', 'true');
        } else {
            Role.removeAttr('data-val');
            Role.removeAttr('data-val-touched');
            Role.removeClass('input-validation-error');
            $('p[data-valmsg-for="Role_Id"]').remove();
        }
    });
    $('.custom-multi-selection').each(function () {
        const container = $(this);
        const chips = container.find('.selected-chips');
        const defaultText = container.find('.default-text');
        const checkboxes = container.find('.dropdown-item input[type="checkbox"]');

        function updateChips() {
            const checked = checkboxes.filter(':checked');
            chips.empty();

            if (checked.length === 0) {
                defaultText.show();
            }
            else {
                defaultText.hide();
                checked.each(function () {
                    const value = $(this).val();
                    chips.append(
                        '<div class="chip d-flex align-items-center gap-1" data-value="' + value + '">' +
                        '<span>' + value + '</span>' +
                        '<span class="remove-chip" style="cursor:pointer;">&times;</span>' +
                        '</div>'
                    );
                });
            }
        }

        checkboxes.on('change', updateChips);
        updateChips();
    });
    $('.remove-chip').on('click', function (e) {
        e.stopPropagation();
        const chip = $(this).closest('.chip');
        const value = chip.data('value');
        const container = chip.closest('.custom-multi-selection');

        if (value === 'all') {
            container.find('.dropdown-item input[type="checkbox"]').prop('checked', false).trigger('change');
        } else {
            container.find('input[type="checkbox"][value="' + value + '"]').prop('checked', false).trigger('change');
        }
    });
}
function BindClickEventsLineofBusiness() {

    //_AppMainPanel.LOBTableHeaderSearchandButtonPanel("Line of Business", "Search with line of business...", '<i class="ri-add-circle-line"></i> New');

    _AppMainPanel.TableHeaderSearchandButtonPanel(
        "Line of Business",
        "Search with line of business...",
        '<i class="ri-add-circle-line"></i> New',
        {
            header: '[data-name="lbllobPageName"]',
            search: '[data-name="txtlobSearch"]',
            button: '[data-name="btnAddLineofBusiness"]'
        }
    );

    $('[data-name="btnAddLineofBusiness"]').off('click');
    $('[data-name="btnAddLineofBusiness"]').click(function () {
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineofBusiness/", function (response) {
            appCommon.OffcanvasGeneralFunction("Add Line of Business", response);
        }, true);
    });

    $('[data-name="btnEditLineofBusiness"]').off('click');
    $('[data-name="btnEditLineofBusiness"]').click(function () {
        var LOB_Id = $(this).attr('data-lineofbusiness-id');
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineofBusiness?LOB_Id=" + LOB_Id, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit Line of Business", response);
        }, true);
    });

    $('[data-name="btnSaveLineofBusiness"]').off('click').on('click', function () {
        var LineofBusinessForm = $('[data-name="FormLineofBusiness"]');
        if (appCommon.IsValidForm(LineofBusinessForm)) {
            appCommon.SubmitForm(LineofBusinessForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal('Line of Business has been added/updated successfully.', 'Success');
                        LoadLineofBusiness(1, 10);
                        $('[data-name="btnClose"]').trigger('click');
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="btnClose"]').trigger('click');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/LineofBusiness/", true, true);
        }
    });

    $('[data-name="btnDeleteLineofBusiness"]').off('click');
    $('[data-name="btnDeleteLineofBusiness"]').click(function () {
        var LOB_Id = $(this).attr('data-lineofbusiness-id');
        var queryString = appCommon.BuildQueryStringParameters({ LOB_Id: LOB_Id });
        var title = "Confirm Line of Business Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteLineofBusiness?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Line of Business deleted successfully.', title);
                    LoadLineofBusiness(1, 10);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete Line of Business. Please try again later");
                }
            }, true, true)

        }, function () {

        });
    });

    $('[data-name="btnlobSearch"]').off('click');
    $('[data-name="btnlobSearch"]').on('click', function () {
        SearchValue = $('[data-name="txtlobSearch"]').val().trim();
        LoadLineofBusiness(1, 10, SearchValue);
    });
}
function BindClickEventsInstallmentPlan() {

    _AppMainPanel.TableHeaderSearchandButtonPanel(
        "Installment Plan",
        "Search with installment plan...",
        '<i class="ri-add-circle-line"></i> New',
        {
            header: '[data-name="lblInstallmentPlanPageName"]',
            search: '[data-name="txtInstallmentPlanSearch"]',
            button: '[data-name="btnAddInstallmentPlan"]'
        }
    );

    $('[data-name="btnAddInstallmentPlan"]').off('click');
    $('[data-name="btnAddInstallmentPlan"]').click(function () {
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/InstallmentPlan/", function (response) {
            //         var isActiveHtml = `<div class="activeSwitch form-check form-switch">
            //	<input class="form-check-input unchecked" data-name="IsPlanActive" name="Status" data-val="true" type="checkbox" id="IsActive" title="IsActive" value="true" checked>
            //</div>`;
            appCommon.GenericModalFunction("Add Installment Plan", response /*, isActiveHtml*/);
            appCommon.InitializePanelOrFormValidations('[data-name="GenericModal"]');
            $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
            $('[data-name="btnClose"]').trigger('click');
        }, true);


    });

    $('[data-name="btnEditInstallmentPlan"]').off('click');
    $('[data-name="btnEditInstallmentPlan"]').click(function () {

        var InstallmentPlan_Id = $(this).attr('data-installmentplan-id');
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/InstallmentPlan?InstallmentPlan_Id=" + InstallmentPlan_Id, function (response) {
            if (response != null && response != undefined) {
                //             var isActiveHtml = `<div class="activeSwitch form-check form-switch">
                //	<input class="form-check-input unchecked" data-name="IsPlanActive" name="Status" data-val="true" type="checkbox" id="IsActive" title="IsActive" value="true" checked>
                //</div>`;
                appCommon.GenericModalFunction("Edit Installment Plan", response/*, isActiveHtml*/);
                $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");

                appCommon.InitializePanelOrFormValidations('[data-name="GenericModal"]');
                BindClickEventsInstallmentPlan();
            }

        }, true, true);
    });

    $('[data-name="btnSaveInstallmentPlan"]').off('click').on('click', function () {
        var InstallmentPlanForm = $('[data-name="frmInstallmentPlan"]');
        //var InstallmentPlanForm = $('[data-name="GenericModal"]'); 
        //let IsPlanActive = $('[data-name="IsPlanActive"]').is(':checked');
        //var queryString = appCommon.BuildQueryStringParameters({ IsActive: IsPlanActive });
        if (appCommon.IsValidForm(InstallmentPlanForm)) {
            appCommon.SubmitForm(InstallmentPlanForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal('Payment Plan has been added/updated successfully.', 'Success');
                        LoadInstallmentPlan(1, 50);
                        $('[data-name="GenericModal"]').modal('hide');
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="GenericModal"]').modal('hide');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/InstallmentPlan/", true, true);
        }
    });

    $('[data-name="btnDeleteInstallmentPlan"]').off('click');
    $('[data-name="btnDeleteInstallmentPlan"]').click(function () {
        var InstallmentPlan_Id = $(this).attr('data-installmentplan-id');
        var queryString = appCommon.BuildQueryStringParameters({ InstallmentPlan_Id: InstallmentPlan_Id });
        var title = "Confirm Installment Plan Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteInstallmentPlan?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Installment Plan deleted successfully.', title);
                    LoadInstallmentPlan(1, 50);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete Installment Plan. Please try again later");
                }
            }, true, true)

        }, function () {

        });
    });

    $('[data-name="btnInstallmentPlanSearch"]').off('click');
    $('[data-name="btnInstallmentPlanSearch"]').on('click', function () {
        let searchData = {
            Name: $("[data-name='txtInstallmentPlanName']").val(),
            Code: $("[data-name='txtInstallmentPlanCode']").val(),
            NoOfPayments: $("[data-name='txtInstallmentPlanNumberOfPayments']").val(),
            Carrier: $("[data-name='txtInstallmentPlanCarrier']").val()
        };
        LoadInstallmentPlan(1, 50, searchData);
    });
}
function BindLineItemsEvents() {
    $('[data-name="btnAddLineItem"]').off('click');
    $('[data-name="btnAddLineItem"]').click(function () {
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineItem/", function (response) {
            appCommon.OffcanvasGeneralFunction("Add Line Item", response);
            BindLineItemsEvents();
        }, true);
    });
    $('[data-name="btnEditLineItem"]').off('click');
    $('[data-name="btnEditLineItem"]').click(function () {
        var lineitem_Id = $(this).attr('data-lineitem-id');
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineItem?LineItem_Id=" + lineitem_Id, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit Line Item", response);
            BindLineItemsEvents();
        }, true);
    });
    $('[data-name="btnSaveLineItem"]').off('click').on('click', function () {
        var LineItemForm = $('[data-name="frmLineItem"]');
        if (appCommon.IsValidForm(LineItemForm)) {
            appCommon.SubmitForm(LineItemForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal('Line item has been added/updated successfully.', 'Success');
                        LoadLineItemsList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                        $('[data-name="btnClose"]').trigger('click');
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="btnClose"]').trigger('click');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/LineItem/", true, true);
        }
    });
    $('[data-name="btnDeleteLineItem"]').off('click');
    $('[data-name="btnDeleteLineItem"]').click(function () {
        var lineitem_Id = $(this).attr('data-lineitem-id');
        var queryString = appCommon.BuildQueryStringParameters({ LineItem_Id: lineitem_Id });
        var title = "Confirm Line item Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteLineItem?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Line item deleted successfully.', title);
                    LoadLineItemsList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete Line item. Please try again later");
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="btnLineItemSearch"]').off('click');
    $('[data-name="btnLineItemSearch"]').on('click', function () {
        SearchValue = $('[data-name="txtLineItemSearch"]').val().trim();
        LoadLineItemsList(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize, SearchValue);
    });
}
function BindLineItemMappingEvents() {
    $('[data-name="btnSaveLineItemsMapping"]').off('click');
    $('[data-name="btnSaveLineItemsMapping"]').on('click', function () {
        // Validate Directions
        const selectedDirections = $('[data-name="direction-options"]:checked').length;
        if (selectedDirections === 0) {
            appCommon.ShowMessageModal("Please select at least one Direction.");
            return;
        }

        // Validate BillTypes
        const selectedBillTypes = $('[data-name="billtype-options"]:checked').length;
        if (selectedBillTypes === 0) {
            appCommon.ShowMessageModal("Please select at least one Bill Type.");
            return;
        }

        var LineItemMappingForm = $('[data-name="frmLineItemsClients"]');
        if (appCommon.IsValidForm(LineItemMappingForm)) {
            appCommon.SubmitForm(LineItemMappingForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status) {
                        appCommon.ShowMessageModal(response.Message, 'Success');
                        $('[data-name="btnClose"]').trigger('click');
                        LoadLineItemMappingList();
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="btnClose"]').trigger('click');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/LineItemMapping/", true, true);
        }
    });
    $('[data-name="btnDeleteLineItemMapping"]').off('click');
    $('[data-name="btnDeleteLineItemMapping"]').click(function () {
        var lineitemsclient_id = $(this).attr('data-lineitems-client-id');
        var queryString = appCommon.BuildQueryStringParameters({ LineItems_Clients_Id: lineitemsclient_id });
        var title = "Confirm line item mapping deletion";
        var message = "Are you sure you want to delete?";
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteLineItemMapping?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Line item mapping deleted successfully.', "Success");
                    LoadLineItemMappingList();
                }
                else {
                    appCommon.ShowMessageModal(response.Message);
                }
            }, true, true)
        }, function () {
        });
    });
    $('[data-name="btnEditLineItemMapping"]').off('click');
    $('[data-name="btnEditLineItemMapping"]').click(function () {
        var lineitemsclient_id = $(this).attr('data-lineitems-client-id');
        var queryString = appCommon.BuildQueryStringParameters({ LineItems_Clients_Id: lineitemsclient_id });
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineItemMapping?" + queryString, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit Line-Item Mapping", response); BindLineItemMappingEvents(); EnableLineItemSerachableDropdown();
        }, true);
    });
    $('[data-name="btnAddLineItemMapping"]').off('click');
    $('[data-name="btnAddLineItemMapping"]').on('click', function () {
        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/LineItemMapping", function (response) {
            appCommon.OffcanvasGeneralFunction("Add Line-Item Mapping", response); BindLineItemMappingEvents(); EnableLineItemSerachableDropdown(); appCommon.InitializePanelOrFormValidations($('[data-name="frmLineItemsClients"]'));
        }, true);
    });
    $('[data-name="btnSearchLineItemMapping"]').off('click');
    $('[data-name="btnSearchLineItemMapping"]').click(function () {
        var direction_id = $('[data-name="Direction_Id"]').val();
        var billingtype_Id = $('[data-name="BillingType_Id"]').val();
        LoadLineItemMappingList(direction_id, billingtype_Id);
    });
}
function LoadLineItemMappingList(Directions_Id, BillingType_Id) {
    if (typeof Directions_Id == "undefined" || Directions_Id == "" || Directions_Id == '' || Directions_Id == null) {
        Directions_Id = "";
    }
    if (typeof BillingType_Id == "undefined" || BillingType_Id == "" || BillingType_Id == '' || BillingType_Id == null) {
        BillingType_Id = "";
    }
    var queryString = appCommon.BuildQueryStringParameters({ Directions_Id: Directions_Id, BillingType_Id: BillingType_Id });
    appCommon.RenderHTMLPanel("/SystemConfiguration/LineItemMappingList?" + queryString, $('[data-name="divLineItemMappingList"]'), function () {
        BindLineItemMappingEvents();
    }, true);
}
function LoadlineItemMappingIndex() {
    appCommon.RenderHTMLPanel("/SystemConfiguration/LineItemMappingIndex", $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnLineItemMapping"]'));
        LoadLineItemMappingList();
    }, true);
}
function UpdatePaginationButtons(PageNumber, TotalPages) {
    if (PageNumber <= 1) {
        $('[data-name="btnPrevious"]').attr('disabled', true);
    } else {
        $('[data-name="btnPrevious"]').attr('disabled', false);
    }
    if (PageNumber >= TotalPages) {
        $('[data-name="btnNext"]').attr('disabled', true);
    } else {
        $('[data-name="btnNext"]').attr('disabled', false);
    }
}
function EnableLineItemSerachableDropdown() {
    document.addEventListener('shown.bs.offcanvas', function (event) {
        const $form = $('form[data-name="frmLineItemsClients"]');
        $form.find('select[data-name="LineItem_Id"]').select2({
            placeholder: "",
            allowClear: true,
            minimumResultsForSearch: 0,
            width: '100%',
            dropdownParent: $form
        });
    });
}

$('[data-name="GenericModal"]').on('show.bs.modal', function () {
    $('.animate-modal').show();
    setTimeout(() => {
        $('.animate-modal').hide();
    }, 1000)
})

function toggleIsExcluded() {
    var agencyBillId = MasterLookup._BillingTypes.Agency;
    var insuredBillId = MasterLookup._BillingTypes.Insured;
    var directBillId = MasterLookup._BillingTypes.Direct;

    var selectedIds = [];

    $("input[name='BillTypeIds']:checked").each(function () {
        selectedIds.push($(this).data("billtype-id").toString());
    });

    // Enable only when Direct Bill is selected
    if ($.inArray(directBillId.toString(), selectedIds) !== -1) {

        $("input[data-name='Is_Excluded']")
            .prop("disabled", false);

    } else {
        var hiddenVal = $("input[name='Is_Excluded'][type='hidden']").val();
        var isExcludedTrue = hiddenVal && hiddenVal.toLowerCase() === "true";

        $("input[data-name='Is_Excluded']").prop("disabled", !allowIsExcludeLineItemMapping);

        if (!isExcludedTrue) {
            $("input[data-name='Is_Excluded']").prop("checked", false);
            $("input[name='Is_Excluded']").val("False");
        }
    }
}

$(document).on("change", "input[name='BillTypeIds']", function () {
    toggleIsExcluded();
});


$("input[data-name='Is_Excluded']").change(function () {
    $("input[name='Is_Excluded']").val($(this).is(":checked"));
});


// Commission Rules Start
function LoadCommissionRules(PageNumber, PageSize, SearchFilters) {
    var data = {
        PageNumber: PageNumber,
        PageSize: PageSize
    };
    if (SearchFilters && typeof SearchFilters === "object") {
        Object.keys(SearchFilters).forEach(k => {
            data[k] = SearchFilters[k];
        });
    }

    var queryString = appCommon.BuildQueryStringParameters(data);
    var Url = "/SystemConfiguration/CommissionRulesList?" + queryString;
    appCommon.RenderHTMLPanel(Url, $('[data-name="ConfigurationTabContainer"]'), function () {
        MakeSubMenuActive($('[data-tab-name="btnCommissionRules"]'))
    }, true, null);
}
function BindClickEventsCommissionRules() {
    appCommon.InitializePanelOrFormValidations($('[data-name="frmCommissionRules"]'));
    _AppMainPanel.TableHeaderSearchandButtonPanel(
        "Commission Rules Details",
        "Search with CommissionRules...",
        '<i class="ri-add-circle-line"></i> New',
        {
            header: '[data-name="lblCommissionRulesPageName"]',
            search: '[data-name="txtCommissionRulesSearch"]',
            button: '[data-name="btnAddCommissionRules"]'
        }
    );

    $('[data-name="btnAddCommissionRules"]').off('click');
    $('[data-name="btnAddCommissionRules"]').click(function () {

        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/GetCommissionRule/", function (response) {
            appCommon.OffcanvasGeneralFunction("Add Commission Rule", response);
        }, true);

    });

    $('[data-name="btnEditCommissionRules"]').off('click');
    $('[data-name="btnEditCommissionRules"]').click(function () {

        var CommissionRules_Id = $(this).attr('data-CommissionRules-id');

        appCommon.ExecuteAjaxGetMethod("/SystemConfiguration/GetCommissionRule?CommissionRule_Id=" + CommissionRules_Id, function (response) {
            appCommon.OffcanvasGeneralFunction("Edit Commission Rule", response);
        }, true);
    });

    $('[data-name="btnSaveCommissionRules"]').off('click').on('click', function () {
        var CommissionRulesForm = $('[data-name="frmCommissionRules"]');

        if ($('input[data-name="submissionType-option"]').length > 0) {
            if ($('input[data-name="submissionType-option"]:checked').length === 0) {
                appCommon.ShowMessageModal("Please select at least one Submission Type.", "Alert");
                return;
            }
        }

        if (appCommon.IsValidForm(CommissionRulesForm)) {
            appCommon.SubmitForm(CommissionRulesForm, function (response) {
                if (response != null && typeof response != 'undefined') {
                    $('[data-name="btnClose"]').trigger('click');
                    if (response.Status) {
                        appCommon.ShowMessageModal(response.Message, 'Success');
                        LoadCommissionRules(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                    }
                    else {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message);
                        } else {
                            appCommon.ShowMessageModal("Unable to process the request.");
                        }
                        $('[data-name="GenericModal"]').modal('hide');
                    }
                } else {
                    appCommon.ShowMessageModal("Unexpected response received.");
                }
            }, "/SystemConfiguration/SaveCommissionRules/", true, true);
        }
    });

    $('[data-name="btnDeleteCommissionRules"]').off('click');
    $('[data-name="btnDeleteCommissionRules"]').click(function () {
        var CommissionRules_Id = $(this).attr('data-CommissionRules-id');
        var queryString = appCommon.BuildQueryStringParameters({ CommissionRule_Id: CommissionRules_Id });
        var title = "Confirm CommissionRules Deletion"
        var message = "Are you sure you want to delete?"
        appCommon.ShowConfirmationModal(message, title, function () {
            appCommon.ExecuteAjaxPostMethod("/SystemConfiguration/DeleteCommissionRule?" + queryString, null, function (response) {
                if (response.Status) {
                    appCommon.ShowMessageModal('Commission Rule deleted successfully.', title);
                    LoadCommissionRules(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize);
                }
                else {
                    appCommon.ShowMessageModal("Unable to delete CommissionRules. Please try again later");
                }
            }, true, true)

        }, function () {

        });
    });

    $('[data-name="btnCommissionRulesSearch"]').off('click');
    $('[data-name="btnCommissionRulesSearch"]').on('click', function () {
        let searchData = {
            SearchValue: $("[data-name='txtCommissionRulesName']").val() || ""
        };
        LoadCommissionRules(MasterLookup._DefaultPageNumber, MasterLookup._DefaultPageSize, searchData);
    });
}
// Commission Rules End

