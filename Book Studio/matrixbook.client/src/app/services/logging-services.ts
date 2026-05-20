

import { Injectable } from '@angular/core';
import { ILogger, ILoggingService, Logger } from '../models/logger';


@Injectable({
    providedIn: 'root',
})
export class LoggingService implements ILoggingService {
    private _enabled = true;

    private _loggers: Map<string, ILogger> = new Map<string, ILogger>();

    get enabled(): boolean {
        return this._enabled;
    }

    set enabled(value: boolean) {
        this._enabled = value;

        for (const logger of this._loggers.values()) {
            logger.enabled = value;
        }
    }

    getLogger(loggerName: string): ILogger {
        if (this._loggers.has(loggerName)) {
            return this._loggers.get(loggerName)!;
        }

        const logger = new Logger(loggerName);
        this._loggers.set(loggerName, logger);
        return logger;
    }
}
