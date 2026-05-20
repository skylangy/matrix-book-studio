export interface PaymentInfo {
    cardNumber: string;
    cardHolderName: string;
    expirationDate: string;
    cvv: string;
}

export const EmptyPaymentInfo: PaymentInfo = {
    cardNumber: '',
    cardHolderName: '',
    expirationDate: '',
    cvv: ''
};