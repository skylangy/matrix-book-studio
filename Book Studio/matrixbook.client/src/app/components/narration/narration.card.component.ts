import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Narration } from 'src/app/models/narration';

@Component({
    selector: 'mtx-narration-card',
    templateUrl: 'narration.card.component.html',
    imports: [CommonModule, FormsModule, RouterModule],
})

export class NarrationCardComponent implements OnInit {
    @Input() model!: Narration;
    @Output() deleted = new EventEmitter<string>();

    constructor() { }

    ngOnInit() { }

    get hasImage(): boolean {
        return false;
    }

    get summary(): string {
        if (this.model.blocks.length > 0) {
            return this.model.blocks[0]?.content?.substring(0, 200) + '...';
        }
        return '';
    }

    onImageDropped(event: any) {
    }

    handleImageError(event: Event) {
    }

    deleteModel(id: string) { }
}