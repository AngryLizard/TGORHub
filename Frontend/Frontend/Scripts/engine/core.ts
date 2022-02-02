import * as THREE from 'three';
import { Context } from './context';
import * as MODELS from './api/models'
import { Session } from './api/session'
import { CreateAssetLoader } from './resources'
import { AssetLoader } from './assets/asset';

export class Core {

	private _camera: THREE.PerspectiveCamera;
	private _renderer: THREE.WebGLRenderer;
	private _scene: THREE.Scene;

	private _context: Context;
	private _root: THREE.Group;

	private _running: boolean;
	private _container: HTMLElement;
	private _frame: HTMLElement;

	constructor(container: HTMLElement, frame: HTMLElement) {
		this._container = container;
		this._frame = frame;
    }
    
	private GetHeight() {
		return this._container.clientHeight;
	}

	private GetWidth() {
		return this._container.clientWidth;
	}

	public Init() {
		
		this._camera = new THREE.PerspectiveCamera(70, this.GetWidth() / this.GetHeight(), 1, 1000);
		this._camera.position.z = 400;

		this._scene = new THREE.Scene();
		this._root = new THREE.Group();
		this._scene.add(this._root);

		let light: THREE.AmbientLight = new THREE.AmbientLight(new THREE.Color(1, 1, 1), 1.0);
		this._scene.add(light)

		this._renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
		this._renderer.setPixelRatio(window.devicePixelRatio);
		this._renderer.autoClear = true;
		this._renderer.setClearColor(new THREE.Color(0,0,0), 0);
		this._renderer.setSize(this.GetWidth(), this.GetHeight());
		this._frame.appendChild(this._renderer.domElement);

		this._running = true;
		this._context = null;

		// Resize when parent changes
		window.addEventListener('resize', () => { this._onWindowResize() });

		// Stop rendering when unloading (prevents white flash)
		window.addEventListener('beforeunload', () => { this._running = false; });
	}

	private _onWindowResize() {

		if (this._running) {
			this._camera.aspect = this.GetWidth() / this.GetHeight();
			this._camera.updateProjectionMatrix();

			this._renderer.setSize(this.GetWidth(), this.GetHeight());
		}
	}

	public LoadPayload(session: Session, payload: MODELS.PayloadData): Promise<AssetLoader> {

		// Terminate previous tasks
		if (this._context) {
			this._context.Terminate();
			this._root.clear();
		}

		// Create new context
		this._context = new Context(this._root);

		// Apply features, make it a task so any added task will have to wait until this promise is done
		return CreateAssetLoader(session, payload)
					.then((loader) => loader.Build(this._context)
						.then(() => {
							this._root.add(loader.Root());
							return loader;
						}));
	}

	public Animate() {

		if (this._running) {
			requestAnimationFrame(() => { this.Animate(); });

			if (this._context) {
				let root = this._context.GetRoot();
				if (root) {
					root.rotation.y += 0.01;
				}
            }

			this._renderer.render(this._scene, this._camera);
        }
	}
}

