import { DatePipe } from '@angular/common';
import { Injectable } from '@angular/core';
import { Album } from '../models/album';
import { CartItem } from '../models/cart-item';
import { IInitializable } from '../models/initializable';
import { ILogger } from '../models/logger';
import { ProductTypes } from '../models/product-types';
import { Promo } from '../models/promo';
import { DiscountTypes, SubscriptionPlan } from '../models/subscription-plan';
import { LocalDataService } from './local.data.service';
import { LoggingService } from './logging.service';
import { PriceService } from './price.service';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class CartService implements IInitializable {
    private readonly cartKey = 'cart-items';
    private readonly promoKey = 'cart-promo';
    private taxRate = 0;
    private _items: CartItem[] = [];
    private logger: ILogger;
    private _promo: Promo | undefined;

    constructor(
        private readonly priceService: PriceService,
        private readonly localDataService: LocalDataService,
        private readonly userService: UserService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('CartService');
    }

    get items(): CartItem[] {
        return this._items;
    }

    get numbers(): number {
        return this._items.length;
    }

    get tax(): number {
        return this.subtotal * this.taxRate;
    }

    get processingFee(): number {
        if (this.numbers == 0)
            return 0;
        return 1;
    }

    get subtotal(): number {
        return this.round(this._items.reduce((sum, item) => sum + item.totalAfterDiscount, 0));
    }

    get total(): number {
        let subtotal = this.subtotal;
        if (subtotal === 0) {
            return 0;
        }

        if (this.promo) {
            if (this.promo.discountType === DiscountTypes.Percentage) {
                subtotal = subtotal * (1 - this.promo.discount / 100) + this.tax;
            } else {
                subtotal = subtotal + this.promo.discount + this.tax;
            }
        } else {
            subtotal = subtotal + this.tax;
        }

        return this.round(subtotal + this.processingFee);
    }

    get promo(): Promo | undefined {
        return this._promo;
    }

    usePromo(promo?: Promo) {
        this._promo = promo;
        this.updateStorage();
    }

    promoDiscount(): number {
        if (this.promo) {
            return this.promo.discount;
        }
        return 0;
    }

    addItem(item: CartItem) {
        this._items.push(item);
        this.updateStorage();
    }

    removeItem(item: CartItem) {
        const index = this._items.indexOf(item);
        if (index > -1) {
            this._items.splice(index, 1);
        }

        this.updateStorage();
    }

    clearCart() {
        this._items = [];
        this._promo = undefined;
        this.updateStorage();
    }

    async initialize(): Promise<void> {
        this._items = this.localDataService.get(this.cartKey) || [];
        this._promo = this.localDataService.get(this.promoKey) || undefined;
    }

    async canAddSubscription(plan: SubscriptionPlan): Promise<{ success: boolean, message: string }> {
        let existing = this._items.find(i => i.productId === plan.id);
        if (existing) {
            return { success: false, message: 'Item already added to shopping cart' };
        }
        existing = this._items.find(i => i.productType === ProductTypes.SubscriptionPlan);
        if (existing) {
            return { success: false, message: 'Only one subscription plan can be added to cart' };
        }

        let subscription = await this.userService.getCurrentUserSubscription();
        this.logger.info('User subscriptions', subscription);
        if (subscription && subscription.startDate && subscription.endDate) {
            let now = new Date();
            let startDate = new Date(subscription.startDate);
            let endDate = new Date(subscription.endDate);

            if (now > startDate && now < endDate) {
                return { success: false, message: `You already have an active subscription which will end at ${this.formatDate(endDate)}` };
            }
        }

        return { success: true, message: '' };
    }

    addSubscription(plan: SubscriptionPlan, description: string) {
        if (!plan)
            return;

        const item: CartItem = {
            productName: plan.name,
            productType: ProductTypes.SubscriptionPlan,
            productId: plan.id,
            description: description,
            price: plan.monthlyRate,
            discount: plan.discount || 0,
            total: plan.total,
            totalAfterDiscount: plan.totalAfterDiscount,
            quantity: plan.period.months,
            dateCreated: new Date()
        };
        this.addItem(item);

        this.logger.info('Item added to cart', item);
    }

    addAlbum(album: Album, description: string) {
        const item: CartItem = {
            productName: album.title || '',
            productType: ProductTypes.Album,
            productId: album.id || '',
            description: description,
            price: this.priceService.albumRate,
            discount: 1,
            total: this.priceService.albumRate,
            totalAfterDiscount: this.priceService.albumRate,
            quantity: 1,
            dateCreated: new Date()
        };
        this.addItem(item);

        this.logger.info('Item added to cart', item);

        this.updateStorage();
    }

    async addUpgrade(upgradeTo: SubscriptionPlan, fromPlan: SubscriptionPlan, description: string) {
        this.logger.info('Upgrade from', fromPlan, 'to', upgradeTo);
        let subscription = await this.userService.getCurrentUserSubscription();
        let now = new Date();
        let passDays = this.toDays(now, new Date(subscription.startDate!));
        let totalDays = this.toDays(new Date(subscription.startDate!), new Date(subscription.endDate!));
        let spend = passDays * fromPlan.totalAfterDiscount / totalDays;
        let remaining = fromPlan.totalAfterDiscount - spend;
        let totalAfterDiscount = upgradeTo.totalAfterDiscount - remaining;
        this.logger.info('Per day cost', fromPlan.totalAfterDiscount / totalDays);
        this.logger.info('Total days', totalDays);
        this.logger.info('used day', passDays);
        this.logger.info('Spent ', spend);
        this.logger.info('Remaining ', remaining);
        this.logger.info('Total after discount', totalAfterDiscount);

        const cartItem: CartItem = {
            productName: upgradeTo.name,
            productType: ProductTypes.SubscriptionPlan,
            productId: upgradeTo.id,
            description: description,
            price: upgradeTo.monthlyRate,
            discount: upgradeTo.discount || 0,
            total: upgradeTo.total,
            totalAfterDiscount: totalAfterDiscount,
            quantity: upgradeTo.period.months,
            dateCreated: new Date()
        };
        this.logger.info('Upgrade item', cartItem);
        this.addItem(cartItem);
    }

    private updateStorage() {
        this.localDataService.set(this.cartKey, this._items);
        this.localDataService.set(this.promoKey, this.promo);
    }

    private round(value: number): number {
        return Math.round(value * 100) / 100;
    }

    private toDays(dateStart: Date, dateEnd: Date): number {
        return Math.ceil((dateEnd.getTime() - dateStart.getTime()) / (1000 * 60 * 60 * 24));
    }

    private formatDate(date: Date): string {
        return new DatePipe('en-US').transform(date, 'shortDate') || '';
    }
}