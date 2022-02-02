import { Session } from "./engine/api/session"
import { hash, delay, setValidity } from "./engine/api/utils"

function validateUsername(): boolean {
    let username = $('#signupUsername').val().toString();
    if (username.length < 3) {
        setValidity('#signupUsername', false);
        return false;
    }
    setValidity('#signupUsername', true);
    return true;
}

function validateEmail(): boolean {
    let email = $('#signupEmail').val().toString();
    let empattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    if (!email.toLowerCase().match(empattern)) {
        setValidity('#signupEmail', false);
        return false;
    }
    setValidity('#signupEmail', true);
    return true;
}

function validatePassword(): boolean {
    let password = $('#signupPassword').val().toString();
    let pwpattern = /^^(?=.*\d)(?=.*[!\"#$%&'()*+,\-./:;<=>?@@\[\\\]^_`{|}~])(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$/
    if (!password.match(pwpattern)) {
        setValidity('#signupPassword', false);
        setValidity('#signupConfirm', false);
        return false;
    }
    setValidity('#signupPassword', true);
    return validateMatching();
}

function validateMatching(): boolean {
    if ($('#signupPassword').val() != $('#signupConfirm').val()) {
        setValidity('#signupConfirm', false);
        return false;
    }
    setValidity('#signupConfirm', true);
    return true;
}

export function SessionRegister(session:Session) {

    $('#signUpErrorAlert').attr('hidden', 'true');

    if (!$('signupForm').hasClass('was-validated')) {
        $('#signupEmail').on("input", validateEmail);
        $('#signupUsername').on("input", validateUsername);
        $('#signupPassword').on("input", validatePassword);
        $('#signupConfirm').on("input", validateMatching);

        $('signupForm').addClass('was-validated');
    }

    let isValid = true;
    isValid = validateUsername() && isValid;
    isValid = validateEmail() && isValid;
    isValid = validatePassword() && isValid;

    if (isValid) {
        let username = $('#signupUsername').val().toString();
        let email = $('#signupEmail').val().toString();
        let password = $('#signupPassword').val().toString();

        $('#signup :input').attr('disabled', 'disabled');
        $('#signUpWaitAlert').removeAttr('hidden');

        let hashed = "";
        delay(500)
            .then(() => hash(password))
            .then((result) => {
                hashed = result;
                console.log(`hashed: ${hashed}`);
                return session.Register(username, email, hashed);
            })
            .then(() => session.Login(email, hashed)
                .then(() => {
                    location.reload();
                }, () => {
                    // User can try again on login screen
                    $('#loginEmail').val(email);
                    $('#loginPassword').val(password);
                    $('#login-tab').click();
                }))
            .catch((reason) => {
                $('#signUpWaitAlert').attr('hidden', 'true');
                $('#signUpErrorAlert').removeAttr('hidden');
                $('#signup :input').removeAttr('disabled');
            });
    }
}