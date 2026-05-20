import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
    selector: 'mtx-admin-pager',
    templateUrl: 'pager.component.html'
})

export class AdminPagerComponent implements OnInit {
    @Input() canGoPrev = false;
    @Output() next = new EventEmitter();
    @Output() prev = new EventEmitter();

    constructor() { }

    ngOnInit() { }

    prevPage() {
        this.prev.emit();
    }

    nextPage() {
        this.next.emit();
    }
}