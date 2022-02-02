import { Session } from "./engine/api/session"
import { hash, delay, setValidity} from "./engine/api/utils"

function validateEmail(): boolean {
    let email = $('#loginEmail').val().toString();
    if (email.length == 0) {
        setValidity('#loginEmail', false);
        return false;
    }
    setValidity('#loginEmail', true);
    return true;
}

function validatePassword(): boolean {
    let password = $('#loginPassword').val().toString();
    if (password.length == 0) {
        setValidity('#loginPassword', false);
        return false;
    }
    setValidity('#loginPassword', true);
    return true;
}

export function SessionLogin(session: Session) {

    $('#loginErrorAlert').attr('hidden', 'true');

    if (!$('loginForm').hasClass('was-validated')) {
        $('#loginEmail').on("input", validateEmail);
        $('#loginPassword').on("input", validatePassword);

        $('signupForm').addClass('was-validated');
    }

    let isValid = true;
    isValid = validateEmail() && isValid;
    isValid = validatePassword() && isValid;

    if (isValid) {
        let email = $('#loginEmail').val().toString();
        let password = $('#loginPassword').val().toString();

        $('#login :input').attr('disabled', 'disabled');
        $('#loginWaitAlert').removeAttr('hidden');

        delay(500)
            .then(() => hash(password))
            .then((hashed) => session.Login(email, hashed))
            .then(() => { location.reload(); })
            .catch((reason) => {
                $('#loginWaitAlert').attr('hidden', 'true');
                $('#login :input').removeAttr('disabled');
                $('#loginErrorAlert').removeAttr('hidden');
            });
    }
}
