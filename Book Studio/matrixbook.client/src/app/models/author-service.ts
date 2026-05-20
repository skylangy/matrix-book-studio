import { Author } from "./author";
import { IPagedList } from "./pagedlist";

export interface IAuthorService {
    getAuthors(): Promise<Author[]>;
    getPagedAuthors(page: number, pageSize: number): Promise<IPagedList<Author>>;
    getAuthor(id: string): Promise<Author>;
    search(keyword: string, page: number, pageSize: number): Promise<IPagedList<Author>>;
    createAuthor(author?: Author): Promise<Author>;
    updateAuthor(author?: Author): Promise<Author>;
    removeAuthor(id: string): Promise<boolean>;
    clearImages(id: string): Promise<void>;
    openFolder(id: string): Promise<void>;
}