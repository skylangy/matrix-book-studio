import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { EditorContext } from '../../models/editor-context';
import { EditorPane } from '../../models/editor-pane';
import { IRange } from '../../models/range';
import { Searcher } from 'src/app/models/searcher';
import { Book } from 'src/app/models/book';
import { IOptionService } from 'src/app/models/option-service';
import { IFile } from 'src/app/models/file';

@Component({
    template: '',

})
export abstract class PaneBaseComponent implements OnInit, OnDestroy {
    _pane?: EditorPane;
    private contentSubscription?: Subscription;

    @Input() get pane(): EditorPane | undefined {
        return this._pane;
    }
    set pane(value: EditorPane | undefined) {
        this._pane = value;
        this.onPaneChanged();
    }

    constructor() { }

    ngOnInit(): void {
        let context = this.context;
        if (context) {
            this.contentSubscription = context.contentChanged?.subscribe(async () => {
                await this.onContentChanged();
            });
        }
    }

    ngOnDestroy(): void {
        if (this.contentSubscription) {
            this.contentSubscription.unsubscribe();
        }
    }

    get context(): EditorContext {
        return this.pane?.context;
    }

    get book(): Book | undefined {
        return this.context?.book;
    }

    get editorModel() {
        return this.context?.rawEditor?.getModel();
    }

    get editorContent() {
        return this.editorModel?.getValue();
    }

    get options(): IOptionService {
        return this.context?.optionService!;
    }

    get bookImage(): string | undefined {
        if (this.book && this.book.defaultImageId) {
            return this.context?.imageService?.getImageUrl(this.book.title!, this.book?.defaultImageId) || '';
        }
        return undefined;
    }

    get bookHasImage(): boolean {
        return this.bookImage !== undefined && this.bookImage !== '';
    }

    regexSearch(regex: RegExp, model: any, content: string) {
        return Searcher.regexSearch(regex, model, content);
    }

    goToLine(item: IRange) {
        if (this.context && this.context.rawEditor) {
            let editor = this.context.rawEditor;
            editor.revealLineInCenter(item.startLineNumber);

            editor.setPosition({ lineNumber: item.startLineNumber, column: item.startColumn });
            editor.focus();
        }
    }

    replace(range: IRange) {
        if (!range.match) {
            return;
        }

        let value = this.editorContent.replace(range.match, range.suggestion || '');
        this.context!.rawEditor!.setValue(value);
    }

    replaceRanges(ranges: IRange[]) {
        let value = '';
        for (let range of ranges) {
            value = this.editorContent.replace(range.match, range.suggestion!);
        }
        this.context!.rawEditor!.setValue(value);
    }

    getRangeContent(range: IRange): string {
        if (range.match) {
            return range.match;
        }
        if (this.editorModel) {
            return this.editorModel.getValueInRange(range);
        }
        return '';
    }

    onPaneChanged() {

    }

    onContentChanged() { }

    async getBookFiles(type: string, pattern: string = '*'): Promise<IFile[]> {
        let service = this.context.bookService;
        return await service?.getFiles(this.book?.title!, type, pattern) || [];
    }

    async reloadBook() {
        let service = this.context.bookService;
        let book = this.context.book;
        this.context!.book = await service?.getBookByName(book?.title!);
    }
}
