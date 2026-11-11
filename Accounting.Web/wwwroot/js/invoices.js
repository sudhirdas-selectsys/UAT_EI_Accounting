$(function () {
    appCommon.UpdateBrowserURLHistory("/PolicyDetails/Index/", PolicyDetails_Id > 0 ? [PolicyDetails_Id] : '');
    document.title = "Policy Details";
    appCommon.MakeLinkActive($('[data-name="InvoicesLeftNav"]'));
    BindEvents();
});
function BindEvents() {
    $('.paid-checkbox').change(function () {
        var $accordion = $(this).closest('.accordion-item');
        var isChecked = $(this).prop('checked');
        $accordion.find('.row-checkbox').prop('checked', isChecked);
        var $paidTextContainer = $accordion.find('.checkbox-container');
        $paidTextContainer.find('.paid-label').remove();
        if (isChecked) {
            $paidTextContainer.append('<span class="paid-label ps-1" style="color: #6f4ef6; font-weight: 600;"> Paid</span>');
        }
    });

    $('.row-checkbox').change(function () {
        var $accordion = $(this).closest('.accordion-item');
        var allChecked = $accordion.find('.row-checkbox').length === $accordion.find('.row-checkbox:checked').length;
        $accordion.find('.paid-checkbox').prop('checked', allChecked);
        if (!allChecked) {
            $accordion.find('.checkbox-container .paid-label').remove();
        }
    });
    //$('[data-name="DownloadInvoice"]').off('click');
    //$('[data-name="DownloadInvoice"]').on('click', function () {
    //    var PolicyDetails_Id = $(this).attr('data-policydetails-id');
    //    var InstallmentNo = $(this).attr('data-installmentNo');
    //    if (!PolicyDetails_Id || PolicyDetails_Id === 'undefined') {
    //        return;
    //    }
    //    appCommon.ShowLoader();
    //    window.location.href = "/Invoices/GenerateInvoicePdf/" + PolicyDetails_Id + "?InstallmentNo=" + InstallmentNo;
    //    setTimeout(function () {
    //        appCommon.HideLoader();
    //    }, 5000);
    //});
    $('[data-name="GenerateInvoice"]').off('click');
    $('[data-name="GenerateInvoice"]').on('click', function () {
        var PolicyDetails_Id = $(this).attr('data-policydetails-id');
        var InstallmentNo = $(this).attr('data-installmentNo');

        appCommon.ExecuteAjaxGetMethod("/Invoices/GenerateInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (response) {
            if (response != null && response != undefined) {
                appCommon.GenericModalFunction("Invoice", response);
                $('[data-name="ModalGeneral"]').find('.modal-dialog');
                BindGenerateInvoiceEvents();
            }

        }, true);
    });
    $('[data-name="DownloadInvoice"]').off('click');
    $('[data-name="DownloadInvoice"]').on('click', function () {
        var InstallmentNo = $(this).attr('data-installmentNo');
        appCommon.ExecuteAjaxGetMethod("/Invoices/DownloadInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (response) {
            if (typeof response != "undefined" && response != null) {
                appCommon.DownloadBase64File(response.Data);
            }
        }, true);
    });
}
function BindGenerateInvoiceEvents() {
    $('[data-name="downloadPdf"]').on("click", function (event) {
        
        var InstallmentNo = $(this).attr('data-installmentNo');
        appCommon.ExecuteAjaxGetMethod("/Invoices/DownloadInvoice?PolicyDetails_Id=" + PolicyDetails_Id + "&InstallmentNo=" + InstallmentNo, function (response) {
            if (typeof response != "undefined" && response != null) {
                appCommon.DownloadBase64File(response.Data);
            }
        }, true);
    });
}