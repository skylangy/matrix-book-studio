import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MediaResource } from 'src/app/models/media-resource';
import { VideoMeta } from 'src/app/models/video-meta';
import { FoldableTitleComponent } from '../foldable-title/foldable.title.component';
import { VoiceSelectorComponent } from '../voice-selector/voice.selector.component';
import { VideoResourceTextComponent } from './video.resource.text.component';

@Component({
    selector: 'mtx-video-audio-editor',
    templateUrl: 'video.audio.editor.component.html',
    imports: [CommonModule, FormsModule,
        VideoResourceTextComponent,
        VoiceSelectorComponent,
        FoldableTitleComponent
    ]
})

export class VideoAudioEditorComponent implements OnInit {
    @Input() model?: VideoMeta;
    @Output() onPopResourceSelector = new EventEmitter();
    @Output() onCloseResourceSelector = new EventEmitter();
    resourceTypeToSelect: string = 'audio';
    selectResourceAction: (resource: MediaResource) => void = () => { };

    constructor() { }

    ngOnInit() { }

    selectIntroAudio() {
        this.resourceTypeToSelect = 'audio';
        this.onPopResourceSelector.emit();
        this.selectResourceAction = (resource: MediaResource) => {
            this.model!.introAudio = resource;
            this.onCloseResourceSelector.emit();
        };
    }
}