import { Component } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { Faq } from '../../../models/faq';
import { LoggingService } from '../../../services/logging.service';
import { FaqService } from '../../../services/faq.service';
import { AdminPagerComponent } from '../common/pager.component';
import { Router, RouterModule } from '@angular/router';
import { ModalService } from '../../../services/modal.service';
import { PromptService } from '../../../services/prompt.service';

@Component({
    selector: 'mtx-admin-faqs',
    templateUrl: 'admin.faqs.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent,
        AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})

export class AdminFaqsComponent extends ManageViewBase<Faq> {
    constructor(
        private readonly router: Router,
        private readonly faqService: FaqService,
        private readonly modalService: ModalService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'person-raised-hand';
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.actions = [
            {
                name: 'Refresh',
                icon: 'arrow-clockwise',
                action: async () => {
                    await this.reload();
                }
            },
            {
                name: 'Add FAQ',
                icon: 'plus-square',
                action: async () => {
                    this.router.navigate(['/control-tower/new/faq']);
                }
            }];
    }

    override async loadItems(): Promise<Faq[]> {
        return this.faqService.getFaqsForAdmin(this.page, this.pageSize);
    }

    override async search() {
        this.page = 1;
        this.items = await this.faqService.search(this.searchText, this.page, this.pageSize);
    }

    async delete(faq: Faq) {
        if (faq) {
            this.modalService.openModal({
                title: 'Delete FAQ ',
                body: `Are you sure you want to delete this FAQ '${faq.question}'?`,
                buttons: [
                    {
                        label: 'Yes',
                        style: 'primary',
                        action: async () => {
                            await this.faqService.delete(faq.id);
                            await this.reload();
                            this.promptService.showSuccess('FAQ Deleted', 'The FAQ has been deleted.');
                            return true;
                        }
                    },
                    {
                        label: 'No',
                        style: 'secondary',
                        action: () => {
                            return true;
                        }
                    }
                ]
            });
        }
    }
}