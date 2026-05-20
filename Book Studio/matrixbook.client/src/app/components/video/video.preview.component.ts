import { Component, Input, OnInit } from '@angular/core';
import { BackgroundImageDirective } from 'src/app/directives/background-image.directive';
import { VideoMeta } from 'src/app/models/video-meta';
import { VideoService } from 'src/app/services/video-service';

@Component({
    selector: 'mtx-video-preview',
    templateUrl: 'video.preview.component.html',
    imports: [BackgroundImageDirective]
})
export class VideoPreviewComponent implements OnInit {
    defaultPreviewImage = './assets/images/video/Youtube-Short-Bg-1.jpg';
    @Input() model: VideoMeta | undefined;
    @Input() style? = '';

    constructor(private readonly videoService: VideoService) { }

    async ngOnInit() {
        for (const image of this.model?.contentImages || []) {
            if (!image.fullUrl) {
                image.fullUrl = await this.getResourceUrl(image.id);
            }
        }

        if (this.model?.logo?.image && !this.model.logo.image.fullUrl) {
            this.model.logo.image.fullUrl = await this.getResourceUrl(this.model.logo.image.id);
        }
    }

    get previewImage(): string {
        if (this.model?.contentImages && this.model.contentImages.length > 0) {
            return this.model.contentImages[0].fullUrl! || this.defaultPreviewImage;
        }
        return this.defaultPreviewImage;
    }

    get logoImage(): string {
        if (this.model?.logo?.image) {
            return this.model.logo.image.fullUrl || this.defaultPreviewImage;
        }
        return this.defaultPreviewImage;
    }

    async getResourceUrl(resourceId: string): Promise<string> {
        return this.videoService.getMediaResourceUrl(resourceId);
    }
}