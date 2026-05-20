import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { ImageResource } from '../models/image-resource';

@Injectable({ providedIn: 'root' })
export class ImageService {
    defaultAvatar = 'assets/images/avatars/boy-10.png';
    avatarPath = 'assets/images/avatars/';
    defaultArtistAvatar = 'assets/images/defaultAvatar.png';
    defaultAlbumSplash = 'assets/images/background/bg-2.png';

    constructor(private readonly apiService: ApiService) { }

    get defaultArtistAvatarUrl(): string {
        return this.defaultArtistAvatar;
    }

    get defaultAlbumSplashUrl(): string {
        return this.defaultAlbumSplash;
    }

    getAvatarUrl(image: string) {
        if (!image) {
            return this.defaultAvatar;
        }
        return `${this.avatarPath}${image}`;
    }

    async uploadPostSplash(file: File): Promise<ImageResource> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);

        const imageResource = await this.apiService.post(`image/post`, formData);
        return imageResource;
    }

    async deleteImage(id: string): Promise<void> {
        await this.apiService.delete(`image/${id}`);
    }
}