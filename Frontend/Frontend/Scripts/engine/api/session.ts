import { TaskHolder, CreateTask } from '../../tasks'
import { delay } from './utils';

export class Session extends TaskHolder {

    private readonly _apiUrl: string;
    private _name: string = "";
    private _token: string = "";
    private _id: number = -1;
    private _access: string = "";

    constructor(url: string) {
        super();
        this._apiUrl = url + /api/;
    }

    public IsUser(id: number): boolean {
        return this._id == id;
    }

    public GetUsername(): string {
        return this._name;
    }

    public Register(name: string, email: string, password: string): Promise<void> {

        var data = {
            Name: name,
            Email: email,
            Password: password
        }

        return this.Put("Account/Register", data)
            .then(() => {
                window.dispatchEvent(new Event("register"));
            }, () => {
                return Promise.reject();
            });
    }

    public Validate(): Promise<void> {

        let token: string = localStorage.getItem('token');
        if (token != null) {

            this._token = token;
            this._name = "";
            this._id = -1;
            this._access = "";

            return this.Get<any>("Account/Validate")
                .then((result: any) => {
                    this._name = result.Name;
                    this._access = result.Access;
                    this._id = result.Id;
                    window.dispatchEvent(new Event("login"));
                },
                () => {
                    // Reset token so we don't try again next load
                    this._token = "";
                    localStorage.removeItem('token');
                    return Promise.reject();
                });
        }
        return Promise.reject();
    }
    
    public Login(email: string, password: string): Promise<void> {
    
        var data = {
            Email: email,  
            Password: password 
        }
        
        return this.Post<any>("Account/Login", data)
            .then(
                (result:any) => {
                    localStorage.setItem('token', result.Token);
                },
                () => {
                    localStorage.removeItem('token');
                    return Promise.reject();
                });
    }

    public Logout(callback:Function) {

        // No backend call needed, we just forget the token
        this._token = "";
        localStorage.removeItem('token');
        window.dispatchEvent(new Event("logout"));
        callback(true);
    }

    public Header() {
        return this._token == "" ? null : { Authorization: "Bearer " + this._token };
    }

    public Get<T>(route: string): Promise<T> {
        return this.Ajax(route, "get", null);
    }

    public Post<T>(route: string, data: object): Promise<T> {
        return this.Ajax(route, "post", data);
    }

    public Delete<T>(route: string): Promise<T> {
        return this.Ajax(route, "delete", null);
    }

    public Put<T>(route: string, data: object): Promise<T> {
        return this.Ajax(route, "put", data);
    }

    public DownloadUrl(id: number):string {
        return this._apiUrl + `Asset/Download/${id}`;
    }

    private Ajax<T>(route: string, type: string, data: any): Promise<T> {
        return CreateTask<T>(`Backend call to ${route}`, 1, this,

            new Promise<T>((resolve, reject) => {
                console.log(`${type} to ${route}`);
                $.ajax({
                    url: this._apiUrl + route,
                    type: type,
                    headers: this.Header(),
                    contentType: "application/json",
                    data: data ? JSON.stringify(data) : undefined,
                    success: (result, status, xhr) => { resolve(<T>result); },
                    error: (xhr, status, error) => { reject({ error }); }
                });
            }));
    }
}