import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, lastValueFrom, Observable } from 'rxjs';
import { IApiService } from '../models/api-service';
import { ConfigurationService } from './config.service';

@Injectable({
    providedIn: 'root',
})
export class ApiService implements IApiService {

    constructor(
        private http: HttpClient,
        private configService: ConfigurationService,

    ) { }

    public getFullUrl(endpoint: string): string {
        return `${this.configService.apiUrl}/${endpoint}`;
    }

    // GET request
    get(endpoint: string, params: HttpParams = new HttpParams()): Promise<any> {
        let path = this.getFullUrl(endpoint);
        let response = this.http.get(path, { params });
        let result = firstValueFrom(response);
        return result;
    }

    getFile(endpoint: string): Promise<any> {
        let path = this.getFullUrl(endpoint);
        let response = this.http.get(path, { responseType: 'blob' });
        return lastValueFrom(response);
    }

    stream(endpoint: string, startByte: number, chunkSize: number): Promise<any> {
        let path = this.getFullUrl(endpoint);
        let end = startByte + chunkSize - 1;
        const headers = new HttpHeaders()
            .set('Range', `bytes=${startByte}-${end}`);

        const response = this.http.get(path, { headers, responseType: 'blob' });
        return lastValueFrom(response);
    }

    // POST request
    post(endpoint: string, data: any): Promise<any> {
        let response = this.http.post(this.getFullUrl(endpoint), data);
        return lastValueFrom(response);
    }

    postRaw(endpoint: string, data: any): Observable<any> {
        return this.http.post(this.getFullUrl(endpoint), data, { responseType: 'json' });
    }

    // PUT request
    put(endpoint: string, data: any): Promise<any> {
        let response = this.http.put(this.getFullUrl(endpoint), data);
        return lastValueFrom(response)
    }

    // DELETE request
    delete(endpoint: string): Promise<any> {
        let response = this.http.delete(this.getFullUrl(endpoint));
        return lastValueFrom(response);
    }

    async downloadFile(endpoint: string, data: any, fileName?: string): Promise<any> {
        try {
            let response = await lastValueFrom(
                this.http.post(this.getFullUrl(endpoint), data, { responseType: 'blob', observe: 'response' })
            );

            const contentDisposition = response.headers.get('Content-Disposition');
            const destFileName = this.extractFileNameFromHeader(contentDisposition) || fileName || 'downloaded-file';


            let url = window.URL.createObjectURL(response.body!);

            let a = document.createElement('a');
            a.href = url;
            a.download = destFileName;

            document.body.appendChild(a);
            a.click();

            // Clean up
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            //this.logger.error('File download failed', error);
            throw error;
        }
    }

    private extractFileNameFromHeader(contentDisposition: string | null): string | null {
        if (!contentDisposition) {
            return null;
        }
        const filenameRegex = /filename\*\s*=\s*UTF-8''(.+)/i;
        const matchExtended = contentDisposition.match(filenameRegex);
        if (matchExtended) {
            return decodeURIComponent(matchExtended[1]);
        }

        // Fallback to regular filename
        const fallbackRegex = /filename\s*=\s*"([^"]+)"/i;
        const matchRegular = contentDisposition.match(fallbackRegex);
        if (matchRegular) {
            return matchRegular[1];
        }

        return null; // No filename found
    }
}
