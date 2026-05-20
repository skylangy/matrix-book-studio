import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VideoTemplate } from 'src/app/models/video-template';
import { VideoTemplateService } from 'src/app/services/video-template-service';
import { HeaderComponent } from '../header/header.component';
import { LoadingComponent } from '../loading/loading.component';
import { WorkViewComponent } from '../work-view/work.view.component';
import { VideoTemplateCardComponent } from './video.template.card.component';

@Component({
    selector: 'mtx-video-template-manage',
    templateUrl: 'video.template.manage.component.html',
    imports: [CommonModule, FormsModule, HeaderComponent,
        LoadingComponent,
        VideoTemplateCardComponent
    ]
})
export class VideoTemplateManageComponent extends WorkViewComponent {
    filterText = '';
    isLoading = signal(false);
    templates = signal<VideoTemplate[]>([]);

    constructor(private readonly templateService: VideoTemplateService) {
        super();
        this.title = 'Templates';
        this.bannerImage = './assets/images/splashes/photo-16.jpg';
    }

    get filter() {
        return this.filterText;
    }

    set filter(value: string) {
        if (this.filterText !== value) {
            this.filterText = value;
            this.onFilterChanged(value);
        }
    }

    override async ngOnInit() {
        this.isLoading.set(true);
        this.templates.set(await this.templateService.getTemplates());

        this.isLoading.set(false);
    }

    onFilterChanged(filter: string): void {

    }

    createTemplate() {

    }

    onTemplateDeleted(id: string) {
        this.templates.set(this.templates().filter(template => template.id !== id));
    }
}