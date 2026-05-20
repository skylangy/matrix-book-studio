import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-info-card',
    templateUrl: 'info.card.component.html',
    imports: [RouterModule, CommonModule]
})
export class InfoCardComponent implements OnInit {
    @Input() title: string = '';
    @Input() info: string = '';
    @Input() icon: string = '';
    @Input() color: string = '';
    @Input() moreText = 'More...';
    @Input() moreLink = '';

    constructor() { }

    ngOnInit() { }
}