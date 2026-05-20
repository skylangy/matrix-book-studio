export interface CartItem {
    productId: string;
    productName: string;
    productType: string; // SubscriptionPlan | Book

    description: string;

    price: number;
    quantity: number;
    discount: number;
    total: number;
    totalAfterDiscount: number;

    dateCreated: Date;
}

