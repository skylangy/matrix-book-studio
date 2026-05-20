import {
    AppendLinedEndSymbolStep, BreakLargeChaptersStep,
    ConvertFullWidthToHalfWidthStep,
    ConvertTitlePatternJuanStep,
    ConvertTitlePatternStep, ConvertToSimplifyStep,
    FormatChaptersStep,
    FormatParagraphStep,
    FormatParenthessNumberStep,
    MergeChaptersStep,
    RemoeLeadingSpacesStep,
    RemoveExtraLineBreaksStep, RemoveLineBreakEndWithSpecialCaseStep, RemoveLineBreakStep,
    RemoveNoteNumbersStep,
    RemovePageNumbersStep, RemoveParenthesesStep, RemovePermilleStep, RemoveSpacesStep,
    RemoveSymbolLinesStep,
    RemoveTailingSpacesStep,
    ReorderChapterTitleStep,
    ReplaceKnownWordsSteps,
    ReplaceSymbolsSteps,
    SanitizeChapterTitleStep, TextProcessor
} from "./text-processor";

export class TextProcessors {
    public static RemoveExtraLineBreaks = 'RemoveExtraLineBreaks';
    public static RemoveSpaces = 'RemoveSpaces';
    public static RemoveLeadingSpaces = 'RemoveLeadingSpaces';
    public static RemoveTailingSpaces = 'RemoveTailingSpaces';
    public static RemovePageNumbers = 'RemovePageNumbers';
    public static RemoveParentheses = 'RemoveParentheses';
    public static RemovePermille = 'RemovePermille';
    public static RemoveNoteNumbers = 'RemoveNoteNumbers';
    public static SanitizeChapterTitle = 'SanitizeChapterTitle';
    public static BreakLargeChapters = 'BreakLargeChapters';
    public static MergeChapters = 'MergeChapters';
    public static ConvertToSimplifyChinese = 'ConvertToSimplifyChinese';
    public static ConvertTitlePattern = 'ConvertTitlePattern';
    public static ConvertTitlePatternJuan = 'ConvertTitlePatternJuan';
    public static ConvertFullWidthToHalfWidth = 'ConvertFullWidthToHalfWidth';
    public static FormatChapters = 'FormatChapters';
    public static FormatParenthessNumber = 'FormatParenthessNumber';
    public static ReorderChapters = 'ReorderChapters';
    public static ReplaceSymbols = 'ReplaceSymbols';
    public static ReplaceKnownWords = 'ReplaceKnownWords';


    private readonly processors = new Map<string, TextProcessor>([
        [TextProcessors.RemoveExtraLineBreaks, new TextProcessor()
            .use(new AppendLinedEndSymbolStep())
            .use(new RemoveSymbolLinesStep())
            .use(new RemoveExtraLineBreaksStep())
            .use(new RemoveLineBreakStep())
            .use(new RemoveLineBreakEndWithSpecialCaseStep())
            .use(new FormatParagraphStep())],
        [TextProcessors.RemoveSpaces, new TextProcessor().use(new RemoveSpacesStep())],
        [TextProcessors.RemoveLeadingSpaces, new TextProcessor().use(new RemoeLeadingSpacesStep())],
        [TextProcessors.RemoveTailingSpaces, new TextProcessor().use(new RemoveTailingSpacesStep())],
        [TextProcessors.RemovePageNumbers, new TextProcessor().use(new RemovePageNumbersStep())],
        [TextProcessors.RemoveParentheses, new TextProcessor().use(new RemoveParenthesesStep())],
        [TextProcessors.RemovePermille, new TextProcessor().use(new RemovePermilleStep())],
        [TextProcessors.SanitizeChapterTitle, new TextProcessor().use(new SanitizeChapterTitleStep())],
        [TextProcessors.BreakLargeChapters, new TextProcessor().use(new BreakLargeChaptersStep()).use(new ReorderChapterTitleStep())],
        [TextProcessors.MergeChapters, new TextProcessor().use(new MergeChaptersStep()).use(new ReorderChapterTitleStep())],
        [TextProcessors.ConvertToSimplifyChinese, new TextProcessor().use(new ConvertToSimplifyStep())],
        [TextProcessors.ConvertTitlePattern, new TextProcessor().use(new ConvertTitlePatternStep())],
        [TextProcessors.ConvertTitlePatternJuan, new TextProcessor().use(new ConvertTitlePatternJuanStep())],
        [TextProcessors.FormatChapters, new TextProcessor().use(new FormatChaptersStep())],
        [TextProcessors.ConvertFullWidthToHalfWidth, new TextProcessor().use(new ConvertFullWidthToHalfWidthStep())],
        [TextProcessors.RemoveNoteNumbers, new TextProcessor().use(new RemoveNoteNumbersStep())],
        [TextProcessors.ReorderChapters, new TextProcessor().use(new ReorderChapterTitleStep())],
        [TextProcessors.FormatParenthessNumber, new TextProcessor().use(new FormatParenthessNumberStep())],
        [TextProcessors.ReplaceSymbols, new TextProcessor().use(new ReplaceSymbolsSteps())],
        [TextProcessors.ReplaceKnownWords, new TextProcessor().use(new ReplaceKnownWordsSteps())]
    ]);

    getProcessor(name: string): TextProcessor | undefined {
        return this.processors.get(name);
    }
}