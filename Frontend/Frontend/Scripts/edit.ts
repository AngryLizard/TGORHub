import * as HANDLE from 'handlebars';
import { Core } from './engine/core';
import { Session } from './engine/api/session';
import { AppState, LoaderState, Instance } from './engine/instance';

export interface SiteWindow extends Window {
    Session: Session,
    Core: Core,
    OnLoadCategory: (categoryId: number, target: number[]) => void,
    OnLoadProject: (projectId: number, target: number[]) => void,
    OnLoadTarget: (target: number[]) => void,
    OnAssetSelected: (assetId: number) => void
}
declare let window: SiteWindow;

let loaderState: LoaderState = null;
let appState: AppState = null;

let instance: Instance = new Instance(
    (state: LoaderState) => {
        loaderState = state;
        appState = null;

        // List assets
        $("#assetContainer").empty();
        var assetItemHandle = HANDLE.compile($("#assetItemTemplate").html());
        for (let asset of state.GetAssets()) {
            $("#assetContainer").append(assetItemHandle(asset));
        }

        // List features
        $("#featureContainer").empty();
        var featureItemHandle = HANDLE.compile($("#featureItemTemplate").html());
        for (let feature of state.GetFeatures()) {
            $("#featureContainer").append(featureItemHandle(feature));
        }

        // Active asset state
        for (let asset of state.GetAssets()) {
            $(`#assetitem${asset.Id}`).toggleClass("active", false);
        }
        $(`#assetitem${state.GetData().Asset}`).toggleClass("active", true);

        // Don't show back button if root
        if (state.IsRoot()) {
            $("#backButton").removeAttr('hidden');
        }
        else {
            $('#backButton').prop('hidden', true);
        }

        return Promise.resolve();
    },
    (state: AppState) => {
        loaderState = null;
        appState = state;

        // Always show back button
        $("#backButton").removeAttr('hidden');

        return Promise.resolve();
    });

window.OnLoadCategory = function (categoryId: number, target: number[]): void {
    instance.LoadCategory(categoryId, target);
}

window.OnLoadProject = function (projectId: number, target: number[]): void {
    instance.LoadProject(projectId, target);
}

window.OnLoadTarget = function (target: number[]): void {
    
}

window.OnAssetSelected = function (assetId: number): void {

    if (loaderState == null) {
        return;
    }

    loaderState.GetData().Asset = assetId;
    instance.ReloadTarget(loaderState.GetTarget());
}
