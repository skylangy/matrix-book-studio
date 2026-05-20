import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { CommonModule } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { Order } from '../../../models/order';
import { OrderService } from '../../../services/order.service';
import { User } from '../../../models/user';
import { UserService } from '../../../services/user.service';
import { AppSettingService } from '../../../services/appsetting.service';
import { ImageService } from '../../../services/image.service';
import { ConfigNames } from '../../../models/config-names';

@Component({
    selector: 'mtx-admin-order-editor',
    templateUrl: 'admin.order.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminOrderEditorComponent extends EditorBase<Order> {
    private logger: ILogger;
    user: User | undefined = undefined;
    avatar = '';

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly orderService: OrderService,
        private readonly userService: UserService,
        private readonly imageService: ImageService,
        private readonly appConfigService: AppSettingService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'cart';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.orderService.getOrder(id);

                if (this.model) {
                    this.user = await this.userService.getUser(this.model.userId!);
                    this.dataForm.patchValue(this.model!);
                }

                this.avatar = this.imageService.getAvatarUrl(this.user?.imageId!) ||
                    this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar) || '';

            }
            this.logger.info('Order loaded', this.model);
        });
    }

    get canEdit(): boolean {
        return this.model != undefined && this.model.orderStatus !== 'Completed';
    }

    override createForm() {
        return new FormGroup({

        });
    }

    override  async onSubmit() {

    }

    override back() {
        this.router.navigate(['/control-tower/orders']);
    }
}