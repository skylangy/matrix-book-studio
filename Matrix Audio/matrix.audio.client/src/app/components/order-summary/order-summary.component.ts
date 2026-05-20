import { Component, Input, OnInit } from '@angular/core';
import { ILogger } from '../../models/logger';
import { CartService } from '../../services/cart.service';
import { LoggingService } from '../../services/logging.service';
import { CartItem } from '../../models/cart-item';
import { CommonModule } from '@angular/common';
import { Promo } from '../../models/promo';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { TranslateService } from '../../services/translate.service';

@Component({
    selector: 'mtx-order-summary',
    templateUrl: 'order-summary.component.html',
    imports: [CommonModule, FormsModule, RouterModule, TranslatePipe]
})
export class OrderSummaryComponent implements OnInit {
    @Input() proceedLabel = 'Proceed to Checkout';
    @Input() proceedRoute = '/public/secure/checkout';
    @Input() showProceedButton = true;
    @Input() promo: Promo | undefined = undefined;
    private logger: ILogger;

    constructor(
        private readonly cartService: CartService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('OrderSummaryComponent');
    }

    async ngOnInit() {
        this.proceedLabel = this.translateService.translate(this.proceedLabel);
    }

    get cartItems(): CartItem[] {
        return this.cartService.items;
    }

    get itemCount(): number {
        return this.cartService.numbers;
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
}