import * as OpenCC from 'opencc-js';
import { ChapterService } from "../services/chapter-service";
import { Book } from "./book";
import { Chapter } from "./chapter";
import { Dictionary } from "./dictionary";
import { Searcher } from "./searcher";
import { IServiceProvider } from "./service-provider";
import { StringHelpers } from "./string-helpers";

export interface ProcessContext {
    model?: any;
    services?: IServiceProvider;
    book?: Book;
}

export interface ITextProcessStep {
    process(rawText: string, context: ProcessContext): string;
}

export abstract class FormatStepBase implements ITextProcessStep {
    process(rawText: string, context: ProcessContext): string {
        return rawText;
    }

    protected formatChapterContent(chapter: Chapter): string {
        let chapterContent = chapter.content?.trim();
        if (!chapterContent?.startsWith('\n')) {
            chapterContent = '\n' + chapterContent;
        }
        if (!chapterContent?.endsWith('\n')) {
            chapterContent = chapterContent + '\n';
        }

        return chapterContent;
    }
}

export class TextProcessor {
    private steps: ITextProcessStep[] = [];

    public use(step: ITextProcessStep): TextProcessor {
        this.steps.push(step);
        return this;
    }

    public process(rawText: string, context: ProcessContext): string {
        let text = rawText;
        for (let step of this.steps) {
            text = step.process(text, context);
        }
        return text;
    }
}

export class RemoveExtraLineBreaksStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        return rawText.replace(/(\r?\n){2,}/g, '\n');
    }
}

export class RemoveSymbolLinesStep implements ITextProcessStep {
    private readonly regexes = new Map<RegExp, string>([
        [/^(…)+$/gm, ''],
        [/※/g, ''],
        [/×{3,}/g, '×××'],
        [/×××/g, '某某某'],
        [/××/g, '某某']
    ]);

    public process(rawText: string, context: ProcessContext): string {
        let text = rawText;
        for (const [regex, replacement] of this.regexes) {
            text = text.replace(regex, replacement);
        }
        return text;
    }
}

export class AppendLinedEndSymbolStep implements ITextProcessStep {
    private readonly regexes = new Map<RegExp, string>([
        [/^(此致|敬礼)$/gm, '$1！'],
        [Dictionary.lineNumberOnly, '$1。'],
        [Dictionary.lineChineseNumberOnly, '$1。'],
    ]);

    public process(rawText: string, context: ProcessContext): string {
        let text = rawText;
        for (const [regex, replacement] of this.regexes) {
            text = text.replace(regex, replacement);
        }
        return text;
    }
}

export class RemoveLineBreakStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let count = 0;
        let content = rawText.replace(Dictionary.lineEndRegexReplace, (
            match: string,
            p1: string,
            p2: string,
            offset: number,
            originalText: string
        ) => {

            const nextLine = Dictionary.getFirstLine(originalText.substring(offset + match.length));

            /**
             * Do not remove line break if:
             * 1. Current line is chapter title 第x章
             * 2. Preface or Postscript 前言|后记|序|序言
             * 3. Next line is chapter title 第x章
             */

            let removeLinebreak = !(Dictionary.isChapterTitle(p1) || Dictionary.isChapterTitle(match))
                && !Dictionary.isChapterTitle(nextLine)
                && !Dictionary.lineNumberOnly.test(p1)
                && !Dictionary.lineChineseNumberOnly.test(nextLine)
                && !Dictionary.lineParenthesesOnly.test(p1)
                ;

            if (removeLinebreak) {
                return p1;
            } else {
                console.log(`Linebreak not removed: ${p1} `);
                console.log(`Is title: ${(Dictionary.isChapterTitle(p1) || Dictionary.isChapterTitle(match))}`);
                console.log(`Is number only: ${Dictionary.lineNumberOnly.test(p1)}`);
                console.log(`Is Chinese number only: ${Dictionary.lineChineseNumberOnly.test(nextLine)}`);
                console.log(`Is Parentheses only: ${Dictionary.lineParenthesesOnly.test(p1)}`);
                console.log('-'.repeat(100));
                return match;
            }
        });

        return content;
    }
}

