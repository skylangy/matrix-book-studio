import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { CommonModule } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { Message } from '../../../models/message';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user';
import { ImageService } from '../../../services/image.service';
import { AppSettingService } from '../../../services/appsetting.service';
import { ConfigNames } from '../../../models/config-names';

@Component({
    selector: 'mtx-admin-message-view',
    templateUrl: 'admin.message.view.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminMessageViewComponent extends EditorBase<Message> {
    private logger: ILogger;
    user: User | undefined = undefined;
    avatar = '';

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly userService: UserService,
        private readonly imageService: ImageService,
        private readonly appConfigService: AppSettingService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'chat-dots';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.userService.getMessage(id);

                this.dataForm.patchValue(this.model);

                this.user = await this.userService.getUser(this.model.userId);

                this.avatar = this.imageService.getAvatarUrl(this.user?.imageId!) ||
                    this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar) || '';
            }
            this.logger.info('Artist loaded', this.model, this.user);
        });
    }

    override createForm() {
        return new FormGroup({
            subject: new FormControl('', [Validators.required]),
            content: new FormControl('', [Validators.required]),
        });
    }

    override  async onSubmit() {

    }

    override back() {
        this.router.navigate(['/control-tower/messages']);
    }
}