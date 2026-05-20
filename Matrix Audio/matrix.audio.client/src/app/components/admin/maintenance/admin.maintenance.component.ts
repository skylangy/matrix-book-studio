import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';

@Component({
    selector: 'admin-maintenance',
    templateUrl: 'admin.maintenance.component.html',
    imports: [HeaderContentComponent]
})

export class AdminMaintenanceComponent implements OnInit {
    icon = 'wrench';
    constructor() { }

    ngOnInit() { }
}