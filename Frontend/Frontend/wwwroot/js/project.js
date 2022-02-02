/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
var __webpack_exports__ = {};

;// CONCATENATED MODULE: external "Handlebars"
const external_Handlebars_namespaceObject = Handlebars;
;// CONCATENATED MODULE: ./Scripts/project.ts

var currentId = 0;
var projectCache = {};
window.Session.Get("Project/List")
    .then(function (projects) {
    var projectItemHandle = external_Handlebars_namespaceObject.compile($("#projectItemTemplate").html());
    for (var _i = 0, projects_1 = projects; _i < projects_1.length; _i++) {
        var project = projects_1[_i];
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
window.Session.Get("Category/Root")
    .then(function (categories) {
    var createItemHandle = external_Handlebars_namespaceObject.compile($("#createItemTemplate").html());
    for (var _i = 0, categories_1 = categories; _i < categories_1.length; _i++) {
        var category = categories_1[_i];
        $("#createContainer").append(createItemHandle(category));
    }
});
window.OnProjectSelected = function (projectId) {
    // Set active button
    if (currentId > 0) {
        $("#projectNav".concat(currentId)).toggleClass("active", false);
    }
    $("#projectNav".concat(projectId)).toggleClass("active", true);
    currentId = projectId;
    if (projectId in projectCache) {
        var project = projectCache[projectId];
        // Set edit button depending on whether project is owned by user
        if (window.Session.IsUser(project.Owner.Id)) {
            $("#editButton").removeAttr('hidden');
            $('#editButton').attr("href", "../Home/Edit?projectId=".concat(projectId));
        }
        else {
            $('#editButton').prop('hidden', true);
        }
        window.Session.Get("Project/Payload/" + projectId)
            .then(function (payload) { return window.Core.LoadPayload(window.Session, payload); })
            .catch(function (e) {
            console.error(e);
        });
    }
};

/******/ })()
;