export class RemoveLineBreakEndWithSpecialCaseStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;
        content = content.replace(Dictionary.lineEndWithQuote, (
            match: string,
            p1: string,
            p2: string,
            offset: number,
            originalText: string
        ) => {
            const nextLine = Dictionary.getFirstLine(originalText.substring(offset + match.length));

            let isChapterTitle = Dictionary.isChapterTitleAsync(p1);
            let isNextChapterTitle = Dictionary.isChapterTitleAsync(nextLine);
            let removeLinebreak = !(isChapterTitle || isNextChapterTitle);

            Dictionary.delay(1);
            // console.log(`Process quote, remove: ${removeLinebreak}, is title: ${Dictionary.isChapterTitle(p1)}, ${p1}`);

            if (removeLinebreak) {
                return p1;
            } else {
                return match;
            }
        });

        content = this.processYear(content);
        content = this.processNo(content);
        content = this.processUnion(content);
        content = this.processComma(content);

        return content;
    }

    private processYear(content: string): string {
        // 1956\n 年
        return content.replace(/(\d+)[\r\n]+(年|月|日|岁|小时|名|多|天|高地|万|节|公里|架)/gm, '$1$2');
    }

    private processNo(content: string): string {
        // 其\n一, 第\n 一
        return content.replace(/(其|第|有|是|过|了)[\r\n]+\s*([一二三四五六七八九十百])/gm, '$1$2');
    }

    private processUnion(content: string): string {
        // 统\n一
        return content.replace(/(统)[\r\n]+(一)/gm, '$1$2');
    }

    private processComma(content: string): string {
        // end with ”, or ,
        return content.replace(/([,])[\r\n]+/gm, '$1');
    }
}

export class FormatParagraphStep implements ITextProcessStep {
    private readonly regexes = new Map<RegExp, string>([
        [/(^[一二三四五六七八九十百千万亿]+、)/g, '\n$1'],
        [/^(第[一二三四五六七八九十百零\d]+章)/gm, '\n$1'],
        [/^(前言|后记|序)/gm, '\n$1'],
        [Dictionary.lineChineseNumberOnly, '\n$1'],
        [Dictionary.lineSubSection, '\n$1'],
        [/。{2,}/gm, '。'],
    ]);

    public process(rawText: string, context: ProcessContext): string {
        let text = rawText;
        for (const [regex, replacement] of this.regexes) {
            text = text.replace(regex, replacement);
        }
        return text;
    }
}

export class RemoveSpacesStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        let processLine = (line: string) => {
            let processedLine = line.trim();

            // Exclude spaces between English words
            if (!Dictionary.getChapterTitleRegex().test(processedLine)
                && !/(前言|后记|序)/.test(processedLine)) {
                processedLine = processedLine.replace(/(?<=(\p{Script=Han}|\d|\p{P}))[^\S\n]+(?=(\p{Script=Han}|\d|\p{P}))/gu, '');
            }
            return processedLine;
        };

        return content.split('\n').map(processLine).join('\n');
    }
}

export class RemoeLeadingSpacesStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        let processLine = (line: string) => {
            // Remove leading and trailing spaces
            let processedLine = line.trimStart();
            return processedLine;
        };

        return content.split('\n').map(processLine).join('\n');
    }
}

export class RemoveTailingSpacesStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        let processLine = (line: string) => {
            // Remove leading and trailing spaces
            let processedLine = line.trimEnd();
            return processedLine;
        };

        return content.split('\n').map(processLine).join('\n');
    }
}

export class RemovePageNumbersStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        for (let regex of Dictionary.getPageNumberRegex()) {
            content = content.replace(regex, '');
        }

        return content;
    }
}

export class RemoveParenthesesStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        content = content.replace(/（(?![一二三四五六七八九十百\d]+）)([^（）]*?)）|\((?![一二三四五六七八九十百\d]+\))([^()]*?)\)/g, '');

        return content;
    }
}

export class RemovePermilleStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        content = content.replace(/(\d+(\.\d+)?)‰/g, function (match: string, p1: string) {
            return `千分之${p1}`;
        });

        return content;
    }
}

export class SanitizeChapterTitleStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        for (let regex of Dictionary.getOutlineRegexs()) {
            let ranges = Searcher.regexSearch(regex, context.model, content);

            for (let range of ranges) {
                let title = range.match || '';

                if (Dictionary.hasInvalidPathChars(title) ||
                    Dictionary.hasWarnPathChars(title)) {

                    let sanitizedTitle = Dictionary.sanitizeTitle(title);
                    if (sanitizedTitle !== title) {
                        content = content.replace(title, sanitizedTitle);
                    }
                }
            }
        }

        return content;
    }
}

export class BreakLargeChaptersStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        let maxLen = 20000;
        let ranges = Searcher.getLargeChapterRanges(Dictionary.getOutlineRegexs(), context.model, content, maxLen);

        for (let range of ranges) {
            let body = range.body || '';
            let chapterContent = StringHelpers.dynamicBreakChapter(range.match!, body, maxLen);

            content = content.replace(range.match!, '');
            content = content.replace(range.body!, chapterContent);
        }

        return content;
    }
}

