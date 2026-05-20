import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { CartService } from './cart.service';
import { AuthService } from './auth.service';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';
import { AppSettingService } from './appsetting.service';
import { SettingNames } from '../models/app-setting';
import { UserService } from './user.service';
import { TranslateService } from './translate.service';

@Injectable({ providedIn: 'root' })
export class InitializerService {
    private logger: ILogger;

    constructor(
        private readonly transateService: TranslateService,
        private readonly appSettingService: AppSettingService,
        private readonly authService: AuthService,
        private readonly cartService: CartService,
        private readonly userService: UserService,
        private readonly titleService: Title,
        private readonly loggingService: LoggingService) {
        this.logger = loggingService.getLogger('InitializerService');
    }

    async initialize() {
        await this.appSettingService.initialize();
        await this.authService.initialize();
        await this.cartService.initialize();
        await this.loggingService.initialize();
        await this.userService.initialize();
        await this.transateService.initialize();

        this.titleService.setTitle(this.appSettingService.getConfig(SettingNames.AppName, '')!);
    }
}