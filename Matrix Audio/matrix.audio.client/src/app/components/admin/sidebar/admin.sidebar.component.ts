import { Component, OnInit } from '@angular/core';
import { SettingNames } from '../../../models/app-setting';
import { AppSettingService } from '../../../services/appsetting.service';
import { AuthService } from '../../../services/auth.service';
import { NavigationProviderService } from '../../../services/navigation.service';
import { SidebarNavComponent } from '../../sidebar/sidebar.nav.component';

@Component({
    selector: 'mtx-admin-sidebar',
    templateUrl: 'admin.sidebar.component.html',
    imports: [SidebarNavComponent]
})
export class AdminSidebarComponent implements OnInit {
    logoUrl = '';
    appName = '';

    constructor(
        private readonly appSettingService: AppSettingService,
        private readonly authService: AuthService,
        private readonly navProviderService: NavigationProviderService) { }

    async ngOnInit() {
        this.appName = this.appSettingService.getConfig(SettingNames.AppName, '')!;
        this.logoUrl = this.appSettingService.getConfig(SettingNames.AppLogo, '')!;
    }

    get adminNavigationItems() {
        return this.navProviderService.getAdminNavigation();
    }

    get adminSettingsNavigationItems() {
        return this.navProviderService.getAdminSettingNavigation();
    }
}