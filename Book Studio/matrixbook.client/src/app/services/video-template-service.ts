import { Injectable } from '@angular/core';
import { DurationTypes, MediaElement, MediaTypes, StartTypes, VideoTemplate } from '../models/video-template';
import { ApiService } from './api-service';

@Injectable({ providedIn: 'root' })
export class VideoTemplateService {
    templates: VideoTemplate[] = [
        {
            id: 'template-001',
            name: 'Sample Template',
            description: 'A demo video template',
            thumbnail: '',
            resolution: { width: 1920, height: 1080 },
            dateCreated: new Date(),
            dateUpdated: new Date(),
            tracks: [
                {
                    id: 'track-3',
                    name: 'logo-text',
                    elements: [
                        <MediaElement>{
                            name: 'Logo',
                            type: MediaTypes.text,
                            start: 0,
                            duration: 30,
                            durationType: DurationTypes.full,
                            fadeIn: 0.3,
                            fadeOut: 0.3,
                            alpha: 0.9,
                            zIndex: 2,
                            content: 'logo',
                            family: 'Arial',
                            fontSize: 24,
                            weight: 'bold',
                            color: '#ffffff',
                            location: { x: 100, y: 100 },
                            align: 'center'
                        }
                    ]
                },
                {
                    id: 'track-3',
                    name: 'logo-image',
                    elements: [
                        <MediaElement>{
                            name: 'Logo Image',
                            type: MediaTypes.image,
                            start: 0,
                            duration: 30,
                            durationType: DurationTypes.full,
                            fadeIn: 0.5,
                            fadeOut: 0.5,
                            alpha: 1,
                            zIndex: 1,
                            inputSource: 'image.png',
                            location: { x: 200, y: 200 },
                            size: { width: 300, height: 200 }
                        }
                    ]
                },
                {
                    id: 'track-3',
                    name: 'logo-text',
                    elements: [
                        <MediaElement>{
                            name: 'Main',
                            type: MediaTypes.text,
                            start: 3,
                            duration: 30,
                            durationType: DurationTypes.auto,
                            fadeIn: 0.3,
                            fadeOut: 0.3,
                            alpha: 0.9,
                            zIndex: 2,
                            content: 'logo',
                            family: 'Arial',
                            fontSize: 24,
                            weight: 'bold',
                            color: '#ffffff',
                            location: { x: 100, y: 100 },
                            align: 'center'
                        }
                    ]
                },
                {
                    id: 'track-2',
                    name: 'audio',
                    elements: [
                        <MediaElement>{
                            name: 'Intro Image',
                            type: MediaTypes.image,
                            start: 0,
                            duration: 3,
                            fadeIn: 0.5,
                            fadeOut: 0.5,
                            alpha: 1,
                            zIndex: 1,
                            inputSource: 'image.png',
                            location: { x: 200, y: 200 },
                            size: { width: 300, height: 200 }
                        },
                        <MediaElement>{
                            name: 'Outro Image',
                            type: MediaTypes.image,
                            start: 0,
                            startType: StartTypes.beforeEnd,
                            duration: 3,
                            fadeIn: 0.5,
                            fadeOut: 0.5,
                            alpha: 1,
                            zIndex: 1,
                            inputSource: 'image.png',
                            location: { x: 200, y: 200 },
                            size: { width: 300, height: 200 }
                        }
                    ]
                },
                {
                    id: 'track-2',
                    name: 'audio',
                    elements: [
                        <MediaElement>{
                            name: 'Intro',
                            type: MediaTypes.audio,
                            start: 0,
                            startType: StartTypes.absolute,
                            duration: 3,
                            fadeIn: 0.5,
                            fadeOut: 0.5,
                            alpha: 1,
                            inputSource: 'background.mp3',
                            zIndex: 0
                        },
                        <MediaElement>{
                            name: 'main audio',
                            type: MediaTypes.combine,
                            start: 0,
                            startType: StartTypes.afterPrevious,
                            duration: 3,
                            durationType: DurationTypes.auto,
                            fadeIn: 0,
                            fadeOut: 0,
                            alpha: 1,
                            inputSource: 'background.mp3',
                            zIndex: 0,
                            children: [],
                            transition: {
                                effect: 'fade',
                                duration: 0.5
                            }
                        },
                        <MediaElement>{
                            name: 'outro audio',
                            type: MediaTypes.audio,
                            start: 0,
                            startType: StartTypes.afterPrevious,
                            duration: 3,
                            durationType: DurationTypes.fixed,
                            fadeIn: 0,
                            fadeOut: 0,
                            alpha: 1,
                            inputSource: 'background.mp3',
                            zIndex: 0
                        }
                    ]
                }
            ]
        }
    ];

    constructor(private readonly apiService: ApiService) { }

    async getTemplates(page = 1, pageSize = 30): Promise<VideoTemplate[]> {
        // return this.templates.slice((page - 1) * pageSize, page * pageSize);
        return this.apiService.get(`template/${page}/${pageSize}`);
    }

    async getById(id: string): Promise<VideoTemplate> {
        return this.apiService.get(`template/${id}`);
        // return this.templates.find(t => t.id === id)!;
    }

    async add(template: VideoTemplate): Promise<VideoTemplate> {
        return this.apiService.post('template', template);
    }

    async update(template: VideoTemplate): Promise<VideoTemplate> {
        return this.apiService.put(`template/update`, template);
    }

    async delete(id: string): Promise<boolean> {
        return this.apiService.delete(`template/${id}`);
    }
}