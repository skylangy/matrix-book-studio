import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Font, fonts } from 'src/app/models/font';
import { ILogger } from 'src/app/models/logger';
import { MediaResource } from 'src/app/models/media-resource';
import { GroupedNavItem, NavItem } from 'src/app/models/nav-item';
import { Resolution } from 'src/app/models/resolution';
import { VideoMeta } from 'src/app/models/video-meta';
import { IdGeneratorService } from 'src/app/services/id-generator';
import { KeyService } from 'src/app/services/key-service';
import { LoggingService } from 'src/app/services/logging-services';
import { MemoryService } from 'src/app/services/memory-service';
import { NotificationService } from 'src/app/services/notification-service';
import { ResourceSelectService } from 'src/app/services/resource-select-service';
import { VideoService } from 'src/app/services/video-service';
import { VoiceService } from 'src/app/services/voice-service';
import { VideoResources } from '../../models/video-meta';
import { FoldableTitleComponent } from '../foldable-title/foldable.title.component';
import { RibbonComponent } from '../toolbar/ribbon.component';
import { VideoAudioEditorComponent } from './video.audio.editor.component';
import { VideoLogoEditorComponent } from './video.logo.editor.component';
import { VideoPreviewComponent } from './video.preview.component';
import { VideoResourceTextComponent } from './video.resource.text.component';

