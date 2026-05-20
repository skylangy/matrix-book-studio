import { Pipe, PipeTransform } from '@angular/core';
import { AlbumService } from '../services/album.service';
import { AppSettingService } from '../services/appsetting.service';

@Pipe({ name: 'image', })
export class ImageUrlPipe implements PipeTransform {

    constructor(private albumService: AlbumService,
        private readonly appSettingService: AppSettingService
    ) { }

    transform(value?: string): string {
        if (!value) {
            return this.appSettingService.defaultSplash;
        }
        return this.albumService.getImageUrl(value);
    }
}