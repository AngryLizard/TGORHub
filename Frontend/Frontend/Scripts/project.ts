import * as MODELS from './engine/api/models';
import * as HANDLE from 'handlebars';

import { Session } from './engine/api/session';
import { Core } from './engine/core';

export interface SiteWindow extends Window {
    Session: Session,
    Core: Core,
    OnProjectSelected: (projectId: number) => void
}
declare let window: SiteWindow;

let currentId: number = 0;
let projectCache: { [id: number]: MODELS.Project } = {};

window.Session.Get<MODELS.Project[]>("Project/List")
    .then((projects: MODELS.Project[]) => {

        var projectItemHandle = HANDLE.compile($("#projectItemTemplate").html());
        for (let project of projects) {
            $("#projectContainer").append(projectItemHandle(project));
            projectCache[project.Id] = project;
        }

        if (projects.length > 0) {
            window.OnProjectSelected(projects[0].Id);
        }
        else {
            console.log("No projects found");
        }
    });

window.Session.Get<MODELS.Category[]>("Category/Root")
    .then((categories) => {

        var createItemHandle = HANDLE.compile($("#createItemTemplate").html());
        for (let category of categories) {
            $("#createContainer").append(createItemHandle(category));
        }
    });

window.OnProjectSelected = function (projectId: number): void {

    // Set active button
    if (currentId > 0) {
        $(`#projectNav${currentId}`).toggleClass("active", false);
    }
    $(`#projectNav${projectId}`).toggleClass("active", true);
    currentId = projectId;

    if (projectId in projectCache) {
        let project = projectCache[projectId];

        // Set edit button depending on whether project is owned by user
        if (window.Session.IsUser(project.Owner.Id)) {
            $("#editButton").removeAttr('hidden');
            $('#editButton').attr("href", `../Home/Edit?projectId=${projectId}`);
        }
        else {
            $('#editButton').prop('hidden', true);
        }

        window.Session.Get<MODELS.PayloadData>("Project/Payload/" + projectId)
            .then((payload) => window.Core.LoadPayload(window.Session, payload))
            .catch((e) => {
                console.error(e);
            });
    }
}
