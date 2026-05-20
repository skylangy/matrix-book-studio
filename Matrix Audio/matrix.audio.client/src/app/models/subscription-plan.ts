

export enum SubscriptionTier {
    Free = 0,
    Silver = 1,
    Gold = 2
}

export const DiscountTypes = {
    Percentage: 'percentage',
    Fixed: 'fixed'
} as const;
export type DiscountType = (typeof DiscountTypes)[keyof typeof DiscountTypes];

export class SubscriptionPeriod {
    // Properties
    name: string;
    months: number;
    days: number;

    // Private constructor to prevent external instantiation
    private constructor(name: string, months: number, days: number) {
        this.name = name;
        this.months = months;
        this.days = days;
    }

    // Static properties to represent predefined periods
    static Monthly = new SubscriptionPeriod("Monthly", 1, 30);
    static Semiannual = new SubscriptionPeriod("Semiannual", 6, 183);
    static Annual = new SubscriptionPeriod("Annual", 12, 366);
}

export interface SubscriptionPlan {
    id: string;
    name: string;
    description?: string;
    tier: SubscriptionTier;
    period: SubscriptionPeriod;
    level: number;
    isActive: boolean;
    monthlyRate: number;
    discountRate: number;
    discountType: DiscountType;
    discount: number;
    total: number;
    totalAfterDiscount: number;
    currency: string;
    startDate: string;
    endDate: string;
    dateCreated: Date;
    dateUpdated: Date;
    features: string[];
}