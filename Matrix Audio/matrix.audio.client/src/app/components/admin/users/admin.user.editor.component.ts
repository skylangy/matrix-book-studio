import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { CommonModule } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { ValueService } from '../../../services/value.service';
import { User } from '../../../models/user';
import { UserService } from '../../../services/user.service';
import { ImageService } from '../../../services/image.service';
import { AppSettingService } from '../../../services/appsetting.service';
import { ConfigNames } from '../../../models/config-names';
import { AdminUserSubscriptionComponent } from './user.subscription.component';

@Component({
    selector: 'mtx-admin-user-editor',
    templateUrl: 'admin.user.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent, AdminUserSubscriptionComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminUserEditorComponent extends EditorBase<User> {
    private logger: ILogger;
    avatar = '';

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly userService: UserService,
        private readonly valueService: ValueService,
        private readonly imageService: ImageService,
        private readonly appConfigService: AppSettingService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'person';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.userService.getUser(id);
            }

            this.avatar = this.imageService.getAvatarUrl(this.model?.imageId!) ||
                this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar) || '';

            this.dataForm.patchValue(this.model!);
            this.logger.info('User loaded', this.model);
        });
    }

    override createForm() {
        return new FormGroup({
            firstName: new FormControl('', [Validators.required]),
            lastName: new FormControl('', [Validators.required]),
            email: new FormControl('', [Validators.required]),
            name: new FormControl('', [Validators.required]),
            bio: new FormControl('', [Validators.required]),
        });
    }

    override  async onSubmit() {

    }

    override back(): void {
        this.router.navigate(['/control-tower/users']);
    }
}