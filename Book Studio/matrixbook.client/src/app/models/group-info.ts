import { Book } from "./book";
import { IPagedList } from "./pagedlist";

export interface GroupInfo {
    id?: string;
    name?: string;
    count?: number;
    isVisible?: boolean;
    isExpanded?: boolean;
    books?: IPagedList<Book>;
}