
export interface IServiceProvider {
    getService<T>(service: string): T | undefined;
    register(service: string, instance: any): IServiceProvider;
}

export class ServiceProvider implements IServiceProvider {
    private readonly services = new Map<string, any>();

    getService<T>(service: string): T | undefined {
        return this.services.get(service);
    }
    register(service: string, instance: any): IServiceProvider {
        this.services.set(service, instance);
        return this;
    }
}