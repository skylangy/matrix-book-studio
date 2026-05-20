import { Injectable } from '@angular/core';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import { ApiService } from './api.service';
import { PaymentConfirmRequest, PaymentRequest, PaymentResult } from '../models/payment-request';
import { Order } from '../models/order';
import { Result } from '../models/result';

@Injectable({ providedIn: 'root' })
export class PaymentService {
    private stripePromise: Promise<Stripe | null> | null = this.apiService.get('payment/key/publishable').then(config => loadStripe(config.publishableKey));

    constructor(private readonly apiService: ApiService) { }

    async getStripe(): Promise<Stripe | null> {
        return await this.stripePromise;
    }

    async createPaymentIntent(paymentRequest: PaymentRequest): Promise<string> {
        console.log('Create payment intent', paymentRequest);
        let result = await this.apiService.post('payment/intent/create', paymentRequest);

        return result.clientSecret;
    }

    async confirmPayment(request: PaymentConfirmRequest): Promise<Result> {
        let result = await this.apiService.post('payment/intent/confirm', request);

        return result;
    }
}