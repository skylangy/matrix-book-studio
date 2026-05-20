import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DurationTypes, MediaElement, VideoTemplate } from 'src/app/models/video-template';
import { VideoTimelineRulerComponent } from './video.timeline.ruler.component';
import { VideoTimelineTrackComponent } from './video.timeline.track.component';

@Component({
    selector: 'mtx-video-timeline',
    templateUrl: 'video.timeline.component.html',
    imports: [CommonModule, FormsModule, RouterModule,
        VideoTimelineTrackComponent,
        VideoTimelineRulerComponent
    ]
})
export class VideoTimelineComponent implements AfterViewInit, OnInit {
    private template!: VideoTemplate;
    @Output() elementSelected = new EventEmitter<MediaElement>();

    pixelsPerSecond = 10;       // Adjusts timeline scale (10px = 1 second)
    maxDuration = 60;           // Maximum timeline duration in seconds
    maxTrackDuration = 0;


    constructor() { }

    @Input()
    get videoTemplate(): VideoTemplate {
        return this.template;
    }
    set videoTemplate(value: VideoTemplate) {
        this.template = value;
        this.initTemplate();
    }

    async ngOnInit() {
        this.initTemplate();
    }

    ngAfterViewInit() {
    }

    onElementSelected(element: MediaElement) {
        this.elementSelected.emit(element);
    }

    initTemplate() {
        console.log('Initializing video template...', this.videoTemplate);
        if (this.videoTemplate && this.videoTemplate.tracks) {
            for (let track of this.videoTemplate.tracks) {
                let trackDuration = 0;
                if (track.elements) {
                    for (let element of track.elements) {
                        if (element.durationType !== DurationTypes.full) {
                            if (element.durationType === DurationTypes.auto) {
                                trackDuration += 10;
                            } else {
                                trackDuration += element.duration;
                            }
                        }
                    }
                    this.maxTrackDuration = Math.max(this.maxTrackDuration, trackDuration);
                }
            }
        }
    }
}