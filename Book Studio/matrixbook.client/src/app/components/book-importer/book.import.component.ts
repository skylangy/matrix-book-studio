import { Component, OnInit } from '@angular/core';
import { WizardItem } from '../wizard/wizard-item';
import { Router } from '@angular/router';
import { WizardItemComponent } from '../wizard/wizard.item.component';
import { WizardComponent } from '../wizard/wizard.component';
import { FileUploaderComponent } from '../file-uploader/file.uploader.component';
import { HeaderComponent } from '../header/header.component';

@Component({
    selector: 'mtx-book-importer',
    templateUrl: './book.import.component.html',

    imports: [
        HeaderComponent,
        FileUploaderComponent,
        WizardComponent,
        WizardItemComponent,
    ],
})
export class BookImporterComponent implements OnInit {

    bookInfo: WizardItem = { index: 0, title: 'Book Info', icon: 'info-circle', isActive: true, data: {} };
    bookContent: WizardItem = { index: 1, title: 'Book content', icon: 'cloud-arrow-up', isActive: false, data: {} };
    bookReview: WizardItem = { index: 2, title: 'Review', icon: 'check-circle', isActive: false, data: {} };

    constructor(private router: Router) { }

    ngOnInit(): void { }

    create() {
        this.router.navigate(['/']);
    }
}
