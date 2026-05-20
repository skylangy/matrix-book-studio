import { Injectable } from '@angular/core';
import { IChapterService } from '../models/chapter-service';
import { Chapter } from '../models/chapter';
import { Dictionary } from '../models/dictionary';
import { Searcher } from '../models/searcher';
import { ChapterChunk } from '../models/chapter-chunk';

@Injectable({
    providedIn: 'root'
})
export class ChapterService implements IChapterService {

    public static readonly ServiceName = 'ChapterService';

    minChunkSize: number = 2500;
    maxChunkSize: number = 3800;

    constructor() { }

    toChapters(model: any, content: string, bookName: string | undefined): Chapter[] {
        let chapters = [];

        for (let regex of Dictionary.getOutlineRegexs()) {
            let ranges = Searcher.searchRange(regex, model, content);
            let chapterRanges = this.rangesToChapters(ranges);
            chapters.push(...chapterRanges);
        }

        if (chapters.length === 0) {
            let chapter = {
                title: bookName,
                content: content,
                textCount: content.length,
                isSelected: true
            }
            chapters.push(chapter);
        }

        return chapters
    }

    getChapters(model: any, content: string): Chapter[] {
        let chapters = [];

        for (let regex of Dictionary.getChapters()) {
            let ranges = Searcher.searchRange(regex, model, content);
            let chapterRanges = this.rangesToChapters(ranges);
            chapters.push(...chapterRanges);
        }

        return chapters
    }

    toChaptersWithChunk(model: any, content: string, bookName: string | undefined): Chapter[] {
        let chapters = this.toChapters(model, content, bookName);
        for (let chapter of chapters) {
            chapter.chunks = this.toChunks(chapter);
        }
        return chapters
    }

    toChunks(chapter: Chapter): ChapterChunk[] {
        let chunks: ChapterChunk[] = [];
        let content = chapter.content;
        let index = 1;
        let chunkArray: string[] = [];
        let currentCount = 0;

        for (let line of content?.split('\n')!) {

            let lineCount = line.length;
            if (currentCount + lineCount > this.minChunkSize) {
                let chunkText = chunkArray.join('\n');

                chunks.push({
                    index: index,
                    content: chunkText,
                    textCount: chunkText.length
                });
                index++;

                chunkArray = [];
                currentCount = 0;
            }

            chunkArray.push(line);
            currentCount += lineCount;
        }

        if (chunkArray.length > 0) {
            let chunkText = chunkArray.join('\n');
            chunks.push({
                index: index,
                content: chunkText,
                textCount: chunkText.length
            });
        }

        return chunks;
    }

    rangeToChapter(range: any): Chapter {
        return {
            title: range.match,
            content: range.body,
            textCount: range.textCount,
            isSelected: true
        }
    }

    rangesToChapters(ranges: any[]): Chapter[] {
        let chapters: Chapter[] = [];
        for (let range of ranges) {
            chapters.push(this.rangeToChapter(range));
        }
        return chapters;
    }

    splitToChapters(content: string, bookName: string | undefined): Chapter[] {
        let chapters = [];

        for (let regex of Dictionary.getOutlineRegexs()) {
            let match;
            while ((match = regex.exec(content)) !== null) {
                let chapter = {
                    title: match[1],
                    content: match[2],
                    textCount: match[2].length,
                    isSelected: true
                }
                chapters.push(chapter);
            }
        }

        if (chapters.length === 0) {
            let chapter = {
                title: bookName,
                content: content,
                textCount: content.length,
                isSelected: true
            }
            chapters.push(chapter);
        }

        return chapters
    }
}