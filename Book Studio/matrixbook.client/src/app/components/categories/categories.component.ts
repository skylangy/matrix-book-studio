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
    selector: 'mtx-categories',
    templateUrl: './categories.component.html',
    imports: [HeaderComponent, CommonModule, FormsModule, GroupedBooksViewComponent]
})
export class CategoriesComponent extends GroupedBooksComponent {

    constructor(
        protected override bookService: BookService,
        localSettingService: LocalSettingService,
        loggingService: LoggingService
    ) {
        super(bookService, localSettingService, loggingService);
        this.title = 'Categories';
        this.bannerImage = './assets/images/splashes/photo-9.jpg';
    }

    onFilterChanged(filter: string): void {
        for (let group of this.groupInfos()) {
            super.filterGroup(group, filter);
        }
    }

    override loadBooksForGroup(bookService: BookService, group: string, page: number): Promise<IPagedList<Book>> {
        this.logger.log('Load books for category: ', group, page, this.pageSize);
        return bookService.getBooksByCategory(group, page, this.pageSize, this.inProgressOnly());
    }

    override async getGroupInfos(): Promise<GroupInfo[]> {
        return this.bookService.getGroupCategories();
    }
}
