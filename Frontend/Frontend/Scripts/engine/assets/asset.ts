import * as THREE from 'three';
import { Context } from '../context';
import { Session } from '../api/session';
import * as MODELS from '../api/models';
import { FeatureApp } from '../features/feature'
import { Buildable } from '../build';
import { CreateTask } from '../../tasks';

export abstract class AssetLoader extends Buildable {

    private _name: string;
    private _loaderParams: any;
    private _assetParams: any;
    private _features: MODELS.Feature[];
    private _apps: FeatureApp<AssetLoader, any>[];

    constructor(name: string, loaderParams: string, assetParams: string, features: MODELS.Feature[], apps: FeatureApp<AssetLoader, any>[]) {
        super();
        this._name = name;
        this._features = features;
        this._apps = apps;

        try {
            this._loaderParams = JSON.parse(loaderParams);
        }
        catch (e) {
            this._loaderParams = {};
            console.warn("Parsing loader params failed: " + e);
        }

        try {
            this._assetParams = JSON.parse(assetParams);
        }
        catch (e) {
            this._assetParams = {};
            console.warn("Parsing loader params failed: " + e);
        }
    }

    protected GetLoaderParam<T>(key: string, defaultValue: T): T {
        if (key in this._loaderParams) {
            let t: T = <T>this._loaderParams[key];
            if (t != null) {
                return t;
            }
        }
        return defaultValue;
    }

    protected GetAssetParam<T>(key: string, defaultValue: T): T {
        if (key in this._assetParams) {
            let t: T = <T>this._assetParams[key];
            if (t != null) {
                return t;
            }
        }
        return defaultValue;
    }

    public GetFeatures(): MODELS.Feature[] {
        return this._features;
    }

    public GetApps(): FeatureApp<AssetLoader, any>[] {
        return this._apps;
    }
        
    public GetName(): string {
        return this._name;
    }

    override Build(context: Context): Promise<void[]> {
        // Recursively build all features of this asset and apply them
        return CreateTask<void[]>(`Building asset ${this._name}`, 1, context,
            Promise.all(this._apps.map((app) => app.Build(context)
                .then(() => app.Apply(context, this)))));
    }

    // Called after construction, use for async preparation tasks e.g. download files from backend
    abstract Prepare(session: Session, data: MODELS.PayloadData): Promise<void>;

    // Return renderable object
    abstract Root(): THREE.Object3D;
}
