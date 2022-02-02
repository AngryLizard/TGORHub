import { hash, delay, setValidity, clearInputValidation } from "./utils";
function validateUsername() {
    var username = $('#signupUsername').val().toString();
    if (username.length < 3) {
        setValidity('#signupUsername', false);
        return false;
    }
    setValidity('#signupUsername', true);
    return true;
}
function validateEmail() {
    var email = $('#signupEmail').val().toString();
    var empattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (!email.toLowerCase().match(empattern)) {
        setValidity('#signupEmail', false);
        return false;
    }
    setValidity('#signupEmail', true);
    return true;
}
function validatePassword() {
    var password = $('#signupPassword').val().toString();
    var pwpattern = /^^(?=.*\d)(?=.*[!\"#$%&'()*+,\-./:;<=>?@@\[\\\]^_`{|}~])(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$/;
    if (!password.match(pwpattern)) {
        setValidity('#signupPassword', false);
        setValidity('#signupConfirm', false);
        return false;
    }
    setValidity('#signupPassword', true);
    return validateMatching();
}
function validateMatching() {
    if ($('#signupPassword').val() != $('#signupConfirm').val()) {
        setValidity('#signupConfirm', false);
        return false;
    }
    setValidity('#signupConfirm', true);
    return true;
}
export function SessionRegister(session) {
    $('#signUpErrorAlert').attr('hidden', 'true');
    if (!$('signupForm').hasClass('was-validated')) {
        $('#signupEmail').on("input", validateEmail);
        $('#signupUsername').on("input", validateUsername);
        $('#signupPassword').on("input", validatePassword);
        $('#signupConfirm').on("input", validateMatching);
        $('signupForm').addClass('was-validated');
    }
    var isValid = true;
    isValid = validateUsername() && isValid;
    isValid = validateEmail() && isValid;
    isValid = validatePassword() && isValid;
    if (isValid) {
        var username_1 = $('#signupUsername').val().toString();
        var email_1 = $('#signupEmail').val().toString();
        var password_1 = $('#signupPassword').val().toString();
        $('#signup :input').attr('disabled', 'disabled');
        $('#signUpWaitAlert').removeAttr('hidden');
        clearInputValidation('#signupUsername');
        clearInputValidation('#signupEmail');
        clearInputValidation('#signupPassword');
        clearInputValidation('#signupConfirm');
        delay(2000).then(function () { return hash(password_1).then(function (hashed) {
            session.Register(username_1, email_1, hashed, function (success) {
                if (success) {
                    session.Login(email_1, hashed, function (success) {
                        $('#signupUsername').val("");
                        $('#signupEmail').val("");
                        $('#signupPassword').val("");
                        $('#signupConfirm').val("");
                        $('signupForm').removeClass('was-validated');
                        if (!success) {
                            // User can try again
                            $('#loginEmail').val(email_1);
                            $('#loginPassword').val(password_1);
                        }
                        $('#signUpWaitAlert').attr('hidden', 'true');
                        $('#signup :input').removeAttr('disabled');
                        // Switch to login screen
                        $('#login-tab').click();
                    });
                }
                else {
                    $('#signUpErrorAlert').removeAttr('hidden');
                }
            });
        }); });
    }
}
