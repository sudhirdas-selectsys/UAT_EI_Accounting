$(function () {
    $('[data-name="EntityName"]').each(function () {
        EnableNameComplete(this);
    });
    $('[data-name="PolicyNumber"]').each(function () {
        EnablePolicyNumberComplete(this);
    });
    $('[data-name="AgencyName"]').each(function () {
        EnableAgencyNameComplete(this);
    });
    $('[data-name="InsuredName"]').each(function () {
        EnableInsuredNameComplete(this);
    });
    $('[data-name="InvoiceNo"]').each(function () {
        EnableInvoiceNumberComplete(this);
    });
    BindCreatePaymentClickEvents();
    MakePaymentsSearchPanelEvents();
    LoadMakepaymentsWithFilters();
    toggleClearSelectionsButton();
});

function EnableNameComplete(ctrl) {
    $(document).on("change", '[data-name="EntityType_Id"]', function () {
        var filterSection = $(this).closest('[data-name="FilterSection"]');

        filterSection.find('[data-name="EntityName"]').val('');
        filterSection.find('[data-name="Entity_Id"]').val('');
    });
    $(document).on("input", '[data-name="EntityName"]', function () {
        $(this)
            .closest('[data-name="FilterSection"]')
            .find('[data-name="Entity_Id"]')
            .val('');
    });
    if ($(ctrl).attr("type") != "text")
        return;

    $(ctrl).autocomplete({
        classes: {
            "ui-autocomplete": "highlight"
        },
        source: function (request, response) {
            var selectedEntity = $(ctrl).closest('[data-name="FilterSection"]').find('[data-name="EntityType_Id"]').val();
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
            $(ctrl).closest('[data-name="FilterSection"]').find('[data-name="EntityName"]').val(ui.item.value);
            $(ctrl).parent().find('[data-name="Entity_Id"]').val(ui.item.EntityID);
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


function CreateBillView(callback) {
    appCommon.ExecuteAjaxGetMethod("/MakePayments/CreateBill", function (response) {
        if (response != null && response != undefined) {
            appCommon.GenericModalFunction("Create Bill", response);
            var modal = $('[data-name="GenericModal"]');
            modal.find(".modal-dialog").css("--bs-modal-width", "500px");
            if (callback) callback(modal);
        }
    },
        true
    );
}
function BindCreatePaymentClickEvents() {

    $(document).off("click", "button[data-name='btnCreateBill']");
    $(document).on("click", "button[data-name='btnCreateBill']", function () {

        var btn = $(this);
        var entity_Id = btn.data("entity_id");
        var entityType_Id = btn.data("entitytype_id");
        var invoice_Id = btn.data("invoice-id");
        var amount = parseFloat(btn.data("amount")) || 0;
        var due = parseFloat(btn.data("due")) || 0;
        var entityName = btn.data('entity-name');
        if (entityType_Id == 7) {
            if (entityName) {
                entityName = entityName.replace(/^State of\s*/, '');
                entity_Id = entityName;
            }
        }
        if (amount > 0) {
            //console.log("getAllSelectedPayments", getAllSelectedPayments());
            var direction_Id = $('[data-name="Direction_Id"]').val();
            CreateBillView(function (modal) {

                var confirmBtn = modal.find("button[data-name='btnConfirmBillCreation']");
                confirmBtn.prop("disabled", true);
                confirmBtn
                    .attr("data-entity_id", entity_Id)
                    .attr("data-entitytype_id", entityType_Id)
                    .attr("data-invoice_id", invoice_Id)
                    .attr("data-amount", amount)
                    .attr("data-due", due)
                    .attr("data-direction_id", direction_Id)
                    .attr("data-entity_name", entityName);
                BindInvoiceLineItemAndAppliedAmount(btn, modal);
                UpdateCommissionCheckbox(entity_Id, modal);
                appCommon.ExecuteAjaxGetMethod(
                    '/MakePayments/CreatePayeeBill?EntityType_Id=' + entityType_Id +
                    "&Entity_Id=" + entity_Id +
                    "&Amount=" + amount +
                    "&Invoice_Id=" + invoice_Id +
                    "&Direction_Id=" + direction_Id +
                    "&EntityName=" + entityName,
                    function (response) {

                        if (response.Status === true && response.Data != null) {

                            var d = response.Data.EntityDetail;
                            var html = "";
                            modal.find("[data-name='ShowPayeeName']").val(d.EntityName ?? "");
                            html += `<span class="fw-bold">${d.EntityName ?? ""}</span><br />`;

                            if (d.EntityAddress) {
                                html += `${d.EntityAddress}<br />`;
                            }

                            if (d.EntityCity || d.EntityState || d.EntityZipCode) {
                                html += [d.EntityCity, d.EntityState, d.EntityZipCode]
                                    .filter(Boolean)
                                    .join(", ") + "<br />";
                            }

                            if (d.EntityEmail) {
                                html += `Email: ${d.EntityEmail}<br />`;
                            }

                            modal.find("[data-name='payeeInformation']").html(html);
                            confirmBtn.removeAttr("disabled");
                            var selectedItems = getAllSelectedPayments();

                            //var invoiceLineItems = response.Data.EntityInvoice;

                            //if (selectedItems?.length > 0) {
                            //    var selectedList = selectedItems[0].Invoices;

                            //    var filtered = invoiceLineItems.filter(function (db) {
                            //        return selectedList.some(function (ui) {
                            //            return ui.InvoiceId == db.InvoiceId &&
                            //                ui.LineItemId == db.LineItemId;
                            //        });
                            //    });

                            //    //console.log(filtered);
                            //}

                            modal.modal("show");

                        } else {
                            appCommon.ShowMessageModal(response.Message, "Error");
                        }
                    },
                    true,
                    true
                );
            });
        }
        else if (amount < 0) {
            appCommon.ShowMessageModal("Applied amount cannot be negative.", "Error");
        }
        else {
            appCommon.ShowMessageModal("The applied amount must be greater than zero.", "Error");
        }
    });
}

function getAllSelectedPayments() {
    var allData = [];
    Object.keys(paymentStore).forEach(function (entityId) {
        var entity = paymentStore[entityId];
        if (!entity || Object.keys(entity.invoices).length === 0) return;

        var invoiceList = [];

        Object.values(entity.invoices).forEach(function (inv) {
            // Try to read live DOM values if container is loaded
            var $cont = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]').first();
            var $row = $cont.find('input.amount-text[data-invoice-id="' + inv.invoiceId + '"][data-lineitem-id="' + inv.lineItemId + '"]').closest('tr');

            var obj = {
                EntityId: entityId,
                InvoiceNo: $row.length ? $row.find('td[name="invoice-No"]').text().trim() : '',
                BillType: $row.length ? $row.find('td[name="invoice-BillType"]').text().trim() : '',
                LineTypeName: $row.length ? $row.find('td[name="invoice-LineTypeName"]').text().trim() : '',
                APAmount: $row.length ? $row.find('td[name="invoice-APAmount"]').text().replace(/[$,]/g, '').trim() : '0',
                AmountCollected: $row.length ? $row.find('td[name="invoice-AmountCollected"]').text().replace(/[$,]/g, '').trim() : '0',
                AmountPaid: $row.length ? $row.find('td[name="invoice-AmountPaid"]').text().replace(/[$,]/g, '').trim() : '0',
                NetDue: $row.length ? $row.find('td[name="invoice-NetDue"]').text().replace(/[$,]/g, '').trim() : '0',
                AmountEntered: inv.amount ? inv.amount.toString() : '0',
                InvoiceId: inv.invoiceId,
                LineItemId: inv.lineItemId,
                InvoiceLineItemId: inv.invoiceLineItemId
            };

            invoiceList.push(obj);
        });

        allData.push({
            AgencyId: entityId,
            PayeeName: entity.entityName,
            Invoices: invoiceList
        });
    });

    return allData;
}

function LoadAgencyPanel() {
    var AmountBasedOnDue = $('[ data-name="AmountDueBasedOnType_Id"]').val();
    var pageIndex = $('[name="pageIndex"]').val() || 1;
    var pageSize = $('[name="pageSize"]').val() || 20;
    var DirectionId = $('[data-name="Direction_Id"]').val();

    appCommon.RenderHTMLPanel(
        "/MakePayments/GetAllEntities"
        + "?AmountDueBasedOnType_Id=" + AmountBasedOnDue
        + "&Direction_Id=" + DirectionId
        + "&pageIndex=" + pageIndex
        + "&pageSize=" + pageSize,
        $('[data-name="LoadAgencyContainer"]'),
        function (response) {
        },
        true
    );
}
var paymentStore = {};
var isLoading = false;
var lastScrollTop = 0;
var loadedPages = [];   // track all loaded pages
var maxPage = null;     // will be set when no more data

function LoadMakepaymentsWithFilters() {
    var agencyContainer = $('[data-name="LoadAgencyContainer"]');
    appCommon.ShowPartialPanelLoader(agencyContainer);
    appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
        if (response != undefined && response != null) {
            $(agencyContainer).html(response);
            PartialInvoiceDetails = [];
            MakePaymentsSearchPanelEvents();
            loadedPages.push(1);
            restoreSelections();
            syncHeaderUIFromStore();
            var $entities = $(agencyContainer)
                .find('[data-name="AgencyAccordion"]');

            if ($entities.length === 1) {
                $entities.first().trigger('click');
            }
        }
    }, "/Search/MakePaymentsSearch", true, false);
}

//$('[data-name="LoadAgencyContainer"]').on('scroll', function () {
$('#payment-container').on('scroll', function () {
    var container = $(this);
    var currentScrollTop = container.scrollTop();

    // Scroll down
    if (!isLoading && currentScrollTop > lastScrollTop && currentScrollTop + container.innerHeight() >= container[0].scrollHeight - 50) {

        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var nextPage = currentPage + 1;

        // Skip if already loaded OR beyond maxPage
        if (loadedPages.indexOf(nextPage) === -1 && (!maxPage || nextPage <= maxPage)) {
            isLoading = true;
            $('[name="pageIndex"]').val(nextPage);
            $('[name="pageSize"]').val(20);
            LoadMakepaymentsWithFiltersForScroll("down", nextPage);
        }
    }

    // Scroll up
    if (!isLoading && currentScrollTop < lastScrollTop && currentScrollTop <= 5) {
        var currentPage = parseInt($('[name="pageIndex"]').val(), 10) || 1;
        var prevPage = currentPage - 1;

        // Stop at page 1 and skip if already loaded
        if (prevPage > 0 && loadedPages.indexOf(prevPage) === -1) {
            isLoading = true;
            $('[name="pageIndex"]').val(prevPage);
            $('[name="pageSize"]').val(20);
            LoadMakepaymentsWithFiltersForScroll("up", prevPage);
        }
    }

    lastScrollTop = currentScrollTop;
});

function LoadMakepaymentsWithFiltersForScroll(direction, pageIndex) {
    var Container = $('[data-name="LoadAgencyContainer"]');

    var oldHeight = Container[0].scrollHeight;
    var oldScrollTop = Container.scrollTop();
    ShowLoaderRowForScroll(direction);
    appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
        if (response) {
            if (response.indexOf("No records found") !== -1) {
                //console.log("No records found, setting maxPage");
                // mark this as the last page
                maxPage = pageIndex;
            } else {
                if (direction === "down") {
                    Container.append(response);
                } else if (direction === "up") {
                    Container.prepend(response);

                    // preserve scroll position after prepend
                    var newHeight = Container[0].scrollHeight;
                    Container.scrollTop(oldScrollTop + (newHeight - oldHeight));
                }
                PartialInvoiceDetails = [];
                MakePaymentsSearchPanelEvents();
                restoreSelections();
                syncHeaderUIFromStore();
            }

            // Mark this page as loaded
            if (loadedPages.indexOf(pageIndex) === -1) {
                loadedPages.push(pageIndex);
            }
        }
        RemoveLoaderRowForScroll();
        isLoading = false;
        lastScrollTopAR = Container.scrollTop();
    }, "/Search/MakePaymentsSearch", true, false);
}

