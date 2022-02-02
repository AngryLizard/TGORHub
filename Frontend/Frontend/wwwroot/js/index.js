/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
var __webpack_exports__ = {};
window.Session.Get("Project/List")
    .then(function (project) { return window.Session.Get("Project/Payload/" + project[0].Id); })
    .then(function (payload) { return window.Core.LoadPayload(window.Session, payload); })
    .catch(function (e) {
    console.error(e);
});


/******/ })()
;