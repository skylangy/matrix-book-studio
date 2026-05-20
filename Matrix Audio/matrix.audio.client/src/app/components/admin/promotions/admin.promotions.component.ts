import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ILogger } from '../../../models/logger';
import { Promo } from '../../../models/promo';
import { LoggingService } from '../../../services/logging.service';
import { PromotionService } from '../../../services/promo.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { ManageViewBase } from '../common/manageviewbase';
import { AdminPagerComponent } from '../common/pager.component';

@Component({
    selector: 'mtx-admin-promotions',
    templateUrl: 'admin.promotions.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})
export class AdminPromotionsComponent extends ManageViewBase<Promo> {
    private readonly logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly promoService: PromotionService,
        loggingService: LoggingService) {
        super();
        this.useSearch = false;
        this.icon = 'cup-hot';
        this.logger = loggingService.getLogger('AdminPromotionsComponent');
    }

    override async ngOnInit() {
        super.ngOnInit();
        this.actions = [
            {
                name: 'New',
                icon: 'file-plus',
                action: () => {
                    this.router.navigate(['/control-tower/new/promotion']);
                }
            }
        ];
    }

    override async loadItems(): Promise<Promo[]> {
        return this.promoService.getPromos();
    }

    async deleteItem(item: Promo) {
        await this.promoService.deletePromo(item.id);
        await this.reload();
    }
}