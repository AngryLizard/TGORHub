import { Context } from './context';

export abstract class Buildable {
    abstract Build(context: Context): Promise<any>;
}
