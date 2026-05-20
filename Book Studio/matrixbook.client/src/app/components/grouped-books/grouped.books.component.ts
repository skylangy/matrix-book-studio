import { Component, OnInit, signal } from '@angular/core';
import { GroupedBooks } from 'src/app/models/groupedBooks';
import { BookService } from 'src/app/services/book-service';
import { WorkViewComponent } from '../work-view/work.view.component';
import { LoggingService } from 'src/app/services/logging-services';
import { ILogger } from 'src/app/models/logger';
import { IPagedList } from 'src/app/models/pagedlist';
import { Book } from 'src/app/models/book';
import { GroupInfo } from 'src/app/models/group-info';
import { LocalSettingService } from 'src/app/services/local-setting.service';

@Component({
    selector: 'mtx-grouped-books-base',
    template: '',
})
export abstract class GroupedBooksComponent extends WorkViewComponent implements OnInit {
    groupInfos = signal<GroupInfo[]>([]);
    filterText = '';
    pageSize = 12;
    currentPage = 1;
    inProgressOnly = signal(false);

    protected logger: ILogger

    constructor(
        protected bookService: BookService,
        protected localSettingService: LocalSettingService,
        loggingService: LoggingService,
    ) {
        super();
        this.logger = loggingService.getLogger('GroupedBooksComponent');
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.inProgressOnly.set(this.localSettingService.get(`${this.constructor.name}_inProgressOnly`, false)!);
        await this.load();
    }

    get filter() {
        return this.filterText;
    }

    set filter(value: string) {
        if (this.filterText !== value) {
            this.filterText = value;
            this.onFilterChanged(value);
        }
    }

    async load() {
        this.logger.log('Start getting groups');

        let groups = await this.getGroupInfos();

        this.logger.log('Initialize groups', groups);

        for (let group of groups) {
            group.isVisible = true;

            group.books = {
                items: [],
                hasNextPage: false,
                pageIndex: 0,
                pageNumber: 0,
            };
        }
        for (let group of groups.slice(0, 4)) {
            group.isExpanded = false;
        }

        this.groupInfos.set(groups);
    }

    filterGroup(group: GroupedBooks, filter: string) {
        let hasBooks = false;
        for (let book of group.books?.items!) {
            book.isVisible = book.title?.includes(filter)
                || book.author?.includes(filter)
                || book.category?.includes(filter)
                || book.subtitle?.includes(filter)
                || book.status?.includes(filter)
                || book.summary?.includes(filter);
            hasBooks = hasBooks || book.isVisible!;
        }
        group.isVisible = group.title?.includes(filter) || hasBooks;
    }

    async loadNextPage(arg: { group: string, page: number }) {
        let nextPage = arg.page + 1;
        this.logger.log(`Load next page [${arg.page}] for ${arg.group}`);

        let page = await this.loadBooksForGroup(this.bookService, arg.group, nextPage);

        let group = this.groupInfos().find(g => g.name === arg.group);

        this.logger.log(`Next page for ${arg.group}: `, page, group);

        if (group && page) {
            this.logger.log(`Append books to group ${arg.group}`);
            group.isVisible = true;
            group.isExpanded = true;
            if (group.books) {
                group.books.pageIndex = page.pageIndex;
                group.books.pageNumber = page.pageNumber;
                group.books.hasNextPage = page.hasNextPage;
            }

            for (let item of page.items!) {
                item.isVisible = true;
                group.books?.items?.push(item);
            }
        }
    }

    async initBookGrouop(group: GroupInfo) {
        group.isVisible = true;
        group.isExpanded = true;
        let books = await this.loadBooksForGroup(this.bookService, group.name!, 1);
        group.books = books;
    }

    collapseAll() {
        for (let group of this.groupInfos()) {
            group.isExpanded = false;
        }
    }

    expandAll() {
        for (let group of this.groupInfos()) {
            group.isExpanded = true;
        }
    }

    toggleInProgressOnly() {
        this.inProgressOnly.set(!this.inProgressOnly());
        this.localSettingService.set(`${this.constructor.name}_inProgressOnly`, this.inProgressOnly());
        this.load();
    }

    abstract getGroupInfos(): Promise<GroupInfo[]>;

    abstract onFilterChanged(filter: string): void;

    abstract loadBooksForGroup(bookService: BookService, group: string, page: number): Promise<IPagedList<Book>>;
}
