import { Injectable } from '@angular/core';
import { MediaResource, MediaResourceGroup } from '../models/media-resource';
import { VideoMeta } from '../models/video-meta';
import { ApiService } from './api-service';

@Injectable({ providedIn: 'root' })
export class VideoService {
    constructor(private apiService: ApiService) { }

    async getAllVideos(page = 1, pageSize = 30): Promise<VideoMeta[]> {
        return this.apiService.get(`video/${page}/${pageSize}`);
    }

    async getVideoById(id: string): Promise<VideoMeta> {
        return this.apiService.get(`video/${id}`);
    }

    async addVideo(video: VideoMeta): Promise<VideoMeta> {
        return this.apiService.post('video', video);
    }

    async updateVideo(video: VideoMeta): Promise<VideoMeta> {
        return this.apiService.post(`video`, video);
    }

    async deleteVideo(id: string): Promise<boolean> {
        return this.apiService.delete(`video/${id}`);
    }

    async export(video: VideoMeta): Promise<string> {
        return this.apiService.post('video/export', video);
    }

    async exportVideo(video: VideoMeta): Promise<string> {
        return this.apiService.post('video/export/video', video);
    }

    async getAllMediaResources(): Promise<MediaResource[]> {
        return this.apiService.get('video/media/resources');
    }

    async getResourcesByType(type: string): Promise<MediaResource[]> {
        return this.apiService.get(`video/media/resources/type/${type}`);
    }

    async getResourceGroupsByType(type: string): Promise<MediaResourceGroup[]> {
        return this.apiService.get(`video/media/resource/groups/${type}`);
    }

    async getMediaResourceById(id: string): Promise<MediaResource> {
        return this.apiService.get(`video/media/resources/${id}`);
    }

    async getMediaResourceUrl(id: string): Promise<string> {
        return this.apiService.getFullUrl(`video/media/resources/url/${id}`);
    }

    async addMediaResource(resource: MediaResource): Promise<MediaResource> {
        return this.apiService.post('video/media/resources', resource);
    }

    async updateMediaResource(id: string, resource: MediaResource): Promise<MediaResource> {
        return this.apiService.put(`video/media/resources/${id}`, resource);
    }

    async deleteMediaResource(id: string): Promise<boolean> {
        return this.apiService.delete(`video/media/resources/${id}`);
    }

    async scan() {
        return this.apiService.get('video/media/scan');
    }

    async updateImages() {
        return this.apiService.post('video/media/update/images', {});
    }

    async openVideoFolder(video: VideoMeta): Promise<void> {
        return this.apiService.post('video/openVideoFolder', video);
    }
}