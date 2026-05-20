import { Chapter } from './chapter';

export interface Book {
    id?: string;
    title?: string;
    subtitle?: string;
    author?: string;
    authorId?: string;
    content?: string;
    summary?: string;
    category?: string;
    tag?: string;
    categoryIds?: string[];
    tagIds?: string[];
    defaultImageId?: string;
    dateCreated?: Date;
    dateUpdated?: Date;
    textCount?: number;
    rank?: number;
    publishOrder?: number;
    images?: string[];
    imageIds?: string[];
    status?: string;
    year?: string;
    isVisible?: boolean;
    speechService?: string;
    language?: string;
    voiceName?: string;
    wavGenerated?: boolean;
    mp3Generated?: boolean;
    mp4Generated?: boolean;
    srtGenerated?: boolean;
    hide?: boolean;
}

export interface ExportBook extends Book {
    chapters?: Chapter[];
    chapterCount?: number;
    chunkCount?: number;
}

export interface BookViewModel extends Book {
    bookCount?: number;
}

export interface BookInfo {
    id?: string;
    title?: string;
    subtitle?: string;
    author?: string;
    status?: string;
    textCount: number;
    dateUpdated?: Date;
    dateCreated?: Date;
}