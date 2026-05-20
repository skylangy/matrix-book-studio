import { Component, EventEmitter, Input, OnInit, output, Output } from '@angular/core';
import { Book } from 'src/app/models/book';
import { CategoryEditorComponent } from '../category-editor/category-editor.component';
import { TagEditorComponent } from '../tag-editor/tag-editor.component';
import { BookService } from 'src/app/services/book-service';
import { NotificationService } from 'src/app/services/notification-service';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from 'src/app/services/logging-services';

@Component({
    selector: 'mtx-book-mini-editor',
    templateUrl: './book-mini-editor.component.html',

    imports: [CategoryEditorComponent, TagEditorComponent,]
})
export class BookMiniEditorComponent implements OnInit {
    @Output() onSave = new EventEmitter<Book>();
    @Output() onCancel = new EventEmitter<void>();

    private categoryValue = '';
    private tagValue = '';
    private bookValue?: Book;
    private logger?: ILogger;

    constructor(private readonly bookService: BookService,
        private readonly notificationService: NotificationService,
        private readonly loggingService: LoggingService
    ) {
        this.logger = this.loggingService?.getLogger('BookMiniEditor');
    }

    ngOnInit(): void { }

    @Input()
    get book(): Book {
        return this.bookValue!;
    }

    set book(value: Book) {
        this.bookValue = value;
        this.categoryValue = value?.category || '';
        this.tagValue = value?.tag || '';
    }

    get category(): string | undefined {
        return this.categoryValue;;
    }

    set category(value: string | undefined) {
        this.categoryValue = value || '';
    }

    get tag(): string | undefined {
        return this.tagValue;
    }

    set tag(value: string | undefined) {
        this.tagValue = value || '';
    }

    async save(): Promise<void> {
        this.logger?.log('Save book', this.book?.title, this.category, this.tag);
        await this.bookService.updateCategoryTag(this.book?.id!, this.category || '', this.tag || '');

        this.notificationService?.showSuccess(
            'Book Updated',
            `Categories and tags of 《${this.book?.title}》 are updated.`,
        );

        this.onSave.emit(this.book!);
    }

    cancel(): void {
        this.onCancel.emit();
    }
}
