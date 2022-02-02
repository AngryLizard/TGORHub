import * as CORE from './engine/core';
import { Session } from './engine/api/session';

export interface SessionWindow extends Window {
    Session: Session
}
declare let window: SessionWindow;


var container = document.getElementById("container");
var frame = document.getElementById("frame");

let core: CORE.Core = new CORE.Core(container, frame);
core.Init();
core.Animate();
