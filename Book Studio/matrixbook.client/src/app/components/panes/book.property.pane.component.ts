import { Clipboard } from '@angular/cdk/clipboard';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ILogger } from 'src/app/models/logger';
import { BookService } from 'src/app/services/book-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { CategoryEditorComponent } from '../category-editor/category-editor.component';
import { RankDisplayComponent } from '../rank/rank.component';
import { TagEditorComponent } from '../tag-editor/tag-editor.component';
import { VoiceSelectorComponent } from '../voice-selector/voice.selector.component';
import { PaneBaseComponent } from './pane.base.component';

@Component({
    selector: 'mtx-book-property-pane',
    templateUrl: './book.property.pane.component.html',
    imports: [
        RankDisplayComponent,
        FormsModule,
        CategoryEditorComponent,
        TagEditorComponent,
        VoiceSelectorComponent,
    ],
})
export class BookPropertyPaneComponent extends PaneBaseComponent {

    private logger?: ILogger;

    constructor(private clipboard: Clipboard, loggingService: LoggingService,
        private bookService: BookService,
        private notificationService: NotificationService
    ) {
        super();
        this.logger = loggingService.getLogger('BookPropertyPaneComponent');
    }

    get title(): string {
        return this.context?.book?.title || '';
    }

    set title(value: string) {
        if (this.book)
            this.book.title = value;
    }

    get subtitle(): string {
        return this.context?.book?.subtitle || '';
    }

    set subtitle(value: string) {
        if (this.book)
            this.book.subtitle = value;
    }

    get author(): string {
        return this.context?.book?.author || '';
    }

    set author(value: string) {
        if (this.book)
            this.book.author = value;
    }

    get category(): string | undefined {
        return this.book?.category || '';
    }

    set category(value: string | undefined) {
        if (this.book)
            this.book.category = value;
    }

    get categoryIds(): string[] | undefined {
        return this.book?.categoryIds || [];
    }

    set categoryIds(value: string[] | undefined) {
        if (this.book)
            this.book.categoryIds = value;
    }

    get tag(): string | undefined {
        return this.context?.book?.tag || '';
    }

    set tag(value: string | undefined) {
        if (this.book)
            this.book.tag = value;
    }

    get year(): string {
        return this.context?.book?.year || '';
    }

    set year(value: string) {
        if (this.book)
            this.book.year = value;
    }

    get summary(): string {
        return this.context?.book?.summary || '';
    }

    set summary(value: string) {
        if (this.book)
            this.book.summary = value;
    }

    get speechService(): string {
        return this.context?.book?.speechService || '';
    }

    set speechService(value: string) {
        if (this.book) {
            this.context!.book!.speechService = value;
            this.logger?.log('speechService', value);
        }
    }

    get language(): string {
        return this.book?.language || '';
    }

    set language(value: string) {
        if (this.book)
            this.book.language = value;
    }

    get voiceName(): string {
        return this.context?.book?.voiceName || '';
    }

    set voiceName(value: string) {
        if (this.book)
            this.book.voiceName = value;
    }

    get hide(): boolean {
        return this.context?.book?.hide || false;
    }

    set hide(value: boolean) {
        if (this.book)
            this.book.hide = value;
    }

    onCategoryIdChanged(ids: string[] | undefined) {
        this.logger?.log('onCategoryIdChanged', ids);
        if (this.book) {
            this.book.categoryIds = ids;
        }
    }

    async save() {
        let service = this.context.bookService;
        let book = this.context?.book;
        if (!book)
            return;

        // book.category = this.getSelectedValues();
        this.logger?.log('Save book', book);

        await service?.updateBook(book);

        this.context?.notificationService?.showSuccess('Book saved', `Book 《${book?.title}》 saved successfully.`);
    }

    copyBookTitle() {
        this.clipboard.copy(this.title);
    }

    copyBookAuthor() {
        this.clipboard.copy(this.author);
    }

    copyBookSubtitle() {
        this.clipboard.copy(this.subtitle);
    }

    async setRank(rank: number) {
        let book = this.book;
        if (!book)
            return;

        try {
            book!.rank = rank;
            let result = await this.bookService?.updateRank(book?.id!, rank);
            if (result) {
                this.notificationService?.showSuccess(
                    'Book Updated',
                    `Book 《${book?.title}》rank is updated successfully.`,
                );
            } else {
                this.notificationService?.showFail(
                    'Update book failed',
                    `Failed to update book rank 《${book?.title}》.`,
                );
            }
        }
        catch (error) {
            this.logger?.error('Update book failed', error);
            this.notificationService?.showFail(
                'Update book failed',
                `Failed to update book 《${book?.title}》.`,
            );
        }
    }

    onPasteTitle(event: ClipboardEvent) {
        event.preventDefault();
        const clipboardData = event.clipboardData;
        const pastedData = clipboardData?.getData('text/plain');
        if (pastedData) {
            this.title = pastedData.trim();
            console.log('Pasted data:', pastedData, this.title);
        }
    }

    onPasteAuthor(event: ClipboardEvent) {
        event.preventDefault();
        const clipboardData = event.clipboardData;
        const pastedData = clipboardData?.getData('text/plain');
        if (pastedData) {
            this.author = pastedData.trim();
        }
    }
}
