import { Address } from "./address";
import { Entity } from "./entity";
import { PaymentInfo } from "./payment-info";
import { Promo } from "./promo";

export interface Order extends Entity {
    userId: string;
    orderType: string;
    orderStatus: string;
    orderNumber: string;
    currency: string;
    description?: string;
    subtotal: number;
    total: number;
    dateCreated: Date;
    dateUpdated: Date;
    items: OrderItem[];
    billingAddress?: Address;
    paymentInfo?: PaymentInfo;
    promo?: Promo;
}

export interface OrderItem extends Entity {
    orderId: string;
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    totalAfterDiscount: number;
    discount: number;
}

export interface OrderViewModel {
    orderId: string;
    userId: string;
    orderNumber: string;
    orderStatus: string;
    productName: string;
    subtotal: number;
    total: number;
    dateCreated: Date;
}