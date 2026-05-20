import { Component, OnInit } from '@angular/core';
import { PaymentService } from '../../services/payment.service';
import { Appearance, Stripe, StripePaymentElementOptions } from '@stripe/stripe-js';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { OrderSummaryComponent } from '../order-summary/order-summary.component';
import { Promo } from '../../models/promo';
import { CartService } from '../../services/cart.service';
import { Router } from '@angular/router';
import { TranslateService } from '../../services/translate.service';
import { PaymentRequest } from '../../models/payment-request';
import { Order, OrderItem } from '../../models/order';
import { CartItem } from '../../models/cart-item';
import { Currencies, OrderStatus, OrderTypes } from '../../models/product-types';
import { UserService } from '../../services/user.service';
import { PromptService } from '../../services/prompt.service';
import { CommonModule } from '@angular/common';
@Component({
    selector: 'mtx-stripe-checkout',
    templateUrl: 'stripe.checkout.component.html',
    imports: [TranslatePipe, OrderSummaryComponent, CommonModule]
})
export class StripeCheckoutComponent implements OnInit {
    private readonly logger: ILogger;
    private stripe: Stripe | null = null;
    private elements: any | null = null;
    private clientSecret: string | null = null;
    private paymentElement: any | null = null;
    private orderObj: Order | null = null;
    private readonly elementAppearance: Appearance = {
        theme: 'night'
    };
    private readonly elementOptions: StripePaymentElementOptions = {
        layout: 'accordion'
    };
    promo: Promo | undefined = undefined;
    errorMessage: string | undefined = undefined;
    isSubmitting = false;
    amount = 0;

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly paymentService: PaymentService,
        private readonly cartService: CartService,
        private readonly translateService: TranslateService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('StripeCheckoutComponent');
    }

    async ngOnInit() {
        if (this.cartService.numbers == 0) {
            this.router.navigate(['/']);
        }

        await this.initPaymentIntent();
    }

    get order(): Order | null {
        if (!this.orderObj) {
            this.orderObj = this.createOrder();
        }
        return this.orderObj;
    }

    async initPaymentIntent() {
        try {
            this.stripe = await this.paymentService.getStripe();

            if (!this.stripe) {
                this.logger.error('Failed to load stripe');
                this.errorMessage = this.translateService.translate('Failed to load stripe');
                return;
            }

            this.amount = this.order!.total;
            this.logger.info('Order', this.order?.total);

            let paymentRequest: PaymentRequest = {
                amount: this.amount,
                currency: 'usd',
                description: this.order?.description || ''
            }

            this.clientSecret = await this.paymentService.createPaymentIntent(paymentRequest);

            if (!this.clientSecret) {
                this.logger.error('Failed to create payment intent');
                this.errorMessage = this.translateService.translate('Failed to create payment intent');
                return;
            }

            this.logger.info('Client secret', this.clientSecret);

            this.elements = this.stripe.elements({ clientSecret: this.clientSecret, appearance: this.elementAppearance });

            this.paymentElement = this.elements.create('payment', this.elementOptions);
            this.paymentElement.mount('#payment-element');

            this.logger.info('Payment intent created', this.paymentElement);
        } catch (e) {
            this.logger.error('Failed to initialize payment intent', e);
            this.errorMessage = this.translateService.translate('Failed to initialize payment intent');
        }
    }

    async submit() {
        if (!this.stripe || !this.elements || !this.clientSecret) {
            this.errorMessage = this.translateService.translate('Stripe or client secret not initialized');
            return;
        }
        if (this.isSubmitting)
            return;

        try {
            this.isSubmitting = true;
            this.logger.info('Submitting payment...');
            let validateResult = await this.elements.submit();

            if (validateResult.error) {
                this.logger.error('Validation failed', validateResult.error);
                this.errorMessage = this.translateService.translate('Validation failed');
                return;
            }

            this.logger.info(`Validation succeeded, start generating payment token...`);
            let options = { elements: this.elements, clientSecret: this.clientSecret };
            const confirmToken = await this.stripe.createConfirmationToken(options);

            if (confirmToken.error) {
                this.logger.error('Failed to create payment token', confirmToken.error);
                this.errorMessage = this.translateService.translate('Failed to create payment token');
                return;
            }

            this.logger.info('Payment token created', confirmToken);

            let request = {
                tokenId: confirmToken.confirmationToken?.id || '',
                amount: this.amount,
                currency: 'usd',
                description: '',
                order: this.order!
            };

            this.logger.info('Start confirming payment', request);
            const result = await this.paymentService.confirmPayment(request);
            this.logger.info('Payment result', result);

            if (result.success) {
                this.cartService.clearCart();
                this.promptService.showSuccess(
                    this.translateService.translate('Payment succeeded'),
                    this.translateService.translate('Your payment was successful, thank you for your purchase!'));

                this.router.navigate(['/public/account']);
            } else {
                this.logger.error('Payment failed', result.message);
                this.errorMessage = this.translateService.translate(result.message || 'Payment failed');
            }
        } catch (e) {
            this.logger.error('Failed to create payment token', e);
            this.errorMessage = this.translateService.translate('Failed to create payment token');
            this.promptService.showError(this.translateService.translate('Payment failed'),
                this.translateService.translate('Your payment was failed, please try again later.'));
        } finally {
            this.isSubmitting = false;
        }
    }

    cartItemToOrderItem(cartItem: CartItem, orderId: string): OrderItem {
        return {
            id: this.generateId('ORDITM'),
            productId: cartItem.productId,
            productName: cartItem.productName,
            quantity: cartItem.quantity,
            unitPrice: cartItem.price,
            discount: cartItem.discount,
            totalPrice: cartItem.total,
            totalAfterDiscount: cartItem.totalAfterDiscount,
            orderId: orderId,
        };
    }

    generateId(prefix: string): string {
        const timestamp = new Date().getTime();
        const randomNum = Math.floor(Math.random() * 1000000);
        return `${prefix}-${timestamp}-${randomNum}`;
    }

    private createOrder(): Order {
        let id = this.generateId('ORD');
        let orderItems: OrderItem[] = this.cartService.items.map(item => this.cartItemToOrderItem(item, id));
        let order: Order = {
            id: id,
            currency: Currencies.USD,
            userId: this.userService.userId!,
            orderType: OrderTypes.Subscription,
            orderStatus: OrderStatus.Pending,
            orderNumber: id,
            description: orderItems[0].productName,
            items: orderItems,
            billingAddress: undefined,
            paymentInfo: undefined,
            promo: this.promo,
            subtotal: this.cartService.subtotal,
            total: this.cartService.total,
            dateCreated: new Date(),
            dateUpdated: new Date()
        };
        return order;
    }
}