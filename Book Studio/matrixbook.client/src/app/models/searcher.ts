import { IRange } from "./range";

export class Searcher {
    public static matchToRange(matches: any[]): IRange[] {
        let ranges: IRange[] = [];
        for (let match of matches) {
            let text = match.matches[0];
            let range = match.range;
            let item = {
                match: text,
                startLineNumber: range.startLineNumber,
                startColumn: range.startColumn,
                endLineNumber: range.endLineNumber,
                endColumn: range.endColumn,
                textCount: match.body?.length,
                body: match.body,
            };
            ranges.push(item);
        }
        return ranges;
    }

    public static regexSearch(regex: RegExp, model: any, content: string): IRange[] {
        let matches: any[] = model.findMatches(regex, false, true, false, false, true);

        let match: any;
        let index = 0;
        let matchCount = matches.length;

        while ((match = regex.exec(content)) !== null) {
            if (index >= matchCount) {
                break;
            }

            matches[index].body = match[2] || '';
            matches[index].textCount = match[2]?.length || 0;

            index++;
        }

        return this.matchToRange(matches);
    }

    public static searchRange(regex: RegExp, model: any, content: string): IRange[] {
        let ranges = [];
        let match;
        while ((match = regex.exec(content)) !== null) {

            const startLineNumber = model.getPositionAt(match.index).lineNumber;
            const endLineNumber = model.getPositionAt(match.index + match[0].length).lineNumber - 1;

            ranges.push({
                startLineNumber: startLineNumber,
                startColumn: 0,
                endLineNumber: endLineNumber,
                endColumn: 0,
                body: match[2],
                textCount: match[2].length,
                match: match[1],
            });
        }


        return ranges;
    }

    public static getLargeChapterRanges(regexes: RegExp[], model: any, content: string, maxLen: number): IRange[] {
        return regexes.flatMap(regex => Searcher.regexSearch(regex, model, content))
            .filter(range => range.textCount > maxLen);
    }
}