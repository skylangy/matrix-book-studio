import { Component, Input, OnInit } from '@angular/core';
import { BackgroundImageDirective } from '../../directives/background-image.directive';

@Component({
    selector: 'mtx-header',
    templateUrl: './header.component.html',

    imports: [BackgroundImageDirective],
})
export class HeaderComponent implements OnInit {
    @Input() title?: string = '';
    @Input() subtitle?: string = '';
    @Input() background?: string = './assets/images/splashes/photo-4.jpg';

    constructor() { }

    ngOnInit(): void { }
}
