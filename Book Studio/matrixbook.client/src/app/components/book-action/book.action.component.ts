import { Component, Input, OnInit } from '@angular/core';

import { IBookAction } from 'src/app/models/book-action';
import { RouterLink } from '@angular/router';


@Component({
    selector: 'mtx-book-action',
    templateUrl: './book.action.component.html',

    imports: [RouterLink]
})
export class BookActionComponent implements OnInit {
    @Input() action?: IBookAction;

    constructor() { }

    ngOnInit(): void { }

    execute() {
        if (this.action?.func) {
            this.action.func();
        }
    }
}
