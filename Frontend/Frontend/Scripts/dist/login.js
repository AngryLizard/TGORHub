import { hash, delay, setValidity, clearInputValidation } from "./utils";
function validateEmail() {
    var email = $('#loginEmail').val().toString();
    if (email.length == 0) {
        setValidity('#loginEmail', false);
        return false;
    }
    setValidity('#loginEmail', true);
    return true;
}
function validatePassword() {
    var password = $('#loginPassword').val().toString();
    if (password.length == 0) {
        setValidity('#loginPassword', false);
        return false;
    }
    setValidity('#loginPassword', true);
    return true;
}
export function SessionLogin(session) {
    $('#loginErrorAlert').attr('hidden', 'true');
    if (!$('loginForm').hasClass('was-validated')) {
        $('#loginEmail').on("input", validateEmail);
        $('#loginPassword').on("input", validatePassword);
        $('signupForm').addClass('was-validated');
    }
    var isValid = true;
    isValid = validateEmail() && isValid;
    isValid = validatePassword() && isValid;
    if (isValid) {
        var email_1 = $('#loginEmail').val().toString();
        var password_1 = $('#loginPassword').val().toString();
        $('#login :input').attr('disabled', 'disabled');
        $('#loginWaitAlert').removeAttr('hidden');
        clearInputValidation('#loginEmail');
        clearInputValidation('#loginPassword');
        $('loginForm').removeClass('was-validated');
        delay(1500).then(function () { return hash(password_1).then(function (hashed) {
            session.Login(email_1, hashed, function (success) {
                $('#loginWaitAlert').attr('hidden', 'true');
                $('#login :input').removeAttr('disabled');
                if (success) {
                    $('#loginEmail').val("");
                    $('#loginPassword').val("");
                }
                else {
                    $('#loginErrorAlert').removeAttr('hidden');
                }
            });
        }); });
    }
    $('#login-tab').click();
}
