import { Pipe, PipeTransform } from '@angular/core';
import { AlbumService } from '../services/album.service';

@Pipe({ name: 'avatar', })
export class PersonAvatarPipe implements PipeTransform {

    constructor(private albumService: AlbumService) { }

    transform(value?: string): string {
        if (!value) {
            return 'assets/images/defaultAvatar.png';
        }
        return this.albumService.getImageUrl(value);
    }
}