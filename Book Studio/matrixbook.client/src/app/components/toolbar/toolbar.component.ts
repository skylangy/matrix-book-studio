import { Component, Input, OnInit } from '@angular/core';
import { NavItem } from 'src/app/models/nav-item';
import { NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'mtx-toolbar',
    templateUrl: './toolbar.component.html',

    imports: [RouterLink, NgClass],
})
export class ToolbarComponent implements OnInit {
    @Input() navItems?: NavItem[] = [];
    @Input() name? = 'toolbar';

    constructor() { }

    ngOnInit(): void { }

    navClicked(item: NavItem) {
        if (item.action) {
            item.action();
        }
    }
}
