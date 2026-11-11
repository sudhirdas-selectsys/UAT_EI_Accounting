$(function () {
    appCommon.UpdateBrowserURLHistory("/PolicyDetails/Index/", PolicyDetails_Id > 0 ? [PolicyDetails_Id] : '');
    document.title = "Policy Details";
    appCommon.MakeLinkActive($('[data-name="PolicyDetailsLeftNav"]'));
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
    BindLeftPanelPolicyDetailsClickEvents();
});
function BindLeftPanelPolicyDetailsClickEvents() {
    $('[data-name="btntab"]').off('click');
    $('[data-name="btntab"]').on('click', function () {
        var _tabName = $(this).attr('data-tab-name');
        var _policyId = $(this).attr('data-policy-id');

        var tabPaths = {
            generalInfo: "/PolicyDetails/GeneralInfo",
            agents: "/PolicyDetails/AgentsDetails",
            carriers: "/PolicyDetails/CarriersDetails",
            insureds: "/PolicyDetails/InsuredsDetails",
            finances: "/PolicyDetails/FinancesDetails",
            underwriters: "/PolicyDetails/UnderwritersDetails",
            interests: "/PolicyDetails/InterestsDetails"
        };
        if (tabPaths[_tabName]) {
            var url = tabPaths[_tabName] + "/" + _policyId;
            appCommon.RenderHTMLPanel(url, $('[data-name="policyDetailsTabContainer"]'), function () { }, true);
        }
    });
}