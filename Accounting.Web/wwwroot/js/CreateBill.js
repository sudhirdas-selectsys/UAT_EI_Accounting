

//function confirmBillCreationAndPayment(btn) {
//    debugger
//    if (!btn) return;
//    var $btn = $(btn);
//    let Report_Id = $btn.data("reportid");
//    let FileType_Id = $btn.data("file-type-id");


//    var totalAppliedAmount = getTotalAppliedAmount(entityId);
//    let SelectedInvoices = [];
//    $('.invoice-check:checked').each(function () {
//        let $row = $(this).closest('tr');

//        var amount = parseFloat($row.find('.amount-text').val()) || 0;  // Amount is Textbox value
//        var apAmount = parseFloat($row.find('[name="invoice-APAmount"]').text().replace(/[$,]/g, '')) || 0;
//        var amountPaid = parseFloat($row.find('[name="invoice-AmountPaid"]').text().replace(/[$,]/g, '')) || 0;
//        var remainingBalance = apAmount - amountPaid;

//        var invoiceAppliedAmount =  parseFloat($row.find('.amount-text').val()) || 0;

//        SelectedInvoices.push({
//            Invoice_Id: $(this).data('invoice-id'),
//            LineItem_Id: $(this).data('lineitem-id'),
//            InvoiceLineItem_Id: $(this).data('invoiceLineitemId'),
//            AppliedAmount: invoiceAppliedAmount,
//            Amount: amount,
//            RemainingBalance: remainingBalance
//        });
//    });
//        var $btn = $(btn); // convert to jQuery object
//        var entityId = $btn.data("entity_id");
//        var entityTypeId = $btn.data("entitytype_id");
//       // var amountapplied = parseFloat($btn.data("amount")) || 0; // applied Amount
//        var due = parseFloat($btn.data("due")) || 0;
//    var BillDetail = { EntityType_Id: entityTypeId, Entity_Id: entityId, Due: due, AmountApplied: totalAppliedAmount };
//        if (SelectedInvoices.length === 0) {
//            appCommon.ShowMessageModal("Please select at least one invoice.");
//            return;
//        }

//        appCommon.ShowConfirmationModal("Do you want to Create Bill?", "Alert", function () {
//            appCommon.ExecuteAjaxPostMethod(
//                "/MakePayments/ConfirmBillCreation",
//                {
//                    Report_Id: Report_Id,
//                    FileType_Id: FileType_Id,
//                    SelectedInvoices: SelectedInvoices,
//                    BillDetail: BillDetail
//                },
//                function (response) {
//                    if (response?.Status) {
//                        appCommon.ShowMessageModal("Bill Created Successfully.", "Success");
//                        PaymentDetails($(this));
//                    } else {
//                        appCommon.ShowMessageModal(response?.Message || "Unable to process request.");
//                    }
//                }.bind(this),
//                true
//            );
//        }.bind(this), function () {

