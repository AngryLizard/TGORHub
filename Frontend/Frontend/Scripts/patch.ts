import { delay } from './engine/api/utils';
import * as CORE from './engine/core';
import * as HANDLE from 'handlebars';

var container = document.getElementById("container");
var frame = document.getElementById("frame");

let core: CORE.Core = new CORE.Core(container, frame);
core.Init();
core.Animate();

var layerItemHandle = HANDLE.compile($("#layerItemTemplate").html());

for (let i = 0; i < 5; i++) {
    $("#layerContainer").append(layerItemHandle({ index: i }));
}