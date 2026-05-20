
export interface IRouter {
    navigate(commands: any[], extras?: any): Promise<boolean>;
    get url(): string
}