function ShowLoaderRowForScroll(direction) {
    var loaderRow = `
    <div class="accordion-item loading-scroll-row" style="padding:10px; border:1px solid #ddd; margin-bottom:10px;">
       <div style="display:flex; flex-wrap:nowrap; justify-content:flex-start; gap:12px;">
            <!-- Entity Name / Entity Type -->
            <div style="width:500px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:30px"></div>
            <!-- Due -->
            <div style="width:150px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:360px"></div>
            <!-- Amount Applied -->
            <div style="width:175px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:230px"></div>
            <!-- Apply All -->
            <div style="width:100px; height:20px; border-radius:4px;
              background:linear-gradient(90deg,#f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
              background-size:200% 100%; animation: shimmer 1.5s infinite;margin-left:235px"></div>
        </div>
        <style>
          @keyframes shimmer {
            0% { background-position:-200% 0; }
            100% { background-position:200% 0; }
          }
        </style>
    </div>`;

    if (direction === "down") {
        $('[data-name="LoadAgencyContainer"]').append(loaderRow);
    } else {
        $('[data-name="LoadAgencyContainer"]').prepend(loaderRow);
    }
}

function RemoveLoaderRowForScroll() {
    $('[data-name="LoadAgencyContainer"] .loading-scroll-row').remove();
}

function ClearloadedPages() {
    $('[name="pageIndex"]').val(1);
    $('[name="pageSize"]').val(20);
    loadedPages = [];
    loadedPages.push(1);
}

//$(document).off('click', '[data-name="AgencyAccordion"]');
//$(document).on('click', '[data-name="AgencyAccordion"]', function (e) {
//    e.stopPropagation();

//    const agentDetailId = $(this).attr('agentdetailsid');
//    const $container = $(this)
//        .closest('.agency')
//        .find('[data-name="MakePaymentsInvoiceContainer"]');

//    appCommon.ExecuteAjaxGetMethod(
//        "/MakePayments/InvoiceTable?AgentDetail_id=" + agentDetailId,
//        function (response) {
//            if (response !== undefined && response !== null) {
//                $container.html(response);
//            }
//        },
//        true
//    );
//});


