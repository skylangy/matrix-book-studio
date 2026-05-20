import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Book } from '../../models/book';
import { LoggingService } from 'src/app/services/logging-services';
import { ILogger } from 'src/app/models/logger';
import { BookService } from 'src/app/services/book-service';
import { NotificationService } from 'src/app/services/notification-service';
import { SortOrder } from 'src/app/models/book-sort';
import { BookFilterPipe } from '../../directives/book-filter.pipe';
import { BookComponent } from '../book/book.component';
import { FormsModule } from '@angular/forms';
import DefaultBookCardConfig, { BookCardConfig } from 'src/app/models/book-config';

@Component({
    selector: 'mtx-books',
    templateUrl: './books.component.html',
    imports: [
        FormsModule,
        BookComponent,
        BookFilterPipe,
    ],
})
export class BooksComponent implements OnInit {
    @Input() title?: string;
    @Input() showFilter = false;
    @Input() showHidden = false;
    @Input() cardConfig: BookCardConfig = DefaultBookCardConfig;
    @Input() sortBy?: keyof Book;
    @Input() thenSortBy?: keyof Book;
    @Input() sortOrder: SortOrder = 'asc';
    @Input() thenOrder: SortOrder = 'asc';
    @Input() groupBy?: string = '';
    @Output() scrollToEnd = new EventEmitter<any>();
    @Output() filterChange = new EventEmitter<string>();

    private logger?: ILogger;
    private _books?: Book[]; // = signal<Book[]>([]); //:
    private _filter = '';


    authorColors = new Map();

    constructor(
        private bookService: BookService,
        private notificationService: NotificationService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('Books');
    }

    ngOnInit(): void {

    }

    get hasBooks(): boolean {
        return this._books !== undefined && this._books.length > 0;
    }

    @Input()
    get books(): Book[] | undefined {
        return this._books;
    }

    set books(value: Book[] | undefined) {
        if (value) {
            if (this.groupBy) {
                this._books = value;
            } else {
                this._books = value;
            }

            this.onFilterChanged(this.filter);
            if (this.sortBy) {
                this.bookService.sortBy(this._books, this.sortBy, this.thenSortBy, this.sortOrder, this.thenOrder);
            }
        }
    }

    @Input()
    get filter() {
        return this._filter;
    }

    set filter(value: string) {
        if (this._filter !== value) {
            this._filter = value;
            this.filterChange.emit(value);
            // this.onFilterChanged(value);
        }
    }

    onFilterChanged(filter: string): void {
        for (let book of this.books!) {
            book.isVisible = (filter === ''
                || filter === undefined
                || book.title?.includes(filter)
                || book.author?.includes(filter))
                && (!book.hide || this.showHidden);
        }
    }

    async onRemoveBook(book: Book) {
        if (book) {
            let result = await this.bookService.removeBook(book.id!);

            this.logger?.log(`Remove book ${result ? 'succeeded' : 'failed'}.`);
            this.notificationService.showNotification({
                title: 'Book removed',
                content: `Book 《${book.title}》 removed successfully.`,
                type: result ? 'success' : 'warning',
                time: new Date().getTime() - 1000
            });

            if (result) {
                this._books = this._books?.filter(b => b.id !== book.id);
            }
        }
    }

    toggleHidden() {
        this.showHidden = !this.showHidden;
        for (let book of this.books!) {
            book.isVisible = !book.hide || this.showHidden;
        }
        if (this.sortBy) {
            this.bookService.sortBy(this._books!, this.sortBy, this.thenSortBy, this.sortOrder, this.thenOrder);
        }
    }

    getAuthorColor(author: string): string {
        if (!this.authorColors.has(author)) {
            this.authorColors.set(author, this.getRandomColor());
        }
        return this.authorColors.get(author);
    }

    getRandomColor(): string {
        return '#' + Math.floor(Math.random() * 16777215).toString(16);
    }

    trackBy(book: Book) {
        return book.id!;
    }

    bookLoaded(book: Book) {
        if (this.books?.at(-4)?.id == book.id) {
            this.scrollToEnd.emit();
        }
    }
}
