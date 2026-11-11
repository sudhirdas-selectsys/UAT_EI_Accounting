$(document).ready(function () {
    BindLeftPanelClickEvents();
    $('.menu-toggle-icon').on('click', function () {
        $('body').toggleClass('menu-expand');
    });
    $('.menu-close-icon').on('click', function () {
        $('body').toggleClass('menu-expand');
    });
})
function BindLeftPanelClickEvents() {
    $('[data-name="PolicyDetailsLeftNav"]').off('click');
    $('[data-name="PolicyDetailsLeftNav"]').on('click', function () {
        _AppMainPanel.LoadPolicyDetails(PolicyDetails_Id);
    });
    $('[data-name="InvoicesLeftNav"]').off('click');
    $('[data-name="InvoicesLeftNav"]').on('click', function () {
        _AppMainPanel.LoadInvoices(PolicyDetails_Id);
    });
}