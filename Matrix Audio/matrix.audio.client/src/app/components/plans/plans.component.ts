import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { SubscriptionPlan, SubscriptionPeriod, SubscriptionTier } from '../../models/subscription-plan';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PriceService } from '../../services/price.service';
import { CartService } from '../../services/cart.service';
import { Selectable } from '../../models/selectable';
import { PromptService } from '../../services/prompt.service';
import { BannerComponent } from '../banner/banner.component';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-plans',
    templateUrl: './plans.component.html',
    imports: [CommonModule, RouterModule, FormsModule, BannerComponent, TranslatePipe]
})
export class PlansComponent implements OnInit {
    private allPlans: SubscriptionPlan[] = [];
    plans: SubscriptionPlan[] = [];
    periods: Selectable[] = [
        { name: 'Monthly', value: SubscriptionPeriod.Monthly, selected: false },
        { name: 'Semiannual', value: SubscriptionPeriod.Semiannual, selected: false },
        { name: 'Annual', value: SubscriptionPeriod.Annual, selected: true }
    ];
    selectedPeriod: Selectable | undefined;
    annually = true;
    private logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly priceService: PriceService,
        private readonly cartService: CartService,
        private readonly promptService: PromptService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('PlansComponent');
    }

    async ngOnInit() {
        this.allPlans = await this.userService.getSubscriptions();
        this.selectedPeriod = this.periods.filter(x => x.selected)[0];
        this.logger.info('All plans', this.allPlans, this.selectedPeriod);
        this.plans = this.allPlans.filter(x => x.isActive && x.period.name === this.selectedPeriod?.name);
        this.logger.info('Plans', this.plans);
    }

    onPeriodSelectionChange(selectable: Selectable) {
        this.periods.forEach(s => s.selected = false);
        selectable.selected = true;
        this.selectedPeriod = selectable;

        this.plans = this.allPlans.filter(x => x.isActive && x.period.name === this.selectedPeriod?.name);
    }

    async addToCart(plan: SubscriptionPlan) {
        this.logger.info('Adding to cart', plan, this.selectedPeriod?.value);
        let preCheck = await this.cartService.canAddSubscription(plan);
        this.logger.info('Pre-check result', preCheck);
        if (!preCheck.success) {
            this.logger.info('Pre-check failed', preCheck);
            this.promptService.showWarning('Warning', preCheck.message);
            return;
        }

        this.cartService.addSubscription(plan, `${this.selectedPeriod?.name} subscription to ${plan.name}`);
        this.router.navigate(['public', 'shopping-cart']);
    }

    getPeriodUnit(): string {
        if (this.selectedPeriod?.name === 'Annual') {
            return 'year';
        } else if (this.selectedPeriod?.name === 'Semiannual') {
            return '6 month';
        } else {
            return 'month';
        }
    }

    showBaseRate(): boolean {
        return this.selectedPeriod?.value.baseRate !== this.selectedPeriod?.value.finalRate;
    }
}
