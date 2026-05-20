import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-alert',
    templateUrl: './alert.component.html',
})
export class AlertComponent implements OnInit {
    @Input() header: string = '';
    @Input() body: string = '';
    @Input() type: string | undefined = 'success';
    @Input() showClose = false;

    constructor() { }

    ngOnInit(): void { }

    buildClass(): string {
        return `alert alert-${this.type}`;
    }
}
