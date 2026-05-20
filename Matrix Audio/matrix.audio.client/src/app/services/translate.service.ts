import { Injectable } from '@angular/core';
import { UserService } from './user.service';
import { SettingNames } from '../models/app-setting';
import { ILocalizer } from '../models/localizer';
import { LocalizerSimplifiedChinese } from '../models/localize-cn';
import { IInitializable } from '../models/initializable';

@Injectable({ providedIn: 'root' })
export class TranslateService implements IInitializable {
    private defaultLocale = 'en-US';
    private locale: string | undefined = undefined;
    private localizers = new Map<string, ILocalizer>([
        ['zh-CN', new LocalizerSimplifiedChinese()]
    ]);

    constructor(private readonly userService: UserService
    ) { }

    async initialize(): Promise<void> {
        const browserLanguage = navigator.language;
        if (browserLanguage.startsWith('zh')) {
            this.locale = 'zh-CN';
        } else if (browserLanguage.startsWith('en')) {
            this.locale = 'en-US';
        }

        let appSettings = await this.userService.getUserSettings();
        if (appSettings) {
            let userLocal = appSettings.settings?.find(s => s.name === SettingNames.Language);
            if (userLocal && userLocal.value) {
                this.locale = userLocal.value;
            }
        }
    }

    translate(value: string, locale: string = ''): string {
        if (!this.locale) {
            this.locale = this.userService.getSetting(SettingNames.Language, this.defaultLocale);
        }

        if (!locale) {
            locale = this.locale;
        }

        if (locale === this.defaultLocale) {
            return value;
        }
        let localizer = this.localizers.get(locale);
        if (localizer) {
            return localizer.translate(value);
        }

        return value;
    }
}