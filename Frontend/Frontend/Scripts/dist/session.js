var Session = /** @class */ (function () {
    function Session(url) {
        this._username = "";
        this._token = "";
        this._apiUrl = url + /api/;
    }
    Session.prototype.Register = function (username, email, password, callback) {
        var data = {
            Username: username,
            Email: email,
            Password: password
        };
        this.Put("Account/Register", data, function (success) {
            window.dispatchEvent(new Event("register"));
            callback(success);
        });
    };
    Session.prototype.GetUsername = function () {
        return this._username;
    };
    Session.prototype.Validate = function (token, callback) {
        var _this = this;
        this._token = token;
        this._username = "";
        this.Get("Account/Validate", function (success, result) {
            if (success) {
                window.dispatchEvent(new Event("login"));
                callback(true);
            }
            else {
                _this._token = result['token'];
                _this._username = result['username'];
                window.dispatchEvent(new Event("logout"));
                callback(false);
            }
        });
    };
    Session.prototype.Login = function (email, password, callback) {
        var _this = this;
        var data = {
            Email: email,
            Password: password
        };
        this.Post("Account/Login", data, function (success, result) {
            if (success) {
                _this._token = result['token'];
                _this._username = result['username'];
                console.log("ma nem is: " + JSON.stringify(result));
                localStorage.setItem('token', _this._token);
                window.dispatchEvent(new Event("login"));
                callback(true);
            }
            else {
                _this._token = "";
                _this._username = "";
                localStorage.setItem('token', "");
                window.dispatchEvent(new Event("logout"));
                callback(false);
            }
        });
    };
    Session.prototype.Logout = function (callback) {
        // No backend call needed, we just forget the token
        this._token = "";
        localStorage.setItem('token', "");
        window.dispatchEvent(new Event("logout"));
        callback(true);
    };
    Session.prototype.Header = function () {
        return this._token == "" ? null : { Authorization: "Bearer " + this._token };
    };
    Session.prototype.Get = function (route, callback) {
        console.log("GET to " + route);
        $.ajax({
            url: this._apiUrl + route,
            type: "get",
            headers: this.Header(),
            contentType: "application/json",
            success: function (result, status, xhr) { callback(true, result); },
            error: function (xhr, status, error) { console.error(error); callback(false, status); }
        });
    };
    Session.prototype.Post = function (route, data, callback) {
        console.log("POST to " + route);
        $.ajax({
            url: this._apiUrl + route,
            type: "post",
            headers: this.Header(),
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (result, status, xhr) { callback(true, result); },
            error: function (xhr, status, error) { console.error(error); callback(false, status); }
        });
    };
    Session.prototype.Delete = function (route, callback) {
        console.log("DELETE to " + route);
        $.ajax({
            url: this._apiUrl + route,
            type: "delete",
            contentType: "application/json",
            success: function (result, status, xhr) { callback(true, result); },
            error: function (xhr, status, error) { console.error(error); callback(false, status); }
        });
    };
    Session.prototype.Put = function (route, data, callback) {
        console.log("PUT to " + route);
        $.ajax({
            url: this._apiUrl + route,
            type: "put",
            headers: this.Header(),
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (result, status, xhr) { callback(true, result); },
            error: function (xhr, status, error) { console.error(error); callback(false, status); }
        });
    };
    return Session;
}());
export { Session };
