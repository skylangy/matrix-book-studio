import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-split-layout',
    templateUrl: './split-layout.component.html',

})
export class SplitLayoutComponent implements OnInit {
    @Input() leftWidth = 'col-12 col-md-4 ';
    @Input() rightWidth = 'col-12 col-md-8';
    @Input() extra = '';
    @Input() leftVisible = ' d-md-block';
    @Input() leftExtra = '';
    @Input() rightExtra = '';

    @Input() showBorder = true;
    @Input() showShadow = true;

    constructor() { }

    ngOnInit(): void { }

    buildClass(): string {
        return `split-layout container  ${this.extra} ${this.showBorder ? 'border' : ''} `;
    }

    buildLeft(): string {
        return `${this.leftWidth} ${this.leftVisible} ${this.leftExtra}`;
    }

    buildRight(): string {
        return `${this.rightWidth} ${this.rightExtra}`;
    }
}
