
import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { Order, OrderViewModel } from '../models/order';
import { Promo } from '../models/promo';
import { ApiService } from './api.service';
import { LoggingService } from './logging.service';

@Injectable({ providedIn: 'root' })
export class OrderService {
    private readonly logger: ILogger

    constructor(
        private readonly apiService: ApiService,
        readonly loggingService: LoggingService) {
        this.logger = loggingService.getLogger('OrderService');
    }

    async getOrders(page: number, pageSize: number): Promise<Order[]> {
        return await this.apiService.get(`order/${page}/${pageSize}`);
    }

    async getPromo(code: string): Promise<Promo | undefined> {
        return await this.apiService.get(`promotion/bycode/${code}`);
    }

    async placeOrder(order: Order) {
        return this.apiService.post(`order`, order);
    }

    async getOrder(orderId: string): Promise<Order | undefined> {
        return this.apiService.get(`order/${orderId}`);
    }

    async getOrderSummaries(userId: string): Promise<OrderViewModel[] | undefined> {
        return this.apiService.get(`order/summary/for/user/${userId}`);
    }

    async cancelOrder(orderId: string) {
        return this.apiService.post(`order/update/status/${orderId}/Cancelled`, {});
    }

    async removeOrder(orderId: string) {
        return this.apiService.post(`order/update/status/${orderId}/Removed`, {});
    }

    async search(keyword: string, page: number, pageSize: number): Promise<Order[]> {
        return this.apiService.get(`order/search/${keyword}/${page}/${pageSize}`);
    }
}