//        });
//    }
function confirmBillCreationAndPayment(btn) {
    if (!btn) return;

    var $btn = $(btn);

    var entityId = $btn.attr("data-entity_id");
    var entityTypeId = $btn.attr("data-entitytype_id");
    var due = parseFloat($btn.attr("data-due")) || 0;
    let Report_Id = $btn.attr("data-reportid") || 0;
    let FileType_Id = $btn.attr("data-file-type-id") || 0;
    var Direction_Id = $('[data-name="Direction_Id"]').val();
    var PaymentToDate = $('[data-name="PaymentToDate"]').val();
    var MemoDescription = $('[data-name ="TextMemo"').val();
    var postToAgentPortal = $('[data-name="SentComissionStatement"]').is(':checked');
    var EntityName = $btn.attr("data-entity_name");
    let PaymentType = parseInt($('[data-name="PaymentType"]').val(), 10);
    let OperatingBank = parseInt($('[data-name="OperatingBank"]').val(), 10);
    let PayeeNameOnCheck = $('[data-name="ShowPayeeName"]').val();
    if (!entityId) {
        console.error("Entity ID not found on button.");
        return;
    }

    if (entityTypeId == 7) { // use == or ensure entityTypeId is a number
        if (EntityName) {
            EntityName = EntityName.replace(/^State of\s*/, '');
            entityId = EntityName;
        }
    }
    var refreshedEntityName = EntityName; // NEW variable for refresh

    if (entityTypeId == 7 && EntityName == undefined) {
        refreshedEntityName = "State of " + EntityName;
        // DO NOT modify entityId
    }
    // Helper to get total applied amount from checked invoices
    //function getTotalAppliedAmount(entityId) {
    //    var total = 0;
    //    var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');
    //    if ($container.length === 0) return 0;

    //    $container.find('.invoice-check:checked').each(function () {
    //        var $row = $(this).closest('tr');
    //        var val = parseFloat($row.find('.amount-text').val()) || 0;
    //        total += val;
    //    });

    //    return total;
    //}

    // Get selected invoices
    //var SelectedInvoices = [];
    //var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');

    //if ($container.length === 0) {
    //    appCommon.ShowMessageModal("Invoice table not loaded yet.");
    //    return;
    //}



    //$container.find('.invoice-check:checked').each(function () {
    //    var $row = $(this).closest('tr');
    //    var invoiceAppliedAmount = parseFloat($row.find('.amount-text').val()) || 0;
    //    var apAmount = parseFloat($row.find('[name="invoice-APAmount"]').text().replace(/[$,]/g, '')) || 0;
    //    var apAmountPaid = parseFloat($row.find('[name="invoice-APAmountPaid"]').text().replace(/[$,]/g, '')) || 0;
    //    var arAmount = parseFloat($row.find('[name="invoice-ARAmount"]').text().replace(/[$,]/g, '')) || 0;
    //    var arAmountPaid = parseFloat($row.find('[name="invoice-ARAmountPaid"]').text().replace(/[$,]/g, '')) || 0;

    //    var apRemainingBalance = apAmount - apAmountPaid;
    //    var arRemainingBalance = arAmount - arAmountPaid;



    //    if (Direction_Id == MasterLookup._Directions.AR) {
    //        SelectedInvoices.push({
    //            Invoice_Id: $(this).data('invoice-id'),
    //            LineItem_Id: $(this).data('lineitem-id'),
    //            InvoiceLineItem_Id: $(this).data('invoiceLineitemId'),
    //            AppliedAmount: invoiceAppliedAmount,
    //            Amount: invoiceAppliedAmount,
    //            RemainingBalance: arRemainingBalance
    //        });
    //    }
    //    else {
    //        SelectedInvoices.push({
    //            Invoice_Id: $(this).data('invoice-id'),
    //            LineItem_Id: $(this).data('lineitem-id'),
    //            InvoiceLineItem_Id: $(this).data('invoiceLineitemId'),
    //            AppliedAmount: invoiceAppliedAmount,
    //            Amount: invoiceAppliedAmount,
    //            RemainingBalance: apRemainingBalance
    //        });
    //    }
    //});

    //if (SelectedInvoices.length === 0) {
    //    appCommon.ShowMessageModal("Please select at least one invoice.");
    //    return;
    //}

    //var totalAppliedAmount = getTotalAppliedAmount(entityId);


    function getTotalAppliedAmount(entityId) {
        if (paymentStore[entityId]) {
            var total = 0;
            Object.values(paymentStore[entityId].invoices).forEach(function (inv) {
                total += inv.amount || 0;
            });
            return total;
        }
        // Fallback: DOM (accordion is open)
        var total = 0;
        var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');
        if ($container.length === 0) return 0;
        $container.find('.invoice-check:checked').each(function () {
            total += parseFloat($(this).closest('tr').find('.amount-text').val()) || 0;
        });
        return total;
    }

    // Build SelectedInvoices from paymentStore — works whether accordion is expanded or collapsed
    var SelectedInvoices = [];
    var entityStore = paymentStore[entityId];

    if (!entityStore || Object.keys(entityStore.invoices).length === 0) {
        appCommon.ShowMessageModal("Please select at least one invoice.");
        return;
    }

    var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');

    Object.values(entityStore.invoices).forEach(function (inv) {
        var apRemainingBalance = 0;
        var arRemainingBalance = 0;

        // Read AP/AR amounts from DOM only if accordion is currently open
        if ($container.length > 0) {
            var $row = $container.find(
                'input.amount-text[data-invoice-id="' + inv.invoiceId +
                '"][data-lineitem-id="' + inv.lineItemId + '"]'
            ).closest('tr');

            if ($row.length > 0) {
                var apAmount = parseFloat($row.find('[name="invoice-APAmount"]').text().replace(/[$,]/g, '')) || 0;
                var apAmountPaid = parseFloat($row.find('[name="invoice-APAmountPaid"]').text().replace(/[$,]/g, '')) || 0;
                var arAmount = parseFloat($row.find('[name="invoice-ARAmount"]').text().replace(/[$,]/g, '')) || 0;
                var arAmountPaid = parseFloat($row.find('[name="invoice-ARAmountPaid"]').text().replace(/[$,]/g, '')) || 0;
                apRemainingBalance = apAmount - apAmountPaid;
                arRemainingBalance = arAmount - arAmountPaid;
            }
        }

        if (Direction_Id == MasterLookup._Directions.AR) {
            SelectedInvoices.push({
                Invoice_Id: inv.invoiceId,
                LineItem_Id: inv.lineItemId,
                InvoiceLineItem_Id: inv.invoiceLineItemId,
                AppliedAmount: inv.amount,
                Amount: inv.amount,
                RemainingBalance: arRemainingBalance
            });
        } else {
            SelectedInvoices.push({
                Invoice_Id: inv.invoiceId,
                LineItem_Id: inv.lineItemId,
                InvoiceLineItem_Id: inv.invoiceLineItemId,
                AppliedAmount: inv.amount,
                Amount: inv.amount,
                RemainingBalance: apRemainingBalance
            });
        }
    });

    var totalAppliedAmount = getTotalAppliedAmount(entityId);

    var BillDetail = {
        EntityType_Id: entityTypeId,
        Entity_Id: entityId,
        Due: due,
        AmountApplied: totalAppliedAmount,
        PostToAgentPortal: postToAgentPortal,
        PaymentToDate: PaymentToDate,
        MemoDescription: MemoDescription,
        EntityName: EntityName,
        PaymentType: PaymentType,
        Operating_Bank_ID: OperatingBank,
        PayeeNameOnCheck: PayeeNameOnCheck,
    };

    let isConfirmed = false;
    // Show confirmation modal
    appCommon.ShowConfirmationModal("Do you want to Create Payment?", "Confirmation", function () {
        // disable when user confirms
        if (isConfirmed) {
            $btn.prop("disabled", true);
        }
        appCommon.ExecuteAjaxPostMethod(
            "/MakePayments/ConfirmBillCreation",
            {
                Report_Id: Report_Id,
                FileType_Id: FileType_Id,
                SelectedInvoices: SelectedInvoices,
                Direction_Id: Direction_Id,
                BillDetail: BillDetail
            },
            function (response) {
                if (response?.Status) {
                    appCommon.ShowMessageModal(response.Message, "Success");

                    if (paymentStore[entityId]) {
                        delete paymentStore[entityId];
                    }

                    $(document).off('click', '[data-name="ModalMessageOk"], [data-name="ModalMessageCancel"]')
                        .on('click', '[data-name="ModalMessageOk"], [data-name="ModalMessageCancel"]', function () {
                            var EntityName = $('[data-name="AgencyAccordion"][data-entity-id="' + entityId + '"]').data('payee-name');
                            if (entityTypeId == 7) {
                                refreshInvoiceTable(entityId, entityTypeId, refreshedEntityName);
                            }
                            else {
                                refreshInvoiceTable(entityId, entityTypeId, EntityName);
                            }
                        });
                } else {
                    appCommon.ShowMessageModal(response?.Message || "Unable to process request.");
                    $btn.prop("disabled", false);
                }
            },
            true
        );
    }, function () {
        // when user clicks on No in modal
        isConfirmed = false;
        $btn.prop("disabled", false);
    });
    // Handles when user clicks on modal (X)/ ESC/ Backdrop
    $('#ModalConfirmationCancel').on('hidden.bs.modal', function () {
        if (!isConfirmed) {
            $btn.prop("disabled", false);
        }
    })
}
$(document).off('change', '[data-name="PaymentType"]');
$(document).on('change', '[data-name="PaymentType"]', function () {
    var $this = $(this);
    var paymentType = $this.val();

    $this.removeClass('is-invalid');
    $('[data-name="PaymentTypeRequired"]').addClass('d-none');

    $('[data-name="qboMessage"], [data-name="checkMessage"]')
        .addClass('d-none');

    if (paymentType == '7') {
        $('[data-name="qboMessage"]').removeClass('d-none');
    }
    else if (paymentType == '6') {
        $('[data-name="checkMessage"]').removeClass('d-none');
    }
});
function refreshInvoiceTable(entityId, entityTypeId, EntityName) {
    var EntityTypeId = $('[data-name="EntityType_Id"]').val();
    var EntityId = $('[data-name="Entity_Id"]').val();
    var AmountBasedOnDue = $('[data-name="AmountDueBasedOnType_Id"]').val();
    var LineItem_Id = $('[data-name="LineItem_Id"]').val();
    var PolicyNumber = $('[data-name="PolicyNumber"]').val();
    var InsuredName = $('[data-name="InsuredName"]').val();
    var InvoiceNo = $('[data-name="InvoiceNo"]').val();
    var OutStandingBalance = $('[name="OutstandingBalances"]').prop('checked');
    var Direction_Id = $('[data-name="Direction_Id"]').val();
    var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');

    if ($container.length === 0) return;

    appCommon.RenderHTMLPanel(
        "/MakePayments/InvoiceTable"
        + "?Payee_Id=" + entityId
        + "&PayeeType_Id=" + entityTypeId
        + "&EntityType_Id=" + EntityTypeId
        + "&Entity_Id=" + EntityId
        + "&AmountDueBasedOnType_Id=" + AmountBasedOnDue
        + "&LineItem_Id=" + LineItem_Id
        + "&PolicyNumber=" + PolicyNumber
        + "&InsuredName=" + InsuredName
        + "&InvoiceNo=" + InvoiceNo
        + "&OutstandingBalances=" + OutStandingBalance
        + "&Direction_Id=" + Direction_Id
        + "&EntityName=" + EntityName,
        $container,
        function () {
            $container.css('min-height', '100px');
            //  Reset Amount Applied,uncheck apply all and Hide createbill button
            var agency = $container.closest('.agency');
            agency.find('div[data-name="MakePaymentsAmountApplied"] strong').text('$0.00');
            agency.find('.apply-all').prop('checked', false);
            agency.find('button[data-name="btnCreateBill"]').hide();

            var totalNetDue = 0;
            $container.find('tbody.invoice-table tr').each(function () {
                var $row = $(this);
                if ($row.find('td').length === 1) {
                    return;
                }
                if (Direction_Id == MasterLookup._Directions.AR) {
                    var netOutgoingPaymentDueText = $row.find('[name="invoice-NetOutgoingPaymentDue"]').text() || "$0.00";
                    var netOutgoingPaymentDue = parseFloat(netOutgoingPaymentDueText.replace(/[$,]/g, '')) || 0;
                    totalNetDue += netOutgoingPaymentDue;
                }
                else {
                    var netDueText = $row.find('[name="invoice-NetDue"]').text() || "$0.00";
                    var netDue = parseFloat(netDueText.replace(/[$,]/g, '')) || 0;
                    totalNetDue += netDue;
                }
            });
            agency.find('div[data-name="totaldue"] strong').text('$' + totalNetDue.toFixed(2));

            // Check if invoice table is empty, remove accordion if empty
            var invoiceRows = $container.find('tbody.invoice-table tr');
            //var noRecords = invoiceRows.length === 0 || invoiceRows.find('td:contains("No records found.")').length > 0;
            //if (noRecords) {
            //    agency.hide();
            //} else {
            //    agency.show();
            //}
            var noInvoiceRecords = invoiceRows.filter(function () {
                return $(this).find('td[name="invoice-No"]').length > 0;
            }).length === 0;

            if (noInvoiceRecords) {
                agency.hide();
            } else {
                agency.show();
            }
            var $allAgencies = $('[data-name="LoadAgencyContainer"] .agency');
            if ($allAgencies.filter(':visible').length === 0) {
                $('[data-name="LoadAgencyContainer"] .no-records-message').remove();
                $('[data-name="LoadAgencyContainer"]').append(
                    '<div class="text-center no-records-message" style="margin:50px 0;">No records found.</div>'
                );
            } else {
                $('[data-name="LoadAgencyContainer"] .no-records-message').remove();
            }
        },
        true
    );
}
$('[data-name="btnConfirmBillCreation"]').on('click', function () {
    var $paymentType = $('[data-name="PaymentType"]');
    var $operatingBank = $('[data-name="OperatingBank"]');

    var checkPaymentTypeId = '6';

    // Reset validation
    $paymentType.removeClass('is-invalid');
    $operatingBank.removeClass('is-invalid');

    // Validate Payment Type
    if (!$paymentType.val()) {
        $paymentType.addClass('is-invalid');
        return;
    }

    // Validate Bank Account only when Payment Type is Check
    if ($paymentType.val() == checkPaymentTypeId && !$operatingBank.val()) {
        $operatingBank.addClass('is-invalid');
        return;
    }

    // Everything is valid
    $('[data-name="GenericModal"]').modal('hide');
    confirmBillCreationAndPayment(this);
});
$(document).on('click', '[data-name="editPayeeNameOnCheck"]', function () {
    var $input = $('[data-name="ShowPayeeName"]');
    $input.prop('readonly', false).focus();
    $(this).hide();
    $('[data-name="savePayeeNameOnCheck"]').show();
});
$(document).on('click', '[data-name="savePayeeNameOnCheck"]', function () {
    var $input = $('[data-name="ShowPayeeName"]');
    var payeeName = $input.val();
    $input.val(payeeName).prop('readonly', true);
    $(this).hide();
    $('[data-name="editPayeeNameOnCheck"]').show();
});