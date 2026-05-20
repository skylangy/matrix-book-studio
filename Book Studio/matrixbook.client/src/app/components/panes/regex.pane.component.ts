import { Component } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from 'src/app/services/logging-services';
import { RegexModel } from 'src/app/models/regex-model';
import { RegexLibService } from 'src/app/services/regexlib-service';
import { NotificationService } from 'src/app/services/notification-service';
import { CN, Dictionary } from 'src/app/models/dictionary';
import { FormsModule } from '@angular/forms';


@Component({
    selector: 'mtx-regex-pane',
    templateUrl: './regex.pane.component.html',
    imports: [FormsModule],
})
export class RegexPaneComponent extends PaneBaseComponent {
    models: RegexModel[] = [];
    model?: RegexModel;
    showEditor = false;
    editMode = false
    icon = 'input-cursor-text';

    private predefinedRegexes: RegexModel[] = [
        { id: '1', name: 'Search Invalid Line End', regex: Dictionary.invalidLineEnd, description: 'Search invalid line end', icon: 'align-end' },
        { id: '2', name: 'Append Period to End', regex: Dictionary.fixLineEnd.toString(), replace: '\\n$1。', description: 'Append period to end', icon: 'chevron-bar-left' },
        { id: '3', name: 'Remove Line End Break', regex: Dictionary.removeLineEnd.toString(), replace: '\\n$1。', description: 'Remove line end break', icon: 'chevron-left' },
        { id: '4', name: 'Search Numbers', regex: CN.numbers, description: 'Search numbers', icon: '0-square' },
        { id: '5', name: 'Search Chapters', regex: Dictionary.chapterTitleMixRegex.toString(), description: 'Search chapters', icon: 'bookmark' },
        { id: '6', name: 'Search Raw Chapter Title', regex: Dictionary.rawChapterTitle.toString(), description: 'Search raw chapter title', icon: 'bookmark-dash' },
        { id: '7', name: 'Search Lead Chinese Numbers', regex: Dictionary.leadChineseNumber, replace: '第$1章 ', description: 'Search lead Chinese numbers', icon: 'bookmark-star' },
    ];
    private logger?: ILogger;

    constructor(
        private regexService: RegexLibService,
        private notificationService: NotificationService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('RegexPaneComponent');
    }

    override async ngOnInit() {
        super.ngOnInit();

        await this.load();
    }

    async load() {
        this.models = this.predefinedRegexes;

        let customModels: RegexModel[] = await this.regexService.getAll();
        customModels.sort((a, b) => {
            if (a.name && b.name) {
                return a.name.localeCompare(b.name);
            } else if (a.name) {
                return -1; // a comes first if b.name is undefined
            } else if (b.name) {
                return 1; // b comes first if a.name is undefined
            } else {
                return 0; // both are undefined, so they're equal
            }
        });
        for (let model of customModels) {
            model.editable = true;
        }
        this.models = this.models.concat(customModels);
        this.model = undefined;
        this.editMode = false;
    }

    execute(model: RegexModel) {
        this.logger?.log('Execute regex: ', model);
        this.popSearchBox(model.regex, model.replace);
    }

    toggleEditor() {
        this.showEditor = !this.showEditor;
        if (this.showEditor && !this.model) {
            this.model = {};
        }
    }

    toggleEditMode() {
        this.editMode = !this.editMode;
    }

    edit(model: RegexModel) {
        this.logger?.log('Edit regex: ', model);
        this.model = { ...model };
        this.showEditor = true;
    }

    async delete(model: RegexModel) {
        this.logger?.log('Delete regex: ', model);
        try {
            await this.regexService.delete(model.id!);
            await this.load();
            this.notificationService.showSuccess('Success', `Regex deleted successfully`);
        }
        catch (err) {
            this.logger?.error('Error deleting model: ', err);
            this.notificationService.showFail('Failed', `Unable to delete the regex`);
        }
    }

    async save() {
        this.logger?.log('Save regex model: ', this.model);
        try {
            if (!this.model?.id) {
                this.model!.id = Math.random().toString(36).substring(2);
            }
            await this.regexService.update(this.model!);
            await this.load();
            this.notificationService.showSuccess('Success', `Regex saved successfully`);
        }
        catch (err) {
            this.logger?.error('Error saving model: ', err);
            this.notificationService.showFail('Failed', `Unable to save the regex`);
        }
        finally {
            this.showEditor = false;
        }
    }

    popSearchBox(regex?: string, replaceText: string = '', eanbleRegex = true) {
        this.logger?.log('Pop search box with: ', regex, replaceText, eanbleRegex);
        let editor = this.context?.rawEditor;
        const findController = editor.getContribution('editor.contrib.findController');

        let state = {
            searchString: regex,
            replaceString: replaceText,
            isRegex: eanbleRegex,
            isReplaceRevealed: replaceText?.length > 0,
        };
        findController.start({
            seedSearchStringFromSelection: 'none',
            forceRevealReplace: replaceText?.length > 0,
            seedSearchStringFromGlobalClipboard: false,
            shouldFocus: 0,
        }, state);
        editor.focus();
    }
}
