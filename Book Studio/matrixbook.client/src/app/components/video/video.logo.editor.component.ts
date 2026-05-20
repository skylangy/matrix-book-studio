import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Font, fonts } from 'src/app/models/font';
import { MediaResource } from 'src/app/models/media-resource';
import { VideoMeta } from 'src/app/models/video-meta';
import { FoldableTitleComponent } from '../foldable-title/foldable.title.component';
import { VideoResourceTextComponent } from './video.resource.text.component';

@Component({
    selector: 'mtx-video-logo-editor',
    templateUrl: 'video.logo.editor.component.html',
    imports: [CommonModule, FormsModule,
        VideoResourceTextComponent,
        FoldableTitleComponent
    ]
})
export class VideoLogoEditorComponent implements OnInit {
    @Input() model?: VideoMeta;
    @Output() onPopResourceSelector = new EventEmitter();
    @Output() onCloseResourceSelector = new EventEmitter();
    resourceTypeToSelect: string = 'audio';
    selectResourceAction: (resource: MediaResource) => void = () => { };
    supportedFonts: Font[] = fonts;

    constructor() { }

    ngOnInit() {
        if (this.model && !this.model?.logo) {
            this.model.logo = {
                fontSize: 36,
                fontFamily: '方正启体繁体',
                text: '恩典笔记',
                shadow: true
            };
        }
    }

    selectLogoImage() {
        this.resourceTypeToSelect = 'image';
        this.onPopResourceSelector.emit();
        this.selectResourceAction = (resource: MediaResource) => {
            if (this.model && this.model.logo) {
                this.model.logo.image = resource;
            }
            this.onCloseResourceSelector.emit();
        };
    }
}