$(document).off('click', '[data-name="AgencyAccordion"]');
$(document).on('click', '[data-name="AgencyAccordion"]', function (e) {
    if (!$(this).hasClass('collapsed')) {

        var EntityTypeId = $('[data-name="EntityType_Id"]').val();
        var EntityId = $('[ data-name="Entity_Id"]').val();
        var AmountBasedOnDue = $('[ data-name="AmountDueBasedOnType_Id"]').val();
        var LineItem_Id = $('[data-name="LineItem_Id"]').val();
        var PolicyNumber = $('[data-name="PolicyNumber"]').val();
        var InsuredName = $('[data-name="InsuredName"]').val();
        var InvoiceNo = $('[data-name="InvoiceNo"]').val();
        var OutStandingBalance = $('[name="OutstandingBalances"]').prop('checked');
        var Direction_Id = $('[data-name="Direction_Id"]').val();

        var accordion = $(this);
        if (accordion.data('loaded') === true) {
            return;
        }

        var lineItemIds = [];

        $("input[data-name='lineitem-options']:checked").each(function () {
            lineItemIds.push($(this).data("lineitem-id"));
        });
        //  Find Apply All checkbox inside this accordion
        var $applyAllCheckbox = accordion.find('input[data-name="applyAll"]');
        var isApplyAllChecked = $applyAllCheckbox.prop('checked');

        var Payee_Id = $(this).attr('data-entity-Id');
        var PayeeType_Id = $(this).attr('data-entity-type-Id');
        var EntityName = $(this).attr('data-payee-name');
        if (PayeeType_Id === '7') {
            Payee_Id = EntityName.replace(/^State of\s*/, '');
        }

        var agentDetailId = $(this).attr('data-agent-detail-id');
        var entityTypeName = accordion.find('.agency-name').last().text().trim();
        var storeEntity = paymentStore[Payee_Id];
        if (storeEntity && storeEntity.entityTypeId) {
            EntityTypeId = storeEntity.entityTypeId; // override from store
        }

        var accordionEntityId = accordion.attr('data-entity-id') || Payee_Id;
        if (PayeeType_Id === '7' || accordionEntityId === '0' || accordionEntityId === '') {
            accordionEntityId = Payee_Id; 
        }

        if (!e.target.closest(".agency-details .prevent-toggle")) {
            var InvoiceDate = $('#InvoiceDate').val();
            var InvoiceCreatedFromDate = $('[name="InvoiceCreatedFromDate"]').val();
            var InvoiceCreatedToDate = $('[name="InvoiceCreatedToDate"]').val();
            var InvoiceEffectiveFromDate = $('[name="InvoiceEffectiveFromDate"]').val();
            var InvoiceEffectiveToDate = $('[name="InvoiceEffectiveToDate"]').val();
            var PaymentBookedDate = $('[name="PaymentBookedDate"]').val();

            var $targetContainer = $('[data-name="MakePaymentsInvoiceContainer_' + Payee_Id + '"]');

            // Find missing stored invoices BEFORE any fetch
            var storeEntityNow = paymentStore[accordionEntityId];
            var missingInvoicesToPrefetch = [];
            if (storeEntityNow && Object.keys(storeEntityNow.invoices).length > 0) {
                Object.values(storeEntityNow.invoices).forEach(function (inv) {
                    var alreadyQueued = missingInvoicesToPrefetch.some(function (m) { return m.invoiceId === inv.invoiceId; });
                    if (!alreadyQueued) {
                        missingInvoicesToPrefetch.push({ invoiceId: inv.invoiceId, invoiceNo: inv.invoiceNo });
                    }
                });
            }

            // Build all AJAX promises at once — filtered fetch + all stored fetches in parallel
            var filteredUrl = "/MakePayments/InvoiceTable"
                + "?Payee_Id=" + Payee_Id
                + "&PayeeType_Id=" + PayeeType_Id
                + "&EntityType_Id=" + EntityTypeId
                + "&Entity_Id=" + EntityId
                + "&AmountDueBasedOnType_Id=" + AmountBasedOnDue
                + "&LineItem_Id=" + LineItem_Id
                + "&LineItemList=" + lineItemIds
                + "&PolicyNumber=" + PolicyNumber
                + "&InsuredName=" + InsuredName
                + "&InvoiceNo=" + InvoiceNo
                + "&OutstandingBalances=" + OutStandingBalance
                + "&Direction_Id=" + Direction_Id
                + "&EntityName=" + EntityName
                + "&InvoiceDate=" + InvoiceDate
                + "&InvoiceCreatedFromDate=" + InvoiceCreatedFromDate
                + "&InvoiceCreatedToDate=" + InvoiceCreatedToDate
                + "&InvoiceEffectiveFromDate=" + InvoiceEffectiveFromDate
                + "&InvoiceEffectiveToDate=" + InvoiceEffectiveToDate
                + "&PaymentBookedDate=" + PaymentBookedDate;

            var allRequests = [$.ajax({ url: filteredUrl })];

            missingInvoicesToPrefetch.forEach(function (missing) {
                var storedUrl = "/MakePayments/InvoiceTable"
                    + "?Payee_Id=" + Payee_Id
                    + "&PayeeType_Id=" + PayeeType_Id
                    + "&EntityType_Id=" + EntityTypeId
                    + "&Entity_Id=" + EntityId
                    + "&AmountDueBasedOnType_Id=" + AmountBasedOnDue
                    + "&LineItem_Id=" + LineItem_Id
                    + "&LineItemList=" + lineItemIds
                    + "&PolicyNumber="
                    + "&InsuredName="
                    + "&InvoiceNo=" + missing.invoiceNo
                    + "&OutstandingBalances=false"
                    + "&Direction_Id=" + Direction_Id
                    + "&EntityName=" + EntityName
                    + "&InvoiceDate="
                    + "&InvoiceCreatedFromDate="
                    + "&InvoiceCreatedToDate="
                    + "&InvoiceEffectiveFromDate="
                    + "&InvoiceEffectiveToDate="
                    + "&PaymentBookedDate=";
                allRequests.push($.ajax({ url: storedUrl }));
            });

            // Show loader while all requests are in flight
            appCommon.ShowPartialPanelLoader($targetContainer);

            // Wait for ALL requests to complete, then render once
            $.when.apply($, allRequests).done(function () {
                // arguments[0] = [filteredHtml, status, xhr]
                // arguments[1] = [storedHtml1, status, xhr]  (if multiple, each is an array)
                // If only one request, arguments itself is [html, status, xhr]
                var responses = allRequests.length === 1
                    ? [arguments]
                    : Array.prototype.slice.call(arguments);

                // First response is always the filtered result
                var filteredHtml = responses[0][0];

                // Render filtered HTML into container
                $targetContainer.html(filteredHtml);
                $targetContainer.css('min-height', '100px');

                if (PayeeType_Id === '7') {
                    Payee_Id = EntityName.replace(/^State of\s*/, '');
                }
                var $container = $('[data-name="MakePaymentsInvoiceContainer_' + Payee_Id + '"]');
                var $tbody = $container.find('tbody');

                // Append stored rows from remaining responses (index 1+)
                //for (var i = 1; i < responses.length; i++) {
                //    var storedHtml = responses[i][0];
                //    var $rows = $(storedHtml).find('tbody tr').filter(function () {
                //        var text = $(this).text().trim();
                //        return text !== '' && text.toLowerCase().indexOf('no records') === -1;
                //    });
                //    if ($rows.length) {
                //        $tbody.append($rows);
                //    }
                //}
                /////////////////////////////////////////newly added code by Navyatha
                // Build a set of rows already returned by the filtered request
                var existingRowKeys = new Set();

                $tbody.find('tr').each(function () {
                    var $input = $(this).find('input.amount-text');

                    if (!$input.length) return;

                    var invoiceId = String($input.data('invoice-id') || '');
                    var lineItemId = String($input.data('lineitem-id') || '');
                    var invoiceLineItemId = String($input.data('invoice-lineitem-id') || '');

                    var key = `${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

                    existingRowKeys.add(key);
                });

                // Append ONLY stored rows that are not already present
                for (var i = 1; i < responses.length; i++) {

                    var storedHtml = responses[i][0];

                    var $rows = $(storedHtml).find('tbody tr').filter(function () {

                        var $input = $(this).find('input.amount-text');

                        if (!$input.length) return false;

                        var text = $(this).text().trim();

                        if (text === '' || text.toLowerCase().indexOf('no records') !== -1) {
                            return false;
                        }

                        var invoiceId = String($input.data('invoice-id') || '');
                        var lineItemId = String($input.data('lineitem-id') || '');
                        var invoiceLineItemId = String($input.data('invoice-lineitem-id') || '');

                        var key = `${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

                        // Already exists in filtered result
                        if (existingRowKeys.has(key)) {
                            return false;
                        }

                        // Mark it as added
                        existingRowKeys.add(key);

                        return true;
                    });

                    if ($rows.length) {
                        $tbody.append($rows);
                    }
                }
                //////////////////////////////////////////////////end

                // Remove rows whose invoiceId is in the store but the specific lineItemId was NOT selected
                var storeEntityNow2 = paymentStore[accordionEntityId];
                if (storeEntityNow2 && Object.keys(storeEntityNow2.invoices).length > 0) {
                    var storedInvoiceIds = new Set(
                        Object.values(storeEntityNow2.invoices).map(function (inv) { return String(inv.invoiceId); })
                    );

                    $tbody.find('tr').each(function () {
                        var $input = $(this).find('input.amount-text');
                        if (!$input.length) return;

                        var rowInvoiceId = String($input.data('invoice-id'));
                        var rowLineItemId = String($input.data('lineitem-id'));
                        var rowInvoiceLineItemId = String($input.data('invoice-lineitem-id')); // ✅ the true unique key

                        if (!storedInvoiceIds.has(rowInvoiceId)) return;

                        // Match on all three — invoiceId + lineItemId + invoiceLineItemId
                        var isInStore = Object.values(storeEntityNow2.invoices).some(function (inv) {
                            return String(inv.invoiceId) === rowInvoiceId
                                && String(inv.lineItemId) === rowLineItemId
                                && String(inv.invoiceLineItemId) === rowInvoiceLineItemId;
                        });

                        if (!isInStore) {
                            $(this).remove();
                        }
                    });
                }

                // Move stored rows to top
                moveStoredRowsToTop(accordionEntityId, $container);

                // Now do all post-render work
                if (isApplyAllChecked) {
                    var totalAmountApplied = 0;
                    $container.find('.invoice-check').each(function () {
                        var $checkbox = $(this);
                        var $row = $checkbox.closest('tr');
                        if ($row.hasClass('disabled-row')) return true;
                        var netDue = parseFloat($row.find('.amount-text').data('net-due')) || 0;
                        $checkbox.prop('checked', true);
                        $row.find('.amount-text').val(netDue.toFixed(2)).prop('disabled', true);
                        totalAmountApplied += netDue;
                        storePayment($checkbox, $row);
                    });
                    $container.closest('.agency')
                        .find('div[data-name="MakePaymentsAmountApplied"] strong')
                        .text('$' + totalAmountApplied.toFixed(2));
                }

                accordion.data('loaded', true);

                var totalEligibleRows = $container.find('.invoice-check').filter(function () {
                    return !$(this).closest('tr').hasClass('disabled-row');
                }).length;

                if (!paymentStore[accordionEntityId]) {
                    paymentStore[accordionEntityId] = {
                        entityTypeId: PayeeType_Id,
                        entityName: EntityName,
                        entityTypeName: entityTypeName,
                        agentDetailId: agentDetailId,
                        invoices: {}
                    };
                }
                paymentStore[accordionEntityId]._totalRows = totalEligibleRows;

                restoreSelectionsForContainer($container, accordionEntityId);
                restoreSelections();

            }).fail(function () {
                // Fallback — just load filtered result normally
                appCommon.RenderHTMLPanel(filteredUrl, $targetContainer, function () {
                    restoreSelectionsForContainer($targetContainer, accordionEntityId);
                    restoreSelections();
                }, true);
            });
        }
        else {
            e.stopPropagation();
            return;
        }
    }

});

