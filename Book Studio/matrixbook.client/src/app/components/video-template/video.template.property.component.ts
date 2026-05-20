import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DurationTypes, MediaElement, PropertyTypes, StartTypes, TemplateProperty, VideoTemplate } from 'src/app/models/video-template';
import { IdGeneratorService } from 'src/app/services/id-generator';
import { ToolPaneComponent } from '../toolpane/toolpane.component';
import { VideoTemplatePropEditorComponent } from './video.template.propeditor.component';

@Component({
    selector: 'mtx-video-template-property',
    templateUrl: 'video.template.property.component.html',
    imports: [CommonModule, FormsModule, ToolPaneComponent,
        VideoTemplatePropEditorComponent
    ]
})

export class VideoTemplatePropertyComponent implements OnInit {
    @Input() model!: VideoTemplate
    @Input() element?: MediaElement;

    constructor(private readonly idGenerator: IdGeneratorService) { }

    ngOnInit() { }

    get durationTypes(): string[] {
        return Object.values(DurationTypes);
    }

    get startTypes(): string[] {
        return Object.values(StartTypes);
    }

    addProperty() {
        if (!this.model.properties) {
            this.model.properties = [];
        }
        this.model.properties.push({ id: this.idGenerator.generateId('prop'), name: '', value: '', type: PropertyTypes.text });
    }

    removeProperty(prop: TemplateProperty) {
        if (this.model.properties) {
            this.model.properties = this.model.properties.filter(p => p !== prop);
        }
    }
}