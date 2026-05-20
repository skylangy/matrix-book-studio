import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-img-pane',
    templateUrl: './img-pane.component.html',

})
export class ImagePaneComponent implements OnInit {
    @Input() imgUrl = '';

    constructor() { }

    ngOnInit(): void { }
}
