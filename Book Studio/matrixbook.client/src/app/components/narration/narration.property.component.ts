import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Narration, NarrationBlock, NarrationBlockType } from 'src/app/models/narration';
import { ToolPaneComponent } from '../toolpane/toolpane.component';
import { VoiceSelectorComponent } from '../voice-selector/voice.selector.component';

@Component({
    selector: 'mtx-narration-property-pane',
    templateUrl: 'narration.property.component.html',
    imports: [CommonModule, FormsModule, ToolPaneComponent, VoiceSelectorComponent]
})
export class NarrationPropertyPaneComponent implements OnInit {
    @Input() model!: Narration;
    @Input() selectedBlock?: NarrationBlock;

    constructor() { }

    ngOnInit() { }

    get NarrationBlockTypes(): string[] {
        return NarrationBlockType.Types;
    }
}