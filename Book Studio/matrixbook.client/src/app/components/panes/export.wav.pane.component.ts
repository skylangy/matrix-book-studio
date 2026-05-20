import { Component, OnInit } from '@angular/core';
import { ExportBasePaneComponent } from './export.base.pane.component';
import { Chapter } from 'src/app/models/chapter';
import { FormsModule } from '@angular/forms';
import { ImageService } from 'src/app/services/image-service';

@Component({
    selector: 'mtx-export-wav-pane',
    templateUrl: './export.wav.pane.component.html',
    imports: [FormsModule],
})
export class ExportWavPaneComponent extends ExportBasePaneComponent {
    constructor(imageService: ImageService) {
        super(imageService);
        this.exportType = 'wav';
    }
}
