import { DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { ILogger } from 'src/app/models/logger';
import { OptionNames } from 'src/app/models/options-names';
import { Searcher } from 'src/app/models/searcher';
import { LoggingService } from 'src/app/services/logging-services';
import { Dictionary } from '../../models/dictionary';
import { IRange } from '../../models/range';
import { PaneBaseComponent } from './pane.base.component';

@Component({
    selector: 'mtx-outline-pane',
    templateUrl: './outline.pane.component.html',
    imports: [DecimalPipe],
})
export class OutlinePaneComponent extends PaneBaseComponent {
    chapterRegexes: RegExp[] = [];
    items: IRange[] = [];
    showDangersOnly = false;
    private logger?: ILogger;

    constructor(private loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('OutlinePaneComponent');

    }

    override ngOnInit(): void {
        super.ngOnInit();

        this.chapterRegexes = Dictionary.getOutlineRegexs();
        this.buildOutline();
    }

    override onContentChanged(): void {
        this.buildOutline();
    }

    buildOutline() {
        if (this.context && this.context.rawEditor) {
            this.items = [];

            let model = this.context.rawEditor.getModel();
            if (!model) return;

            let content = model.getValue();

            for (let regex of this.chapterRegexes) {
                let ranges = Searcher.searchRange(regex, model, content);
                for (let range of ranges) {
                    range.tag = {
                        countLevel: this.getTextCountLevel(range.textCount),
                        titleLevel: this.getTitleLevel(range.match),
                        visible: true,
                    };
                }
                this.items.push(...ranges);
            }
            this.updateVisibility();
        }
    }

    getTextCountLevel(count: number): string {
        if (count > this.options.getConfigValue<number>(OptionNames.chapterDangerSize, 36000)
            || count < this.options.getConfigValue<number>(OptionNames.chapterMinDangerSize, 100)) {
            return 'text-bg-danger';
        } else if (count > this.options.getConfigValue<number>(OptionNames.chapterWarningSize, 18000)
            || count < this.options.getConfigValue<number>(OptionNames.chapterMinWarnSize, 1000)) {
            return 'text-bg-warning';
        } else {
            return 'text-bg-secondary';
        }
    }

    getTitleLevel(title?: string): string {
        let level = '';
        if (!title) {
            level = 'text-bg-warning';
        } else if (Dictionary.hasInvalidPathChars(title!)
            || title?.length > this.options.getConfigValue<number>(OptionNames.titleDangerLength, 50)) {
            level = 'text-bg-danger';
        } else if (Dictionary.hasWarnPathChars(title!)
            || title?.length > this.options.getConfigValue<number>(OptionNames.titleWarningLength, 30)) {
            level = 'text-bg-warning';
        } else if (title.startsWith(Dictionary.firstChapter)) {
            level = 'text-success';
        }

        return level;
    }

    showDangers() {
        this.showDangersOnly = !this.showDangersOnly;
        this.updateVisibility();
    }

    updateVisibility() {
        if (this.showDangersOnly) {
            for (let item of this.items) {
                item.tag!.visible = item.tag!.titleLevel === 'text-bg-danger' || item.tag!.countLevel === 'text-bg-danger';
            }
        } else {
            for (let item of this.items) {
                item.tag!.visible = true;
            }
        }
    }
}
