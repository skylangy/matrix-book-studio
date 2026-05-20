import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Promo } from '../models/promo';

@Injectable({ providedIn: 'root' })
export class PromotionService {
    constructor(private readonly apiService: ApiService) { }

    async getPromo(id: string): Promise<Promo> {
        return this.apiService.get(`promotion/${id}`);
    }

    async getPromos(page = 1, pageSize = 10): Promise<Promo[]> {
        return this.apiService.get(`promotion/all/${page}/${pageSize}`);
    }

    async createPromo(promo: any) {
        return this.apiService.post('promotion', promo);
    }

    async updatePromo(promo: any) {
        return this.apiService.post('promotion', promo);
    }

    async deletePromo(id: string) {
        return this.apiService.delete(`promotion/${id}`);
    }
}