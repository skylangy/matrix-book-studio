import { Injectable } from '@angular/core';
import { IOptions } from '../models/options';
import { IOptionService } from '../models/option-service';
import { OptionNames } from '../models/options-names';
import { ApiService } from './api-service';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging-services';

@Injectable({
    providedIn: 'root',
})
export class OptionService implements IOptionService {

    configuration?: IOptions;

    private logger?: ILogger;

    constructor(private apiService: ApiService,
        loggingService: LoggingService) {
        this.logger = loggingService?.getLogger('OptionService');

        this.getOptions();
    }

    async getOptions(): Promise<IOptions> {
        this.configuration = await this.apiService.get(`options/all`);
        if (this.configuration?.items.length === 0) {
            let defaultConfig = this.initilizeOptions();
            this.logger?.log(`Need initialize options`, defaultConfig);
            this.configuration = await this.updateOptions(defaultConfig);
        }

        this.logger?.log(`Options`, this.configuration);

        return this.configuration!;
    }

    async updateOptions(options: IOptions): Promise<IOptions> {
        let updated = await this.apiService.put(`options/update`, options);
        return updated;
    }

    getConfigValue<T>(name: string, defaultValue?: T): T {
        if (!this.configuration?.items) {
            this.getOptions();
        }
        let item = this.configuration?.items.find(i => i.id === name);
        if (!item)
            return defaultValue!;

        return item?.value as T;
    }

    initilizeOptions(): IOptions {
        return {
            items: [
                { id: 'option-recent-count', name: OptionNames.recentBookCount, displayName: 'Recent Book Count', group: OptionNames.groupGeneral, value: '20', valueType: 'number' },
                { id: 'option-chapter-warn-size', name: OptionNames.chapterWarningSize, displayName: 'Chapter Warning Size', group: OptionNames.groupEditor, value: '18000', valueType: 'number' },
                { id: 'option-chapter-danger-size', name: OptionNames.chapterDangerSize, displayName: 'Chapter Danger Size', group: OptionNames.groupEditor, value: '36000', valueType: 'number' },
                { id: 'option-chapter-min-warn-size', name: OptionNames.chapterMinWarnSize, displayName: 'Chapter Min Warning Size', group: OptionNames.groupEditor, value: '1000', valueType: 'number' },
                { id: 'option-chapter-min-danger-size', name: OptionNames.chapterMinDangerSize, displayName: 'Chapter Min Danger Size', group: OptionNames.groupEditor, value: '100', valueType: 'number' },
                { id: 'option-title-warn-length', name: OptionNames.titleWarningLength, displayName: 'Title Warning Length', group: OptionNames.groupEditor, value: '30', valueType: 'number' },
                { id: 'option-title-danger-length', name: OptionNames.titleDangerLength, displayName: 'Title Danger Length', group: OptionNames.groupEditor, value: '50', valueType: 'number' },
            ]
        };
    }
}