function moveStoredRowsToTop(entityId, $container) {
    var storeEntity = paymentStore[entityId];
    if (!storeEntity || Object.keys(storeEntity.invoices).length === 0) return;

    var $tbody = $container.find('tbody');

    // Collect stored invoiceId+lineItemId pairs for matching
    var storedKeys = Object.values(storeEntity.invoices).map(function (inv) {
        return { invoiceId: String(inv.invoiceId), lineItemId: String(inv.lineItemId) };
    });

    var $storedRows = $tbody.find('tr').filter(function () {
        var $input = $(this).find('input.amount-text');
        if (!$input.length) return false;
        var rowInvoiceId = String($input.data('invoice-id'));
        var rowLineItemId = String($input.data('lineitem-id'));
        return storedKeys.some(function (k) {
            return k.invoiceId === rowInvoiceId && k.lineItemId === rowLineItemId;
        });
    });

    if ($storedRows.length) {
        $tbody.prepend($storedRows); // detaches from current position and moves to top
    }
}

//$(document).off('click', '[data-name="AgencyAccordion"]');
//$(document).on('click', '[data-name="AgencyAccordion"]', function () {
//    if (!$(this).hasClass('collapsed')) {
//        var agentDetailId = $(this).attr('data-agent-detail-Id');
//        var $container = $('[data-name="MakePaymentsInvoiceContainer_' + agentDetailId + '"]');
//        var $loader = $container.find('.transaction-table-skeleton');      
//        $loader.show();
//        appCommon.RenderHTMLPanel("/MakePayments/InvoiceTable" + "?AgentDetail_id=" + agentDetailId, $container, function () {           
//            $loader.hide();
//            $container.css('min-height', '100px');
//        }, true);
//    }
//});


function MakePaymentsSearchPanelEvents() {
    $(document).off('click', '[data-name="btnReset"]')
    $(document).on('click', '[data-name="btnReset"]', function () {
        ClearloadedPages();
        var MakePaymentsPanel = $('[data-name="frmAdditionalFilters"]')
        $('[data-name="Entity_Id"]').val('');
        appCommon.ResetFieldsInPanel(MakePaymentsPanel);
        var multiSelect = MakePaymentsPanel.find('.custom-multi-selection');

        multiSelect.find('input[type="checkbox"][data-name="lineitem-options"]').prop('checked', false);
        multiSelect.find('.chip').remove();
        multiSelect.find('.default-text').show();
        multiSelect.find('input[name="LineItemList"]').val('');
        PartialInvoiceDetails = [];

        // restore defaults
        $('[data-name="AmountDueBasedOnType_Id"]').prop('selectedIndex', 0);
        $('[data-name="Direction_Id"]').val('2');
        $('#OutstandingBalances').prop('checked', true);
        $('[data-date-type="CreatedDate"]').prop('checked', true);
        $("#InvoiceDate").val("InvoiceCreatedDate");
        //LoadAgencyPanel();
        LoadMakepaymentsWithFilters();
        resetPaymentStore();

    });

    $(document).off('click', '[data-name="btnApplyFilters"]')
    $(document).on('click', '[data-name="btnApplyFilters"]', function () {
        //console.log("btnApplyFilters", paymentStore);
        ClearloadedPages();
        var LoadAgencyContainer = $('[data-name="LoadAgencyContainer"]');
        appCommon.SubmitForm($('[data-name="frmAdditionalFilters"]'), function (response) {
            console.log("SEARCH RESPONSE:", response);
            if (response != undefined && response != null) {
                var hasStoredSelections = Object.keys(paymentStore).some(function (entityId) {
                    return Object.keys(paymentStore[entityId].invoices).length > 0;
                });

                var isNoRecords = response.indexOf("No records found") !== -1;

                if (isNoRecords && hasStoredSelections) {
                    // Don't show "No records found" — just show stored entities
                    $(LoadAgencyContainer).html('');
                } else if (isNoRecords && !hasStoredSelections) {
                    // Truly no records and nothing stored — show the message
                    $(LoadAgencyContainer).html(response);
                    return;
                } else {
                    // Normal response with records
                    $(LoadAgencyContainer).html(response);
                }

                // Always prepend stored entities that are not in current results
                var selectedHtml = buildSelectedAgenciesHtml(response);
                if (selectedHtml) {
                    $(LoadAgencyContainer).prepend(selectedHtml);
                }
                $("#payment-container").scrollTop(0);
                MakePaymentsSearchPanelEvents();
                setTimeout(function () {
                    restoreSelections();
                    //syncHeaderUIFromStore();
                    toggleClearSelectionsButton();
                    var $entities = $(LoadAgencyContainer)
                        .find('[data-name="AgencyAccordion"]');

                    if ($entities.length === 1) {
                        $entities.first().trigger('click');
                    }
                }, 0);
            }
        }, "/Search/MakePaymentsSearch", true, true);
    });

    $(document).off('change', '[data-name="Direction_Id"]');
    $(document).on('change', '[data-name="Direction_Id"]', function () {
        var Direction_Id = $(this).val();
        var Outstanding = $('[name="OutstandingBalances"]');

        ClearloadedPages();

        if (!Outstanding.prop('checked')) {
            Outstanding.prop('checked', true);
        }

        $('[data-name="frmSeach"] input[name="Direction_Id"]').val(Direction_Id);
        //if (Direction_Id == MasterLookup._Directions.AR) {
        //    LoadMakepaymentsWithFilters();
        //    MakePaymentsSearchPanelEvents();
        //}
        //else if (Direction_Id == MasterLookup._Directions.AP) {
        //    LoadMakepaymentsWithFilters();
        //}
        resetPaymentStore();
        LoadMakepaymentsWithFilters();
        MakePaymentsSearchPanelEvents();
        PartialInvoiceDetails = [];
    });

    $(document).off('change', '[name="OutstandingBalances"]')
    $(document).on('change', '[name="OutstandingBalances"]', function () {
        ClearloadedPages();
        var Direction_Id = $('[data-name="Direction_Id"]').val();
        if (Direction_Id == MasterLookup._Directions.AR) {
            MakePaymentsSearchPanelEvents();
        }
        else if (Direction_Id == MasterLookup._Directions.AP) {
            LoadMakepaymentsWithFilters();
        }
        PartialInvoiceDetails = [];
    });
}

function setAmountApplied($container, total) {
    var $agency = $container.closest('.agency');
    $agency.find('div[data-name="MakePaymentsAmountApplied"] strong').text('$' + total.toFixed(2));
    $agency.find('button[data-name="btnCreateBill"]').data('amount', total);
}


// Below Two Method for if we again Check the Invoice checkbox manually then if all checkbox'es are checked then it will check again Apply Check (CheckBox)
function recalcAmountApplied($container) {
    var total = 0;
    $container.find('.invoice-check:checked').each(function () {
        var val = parseFloat($(this).closest('tr').find('.amount-text').val()) || 0;
        total += val;
    });
    setAmountApplied($container, total);
}

function updateApplyAllStatus($container) {
    var $applyAll = $container.closest('.agency').find('.apply-all');
    var total = $container.find('.invoice-check').length;
    var checked = $container.find('.invoice-check:checked').length;

    $applyAll.prop('checked', total > 0 && total === checked);
}


function parseCurrency(value) {
    if (!value) return 0;
    return parseFloat(value.replace(/[$,]/g, '')) || 0;
}
function toggleCreateBill($agency) {
    var $header = $agency.children('.agency-details').find('[data-name="AgencyAccordion"]');
    var entityId = $header.attr('data-entity-id');
    var agentDetailId = $header.attr('data-agent-detail-id');

    // For State entities: entityId in DOM is numeric but store key is state abbrev.
    // Match via agentDetailId to find the correct store entry.
    if (entityId && (!paymentStore[entityId])) {
        // Try to find by agentDetailId in store
        var matchedKey = Object.keys(paymentStore).find(function (key) {
            return paymentStore[key].agentDetailId === agentDetailId;
        });
        if (matchedKey) entityId = matchedKey;
    }

    // Fallback: derive from container name (ground truth for State entities)
    if (!entityId || entityId === '0') {
        var $c = $agency.children('.multi-collapse')
            .find('[data-name^="MakePaymentsInvoiceContainer_"]');
        entityId = getEntityIdFromContainer($c);
    }

    if (!entityId) return;

    var hasInStore = paymentStore[entityId] && Object.keys(paymentStore[entityId].invoices).length > 0;

    var $ownContainer = $agency.children('.multi-collapse').find('[data-name^="MakePaymentsInvoiceContainer_"]');
    var anyCheckedInDom = $ownContainer.find('.invoice-check:checked').length > 0;

    var $btn = $agency.children('.agency-details').find('.btn-payment, button[data-name="btnCreateBill"]');
    // Add at the very top of toggleCreateBill, temporarily
    //console.log("toggleCreateBill called", $agency.find('.agency-name').first().text(), "hasInStore:", hasInStore, "anyCheckedInDom:", anyCheckedInDom, "entityId:", entityId, "paymentStore keys:", Object.keys(paymentStore));
    if (hasInStore || anyCheckedInDom) {
        $btn.show();
    } else {
        $btn.hide();
    }
}


