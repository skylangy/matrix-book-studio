import { Component, OnInit, signal, Signal } from '@angular/core';
import { BannerComponent } from '../banner/banner.component';
import { FormsModule } from '@angular/forms';
import { EmailSubscribeComponent } from '../email-subscribe/email-subscribe.component';
import { AppSettingService } from '../../services/appsetting.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { AppSummary } from '../../models/app-summary';
import { DecimalPipe } from '@angular/common';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-about',
    templateUrl: './about.component.html',
    imports: [FormsModule, BannerComponent, EmailSubscribeComponent, DecimalPipe, TranslatePipe]
})
export class AboutComponent implements OnInit {
    subscribeEmail: Signal<string> = signal('');
    ip: string = '';
    summary: AppSummary | undefined = undefined;
    private logger: ILogger;

    constructor(
        private readonly appConfigService: AppSettingService,
        readonly loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AboutComponent');
    }

    async ngOnInit() {
        this.ip = await this.appConfigService.getIp() || '';
        this.logger.info('IP address:', this.ip);

        this.summary = await this.appConfigService.getSummary();
        this.logger.info('Summary:', this.summary);
    }

    async subscribe() {

    }
}
