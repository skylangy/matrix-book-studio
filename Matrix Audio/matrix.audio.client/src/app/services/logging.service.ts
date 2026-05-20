

import { Injectable } from '@angular/core';
import { ILogger, ILoggingService, Logger } from '../models/logger';
import { AppSettingService } from './appsetting.service';
import { IInitializable } from '../models/initializable';
import { SettingNames } from '../models/app-setting';


@Injectable({
    providedIn: 'root',
})
export class LoggingService implements ILoggingService, IInitializable {
    private _enabled = true;
    private _loggers: Map<string, ILogger> = new Map<string, ILogger>();

    constructor(private readonly appSettingService: AppSettingService) {

    }

    async initialize(): Promise<void> {
        this._enabled = this.appSettingService.getConfig<string>(SettingNames.AppLogging, 'false') === 'true' || false;
    }

    get enabled(): boolean {
        return this._enabled;
    }

    set enabled(value: boolean) {
        this._enabled = value;

        for (const logger of this._loggers.values()) {
            logger.enabled = value;
        }
    }

    getLogger(loggerName: string, enabled: boolean = false): ILogger {
        if (this._loggers.has(loggerName)) {
            return this._loggers.get(loggerName)!;
        }

        const logger = new Logger(loggerName, this._enabled || enabled);
        this._loggers.set(loggerName, logger);
        return logger;
    }
}
