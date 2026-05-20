import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Artist } from '../models/artist';
import { Result } from '../models/result';

@Injectable({ providedIn: 'root' })
export class ArtistService {
    constructor(private apiService: ApiService) { }

    async getAllArtists(page = 1, pageSize = 12): Promise<Artist[]> {
        return this.apiService.get(`artist/all/${page}/${pageSize}`);
    }

    async getRecentArtists(page = 1, pageSize = 9): Promise<Artist[]> {
        return this.apiService.get(`artist/recents/${page}/${pageSize}`);
    }

    async getPopularArtists(page = 1, pageSize = 9): Promise<Artist[]> {
        return this.apiService.get(`artist/popular/${page}/${pageSize}`);
    }

    async getArtist(id: string): Promise<Artist> {
        return this.apiService.get(`artist/${id}`);
    }

    async searchArtists(keyword: string, page = 1, pageSize = 9): Promise<Artist[]> {
        return this.apiService.get(`artist/search/${keyword}/${page}/${pageSize}`);
    }

    async deleteArtist(id: string): Promise<void> {
        return this.apiService.delete(`artist/${id}`);
    }

    async updateArtist(artist: Artist): Promise<void> {
        return this.apiService.post('artist/update', artist);
    }

    async fixArtistAvatar(): Promise<void> {
        return this.apiService.post('image/fix/artist/avatars', {});
    }

    async scanArtistMetadata(): Promise<Result> {
        return this.apiService.post('artist/scan/metadata', {});
    }
}