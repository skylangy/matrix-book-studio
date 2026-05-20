import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-signin-prompt',
    templateUrl: './signin-prompt.component.html',
    imports: []
})
export class SigninPromptComponent implements OnInit {
    @Input() isMiniMode: boolean = false;
    constructor() { }

    ngOnInit(): void { }
}
