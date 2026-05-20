import { Component, Input, OnInit } from '@angular/core';
import { GroupedNavItem, NavItem } from 'src/app/models/nav-item';
import { ToolbarComponent } from './toolbar.component';
import { NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'mtx-ribbon',
    templateUrl: './ribbon.component.html',

    imports: [RouterLink, NgClass, ToolbarComponent]
})
export class RibbonComponent implements OnInit {
    @Input() mainNavItems?: GroupedNavItem[] = [];
    @Input() fileNavItems?: NavItem[] = [];

    constructor() { }

    ngOnInit(): void { }
}
