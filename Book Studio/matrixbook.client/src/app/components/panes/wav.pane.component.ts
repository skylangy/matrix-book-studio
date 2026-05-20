import { Component, OnInit } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { IFile } from 'src/app/models/file';
import { FileSizePipe } from '../../directives/file-size.pipe';

@Component({
    selector: 'mtx-wav-pane',
    templateUrl: './wav.pane.component.html',

    imports: [FileSizePipe],
})
export class WavPaneComponent extends PaneBaseComponent {
    files: IFile[] = [];

    constructor() { super(); }

    override async ngOnInit() {
        super.ngOnInit();
        await this.refresh();
    }

    async refresh() {
        if (this.book) {
            this.files = await this.getBookFiles('wav', '*.wav');
        }
    }
}
