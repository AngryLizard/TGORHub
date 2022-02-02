import { Session } from "./engine/api/session"
import { Core } from './engine/core';

import { SessionLogin } from "./login"
import { SessionRegister } from "./register"
import * as HANDLE from 'handlebars';

export interface SiteWindow extends Window {
    Username: Function,
    Login: Function,
    Logout: Function,
    Register: Function,
    Session: Session,
    Core: Core
}
declare let window: SiteWindow;

HANDLE.registerHelper('dateFormat', function (date: Date) {
    return new Date(date).toLocaleDateString();
});

HANDLE.registerHelper('projectOwner', function (id: number, tr: string, fl: string) {
    return window.Session.IsUser(id) ? tr : fl;
});

// Validate session
window.Session = new Session("https://localhost:7134");
window.Session.Validate().catch(() => { });

var container = document.getElementById("container");
var frame = document.getElementById("frame");
window.Core = new Core(container, frame);
window.Core.Init();
window.Core.Animate();

// Global functions
window.Username = function(): string {
    return window.Session.GetUsername();
}

window.Login = function (): void {
    SessionLogin(window.Session);
}

window.Logout = function (): void {
    window.Session.Logout(() => { });
}

window.Register = function (): void {
    SessionRegister(window.Session);
}