$(document).off('change', '.apply-all');
$(document).on('change', '.apply-all', function () {
    var $applyAll = $(this);
    var Payee_Id = $applyAll.data('entity-id');
    var PayeeType_Id = $applyAll.data('entity-type-id');
    var EntityName = $applyAll.data('payee-name');
    var EntityTypeId = $('[data-name="EntityType_Id"]').val();
    var EntityId = $('[data-name="Entity_Id"]').val();
    var AmountBasedOnDue = $('[data-name="AmountDueBasedOnType_Id"]').val();
    var LineItem_Id = $('[data-name="LineItem_Id"]').val();
    var PolicyNumber = $('[data-name="PolicyNumber"]').val();
    var InsuredName = $('[data-name="InsuredName"]').val();
    var InvoiceNo = $('[data-name="InvoiceNo"]').val();
    var DirectionId = $('[data-name="Direction_Id"]').val();
    var OutStandingBalance = $('[name="OutstandingBalances"]').prop('checked');
    var PaymentBookedDate = $('[name="PaymentBookedDate"]').val();
    var InvoiceDate = $("#InvoiceDate").val();
    var lineItemIds = [];

    $("input[data-name='lineitem-options']:checked").each(function () {
        lineItemIds.push($(this).data("lineitem-id"));
    });
    //debugger;
    var $agency = $applyAll.closest('.agency');
    if (PayeeType_Id == 7) { // use == or ensure entityTypeId is a number
        if (EntityName) {
            EntityName = EntityName.replace(/^State of\s*/, '');
            Payee_Id = EntityName;
        }
    }
    var $container = $('[data-name="MakePaymentsInvoiceContainer_' + Payee_Id + '"]');

    if ($container.length === 0) return;

    var isChecked = this.checked;

    var $agency = $applyAll.closest('.agency');
    var $header = $agency.find('[data-name="AgencyAccordion"]');
    var agentDetailId = $header.data('agent-detail-id');
    var entityTypeName = $agency.find('.agency-details .agency-name').last().text().trim();
    var storeEntity = paymentStore[Payee_Id];
    if (storeEntity && storeEntity.entityTypeId) {
        EntityTypeId = storeEntity.entityTypeId; // override from store
    }

    appCommon.RenderHTMLPanel(
        "/MakePayments/InvoiceTable"
        + "?Payee_Id=" + Payee_Id
        + "&PayeeType_Id=" + PayeeType_Id
        + "&EntityType_Id=" + EntityTypeId
        + "&Entity_Id=" + EntityId
        + "&AmountDueBasedOnType_Id=" + AmountBasedOnDue
        + "&LineItem_Id=" + LineItem_Id
        + "&LineItemList=" + lineItemIds
        + "&PolicyNumber=" + PolicyNumber
        + "&InsuredName=" + InsuredName
        + "&InvoiceNo=" + InvoiceNo
        + "&OutstandingBalances=" + OutStandingBalance
        + "&Direction_Id=" + DirectionId
        + "&EntityName=" + EntityName
        + "&PaymentBookedDate=" + PaymentBookedDate
        + "&InvoiceDate=" + InvoiceDate,
        $container,
        function (response) {
            $container.css('min-height', '100px');

            var totalAmountApplied = 0;

            $container.find('.invoice-check').each(function () {
                var $checkbox = $(this);
                var $row = $checkbox.closest('tr');

                if ($row.hasClass('disabled-row')) {
                    return true;
                }

                var netDue = parseFloat($row.find('.amount-text').data('net-due')) || 0;
                //console.log("isChecked", isChecked);
                if (isChecked) {
                    $checkbox.prop('checked', true);
                    $row.find('.amount-text').val(netDue.toFixed(2)).prop('disabled', true);
                    totalAmountApplied += netDue;
                    storePayment($checkbox, $row);
                } else {
                    $checkbox.prop('checked', false);
                    $row.find('.amount-text').val('').prop('disabled', false);
                    removePayment($checkbox, $row);
                }
            });

            var totalEligibleRows = $container.find('.invoice-check').filter(function () {
                return !$(this).closest('tr').hasClass('disabled-row');
            }).length;
            if (!paymentStore[Payee_Id]) {
                paymentStore[Payee_Id] = { entityTypeId: PayeeType_Id, entityName: EntityName, entityTypeName: entityTypeName, agentDetailId: agentDetailId, invoices: {} };
            }
            paymentStore[Payee_Id]._totalRows = totalEligibleRows;

            // Update Amount Applied for this payee only
            var $amountDiv = $container.closest('.agency').find('div[data-name="MakePaymentsAmountApplied"] strong');
            $amountDiv.text('$' + totalAmountApplied.toFixed(2));
            toggleCreateBill($agency);
        },
        true
    );
});

// ----------------------
// Individual invoice checkbox logic
// ----------------------
$(document).off('change', '.invoice-check');
$(document).on('change', '.invoice-check', function () {
    var $checkbox = $(this);
    var $row = $checkbox.closest('tr');
    var $container = $checkbox.closest('[data-name^="MakePaymentsInvoiceContainer_"]');
    var $agency = $container.closest('.agency');  //single declaration, correct scope
    var $applyAll = $agency.children('.agency-details').find('.apply-all');

    storePayment($checkbox, $row);

    if (!$checkbox.is(':checked')) {
        $row.find('.amount-text').prop('disabled', false).val('');
        $applyAll.prop('checked', false);
    } else {
        var netDue = parseFloat($row.find('.amount-text').data('net-due')) || 0;
        $row.find('.amount-text').val(netDue.toFixed(2));
    }

    recalcAmountApplied($container);
    updateApplyAllStatus($container);

    var totalAmountApplied = 0;
    $container.find('.invoice-check:checked').each(function () {
        var val = parseFloat($(this).closest('tr').find('.amount-text').val()) || 0;
        totalAmountApplied += val;
    });

    $agency.children('.agency-details')
        .find('div[data-name="MakePaymentsAmountApplied"] strong')
        .text('$' + totalAmountApplied.toFixed(2));

    toggleCreateBill($agency);
});

$(document).on('input', '[data-name="MakePaymentsAmount"]', function () {
    let value = $(this).val();
    value = value.replace(/[^0-9\-\.]/g, '');
    if (value.indexOf('-') > 0) {
        value = value.replace('-', '');
    }
    const parts = value.split('.');
    if (parts.length > 2) {
        value = parts[0] + '.' + parts.slice(1).join('');
    }

    $(this).val(value);
});

$(document).on('focusout', '[data-name="MakePaymentsAmount"]', function () {
    var $input = $(this);
    var currentValue = $input.val();
    var numValue = parseFloat(currentValue) || 0;
    var invoiceLineitemId = $input.attr('data-invoice-lineitem-id');
    var $checkbox = $('[data-name="payment-checkBox"][data-invoice-lineitem-id="' + invoiceLineitemId + '"]');

    var netDue = parseFloat($input.data('net-due')) || 0;
    if (Math.sign(numValue) != Math.sign(netDue)) {
        appCommon.ShowMessageModal("Entered amount must have the same sign as Net Due.", "Error");
        if ($checkbox.is(':checked')) { $checkbox.prop('checked', false); $input.val(''); }
    }
    if (Math.abs(numValue) > Math.abs(netDue)) {
        appCommon.ShowMessageModal("Entered amount should be less than Net Due.", "Error");
        if ($checkbox.is(':checked')) { $checkbox.prop('checked', false); $input.val(''); }
    }

    // ✅ Derive from container, not checkbox
    var $container = $input.closest('[data-name^="MakePaymentsInvoiceContainer_"]');
    var $agency = $container.closest('.agency');

    setTimeout(() => {
        recalcAmountApplied($container);
        updateApplyAllStatus($container);
        toggleCreateBill($agency);
    }, 0);
});