@Component({
    selector: 'mtx-video-meta-editor',
    templateUrl: 'video.editor.component.html',
    imports: [CommonModule, FormsModule, RouterModule,
        RibbonComponent,
        VideoPreviewComponent,
        VideoResourceTextComponent,
        VideoLogoEditorComponent,
        VideoAudioEditorComponent,
        FoldableTitleComponent
    ]
})
export class VideoEditorComponent implements OnInit {
    fileGroup = 'file';
    homeGroup = 'home';
    previewImage = './assets/images/video/Youtube-Short-Bg-1.jpg';
    model?: VideoMeta;
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
        { name: 'export', text: 'Export All', icon: 'send text-success', group: this.homeGroup, action: () => { this.export(); } },
        { name: 'exportVideo', text: 'Export Video', icon: 'send text-primary', group: this.homeGroup, action: () => { this.exportVideo(); } }

    ];
    fileNavItems?: NavItem[] = this.filterByGroup(this.navItems, this.fileGroup);
    mainNav?: GroupedNavItem[] = [
        { title: 'Home', items: this.filterByGroup(this.navItems, this.homeGroup), target: '#home-tab', isActive: true },
    ];
    resolutions: Resolution[] = [
        { name: 'YouTube Short', width: 1080, height: 1920, style: 'aspect-box-9x16' },
        { name: 'YouTube HD', width: 1920, height: 1080, style: 'aspect-box-16x9' },
    ];
    selectedResolutionValue: Resolution = this.resolutions[0];
    categories: string[] = ['Bible'];

    showMoreSettings = false;
    selectResourceAction = (resource: MediaResource) => { };
    resourceTypeToSelect: string = 'image';
    isNewVideo = false;
    isExporting = false;
    supportedFonts: Font[] = fonts;
    resourceSelectSubscribe: any;

    private logger?: ILogger;

    constructor(
        private readonly renderer: Renderer2,
        private readonly activateRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly idGeneratorService: IdGeneratorService,
        private readonly videoService: VideoService,
        private readonly notificationService: NotificationService,
        private readonly voiceService: VoiceService,
        private readonly keyService: KeyService,
        private readonly memoryService: MemoryService,
        private readonly resourceSelectService: ResourceSelectService,
        readonly loggingService: LoggingService
    ) {
        this.logger = this.loggingService.getLogger('VideoEditorComponent');
    }

    get selectedResolution(): Resolution {
        return this.selectedResolutionValue;
    }

    set selectedResolution(value: Resolution) {
        this.selectedResolutionValue = value;
        if (this.model) {
            this.model.width = value.width;
            this.model.height = value.height;
        }
    }

    ngOnInit() {
        this.activateRoute.params.subscribe(async params => {
            let videoId = params['id?'];

            if (videoId) {
                this.model = await this.videoService.getVideoById(videoId) || { id: 'unknown' };
                if (this.model !== undefined && this.model.width && this.model.height) {
                    this.selectedResolutionValue = this.resolutions.find(r => r.width === (this.model?.width ?? 0) && r.height === (this.model?.height ?? 0)) || this.resolutions[0];
                } else {
                    this.selectedResolutionValue = this.resolutions[0];
                }
            } else {
                this.model = {
                    id: this.idGeneratorService.generateId('video'),
                    name: this.memoryService.get('videoName') || '',
                    title: this.memoryService.get('videoTitle') || '',
                    subtitle: this.memoryService.get('videoSubtitle') || 'Verse of the Day',
                    category: this.memoryService.get('videoCategory') || this.categories[0],
                    tag: this.memoryService.get('videoTag') || '',
                    speechService: this.voiceService.azureService,
                    language: this.voiceService.azureLanguage,
                    voiceName: this.voiceService.azureVoice,
                    contentImages: [],
                    logo: VideoResources.bibleLogo,
                    introImage: VideoResources.defaultIntroImage,
                    outroImage: VideoResources.defaultOutroImage,
                    introAudio: VideoResources.defaultIntroAudio,
                    bottomNote: VideoResources.getBibleNote(),
                    height: this.selectedResolution.height,
                    width: this.selectedResolution.width
                };
                this.isNewVideo = true;
            }
        });
    }

    @HostListener('document:keydown', ['$event'])
    handleKeyDown(event: KeyboardEvent): void {
        this.keyService.handleKeyDown(event, this, this.keyService.videoEditorName);
    }

    filterByGroup(items: NavItem[], group: string): NavItem[] {
        return items.filter(item => item.group === group);
    }

    popResourceSelector() {
        this.resourceSelectService.openModal();

        this.resourceSelectSubscribe = this.resourceSelectService.onResourceSelected.subscribe(resource => {
            console.log('Resource selected:', resource);
        });
    }

    onResourceSelected(resource: MediaResource) {
        console.log('Resource selected:', resource);
        if (this.selectResourceAction) {
            this.selectResourceAction(resource);
        }
    }

    toggleMoreSettings() {
        this.showMoreSettings = !this.showMoreSettings;
    }

    selectIntroImage() {
        this.resourceTypeToSelect = 'image';
        this.popResourceSelector();
        this.selectResourceAction = (resource: MediaResource) => {
            this.model!.introImage = resource;
        }
    }

    selectOutroImage() {
        this.resourceTypeToSelect = 'image';
        this.popResourceSelector();
        this.selectResourceAction = (resource: MediaResource) => {
            this.model!.outroImage = resource;

        };
    }

    selectContentImage() {
        this.resourceTypeToSelect = 'image';
        this.resourceSelectService.openModal();

        this.resourceSelectSubscribe = this.resourceSelectService.onResourceSelected.subscribe(resource => {
            console.log('Resource selected:', resource);
            if (!this.model!.contentImages) {
                this.model!.contentImages = [];
            }
            if (resource) {
                this.model!.contentImages.push(resource);
            }
        });
    }

    selectIntroAudio() {
        this.resourceTypeToSelect = 'audio';
        this.popResourceSelector();
        this.selectResourceAction = (resource: MediaResource) => {
            this.model!.introAudio = resource;
        };
    }

    selectLogoImage() {
        this.resourceTypeToSelect = 'image';
        this.popResourceSelector();
        this.selectResourceAction = (resource: MediaResource) => {
            if (this.model && this.model.logo) {
                this.model.logo.image = resource;
            }
        };
    }

    removeContentImage(resource: MediaResource) {
        if (!this.model!.contentImages) {
            return;
        }
        const index = this.model!.contentImages.indexOf(resource);
        if (index > -1) {
            this.model!.contentImages.splice(index, 1);
        }
    }

    async new() {
        this.router.navigate(['/videos', 'new']);
    }

    async save() {
        this.logger?.log('Saving video:', this.model);
        await this.videoService.updateVideo(this.model!);

        if (this.isNewVideo) {
            this.router.navigate(['videos', 'edit', this.model!.id]);
        }

        this.notificationService?.showSuccess(
            'Video Updated',
            `Video 《${this.model?.title}》is updated successfully.`,
        );
        this.memoryService.set('videoTitle', this.model?.title || '');
        this.memoryService.set('videoName', this.model?.name || '');
        this.memoryService.set('videoCategory', this.model?.category || '');
        this.memoryService.set('videoTag', this.model?.tag || '');
    }

    async export() {
        this.isExporting = true;
        let result = await this.videoService.export(this.model!);
        this.notificationService?.showSuccess(
            'Video Exported',
            `Video 《${this.model?.title}》is exported successfully.`,
        );
        this.isExporting = false;
    }

    async exportVideo() {
        this.isExporting = true;
        let result = await this.videoService.export(this.model!);
        this.notificationService?.showSuccess(
            'Video Exported',
            `Video 《${this.model?.title}》is exported successfully.`,
        );
        this.isExporting = false;
    }

    onPasteContent(event: ClipboardEvent): void {
        const pastedText = event.clipboardData?.getData('text') || '';
        console.log('Pasted:', pastedText);
        event.preventDefault();
        this.model!.content = this.cleanBibleText(pastedText);
    }

    cleanBibleText(text: string): string {
        return text
            .replace(/\d+\s?/g, '')      // Remove numbers followed by optional space
            .replace(/\[[a-z]+\]/gi, '') // Remove footnote markers like [a], [b], case-insensitive
            .replace(/["'‘’“”]/g, '')
            .replace(/（[^）]*）/g, "")
            .replace(/\([^)]*\)/g, "")
            .replace(/　神/g, '神')
            .replace(/[\s\u3000]+/g, '')
            .trim();
    }

    async openVideoFolder() {
        await this.videoService.openVideoFolder(this.model!);
    }

    validateContent(): boolean {
        if (!this.model || !this.model.content) {
            return false;
        }

        for (let line of this.model!.content.split('\n')) {
            if (line.trim()) {
                if (line.length > 15) {
                    return false;
                }
            }
        }
        return true;
    }

    closeResourceSelector() { }
}