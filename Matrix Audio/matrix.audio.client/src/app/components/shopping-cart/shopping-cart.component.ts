import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartItem } from '../../models/cart-item';
import { ILogger } from '../../models/logger';
import { Promo } from '../../models/promo';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { CartService } from '../../services/cart.service';
import { LoggingService } from '../../services/logging.service';
import { OrderService } from '../../services/order.service';
import { UserService } from '../../services/user.service';
import { OrderSummaryComponent } from '../order-summary/order-summary.component';

@Component({
    selector: 'mtx-shopping-cart',
    templateUrl: 'shopping-cart.component.html',
    imports: [CommonModule, FormsModule, RouterModule, OrderSummaryComponent, TranslatePipe]
})
export class ShoppingCartComponent implements OnInit {
    enablePromo = true;
    promoCode = '';
    promoMessage = '';
    promo: Promo | undefined = undefined;
    private logger: ILogger;

    constructor(
        private readonly userService: UserService,
        private readonly cartService: CartService,
        private readonly orderService: OrderService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('ShoppingCartComponent');
    }

    async ngOnInit() {
        this.logger.info('Shopping cart initialized', this.cartService.items);
        this.promo = this.cartService.promo;
    }

    get cartItems(): CartItem[] {
        return this.cartService.items;
    }

    get tax(): number {
        return this.cartService.tax;
    }

    get promoDiscount(): number {
        return this.cartService.promoDiscount();
    }

    get processingFee(): number {
        return this.cartService.processingFee;
    }

    get subtotal(): number {
        return this.cartService.subtotal;
    }

    get total(): number {
        return this.cartService.total;
    }

    get canRedeem(): boolean {
        return this.promoCode.length > 0 && this.enablePromo && !this.promo;
    }

    get canInputPromoCode(): boolean {
        return this.enablePromo && !this.promo;
    }

    removeItem(item: any): void {
        this.cartService.removeItem(item);
    }

    async redeemPromoCode() {
        let promo = await this.orderService.getPromo(this.promoCode);
        if (!promo) {
            this.logger.error('Invalid promo code');
            this.promoMessage = 'Invalid promo code';
            return;
        }

        const now = new Date();
        const from = new Date(promo.validFrom);
        const to = new Date(promo.validTo);

        this.logger.info('Promo found', promo, now, from, to);

        if (now < from || now > to || !promo.isActive) {
            this.logger.error('Promo code expired');
            this.promoMessage = 'Promo code expired';
            return;
        }
        this.promoMessage = '';
        this.promoCode = '';
        this.promo = promo;
        this.cartService.usePromo(promo);
    }

    clearPromo() {
        this.promo = undefined;
        this.cartService.usePromo(this.promo);
    }
}