import * as MODELS from './api/models';
import * as HANDLE from 'handlebars';
import { Core } from './core';
import { Session } from './api/session';
import { AssetLoader } from './assets/asset';
import { FeatureApp } from './features/feature';

export interface SiteWindow extends Window {
    Session: Session,
    Core: Core
}
declare let window: SiteWindow;

export class InstanceState {
    private _target: number[];
    constructor(target: number[]) {
        this._target = target;
    }
    public GetTarget(): number[] {
        return this._target;
    }
    public IsRoot(): boolean {
        if (this._target && this._target.length) {
            return true;
        }
        return false;
    }
}

export class AppState extends InstanceState {
    private _feature: MODELS.Feature;
    private _data: MODELS.FeatureData;
    constructor(target: number[], feature: MODELS.Feature, data: MODELS.FeatureData) {
        super(target);
        this._feature = feature;
        this._data = data;
    }
    public GetFeature() {
        return this._feature;
    }
    public GetData() {
        return this._data;
    }
}

export class LoaderState extends InstanceState {
    private _category: MODELS.Category;
    private _assets: MODELS.Asset[];
    private _features: MODELS.Feature[];
    private _data: MODELS.PayloadData;
    constructor(target: number[], category: MODELS.Category, assets: MODELS.Asset[], features: MODELS.Feature[], data: MODELS.PayloadData) {
        super(target);
        this._category = category;
        this._assets = assets;
        this._features = features;
        this._data = data;
    }
    public GetCategory() {
        return this._category;
    }
    public GetAssets() {
        return this._assets;
    }
    public GetFeatures() {
        return this._features;
    }
    public GetData() {
        return this._data;
    }
}

export class Instance {
    
    private _rootProjectId: number = 0;
    private _rootCategory: MODELS.Category = null;
    private _rootPayload: MODELS.PayloadData = null;
    private _rootLoader: AssetLoader = null;

    private _onLoaderUpdate: (state: LoaderState) => Promise<void>;
    private _onAppUpdate: (state: AppState) => Promise<void>;

    constructor(
        onLoaderUpdate: (state: LoaderState) => Promise<void>,
        onAppUpdate: (state: AppState) => Promise<void>) {
        this._onLoaderUpdate = onLoaderUpdate;
        this._onAppUpdate = onAppUpdate;
    }

    public LoadCategory(categoryId: number, target: number[]): void {
        this._rootProjectId = 0;

        window.Session.Get<MODELS.Category>("Category/" + categoryId)
            .then((category) => {
                this._rootCategory = category;

                return window.Session.Get<MODELS.PayloadData>("Asset/Default/" + category.Default.Id)
                    .then((payload) => this.RootPayload(payload, target))
                    .catch((e) => {
                        console.error(e);
                    });
            });
    }

    public LoadProject(projectId: number, target: number[]): void {
        this._rootProjectId = projectId;

        window.Session.Get<MODELS.Project>("Project/" + projectId)
            .then((project) => window.Session.Get<MODELS.Category>("Category/" + project.Category.Id)
                .then((category) => {
                    return window.Session.Get<MODELS.PayloadData>("Project/Payload/" + this._rootProjectId)
                        .then((payload) => this.RootPayload(payload, target));
                }));
    }

    private RootPayload(payload: MODELS.PayloadData, target: number[]): Promise<void> {
        this._rootPayload = payload;
        return window.Core.LoadPayload(window.Session, payload)
            .then((loader) => {
                this._rootLoader = loader;
                return this.LoadLoader(this._rootLoader, this._rootCategory, this._rootPayload, target);
            })
            .catch((e) => {
                console.error("Error " + e);
            });
    }

    public ReloadTarget(target: number[]): Promise<void> {
        return window.Core.LoadPayload(window.Session, this._rootPayload)
            .then((loader) => {
                this._rootLoader = loader;
                return this.LoadLoader(this._rootLoader, this._rootCategory, this._rootPayload, target);
            })
            .catch((e) => {
                console.error("Error " + e);
            });
    }

    private LoadApp(app: FeatureApp<AssetLoader, any>, feature: MODELS.Feature, data: MODELS.FeatureData, target: number[]): Promise<void> {

        let categories = app.GetCategories();
        let loaders = app.GetLoaders();

        if (target && target.length > 1) {
            return this.LoadLoader(
                loaders[target[1]],
                categories[target[1]],
                data.Layers[target[0]].Assets[target[1]],
                target.slice(2));
        }

        return this._onAppUpdate(new AppState(target, feature, data));
    }

    private LoadLoader(loader: AssetLoader, category: MODELS.Category, data: MODELS.PayloadData, target: number[]): Promise<void> {

        let features = loader.GetFeatures();
        let apps = loader.GetApps();

        if (target && target.length) {
            return this.LoadApp(apps[
                target[0]],
                features[target[0]],
                data.Features[target[0]],
                target.slice(1));
        }

        // List assets of given category
        return window.Session.Get<MODELS.Asset[]>("Asset/List/" + category.Id)
            .then((assets) => {
                return this._onLoaderUpdate(new LoaderState(target, category, assets, features, data));
            });
    }
}
