import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NarrationBlock, NarrationBlockType } from 'src/app/models/narration';

@Component({
    selector: 'mtx-narration-block-editor',
    templateUrl: 'narration.block.editor.component.html',
    imports: [CommonModule, FormsModule, RouterModule]
})
export class NarrationBlockEditorComponent implements OnInit {
    @Input() model!: NarrationBlock;
    @Output() selected = new EventEmitter<NarrationBlock>();
    @Output() deleted = new EventEmitter<string>();

    constructor() { }

    ngOnInit() { }

    get isEven(): boolean {
        return this.model.type !== undefined && this.model.type === NarrationBlockType.Note;
    }

    onSelected() {
        this.selected.emit(this.model);
    }

    onDeleted() {
        this.deleted.emit(this.model.id);
    }
}