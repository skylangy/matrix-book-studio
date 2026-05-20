import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DurationTypes, MediaElement, StartTypes, VideoTrack } from 'src/app/models/video-template';

@Component({
    selector: 'mtx-video-timeline-track',
    templateUrl: 'video.timeline.track.component.html',
    imports: [
        CommonModule, FormsModule, RouterModule,
    ]
})
export class VideoTimelineTrackComponent implements OnInit {
    @Input() track!: VideoTrack;
    @Input() timelineDuration: number = 0;
    @Output() elementSelected = new EventEmitter<MediaElement>();

    pixelsPerSecond = 28;

    constructor() { }

    ngOnInit() {
        if (this.track && this.track.elements) {
            let start = 0;

            for (let element of this.track.elements) {
                if (element.startType === StartTypes.afterPrevious) {
                    element.start = start + element.start;
                } else if (element.startType === StartTypes.beforeEnd) {
                    element.start = this.timelineDuration - element.duration;
                }

                if (element.durationType === DurationTypes.auto) {
                    element.duration = 10;
                }

                start += element.duration;
            }

            for (let element of this.track.elements) {
                if (element.durationType === DurationTypes.full) {
                    element.duration = this.timelineDuration;
                }
            }
        }
    }

    selectElement(element: MediaElement) {
        this.elementSelected.emit(element);
    }

    isAutoDuration(element: MediaElement): boolean {
        return element.durationType === DurationTypes.auto;
    }

    getElementClass(element: MediaElement): { [key: string]: boolean } {
        return {
            [element.type]: true,
            'auto-duration': this.isAutoDuration(element)
        };
    }
}