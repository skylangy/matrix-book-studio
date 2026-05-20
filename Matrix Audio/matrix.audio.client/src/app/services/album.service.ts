import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Album, AlbumCollection } from '../models/album';
import { AlbumGroup } from '../models/album-group';
import { Category } from '../models/category';
import { Episode } from '../models/episode';
import { ILogger } from '../models/logger';
import { Result } from '../models/result';
import { Tag } from '../models/tag';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { ConfigurationService } from './config.service';
import { LoggingService } from './logging.service';

@Injectable({
    providedIn: 'root'
})
export class AlbumService {
    private logger: ILogger

    constructor(
        private configService: ConfigurationService,
        private authService: AuthService,
        private apiService: ApiService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('AlbumService');
    }

    getAlbumStreamUrl(id: string): string {
        return `${this.configService.apiUrl}/album/stream/episode/${id}`;
    }

    getImageUrl(id: string): string {
        return `${this.configService.apiUrl}/image/${id}`;
    }

    async getAlbums(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/all/${page}/${pageSize}`);
    }

    async getRecents(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/recents/${page}/${pageSize}`);
    }

    async getSuggested(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/suggested/${page}/${pageSize}`);
    }

    async getMostLikes(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/by/likes/${page}/${pageSize}`);
    }

    async getByPlaysWeek(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/by/plays/week/${page}/${pageSize}`);
    }

    async getByPlaysMonth(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/by/plays/month/${page}/${pageSize}`);
    }

    async getByPlaysYear(page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/by/plays/year/${page}/${pageSize}`);
    }

    async getHistory(page = 1, pageSize = 9): Promise<Album[]> {
        let userId = this.authService.user?.id!;
        return this.apiService.get(`user/history/${userId}/${page}/${pageSize}`);
    }

    async getFavorites(page = 1, pageSize = 9): Promise<Album[]> {
        let userId = this.authService.user?.id!;
        return this.apiService.get(`user/favorites/${userId}/${page}/${pageSize}`);
    }

    async getPlayList(page = 1, pageSize = 9): Promise<Album[]> {
        let userId = this.authService.user?.id!;
        return this.apiService.get(`user/playlist/${userId}/${page}/${pageSize}`);
    }

    async getDownloadAlbums(page = 1, pageSize = 9): Promise<Album[]> {
        let userId = this.authService.user?.id!;
        return this.apiService.get(`user/record/download/album/${userId}/${page}/${pageSize}`);
    }

    async getDownloadEpisodes(page = 1, pageSize = 9): Promise<Album[]> {
        let userId = this.authService.user?.id!;
        return this.apiService.get(`user/record/download/episode/${userId}/${page}/${pageSize}`);
    }

    async getByCategory(categoryId: string, page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/category/${categoryId}/${page}/${pageSize}`);
    }

    async getByTag(tagId: string, page = 1, pageSize = 9): Promise<Album[]> {
        return this.apiService.get(`album/tag/${tagId}/${page}/${pageSize}`);
    }

    async getByDate(start: Date, end: Date, page = 1, pageSize = 500): Promise<Album[]> {
        if (!end) {
            end = new Date(start.getTime() + 24 * 60 * 60 * 1000);
        }
        return this.apiService.get(`album/by/date/${start.toISOString()}/${end.toISOString()}/${page}/${pageSize}`);
    }

    async search(keyword: string, page = 1, pageSize = 9): Promise<Album[]> {
        let params = new HttpParams()
            .set('keyword', keyword)
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        return this.apiService.get(`album/search`, params);
    }

    async getAlbum(id: string): Promise<Album> {
        return this.apiService.get(`album/${id}`);
    }

    async getEpisode(albumId: string, episodeId: string): Promise<Episode> {
        return this.apiService.get(`album/episode/${albumId}/${episodeId}`);
    }

    async getEpisodeStream(albumId: string, episodeId: string, startByte: number, chunkSize: number): Promise<Blob> {
        return this.apiService.stream(`album/episode/stream/${albumId}/${episodeId}`, startByte, chunkSize);
    }

    async streamEpisode(episodeId: string, startByte: number, chunkSize: number): Promise<Blob> {
        return this.apiService.stream(`album/stream/episode/${episodeId}`, startByte, chunkSize);
    }

    async getEpisodeUrl(albumId: string, episodeId: string) {
        return this.apiService.get(`album/episode/url/${albumId}/${episodeId}`);
    }

    async scanAlbumMetadata(): Promise<Result> {
        return this.apiService.post('album/scan/metadata', {});
    }

    async fixAlbumSplash(): Promise<void> {
        return this.apiService.post('image/fix/album/splashes', {});
    }

    async convertPngToJpg(): Promise<Result> {
        return this.apiService.post('image/convert/png/to/jpg', {});
    }

    getCategories(): Promise<Category[]> {
        return this.apiService.get('album/categories');
    }

    getTags(): Promise<Tag[]> {
        return this.apiService.get('album/tags');
    }

    getGroups(): Promise<AlbumGroup[]> {
        return this.apiService.get('album/groups');
    }

    async updateAlbum(album: Album): Promise<void> {
        return this.apiService.post('album/update', album);
    }

    async getAlbumCollections(page: number, pageSize: number): Promise<AlbumCollection[]> {
        return this.apiService.get(`albumcollection/${page}/${pageSize}`);
    }

    async getAlbumCollectionsAdmin(page: number, pageSize: number): Promise<AlbumCollection[]> {
        return this.apiService.get(`albumcollection/admin/${page}/${pageSize}`);
    }

    async getAlbumCollection(id: string): Promise<AlbumCollection> {
        return this.apiService.get(`albumcollection/${id}`);
    }

    async getAlbumCollectionAdmin(id: string): Promise<AlbumCollection> {
        return this.apiService.get(`albumcollection/admin/${id}`);
    }

    async updateAlbumCollection(collection: AlbumCollection): Promise<Result> {
        return this.apiService.post('albumcollection', collection);
    }

    async deleteAlbumCollection(id: string): Promise<Result> {
        return this.apiService.delete(`albumcollection/${id}`);
    }

    sortEpisodesByChapter(episodes: Episode[]): Episode[] {
        return episodes.sort((a, b) => this.chineseChapterSort(a.title!, b.title!));
    }

    private chineseChapterSort(a: string, b: string): number {
        const chapterNumberA = this.parseChineseNumeral(this.extractChineseNumber(a));
        const chapterNumberB = this.parseChineseNumeral(this.extractChineseNumber(b));

        if (chapterNumberA < chapterNumberB) {
            return -1;
        } else if (chapterNumberA > chapterNumberB) {
            return 1;
        } else {
            return 0;
        }
    }

    private parseChineseNumeral(chineseNumeral: string): number {
        const chineseNumbers: Record<string, number> = {
            '零': 0, '一': 1, '二': 2, '三': 3, '四': 4, '五': 5,
            '六': 6, '七': 7, '八': 8, '九': 9, '十': 10,
            '百': 100, '千': 1000, '万': 10000, '亿': 100000000
        };

        const isDigitSequence = /^[零一二三四五六七八九]+$/.test(chineseNumeral);
        if (isDigitSequence) {
            const arabicStr = chineseNumeral
                .split('')
                .map(c => chineseNumbers[c])
                .join('');
            const result = parseInt(arabicStr, 10);
            console.log(`Parsing digit-style Chinese numeral: ${chineseNumeral} -> ${result}`);
            return result;
        }

        let result = 0;
        let temp = 0;

        for (let i = 0; i < chineseNumeral.length; i++) {
            const char = chineseNumeral[i];
            const num = chineseNumbers[char];

            if (num != null) {
                if (num < 10) {
                    temp += num;
                } else {
                    if (temp === 0) temp = 1;
                    result += temp * num;
                    temp = 0;
                }
            }
        }

        result += temp;
        console.log(`Parsing unit-style Chinese numeral: ${chineseNumeral} -> ${result}`);
        return result;
    }

    private extractChineseNumber(chapterNumber: string): string {
        const match = chapterNumber.match(/第([\u4e00-\u9fa5]+)[章|回]/);
        return match ? match[1] : '';
    }
}