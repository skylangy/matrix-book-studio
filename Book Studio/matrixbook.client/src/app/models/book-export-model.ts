import { Chapter } from "./chapter";

export interface BookExportModel {
    bookName: string;
    author: string;
    type: string;
    speechService?: string;
    language?: string;
    voiceName?: string;
    image?: string;
    chapters: Chapter[];
}