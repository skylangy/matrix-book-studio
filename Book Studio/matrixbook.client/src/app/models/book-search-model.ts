export interface BookSearchModel {
    status?: string;
    keyword?: string;
    sortBy?: string;
    thenBy?: string;
    page: number;
    pageSize: number;
    noImage?: boolean;
}