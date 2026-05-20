export class CN {
    public static readonly num = '一二三四五六七八九';
    public static numbers = '\\b\\d+\\b';
}

export class Dictionary {
    public static readonly warnWords = new Map<RegExp, string>([
        [/姊/g, '姐'],
        [/泠/g, '冷'],
        [/○/g, '零'],
        [/书城|电子书搜索下载|学习资源分享|禁书网|羽生堂|潇湘书院|潇洒书院|旧雨楼|注释|编注|设为首页|加入收藏|欢迎加入书社|每日海量书籍|大师课精彩分享|微 信|微信|联系我们/g, ''],
        [/扫码关注/g, ''],
        [/注：|按：|附：|※|〖|〗|■|★|∈γ|γ|＼|✽/g, ''],
        [/#+/g, ''],
        [/扫描/g, ''],
        [/书屋/g, ''],
        [/合影|摄于|留影/g, ''],
        [/@|●|•|■|□|★|∈γ|γ|＼|↓|#|▲|⏎|＊|\*|‛||ζ|щ|Ё|л/g, ''],
        [/⑴|⑵|⑶|⑷|⑸⑹|⑺|⑻|⑼/g, ''],
        [/①|②|③|④|⑤|⑥|⑦|⑧|⑨|⑩|①/g, ''],
        [/❶|❷|❸|❹|❺|❻|❼|❽|❾/g, ''],
        [/１|２|３|４|５|６|７|８|９|０/g, ''],
        [/ＸＸＸ|XXX|×××/g, '某某某'],
        [/ＸＸ|XX/g, '某某'],
        [/×年×月×日/g, '某年某月某日'],
        [/前一页|下一页|回目录|返回总目录|返回主目录|宝猪注|校者按|最新电子书|群主V信|明镜|请不吝点赞|订阅|转发|打赏|点点栏目|扫二维码|版权信息|百度|全书完|公众号/g, ''],
        [/大事年表|Table of Contents|目录|作者注|编者注|译者注|笔者注|序言|译后语/g, ''],
        [/史达林/g, '斯大林'],
        [/”\n\s*([。|，|的])/g, '”$1'],
        [/幷/g, '并'],
        [/5・16/g, '五一六'],
        [/图\d+/g, ''],
        [/&frac34;?/g, '¾'],
    ]);

    public static readonly ignoreWords = [
        /菊香书屋/g, /三味书屋/g, /高清扫描/g, /扫描仪/g
    ];

    public static readonly errorWords = new Map<string, string>([
        ['治金', '冶金'],
        ['政冶', '政治'],
        ['膛目', '瞠目'],
        ['未班车', '末班车'],
    ]);

    public static readonly NamedRegexes = new Map<string, RegExp>([
        ['preface', /(^前言$)([\s\S]*?)(^第一章)/gm],
        ['chapter', /(^第[一二三四五六七八九十百零〇\d]+章$|^第[一二三四五六七八九十百零〇\d]+章\s+.+?$)([^第后]*(?:(?!(^第[一二三四五六七八九十百零〇\d]+章|后记$))[\s\S])*)(?=(第[一二三四五六七八九十百零〇\d]+章|$))/gm],
        ['postscript', /(^后记)([\s\S]*)$/gm],
    ]);

    public static httpRegex = /https?:\/\/[^\s]+/g;
    public static wwwRegex = /www\.[^\s]+/g;
    public static emailRegex = /[^\s]+@[^\s]+/g;
    public static webUrlRegex = /\b(?:\w+\.)+(?:com|net|org|gov)\b/g;
    public static numbers = /\b(\d+(\.\d*)?|\.\d+)\b/;


    // public static chineseNumber = `(?:[一二三四五六七八九]|十[一二三四五六七八九]?|[二三四五六七八九]十[一二三四五六七八九]?|一百[零一二三四五六七八九]?|[二三四五六七八九]百[零一二三四五六七八九]?|[一二三四五六七八九]百[一二三四五六七八九]?|[一二三四五六七八九]百零[一二三四五六七八九]?|[一二三四五六七八九]百[一二三四五六七八九]十[零一二三四五六七八九]?|一千[一二三四五六七八九]?|[二三四五六七八九]千[零一二三四五六七八九]?)`;
    public static chineseNumber = `(?:[一二三四五六七八九](?:千(?:[一二三四五六七八九]?百)?(?:[一二三四五六七八九]?十)?(?:[一二三四五六七八九]|零)?)?|[一二三四五六七八九](?:百(?:[一二三四五六七八九]?十)?(?:[一二三四五六七八九]|零)?)?|[一二三四五六七八九]?十[一二三四五六七八九]?|[一二三四五六七八九])`;
    public static chapterTitleHuiRegex = new RegExp(`^第(${Dictionary.chineseNumber})回.*$`, 'gm');
    public static chapterTitleJuanRegex = new RegExp(`^第(${Dictionary.chineseNumber})卷.*$`, 'gm');
    public static chapterTitleZhangRegex = new RegExp(`^第(${Dictionary.chineseNumber})章.*$`, 'gm');
    public static chapterTitleJieRegex = new RegExp(`^第(${Dictionary.chineseNumber})节.*$`, 'gm');
    public static chapterTitleMixRegex = new RegExp(`^第(${Dictionary.chineseNumber})[回|章|卷|节]`, 'gm');

    public static numberSectionRegex = /^\d+、.*$/gm; //new RegExp('^\\d+、.*$', 'gm');
    public static numberWithQuoteSectionRegex = /^（\d+）[、\s　]*.*$/gm; // new RegExp(`^（\\d+）[、 　 ]?.*$`, 'gm');

    public static chapterSectionRegex = new RegExp(`^(${Dictionary.chineseNumber})[、|\s|　].*$`, 'gm');
    public static chapterSectionWithParenthesesRegex = new RegExp(`^（${Dictionary.chineseNumber}）\s?.*$`, 'gm');

    public static leadChineseNumber = `^(十[${CN.num}]?|[${CN.num}]十[${CN.num}]?|[${CN.num}]) `;
    public static rawChapterTitle = `^([一二三四五六七八九十百零d]+|[一二三四五六七八九十百零])、`;

    public static lineCharactersOnly = /^[\u4E00-\u9FA5A-Za-z0-9\s]+$/g;
    public static lineParenthesesOnly = /^（.*）$/g;
    public static lineEndRegexReplace = /([^\r\n。！？】.?!……]+)(\r?\n|\r)+/g
    public static invalidLineEnd = `^(?!\\s|第)\\S.+?(?<!。|”|。"|？"|！"|！|!|？|:|：|;|\\?|；|……|」|】)$`
    public static fixLineEnd = `^(?!\\s|第|前言)(\\S.+?)(?<!。|”|。"|？"|！"|！|!|？|:|：|;|\\?|；|……|」|】)$`
    public static removeLineEnd = `^(?!\\s|第|前言)\\S(.+)?(?<!。|”|。"|？"|！"|！|!|？|:|：|;|\\?|；|……|」|】)\\n`

    public static lineEndWithQuote = /(^.*(?<!。？)”)(\r?\n|\r)+/mg
    public static lineFollowQuoteEnd = /^\s*([。|，])/g
    public static lineWithoutSymbols = /^[^，。（）《》]*$/g
    public static lineNumberOnly = /^([0-9!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]+)$/gm;
    public static lineChineseNumberOnly = /^([一二三四五六七八九十百]+。?)$/gm;


    public static parenthesesRegex = /（(?![一二三四五六七八九十百]+）)([^（）]*?)）|\((?![一二三四五六七八九十百]+\))([^()]*?)\)/g;
    public static bracketsRegex = /【([^【】]*?)】|\[([^[\]]*?)\]/g; // |「(.*?)」
    public static bracesRegex = /《([^《》]*?)》|\{([^{}]*?)\}/g
    public static angleBracketsRegex = /〈([^〈〉]*?)〉|<([^<>]*?)>/g
    public static squareBracketsRegex = /〔([^〔〕]*?)〕|\[([^[\]]*?)\]/g


    public static windowFileNameRegex = /[<>:"\/\\|?*]/g;
    public static windowsFileNameWarnRegex = /[“|”|、]/g;
    public static windowsFileNameRegex = /：/g;
    public static lineEndWithoutSymbolRegex = /^(?!前言|后记|此致|敬礼|\d+、|[一二三四五六七八九十]+、|第[一二三四五六七八九十百零\d]+(节|章))[^。！：？】.?!」]+[^。！：:；？】.?!……”」]$/gm;
    public static firstChapter = '第一章';
    public static prefaceOrPostscript = /^(前言|后记|序)$/;
    public static sectionStart = /^[一二三四五六七八九十百千万亿]+、|第[一二三四五六七八九十百零\d]+节/;
    public static dectectOInNumber = /\b\d+o+\d+\b/g;
    public static lineSubSection = /(（[一二三四五六七八九十百]）)/g;

    public static circleNote = /[①|②|③|④|⑤|⑥|⑦|⑧|⑨|⑩]/g;
    public static circleNote2 = /[❶|❷|❸|❹|❺|❻|❼|❽|❾]/g;



    public static getRegex(name: string): RegExp | undefined {
        if (Dictionary.NamedRegexes.has(name))
            return Dictionary.NamedRegexes.get(name);

        return undefined;
    }

    public static getOutlineRegexs(): RegExp[] {
        return [
            Dictionary.getRegex('preface')!,
            Dictionary.getRegex('chapter')!,
            Dictionary.getRegex('postscript')!,
        ];
    }

    public static getChapters(): RegExp[] {
        return [
            Dictionary.getRegex('chapter')!,
        ];
    }

    public static getChapterTitleOrSectionRegex(): RegExp[] {
        return [
            Dictionary.prefaceOrPostscript,

            Dictionary.chapterTitleZhangRegex,
            Dictionary.chapterTitleHuiRegex,
            Dictionary.chapterTitleJuanRegex,

            Dictionary.chapterTitleJieRegex,
            Dictionary.chapterSectionRegex,
            Dictionary.chapterSectionWithParenthesesRegex,

            Dictionary.numberSectionRegex,
            Dictionary.numberWithQuoteSectionRegex
        ];
    }

    public static getChapterTitleRegex(): RegExp {
        return /^第[一二三四五六七八九十百零\d]+(章|回|卷|集|部|篇|节)/;
    }

    public static getWarnRegex() {
        let keys = Array.from(Dictionary.warnWords.keys());
        return new RegExp(`${keys.join('|')}`);
    }

    public static getErrorRegex() {
        let keys = Array.from(Dictionary.errorWords.keys());
        return new RegExp(`${keys.join('|')}`);
    }

    public static getPageNumberRegex(): RegExp[] {
        return [
            /^(-\s*(\d+)\s*-)[\r\n]*$/gm,
            /^(—\s*(\d+)\s*—)[\r\n]*$/gm,
            /^(\d+)[\r\n]*$/gm,
            /^(第\s*\d+\s*页.*)[\r\n]*$/gm,
            /^(第\s*\d+\s*(\/|／)\s*\d+\s*页)[\r\n]*$/gm,
        ];
    }

    public static getErrorSuggestion(value?: string): string {
        if (Dictionary.errorWords.has(value!))
            return Dictionary.errorWords.get(value!)!;

        return '';
    }

    public static getWarnSuggestion(value?: RegExp): string {
        if (Dictionary.warnWords.has(value!))
            return Dictionary.warnWords.get(value!)!;

        return '';
    }

    public static getBracketsRegex(): RegExp[] {
        return [
            Dictionary.parenthesesRegex,
            Dictionary.bracketsRegex,
            // Dictionary.bracesRegex,
            Dictionary.angleBracketsRegex,
            Dictionary.squareBracketsRegex,
        ];
    }

    public static getWebRegexes(): RegExp[] {
        return [
            Dictionary.httpRegex,
            Dictionary.wwwRegex,
            Dictionary.emailRegex,
            Dictionary.webUrlRegex,
        ];
    }

    public static getAttentionRegexes(): RegExp[] {

        return [
            /(?<![地])图\d+/g,
            /(?<![手|代|发])表\d+/g,
            /(?<![按])照片\d+/g,
            /(?<![最])后一页/g
        ];
    }

    public static hasInvalidPathChars(title: string): boolean {
        return !!title &&
            Dictionary.windowFileNameRegex.test(title);
    }

    public static hasWarnPathChars(title: string): boolean {
        let result = !!title && (Dictionary.windowsFileNameWarnRegex.test(title) || Dictionary.windowsFileNameRegex.test(title));
        return result;
    }

    public static sanitizeTitle(title: string): string {
        return title.replace(Dictionary.windowFileNameRegex, '')
            .replace(Dictionary.windowsFileNameWarnRegex, '')
            .replace(Dictionary.windowsFileNameRegex, '-');
    }

    public static getFirstLine(content: string): string {

        const unixLineBreakPos = content.indexOf('\n');
        const windowsLineBreakPos = content.indexOf('\r\n');

        let firstLineBreakPos;
        if (unixLineBreakPos === -1 && windowsLineBreakPos === -1) {
            return content; // No line breaks found
        } else if (unixLineBreakPos === -1) {
            firstLineBreakPos = windowsLineBreakPos;
        } else if (windowsLineBreakPos === -1) {
            firstLineBreakPos = unixLineBreakPos;
        } else {
            firstLineBreakPos = Math.min(unixLineBreakPos, windowsLineBreakPos);
        }

        return content.substring(0, firstLineBreakPos);

    }

    public static isPrefaceOrEpilogue(line: string): boolean {
        return Dictionary.prefaceOrPostscript.test(line);
    }

    public static isChapterTitle(line: string): boolean {
        let trim = line.trim();
        if (trim.length === 0) {
            return false;
        }

        let regexes = Dictionary.getChapterTitleOrSectionRegex();

        let result = false;
        for (let regex of regexes) {
            if (regex.test(trim)) {
                result = true;
                break;
            }
        }
        return result;
    }

    public static async isChapterTitleAsync(line: string): Promise<boolean> {
        await Dictionary.delay(1); // Introduce a small delay for debugging

        let trim = line.trim();
        if (trim.length === 0) {
            return false;
        }

        let regexes = Dictionary.getChapterTitleOrSectionRegex();

        let result = false;
        for (let regex of regexes) {
            if (regex.test(trim)) {
                result = true;
                console.log('Is Chapter title:', line, regex);
                break;
            }
        }

        return result;
    }

    public static delay(ms: number) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}