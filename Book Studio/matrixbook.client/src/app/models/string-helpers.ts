class TextBlock {
    title: string = '';
    content: string[] = [];
    length: number = 0;

    constructor(title: string, content: string = '') {
        this.title = title.trim();

        content = content.trim();
        this.content.push(content);
        this.length = content.length;
    }

    append(text: string) {
        let content = text.trim();
        this.content.push(content);
        this.length += content.length;
    }

    merge(text: string[]) {
        this.content.push(...text);
        this.length += text.map(x => x.length).reduce((a, b) => a + b, 0);
    }

    toString() {
        return `${this.title}\n${this.content.join('\n')}`;
    }
}

export class StringHelpers {
    public static convertToChinese(num: string): string {
        const chineseNumerals = ['一', '二', '三', '四', '五', '六', '七', '八', '九'];
        const units = ['', '十', '百', '千'];

        function numberToChinese(n: number): string {
            if (n === 0) return '零';
            let result = '';
            let str = n.toString();
            let len = str.length;
            let zeroFlag = false;
            for (let i = 0; i < len; i++) {
                let digit = parseInt(str[i], 10);
                let pos = len - i - 1;
                if (digit === 0) {
                    zeroFlag = true;
                } else {
                    if (zeroFlag) {
                        result += '零';
                        zeroFlag = false;
                    }
                    if (!(digit === 1 && pos === 1 && i === 0 && len === 2)) {
                        result += chineseNumerals[digit - 1];
                    }
                    result += units[pos];
                }
            }
            return result;
        }

        return num.replace(/[0-9]+/g, (match) => {
            const number = parseInt(match, 10);
            if (number === 0) return '零';
            if (number <= 9999) {
                return numberToChinese(number);
            }
            return match; // fallback for numbers > 9999
        });
    }

    public static breakLargeChapter(title: string, content: string, maxLen: number, minLen: number): string {

        let paragraphs = content.split('\n');
        let resultChapters = [];

        let chapterIndex = 1;
        let currentChapter = new TextBlock(`${title}\n`);
        chapterIndex++;

        for (let paragraph of paragraphs) {
            paragraph = paragraph.trim();

            if (currentChapter.length + paragraph.length > maxLen) {
                // Start a new chapter
                resultChapters.push(currentChapter);
                currentChapter = new TextBlock(
                    `${title}`,
                    `${paragraph}\n`
                );

                chapterIndex++;
            }
            else {
                currentChapter.append(`${paragraph}\n`);
            }
        }
        resultChapters.push(currentChapter);

        let lastChapter = resultChapters[resultChapters.length - 1];
        if (lastChapter.length < minLen) {

            let nextChapter = resultChapters[resultChapters.length - 2];
            nextChapter.merge(lastChapter.content);
            resultChapters.pop();
        }

        return resultChapters.map(x => x.toString()).join('\n');
    }

    public static dynamicBreakChapter(title: string, content: string, maxLen: number, minLen: number = 1000): string {
        let contentLength = content.length;

        if (contentLength < maxLen) {
            return content;
        }

        maxLen = maxLen - minLen;
        let halfLen = maxLen / 2;
        let partLen = maxLen;
        let remain = contentLength % maxLen;

        if (remain < halfLen) {
            for (let start = maxLen; start >= halfLen; start -= 500) {
                let remainCurrent = contentLength % start;
                if (remainCurrent > remain) {
                    partLen = start;
                    remain = remainCurrent;
                }
            }
        }

        return StringHelpers.breakLargeChapter(title, content, partLen, minLen);
    }
}