import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavigationProviderService } from '../../services/navigation.service';
import { SidebarNavComponent } from './sidebar.nav.component';
import { AppSettingService } from '../../services/appsetting.service';
import { SettingNames } from '../../models/app-setting';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-sidebar',
    templateUrl: './sidebar.component.html',
    imports: [RouterModule, SidebarNavComponent, TranslatePipe]
})
export class SidebarComponent implements OnInit {
    appName = '';
    logoUrl = '';

    constructor(
        private readonly appSettingService: AppSettingService,
        private readonly authService: AuthService,
        private readonly navProviderService: NavigationProviderService
    ) { }

    async ngOnInit(): Promise<void> {
        this.appName = this.appSettingService.getConfig(SettingNames.AppName, '')!;
        this.logoUrl = this.appSettingService.getConfig(SettingNames.AppLogo, '')!;
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get accountNavigationItems() {
        return this.navProviderService.getAccountNavigation();
    }

    get logInOutNavigationItems() {
        return this.navProviderService.getLoginOutNavigation();
    }

    get appNavigationItems() {
        return this.navProviderService.getAppNavigation();
    }

    logout() {
        this.authService.logout();
    }
}
