import { ErrorHandler, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';

@Injectable({
    providedIn: 'root'
})
export class GlobalErrorHandler implements ErrorHandler {
    private logger: ILogger | undefined = undefined;

    constructor(private router: Router,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('GlobalErrorHandler');
    }

    handleError(error: any): void {

        this.logger?.error('An unexpected error occurred:', error);

        const errorMessage = error?.message || 'Unknown error';
        this.router.navigate(['/error'], { queryParams: { error: errorMessage } });
    }
}
