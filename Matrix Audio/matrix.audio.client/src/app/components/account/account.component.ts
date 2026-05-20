import { Component, OnInit } from '@angular/core';
import { User, UserSubscription } from '../../models/user';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../services/user.service';
import { ExpandableComponent } from '../expandable/expandable.component';
import { ResetPasswordComponent } from '../reset-password/reset-password.component';
import { AvatarComponent } from '../avatar/avatar.component';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { OrdersComponent } from '../orders/orders.component';
import { SubscriptionPlan } from '../../models/subscription-plan';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { TranslateService } from '../../services/translate.service';

export interface Action {
    title: string;
    description: string;
    route: string;
}

@Component({
    selector: 'mtx-account',
    templateUrl: './account.component.html',
    imports: [
        CommonModule, FormsModule, RouterModule,
        ExpandableComponent, ResetPasswordComponent,
        AvatarComponent, OrdersComponent, TranslatePipe]
})
export class AccountComponent implements OnInit {
    user: User | undefined = undefined;
    subscription: UserSubscription | undefined = undefined;
    actions: Action[] = [];
    plans: SubscriptionPlan[] = [];
    upgradeToId: string | undefined = undefined;
    canUpgradePlan = false;
    private logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly cartservice: CartService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AccountComponent');
    }

    async ngOnInit() {
        this.user = await this.userService.getCurrentUser();
        this.subscription = await this.userService.getCurrentUserSubscription();
        this.plans = await this.userService.getSubscriptions();
        this.plans.sort((a, b) => a.level - b.level);
        this.actions = this.initActions();

        this.logger.info('All plans', this.plans);

        let checkResult = this.canUpgrade(this.subscription);
        this.canUpgradePlan = checkResult.can;
        if (this.canUpgradePlan) {
            this.plans = checkResult.availablePlans;
            this.upgradeToId = this.plans[0].id;
        }
        this.logger.info('User', this.user, this.subscription, this.plans);
    }

    canUpgrade(plan: UserSubscription): { can: boolean, availablePlans: SubscriptionPlan[] } {
        if (!this.subscription) {
            return { can: false, availablePlans: this.plans };
        }
        let subPlan = this.plans.find(p => p.id === this.subscription?.subscriptionId);
        if (!subPlan) {
            return { can: false, availablePlans: this.plans };
        }
        this.logger.info('Current plan', subPlan, this.subscription);
        return {
            can: this.plans.find(x => x.level > subPlan.level && x.period === subPlan.period) !== undefined,
            availablePlans: this.plans.filter(x => x.level > subPlan.level && x.period === subPlan.period)
        };
    }

    async upgrade() {
        let plans: SubscriptionPlan[] = await this.userService.getSubscriptions() || [];
        let upgradeTo = plans.find(x => x.id === this.upgradeToId);
        if (!upgradeTo) {
            return;
        }
        let currentPlan = plans.find(x => x.id === this.subscription?.subscriptionId);
        this.cartservice.addUpgrade(upgradeTo, currentPlan!, `Upgrade from ${currentPlan?.name} to ${upgradeTo.name}`);
        this.router.navigate(['public', 'shopping-cart']);
    }

    private initActions() {
        return [
            { title: this.translateService.translate('Reset Password'), description: this.translateService.translate('Reset your password'), route: '/reset-password' },
            { title: this.translateService.translate('Change Plan'), description: this.translateService.translate('Change your subscription plan'), route: '/public/plans' },
            { title: this.translateService.translate('Billing Information'), description: this.translateService.translate('Update your billing information'), route: '/billing-information' }
        ];
    }
}
