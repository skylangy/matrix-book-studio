import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'mtx-admin-footer',
    templateUrl: 'admin.footer.component.html',
    imports: [CommonModule]
})

export class AdminFooterComponent implements OnInit {

    date = new Date();
    constructor() { }

    ngOnInit() { }
}