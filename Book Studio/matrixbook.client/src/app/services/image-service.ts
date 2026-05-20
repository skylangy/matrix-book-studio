import { Injectable } from '@angular/core';
import { ApiService } from './api-service';

@Injectable({
    providedIn: 'root'
})
export class ImageService {
    constructor(private apiService: ApiService) { }

    getImageUrl(bookName: string, imageFile: string) {
        return this.apiService.getFullUrl(this.buildPath(bookName, imageFile));
    }

    getAuthorImageUrl(authorName: string, imageFile: string) {
        return this.apiService.getFullUrl(`image/author/${authorName}/${imageFile}`);
    }

    getImageById(imageId: string) {
        return this.apiService.get(`image/by/id/${imageId}`);
    }

    getImageUrlById(imageId: string) {
        return this.apiService.getFullUrl(`image/by/id/${imageId}`);
    }

    async getImageResourceById(imageId: string) {
        return await this.apiService.get(`image/resource/by/id/${imageId}`);
    }

    async deleteImage(bookName: string, imageFile: string) {
        let path = this.buildPath(bookName, imageFile);
        return await this.apiService.delete(path);
    }

    generateId() {
        return Math.random().toString(36).substring(2);
    }

    private buildPath(bookName: string, imageFile: string) {
        return `image/${bookName}/${imageFile}`;
    }
}