import { Searcher } from 'src/app/models/searcher';
import { Dictionary } from '../../models/dictionary';

declare var monaco: any;

export class BookLanguage {
    languageName = 'textBook';
    themeName = 'bookTheme';
    fontSize = 16;

    warnLow = '#ffeb3b';
    warnModerate = '#ff9800';
    warnHigh = '#ff5722';
    warnCritical = '#f44336';
    info = '#2196f3';

    initialize() {
        this.initLanguage(monaco.languages);
        this.initTheme(monaco.editor);
    }

    initLanguage(languages: any) {
        languages.register({ id: this.languageName });

        let rules = this.initHightlightRules();

        languages.setMonarchTokensProvider(this.languageName, {
            tokenizer: {
                root: rules,
            },
        });


        languages.registerFoldingRangeProvider(this.languageName, {
            provideFoldingRanges: function (model: any, context: any, token: any) {
                const content = model.getValue();
                const ranges = [];

                let chapterRegexes = Dictionary.getOutlineRegexs();

                for (let regex of chapterRegexes) {
                    let searchResult = Searcher.searchRange(regex, model, content);
                    for (let item of searchResult) {
                        ranges.push({
                            start: item.startLineNumber,
                            end: item.endLineNumber,
                            kind: monaco.languages.FoldingRangeKind.Region,
                        });
                    }
                }

                return ranges;
            },
        });
    }

    initHightlightRules() {
        let rules = [
            [/\b[a-zA-Z]+\b/, 'en-words'],

            [Dictionary.getErrorRegex(), 'content-error'],
        ];
        this.mergeRules(rules, Dictionary.ignoreWords, 'ignored');
        this.mergeRules(rules, Dictionary.getWebRegexes(), 'http-links');
        this.mergeRules(rules, Dictionary.getBracketsRegex(), 'custom-brackets');
        this.mergeRules(rules, Dictionary.getPageNumberRegex(), 'page-number');
        this.mergeRules(rules, Array.from(Dictionary.warnWords.keys()), 'content-warning');
        this.mergeRules(rules, Dictionary.getChapterTitleOrSectionRegex(), 'chapter-title');

        // highlight numbers must after the chapter title, otherwise the chapter title will be highlighted as numbers
        rules.push([Dictionary.numbers, 'numbers']);

        return rules;
    }

    private mergeRules(rules: (string | RegExp)[][], regexes: RegExp[], style: string) {
        for (let regex of regexes) {
            rules.push([regex, style]);
        }
    }

    initCodeActionProvider(languages: any) {
        languages.registerCodeActionProvider(this.languageName, {
            provideCodeActions: (model: any, range: any, context: any, token: any) => {
                let actions = [

                    {
                        title: `Replace`,
                        command: {
                            id: 'replaceText',
                            title: 'Replace Text',
                            arguments: [range],
                        },
                        diagnostics: [],
                        kind: 'quickfix',
                        edit: {
                            edits:
                            {
                                resource: model.uri,
                                edits: [
                                    {
                                        range: '',
                                        text: 'This text replaces the text with the error'
                                    }
                                ]
                            }
                        },
                        isPreferred: false
                    },

                ];
                return {
                    actions: actions,
                    dispose: () => { }
                }
            },
        });
    }

    initHints(languages: any) {
        languages.registerHoverProvider(this.languageName, {
            provideHover: function (model: any, position: any) {
                return {
                    range: new monaco.Range(
                        1,
                        1,
                        model.getLineCount(),
                        model.getLineMaxColumn(model.getLineCount())
                    ),
                    contents: [
                        { value: '**SOURCE**' },
                        {
                            value:
                                '```html\n' +
                                // res.responseText.substring(0, 200) +
                                '\n```',
                        },
                    ],
                };
            },
        });

        languages.registerInlayHintsProvider(this.languageName, {
            provideInlayHints(model: any, range: any, token: any) {
                return {
                    hints: [
                        {
                            kind: monaco.languages.InlayHintKind.Type,
                            position: { column: 7, lineNumber: 4 },
                            label: `(-> 政治)`,
                        },

                    ],
                    dispose: () => { },
                };
            },
        });
    }

    initTheme(editor: any) {

        editor.defineTheme(this.themeName, {
            base: 'vs-dark',
            inherit: true,
            rules: [
                { token: 'custom-brackets', foreground: this.warnModerate },
                { token: 'en-words', foreground: '008800' },
                { token: 'numbers', foreground: '008800' },
                { token: 'http-links', foreground: this.warnModerate },
                { token: 'page-number', foreground: this.warnModerate },
                { token: 'whiteSpace', foreground: this.warnLow, fontStyle: 'bold' },
                { token: 'chapter-title', foreground: this.info, fontStyle: 'bold' },
                { token: 'content-warning', foreground: this.warnHigh, fontStyle: 'bold' },
                { token: 'content-error', foreground: this.warnCritical, fontStyle: 'bold' },

            ],
            colors: {
                // 'editor.foreground': '#F8F8F8',
                // 'editor.background': '#1E1E1E',
            },
        });
    }

    initCommands(commands: any, editor: any) {
        commands.registerCommand('replaceText', (range: any) => {

        });
    }

    buildMarkers(editor: any) {

    }

    initCompletionItemProvider(languages: any) {
        languages.registerCompletionItemProvider(this.languageName, {
            provideCompletionItems: (model: any, position: any) => {
                var word = model.getWordUntilPosition(position);
                var range = {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn,
                    endColumn: word.endColumn,
                };
                var suggestions = [
                    {
                        label: 'simpleText',
                        kind: monaco.languages.CompletionItemKind.Text,
                        insertText: 'simpleText',
                        range: range,
                    }
                ];
                return { suggestions: suggestions };
            },
        });
    }
}