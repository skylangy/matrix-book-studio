import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { UserService } from '../../services/user.service';
import { OrderViewModel } from '../../models/order';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-orders',
    templateUrl: 'orders.component.html',
    imports: [CommonModule, TranslatePipe]
})
export class OrdersComponent implements OnInit {
    orders: OrderViewModel[] = [];
    private readonly logger: ILogger;

    constructor(
        private readonly userService: UserService,
        private readonly orderService: OrderService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('OrdersComponent');
    }

    async ngOnInit() {
        await this.loadOrders();
    }

    get hasOrders(): boolean {
        return this.orders.length > 0;
    }

    async loadOrders() {
        this.orders = await this.orderService.getOrderSummaries(this.userService.userId!) || [];
        this.logger.info('Orders loaded', this.orders);
    }

    async removeOrder(orderId: string) {
        await this.orderService.removeOrder(orderId);
        await this.loadOrders();
    }

    async cancelOrder(orderId: string) {
        await this.orderService.cancelOrder(orderId);
        await this.loadOrders();
    }
}