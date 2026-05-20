import { Injectable } from '@angular/core';
import { BookExportModel } from '../models/book-export-model';
import { IExportService } from '../models/export-service';
import { ExportTypes } from '../models/export-types';
import { ILogger } from '../models/logger';
import { TaskStatus } from '../models/task-status';
import { BookService } from './book-service';
import { ChapterService } from './chapter-service';
import { LoggingService } from './logging-services';
import { NotificationService } from './notification-service';

@Injectable({
    providedIn: 'root'
})
export class ExportService implements IExportService {

    private logger?: ILogger;

    constructor(
        private bookService: BookService,
        private chapterService: ChapterService,
        private notificationService: NotificationService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('ExportService');
    }

    async exportChapters(exportModel: BookExportModel): Promise<void> {
        this.logger?.log('Exporting chapters ', exportModel);
        await this.bookService.exportBook(exportModel);
        this.notificationService?.showSuccess(`Export ${exportModel.type}`, `Start exporting selected chapters to ${exportModel.type} files.`);
        this.logger?.log('Exporting chapters ', exportModel);
    }

    async exportBookContent(id: string): Promise<void> {
        let book = await this.bookService.getBook(id);
        if (!book || !book.content) {
            this.logger?.log(`Book ${book.title} has no content.`);
            return;
        }
        let chapters = this.chapterService?.splitToChapters(book.content!, book?.title);
        this.logger?.log(`Exporting ${book.title} with ${chapters.length} chapters.`);

        await this.exportChapters({
            bookName: book.title!,
            author: book.author!,
            type: ExportTypes.text,
            chapters: chapters
        });

        this.notificationService.showSuccess(`Export ${book.title}`, `Book "${book.title}" exported successfully.`);
    }

    async exportBookSubtitle(id: string): Promise<void> {
        let book = await this.bookService.getBook(id);
        if (!book || !book.content) {
            this.logger?.log(`Book ${book.title} has no content.`);
            return;
        }
        let chapters = this.chapterService?.splitToChapters(book.content!, book?.title);
        this.logger?.log(`Exporting ${book.title} with ${chapters.length} chapters.`);

        await this.exportChapters({
            bookName: book.title!,
            author: book.author!,
            type: ExportTypes.subtitle,
            chapters: chapters
        });

        this.notificationService.showSuccess(`Export ${book.title}`, `Book "${book.title}" exported successfully.`);
    }

    async exportFinishedBookContent(): Promise<void> {
        let books = await this.bookService.getBooks();
        let finishedBooks = books.filter(b => b.status === TaskStatus.Finished);

        for (let finishedBook of finishedBooks) {
            await this.exportBookContent(finishedBook.id!);
        }
    }

    async generateMetadataForFinishedBook(): Promise<void> {
        var books = await this.bookService.getFinishedBooks();;
        for (let book of books) {
            await this.bookService.generateMeta(book.id!);
        }
    }

    async combineVideos(id: string): Promise<void> {
        let book = await this.bookService.getBook(id);
        if (!book || !book.content) {
            this.logger?.log(`Book ${book.title} has no content.`);
            return;
        }
        let chapters = this.chapterService?.splitToChapters(book.content!, book?.title);
        this.logger?.log(`Combine videos ${book.title} with ${chapters.length} chapters.`);

        await this.exportChapters({
            bookName: book.title!,
            author: book.author!,
            type: ExportTypes.video,
            chapters: chapters
        });

        this.notificationService.showSuccess(`Combine videos ${book.title}`, `Book "${book.title}" videos combined successfully.`);
    }

    async combineFinishedBooksVideos(): Promise<void> {
        let books = await this.bookService.getBooks();
        let finishedBooks = books.filter(b => b.status === TaskStatus.Finished);

        for (let finishedBook of finishedBooks) {
            await this.combineVideos(finishedBook.id!);
        }
    }
}