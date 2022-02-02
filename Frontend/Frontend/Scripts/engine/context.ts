import { TaskHolder } from '../tasks'

export class Context extends TaskHolder {

    private _root: THREE.Group;

    constructor(root: THREE.Group) {
        super();
        this._root = root;
    }

    public GetRoot(): THREE.Group {
        return this._root;
    };
}