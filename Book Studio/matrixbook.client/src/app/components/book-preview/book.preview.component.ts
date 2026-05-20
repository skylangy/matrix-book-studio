import { Component, Input, OnInit } from '@angular/core';
import { ExportBook } from 'src/app/models/book';
import { EditorContext } from 'src/app/models/editor-context';
import { textToHtmlPipe } from '../../directives/text-to-html.pipe';
import { DecimalPipe } from '@angular/common';

@Component({
    selector: 'mtx-book-preview',
    templateUrl: './book.preview.component.html',

    imports: [DecimalPipe, textToHtmlPipe],
})
export class BookPreviewComponent implements OnInit {
    @Input() book?: ExportBook;
    @Input() editorContext?: EditorContext;

    constructor() { }

    ngOnInit(): void { }

    close() {
        this.editorContext!.showPreview = false;
    }
}
