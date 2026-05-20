import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { ApiService } from './api-service';
import { LoggingService } from './logging-services';
import { RegexModel } from '../models/regex-model';

@Injectable({
    providedIn: 'root'
})
export class RegexLibService {
    private logger?: ILogger;

    constructor(private apiService: ApiService,
        loggingService: LoggingService) {
        this.logger = loggingService?.getLogger('RegexLibService');
    }

    getAll(): Promise<RegexModel[]> {
        return this.apiService.get('regex/all');
    }

    get(id: string): Promise<RegexModel> {
        return this.apiService.get(`regex/${id}`);
    }

    create(model?: RegexModel): Promise<RegexModel> {
        if (!model) {
            throw new Error('RegexModel is undefined');
        }

        return this.apiService.post(`regex/add`, model);
    }

    update(model: RegexModel): Promise<RegexModel> {
        return this.apiService.put(`regex/update`, model);
    }

    delete(id: string): Promise<boolean> {
        return this.apiService.delete(`regex/${id}`);
    }
}