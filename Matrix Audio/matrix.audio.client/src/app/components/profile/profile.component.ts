import { Component, OnInit, signal, Signal } from '@angular/core';
import { User } from '../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { AppSettingService } from '../../services/appsetting.service';
import { AvatarComponent } from '../avatar/avatar.component';
import { TranslatePipe } from '../../pipes/translate.pipe';


@Component({
    selector: 'mtx-profile',
    templateUrl: './profile.component.html',
    imports: [CommonModule, FormsModule, AvatarComponent, TranslatePipe]
})
export class ProfileComponent implements OnInit {
    user: User | undefined = undefined
    defaultImage: string | undefined = '';

    firstName = signal('');
    lastName = signal('');
    displayName = signal('');
    email = signal('');
    bio = signal('');

    private logger: ILogger;

    constructor(
        private readonly userService: UserService,
        private readonly appConfigService: AppSettingService,
        readonly loggingService: LoggingService) {
        this.logger = loggingService.getLogger('ProfileComponent');
    }

    async ngOnInit() {
        this.defaultImage = this.appConfigService.getConfig<string>('DefaultAvatar', 'assets/images/defaultAvatar.png');
        this.user = await this.userService.getCurrentUser();

        this.logger.info('User', this.user);

        this.firstName.set(this.user?.firstName || '');
        this.lastName.set(this.user?.lastName || '');
        this.displayName.set(this.user?.name || '');
        this.email.set(this.user?.email || '');
        this.bio.set(this.user?.bio || '');
    }

    async save(): Promise<void> {
        this.user!.firstName = this.firstName();
        this.user!.lastName = this.lastName();
        this.user!.name = this.displayName();
        this.user!.bio = this.bio();
        this.user!.imageId = this.defaultImage;
        this.user!.level = 1000;

        this.logger.info('User', this.user);

        await this.userService.updateProfile(this.user);
    }
}
