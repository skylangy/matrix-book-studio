import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CartService } from '../../services/cart.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Address, EmptyAddress } from '../../models/address';
import { EmptyPaymentInfo, PaymentInfo } from '../../models/payment-info';
import { FormBasedComponent } from '../form-component/form.component';
import { Order, OrderItem } from '../../models/order';
import { UserService } from '../../services/user.service';
import { CartItem } from '../../models/cart-item';
import { Promo } from '../../models/promo';
import { OrderSummaryComponent } from '../order-summary/order-summary.component';
import { Router } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Currencies, OrderStatus, OrderTypes } from '../../models/product-types';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-checkout',
    templateUrl: 'checkout.component.html',
    imports: [CommonModule, FormsModule, ReactiveFormsModule, OrderSummaryComponent, TranslatePipe]
})
export class CheckoutComponent extends FormBasedComponent implements OnInit {

    private logger: ILogger;
    @ViewChild('checkoutModal') modal!: ElementRef;
    promo: Promo | undefined = undefined;
    formValid = true;
    checkoutDone = false;
    billingAddress: Address = EmptyAddress;
    paymentInfo: PaymentInfo = EmptyPaymentInfo;
    countries: string[] = ['United States'];
    states: string[] = [
        'Alabama', 'Alaska', 'Arizona', 'Arkansas', 'California', 'Colorado',
        'Connecticut', 'Delaware', 'Florida', 'Georgia', 'Hawaii', 'Idaho',
        'Illinois', 'Indiana', 'Iowa', 'Kansas', 'Kentucky', 'Louisiana', 'Maine',
        'Maryland', 'Massachusetts', 'Michigan', 'Minnesota', 'Mississippi',
        'Missouri', 'Montana', 'Nebraska', 'Nevada', 'New Hampshire', 'New Jersey',
        'New Mexico', 'New York', 'North Carolina', 'North Dakota', 'Ohio',
        'Oklahoma', 'Oregon', 'Pennsylvania', 'Rhode Island', 'South Carolina',
        'South Dakota', 'Tennessee', 'Texas', 'Utah', 'Vermont', 'Virginia',
        'Washington', 'West Virginia', 'Wisconsin', 'Wyoming'
    ];
    checkoutStatus: string[] = [];

    constructor(
        private readonly router: Router,
        private readonly userService: UserService,
        private readonly cartService: CartService,
        private readonly orderService: OrderService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('CheckoutComponent');
    }

    async ngOnInit() {
        if (this.cartService.numbers == 0) {
            this.router.navigate(['/']);
        }

        this.dataForm = new FormGroup({
            firstName: new FormControl('', [Validators.required]),
            lastName: new FormControl('', [Validators.required]),
            addressLine1: new FormControl('', [Validators.required]),
            addressLine2: new FormControl('', []),
            city: new FormControl('', [Validators.required]),
            state: new FormControl('', [Validators.required]),
            postalCode: new FormControl('', [Validators.required, Validators.pattern('^[0-9]{5}$')]),
            country: new FormControl('', [Validators.required]),
            cardNumber: new FormControl('', [Validators.required, Validators.pattern('^[0-9]{16}$')]),
            cardHolderName: new FormControl('', [Validators.required]),
            expirationDate: new FormControl('', [Validators.required, this.validateExpirationDate]),
            cvv: new FormControl('', [Validators.required, Validators.pattern('^[0-9]{3,4}$')]),
        });
        this.promo = this.cartService.promo;
        this.logger.info('Items', this.cartService.items);
    }

    get cartItems() {
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

    get itemCount() {
        return this.cartService.items.length;
    }

    async submit(dataFormValue: any) {
        if (!this.dataForm.valid) {
            this.dataForm.markAllAsTouched();
            this.logger.info('Invalid form submitted');
            this.formValid = false;
            return;
        }
        this.checkoutStatus = [];

        const formValues = { ...dataFormValue };

        this.checkoutStatus.push('Building order...');
        this.billingAddress = {
            firstName: formValues.firstName,
            lastName: formValues.lastName,
            addressLine1: formValues.addressLine1,
            addressLine2: formValues.addressLine2,
            city: formValues.city,
            state: formValues.state,
            postalCode: formValues.postalCode,
            country: formValues.country
        };
        this.paymentInfo = {
            cardNumber: formValues.cardNumber,
            cardHolderName: formValues.cardHolderName,
            expirationDate: formValues.expirationDate,
            cvv: formValues.cvv
        };

        let id = this.generateId('ORD');
        let orderItems: OrderItem[] = this.cartItems.map(item => this.cartItemToOrderItem(item, id));
        let order: Order = {
            id: id,
            currency: Currencies.USD,
            userId: this.userService.userId!,
            orderType: OrderTypes.Subscription,
            orderStatus: OrderStatus.Pending,
            orderNumber: id,
            description: orderItems[0].productName,
            items: orderItems,
            billingAddress: this.billingAddress,
            paymentInfo: this.paymentInfo,
            promo: this.promo,
            subtotal: this.subtotal,
            total: this.total,
            dateCreated: new Date(),
            dateUpdated: new Date()
        };

        this.checkoutStatus.push('Submitting order...');
        this.logger.info('Submitting order', this.billingAddress, this.paymentInfo, order);

        let result = await this.orderService.placeOrder(order);

        this.logger.info('Order submitted', result);

        this.checkoutStatus.push('Order submitted successfully');
        this.checkoutStatus.push('Your order is processed, your stuatus will be updated shortly');
        this.cartService.clearCart();
        this.checkoutDone = true;
    }

    generateId(prefix: string): string {
        const timestamp = new Date().getTime();
        const randomNum = Math.floor(Math.random() * 1000000);
        return `${prefix}-${timestamp}-${randomNum}`;
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

    validateExpirationDate(control: FormControl): ValidationErrors | null {
        const currentDate = new Date();
        const [month, year] = control.value.split('/').map(Number);
        const expirationDate = new Date(year, month - 1);

        return expirationDate > currentDate
            ? null
            : { expirationDateInvalid: true };
    }

    goToMyOrders() {
        this.router.navigate(['/public/account']);
    }
}