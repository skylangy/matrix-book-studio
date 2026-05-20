import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ILogger } from '../../../models/logger';
import { NameValue } from '../../../models/namevalue';
import { DiscountTypes, SubscriptionPeriod, SubscriptionPlan, SubscriptionTier } from '../../../models/subscription-plan';
import { LoggingService } from '../../../services/logging.service';
import { UserService } from '../../../services/user.service';
import { ValueService } from '../../../services/value.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { EditorBase } from '../common/editor.base';

@Component({
    selector: 'mtx-admin-subscription-editor',
    templateUrl: 'admin.subscription.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminSubscriptionEditorComponent extends EditorBase<SubscriptionPlan> {
    private logger: ILogger;
    tiers: NameValue[] = [
        { name: 'Free', value: SubscriptionTier.Free },
        { name: 'Silver', value: SubscriptionTier.Silver },
        { name: 'Gold', value: SubscriptionTier.Gold },
    ];

    periods: NameValue[] = [
        { name: 'Monthly', value: SubscriptionPeriod.Monthly },
        { name: 'Semiannual', value: SubscriptionPeriod.Semiannual },
        { name: 'Annual', value: SubscriptionPeriod.Annual },
    ];

    discountTypes: NameValue[] = [
        { name: 'Percentage', value: DiscountTypes.Percentage },
        { name: 'Fixed', value: DiscountTypes.Fixed },
    ];

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly userService: UserService,
        private readonly valueService: ValueService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'calendar-heart';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.userService.getSubscription(id);
                this.model.startDate = this.valueService.formatDate(new Date(this.model.startDate));
                this.model.endDate = this.valueService.formatDate(new Date(this.model.endDate));
            } else {
                this.model = this.newSubscription();
            }
            this.dataForm.patchValue(this.model);
            this.logger.info('Artist loaded', this.model);
        });
    }

    override createForm() {
        return new FormGroup({
            name: new FormControl('', [Validators.required]),
            description: new FormControl('', [Validators.required]),
            tier: new FormControl('', [Validators.required]),
            period: new FormControl('', [Validators.required]),
            startDate: new FormControl('', [Validators.required]),
            endDate: new FormControl('', [Validators.required]),
            monthlyRate: new FormControl('', [Validators.required]),
            discountType: new FormControl('', [Validators.required]),
            discountRate: new FormControl('', [Validators.required]),
            discount: new FormControl('', [Validators.required]),
            total: new FormControl('', [Validators.required]),
            totalAfterDiscount: new FormControl('', [Validators.required]),
        });
    }

    override  async onSubmit() {

    }

    override back() {
        this.router.navigate(['/control-tower/subscriptions']);
    }

    private newSubscription(): SubscriptionPlan {
        const now = new Date();
        const nowString = this.valueService.formatDate(now);
        return {
            id: Math.random().toString(36).substring(7),
            name: '',
            description: '',
            currency: 'USD',
            tier: SubscriptionTier.Free,
            period: SubscriptionPeriod.Monthly,
            startDate: nowString,
            endDate: nowString,
            dateCreated: now,
            dateUpdated: now,
            monthlyRate: 0,
            discountType: DiscountTypes.Percentage,
            discountRate: 0,
            discount: 0,
            total: 0,
            totalAfterDiscount: 0,
            level: 0,
            isActive: true,
            features: [],
        };
    }
}