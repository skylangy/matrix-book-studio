import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Faq } from '../models/faq';

@Injectable({ providedIn: 'root' })
export class FaqService {

    constructor(private apiService: ApiService) { }

    async getFaq(id: string): Promise<Faq> {
        return this.apiService.get(`faq/${id}`);
    }

    async getFaqs(page: number = 1, pageSize: number = 12): Promise<Faq[]> {
        return this.apiService.get(`faq/all/${page}/${pageSize}`);
    }

    async getFaqsForAdmin(page: number = 1, pageSize: number = 12): Promise<Faq[]> {
        return this.apiService.get(`faq/admin/all/${page}/${pageSize}`);
    }

    async search(keyword: string, page: number = 1, pageSize: number = 12): Promise<Faq[]> {
        return this.apiService.get(`faq/search/${keyword}/${page}/${pageSize}`);
    }

    async delete(id: string): Promise<void> {
        return this.apiService.delete(`faq/${id}`);
    }

    async updateFaq(faq: Faq): Promise<Faq> {
        return this.apiService.put(`faq/update`, faq);
    }
}