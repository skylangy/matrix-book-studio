import { Clipboard } from '@angular/cdk/clipboard';
import { CdkDrag, moveItemInArray } from '@angular/cdk/drag-drop';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import DefaultBookCardConfig, { BookCardConfig } from 'src/app/models/book-config';
import { ILogger } from 'src/app/models/logger';
import { StageStatus } from 'src/app/models/stage-status';
import { TaskStatus } from 'src/app/models/task-status';
import { BookService } from 'src/app/services/book-service';
import { FileUploadService } from 'src/app/services/file-upload-service';
import { ImageService } from 'src/app/services/image-service';
import { LoggingService } from 'src/app/services/logging-services';
import { ModalService } from 'src/app/services/modal-service';
import { NotificationService } from 'src/app/services/notification-service';
import { BackgroundImageDirective } from '../../directives/background-image.directive';
import { Book } from '../../models/book';
import { BookMiniEditorComponent } from '../book-mini-editor/book-mini-editor.component';
import { RankDisplayComponent } from '../rank/rank.component';

@Component({
    selector: 'mtx-book',
    templateUrl: './book.component.html',
    imports: [
        RouterLink,
        CdkDrag,
        BackgroundImageDirective,
        RankDisplayComponent,
        BookMiniEditorComponent,
        DecimalPipe,
        DatePipe,
    ],
})
export class BookComponent implements OnInit {

    @Input() viewMode: 'imageRight' | 'imageTop' = 'imageTop';
    @Input() authorColor = '';
    @Input() cardConfig: BookCardConfig = DefaultBookCardConfig;

    @Output() removeBookStarted = new EventEmitter<any>();
    @Output() bookLoaded = new EventEmitter<Book>();
    @Output() bookFinished = new EventEmitter<Book>();

    stageStatuses: StageStatus[] = [];
    isDragging = false;
    hasValidImage = true;
    isMiniEditorVisible = false;
    placeholder = 'assets/images/splashes/photo-1.jpg';

    private bookValue?: Book;
    private logger?: ILogger;

    constructor(
        private clipboard: Clipboard,
        private modalService: ModalService,
        private bookService: BookService,
        private imageService: ImageService,
        private notificationService: NotificationService,
        private fileUploadService: FileUploadService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('Book');
    }

    ngOnInit(): void {
        this.stageStatuses = [
            { name: 'Generate WAV', done: this.book?.wavGenerated ?? false, doneStyle: 'wav-done' },
            { name: 'Generate MP3', done: this.book?.mp3Generated ?? false, doneStyle: 'mp3-done' },
            { name: 'Generate MP4', done: this.book?.mp4Generated ?? false, doneStyle: 'mp4-done' },
            { name: 'Generate Subtitle', done: this.book?.srtGenerated ?? false, doneStyle: 'srt-done' },
        ];

        this.bookLoaded.emit(this.book);
    }

    @Input()
    get book(): Book {
        return this.bookValue!;
    }

    set book(value: Book) {
        this.bookValue = value;
    }

    get isFinished(): boolean {
        return this.book?.status === TaskStatus.Finished;
    }

    get summary(): string {
        return this.book?.summary || this.book?.content?.substring(0, 240) || '';
    }

    getBookImage(): string {
        if (this.book && this.book.defaultImageId) {
            return this.imageService.getImageUrl(this.book?.title!, this.book?.defaultImageId!) || '';
        }
        return '';
    }

    hasImage(): boolean {
        return this.getBookImage() !== '' && this.hasValidImage;
    }

    async remove() {
        this.modalService.openModal({
            title: 'Remove book',
            icon: 'exclamation-triangle',
            body: `Do you want to remove book ${this.book?.title}?`,
            buttons: [
                {
                    label: 'OK',
                    style: 'secondary',
                    action: () => {
                        this.removeBookStarted.emit(this.book);
                        return true;
                    }
                },
                {
                    label: 'Cancel',
                    style: 'primary',
                    action: () => {
                        this.logger?.log('Remove book canceled');
                        return true;
                    }
                }
            ]
        });
    }

    async finish() {
        let book = this.book;
        try {
            await this.bookService?.finish(book);
            this.bookFinished.emit(book);
            this.notificationService?.showSuccess(
                'Book finished',
                `Book 《${book?.title}》is marked as finished successfully.`,
            );
        }
        catch (error) {
            this.logger?.error('Finish book failed', error);
            this.notificationService?.showFail(
                'Finish book failed',
                `Failed to mark book 《${book?.title}》as finished.`,
            );
        }
    }

    async setRank(rank: number) {
        let book = this.book;
        try {
            await this.bookService?.updateRank(book?.id!, rank);
            this.notificationService?.showSuccess(
                'Book Updated',
                `Book 《${book?.title}》rank is updated successfully.`,
            );
        }
        catch (error) {
            this.logger?.error('Update book failed', error);
            this.notificationService?.showFail(
                'Update book failed',
                `Failed to update book 《${book?.title}》.`,
            );
        }
    }

    getAuthorStyle() {
        return {
            'border-bottom-color': this.authorColor,
            'border-bottom-width': '2px',
            'border-bottom-style': 'solid'
        }
    }

    openFolder() {
        this.bookService.openFolder(this.book?.id!);
    }

    async save() {
        this.book = await this.bookService.getBook(this.book?.id!);
        this.notificationService?.showSuccess(
            'Book Updated',
            `Book 《${this.book?.title}》is updated successfully.`,
        );
    }

    @HostListener('dragover', ['$event'])
    onDragOver(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = true;
    }

    @HostListener('dragleave', ['$event'])
    onDragLeave(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = false;
    }

    @HostListener('drop', ['$event'])
    onDrop(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = false;

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.processImage(files[0]);
        }
    }

    handleFileInput(event: Event) {
        const input = event.target as HTMLInputElement;
        const files = input.files;
        if (files && files.length > 0) {
            this.processImage(files[0]);
        }
    }

    onImageDropped(event: any) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    }

    handleImageError(event: Event) {
        this.hasValidImage = false;
    }

    copyTitle() {
        this.clipboard.copy(this.book?.title!);
    }

    copyAuthor() {
        this.clipboard.copy(this.book?.author!);
    }

    copySubtitle() {
        this.clipboard.copy(this.book?.subtitle!);
    }

    showMiniEditor() {
        this.isMiniEditorVisible = true;
    }

    hideMiniEditor() {
        this.isMiniEditorVisible = false;
    }

    async categoryUpdated(book: Book) {
        this.hideMiniEditor();
        this.book = await this.bookService.getBook(this.book?.id!);
    }

    private async processImage(file: File) {
        let bookTitle = this.book?.title!;
        await this.fileUploadService.uploadBookImage(file, bookTitle);
        this.book = await this.bookService.getBookByName(bookTitle);
    }
}
