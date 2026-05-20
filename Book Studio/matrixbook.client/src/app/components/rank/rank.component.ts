import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'mtx-rank',
    templateUrl: './rank.component.html',

})
export class RankDisplayComponent {
    @Input() rank?: number = 3;
    @Output() rankChange = new EventEmitter<number>();

    setRank(newRank: number): void {
        this.rank = newRank;
        this.rankChange.emit(newRank);
    }
}