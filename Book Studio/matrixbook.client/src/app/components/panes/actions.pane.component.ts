import { Component } from '@angular/core';
import { ILogger } from 'src/app/models/logger';
import { IServiceProvider, ServiceProvider } from 'src/app/models/service-provider';
import { ProcessContext } from 'src/app/models/text-processor';
import { TextProcessors } from 'src/app/models/text-processors';
import { ChapterService } from 'src/app/services/chapter-service';
import { HighlightService } from 'src/app/services/highlight-service';
import { LoggingService } from 'src/app/services/logging-services';
import { Action } from '../../models/action';
import { PaneBaseComponent } from './pane.base.component';

@Component({
    selector: 'mtx-actions-pane',
    templateUrl: './actions.pane.component.html',

})
export class ActionsPaneComponent extends PaneBaseComponent {
    actions: Action[] = [
        { id: 'remove-spaces', name: 'Remove Spaces', icon: 'arrows-collapse-vertical', execute: () => { this.callProcessor(TextProcessors.RemoveSpaces); } },
        { id: 'remove-leading-spaces', name: 'Remove Leading Spaces', icon: 'align-start', execute: () => { this.callProcessor(TextProcessors.RemoveLeadingSpaces); } },
        { id: 'remove-tailing-spaces', name: 'Remove Tailing Spaces', icon: 'align-end', execute: () => { this.callProcessor(TextProcessors.RemoveTailingSpaces); } },
        { id: 'remove-page-numbers', name: 'Remove Page Numbers', icon: 'shield-x', execute: () => { this.callProcessor(TextProcessors.RemovePageNumbers); } },
        { id: 'remove-line-breaks', name: 'Remove Line Breaks', icon: 'arrow-return-left', execute: () => { this.callProcessor(TextProcessors.RemoveExtraLineBreaks); } },
        { id: 'remove-num-in-brackets', name: 'Remove Note Numbers', icon: 'bookmark-dash', execute: () => { this.callProcessor(TextProcessors.RemoveNoteNumbers); } },
        { id: 'remove-parentheses', name: 'Remove Parentheses', icon: 'braces', execute: () => { this.callProcessor(TextProcessors.RemoveParentheses); } },
        { id: 'format-parentheses', name: 'Format Parentheses Number', icon: '7-circle', execute: () => { this.callProcessor(TextProcessors.FormatParenthessNumber); } },
        { id: 'replace-permille', name: 'Replace Permille', icon: 'percent', execute: () => { this.callProcessor(TextProcessors.RemovePermille); } },
        { id: 'replace-symbols', name: 'Replace Symbols', icon: 'dot', execute: () => { this.callProcessor(TextProcessors.ReplaceSymbols); } },
        { id: 'sanitize-title', name: 'Sanitize Chapter Title', icon: 'type-h1', execute: () => { this.callProcessor(TextProcessors.SanitizeChapterTitle); } },
        { id: 'break-large_chapters', name: 'Break Chapters', icon: 'file-break', execute: () => { this.callProcessor(TextProcessors.BreakLargeChapters); } },
        { id: 'merge-chapters', name: 'Merge Chapters', icon: 'sign-merge-left', execute: () => { this.callProcessor(TextProcessors.MergeChapters); } },
        { id: 'reorder-chapter-titles', name: 'Reorder Chapters', icon: 'sort-numeric-down', execute: () => { this.callProcessor(TextProcessors.ReorderChapters); } },
        { id: 'format-chapters', name: 'Format Chapters', icon: 'text-paragraph', execute: () => { this.callProcessor(TextProcessors.FormatChapters); } },
        { id: 'convert-full-num-to-half', name: 'Full-Width Chars to Half-Width', icon: 'arrow-down-right-circle', execute: () => { this.callProcessor(TextProcessors.ConvertFullWidthToHalfWidth); } },
        { id: 'convert-to-simplify', name: 'Convert to Simplify', icon: 'c-square', execute: () => { this.callProcessor(TextProcessors.ConvertToSimplifyChinese); } },
        { id: 'convert-title-pattern', name: 'Convert Title Pattern (回->章)', icon: 'arrow-bar-right', execute: () => { this.callProcessor(TextProcessors.ConvertTitlePattern); } },
        { id: 'convert-title-pattern-2', name: 'Convert Title Pattern (卷->章)', icon: 'arrow-bar-right', execute: () => { this.callProcessor(TextProcessors.ConvertTitlePatternJuan); } },
        { id: 'highlight-long-paragraph', name: 'Highlight Long Paragraph', icon: 'text-paragraph', execute: () => { this.highlightLongParagraph(true); } },
        { id: 'replace-known-words', name: 'Replace Known Words', icon: 'file-word', execute: () => { this.callProcessor(TextProcessors.ReplaceKnownWords); } }
    ];

    private logger?: ILogger;
    private serviceProvider: IServiceProvider;
    private textProcessors = new TextProcessors();

    constructor(
        private chapterService: ChapterService,
        private highlightService: HighlightService,
        private loggingService: LoggingService) {
        super();

        this.logger = this.loggingService?.getLogger('ActionsPane');

        this.serviceProvider = new ServiceProvider().register(ChapterService.ServiceName, this.chapterService);
    }

    override onContentChanged() {
        return new Promise<void>((resolve, reject) => {
            this.logger?.log('Content changed');
            this.highlightLongParagraph();
            resolve();
        });
    }

    execute(action?: Action) {
        action?.execute();
    }

    callProcessor(processorName: string) {
        let processor = this.textProcessors.getProcessor(processorName);

        if (processor) {
            let context: ProcessContext = {
                model: this.editorModel,
                services: this.serviceProvider,
                book: this.book,
            };
            this.logger?.log('Processing content with: ', processorName, context);
            let content = processor?.process(this.editorContent, context);
            this.context.editor?.setContent(processorName, content);
        }
        else {
            this.logger?.error('Processor not found: ', processorName);
        }
    }

    highlightLongParagraph(goToBeginning = false) {
        let editor = this.context.rawEditor;
        let firstLongParagraph = this.highlightService.highlightLongParagraph(editor);

        if (firstLongParagraph && goToBeginning) {
            editor.revealLineInCenter(firstLongParagraph.range.startLineNumber);
            editor.setPosition({ lineNumber: firstLongParagraph.range.startLineNumber, column: 1 });
            editor.focus();
        }
    }
}
