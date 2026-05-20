import { Component, OnInit } from '@angular/core';
import { ILogger } from 'src/app/models/logger';
import { OptionGroup, OptionItem } from 'src/app/models/options';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { OptionService } from 'src/app/services/option-service';
import { HeaderComponent } from '../header/header.component';
import { WorkViewComponent } from '../work-view/work.view.component';
import { SettingGroupComponent } from './setting.group.component';

@Component({
    selector: 'mtx-settings',
    templateUrl: './settings.component.html',
    imports: [HeaderComponent, SettingGroupComponent],
})
export class SettingsComponent extends WorkViewComponent implements OnInit {
    groupedOptions: OptionGroup[] = [];
    private logger?: ILogger;

    constructor(
        private optionService: OptionService,
        private notificationService: NotificationService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService?.getLogger('SettingsComponent');
        this.title = 'Settings';
        this.subtitle = 'Book library settings';
        this.bannerImage = './assets/images/splashes/photo-10.jpg';
    }

    override async ngOnInit() {
        super.ngOnInit();

        let options = await this.optionService.getOptions();

        for (let item of options.items) {
            if (item.valueType === 'boolean') {
                item.value = item.value === 'true';
            } else if (item.valueType === 'number') {
                item.value = parseFloat(item.value);
            }
        }

        this.groupedOptions = this.groupOptions(options.items);

        this.logger?.log('Settings loaded.', options, this.groupedOptions);
    }

    groupOptions(optionItems: OptionItem[]): OptionGroup[] {
        const groupedOptions: { [group: string]: OptionItem[] } = optionItems.reduce((acc: any, item) => {
            const groupName = item.group || 'Miscellaneous';
            acc[groupName] = acc[groupName] || [];
            acc[groupName].push(item);
            return acc;
        }, {});

        return Object.entries(groupedOptions).map(([groupName, items]) => ({ groupName, items }));
    }

    async save() {
        let options: OptionItem[] = [];
        this.groupedOptions.forEach(g => {
            for (let item of g.items) {
                item.value = item.value?.toString();
            }
            options = options.concat(g.items);
        });
        this.logger?.log('Save settings', options);
        await this.optionService.updateOptions({ items: options });
        this.notificationService.showSuccess('Saved.', 'Setting changes are saved.');
    }
}
