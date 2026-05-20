import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MediaResource } from 'src/app/models/media-resource';

@Component({
    selector: 'mtx-video-resource-text',
    templateUrl: 'video.resource.text.component.html',
    imports: [CommonModule, FormsModule]
})
export class VideoResourceTextComponent implements OnInit {
    @Input() name: string = '';
    @Input() resource?: MediaResource;
    @Input() resourceType: string = 'image';

    @Output() onSelectResource = new EventEmitter<MediaResource>();

    constructor() { }

    ngOnInit() { }

    selectResource() {
        this.onSelectResource.emit(this.resource);
    }
}