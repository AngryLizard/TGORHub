import * as MODELS from './api/models'
import { Session } from './api/session';

import { FeatureApp } from './features/feature';
import { TintFeature } from "./features/tint";
import { SkinFeature } from "./features/skin";

import { AssetLoader } from './assets/asset';
import { TextureAsset } from "./assets/texture";
import { MeshAsset } from "./assets/mesh";

export const Apps: {
    [key: string]: (params: string, categories: MODELS.Category[], loaders: [...AssetLoader[]][]) => FeatureApp<any, any>
} = {
    'Tint': (params, categories, loaders: []) => new TintFeature(params, categories, loaders),
    'Skin': (params, categories, loaders: [TextureAsset][]) => new SkinFeature(params, categories, loaders),
}

export const Loaders: {
    [name: string]: (loaderParams: string, assetParams: string, features: MODELS.Feature[], apps: FeatureApp<AssetLoader, any>[]) => AssetLoader
} = {
    'Mesh': (loaderParams, assetParams, features, apps) => new MeshAsset(loaderParams, assetParams, features, apps),
    'Texture': (loaderParams, assetParams, features, apps) => new TextureAsset(loaderParams, assetParams, features, apps)
}

export function CreateFeatureApps(session: Session, features: MODELS.Feature[], data: MODELS.FeatureData[]): Promise<FeatureApp<any, any>[]> {

    // Create apps for all features
    return Promise.all<FeatureApp<any, any>>(features.map((feature, index) => {
            let featureData = data[index];

            // Get asset categories for this feature
            return session.Get<MODELS.Category[]>("Category/List/" + feature.Id)
                // Create assets for all layers
                .then((categories) => Promise.all<AssetLoader[]>(featureData.Layers.map((layer) =>
                    Promise.all<AssetLoader>(layer.Assets.map((asset) =>
                        CreateAssetLoader(session, asset)))))
                    // Create and prepare app
                    .then((loaders) => {
                        if (feature.App in Apps) {
                            return Apps[feature.App](feature.Params, categories, loaders);
                        }
                        throw new Error(`Couldn't find loader ${feature.App}`);
                    })
                    .then((app) => app.Prepare(session, featureData)
                        .then(() => app))
                    .catch((e) => {
                        console.error("preparing app " + e);
                        return null;
                    }));
    }))
        .catch((e) => {
            console.error("loading features " + e);
            return [];
        });
}

export function CreateAssetLoader(session: Session, data: MODELS.PayloadData): Promise<AssetLoader> {

    // Don't create loaders for nothing
    if (data == null) {
        return null;
    }

    // Load all features owned by this loader
    return session.Get<MODELS.Feature[]>("Asset/Feature/" + data.Asset)
        .then((features) => CreateFeatureApps(session, features, data.Features)
            // Get asset data for this payload
            .then((apps) => session.Get<MODELS.Asset>("Asset/" + data.Asset)
                // Create and prepare loader from asset category
                .then((asset) => session.Get<MODELS.Category>("Category/" + asset.Category.Id)
                    .then((category) => {
                        if (category.Loader in Loaders) {
                            return Loaders[category.Loader](category.Params, asset.Params, features, apps);
                        }
                        throw new Error(`Couldn't find loader ${category.Loader}`);
                    })
                    .then((loader) => loader.Prepare(session, data)
                        .then(() => loader))
                    .catch((e) => {
                        console.error("preparing loader " + e);
                        return null;
                    }))))
        .catch((e) => {
            console.error("loading asset " + e);
            return null;
        });
}