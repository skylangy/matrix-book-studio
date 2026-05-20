import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PropertyType, PropertyTypes, TemplateProperty } from 'src/app/models/video-template';
import { ResourceSelectService } from 'src/app/services/resource-select-service';

@Component({
    selector: 'mtx-video-template-prop-editor',
    templateUrl: 'video.template.propeditor.component.html',
    imports: [CommonModule, FormsModule]
})
export class VideoTemplatePropEditorComponent implements OnInit {
    @Input() model!: TemplateProperty;
    @Output() remove = new EventEmitter<TemplateProperty>();

    subscribe: any;

    constructor(private readonly resourceSelectService: ResourceSelectService) { }

    ngOnInit() {

    }

    get propertyTypes(): string[] {
        return Object.values(PropertyTypes);
    }

    removeProperty() {
        this.remove.emit(this.model);
    }

    setType(type: string) {
        this.model.type = type as PropertyType;
    }

    selectResource() {
        this.resourceSelectService.openModal();

        this.subscribe = this.resourceSelectService.onResourceSelected.subscribe(resource => {

            if (resource !== null) {
                this.model.value = resource.url;
            }

            if (this.subscribe) {
                this.subscribe.unsubscribe();
            }
        });
    }
}