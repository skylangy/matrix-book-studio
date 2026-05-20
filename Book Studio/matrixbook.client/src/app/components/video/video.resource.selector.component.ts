import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MediaResource, MediaResourceGroup } from 'src/app/models/media-resource';
import { LocalSettingService } from 'src/app/services/local-setting.service';
import { VideoService } from 'src/app/services/video-service';
import { FileUploaderComponent } from '../file-uploader/file.uploader.component';

@Component({
    selector: 'mtx-video-resource-selector',
    templateUrl: 'video.resource.selector.component.html',
    imports: [CommonModule, FormsModule, FileUploaderComponent],
})
export class VideoResourceSelectorComponent implements OnInit {
    @Input() resourceType: string = 'image';
    @Input() enableUpload: boolean = true;
    @Input() enableTypeSelector: boolean = false;
    @Output() closed = new EventEmitter<void>();
    @Output() resourceSelected = new EventEmitter<MediaResource>();

    resourceGroups = signal<MediaResourceGroup[]>([]);
    selectedGroup = signal<MediaResourceGroup | null>(null);
    files: File[] = [];
    resources: MediaResource[] = [];
    selectedResource: MediaResource | null = null;
    previewSelection = signal(false);
    showUpload = false;
    filterContent = '';
    mediaTypes: string[] = ['All', 'Image', 'Audio'];

    constructor(private readonly videoService: VideoService,
        private readonly localSettingService: LocalSettingService
    ) { }

    async ngOnInit() {
        await this.loadResources();
        this.onFilterChanged(this.filterContent);
    }

    get filter() {
        return this.filterContent;
    }

    set filter(value: string) {
        if (this.filterContent !== value) {
            this.filterContent = value;
            this.onFilterChanged(value);
        }
    }

    get mediaType() {
        return this.resourceType;
    }
    set mediaType(value: string) {
        if (this.resourceType !== value) {
            this.resourceType = value;
            this.onFilterChanged(this.filterContent);
        }
    }

    async loadResources() {
        this.resources = await this.videoService.getResourcesByType(this.resourceType);
        this.resources.forEach(async resource => {
            resource.isHidden = false;
            resource.isSelected = false;
            resource.fullUrl = await this.videoService.getMediaResourceUrl(resource.id);
        });
        console.log('Loading resources for type:', this.resourceType, this.resources);

        let resourceGroups = await this.videoService.getResourceGroupsByType(this.resourceType);
        console.log('Loading resource groups for type:', this.resourceType, resourceGroups);
        this.resourceGroups.set(resourceGroups);

        const lastSelectedGroup = this.localSettingService.get<string>('lastSelectedGroup');
        if (lastSelectedGroup) {
            const group = resourceGroups.find(g => g.name === lastSelectedGroup);
            if (group) {
                this.showGroup(group);
                return;
            }
        }
    }

    showGroup(group: MediaResourceGroup) {
        this.selectedGroup.set(group);
        this.resources = group.resources;
        this.resources.forEach(async resource => {
            resource.isHidden = false;
            resource.isSelected = false;
            resource.fullUrl = await this.videoService.getMediaResourceUrl(resource.id);
        });
        this.localSettingService.set('lastSelectedGroup', group.name);
    }

    onFilterChanged(filter: string): void {
        const keyword = filter.trim().toLowerCase();
        const selectedType = this.resourceType.toLowerCase();

        this.resources.forEach(resource => {
            const matchesType =
                selectedType === 'all' ||
                resource.type.toLowerCase() === selectedType;
            const matchesKeyword =
                !keyword ||
                resource.name.toLowerCase().includes(keyword);

            resource.isHidden = !(matchesType && matchesKeyword);
        });
    }

    toggleUpload() {
        this.showUpload = !this.showUpload;
    }

    async upload() { }

    onFileReady(files: File[]) {
        this.files = files;
    }

    canUpload() {
        return this.files && this.files.length > 0;
    }

    selectResource(resource: MediaResource) {
        if (this.selectedResource) {
            this.selectedResource.isSelected = false;
        }
        this.selectedResource = resource;
        this.selectedResource.isSelected = true;
    }

    closePreview() {
        this.selectedResource = null;
    }

    playAudio() {

    }

    confirm() {
        if (this.selectedResource) {
            this.resourceSelected.emit(this.selectedResource);
        }
        this.closed.emit();
    }

    cancel() {
        this.closed.emit();
    }

    async refreshResources() {
        // await this.videoService.scan();
        await this.videoService.updateImages();
        await this.loadResources();
    }
}