import { Component, Input, OnInit } from '@angular/core';
import { User, UserSubscription } from '../../../models/user';
import { SubscriptionPeriod, SubscriptionPlan } from '../../../models/subscription-plan';
import { Selectable } from '../../../models/selectable';
import { UserService } from '../../../services/user.service';
import { ILogger } from '../../../models/logger';
import { LoggingService } from '../../../services/logging.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PromptService } from '../../../services/prompt.service';

@Component({
    selector: 'mtx-admin-user-subscription',
    templateUrl: 'user.subscription.component.html',
    imports: [CommonModule, FormsModule]
})
export class AdminUserSubscriptionComponent implements OnInit {
    @Input() user: User | undefined;

    plans: SubscriptionPlan[] = [];
    periods: Selectable[] = [
        { name: 'Monthly', value: SubscriptionPeriod.Monthly.days, selected: false },
        { name: 'Semiannual', value: SubscriptionPeriod.Semiannual.days, selected: false },
        { name: 'Annual', value: SubscriptionPeriod.Annual.days, selected: true }
    ];
    selectedPeriod: number | undefined
    selectedPlan: string | undefined;
    userSubscription: UserSubscription | undefined;

    private logger: ILogger;

    constructor(private readonly userService: UserService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AdminUserSubscriptionComponent');
    }

    async ngOnInit() {
        this.plans = await this.userService.getAssignableSubscriptions();
        this.logger.info('All plans', this.plans);
        this.logger.info('Periods', this.periods);

        this.userSubscription = await this.userService.getUserSubscription(this.user?.id!);
        this.logger.info('User subscription', this.userSubscription);

        this.selectedPeriod = this.periods[0].value;
        this.selectedPlan = this.plans[0].id;

        this.logger.info('Selected period', this.selectedPeriod);
        this.logger.info('Selected plan', this.selectedPlan);
    }

    canAssign(): boolean {
        return this.selectedPeriod !== undefined && this.selectedPlan !== undefined;
    }

    async assignSubscription() {
        let plan = this.plans.find(x => x.id === this.selectedPlan);
        this.logger.info('Assigning subscription', this.selectedPlan, this.selectedPeriod);
        let result = await this.userService.assignSubscription(this.user!.id!, plan?.id!, this.selectedPeriod!);
        if (result.success) {
            this.promptService.showSuccess('Subscription', `Subscription assigned to ${this.user?.name} successfully`);
        } else {
            this.promptService.showError('Subscription', `Failed to assign subscription to ${this.user?.name}`);
        }
    }
}