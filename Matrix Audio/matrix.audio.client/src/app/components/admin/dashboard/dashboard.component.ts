import { Component, OnInit, ViewChild } from '@angular/core';
import { AdminFooterComponent } from "../footer/admin.footer.component";
import { AdminHeaderComponent } from '../header/admin.header.component';
import { AdminSidebarComponent } from '../sidebar/admin.sidebar.component';
import { RouterOutlet } from '@angular/router';
import { PromptComponent } from '../../prompt/prompt.component';
import { ModalComponent } from '../../modal/modal.component';
import { Subscription } from 'rxjs';
import { ModalService } from '../../../services/modal.service';

@Component({
    selector: 'mtx-admin-dashboard',
    templateUrl: 'dashboard.component.html',
    imports: [RouterOutlet, AdminFooterComponent, AdminHeaderComponent, AdminSidebarComponent,
        PromptComponent,
        ModalComponent]
})
export class DashboardComponent implements OnInit {
    @ViewChild('modalRef') modalRef!: ModalComponent;
    private modalSubscription?: Subscription;

    constructor(private modalService: ModalService) {

    }

    async ngOnInit() {
        this.modalSubscription = this.modalService.onShowModal.subscribe(modal => {
            this.modalRef.openModal(modal);
        });
    }

    ngOnDestroy(): void {
        if (this.modalSubscription) {
            this.modalSubscription.unsubscribe();
        }
    }
}