function BindInvoiceLineItemAndAppliedAmount(btn, modal) {

    var entityId = btn.data('entity_id');
    var entityTypeId = btn.data('entitytype_id');
    var entityName = btn.data('entity-name');

    // State entities use state abbreviation as store key
    if (entityTypeId == 7 && entityName) {
        entityId = entityName.replace(/^State of\s*/, '').trim();
    }

    var selectedCount = 0;
    var totalAmount = 0;

    // Primary source: paymentStore
    if (paymentStore[entityId] && paymentStore[entityId].invoices && Object.keys(paymentStore[entityId].invoices).length > 0)
    {
        Object.values(paymentStore[entityId].invoices).forEach(function (invoice) {
            selectedCount++;
            totalAmount += parseFloat(invoice.amount) || 0;
        });
    }
    else {
        // Fallback: DOM
        var $container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');

        if ($container.length) {

            selectedCount = $container.find('.invoice-check:checked').length;

            $container.find('.invoice-check:checked').each(function () {
                var amount =
                    parseFloat(
                        $(this)
                            .closest('tr')
                            .find('.amount-text')
                            .val()
                    ) || 0;

                totalAmount += amount;
            });
        }
    }

    modal.find('[data-name="selectedLineItemCount"]').text(selectedCount);
    modal.find('[data-name="totalSelectedAmount"]').text('$' + totalAmount.toFixed(2));
}
function UpdateCommissionCheckbox(entityId, modal) {
    var enableCheckbox = false;
    var container = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]');
    if (container.length === 0) return;
    container.find('.invoice-check:checked').each(function () {

        var lineitemName = $(this).data('lineitemname');
        if (lineitemName.includes('Agency/Broker Commission')) {
            enableCheckbox = true;
            return false;
        }

    });

    var CommisionStatementcheckbox = modal.find('[data-name="SentComissionStatement"]');

    if (enableCheckbox) {
        CommisionStatementcheckbox.prop('disabled', false);
    }
    else {
        CommisionStatementcheckbox.prop('checked', false)
        CommisionStatementcheckbox.prop('disabled', true);
    }
}



function toggleCollapse(action) {
    $('.agency').each(function () {
        var $agency = $(this);
        var $accordionHeader = $agency.find('[data-name="AgencyAccordion"]');
        var $collapseBody = $agency.find('.multi-collapse');
        let bsCollapse = bootstrap.Collapse.getOrCreateInstance($collapseBody[0], { toggle: false });

        if (action === 'show') {
            if (!$accordionHeader.data('loaded')) {
                // Lazy-load with callback
                $accordionHeader.trigger('click', [function () {
                    bsCollapse.show();
                }]);
            } else {
                bsCollapse.show();
            }
        } else if (action === 'hide') {
            bsCollapse.hide();
        }
    });
}


$(document).off('click', '[data-name="btnExpandAll"]')
$(document).on('click', '[data-name="btnExpandAll"]', function () {
    toggleCollapse('show');
})

$(document).off('click', '[data-name="btnCollapseall"]')
$(document).on('click', '[data-name="btnCollapseall"]', function () {
    toggleCollapse('hide');
})

$('[data-name="lineitem-options"]').on("change", function () {
    updateLineItemList();
});
function updateLineItemList() {
    var selected = [];
    $('input[data-name="lineitem-options"]:checked').each(function () {
        selected.push($(this).data("lineitem-id"));
    });
    $('input[name="LineItemList"]').val(selected.join(","));
}



$(document).off('click', '[data-name="btnClearSelections"]');
$(document).on('click', '[data-name="btnClearSelections"]', function () {
    resetPaymentStore();
    LoadMakepaymentsWithFilters();
});

function toggleClearSelectionsButton() {
    var hasData = Object.keys(paymentStore).some(function (entityId) {
        return paymentStore[entityId] &&
            paymentStore[entityId].invoices &&
            Object.keys(paymentStore[entityId].invoices).length > 0;
    });

    var $btn = $('[data-name="btnClearSelections"]');

    if (hasData) {
        $btn.show();
    } else {
        $btn.hide();
    }
}

function formatCurrency(amount) {
    return '$' + (amount || 0).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}
function buildSelectedAgenciesHtml(responseHtml) {
    let html = '';

    // Step 1: find existing entityIds from response
    var $temp = $('<div>').html(responseHtml);
    let existingEntities = new Set();
    $temp.find('.agency-details').each(function () {
        let entityId = $(this).attr('data-entity-id') || '';
        let entityTypeId = $(this).attr('data-entity-type-id') || '';
        let payeeName = $(this).attr('data-payee-name') || '';
        let key = `${entityId}_${payeeName}_${entityTypeId}`;
        existingEntities.add(key);
    });

    //console.log("buildSelectedAgenciesHtml-existingEntities", existingEntities)

    // Step 2: build only missing entities from store
    Object.keys(paymentStore).forEach(entityId => {
        let entity = paymentStore[entityId];
        //console.log("buildSelectedAgenciesHtml", paymentStore)
        let invoices = Object.values(entity.invoices);
        if (invoices.length === 0) return;
        let lastInvoiceId = invoices.length > 0
            ? invoices[invoices.length - 1].invoiceId
            : null;

        // Applied Amount
        let appliedAmount = 0;
        let selectedCount = 0;

        Object.values(entity.invoices || {}).forEach(inv => {
            appliedAmount += parseFloat(inv.amount) || 0;
            selectedCount++;
        });

        // Total Due (fallback if missing)
        let totalDue = entity.totalDue || appliedAmount;

        // Apply checkbox logic
        let totalRows = entity._totalRows || 0;
        let agentDetailId = entity.agentDetailId;
        let entityTypeId = entity.entityTypeId;
        let tempEntityId = entityId;
        if (entity.entityTypeName == "State") {
            entityId = 0;
        }


        let currentKey = `${entityId}_${entity.entityName}_${entity.entityTypeId}`;

        if (existingEntities.has(currentKey)) {
            return;
        }

        if (entity.entityTypeName == "State") {
            entityId = tempEntityId;
        }

        html += `
        <div class="agency mb-3">
            <div class="agency-details d-flex justify-content-between" data-bs-toggle="collapse" data-bs-target="#agency-${agentDetailId}-${lastInvoiceId}-0" aria-expanded="false"
              aria-controls="agency-${agentDetailId}-${lastInvoiceId}-0" 
              "
              data-entity-id="${entityId}"
              data-entity-type-id="${entityTypeId}"
              data-payee-name="${entity.entityName}">

                <div class="d-flex align-items-center gap-2 flex-1" data-name="AgencyAccordion" 
                  "
                  data-entity-id="${entityId}"
                  data-entity-type-id="${entityTypeId}"
                  data-payee-name="${entity.entityName}">
                    <div class="collapse-icon"><i class="ri-arrow-up-s-line"></i></div>
                    Entity Name:<div class="agency-name">${entity.entityName}</div> &nbsp;
                    Entity Type:<div class="agency-name">${entity.entityTypeName}</div>
                </div>

                <div class="d-flex gap-3 align-items-center flex-1 justify-content-between info">
                    <div class="d-flex gap-2">
                        <div class="chip" data-name="totaldue"><span>Due:</span><strong class="text-danger">${formatCurrency(totalDue)}</strong></div>
                    </div>
                    
                    <div class="d-flex">
                        <div class="d-flex align-items-center" data-name="MakePaymentsAmountApplied"><span>Amount Applied:</span> <strong>${formatCurrency(appliedAmount)}</strong></div>
                    </div>

                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center">
                            <div class="form-check mb-0 prevent-toggle" data-bs-toggle="collapse"> <input class="form-check-input apply-all" type="checkbox" value="" data-entity-id="${entityId}" data-entity-type-id="${entityTypeId}" data-name="applyAll" data-netdue="${formatCurrency(totalDue)}"  data-payee-name="${entity.entityName}"> <label for="${entityId}">Apply All</label> 
                            </div>
                         </div>

                        <button href="javascript:void(0)" class="btn btn-primary btn-sm fw-medium fz-13 btn-payment prevent-toggle" data-bs-toggle="collapse" data-name="btnCreateBill" data-entity_id="${entityId}" 
                        data-entityType_id="${entityTypeId}" data-amount="${formatCurrency(appliedAmount)}" data-due="${formatCurrency(totalDue)}" data-entity-name ="${entity.entityName}" style=" display:none;">Create Bill</button> 
                    </div>
                </div>
            </div>
            <div class="collapse multi-collapse" id="entity-${entityTypeId}-${entityId}" >
                <div data-name="MakePaymentsInvoiceContainer_${entityId}" class="position-relative">
                    <div class="position-absolute transaction-table-skeleton" style="inset:0; z-index:9999; background:#fff; display:none;">
                        <div class="d-flex justify-content-center align-items-center" style="height:100px;"> </div>
                    </div>
                </div>
            </div>
        </div>`;
    });
    //console.log("html", html);
    return html;
}

