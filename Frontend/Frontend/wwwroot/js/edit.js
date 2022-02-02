/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
var __webpack_exports__ = {};

;// CONCATENATED MODULE: external "Handlebars"
const external_Handlebars_namespaceObject = Handlebars;
;// CONCATENATED MODULE: ./Scripts/engine/instance.ts
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var InstanceState = /** @class */ (function () {
    function InstanceState(target) {
        this._target = target;
    }
    InstanceState.prototype.GetTarget = function () {
        return this._target;
    };
    InstanceState.prototype.IsRoot = function () {
        if (this._target && this._target.length) {
            return true;
        }
        return false;
    };
    return InstanceState;
}());

var AppState = /** @class */ (function (_super) {
    __extends(AppState, _super);
    function AppState(target, feature, data) {
        var _this = _super.call(this, target) || this;
        _this._feature = feature;
        _this._data = data;
        return _this;
    }
    AppState.prototype.GetFeature = function () {
        return this._feature;
    };
    AppState.prototype.GetData = function () {
        return this._data;
    };
    return AppState;
}(InstanceState));

var LoaderState = /** @class */ (function (_super) {
    __extends(LoaderState, _super);
    function LoaderState(target, category, assets, features, data) {
        var _this = _super.call(this, target) || this;
        _this._category = category;
        _this._assets = assets;
        _this._features = features;
        _this._data = data;
        return _this;
    }
    LoaderState.prototype.GetCategory = function () {
        return this._category;
    };
    LoaderState.prototype.GetAssets = function () {
        return this._assets;
    };
    LoaderState.prototype.GetFeatures = function () {
        return this._features;
    };
    LoaderState.prototype.GetData = function () {
        return this._data;
    };
    return LoaderState;
}(InstanceState));

var Instance = /** @class */ (function () {
    function Instance(onLoaderUpdate, onAppUpdate) {
        this._rootProjectId = 0;
        this._rootCategory = null;
        this._rootPayload = null;
        this._rootLoader = null;
        this._onLoaderUpdate = onLoaderUpdate;
        this._onAppUpdate = onAppUpdate;
    }
    Instance.prototype.LoadCategory = function (categoryId, target) {
        var _this = this;
        this._rootProjectId = 0;
        window.Session.Get("Category/" + categoryId)
            .then(function (category) {
            _this._rootCategory = category;
            return window.Session.Get("Asset/Default/" + category.Default.Id)
                .then(function (payload) { return _this.RootPayload(payload, target); })
                .catch(function (e) {
                console.error(e);
            });
        });
    };
    Instance.prototype.LoadProject = function (projectId, target) {
        var _this = this;
        this._rootProjectId = projectId;
        window.Session.Get("Project/" + projectId)
            .then(function (project) { return window.Session.Get("Category/" + project.Category.Id)
            .then(function (category) {
            return window.Session.Get("Project/Payload/" + _this._rootProjectId)
                .then(function (payload) { return _this.RootPayload(payload, target); });
        }); });
    };
    Instance.prototype.RootPayload = function (payload, target) {
        var _this = this;
        this._rootPayload = payload;
        return window.Core.LoadPayload(window.Session, payload)
            .then(function (loader) {
            _this._rootLoader = loader;
            return _this.LoadLoader(_this._rootLoader, _this._rootCategory, _this._rootPayload, target);
        })
            .catch(function (e) {
            console.error("Error " + e);
        });
    };
    Instance.prototype.ReloadTarget = function (target) {
        var _this = this;
        return window.Core.LoadPayload(window.Session, this._rootPayload)
            .then(function (loader) {
            _this._rootLoader = loader;
            return _this.LoadLoader(_this._rootLoader, _this._rootCategory, _this._rootPayload, target);
        })
            .catch(function (e) {
            console.error("Error " + e);
        });
    };
    Instance.prototype.LoadApp = function (app, feature, data, target) {
        var categories = app.GetCategories();
        var loaders = app.GetLoaders();
        if (target && target.length > 1) {
            return this.LoadLoader(loaders[target[1]], categories[target[1]], data.Layers[target[0]].Assets[target[1]], target.slice(2));
        }
        return this._onAppUpdate(new AppState(target, feature, data));
    };
    Instance.prototype.LoadLoader = function (loader, category, data, target) {
        var _this = this;
        var features = loader.GetFeatures();
        var apps = loader.GetApps();
        if (target && target.length) {
            return this.LoadApp(apps[target[0]], features[target[0]], data.Features[target[0]], target.slice(1));
        }
        // List assets of given category
        return window.Session.Get("Asset/List/" + category.Id)
            .then(function (assets) {
            return _this._onLoaderUpdate(new LoaderState(target, category, assets, features, data));
        });
    };
    return Instance;
}());


;// CONCATENATED MODULE: ./Scripts/edit.ts


var loaderState = null;
var appState = null;
var instance = new Instance(function (state) {
    loaderState = state;
    appState = null;
    // List assets
    $("#assetContainer").empty();
    var assetItemHandle = external_Handlebars_namespaceObject.compile($("#assetItemTemplate").html());
    for (var _i = 0, _a = state.GetAssets(); _i < _a.length; _i++) {
        var asset = _a[_i];
        $("#assetContainer").append(assetItemHandle(asset));
    }
    // List features
    $("#featureContainer").empty();
    var featureItemHandle = external_Handlebars_namespaceObject.compile($("#featureItemTemplate").html());
    for (var _b = 0, _c = state.GetFeatures(); _b < _c.length; _b++) {
        var feature = _c[_b];
        $("#featureContainer").append(featureItemHandle(feature));
    }
    // Active asset state
    for (var _d = 0, _e = state.GetAssets(); _d < _e.length; _d++) {
        var asset = _e[_d];
        $("#assetitem".concat(asset.Id)).toggleClass("active", false);
    }
    $("#assetitem".concat(state.GetData().Asset)).toggleClass("active", true);
    // Don't show back button if root
    if (state.IsRoot()) {
        $("#backButton").removeAttr('hidden');
    }
    else {
        $('#backButton').prop('hidden', true);
    }
    return Promise.resolve();
}, function (state) {
    loaderState = null;
    appState = state;
    // Always show back button
    $("#backButton").removeAttr('hidden');
    return Promise.resolve();
});
window.OnLoadCategory = function (categoryId, target) {
    instance.LoadCategory(categoryId, target);
};
window.OnLoadProject = function (projectId, target) {
    instance.LoadProject(projectId, target);
};
window.OnLoadTarget = function (target) {
};
window.OnAssetSelected = function (assetId) {
    if (loaderState == null) {
        return;
    }
    loaderState.GetData().Asset = assetId;
    instance.ReloadTarget(loaderState.GetTarget());
};

/******/ })()
;