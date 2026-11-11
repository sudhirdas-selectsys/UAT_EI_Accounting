$(function () {

    var _enumOTPtype = { Email: 'Email', Phone: 'Phone' };
    var _resendOTPtimeSeconds = 60;
    $('[data-name="submitlogin"]').on('click', function () {
        var form = $('[data-name="loginform"]');
        if (appCommon.IsValidForm(form)) {
            appCommon.SubmitForm(form, function (response) {
                if (response != null && typeof response != 'undefined') {
                    if (response.Status && response.IsTrustedDevice)
                    {
                        window.location.href = '/';
                    }
                    else if (response.Status && !response.IsTrustedDevice)
                    {
                        appCommon.RenderHTMLPanel('/Login/MFAoption', $('[data-name="LoginContainer"]'), null, true);
                    }
                    else
                    {
                        if (response.Message != null && response.Message != '') {
                            appCommon.ShowMessageModal(response.Message,'Failed');
                        }
                        else {
                            appCommon.ShowMessageModal("Unable to process the request.",'Failed')
                        }
                    }
                }
            }, "/Login/Index",true,true);
        }
    });

    $(document).on('click', '[data-name="btnmail"]', function (e) {
        e.preventDefault();
        sendOtp(_enumOTPtype.Email, false);
    });

    $(document).on('click', '[data-name="btnresendotp"]', function (e) {
        e.preventDefault();
        sendOtp(_enumOTPtype.Email, true);
    });

    $(document).on('click', '[data-name="btnbacklogin"]', function () {
        appCommon.RenderHTMLPanel( 
            `/Login/Index`,
            $('[data-name="LoginContainer"]'),
            null,
            null
        );
    })
    function sendOtp(type, isResend) {
        if (!type) {
            appCommon.ShowMessageModal('Invalid OTP method.');
            return;
        }
        const queryString = appCommon.BuildQueryStringParameters({ method: type });

        appCommon.ShowLoader();

        try {
            appCommon.ExecuteAjaxPostMethod('/Login/SendOtp?' + queryString, null,
                function (response) {
                    appCommon.HideLoader();

                    if (response && response.Status) {
                        appCommon.RenderHTMLPanel(`/Login/OtpVerfication?` + queryString, $('[data-name="LoginContainer"]'), null, null);
                           setTimeout(function () { startCounter(_resendOTPtimeSeconds); }, 500);
                        

                        if (isResend) {
                            appCommon.ShowMessageModal(response.Message || 'OTP resent successfully.','Success');
                        }
                    } else {
                        appCommon.ShowMessageModal((response && response.Message) ? response.Message : 'Failed to send OTP.','Failed');
                    }
                },
                function (err) {
                    appCommon.HideLoader();
                    appCommon.ShowMessageModal('Network error while sending OTP. Please try again.');
                    console.error('SendOtp error', err);
                }
            ,true);
        } catch (ex) {
            appCommon.HideLoader();
            appCommon.ShowMessageModal('Unexpected error while sending OTP. Please try again.');
            console.error('SendOtp unexpected error', ex);
        }
    }

    var otpCounterInterval = null;
    function startCounter(seconds) {

        // Clear old counter (important on resend)
        if (otpCounterInterval) {
            clearInterval(otpCounterInterval);
            otpCounterInterval = null;
        }

        var remaining = seconds;

        $('#counterText').show();       

        $('[data-name="OtpTimer"]').text(remaining);

        otpCounterInterval = setInterval(function () {

            remaining--;

            if (remaining <= 0) {
                clearInterval(otpCounterInterval);
                otpCounterInterval = null;

                $('#counterText').hide();
                $('#btnResendOtp').removeClass('d-none');
                return;
            }

            $('[data-name="OtpTimer"]').text(remaining);

        }, 1000);
    }

    $(document).on('keyup', '.otp-input', function (e) {

        const $inputs = $('.otp-input');
        const index = $inputs.index(this);
        const key = e.key;

        // Allow only digits
        if (this.value.length > 1) {
            this.value = this.value.slice(0, 1);
        }

        // Move forward on digit
        if (/^[0-9]$/.test(key)) {
            if (index < $inputs.length - 1) {
                $inputs.eq(index + 1).focus();
            }
        }

        // Move backward on backspace
        if (key === 'Backspace') {
            if (this.value === '' && index > 0) {
                $inputs.eq(index - 1).focus();
            }
        }
    });

    // Handle paste (paste 6-digit OTP at once) 
    $(document).on('paste', '.otp-input', function (e)
    {
        e.preventDefault();
        const pasteData = (e.originalEvent || e).clipboardData.getData('text');
        const digits = pasteData.replace(/\D/g, '').slice(0, 6);
        $('.otp-input').each(function (index)
        {
            $(this).val(digits[index] || '');
        });

        $('.otp-input').eq(digits.length - 1).focus();
    });

    $(document).on('click', '[data-name="btnverifyotp"]', function (e) {
        e.preventDefault();

        var otp = '';
        $('.otp-input').each(function () {
            otp += $(this).val();
        });

        if (otp.length !== 6) {
            appCommon.ShowMessageModal('Please enter the 6-digit OTP.');
            return false;
        }
        $('[data-name="LoginContainer"]').find('[data-name="Otp"]').val(otp);
        const $form = $('[data-name="formverifyotp"]');
        if (appCommon.IsValidForm($form)) {
            appCommon.ShowLoader();
            appCommon.SubmitForm($form, function (ServiceStatus) {
                appCommon.HideLoader();

                if (ServiceStatus && ServiceStatus.Status) { 

                     appCommon.RenderHTMLPanel('/Login/TrustedDevice',$('[data-name="LoginContainer"]'));
                            
                }
                else {
                    appCommon.ShowMessageModal(ServiceStatus?.Message || 'Invalid or expired OTP.');
                }
            }, '/Login/VerifyOtp', true, false);
        }
        return false;
    });

    $(document).on('click', '[data-name="btnTrustedDevice"]', function (e) {
        e.preventDefault();
        appCommon.ExecuteAjaxPostMethod('/Login/SaveTrustedDevice', null,function (response) {
                if (response && response.Status) {
                    appCommon.ShowMessageModal(response.Message, 'Success');
                    window.location.href = '/';
                }
                else {
                    appCommon.ShowMessageModal(response.Message, 'Failure');
                }
            },true);
    });

    $(document).on('click', '#showForgotPassword', function () {
        $('#loginSection').addClass('d-none');
        $('#forgotPasswordSection').removeClass('d-none');
    });

    $(document).on('click', '#cancelForgetPassword', function () {
        $('#loginSection').removeClass('d-none');
        $('#forgotPasswordSection').addClass('d-none');

        $('#Username').val("");
        $('#Password').val("");
    });

    $(document).on('click', '[data-name="formSubmitPassword"]', function (e) {
        e.preventDefault();
        var form = $('[data-name="form"]');
        if (appCommon.IsValidForm(form)) {
            appCommon.SubmitForm(form, function (response) {
                if (response && response.Status) {
                    sendPasswordVerificationOtp(_enumOTPtype.Email, false);
                } else {
                    appCommon.ShowMessageModal("Please Check the Username.");
                }
            }, "/Login/ForgotPassword", false, true);
        }
    });

    $(document).on('click', '[data-name="formSubmit"]', function (e) {
        e.preventDefault();
        var form = $(this).closest('[data-name="form"]');
        var password = form.find('input[name="Password"]').val();
        var confirmPassword = form.find('input[name="ConfirmPassword"]').val();
        if (!password || !confirmPassword) {
            appCommon.ShowMessageModal("Password and Confirm Password are required.");
            return;
        }
        if (password !== confirmPassword) {
            appCommon.ShowMessageModal("Password and Confirm Password do not match.");
            return;
        }
        if (password.length < 8) {
            appCommon.ShowMessageModal("Password must be at least 8 characters.");
            return;
        }
        if (appCommon.IsValidForm(form)) {
            appCommon.SubmitForm(form, function (response) {
                if (response && response.Status) {
                    appCommon.ShowMessageModal('Password Updated Successfully!', 'Success');
                    window.location.href = '/Login/Index';
                }
                else {
                    appCommon.ShowMessageModal("Unable to process the request.");
                }

            }, "/Login/NewPassword", true, true);
        }
    });

    const params = new URLSearchParams(window.location.search);
    if (params.get('showForgotPassword') === 'true') {
        $('#loginSection').hide();
        $('#forgotPasswordSection').show();
        params.delete('showForgotPassword');
        const newUrl = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
        window.history.replaceState({}, document.title, newUrl);
    }

    function sendPasswordVerificationOtp(type, isResend) {
        if (!type) {
            appCommon.ShowMessageModal('Invalid OTP method.');
            return;
        }
        const queryString = appCommon.BuildQueryStringParameters({ method: type });

        appCommon.ShowLoader();

        try {
            appCommon.ExecuteAjaxPostMethod('/Login/SendOtp?' + queryString, null,
                function (response) {
                    appCommon.HideLoader();

                    if (response && response.Status) {
                        appCommon.RenderHTMLPanel(`/Login/PasswordVerification?` + queryString, $('[data-name="Containerbody"]'), null, null);
                        setTimeout(function () { startCounterPasswordVerification(_resendOTPtimeSeconds); }, 500);

                        if (window.history.replaceState) {
                            window.history.replaceState({}, document.title, window.location.pathname);
                        }

                        if (isResend) {
                            appCommon.ShowMessageModal(response.Message || 'OTP resent successfully.', 'Success');
                        }
                    } else {
                        appCommon.ShowMessageModal((response && response.Message) ? response.Message : 'Failed to send OTP.', 'Failed');
                    }
                },
                function (err) {
                    appCommon.HideLoader();
                    appCommon.ShowMessageModal('Network error while sending OTP. Please try again.');
                    console.error('SendOtp error', err);
                }
                , true);
        } catch (ex) {
            appCommon.HideLoader();
            appCommon.ShowMessageModal('Unexpected error while sending OTP. Please try again.');
            console.error('SendOtp unexpected error', ex);
        }
    }

    var otpCounterIntervalPasswordVerification = null;
    function startCounterPasswordVerification(seconds) {

        // Clear old counter (important on resend)
        if (otpCounterIntervalPasswordVerification) {
            clearInterval(otpCounterIntervalPasswordVerification);
            otpCounterIntervalPasswordVerification = null;
        }

        var remaining = seconds;

        $('#counterTextPasswordVerification').show();

        $('[data-name="OtpTimerPasswordVerification"]').text(remaining);

        otpCounterIntervalPasswordVerification = setInterval(function () {

            remaining--;

            if (remaining <= 0) {
                clearInterval(otpCounterIntervalPasswordVerification);
                otpCounterIntervalPasswordVerification = null;

                $('#counterTextPasswordVerification').hide();
                $('#btnResendOtpPasswordVerification').removeClass('d-none');
                return;
            }

            $('[data-name="OtpTimerPasswordVerification"]').text(remaining);

        }, 1000);
    }

    $(document).on('click', '[data-name="verifyotppasswordverification"]', function (e) {
        e.preventDefault();

        var otp = '';
        $('.otp-input').each(function () {
            otp += $(this).val();
        });

        if (otp.length !== 6) {
            appCommon.ShowMessageModal('Please enter the 6-digit OTP.');
            return false;
        }

        $('[data-name=OtpPasswordVerification]').val(otp);

        var $form = $('[data-name="formverifyotppasswordverification"]');

        if (appCommon.IsValidForm($form)) {

            appCommon.ShowLoader();

            appCommon.SubmitForm($form, function (response) {

                appCommon.HideLoader();

                if (response && response.Status) {
                    appCommon.RenderHTMLPanel(`/Login/NewPassword`, $('[data-name="Containerbody"]'), null, null);
                }
                else {
                    appCommon.ShowMessageModal(response?.Message || "Invalid or expired OTP.");
                }

            }, "/Login/VerifyOtp", true, false);
        }

        return false;
    });

    $(document).on('click', '[data-name="btnResendOtpPasswordVerification"]', function (e) {
        e.preventDefault();
        sendPasswordVerificationOtp(_enumOTPtype.Email, true);
    });

    $(document).on('click', '[data-name="btnUpdatePassword"]', function (e) {
        e.preventDefault();

        window.location.href = '/Login/Index?flow=UpdatePassword';
    });
    var initialFlow = $('[data-name="LoginContainer"]').data('flow');
    if (initialFlow === 'UpdatePassword') {
        sendPasswordVerificationOtp(_enumOTPtype.Email, false);
    }
});