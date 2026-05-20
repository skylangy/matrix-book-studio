
import { Component, OnInit } from '@angular/core';
import { ExportBasePaneComponent } from './export.base.pane.component';
import { FormsModule } from '@angular/forms';
import { ImageService } from 'src/app/services/image-service';

@Component({
    selector: 'mtx-export-text-pane',
    templateUrl: './export.text.pane.component.html',
    imports: [FormsModule],
})
export class ExportTextPaneComponent extends ExportBasePaneComponent {
    constructor(imageService: ImageService) {
        super(imageService);
        this.exportType = 'txt';
    }
}
