import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ILogger } from '../../../models/logger';
import { NameValue } from '../../../models/namevalue';
import { Promo } from '../../../models/promo';
import { DiscountTypes } from '../../../models/subscription-plan';
import { LoggingService } from '../../../services/logging.service';
import { PromotionService } from '../../../services/promo.service';
import { PromptService } from '../../../services/prompt.service';
import { ValueService } from '../../../services/value.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { EditorBase } from '../common/editor.base';

@Component({
    selector: 'mtx-admin-promotion-editor',
    templateUrl: 'admin.promotion.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminPromotionEditorComponent extends EditorBase<Promo> {
    discountTypes: NameValue[] = [
        { name: 'Percentage', value: DiscountTypes.Percentage },
        { name: 'Fixed', value: DiscountTypes.Fixed },
    ];
    private logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly promoService: PromotionService,
        private readonly promptService: PromptService,
        private readonly valueService: ValueService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'person-raised-hand';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.promoService.getPromo(id);
                this.model.validFrom = this.valueService.formatDate(new Date(this.model.validFrom));
                this.model.validTo = this.valueService.formatDate(new Date(this.model.validTo));
            } else {
                this.model = this.createPromo();
            }

            this.dataForm.patchValue(this.model);
            this.logger.info('Faq loaded', this.model);
        });
    }

    override createForm() {
        return new FormGroup({
            code: new FormControl('', [Validators.required]),
            min: new FormControl('', [Validators.required]),
            discount: new FormControl('', [Validators.required]),
            discountType: new FormControl('', [Validators.required]),
            isActive: new FormControl('', [Validators.required]),
            validFrom: new FormControl(new Date(), [Validators.required]),
            validTo: new FormControl(new Date(), [Validators.required])
        });
    }

    override  async onSubmit() {
        if (this.dataForm.invalid) {
            this.logger.warn('Form is invalid', this.dataForm.errors);
            return;
        }

        if (this.model) {
            this.logger.info('Form submitted', this.dataForm.value, this.model);
            const promo = this.dataForm.value as Promo;
            promo.id = this.model.id;
            promo.dateCreated = this.model.dateCreated;
            promo.validFrom = this.valueService.formatDate(new Date(promo.validFrom));
            promo.validTo = this.valueService.formatDate(new Date(promo.validTo));
            this.logger.info('Submitting promo', promo);
            await this.promoService.updatePromo(promo);

            this.promptService.showSuccess('Success', `Promo '${promo.code}' is updated successfully`);
            this.logger.info('Submitting promo', promo);
        }
    }

    override back(): void {
        this.router.navigate(['/control-tower/promotions']);
    }

    private createPromo(): Promo {
        const now = new Date();
        const nowString = this.valueService.formatDate(now);
        const toString = this.valueService.formatDate(new Date(now.getTime() + 1000 * 60 * 60 * 24 * 30));
        return {
            id: Math.random().toString(36).substring(7),
            code: '',
            discount: 0,
            min: 0,
            discountType: DiscountTypes.Percentage,
            isActive: true,
            validFrom: nowString,
            validTo: toString,
            dateCreated: now
        };
    }
}