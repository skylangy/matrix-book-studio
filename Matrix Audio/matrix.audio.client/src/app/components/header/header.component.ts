import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NavigationProviderService } from '../../services/navigation.service';
import { AppSettingService } from '../../services/appsetting.service';
import { ConfigNames } from '../../models/config-names';
import { UserService } from '../../services/user.service';
import { ImageService } from '../../services/image.service';
import { CartService } from '../../services/cart.service';
import { SettingNames } from '../../models/app-setting';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-header',
    templateUrl: './header.component.html',
    imports: [RouterModule, FormsModule, TranslatePipe]
})
export class HeaderComponent implements OnInit, OnDestroy {
    logoUrl = '';
    searchText = '';
    avatar: string | undefined = '';
    userName: string | undefined = '';
    userLevel: number | undefined = 0;
    private subscription?: Subscription;

    constructor(
        private readonly router: Router,
        private readonly appSettingService: AppSettingService,
        private readonly authService: AuthService,
        private readonly navProviderService: NavigationProviderService,
        private readonly appConfigService: AppSettingService,
        private readonly userService: UserService,
        private readonly imageService: ImageService,
        private readonly cartService: CartService
    ) { }

    async ngOnInit() {
        this.logoUrl = this.appSettingService.getConfig(SettingNames.AppLogo, '')!;
        let user = await this.userService.getCurrentUser();

        this.avatar = this.imageService.getAvatarUrl(user?.imageId!) ||
            this.appConfigService.getConfig<string>(ConfigNames.DefaultAvatar, this.imageService.defaultAvatar);
        this.userName = user?.name;
        this.userLevel = user?.level;

        this.subscription = this.userService.avatarUpdated.subscribe((imageId: string) => {
            this.avatar = this.imageService.getAvatarUrl(imageId);
        });
    }

    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get accountNavigationItems() {
        return this.navProviderService.getAccountNavigation();
    }

    get headerNavigationItems() {
        return this.navProviderService.getHeaderNavigation();
    }

    get cartCount() {
        return this.cartService.numbers;
    }

    search() {
        this.router.navigate(['/public/albums/search/list/horizontal', this.searchText, 50, 1]);
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/public/welcome']);
    }
}
