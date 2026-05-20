import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ILogger } from 'src/app/models/logger';
import { GroupedNavItem, NavItem } from 'src/app/models/nav-item';
import { MediaElement, VideoTemplate } from 'src/app/models/video-template';
import { IdGeneratorService } from 'src/app/services/id-generator';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { VideoTemplateService } from 'src/app/services/video-template-service';
import { RibbonComponent } from '../toolbar/ribbon.component';
import { VideoTemplatePropertyComponent } from './video.template.property.component';
import { VideoTimelineComponent } from './video.timeline.component';

@Component({
    selector: 'mtx-video-template-editor',
    templateUrl: 'video.template.editor.component.html',
    imports: [
        CommonModule, FormsModule, RouterModule,
        RibbonComponent, VideoTimelineComponent,
        VideoTemplatePropertyComponent]
})
export class VideoTemplateEditorComponent implements OnInit {
    fileGroup = 'file';
    homeGroup = 'home';
    navItems: NavItem[] = [
        { name: 'new', text: 'New', icon: 'file', group: this.fileGroup, action: () => { this.new(); } },
        { name: 'save', text: 'Save', icon: 'floppy', group: this.fileGroup, action: () => { this.save(); } },
        { name: 'openFolder', text: 'Open Video Folder', icon: 'folder2-open', group: this.fileGroup, action: async () => { await this.openVideoFolder(); } },
        { name: 'file-separator1', isSeparator: true, group: this.fileGroup },
        { text: 'Close', icon: 'backspace', group: this.fileGroup, action: () => { } },

        // home
        { name: 'new', text: 'New', icon: 'file', group: this.homeGroup, action: () => { this.new(); } },
        { name: 'save', text: 'Save', icon: 'floppy', group: this.homeGroup, action: () => { this.save(); } },
        { name: 'sep1', isSeparator: true, group: this.homeGroup },
        { name: 'addTrack', text: 'Add Track', icon: 'node-plus', group: this.homeGroup, action: () => { this.addTrack(); } },
        { name: 'addBlock', text: 'Add Block', icon: 'plus-square', group: this.homeGroup, action: () => { } },
        { name: 'sep2', isSeparator: true, group: this.homeGroup },
        { name: 'export', text: 'Export', icon: 'send text-success', group: this.homeGroup, action: () => { this.export(); } }

    ];
    fileNavItems?: NavItem[] = this.filterByGroup(this.navItems, this.fileGroup);
    mainNav?: GroupedNavItem[] = [
        { title: 'Home', items: this.filterByGroup(this.navItems, this.homeGroup), target: '#home-tab', isActive: true },
    ];
    @Input() model!: VideoTemplate;

    isNew = false;
    selectedElement?: MediaElement;
    logger?: ILogger;

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly idGeneratorService: IdGeneratorService,
        private readonly templateService: VideoTemplateService,
        private readonly notificationService: NotificationService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('VideoTemplateEditor');
    }

    ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            let videoId = params['id?'];
            if (videoId) {
                this.model = await this.templateService.getById(videoId);
            } else {
                this.model = {
                    id: this.idGeneratorService.generateId(),
                    name: '',
                    description: '',
                    thumbnail: '',
                    resolution: {
                        width: 1920,
                        height: 1080
                    },
                    tracks: [],
                    dateCreated: new Date(),
                    dateUpdated: new Date()
                };

                this.isNew = true;
            }
        });
    }

    filterByGroup(items: NavItem[], group: string): NavItem[] {
        return items.filter(item => item.group === group);
    }

    async new() {
    }

    async save() {
        this.logger?.log('Saving video template:', this.model);
        if (this.isNew) {
            await this.templateService.add(this.model);
            this.router.navigate(['templates', 'edit', this.model.id]);
        } else {
            await this.templateService.update(this.model);
        }
        this.notificationService?.showSuccess(
            'Template Updated',
            `Template 《${this.model?.name}》is updated successfully.`,
        );
    }

    async openVideoFolder() {
    }

    async export() {
    }

    addTrack() {
        const newTrack = {
            id: this.idGeneratorService.generateId(),
            name: 'New Track',
            elements: []
        };
        this.model.tracks.push(newTrack);
        console.log('Track added:', newTrack);
    }

    onElementSelected(element: MediaElement) {
        this.selectedElement = element;
    }
}