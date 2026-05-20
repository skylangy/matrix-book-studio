import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { ApiService } from './api-service';
import { LoggingService } from './logging-services';
import { WorkProgress } from '../models/work-progress';

@Injectable({
    providedIn: 'root'
})
export class WorkProgressService {
    private logger?: ILogger;

    constructor(private apiService: ApiService,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('WorkProgressService');
    }

    getWorkProgresses(category: string): Promise<WorkProgress[]> {
        return this.apiService.get(`logging/${category}`);
    }
}