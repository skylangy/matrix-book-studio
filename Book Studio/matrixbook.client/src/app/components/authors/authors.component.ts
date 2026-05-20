import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Book } from 'src/app/models/book';
import { GroupInfo } from 'src/app/models/group-info';
import { IPagedList } from 'src/app/models/pagedlist';
import { BookService } from 'src/app/services/book-service';
import { LocalSettingService } from 'src/app/services/local-setting.service';
import { LoggingService } from 'src/app/services/logging-services';
import { GroupedBooksViewComponent } from '../grouped-book-view/grouped.bookview.component';
import { GroupedBooksComponent } from '../grouped-books/grouped.books.component';
import { HeaderComponent } from '../header/header.component';

@Component({
    selector: 'mtx-authors',
    templateUrl: './authors.component.html',
    imports: [
        CommonModule,
        FormsModule,
        HeaderComponent,
        GroupedBooksViewComponent,
    ],
})
export class AuthorsComponent extends GroupedBooksComponent {

    constructor(protected override bookService: BookService,
        localSettingService: LocalSettingService,
        loggingService: LoggingService
    ) {
        super(bookService, localSettingService, loggingService);
        this.title = 'Books by Author';
        this.bannerImage = './assets/images/splashes/photo-14.jpg';
    }

    onFilterChanged(filter: string): void {
        for (let group of this.groupInfos()) {
            super.filterGroup(group, filter);
        }
    }

    override loadBooksForGroup(bookService: BookService, group: string, page: number): Promise<IPagedList<Book>> {
        return bookService.getAuthorBooks(group, page, this.pageSize, this.inProgressOnly());
    }

    override async getGroupInfos(): Promise<GroupInfo[]> {
        return await this.bookService.getGroupAuthors();
    }
}
