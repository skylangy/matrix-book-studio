import { Book } from "./book";
import { IPagedList } from "./pagedlist";

export interface GroupedBooks {
    title?: string;
    books?: IPagedList<Book>;
    isVisible?: boolean;
    isExpanded?: boolean;
}