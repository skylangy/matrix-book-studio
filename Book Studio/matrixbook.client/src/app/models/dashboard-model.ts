export interface BookInfo {
    title?: string;
    author?: string;
    status?: string;
    textCount?: number;
    dateCreated?: Date;
    dateUpdated?: Date;
}
export interface DashboardModel {
    bookCount?: number;
    finishedBookCount?: number;
    inProgressBookCount?: number;
    wordCount?: number;
    finishedWordCount?: number;
    unfinishedWordCount?: number;
    authorCount?: number;
    books?: BookInfo[];
}