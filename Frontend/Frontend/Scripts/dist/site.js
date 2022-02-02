import { Session } from "./session";
import { SessionLogin } from "./login";
import { SessionRegister } from "./signUp";
console.log("Initialize");
var session = new Session("https://localhost:7119");
function forceLogin() {
    /*
    session.Register("Hopfel", "draconity@netliz.net", "1234", () => {
        session.Login("draconity@netliz.net", "1234", () => {
        });
    });
    */
}
var token = localStorage.getItem("token");
if (token != null) {
    session.Validate(token, function (success) {
        if (!success) {
            forceLogin();
        }
    });
}
else {
    forceLogin();
}
// Global functions
window.Username = function () {
    return session.GetUsername();
};
window.Login = function () {
    SessionLogin(session);
};
window.Logout = function () {
    session.Logout(function () { });
};
window.Register = function () {
    SessionRegister(session);
};
