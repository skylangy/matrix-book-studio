import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { CheckboxComponent } from '../checkbox/checkbox.component';
import { UserService } from '../../services/user.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { UserSettings } from '../../models/user-setting';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PromptService } from '../../services/prompt.service';
import { BannerComponent } from '../banner/banner.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { TranslateService } from '../../services/translate.service';
import { Router } from '@angular/router';

export class SettingNames {
    static receiveEmailNotification = 'receiveEmailNotification';
    static language = 'language';
    static theme = 'theme';
}

@Component({
    selector: 'mtx-settings',
    templateUrl: './settings.component.html',
    imports: [CommonModule, FormsModule, HeaderContentComponent,
        CheckboxComponent, BannerComponent,
        TranslatePipe
    ]
})
export class SettingsComponent implements OnInit {
    receiveEmailNotification = true;
    language = 'en';
    theme = 'dark';

    languages = [
        { name: this.translateService.translate('Select your language'), value: '', enabled: false },
        { name: 'English', value: 'en-US', enabled: true },
        { name: '中文', value: 'zh-CN', enabled: true }
    ];
    themes = [
        { name: this.translateService.translate('Select your theme'), value: '', enabled: false },
        { name: this.translateService.translate('Dark'), value: 'dark', enabled: true },
        { name: this.translateService.translate('Light'), value: 'light', enabled: true }
    ];

    private settings: UserSettings = {};
    private logger: ILogger

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly promptService: PromptService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('SettingsComponent');
    }

    async ngOnInit() {
        this.settings = await this.userService.getUserSettings();
        this.logger.info('Settings', this.settings);

        this.receiveEmailNotification = this.getSettingValue(SettingNames.receiveEmailNotification, 'true') === 'true';
        this.language = this.getSettingValue(SettingNames.language, 'en-US');
        this.theme = this.getSettingValue(SettingNames.theme, 'dark');
    }

    getSettingValue(name: string, defaultValue = ''): string {
        if (!this.settings)
            return '';
        let setting = this.settings?.settings?.find(s => s.name === name);
        if (!setting) {
            setting = {
                id: Math.random().toString(36).substring(2),
                name: name,
                value: defaultValue
            };

            this.settings!.settings!.push(setting);
        }

        return setting?.value || defaultValue;
    }

    updateSetting(name: string, value: string) {
        if (!this.settings)
            return;
        let setting = this.settings?.settings?.find(s => s.name === name);
        if (!setting) {
            setting = {
                id: Math.random().toString(36).substring(2),
                name: name,
                value: value
            };
        } else {
            setting.value = value;
        }
    }

    async save() {
        this.updateSetting(SettingNames.receiveEmailNotification, this.receiveEmailNotification.toString());
        this.updateSetting(SettingNames.language, this.language);
        this.updateSetting(SettingNames.theme, this.theme);

        this.logger.info('Save Settings', this.settings);
        await this.userService.updateUserSettings(this.settings);
        await this.promptService.showSuccess('Success', 'Settings saved');

        this.reload();
    }

    private reload() {
        // const currentUrl = this.router.url;
        // this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        //     this.router.navigate([currentUrl]);
        // });

        window.location.reload();
    }
}
