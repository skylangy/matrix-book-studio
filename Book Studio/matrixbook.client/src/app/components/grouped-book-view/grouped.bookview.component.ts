import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { Book } from '../../models/book';
import { SortOrder } from 'src/app/models/book-sort';
import { PagedBooksComponent } from '../books/pagedbooks.component';
import { CommonModule } from '@angular/common';
import { GroupInfo } from 'src/app/models/group-info';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from 'src/app/services/logging-services';

@Component({
    selector: 'mtx-grouped-books',
    templateUrl: './grouped.bookview.component.html',
    imports: [CommonModule, PagedBooksComponent],
})
export class GroupedBooksViewComponent implements OnInit {
    @Input() group: GroupInfo | undefined;
    @Input() sortBy: keyof Book = 'dateUpdated';
    @Input() thenBy: keyof Book = 'author';
    @Input() sortOrder: SortOrder = 'desc';
    @Output() readyToLoadNextPage = new EventEmitter<any>();
    @Output() bookFinished = new EventEmitter<Book>();

    private logger: ILogger

    constructor(loggingService: LoggingService) {
        this.logger = loggingService.getLogger('GroupedBooksViewComponent');
    }

    ngOnInit(): void { }

    loadNexPage(page: number): void {
        if (this.group) {
            if (this.group.books?.hasNextPage) {
                this.readyToLoadNextPage.emit({ page: this.group.books.pageNumber, group: this.group.name });
            }
        }
    }

    onBookFinished(book: Book) {
        this.bookFinished.emit(book);
    }

    toggleState() {
        if (!this.group) {
            return;
        }

        this.group.isExpanded = !this.group.isExpanded;
        this.logger.log(`Toggle group: ${this.group?.name} to ${this.group?.isExpanded ? 'Expanded' : 'Collapsed'}`);
        this.logger.log('Group: ', this.group)
        if (this.group.isExpanded && this.group && this.group.books?.items?.length == 0) {
            this.logger.log(`Load books for group: ${this.group?.name}`);
            this.readyToLoadNextPage.emit({ page: 0, group: this.group.name });
        }
    }
}
