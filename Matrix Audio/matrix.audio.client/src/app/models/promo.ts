import { Entity } from "./entity";
import { DiscountType } from "./subscription-plan";

export interface Promo extends Entity {
    code: string;
    min: number;
    discount: number;
    discountType: DiscountType; // percentage or fixed
    validFrom: string;
    validTo: string;
    dateCreated: Date;
    isActive: boolean;
}