import { Component, OnInit } from '@angular/core';
import { Action } from '../../../models/action';
import { AlbumService } from '../../../services/album.service';
import { AppSettingService } from '../../../services/appsetting.service';
import { PromptService } from '../../../services/prompt.service';
import { UserService } from '../../../services/user.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';



@Component({
    selector: 'mtx-admin-settings',
    templateUrl: 'admin.settings.component.html',
    imports: [HeaderContentComponent]
})

export class AdminSettingsComponent implements OnInit {
    icon = 'gear';
    actions: Action[] = [
        {
            name: 'Update settings',
            description: 'Update settings from configuration',
            icon: 'cog',
            size: 'small',
            action: () => this.updateSettings(),
            isEnable: () => true
        },
        {
            name: 'Update subscriptions',
            description: 'Update subscription plans from configuration',
            icon: 'credit-card',
            size: 'small',
            action: () => this.updateSubscription(),
            isEnable: () => true
        },
        {
            name: 'Convert',
            description: 'Convert PNG to JPG',
            icon: 'save',
            size: 'small',
            action: () => this.convertPngToJpg(),
            isEnable: () => true
        }
    ];

    constructor(
        private readonly albumService: AlbumService,
        private readonly appSettingService: AppSettingService,
        private readonly userService: UserService,
        private readonly promptService: PromptService,
    ) { }

    ngOnInit() { }

    save() { }

    runAction(action: Action) {
        if (action && action.isEnable && action.isEnable()) {
            action.action?.();
        }
    }

    async updateSettings() {
        let result = await this.appSettingService.populateFromConfig();
        if (result.success) {
            this.promptService.showSuccess('Settings', 'Settings updated');
        } else {
            this.promptService.showError('Settings', 'Failed to update settings');
        }
    }

    async updateSubscription() {
        let result = await this.userService.populateSubscriptionFromConfig();
        if (result.success) {
            this.promptService.showSuccess('Subscription', 'Subscription updated');
        } else {
            this.promptService.showError('Subscription', 'Failed to update subscription');
        }
    }

    async convertPngToJpg() {
        await this.albumService.convertPngToJpg();
    }
}