import * as MODELS from './engine/api/models';
import { Core } from './engine/core';
import { Session } from './engine/api/session';

export interface SiteWindow extends Window {
    Session: Session,
    Core: Core
}
declare let window: SiteWindow;

window.Session.Get<MODELS.Project[]>("Project/List")
    .then((project: MODELS.Project[]) => window.Session.Get<MODELS.PayloadData>("Project/Payload/" + project[0].Id))
    .then((payload) => window.Core.LoadPayload(window.Session, payload))
    .catch((e) => {
        console.error(e);
    });
