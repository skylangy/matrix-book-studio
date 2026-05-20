import { Component, Input, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-tile-card',
    templateUrl: './tile-card.component.html',
    imports: [RouterModule]
})
export class TileCardComponent implements OnInit {
    @Input() title = '';
    @Input() image = '';
    @Input() route = '';

    constructor() { }

    ngOnInit(): void { }
}
