import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Book } from 'src/app/models/book';
import { GroupInfo } from 'src/app/models/group-info';
import { IPagedList } from 'src/app/models/pagedlist';
import { BookService } from 'src/app/services/book-service';
import { LocalSettingService } from 'src/app/services/local-setting.service';
import { LoggingService } from 'src/app/services/logging-services';
import { GroupedBooksViewComponent } from '../grouped-book-view/grouped.bookview.component';
import { GroupedBooksComponent } from '../grouped-books/grouped.books.component';
import { HeaderComponent } from '../header/header.component';
import { LoadingComponent } from '../loading/loading.component';

export interface SortInfo {
    title?: string;
    field?: string;
    direction?: string;
    isEnable?: boolean;
}

@Component({
    selector: 'mtx-status-books',
    templateUrl: './status.books.component.html',
    imports: [
        CommonModule,
        FormsModule,
        HeaderComponent,
        GroupedBooksViewComponent,
        LoadingComponent
    ],
})
export class StatusBooksComponent extends GroupedBooksComponent {
    sortBys = [
        { title: 'Date', field: 'DateUpdated', direction: 'desc', isEnable: true },
        { title: 'Author', field: 'Author', direction: 'desc', isEnable: false },
        { title: 'Title', field: 'Title', direction: 'desc', isEnable: false },
    ];
    thenBys = [
        { title: 'Date', field: 'DateUpdated', direction: 'desc', isEnable: false },
        { title: 'Author', field: 'Author', direction: 'desc', isEnable: true },
        { title: 'Title', field: 'Title', direction: 'desc', isEnable: false },
    ];

    status: string = '';
    sortBy?: SortInfo;
    thenBy?: SortInfo;
    isLoading = signal(false);
    showNoImageBooks = false;

    constructor(
        private activateRoute: ActivatedRoute,
        protected override bookService: BookService,
        localSettingService: LocalSettingService,
        loggingService: LoggingService
    ) {
        super(bookService, localSettingService, loggingService);
        this.title = 'Status';
        this.bannerImage = './assets/images/splashes/photo-10.jpg';
    }

    override async ngOnInit() {
        this.activateRoute.params.subscribe(async params => {
            this.status = params['status?'] || '';
            this.logger.log('Start getting books for status', this.status);

            this.showNoImageBooks = this.localSettingService.get(`${this.status}-showNoImageBooks`, false)!;
            this.sortBy = this.localSettingService.get(`${this.status}-sortBy`, this.sortBys[0])!;
            this.thenBy = this.localSettingService.get(`${this.status}-thenBy`, this.thenBys[1])!;
            this.filterText = this.localSettingService.get(`${this.status}-filter`, '')!;
            this.updateSortByStatus(this.sortBy);
            this.updateThenByStatus(this.thenBy);

            this.logger.log('Sort by', this.sortBy, this.sortBys, 'Then by', this.thenBy, this.thenBys);
            this.logger.log('Show no image books', this.showNoImageBooks);

            this.groupInfos.set(await this.getGroupInfos());
            for (let group of this.groupInfos()) {
                if (group.name === this.status) {
                    group.isVisible = true;
                    group.isExpanded = true;
                }
            }
            await this.loadBooks();
        });
    }

    async onFilterChanged(filter: string) {
        await this.loadBooks();
        this.localSettingService.set(`${this.status}-filter`, filter);
    }

    async toggleNoImageBooks() {
        this.showNoImageBooks = !this.showNoImageBooks;
        await this.loadBooks();
        this.localSettingService.set(`${this.status}-showNoImageBooks`, this.showNoImageBooks);
    }

    override loadBooksForGroup(bookService: BookService, group: string, page: number = 1): Promise<IPagedList<Book>> {
        return bookService.searchGroupedBooks({
            status: this.status,
            keyword: this.filterText,
            sortBy: this.sortBy?.field,
            thenBy: this.thenBy?.field,
            page: page,
            pageSize: this.pageSize,
            noImage: this.showNoImageBooks
        });
    }

    override async getGroupInfos(): Promise<GroupInfo[]> {
        return await this.bookService.getGroupStatuses();
    }

    trackByGroupId(group: any): any {
        return group.id;
    }

    async setSortBy(sortBy: SortInfo, loadBooks: boolean = true) {
        if (this.sortBy == sortBy)
            return;

        this.sortBy = sortBy;
        this.sortBy.isEnable = true;

        this.updateSortByStatus(sortBy);

        this.logger.log('Sort by', this.sortBy);
        if (loadBooks) {
            this.localSettingService.set(`${this.status}-sortBy`, sortBy);
            await this.loadBooks();
        }
    }

    private updateSortByStatus(sortBy: SortInfo) {
        for (let sort of this.sortBys) {
            if (sort.field !== sortBy.field) {
                sort.isEnable = false;
            } else {
                sort.isEnable = true;
            }
        }
    }

    async setThenBy(thenBy: SortInfo, loadBooks: boolean = true) {
        if (this.thenBy == thenBy)
            return;

        this.thenBy = thenBy;
        this.thenBy.isEnable = true;

        this.updateThenByStatus(thenBy);

        this.logger.log('Then by', this.thenBy);
        if (loadBooks) {
            this.localSettingService.set(`${this.status}-thenBy`, thenBy);
            await this.loadBooks();
        }
    }

    private updateThenByStatus(thenBy: SortInfo) {
        for (let sort of this.thenBys) {
            if (sort.field !== thenBy.field)
                sort.isEnable = false;
            else
                sort.isEnable = true;
        }
    }

    async loadBooks() {
        this.isLoading.set(true);
        let page = await this.loadBooksForGroup(this.bookService, this.status, this.currentPage);

        let group = this.groupInfos().find(g => g.name === this.status);
        if (group && page) {
            group.books = page;
            group.count = page.totalItemCount;
        }
        this.logger.log('Get grouped books', page, this.groupInfos());
        this.logger.log('Finish getting books for status', this.status);
        this.isLoading.set(false);
    }

    async onBookFinished(book: Book) {
        this.logger.log('Book finished', book.title);
        if (book) {
            this.logger.log('Remove book from group', book.title);
            for (let group of this.groupInfos()) {
                if (group.books) {
                    let index = group.books.items?.findIndex(b => b.id === book.id);
                    if (index !== undefined && index >= 0) {
                        this.logger.log('Remove book from group', group.name, book.title);
                        group.books.items = group.books.items?.splice(index, 1);
                        group.count = group.books.items?.length || 0;
                    }
                }
            }

            this.groupInfos.set(this.groupInfos());
        }
    }
}
