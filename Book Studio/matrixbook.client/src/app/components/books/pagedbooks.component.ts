
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
import { IPagedList } from 'src/app/models/pagedlist';

@Component({
    selector: 'mtx-paged-books',
    templateUrl: './pagedbooks.component.html',
    imports: [
        FormsModule,
        BookComponent,
        BookFilterPipe,
    ],
})
export class PagedBooksComponent implements OnInit {
    @Input() title?: string;
    @Input() showFilter = false;
    @Input() showHidden = false;
    @Input() sortBy?: keyof Book;
    @Input() thenSortBy?: keyof Book;
    @Input() sortOrder: SortOrder = 'asc';
    @Input() thenOrder: SortOrder = 'asc';
    @Input() groupBy?: string = '';
    @Input() indexToNotify = -4;
    @Output() scrollToEnd = new EventEmitter<number>();
    @Output() needRefresh = new EventEmitter();
    @Output() bookFinished = new EventEmitter<Book>();

    private logger?: ILogger;
    private _books?: IPagedList<Book>;
    private _filter = '';

    authorColors = new Map();

    constructor(
        private bookService: BookService,
        private notificationService: NotificationService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('Books');
    }

    ngOnInit(): void { }

    get hasBooks(): boolean {
        return this._books !== undefined
            && this._books.items !== undefined
            && this._books.items.length > 0;
    }

    @Input()
    get books(): IPagedList<Book> | undefined {
        return this._books;
    }

    set books(value: IPagedList<Book> | undefined) {
        if (value) {
            if (this.groupBy) {
                this._books = value;
            } else {
                this._books = value;
            }

            this.onFilterChanged(this.filter);
            if (this.sortBy) {
                this.bookService.sortBy(this._books.items!, this.sortBy, this.thenSortBy, this.sortOrder, this.thenOrder);
            }
        }
    }

    get filter() {
        return this._filter;
    }

    set filter(value: string) {
        if (this._filter !== value) {
            this._filter = value;
            this.onFilterChanged(value);
        }
    }

    onFilterChanged(filter: string): void {
        for (let book of this.books?.items!) {
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
                this._books!.items = this._books?.items?.filter(b => b.id !== book.id);
            }
        }
    }

    async onBookFinished(book: Book) {
        // this.bookFinished.emit(book);
        this._books!.items = this._books?.items?.filter(b => b.id !== book.id);
    }

    toggleHidden() {
        this.showHidden = !this.showHidden;
        for (let book of this.books?.items!) {
            book.isVisible = !book.hide || this.showHidden;
        }
        if (this.sortBy) {
            this.bookService.sortBy(this._books?.items!, this.sortBy, this.thenSortBy, this.sortOrder, this.thenOrder);
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

    onBookLoaded(book: Book) {
        if (this.books?.items?.at(this.indexToNotify)?.id == book.id) {
            this.scrollToEnd.emit(this.books?.pageNumber);
        }
    }
}
