import * as THREE from 'three';
import { FeatureApp } from './feature'
import { Context } from '../context'
import { Session } from '../api/session'
import * as MODELS from '../api/models'
import { AssetLoader } from '../assets/asset';
import { MeshAsset } from '../assets/mesh';
import { TextureAsset } from '../assets/texture';

export class SkinFeature extends FeatureApp<MeshAsset, [TextureAsset]> {

	constructor(params: string, categories: MODELS.Category[], loaders: [TextureAsset][]) {
		super("Skin", params, categories, loaders);
	}

	override Prepare(session: Session, data: MODELS.FeatureData): Promise<void> {

		return new Promise((resolve, reject) => {
			resolve();
        });
	}

	override Apply(context: Context): Promise<void> {

		return new Promise((resolve, reject) => {
			resolve();
		});
	}
}
