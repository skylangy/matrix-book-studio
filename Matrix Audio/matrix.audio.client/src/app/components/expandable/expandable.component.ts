import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-expandable',
    templateUrl: 'expandable.component.html',
    imports: [CommonModule]
})

export class ExpandableComponent implements OnInit {
    @Input() title: string = '';
    @Input() subtitle: string = '';
    @Input() expanded: boolean = false;
    @Input() isEnable: boolean = true;
    id = '';

    constructor() { }

    ngOnInit() {
        this.id = 'collapse-' + Math.random().toString(36).substring(2, 15);
    }
}