import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { Order } from '../../../models/order';
import { OrderService } from '../../../services/order.service';
import { AdminPagerComponent } from '../common/pager.component';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-admin-orders',
    templateUrl: 'admin.orders.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})
export class AdminOrdersComponent extends ManageViewBase<Order> {
    private readonly logger: ILogger;

    constructor(
        private readonly orderService: OrderService,
        loggingService: LoggingService
    ) {
        super();
        this.logger = loggingService.getLogger('AdminOrdersComponent');
        this.icon = 'cart';
    }

    override async ngOnInit() {
        super.ngOnInit();
    }

    override async loadItems(): Promise<Order[]> {
        return this.orderService.getOrders(this.page, this.pageSize);
    }

    override async search() {
        this.page = 1;
        this.items = await this.orderService.search(this.searchText, this.page, this.pageSize);
    }
}