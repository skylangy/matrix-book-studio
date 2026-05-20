import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiService } from './api-service';

@Injectable({
    providedIn: 'root'
})
export class FileUploadService {

    constructor(private apiService: ApiService) { }

    uploadBookImage(file: File, bookName: string): Promise<any> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        formData.append('bookName', bookName);

        return this.apiService.post(`image`, formData);
    }

    uploadAuthorImage(file: File, authorName: string): Promise<any> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);
        formData.append('authorName', authorName);

        return this.apiService.post(`image/author`, formData);
    }
}