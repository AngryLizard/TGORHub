import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';
import { AssetLoader } from './asset'
import { Session } from '../api/session'
import * as MODELS from '../api/models'
import { FeatureApp } from '../features/feature';
import { CreateTask } from '../../tasks';

export class MeshAsset extends AssetLoader {

	private _group: THREE.Group;

	constructor(loaderParams: string, assetParams: string, features: MODELS.Feature[], apps: FeatureApp<MeshAsset, any>[]) {

		console.log("MeshAsset constructor");
		super("Mesh", loaderParams, assetParams, features, apps);
	}

	public GetGroup(): THREE.Group {
		return this._group;
    }

	override Prepare(session: Session, data: MODELS.PayloadData): Promise<void> {

		return CreateTask("Downloading GLTF model", 1, session,
			new Promise<void>((resolve, reject) => {

				const assetId: number = data.Asset;
				if (assetId == 0) {
					reject("No asset supplied.");
					return;
				}

				let url: string = session.DownloadUrl(assetId);
				console.log(`Download: ${url}`);

				const loader = new GLTFLoader();
				//loader.requestHeader = session.Header();

				loader.load(url,
					(gltf: GLTF) => {

						this._group = gltf.scene;
						let scale = this.GetAssetParam<number>("Scale", 1);
						this._group.scale.set(scale, scale, scale);

						resolve();

						gltf.animations; // Array<THREE.AnimationClip>
						gltf.scene; // THREE.Group
						gltf.scenes; // Array<THREE.Group>
						gltf.cameras; // Array<THREE.Camera>
						gltf.asset; // Object
					},
					(xhr) => {
						//progress(xhr.loaded / xhr.total, "Downloading GLTF model");
					},
					(error) => {
						console.error(error);
						reject("Downlading GLTF file failed.");
					});
			}));
	}

	// Return renderable object
	override Root(): THREE.Object3D {
		return this._group;
    }
}
/*
public AddBox(textureUrl: string) {

	const texture = new THREE.TextureLoader().load(textureUrl);
	const geometry = new THREE.BoxGeometry(200, 200, 200);
	const material = new THREE.MeshBasicMaterial({ map: texture });

	let root = this._context.GetRoot();
	let mesh = new THREE.Mesh(geometry, material);
	root.add(mesh);
}
 */