function resetPaymentStore() {
    // Clear the store completely
    paymentStore = {};
    localStorage.removeItem("paymentStore");
    // Reset all UI across all visible agencies
    $('.agency').each(function () {
        var $agency = $(this);

        // Reset amount applied
        $agency.find('div[data-name="MakePaymentsAmountApplied"] strong')
            .text('$0.00');

        // Hide create bill button and reset amount
        var $btn = $agency.find('button[data-name="btnCreateBill"]');
        $btn.data('amount', 0);
        $btn.attr('data-amount', 0);
        $btn.hide();

        // Uncheck apply-all
        $agency.find('.apply-all').prop('checked', false);

        // Uncheck all invoice checkboxes and clear amounts
        $agency.find('.invoice-check').prop('checked', false);
        $agency.find('.amount-text').val('').prop('disabled', false);
    });
    updateGlobalSummaryInButtons();
    updateTooltipSummary();
    toggleClearSelectionsButton();
}

function updateTooltipSummary() {
    let total = 0;
    let count = 0;

    Object.values(paymentStore).forEach(entity => {
        Object.values(entity.invoices).forEach(inv => {
            total += inv.amount || 0;
            count++;
        });
    });

    $('button[data-name="btnCreateBill"]').attr(
        'title',
        `Selected: ${count} invoices | Total: $${total.toFixed(2)}`
    );
}

function updateGlobalSummaryInButtons() {
    let total = 0;
    let count = 0;

    Object.values(paymentStore).forEach(entity => {
        Object.values(entity.invoices).forEach(inv => {
            total += inv.amount || 0;
            count++;
        });
    });

    $('button[data-name="btnCreateBill"]').each(function () {
        $(this).attr(
            'title',
            `Selected: ${count} invoices | Total: $${total.toFixed(2)}`
        );
    });
}

function updateApplyAllFromStore($container) {
    var $agency = $container.closest('.agency');
    var $header = $agency.find('[data-name="AgencyAccordion"]');

    // FIX: use attr()
    var entityId = $header.attr('data-entity-id') || $header.attr('data-entity-Id');

    var totalRows = $container.find('.invoice-check').length;
    var storedCount = paymentStore[entityId]
        ? Object.keys(paymentStore[entityId].invoices).length
        : 0;

    $agency.find('.apply-all').prop('checked', totalRows > 0 && totalRows === storedCount);
}

function syncHeaderUIFromStore() {
    // PASS 1: Hide ALL buttons
    $('.agency').each(function () {
        $(this).children('.agency-details')
            .find('button[data-name="btnCreateBill"]')
            .hide();
    });

    // PASS 2: Show only correct ones via container name lookup
    Object.keys(paymentStore).forEach(function (entityId) {
        var entity = paymentStore[entityId];
        if (!entity || Object.keys(entity.invoices).length === 0) return;

        // Container name is ground truth — never use agentDetailId for DOM lookup
        var $cont = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]').first();
        if (!$cont.length) return;

        var $agency = $cont.closest('.agency');
        if (!$agency.length) return;

        var total = 0;
        var storedCount = Object.keys(entity.invoices).length;
        Object.values(entity.invoices).forEach(function (x) { total += x.amount || 0; });

        $agency.children('.agency-details')
            .find('div[data-name="MakePaymentsAmountApplied"] strong')
            .text('$' + total.toFixed(2));

        var $btn = $agency.children('.agency-details')
            .find('button[data-name="btnCreateBill"]');
        $btn.data('amount', total).attr('data-amount', total).show();

        var domRows = $cont.find('.invoice-check').filter(function () {
            return !$(this).closest('tr').hasClass('disabled-row');
        }).length;
        var totalRows = domRows > 0 ? domRows : (entity._totalRows || 0);

        $agency.children('.agency-details')
            .find('.apply-all')
            .prop('checked', totalRows > 0 && totalRows === storedCount);
    });
}

function syncUIFromStoreAfterFilter() {
    Object.keys(paymentStore).forEach(function (entityId) {
        var $agency = $('.agency').filter(function () {
            var id = $(this).find('[data-name="AgencyAccordion"]').attr('data-entity-Id');
            return id !== undefined && id !== '' && id === entityId;
        });

        if ($agency.length === 0) return;

        var invoices = paymentStore[entityId].invoices;

        let total = 0;
        let storedCount = Object.keys(invoices).length;

        Object.values(invoices).forEach(x => {
            total += x.amount || 0;
        });

        // Update Amount Applied
        $agency.find('div[data-name="MakePaymentsAmountApplied"] strong').text('$' + total.toFixed(2));

        // Update Create Bill button
        var $btn = $agency.find('button[data-name="btnCreateBill"]');
        $btn.data('amount', total);
        $btn.attr('data-amount', total); // important

        // Apply All Fix
        var $container = $agency.find('[data-name^="MakePaymentsInvoiceContainer_"]');

        if ($container.length > 0) {

            var totalRows = $container.find('.invoice-check').filter(function () {
                return !$(this).closest('tr').hasClass('disabled-row');
            }).length;

            $agency.find('.apply-all').prop('checked', totalRows > 0 && totalRows === storedCount);
        }

        // Toggle button
        toggleCreateBill($agency);
    });
}

function getExistingEntities() {
    var existing = new Set();

    $('[data-name="AgencyAccordion"]').each(function () {
        var payeeName = $(this).attr('data-payee-name');
        var entityTypeId = $(this).attr('data-entity-type-id');

        existing.add(entityTypeId + '_' + payeeName);
    });

    return existing;
}

