$(function () {

    $('[data-name="PayeeName"]').each(function () {
        EnableNameCompletely(this);
    });
});
function EnableNameCompletely(ctrl) {
    if ($(ctrl).attr("type") != "text")
        return;

    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },

        appendTo: '[data-name="GenericModal"]',
        classes: {
            "ui-autocomplete": "highlight"
        },

        source: function (request, response) {
            var selectedEntity = $(ctrl).closest('[data-name="EditEntityNameSection"]').find('[data-name="PayeeType_Id"]').val();
            var Name = $(ctrl).val();
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
            $(ctrl).closest('[data-name="EditEntityNameSection"]').find('[data-name="PayeeName"]').val(ui.item.value);
            $(ctrl).parent().find('[data-name="Payee_Id"]').val(ui.item.EntityID);
        },

    });
}

$('[data-name="btnEditInvoiceDetail"]').off('click').on('click', function () {
    var Id = $(this).attr('data-invoicedetail-id');  // use correct attribute
    var EntityId = $(this).attr('data-entity-id');
    var EntitytypeId = $(this).attr('data-entitytype-id');
    var InvoiceId = $(this).attr('data-invoice-id');
    
    appCommon.ExecuteAjaxGetMethod("/AccountsPayable/EditEntityDetails?InvoiceId=" + InvoiceId, function (response) {
        if (response) {
            appCommon.GenericModalFunction("Edit Payee Entity / Name", response);

            var modal = $('[data-name="GenericModal"]');

            // Set hidden field values AFTER modal content loads
            modal.find('[data-name="Id"]').val(Id);         
            $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1140px");
            appCommon.InitializePanelOrFormValidations('[data-name="GenericModal"]');
            
        }
    }, true, true);
});

$(document).off('change', '[data-name="PayeeType_Id"]');
$(document).on('change', '[data-name="PayeeType_Id"]', function () {
    var entityTypeId = $(this).val();
    var invoiceId = $('[data-name="Invoice_Id"]').val();

    if (entityTypeId !== "") {
        appCommon.ExecuteAjaxGetMethod(
            "/AccountsPayable/GetEntityName?InvoiceId=" + invoiceId + "&EntityType_Id=" + entityTypeId,
            function (response) {
                if (response.Status && response.Data) {
                    $('[data-name="PayeeName"]').val(response.Data.EntityName);
                    $('[data-name="Payee_Id"]').val(response.Data.Entity_Id);
                    $('[data-name="PayorType_Id"]').val(response.Data.PayorType_Id);
                }
                else {
                    $('[data-name="PayeeName"]').val('');
                    $('[data-name="Payee_Id"]').val('');
                    $('[data-name="PayorType_Id"]').val('');
                    if (response.Message) {
                        appCommon.ShowMessageModal(response.Message);
                    }
                }
            },
            true,
            true
        );
    } else {
        $('[data-name="PayeeName"]').val('');
        $('[data-name="Payee_Id"]').val('');
    }
});


$('[data-name="btnSaveEntity"]').off('click').on('click', function () {
    var modal = $('[data-name="GenericModal"]');
    var EntityDetailsForm = modal.find('[data-name="frmEditEntityNameSearch"]');    
    if (appCommon.IsValidForm(EntityDetailsForm)) {        
        appCommon.SubmitForm(EntityDetailsForm, function (response) {
            if (response != null && typeof response != 'undefined') {
                if (response.Status) {
                    appCommon.ShowMessageModal('Payee Entity / Name updated successfully.', 'Success');
                    $('[data-name="GenericModal"]').modal('hide');
                    $(document).off('click', '[data-name="ModalMessageOk"], [data-name="ModalMessageCancel"]')
                        .on('click', '[data-name="ModalMessageOk"], [data-name="ModalMessageCancel"]', function () {
                            LoadARGridWithFilters()
                        });
                    $('[data-name="GenericModal"]').modal('hide');
                    LoadAPWithFilters();
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
        }, "/AccountsPayable/UpdateEntityDetails/", true, true);
    }
});

function LoadAPWithFilters() {
    var APContainer = $('[data-name="divAccountsPayableGrid"]');
    appCommon.ShowPartialPanelLoader(APContainer);
    appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
        if (response != undefined && response != null) {
            $(APContainer).html(response);
            PartialInvoiceDetails = [];
            BindAPSearchPanelEvents();
            BindARAPCommonEvents();
        }
    }, "/Search/SearchAP", true, false);
}