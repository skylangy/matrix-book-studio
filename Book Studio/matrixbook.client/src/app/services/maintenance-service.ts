import { Injectable } from '@angular/core';
import { ApiService } from './api-service';

@Injectable({ providedIn: 'root' })
export class MaintenanceService {
    constructor(private readonly apiService: ApiService) { }

    async cleanCombineVideos(): Promise<string> {
        let result = await this.apiService.post('maintenance/clean/combinevideos', {});
        return result.message;
    }

    async cleanWavFiles(): Promise<string> {
        let result = await this.apiService.post('maintenance/clean/wav', {});
        return result.message;
    }
}