export class MergeChaptersStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        const maxLen = 20000;
        const chapters = [];
        for (const regex of Dictionary.getOutlineRegexs()) {
            chapters.push(...Searcher.searchRange(regex, context.model, rawText));
        }

        // console.log(`Found ${chapters.length} chapters to merge.`);

        const totalLines: string[] = [];
        let currentRanges = [];
        let totalTextCount = 0;

        for (const range of chapters) {
            // If adding the current chapter exceeds maxLen, finalize the current group
            if (totalTextCount + range.textCount > maxLen && currentRanges.length > 0) {
                totalLines.push(currentRanges[0].match!); // Add title of first chapter

                for (const r of currentRanges) {
                    totalLines.push(r.body!); // Add bodies of all chapters
                }
                currentRanges = [];
                totalTextCount = 0;
            }

            currentRanges.push(range);
            totalTextCount += range.textCount;
        }

        // Finalize remaining chapters
        if (currentRanges.length > 0) {
            totalLines.push(currentRanges[0].match!); // Add title of first chapter
            for (const r of currentRanges) {
                totalLines.push(r.body!); // Add bodies of all chapters
            }
        }

        return totalLines.length > 0 ? totalLines.join('\n') : rawText;
    }
}

export class ConvertToSimplifyStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        content = OpenCC.Converter({ from: 'hk', to: 'cn' })(content);
        return content;
    }
}

export class ConvertTitlePatternStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        content = content.replace(Dictionary.chapterTitleHuiRegex, '第$1章');
        return content;
    }
}

export class ConvertTitlePatternJuanStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;
        content = content.replace(Dictionary.chapterTitleJuanRegex, '第$1章');
        return content;
    }
}

export class FormatChaptersStep extends FormatStepBase {

    public override process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        let chapterService = context.services?.getService<ChapterService>(ChapterService.ServiceName);
        let chapters = chapterService?.toChapters(context.model, content, '') || [];

        let paragraphs: string[] = [];
        for (let chapter of chapters) {
            paragraphs.push(chapter.title!);
            paragraphs.push(this.formatChapterContent(chapter));
        }

        content = paragraphs.join('\n');

        return content;
    }

}

export class FormatParenthessNumberStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;
        content = content.replace(/\（(\d+)\）/g, '$1) ');
        return content;
    }
}

export class ConvertFullWidthToHalfWidthStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;

        content = content.replace(/[０-９]/g, (s: string) => {
            return String.fromCharCode(s.charCodeAt(0) - 65248);
        });

        content = content.replace(/[\uff01-\uff5e]/g, (char: string) => {
            const fullwidthCode = char.charCodeAt(0);
            const halfwidthCode = fullwidthCode - 0xfee0;
            return String.fromCharCode(halfwidthCode);
        });

        return content;
    }
}

export class RemoveNoteNumbersStep implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText;
        content = content.replace(/\[\d+\]/g, '');
        return content;
    }
}

export class ReorderChapterTitleStep extends FormatStepBase {
    public override process(rawText: string, context: ProcessContext): string {
        let content = rawText;
        let chapterService = context.services?.getService<ChapterService>(ChapterService.ServiceName);
        let chapters = chapterService?.toChapters(context.model, content, context.book?.title) || [];

        let index = 1;
        let paragraphs: string[] = [];
        for (let chapter of chapters) {

            if (Dictionary.prefaceOrPostscript.test(chapter.title!)) {
                paragraphs.push(chapter.title!);
                paragraphs.push(this.formatChapterContent(chapter));
                continue;
            }

            if (chapter.title) {
                chapter.title = chapter.title.replace(/第([一二三四五六七八九十百零〇\d]+|[一二三四五六七八九十百零〇]\d{0,1}十[一二三四五六七八九〇\d]*)章/, (match: string, p1: string) => {
                    return `第${StringHelpers.convertToChinese(index.toString())}章`;
                });

                paragraphs.push(chapter.title!);
                paragraphs.push(this.formatChapterContent(chapter));
                index++;
            }
        }
        content = paragraphs.join('\n');

        return content;
    }
}

export class ReplaceSymbolsSteps implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        let content = rawText.replace(/(,)/g, '，')
            .replace(/(;)/g, '；')
            .replace(/(:)/g, '：')
            .replace(/•/g, '·');
        return content;
    }
}

export class ReplaceKnownWordsSteps implements ITextProcessStep {
    public process(rawText: string, context: ProcessContext): string {
        const circledNumbers = '①②③④⑤⑥⑦⑧⑨⑩⑪⑫⑬⑭⑮⑯⑰⑱⑲⑳';
        const braceNumbers = '⑴⑵⑶⑷⑸⑹⑺⑻⑼⑽⑾⑿⒀⒁⒂⒃⒄⒅⒆⒇'
        let content = rawText.replace(/○/g, '零')
            .replace(/史达林/g, '斯大林')
            .replace(/未班车/g, '末班车')
            .replace(/尼克森/g, '尼克松')
            .replace(/9·11/g, '九一一')
            .replace(/雪梨/g, '悉尼')
            .replace(/[①-⑳]/g, (match) => {
                const index = circledNumbers.indexOf(match) + 1;
                return index > 0 ? `(${index})` : match;
            })
            .replace(/[⑴-⒇]/g, (match) => {
                const index = braceNumbers.indexOf(match) + 1;
                return index > 0 ? `(${index})` : match;
            })
            ;
        return content;
    }
}

