import { Order } from "./order";

export interface PaymentRequest {
    amount: number;
    currency: string;
    description: string;
}

export interface PaymentConfirmRequest {
    tokenId: string;
    order: Order;
}

export interface PaymentResult {
    isSuccess: boolean;
    transactionId?: string;
    errorMessage?: string;
}
