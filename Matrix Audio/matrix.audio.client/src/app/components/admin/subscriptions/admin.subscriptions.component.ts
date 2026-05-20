import { Component, OnInit } from '@angular/core';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { SubscriptionPlan } from '../../../models/subscription-plan';
import { UserService } from '../../../services/user.service';
import { AdminPagerComponent } from '../common/pager.component';
import { Router, RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-admin-subscriptions',
    templateUrl: 'admin.subscriptions.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})
export class AdminSubscriptionsComponent extends ManageViewBase<SubscriptionPlan> {
    private readonly logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        loggingService: LoggingService
    ) {
        super();
        this.logger = loggingService.getLogger('AdminSubscriptionsComponent');
        this.useSearch = false;
        this.icon = 'calendar-heart';
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.actions = [
            {
                name: 'Add Subscription',
                icon: 'plus-square',
                action: async () => {
                    this.router.navigate(['/control-tower/new/subscription']);
                }
            }];
    }

    override async loadItems(): Promise<SubscriptionPlan[]> {
        return this.userService.getSubscriptions();
    }
}