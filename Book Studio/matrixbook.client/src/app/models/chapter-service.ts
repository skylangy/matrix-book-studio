import { Chapter } from "./chapter";
import { ChapterChunk } from "./chapter-chunk";

export interface IChapterService {

    toChapters(model: any, content: string, bookName: string | undefined): Chapter[];

    toChaptersWithChunk(model: any, content: string, bookName: string | undefined): Chapter[];

    toChunks(chapter: Chapter): ChapterChunk[];

    splitToChapters(content: string, bookName: string | undefined): Chapter[];
}