import { Injectable } from '@angular/core';
import { Book, BookInfo } from '../models/book';
import { BookExportModel } from '../models/book-export-model';
import { BookSearchModel } from '../models/book-search-model';
import { IBookService } from '../models/book-service';
import { BookSorter, SortOrder } from '../models/book-sort';
import { DashboardModel } from '../models/dashboard-model';
import { IFile } from '../models/file';
import { GroupInfo } from '../models/group-info';
import { GroupedBooks } from '../models/groupedBooks';
import { ILogger } from '../models/logger';
import { IPagedList } from '../models/pagedlist';
import { SpeechConfig } from '../models/speech-config';
import { WorkProgress } from '../models/work-progress';
import { ApiService } from './api-service';
import { LoggingService } from './logging-services';

@Injectable({
    providedIn: 'root'
})
export class BookService implements IBookService {
    private logger?: ILogger;

    constructor(private apiService: ApiService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('BookService');
    }

    getBooks(): Promise<Book[]> {
        return this.apiService.get('book/all');
    }

    getFinishedBooks(): Promise<BookInfo[]> {
        return this.apiService.get('book/finished');
    }

    getRecentBooks(count: number = 24, filter?: string): Promise<Book[]> {
        return this.apiService.get(`book/recent/${count}/${filter}`);
    }

    getBook(id: string): Promise<Book> {
        return this.apiService.get(`book/${id}`);
    }

    getBookByName(name: string): Promise<Book> {
        return this.apiService.get(`book/name/${name}`);
    }

    getGroupCategories(): Promise<GroupInfo[]> {
        return this.apiService.get(`book/group/categories`);
    }

    getGroupTags(): Promise<GroupInfo[]> {
        return this.apiService.get(`book/group/tags`);
    }

    getGroupAuthors(): Promise<GroupInfo[]> {
        return this.apiService.get(`book/group/authors`);
    }

    getGroupStatuses(): Promise<GroupInfo[]> {
        return this.apiService.get(`book/group/status`);
    }

    getBooksByCategory(category: string, page: number = 1, pageSize: number = 12, inProgressOnly = false): Promise<IPagedList<Book>> {
        return this.apiService.get(`book/byCategory/${category}/${page}/${pageSize}/${inProgressOnly}`);
    }

    getTagedBooks(tag: string, page: number = 1, pageSize: number = 12, inProgressOnly = false): Promise<IPagedList<Book>> {
        return this.apiService.get(`book/byTag/${tag}/${page}/${pageSize}/${inProgressOnly}`);
    }

    getAuthorBooks(author: string, page: number = 1, pageSize: number = 12, inProgressOnly = false): Promise<IPagedList<Book>> {
        return this.apiService.get(`book/byAuthor/${author}/${page}/${pageSize}/${inProgressOnly}`);
    }

    getStatusBooks(status?: string, keyword?: string, sortBy?: string, thenBy?: string, page: number = 1, pageSize: number = 12, noImage = false): Promise<GroupedBooks[]> {
        let path = `book/byStatus/${status || 'all'}/${sortBy}/${thenBy}/${page}/${pageSize}/${keyword}`;
        this.logger?.log('Get status books', path);
        return this.apiService.get(path);
    }

    getPublishQueue(): Promise<Book[]> {
        return this.apiService.get('book/publishQueue');
    }

    getWorkItems(): Promise<WorkProgress[]> {
        return this.apiService.get('book/workItems');
    }

    getCategories(): Promise<string[]> {
        return this.apiService.get('book/categories');
    }

    getTags(): Promise<string[]> {
        return this.apiService.get('book/tags');
    }

    getSpeechConfig(): Promise<SpeechConfig> {
        return this.apiService.get('book/speech/config');
    }

    searchGroupedBooks(criteria: BookSearchModel): Promise<IPagedList<Book>> {
        return this.apiService.post('book/search', criteria);
    }

    create(book?: Book): Promise<Book> {
        if (!book) {
            throw new Error('Book is undefined');
        }
        if (!book.id) {
            book.id = '';
        }

        return this.apiService.post(`book`, book);
    }

    updateBook(book?: Book): Promise<Book> {
        this.logger?.log('Start Update book:', book?.title);

        this.checkImages(book);

        this.logger?.log('Update book', book);
        if (book?.id === undefined) {
            this.logger?.log('Go to Create book', book?.title);
            return this.create(book);
        }

        if (!book) {
            throw new Error('Book is undefined');
        }

        return this.apiService.put(`book/update`, book);
    }