function restoreSelections() {
    var processedContainers = new Map();
    $('.invoice-check').each(function () {
        var $checkbox = $(this);
        var $row = $checkbox.closest('tr');
        var $container = $checkbox.closest('[data-name^="MakePaymentsInvoiceContainer_"]');
        var $agency = $checkbox.closest('.agency');
        var $header = $agency.find('[data-name="AgencyAccordion"]');

        // FIX: use attr() not data()
        var entityId = $header.attr('data-entity-id') || $header.attr('data-entity-Id');
        if (!entityId) return;

        var invoiceId = $row.find('.amount-text').data('invoice-id');
        var lineItemId = $row.find('.amount-text').data('lineitem-id');
        var invoiceLineItemId = $row.find('.amount-text').data('invoice-lineitem-id');

        var key = `${entityId}_${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

        if (paymentStore[entityId] && paymentStore[entityId].invoices[key]) {
            var storedAmount = paymentStore[entityId].invoices[key].amount;
            $checkbox.prop('checked', true);
            $row.find('.amount-text').val(storedAmount);

            var totalRows = paymentStore[entityId]?._totalRows || 0;
            var selectedCount = Object.keys(paymentStore[entityId]?.invoices || {}).length;
            $row.find('.amount-text').prop('disabled', selectedCount === totalRows && totalRows > 0);

            processedContainers.set($container[0], $container);
        }
    });

    processedContainers.forEach(function ($container) {
        var $agency = $container.closest('.agency');
        var $header = $agency.find('[data-name="AgencyAccordion"]');

        // FIX: use attr() not data()
        var entityId = $header.attr('data-entity-id') || $header.attr('data-entity-Id');
        var totalEligibleRows = $container.find('.invoice-check').filter(function () {
            return !$(this).closest('tr').hasClass('disabled-row');
        }).length;

        if (paymentStore[entityId] && totalEligibleRows > 0) {
            paymentStore[entityId]._totalRows = totalEligibleRows;
        }

        updateEntityAmountApplied(entityId);
        updateApplyAllFromStore($container);
        toggleCreateBill($agency);
    });

    syncHeaderUIFromStore();
    updateGlobalSummaryInButtons();
    updateTooltipSummary();
    toggleClearSelectionsButton();
}

$(document).on('focusout', '.amount-text', function () {
    var $input = $(this);
    var $row = $input.closest('tr');
    var $checkbox = $row.find('.invoice-check');

    if (!$checkbox.is(':checked')) return;

    // ✅ Derive agency from the invoice container — ground truth
    var $container = $input.closest('[data-name^="MakePaymentsInvoiceContainer_"]');
    var $agency = $container.closest('.agency');
    var entityId = getEntityIdFromContainer($container);

    if (!entityId) return;

    var invoiceId = $input.data('invoice-id');
    var lineItemId = $input.data('lineitem-id');
    var invoiceLineItemId = $input.data('invoice-lineitem-id');

    var amount = parseFloat($input.val());
    amount = isNaN(amount) ? 0 : amount;

    var key = `${entityId}_${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

    if (paymentStore[entityId] && paymentStore[entityId].invoices[key]) {
        paymentStore[entityId].invoices[key].amount = amount;
    }

    updateEntityAmountApplied(entityId);
    updateGlobalSummaryInButtons();
    updateTooltipSummary();
});

function updateEntityAmountApplied(entityId) {
    var entity = paymentStore[entityId];

    // Container name is the ONLY reliable ground truth
    // data-name="MakePaymentsInvoiceContainer_TX" directly maps to store key "TX"
    var $cont = $('[data-name="MakePaymentsInvoiceContainer_' + entityId + '"]').first();
    if (!$cont.length) return;

    var $agency = $cont.closest('.agency');
    if (!$agency.length) return;

    if (!entity || Object.keys(entity.invoices).length === 0) {
        $agency.children('.agency-details')
            .find('div[data-name="MakePaymentsAmountApplied"] strong')
            .text('$0.00');
        $agency.children('.agency-details')
            .find('button[data-name="btnCreateBill"]')
            .data('amount', 0).attr('data-amount', 0);
        toggleCreateBill($agency);
        return;
    }

    var total = 0;
    Object.values(entity.invoices).forEach(function (x) { total += x.amount || 0; });

    $agency.children('.agency-details')
        .find('div[data-name="MakePaymentsAmountApplied"] strong')
        .text('$' + total.toFixed(2));

    $agency.children('.agency-details')
        .find('button[data-name="btnCreateBill"]')
        .data('amount', total).attr('data-amount', total);

    toggleCreateBill($agency);
}

function getEntityIdFromContainer($container) {
    var containerName = $container.attr('data-name') || '';
    // data-name="MakePaymentsInvoiceContainer_12345"
    return containerName.replace('MakePaymentsInvoiceContainer_', '').trim();
}

function storePayment($checkbox, $row) {
    var $container = $checkbox.closest('[data-name^="MakePaymentsInvoiceContainer_"]');
    var $agency = $container.closest('.agency'); // scope agency from container, not checkbox
    var $header = $agency.children('.agency-details').find('[data-name="AgencyAccordion"]');

    // Container name is the ground truth — especially for State entities
    // where Payee_Id was replaced with state name (e.g. "PA", "OR")
    var entityId = getEntityIdFromContainer($container);
    if (!entityId) return;

    // Get metadata from header attrs (but DON'T re-read entityId from here)
    var entityTypeId = $header.attr('data-entity-type-id') || $header.attr('data-entity-type-Id');
    var entityName = $header.attr('data-payee-name');

    var agentDetailId = $header.attr('data-agent-detail-id') || $header.attr('data-agent-detail-Id');
    var entityTypeName = $agency.children('.agency-details').find('.agency-name').last().text().trim();

    var invoiceId = $row.find('.amount-text').data('invoice-id');
    var lineItemId = $row.find('.amount-text').data('lineitem-id');
    var invoiceLineItemId = $row.find('.amount-text').data('invoice-lineitem-id');
    var invoiceNo = $row.find('td[name="invoice-No"]').text().trim();
    var $amountInput = $row.find('.amount-text');
    var amount = parseFloat($amountInput.val());

    if (isNaN(amount) || amount === 0) {
        var netDue = parseFloat($amountInput.data('net-due')) || 0;
        amount = netDue;
        $amountInput.val(netDue.toFixed(2));
    }

    var totalNetDue = 0;
    $agency.find('.amount-text').each(function () {
        totalNetDue += parseFloat($(this).data('net-due')) || 0;
    });

    var key = `${entityId}_${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

    if ($checkbox.is(':checked')) {

        if (!paymentStore[entityId]) {
            paymentStore[entityId] = {
                agentDetailId: agentDetailId,
                entityTypeId: entityTypeId,
                entityName: entityName,
                entityTypeName: entityTypeName,
                totalDue: totalNetDue,
                invoices: {}
            };
        }
        paymentStore[entityId].totalDue = totalNetDue;
        paymentStore[entityId].invoices[key] = { invoiceId, lineItemId, invoiceLineItemId, amount, invoiceNo };
    } else {
        if (paymentStore[entityId]) {
            delete paymentStore[entityId].invoices[key];
            if (Object.keys(paymentStore[entityId].invoices).length === 0) {
                delete paymentStore[entityId];
            }
        }
    }

    updateEntityAmountApplied(entityId);
    toggleCreateBill($agency);
    updateGlobalSummaryInButtons();
    updateTooltipSummary();
    toggleClearSelectionsButton();
    //console.log("storePayment", paymentStore);
}

function removePayment($checkbox, $row) {
    var $container = $checkbox.closest('[data-name^="MakePaymentsInvoiceContainer_"]');

    // Use container name as source of truth (handles State entities correctly)
    var entityId = getEntityIdFromContainer($container);

    var invoiceId = $row.find('.amount-text').data('invoice-id');
    var lineItemId = $row.find('.amount-text').data('lineitem-id');
    var invoiceLineItemId = $row.find('.amount-text').data('invoice-lineitem-id');

    var key = `${entityId}_${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

    if (paymentStore[entityId] && paymentStore[entityId].invoices[key]) {
        delete paymentStore[entityId].invoices[key];
    }
    toggleClearSelectionsButton();
}

function restoreSelectionsForContainer($container, entityId) {
    if (!paymentStore[entityId]) return;

    var invoices = paymentStore[entityId].invoices;
    if (Object.keys(invoices).length === 0) return;

    var checkedCount = 0;

    $container.find('.invoice-check').each(function () {
        var $checkbox = $(this);
        var $row = $checkbox.closest('tr');

        var invoiceId = $row.find('.amount-text').data('invoice-id');
        var lineItemId = $row.find('.amount-text').data('lineitem-id');
        var invoiceLineItemId = $row.find('.amount-text').data('invoice-lineitem-id');

        var key = `${entityId}_${invoiceId}_${lineItemId}_${invoiceLineItemId}`;

        if (invoices[key]) {
            $checkbox.prop('checked', true);
            $row.find('.amount-text')
                .val(invoices[key].amount);

            // disable ONLY if Apply All OR all rows selected
            var totalRows = paymentStore[entityId]._totalRows || 0;
            var selectedCount = Object.keys(invoices).length;

            if (selectedCount === totalRows && totalRows > 0) {
                $row.find('.amount-text').prop('disabled', true);
            } else {
                $row.find('.amount-text').prop('disabled', false);
            }

            checkedCount++;
        }
    });

    var totalRows = paymentStore[entityId]._totalRows || 0;

    // If ALL selected disable inputs
    if (checkedCount === totalRows && totalRows > 0) {
        $container.find('.amount-text').prop('disabled', true);
        $container.closest('.agency').find('.apply-all').prop('checked', true);
    }

    var $agency = $container.closest('.agency');
    toggleCreateBill($agency);
}

$(document).off('click', '[data-name="btnBatchBillView"]')
$(document).on('click', '[data-name="btnBatchBillView"]', function () {
    var billProcessId = $(this).data('id');
    var transactionDate = $(this).data('date');
    var targetURL = "/MakePayments/BillBatchProcessView?BillProcess_Id=" + billProcessId + "&date=" + transactionDate;
    appCommon.ExecuteAjaxGetMethod(targetURL, function (response) {
        if (response != null && response != undefined) {
            $('#BillBatchViewModel').find('.modal-body').html(response);
            var myModal = new bootstrap.Modal(document.getElementById('BillBatchViewModel'));
            myModal.show();

        }
    }, true)
})
$(document).off('click', '[data-name="btnBatchBill"]')
$(document).on('click', '[data-name="btnBatchBill"]', function () {
    appCommon.ExecuteAjaxGetMethod("/MakePayments/BillBatchProcess", function (response) {
        if (response != null && response != undefined) {
            appCommon.GenericModalFunction("Bill Batch Processing", response);
            $('[data-name="GenericModal"]').find(".modal-dialog").css("--bs-modal-width", "1250px");
        }
    }, true)
})


$(document).off('click', '[data-name = "btnViewServiceResponseMsg"]')
$(document).on('click', '[data-name="btnViewServiceResponseMsg"]', function () {
    var ServiceresponseMsg = $(this).attr('data-service-response-msg');
    if (ServiceresponseMsg != null && ServiceresponseMsg != "") {
        var jsonObject = JSON.parse(ServiceresponseMsg);
        var formattedJsonString = JSON.stringify(jsonObject, null, 4);
        $('#preContent').text(formattedJsonString);
        var myModal = new bootstrap.Modal(document.getElementById('responseMessageModal'));
        myModal.show();
    }
})

$(document).off('click', '[data-name = "CopyButton"]')
$(document).on('click', '[data-name="CopyButton"]', function () {
    var textCopy = $("#preContent").text();
    var $copiedMessage = $('[data-name="CopyRequestResponse"]');
    navigator.clipboard.writeText(textCopy).then(function () {
        $copiedMessage.show();
        setTimeout(function () {
            $copiedMessage.hide();
        }, 2000);
    })
})