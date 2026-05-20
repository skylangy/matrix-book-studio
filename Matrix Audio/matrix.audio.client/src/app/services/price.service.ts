import { Injectable } from '@angular/core';
import { SubscriptionPlan } from '../models/subscription-plan';

@Injectable({ providedIn: 'root' })
export class PriceService {
    constructor() { }

    get albumRate(): number {
        return 0.99;
    }

    getPlanTotal(plan: SubscriptionPlan, period: string): number {
        // let planRate = this.getPlanRate(plan, period);
        // if (!planRate) {
        //     return 0;
        // }
        // return planRate?.baseRate * planRate?.cycles;
        return 0;
    }

    getDiscountedPlanTotal(plan: SubscriptionPlan, period: string): number {
        // let planRate = this.getPlanRate(plan, period);
        // if (!planRate) {
        //     return 0;
        // }
        // return planRate?.finalRate * planRate?.cycles;
        return 0;
    }
}