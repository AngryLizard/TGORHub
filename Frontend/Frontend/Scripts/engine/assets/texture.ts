import * as THREE from 'three';
import { AssetLoader } from './asset'
import { Context } from '../context'
import { Session } from '../api/session'
import * as MODELS from '../api/models'
import { FeatureApp } from '../features/feature';
import { CreateTask } from '../../tasks';

export class TextureAsset extends AssetLoader {

	private _texture: THREE.Texture;

	constructor(loaderParams: string, assetParams: string, features: MODELS.Feature[], apps: FeatureApp<TextureAsset, any>[]) {
		super("Texture", loaderParams, assetParams, features, apps);
	}

	public GetTexture(): THREE.Texture {
		return this._texture;
    }

	override Prepare(session: Session, data: MODELS.PayloadData): Promise<void> {

		return CreateTask("Downloading texture file", 1, session,
			new Promise<void>((resolve, reject) => {

				const assetId: number = data.Asset;
				if (assetId == 0) {
					reject("No asset supplied.");
					return;
				}

				let url: string = session.DownloadUrl(assetId);
				console.log(`Download: ${url}`);

				const loader = new THREE.TextureLoader();
				//loader.requestHeader = session.Header();

				this._texture = loader.load(url);
			}));
	}

	// Return renderable object
	override Root(): THREE.Object3D {
		return new THREE.Group;
	}
}
