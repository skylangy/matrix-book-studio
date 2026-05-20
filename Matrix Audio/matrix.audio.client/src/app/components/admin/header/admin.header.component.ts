import { Component, OnInit } from '@angular/core';
import { NavigationProviderService } from '../../../services/navigation.service';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavigationModel } from '../../../models/navigation';
import { ImageService } from '../../../services/image.service';
import { AppSettingService } from '../../../services/appsetting.service';
import { UserService } from '../../../services/user.service';
import { ConfigNames } from '../../../models/config-names';

@Component({
    selector: 'mtx-admin-header',
    templateUrl: 'admin.header.component.html',
    imports: [RouterModule, CommonModule, FormsModule]
})

export class AdminHeaderComponent implements OnInit {
    searchText = '';
    avatar: string | undefined = '';
    userName: string | undefined = '';

    constructor(
        private readonly appConfigService: AppSettingService,
        private readonly navigationProviderService: NavigationProviderService,
        private readonly userService: UserService,
        private readonly imageService: ImageService,) { }

    async ngOnInit() {
        let user = await this.userService.getCurrentUser();

        this.avatar = this.imageService.getAvatarUrl(user?.imageId!) ||
            this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar);
        this.userName = user?.name;
    }

    get navigationItems() {
        return this.navigationProviderService.getAdminHeaderNavigation();
    }

    get accountNavigationItems() {
        return this.navigationProviderService.getAdminAccountNavigation();
    }

    search() {

    }
}