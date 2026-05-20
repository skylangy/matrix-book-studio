import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-admin-container',
    templateUrl: 'admin.container.component.html',
    imports: [CommonModule]
})
export class AdminContainerComponent implements OnInit {
    @Input() enablePadding: boolean = true;
    constructor() { }

    ngOnInit() { }
}