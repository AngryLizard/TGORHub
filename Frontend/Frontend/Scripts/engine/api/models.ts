
export interface Entity {
    Id: number;
    Name: string;
    Date: Date;
}

export interface Project extends Entity {
    Visibility: string;
    Suspended: boolean;
    Category: Entity;
    Owner: Entity;
}

export interface Asset extends Entity {
    Creator: Entity;
    Visibility: string;
    Category: Entity;
    Params: string;
}

export interface Feature extends Entity {
    App: string;
    Params: string;
    MinLayers: number;
    MaxLayers: number;
    Categories: Entity[];
    Floats: number;
    Integers: number;
}

export interface Category extends Entity {
    Loader: string;
    Params: string;
    Default: Entity;
}

export interface PayloadData {

    Asset: number;
    Features: FeatureData[];
}

export interface FeatureData {

    Layers: LayerData[];
}

export interface LayerData {

    Assets: PayloadData[];
    Floats: number[];
    Integer: number[];
}