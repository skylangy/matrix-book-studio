

import { Injectable } from '@angular/core';
import { NavigationModel } from '../models/navigation';
import { TranslateService } from './translate.service';

@Injectable({
    providedIn: 'root'
})
export class NavigationProviderService {

    constructor(private readonly translateService: TranslateService) { }

    getAccountNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Account'), route: '/public/account', icon: 'person-square', hasSeparator: false },
            { name: this.translate('Profile'), route: '/public/profile', icon: 'person-lines-fill', hasSeparator: false },
            { name: this.translate('Settings'), route: '/public/settings', icon: 'gear', hasSeparator: false },
            { name: this.translate('Sign Out'), route: '/logout', icon: 'door-closed', hasSeparator: true }
        ];
    }

    getLoginOutNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Sign In'), route: '/login', hasSeparator: false },
            { name: this.translate('Register'), route: '/register', hasSeparator: false }
        ];
    }

    getHeaderNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Blog'), route: '/public/posts', hasSeparator: false },
            { name: this.translate('Plans'), route: '/public/plans', hasSeparator: false },
            { name: this.translate('Contact'), route: '/public/contact', hasSeparator: false },
            { name: this.translate('About'), route: '/public/about', hasSeparator: false }
        ];
    }

    getAppNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Welcome'), route: '/public/welcome', icon: 'house', isActive: true, hasSeparator: false },
            { name: this.translate('Discover'), route: '/public/discover', icon: 'compass', isActive: false, hasSeparator: false },
            { name: this.translate('Artists'), route: '/public/artists/all/grid/medium/All Authors/1/21', icon: 'person', isActive: false, hasSeparator: false },
            { name: this.translate('Library'), route: '/public/library', icon: 'music-note-list', isActive: false, hasSeparator: false }
        ];
    }

    getAdminNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Overview'), route: '/control-tower/home', icon: 'house', isActive: true, hasSeparator: false },
            { name: this.translate('Albums'), route: '/control-tower/albums', icon: 'book', isActive: true, hasSeparator: false },
            { name: this.translate('Album Collections'), route: '/control-tower/album-collections', icon: 'bookmarks', isActive: true, hasSeparator: false },
            { name: this.translate('Artist'), route: '/control-tower/artists', icon: 'people', isActive: true, hasSeparator: false },
            { name: this.translate('Posts'), route: '/control-tower/posts', icon: 'file', isActive: true, hasSeparator: false },
            { name: this.translate('FAQ'), route: '/control-tower/faqs', icon: 'person-raised-hand', isActive: true, hasSeparator: false },
            { name: this.translate('Messages'), route: '/control-tower/messages', icon: 'chat-dots', isActive: true, hasSeparator: false },

            { name: this.translate('Orders'), route: '/control-tower/orders', icon: 'cart', isActive: true, hasSeparator: true },
            { name: this.translate('Subscription'), route: '/control-tower/subscriptions', icon: 'calendar-heart', isActive: true, hasSeparator: false },
            { name: this.translate('Promotions'), route: '/control-tower/promotions', icon: 'cup-hot', isActive: true, hasSeparator: false },
            { name: this.translate('Users'), route: '/control-tower/users', icon: 'person', isActive: true, hasSeparator: false },
        ];
    }

    getAdminSettingNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Site Setting'), route: '/control-tower/settings', icon: 'gear', isActive: true, hasSeparator: false },
            // { name: this.translate('Maintenance'), route: '/control-tower/maintenance', icon: 'wrench', isActive: true, hasSeparator: false },
        ];
    }

    getAdminHeaderNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Home'), route: '/', hasSeparator: false },
        ];
    }

    getAdminAccountNavigation(): NavigationModel[] {
        return [
            { name: this.translate('Sign Out'), route: '/control-tower/logout', hasSeparator: false }
        ];
    }

    private translate(name: string): string {
        return this.translateService.translate(name);
    }
}