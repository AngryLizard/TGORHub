export interface TaskWindow extends Window {
    OnTaskUpdate: ()=>void
}
declare let window: TaskWindow;

var TasksDone = 0;
var TasksTotal = 0;
var TaskName = "";
var TaskError = false;
var TaskIdle = true;

export class TaskHolder {
    
    private _isEnabled: boolean;
    constructor() {
        this._isEnabled = true;
    }

    public IsEnabled(): boolean {
        return this._isEnabled;
    }

    public Terminate() {
        this._isEnabled = false;
    }
}

export function CreateTask<T>(name: string, workload: number, holder: TaskHolder, promise: Promise<T>): Promise<T> {

    if (!holder.IsEnabled()) {
        throw new Error("Could start task on terminated holder");
    }

    // Undoes this task if it got terminated while running
    let checkHolder = () => {
        if (!holder.IsEnabled()) {
            TasksTotal -= workload;
            throw new Error("Task was terminated");
        }
    }

    // Reset if new session
    if (TaskIdle) {

        TasksTotal = 0;
        TasksDone = 0;
        TaskError = false;

        TaskIdle = false;
    }

    // Register the workload
    TasksTotal += workload;

    return new Promise<T>((resolve, reject) => {
        checkHolder();

        // Start the task
        TaskName = name;

        promise.then((result) => {
            checkHolder();

            TasksDone += workload;
            resolve(result);

        }, (reason) => {
            checkHolder();

            TasksTotal -= workload;
            TaskError = true;

            reject(reason);
        })
    });
}

window.setInterval(() => {
    if (TasksDone == TasksTotal || TaskError) {
        TaskIdle = true;
    }

    window.OnTaskUpdate();
}, 500);

window.OnTaskUpdate = () => {
    $('#progressBar').toggleClass("bg-danger", false);
    $('#progressBar').toggleClass("bg-info", false);
    $('#progressBar').toggleClass("bg-success", false);

    if (TaskError) {
        $('#progressBar').toggleClass("bg-danger", true);
        $('#progressBar').css("width", `100%`);
        $('#progressBar').removeClass("bg-success");
        $('#progressBar').addClass("bg-info");
        $('#progressNum').html(`[-/-]`);
        $('#progressState').html(`${TaskName}`);
        $('#progressPercent').html(`..%`);
        $('#progressContainer').fadeIn();
    }
    else if (TasksDone == TasksTotal) {
        $('#progressBar').toggleClass("bg-success", true);
        $('#progressBar').css("width", `100%`);
        $('#progressNum').html(`[${TasksDone}/${TasksTotal}]`);
        $('#progressState').html("Done!");
        $('#progressPercent').html(`100%`);
        $('#progressContainer').fadeOut();
    }
    else {
        $('#progressBar').toggleClass("bg-info", true);
        let progress = Math.round(TasksDone * 100 / TasksTotal);
        $('#progressBar').css("width", `${progress}%`);
        $('#progressBar').removeClass("bg-danger");
        $('#progressBar').addClass("bg-info");
        $('#progressNum').html(`[${TasksDone}/${TasksTotal}]`);
        $('#progressState').html(`${TaskName}`);
        $('#progressPercent').html(`${progress}%`);
        $('#progressContainer').fadeIn();
    }
};
