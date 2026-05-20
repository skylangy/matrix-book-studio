
export interface ILogger {
    get name(): string;
    get enabled(): boolean;
    set enabled(value: boolean);
    info(message: string, ...params: any[]): void;
    warn(message: string, ...params: any[]): void;
    error(message: string, ...params: any[]): void;
}

export interface ILoggingService {
    getLogger(loggerName: string, enabled: boolean): ILogger;
}

export class Logger implements ILogger {
    private _name: string;
    private _enabled: boolean = true;

    constructor(loggerName: string, enabled: boolean = true) {
        this._name = loggerName;
        this._enabled = enabled;
    }

    get name(): string {
        return this._name;
    }

    get enabled(): boolean {
        return this.enabled;
    }

    set enabled(value: boolean) {
        this._enabled = value;
    }

    info(message: string, ...params: any[]): void {
        if (!this._enabled) {
            return;
        }
        console.log(this.formatMessage('INFO', message), ...params);
    }

    warn(message: string, ...params: any[]): void {
        if (!this._enabled) {
            return;
        }
        console.warn(this.formatMessage('WARNING', message), ...params);
    }

    error(message: string, ...params: any[]): void {
        if (!this._enabled) {
            return;
        }
        console.error(this.formatMessage('ERROR', message), ...params);
    }

    private getCurrentTimestamp(): string {
        const currentDateTime = new Date();
        return currentDateTime.toISOString(); // Format as ISO string
    }

    private formatMessage(logLevel: string, message: string): string {
        const timestamp = this.getCurrentTimestamp();

        return `[${timestamp}] [${logLevel}] [${this._name}] ${message}`;
    }
}