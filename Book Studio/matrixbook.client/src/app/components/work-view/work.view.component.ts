import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-work-view',
    template: '',

})
export abstract class WorkViewComponent implements OnInit {
    bannerImage?: string = '';
    title?: string = '';
    subtitle?: string = '';

    ngOnInit() {

    }
}