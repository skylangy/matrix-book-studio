export interface IPagedList<T> {
    hasNextPage?: boolean;
    hasPrevviousPage?: boolean;
    isFirstPage?: boolean;
    isLastPage?: boolean;
    pageCount?: number;
    pageIndex?: number;
    pageNumber?: number;
    pageSize?: number;
    totalItemCount?: number;
    items?: T[];
}