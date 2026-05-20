import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { IApiService } from '../models/api-service';
import { ConfigurationService } from './config-service';

@Injectable({
    providedIn: 'root',
})
export class ApiService implements IApiService {
    private headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    constructor(private http: HttpClient,
        private configService: ConfigurationService) {

    }

    public getFullUrl(endpoint: string): string {
        return `${this.configService.apiUrl}/${endpoint}`;
    }

    // GET request
    get(endpoint: string): Promise<any> {
        let path = this.getFullUrl(endpoint);
        let response = this.http.get(path);
        return lastValueFrom(response);
    }

    getFile(endpoint: string): Promise<any> {
        let path = this.getFullUrl(endpoint);
        let response = this.http.get(path, { responseType: 'blob' });
        return lastValueFrom(response);
    }

    // POST request
    post(endpoint: string, data: any): Promise<any> {
        let response = this.http.post(this.getFullUrl(endpoint), data);
        return lastValueFrom(response);
    }

    // PUT request
    put(endpoint: string, data: any): Promise<any> {
        let response = this.http.put(this.getFullUrl(endpoint), data, { headers: this.headers });
        return lastValueFrom(response)
    }

    // DELETE request
    delete(endpoint: string): Promise<any> {
        let response = this.http.delete(this.getFullUrl(endpoint));
        return lastValueFrom(response);
    }
}
