import { Component } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { Dictionary } from 'src/app/models/dictionary';
import { IRange } from 'src/app/models/range';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from 'src/app/services/logging-services';
import { HighlightService } from 'src/app/services/highlight-service';


@Component({
    selector: 'mtx-validate-pane',
    templateUrl: './validate.pane.component.html',
})
export class ValidatePaneComponent extends PaneBaseComponent {
    errors: IRange[] = [];
    warnings: IRange[] = [];
    attentions: IRange[] = [];

    private logger?: ILogger;

    constructor(
        private highlightService: HighlightService,
        private loggingService: LoggingService) {
        super();
        this.logger = this.loggingService?.getLogger('ValidatePaneComponent');
    }

    override ngOnInit(): void {
        super.ngOnInit();
        this.validate();
    }

    override onContentChanged() {
        return new Promise<void>((resolve, reject) => {
            this.validate();
            resolve();
        });
    }

    hasError(): boolean {
        return this.errors && this.errors.length > 0;
    }

    hasWarning(): boolean {
        return this.warnings && this.warnings.length > 0;
    }

    hasAttention(): boolean {
        return this.attentions && this.attentions.length > 0;
    }

    async validate() {
        if (!this.context || !this.context.rawEditor) {
            return;
        }

        this.logger?.log('Start validate document');

        this.errors = this.regexSearch(Dictionary.getErrorRegex(), this.editorModel, this.editorContent);
        for (let error of this.errors) {
            error.suggestion = Dictionary.getErrorSuggestion(error.match);
        }

        this.warnings = this.regexSearch(Dictionary.getWarnRegex(), this.editorModel, this.editorContent);

        for (let regex of [Dictionary.lineEndWithoutSymbolRegex, Dictionary.dectectOInNumber]) {
            for (let item of this.regexSearch(regex, this.editorModel, this.editorContent)) {
                // await Dictionary.delay(50);
                let isTitle = Dictionary.isChapterTitle(item.match!);
                console.log(`${item.match} is title: ${isTitle} ${regex}`);
                if (isTitle === false) {
                    console.log('Add Warning: ', isTitle, item);
                    this.warnings.push(item);
                }
            }
        }

        //
        this.attentions = [];
        for (let warnRegex of Dictionary.getBracketsRegex()) {
            let attentions = this.regexSearch(warnRegex, this.editorModel, this.editorContent);
            this.attentions.push(...attentions);
        }

        for (let regex of Dictionary.getWebRegexes()) {
            let attentions = this.regexSearch(regex, this.editorModel, this.editorContent);
            this.attentions.push(...attentions);
        }

        for (let regex of Dictionary.getAttentionRegexes()) {
            let attentions = this.regexSearch(regex, this.editorModel, this.editorContent);
            this.attentions.push(...attentions);
        }

        this.highlightService.highlightLongParagraph(this.context.rawEditor);
        this.logger?.log('Document validated');
    }

    fixAllErros() {
        this.replaceRanges(this.errors);
    }

    fixAllWarnings() {
        this.replaceRanges(this.warnings);
    }

    fixAllAttentions() {
        this.replaceRanges(this.attentions);
    }
}

