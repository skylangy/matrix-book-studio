import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IdGeneratorService } from 'src/app/services/id-generator';

@Component({
    selector: 'mtx-foldable-title',
    templateUrl: 'foldable.title.component.html',
    imports: [CommonModule, FormsModule]
})
export class FoldableTitleComponent implements OnInit {
    @Input() title: string = '';
    @Input() subtitle: string = '';
    @Input() isFolded: boolean = false;
    @Input() enableFold: boolean = true;

    id = this.idGeneratorService.generateId('foldable-title-');

    constructor(private readonly idGeneratorService: IdGeneratorService) { }

    ngOnInit() { }

    toggleFold() {
        if (this.enableFold) {
            this.isFolded = !this.isFolded;
        }
    }
}