import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-banner',
    templateUrl: './banner.component.html',
    imports: []
})
export class BannerComponent implements OnInit {
    @Input() imageUrl!: string;

    constructor() { }

    ngOnInit(): void { }
}
