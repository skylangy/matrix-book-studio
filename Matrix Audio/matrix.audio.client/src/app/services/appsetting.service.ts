import { Injectable } from '@angular/core';
import { AppSetting } from '../models/app-setting';
import { AppSummary } from '../models/app-summary';
import { IInitializable } from '../models/initializable';
import { Result } from '../models/result';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AppSettingService implements IInitializable {

    private settings: AppSetting[] = [];
    private isLoaded = false;

    constructor(private readonly apiService: ApiService) { }

    get appSettings(): AppSetting[] {
        return this.settings!;
    }

    async initialize(): Promise<void> {
        if (this.isLoaded) {
            return;
        }
        this.settings = await this.apiService.get(`app/settings`);
        this.isLoaded = true;
    }

    get defaultSplash(): string {
        return this.getSplashImageForToday();
    }

    async getIp(): Promise<string> {
        let info = await this.apiService.get('serverinfo/tag');
        return info.ip;
    }

    async getSummary(): Promise<AppSummary> {
        return this.apiService.get('app/summary');
    }

    async populateFromConfig(): Promise<Result> {
        return await this.apiService.post('app/populate/settings/from/config', {});
    }

    getConfig<T>(key: string, defaultValue: T | undefined = undefined): T | undefined {
        const setting = this.settings.find((s) => s.name === key);
        return (setting?.value as T) || defaultValue;
    }

    private getSplashImageForToday(): string {
        const dayOfWeek = new Date().getDay(); // 0 (Sunday) to 6 (Saturday)
        const images = [
            'assets/images/background/bg-1.png', // Sunday
            'assets/images/background/bg-2.png', // Monday
            'assets/images/background/bg-3.png', // Tuesday
            'assets/images/background/bg-4.png', // Wednesday
            'assets/images/background/bg-5.png', // Thursday
            'assets/images/background/bg-6.png', // Friday
            'assets/images/background/bg-7.png'  // Saturday
        ];
        return images[dayOfWeek];
    }
}

