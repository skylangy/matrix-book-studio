import { Book } from "./book";
import { BookSearchModel } from "./book-search-model";
import { SortOrder } from "./book-sort";
import { IFile } from "./file";
import { GroupedBooks } from "./groupedBooks";
import { IPagedList } from "./pagedlist";

export interface IBookService {
    getBooks(): Promise<Book[]>;
    getBook(id: string): Promise<Book>;
    getBookByName(name: string): Promise<Book>;

    getBooksByCategory(category: string, page: number, pageSize: number): Promise<IPagedList<Book>>;
    getAuthorBooks(author: string, page: number, pageSize: number): Promise<IPagedList<Book>>;
    getStatusBooks(status?: string, keyword?: string, sortBy?: string, thenBy?: string, page?: number, pageSize?: number): Promise<GroupedBooks[]>;
    getRecentBooks(count: number, filter?: string): Promise<Book[]>;
    getPublishQueue(): Promise<Book[]>;

    searchGroupedBooks(criteria: BookSearchModel): Promise<IPagedList<Book>>;

    create(book?: Book): Promise<Book>;

    updateBook(book?: Book): Promise<Book>;
    updateBookProperties(book?: Book): Promise<Book>;
    updateBookContent(book?: Book): Promise<Book>;
    finish(book?: Book): Promise<Book>;
    updateRank(id: string, rank: number): Promise<boolean>;
    updatePublishOrder(id: string, order: number): Promise<Book>;
    generateMeta(id: string): Promise<boolean>;
    fixSplash(id: string): Promise<string>;
    syncToLibrary(id: string): Promise<boolean>;
    syncFinishedToLibrary(): Promise<boolean>;
    initFolders(bookId: string): Promise<boolean>;
    enhanceMp3(bookId: string): Promise<boolean>;

    openFolder(id: string): Promise<void>;
    removeBook(id: string): Promise<boolean>;
    resetBook(id: string): Promise<boolean>;

    getFiles(bookName: string, type: string, pattern: string): Promise<IFile[]>;

    downloadTxt(bookName: string): Promise<void>;
    downloadPdf(bookName: string): Promise<void>;

    sortBy(books: Book[], sortBy?: keyof Book, thenBy?: keyof Book, sortOrder?: SortOrder, thenOrder?: SortOrder): void;
}