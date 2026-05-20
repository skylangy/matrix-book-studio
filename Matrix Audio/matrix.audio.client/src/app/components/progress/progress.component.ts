import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-progress',
    templateUrl: './progress.component.html',
})
export class ProgressComponent implements OnInit {
    @Input() progress: number = 0;
    @Input() height = 2;

    constructor() { }

    ngOnInit(): void { }
}
