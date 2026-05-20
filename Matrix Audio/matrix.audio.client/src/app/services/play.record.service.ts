import { Injectable } from '@angular/core';
import { UserService } from './user.service';

@Injectable({
    providedIn: 'root'
})
export class PlayRecordService {
    private lastPlayedAlbum: string = 'LastPlayedAlbum';
    private lastPlayedEpisode: string = 'LastPlayedEpisode';

    constructor(private userService: UserService) { }

    async recordEpisode(albumId: string, episodeId: string, position: number, duration: number) {
        localStorage.setItem(this.lastPlayedAlbum, JSON.stringify({
            albumId,
            episodeId,
            position,
            duration,
            date: new Date(),
            userId: this.userService.userId
        }));

        localStorage.setItem(this.lastPlayedEpisode, JSON.stringify({
            albumId,
            episodeId,
            position,
            duration,
            date: new Date(),
            userId: this.userService.userId
        }));

        localStorage.setItem(`${albumId}`, JSON.stringify({
            episodeId,
            duration,
            date: new Date(),
            userId: this.userService.userId
        }));

        localStorage.setItem(`${albumId}-${episodeId}`, JSON.stringify({
            position,
            duration,
            date: new Date(),
            userId: this.userService.userId
        }));

        await this.userService.updatePlayRecord(albumId, episodeId, position, duration);
    }

    async getLastPlayedAlbum() {
        let record = localStorage.getItem(this.lastPlayedAlbum);
        return record ? JSON.parse(record) : undefined;
    }

    async getLastPlayedEpisode() {
        let record = localStorage.getItem(this.lastPlayedEpisode);
        return record ? JSON.parse(record) : undefined;
    }

    async getAlbumRecord(albumId: string) {
        let record = localStorage.getItem(`${albumId}`);
        return record ? JSON.parse(record) : undefined;
    }

    async getEpisodeRecord(albumId: string, episodeId: string) {
        let record = localStorage.getItem(`${albumId}-${episodeId}`);
        return record ? JSON.parse(record) : undefined;
    }
}