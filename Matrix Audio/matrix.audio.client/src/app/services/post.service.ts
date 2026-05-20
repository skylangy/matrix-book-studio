import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { ApiService } from './api.service';
import { LoggingService } from './logging.service';
import { Post } from '../models/post';

@Injectable({
    providedIn: 'root'
})
export class PostService {
    private logger: ILogger

    constructor(
        private apiService: ApiService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('PostService');
    }

    async search(keyword: string, page = 1, pageSize = 9): Promise<Post[]> {
        return this.apiService.get(`post/search/${keyword}/${page}/${pageSize}`);
    }

    async getAllPosts(page = 1, pageSize = 9): Promise<Post[]> {
        return this.apiService.get(`post/all/${page}/${pageSize}`);
    }

    async getRecents(count = 12): Promise<Post[]> {
        return this.apiService.get(`post/recents/${count}`);
    }

    async getPost(id: string): Promise<Post> {
        return this.apiService.get(`post/${id}`);
    }

    async getPostForEdit(id: string): Promise<Post> {
        return this.apiService.get(`post/for/edit/${id}`);
    }

    async updatePost(post: Post): Promise<Post> {
        return this.apiService.put(`post/update`, post);
    }

    async removeSplash(id: string): Promise<Post> {
        return this.apiService.post(`post/remove/splash/${id}`, {});
    }

    async createPost(post: Post): Promise<Post> {
        return this.apiService.post('post', post);
    }

    async deletePost(id: string): Promise<any> {
        return this.apiService.delete(`post/${id}`);
    }
}