    updateBookProperties(book?: Book): Promise<Book> {
        this.logger?.log('Update book properties', book?.title);

        if (book?.id === undefined) {
            this.logger?.log('Go to Create book', book?.title);
            return this.create(book);
        }

        if (!book) {
            throw new Error('Book is undefined');
        }
        this.logger?.log('Update book', book);
        return this.apiService.put(`book/update/properties`, book);
    }

    updateBookContent(book?: Book): Promise<Book> {
        this.logger?.log('Update book content', book?.title);

        this.checkImages(book);

        if (book?.id === undefined) {
            this.logger?.log('Go to Create book', book?.title);
            return this.create(book);
        }
        if (!book) {
            throw new Error('Book is undefined');
        }
        this.logger?.log('Update book', book);
        return this.apiService.put(`book/update/content`, book);
    }

    updateCategoryTag(id: string, category: string, tag: string): Promise<boolean> {
        return this.apiService.put(`book/updateCategoryTag`, { id, category, tag });
    }

    finish(book?: Book): Promise<Book> {
        return this.apiService.post(`book/finish/${book?.id}`, {});
    }

    generateMeta(id: string): Promise<boolean> {
        return this.apiService.post(`book/generateMeta/${id}`, {});
    }

    fixSplash(id: string): Promise<string> {
        return this.apiService.post(`image/fix/splashes/book/${id}`, {});
    }

    async enhanceMp3(bookId: string): Promise<boolean> {
        let result = await this.apiService.post(`book/enhance/mp3/${bookId}`, {});
        return result.success;
    }

    async syncToLibrary(id: string): Promise<boolean> {
        let result = await this.apiService.post(`book/publish/library/${id}`, {});
        return result.success;
    }

    async syncFinishedToLibrary(): Promise<boolean> {
        await this.apiService.post(`book/publish/library/finished`, {});
        return true;
    }

    openFolder(id: string): Promise<void> {
        return this.apiService.post(`book/openBookFolder`, { id: id });
    }

    updateRank(id: string, rank: number): Promise<boolean> {
        return this.apiService.put(`book/updateRank/${id}/${rank}`, {});
    }

    updatePublishOrder(id: string, order: number): Promise<Book> {
        return this.apiService.put(`book/updatePublishOrder/${id}/${order}`, {});
    }

    async resetBook(id: string): Promise<boolean> {
        let result = await this.apiService.post(`book/reset/${id}`, {});
        return result.success;
    }

    removeBook(id: string): Promise<boolean> {
        return this.apiService.delete(`book/${id}`);
    }

    async getFiles(bookName: string, type: string, pattern: string): Promise<IFile[]> {
        return await this.apiService.get(`book/files/${bookName}/${type}/${pattern}`);
    }

    exportBook(exportModel: BookExportModel): Promise<void> {
        return this.apiService.post(`book/export`, exportModel);
    }

    async downloadTxt(bookName: string): Promise<void> {
        await this.apiService.getFile(`book/download/txt/${bookName}`);
    }

    async downloadPdf(bookName: string): Promise<void> {
        await this.apiService.getFile(`book/download/pdf/${bookName}`);
    }

    async getDashboard(): Promise<DashboardModel> {
        return await this.apiService.get(`dashboard`);
    }

    async initFolders(bookId: string): Promise<boolean> {
        let result = await this.apiService.post(`book/initFolders/${bookId}`, {});
        return result.success;
    }

    async generateSubtitleJobSchedule(): Promise<boolean> {
        let result = await this.apiService.get(`book/generateSubtitleJobSchedule/100`);
        return result.message;
    }

    sortBy(books: Book[], sortBy?: keyof Book, thenBy?: keyof Book, sortOrder?: SortOrder, thenOrder?: SortOrder): void {
        BookSorter.sort(books, sortBy, thenBy, sortOrder, thenOrder);
    }

    private checkImages(book?: Book): void {
        // for (let image of book?.images || []) {
        //     if (!image.id) {
        //         this.logger?.log('Update image', image);
        //         image.id = Math.random().toString(36).substring(2);
        //     }
        // }
    }
}