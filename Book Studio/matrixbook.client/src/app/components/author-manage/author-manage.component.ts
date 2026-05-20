import { CommonModule } from '@angular/common';
import { Component, OnInit, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Author } from 'src/app/models/author';
import { ILogger } from 'src/app/models/logger';
import { IPagedList } from 'src/app/models/pagedlist';
import { AuthorService } from 'src/app/services/author-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { AuthorEditorComponent } from '../author-editor/author-editor.component';
import { AuthorComponent } from '../author/author.component';
import { HeaderComponent } from '../header/header.component';
import { LoadingComponent } from '../loading/loading.component';
import { WorkViewComponent } from '../work-view/work.view.component';

@Component({
    selector: 'mtx-author-manage',
    templateUrl: './author-manage.component.html',
    imports: [
        HeaderComponent,
        FormsModule,
        CommonModule,
        AuthorComponent,
        AuthorEditorComponent,
        LoadingComponent
    ],
})
export class AuthorManageComponent extends WorkViewComponent implements OnInit {
    authors?: IPagedList<Author>;
    editAuthor: Author | null = null;
    isLoading = false;
    page = 1;
    pageSize = 24;
    private indexToNotify = -4;
    private _filter = '';
    private logger?: ILogger;

    constructor(
        private readonly router: Router,
        private readonly authorService: AuthorService,
        private readonly loggingService: LoggingService,
        private readonly notificationService: NotificationService,
        private readonly renderer: Renderer2
    ) {
        super();
        this.title = 'Authors';
        this.subtitle = 'Manage Authors';
        this.bannerImage = './assets/images/splashes/photo-8.jpg';

        this.logger = this.loggingService?.getLogger('AuthorManage');
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

    get isEditing(): boolean {
        return this.editAuthor !== null;
    }

    async onFilterChanged(filter: string) {
        if (filter) {
            this.page = 1;
            this.authors = await this.authorService.search(filter);
            this.logger?.log('Authors loaded', this.authors.items?.length, this.authors);
        } else {
            await this.load();
        }
    }
    override async ngOnInit() {
        await this.load();
    }

    async load() {
        this.isLoading = true;
        this.authors = await this.authorService.getPagedAuthors(this.page, this.pageSize);
        this.logger?.log('Authors loaded', this.authors.items?.length, this.authors);
        this.isLoading = false;
    }

    async authorRemoved(author: Author) {
        this.page = 1;
        this.authors = await this.authorService.getPagedAuthors(this.page, this.pageSize);
    }

    authorEdit(author: Author) {
        this.logger?.log('Edit author', author.name);
        this.editAuthor = author;
        this.startEditing();
    }

    addAuthor() {
        this.router.navigate(['/new-author']);
    }

    authorSaved(author: Author) {
        this.logger?.log('Author saved', author.name);
        this.notificationService.showSuccess('Author saved', `Author "${author.name}" saved`);
        this.editAuthor = null;
        this.endEditing();
    }

    authorCanceled() {
        this.editAuthor = null;
        this.endEditing();
    }

    async fixAvatars() {
        await this.authorService.fixAuthorSplashes();
        this.notificationService.showSuccess('Avatar', `Author avatars updated`);
    }

    authorLoaded(author: Author) {
        if (this.authors?.items?.at(this.indexToNotify)?.id === author.id
            && this.authors?.hasNextPage) {
            this.loadNextPage();
            this.logger?.log(`Page ${this.page} loaded`);
        }
    }

    async loadNextPage() {
        this.page++;
        let authors = await this.authorService.getPagedAuthors(this.page, this.pageSize);

        if (this.authors) {
            this.authors.hasNextPage = authors.hasNextPage;
            this.authors.pageNumber = authors.pageNumber;
            this.authors.hasNextPage = authors.hasNextPage;
            this.authors?.items?.push(...authors.items!);
        }
    }

    private startEditing() {
        this.renderer.addClass(document.body, 'modal-open');
    }

    private endEditing() {
        this.renderer.removeClass(document.body, 'modal-open');
    }
}
