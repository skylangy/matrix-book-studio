import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { VideoTemplate } from 'src/app/models/video-template';
import { NotificationService } from 'src/app/services/notification-service';
import { VideoTemplateService } from 'src/app/services/video-template-service';

@Component({
    selector: 'mtx-video-template-card',
    templateUrl: 'video.template.card.component.html',
    imports: [CommonModule, FormsModule, RouterModule]
})
export class VideoTemplateCardComponent implements OnInit {
    @Input() model!: VideoTemplate;
    @Output() templateDeleted = new EventEmitter<string>();
    defaultImage = 'assets/images/splashes/photo-16.jpg';

    constructor(private readonly videoTemplateService: VideoTemplateService,
        private readonly notificationService: NotificationService,

    ) { }

    ngOnInit() { }

    get hasImage(): boolean {
        return this.model.thumbnail !== undefined && this.model.thumbnail !== '';
    }

    get image(): string {
        return this.model.thumbnail;
    }

    async deleteTemplate(id: string) {
        let deleted = await this.videoTemplateService.delete(id);
        if (deleted) {
            this.notificationService.showSuccess('Template Deleted', `Template has been deleted successfully.`);
        } else {
            this.notificationService.showFail('Delete Failed', `Failed to delete the template.`);
        }

        this.templateDeleted.emit(id);
    }

    onImageDropped(event: any) {
    }

    handleImageError(event: Event) {
    }
}