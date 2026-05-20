import { Component, Input, OnInit } from '@angular/core';
import { LoadingStyles } from '../../models/views';

@Component({
    selector: 'mtx-loading',
    templateUrl: 'loading.component.html'
})
export class LoadingComponent implements OnInit {
    @Input() style: LoadingStyles = 'step';

    constructor() { }

    ngOnInit() { }
}