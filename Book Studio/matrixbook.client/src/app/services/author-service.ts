import { Injectable } from '@angular/core';
import { ApiService } from './api-service';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging-services';
import { Author } from '../models/author';
import { IPagedList } from '../models/pagedlist';
import { IAuthorService } from '../models/author-service';

@Injectable({
    providedIn: 'root'
})
export class AuthorService implements IAuthorService {
    private logger?: ILogger;

    constructor(private apiService: ApiService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('AuthorService');
    }

    getAuthors(): Promise<Author[]> {
        return this.apiService.get('author/all');
    }

    getPagedAuthors(page: number = 1, pageSize: number = 12): Promise<IPagedList<Author>> {
        return this.apiService.get(`author/paged/${page}/${pageSize}`);
    }

    getAuthor(id: string): Promise<Author> {
        return this.apiService.get(`author/${id}`);
    }

    search(keyword: string, page: number = 1, pageSize: number = 12): Promise<IPagedList<Author>> {
        return this.apiService.get(`author/search/${keyword}/${page}/${pageSize}`);
    }

    createAuthor(author?: Author): Promise<Author> {
        if (!author) {
            throw new Error('Author is undefined');
        }
        if (!author.id) {
            author.id = '';
        }

        return this.apiService.post(`author`, author);
    }

    updateAuthor(author?: Author): Promise<Author> {
        this.logger?.log('Update author', author?.name);

        if (author?.id === undefined) {
            this.logger?.log('Go to Create author', author?.name);
            return this.createAuthor(author);
        }

        if (!author) {
            throw new Error('Author is undefined');
        }

        return this.apiService.put(`author/update`, author);
    }

    removeAuthor(id: string): Promise<boolean> {
        return this.apiService.delete(`author/${id}`);
    }

    clearImages(id: string): Promise<void> {
        return this.apiService.post(`author/clear/image/${id}`, {});
    }

    openFolder(id: string): Promise<void> {
        return this.apiService.post(`author/openAuthorFolder`, { id: id });
    }

    async syncAuthors(): Promise<void> {
        await this.apiService.post('author/publish/library/all', {});
    }

    async syncAuthor(authorId: string): Promise<void> {
        await this.apiService.post(`author/publish/library/${authorId}`, {});
    }

    async fixAuthorSplashes(): Promise<void> {
        await this.apiService.post('image/fix/splashes/author/missing', {});
    }
}