
export interface IApiService {
    get(endpoint: string): Promise<any>;
    getFile(endpoint: string): Promise<any>;
    post(endpoint: string, data: any): Promise<any>;
    put(endpoint: string, data: any): Promise<any>;
    delete(endpoint: string): Promise<any>;

    getFullUrl(endpoint: string): string;
}