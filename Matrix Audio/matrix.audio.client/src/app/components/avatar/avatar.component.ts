import { Component, Input, OnInit } from '@angular/core';
import { User } from '../../models/user';
import { AppSettingService } from '../../services/appsetting.service';
import { ConfigNames } from '../../models/config-names';
import { UserService } from '../../services/user.service';
import { SelectableImage } from '../../models/selectable';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { ImageService } from '../../services/image.service';

@Component({
    selector: 'mtx-avatar',
    templateUrl: 'avatar.component.html',
    imports: [CommonModule, FormsModule]
})
export class AvatarComponent implements OnInit {
    @Input() user: User | undefined = undefined;

    avatar: string | undefined = '';

    images: SelectableImage[] = [];
    selectedImage: SelectableImage | undefined = undefined;
    private logger: ILogger;

    constructor(
        private readonly appConfigService: AppSettingService,
        private readonly userService: UserService,
        private readonly imageService: ImageService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AvatarComponent');
    }

    async ngOnInit() {
        this.user = await this.userService.getCurrentUser();

        this.avatar = this.imageService.getAvatarUrl(this.user?.imageId!) ||
            this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar);
    }

    async showAvatarSelector() {
        this.images = [];
        let images = await this.userService.getAvatars();
        for (let image of images) {
            this.images.push({
                url: this.imageService.getAvatarUrl(image),
                name: image,
                selected: this.user?.imageId === image
            });
        }
    }

    selectAvatar(image: SelectableImage) {
        this.selectedImage = image;
        this.images.forEach(i => i.selected = i.url === image.url);
    }

    async saveAvatar() {
        if (this.user && this.selectedImage) {
            this.user.imageId = this.avatar;
            this.logger.info('Selected avatar: ', this.selectedImage);
            let result = await this.userService.updateAvatar(this.selectedImage.name);

            this.logger.info('Update avatar result: ', result);
            if (result.success) {
                this.avatar = this.imageService.getAvatarUrl(this.selectedImage.name);
            }
        }
    }
}