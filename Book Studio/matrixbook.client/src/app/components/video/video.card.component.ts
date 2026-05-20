import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ActiveItem } from 'src/app/models/active-item';
import { VideoMeta } from 'src/app/models/video-meta';
import { NotificationService } from 'src/app/services/notification-service';
import { VideoService } from 'src/app/services/video-service';

@Component({
    selector: 'mtx-video-card',
    templateUrl: 'video.card.component.html',
    imports: [RouterModule, CommonModule, FormsModule]
})
export class VideoCardComponent implements OnInit {
    @Input() video!: VideoMeta;
    @Output() videoDeleted = new EventEmitter<string>();

    imageUrls: string[] = [];

    constructor(
        private readonly videoService: VideoService,
        private readonly notificationService: NotificationService
    ) { }

    async ngOnInit() {
        if (!this.video.introImage?.fullUrl) {
            this.video.introImage!.fullUrl = await this.getResourceUrl(this.video.introImage!.id);
        }
        for (const image of this.video.contentImages || []) {
            if (!image.fullUrl) {
                image.fullUrl = await this.getResourceUrl(image.id);
            }
        }
    }

    get hasImage(): boolean {
        return this.video?.introImage !== undefined;
    }

    get images(): ActiveItem[] {
        if (!this.video) {
            return [];
        }


        const intro = this.video.introImage ? [{ value: this.video.introImage.fullUrl!, name: this.video.introImage.name, isActive: true }] : [];
        // const outro = this.video.outroImage ? [{ value: this.video.outroImage.fullUrl!, name: this.video.outroImage.name, isActive: false }] : [];
        const content = this.video.contentImages ? this.video.contentImages.map(img => ({ value: img.fullUrl!, name: img.name, isActive: false })) : [];

        return [
            ...intro,
            ...content,
            // ...outro
        ];
    }

    get isVertical(): boolean {
        return true;
        //return this.video.height !== undefined && this.video.width !== undefined && this.video.height > this.video.width;
    }

    getVideoImage(): string {
        if (this.video.contentImages && this.video.contentImages.length > 0) {
            return this.video.contentImages[0].fullUrl || '';
        }
        return this.video?.introImage?.fullUrl || '';
    }

    handleFileInput(event: Event) {
        const input = event.target as HTMLInputElement;
        const files = input.files;
        if (files && files.length > 0) {
            // this.processImage(files[0]);
        }
    }

    onImageDropped(event: any) {
    }

    handleImageError(event: Event) {
    }

    async deleteVideo(id: string) {
        let deleted = await this.videoService.deleteVideo(id);
        if (deleted) {
            this.notificationService.showSuccess('Video Deleted', `Video has been deleted successfully.`);
        } else {
            this.notificationService.showFail('Delete Failed', `Failed to delete the video.`);
        }
        this.videoDeleted.emit(id);
    }

    async getResourceUrl(resourceId: string): Promise<string> {
        return this.videoService.getMediaResourceUrl(resourceId);
    }
}