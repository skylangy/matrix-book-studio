import { Component, Input, OnInit } from '@angular/core';
import { NavigationModel } from '../../models/navigation';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-sidebar-nav',
    templateUrl: './sidebar.nav.component.html',
    imports: [RouterModule]
})
export class SidebarNavComponent implements OnInit {
    @Input() navItems: NavigationModel[] = [];

    constructor() { }

    ngOnInit(): void { }
}
