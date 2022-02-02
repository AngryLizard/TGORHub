/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	// The require scope
/******/ 	var __webpack_require__ = {};
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
/* unused harmony exports TaskHolder, CreateTask */
var TasksDone = 0;
var TasksTotal = 0;
var TaskName = "";
var TaskError = false;
var TaskIdle = true;
var TaskHolder = /** @class */ (function () {
    function TaskHolder() {
        this._isEnabled = true;
    }
    TaskHolder.prototype.IsEnabled = function () {
        return this._isEnabled;
    };
    TaskHolder.prototype.Terminate = function () {
        this._isEnabled = false;
    };
    return TaskHolder;
}());

function CreateTask(name, workload, holder, promise) {
    if (!holder.IsEnabled()) {
        throw new Error("Could start task on terminated holder");
    }
    // Undoes this task if it got terminated while running
    var checkHolder = function () {
        if (!holder.IsEnabled()) {
            TasksTotal -= workload;
            throw new Error("Task was terminated");
        }
    };
    // Reset if new session
    if (TaskIdle) {
        TasksTotal = 0;
        TasksDone = 0;
        TaskError = false;
        TaskIdle = false;
    }
    // Register the workload
    TasksTotal += workload;
    return new Promise(function (resolve, reject) {
        checkHolder();
        // Start the task
        TaskName = name;
        promise.then(function (result) {
            checkHolder();
            TasksDone += workload;
            resolve(result);
        }, function (reason) {
            checkHolder();
            TasksTotal -= workload;
            TaskError = true;
            reject(reason);
        });
    });
}
window.setInterval(function () {
    if (TasksDone == TasksTotal || TaskError) {
        TaskIdle = true;
    }
    window.OnTaskUpdate();
}, 500);
window.OnTaskUpdate = function () {
    $('#progressBar').toggleClass("bg-danger", false);
    $('#progressBar').toggleClass("bg-info", false);
    $('#progressBar').toggleClass("bg-success", false);
    if (TaskError) {
        $('#progressBar').toggleClass("bg-danger", true);
        $('#progressBar').css("width", "100%");
        $('#progressBar').removeClass("bg-success");
        $('#progressBar').addClass("bg-info");
        $('#progressNum').html("[-/-]");
        $('#progressState').html("".concat(TaskName));
        $('#progressPercent').html("..%");
        $('#progressContainer').fadeIn();
    }
    else if (TasksDone == TasksTotal) {
        $('#progressBar').toggleClass("bg-success", true);
        $('#progressBar').css("width", "100%");
        $('#progressNum').html("[".concat(TasksDone, "/").concat(TasksTotal, "]"));
        $('#progressState').html("Done!");
        $('#progressPercent').html("100%");
        $('#progressContainer').fadeOut();
    }
    else {
        $('#progressBar').toggleClass("bg-info", true);
        var progress = Math.round(TasksDone * 100 / TasksTotal);
        $('#progressBar').css("width", "".concat(progress, "%"));
        $('#progressBar').removeClass("bg-danger");
        $('#progressBar').addClass("bg-info");
        $('#progressNum').html("[".concat(TasksDone, "/").concat(TasksTotal, "]"));
        $('#progressState').html("".concat(TaskName));
        $('#progressPercent').html("".concat(progress, "%"));
        $('#progressContainer').fadeIn();
    }
};

/******/ })()
;