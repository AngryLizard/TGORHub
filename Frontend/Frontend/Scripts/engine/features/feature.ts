import * as THREE from 'three';
import { CreateTask } from '../../tasks';
import * as MODELS from '../api/models'
import { Session } from '../api/session'
import { Buildable } from '../build';
import { Context } from '../context'

export abstract class FeatureApp<TParent extends Buildable, TLoader extends [...Buildable[]]> extends Buildable {

    private _name: string;
    private _params: any;
    private _categories: MODELS.Category[];
    private _loaders: TLoader[];

    constructor(name: string, params: string, categories: MODELS.Category[], loaders: TLoader[]) {
        super();
        this._name = name;
        this._categories = categories;
        this._loaders = loaders;

        try {
            this._params = JSON.parse(params);
        }
        catch (e) {
            this._params = {};
            console.warn("Parsing params failed: " + e);
        }
    }

    protected GetParam(key: string) {
        return this._params[key];
    }

    public GetCategories(): MODELS.Category[] {
        return this._categories;
    }

    public GetLoaders(): TLoader[] {
        return this._loaders;
    }

    public GetName() {
        return this._name;
    }

    public Build(context: Context): Promise<void[][][]> {
        // Recursively build all assets of this feature
        return CreateTask<void[][][]>(`Building feature ${this._name}`, 1, context,
            Promise.all(this._loaders.map((loaders) =>
                Promise.all(loaders.filter((loader) => loader != null).map((loader) =>
                    loader.Build(context))))));
    }

    // Called after construction, use for async preparation tasks e.g. download files from backend
    abstract Prepare(session: Session, data: MODELS.FeatureData): Promise<void>;

    // Apply feature to given context
    abstract Apply(context: Context, parent: TParent): Promise<void>;
}
