import { ChapterChunk } from './chapter-chunk';


export interface Chapter {
    title?: string;
    textCount?: number;
    content?: string;
    isSelected?: boolean;
    chunks?: ChapterChunk[];
}