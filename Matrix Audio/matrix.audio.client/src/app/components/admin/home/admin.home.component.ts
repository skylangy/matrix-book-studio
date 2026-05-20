import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { InfoCardComponent } from '../info-card/info.card.component';
import { AppSettingService } from '../../../services/appsetting.service';


export interface InfoCard {
    title: string;
    info: string;
    icon: string;
    link: string;
}

@Component({
    selector: 'mtx-admin-home',
    templateUrl: 'admin.home.component.html',
    imports: [HeaderContentComponent, InfoCardComponent]
})
export class AdminHomeComponent implements OnInit {
    infoList: InfoCard[] = [];
    icon = 'house';

    constructor(private readonly appSettingService: AppSettingService) { }

    async ngOnInit() {
        let summary = await this.appSettingService.getSummary();
        this.infoList = [
            { title: 'Albums', info: `${summary.albumCount}`, icon: 'book', link: '/control-tower/albums' },
            { title: 'Artists', info: `${summary.authorCount}`, icon: 'people', link: '/control-tower/artists' },
            { title: 'Posts', info: `${summary.postCount}`, icon: 'file', link: '/control-tower/posts' },
            { title: 'FAQ', info: `${summary.faqCount}`, icon: 'person-raised-hand', link: '/control-tower/faqs' },
            { title: 'Messages', info: `${summary.userMessageCount}`, icon: 'chat-dots', link: '/control-tower/messages' },
            { title: 'Orders', info: `${summary.orderCount}`, icon: 'cart', link: '/control-tower/cart' },
            { title: 'Promotions', info: `${summary.promoCount}`, icon: 'cup-hot', link: '/control-tower/promotions' },
            { title: 'Subscriptions', info: `${summary.subscriptionCount}`, icon: 'calendar-heart', link: '/control-tower/subscriptions' },
            { title: 'Users', info: `${summary.userCount}`, icon: 'person', link: '/control-tower/users' },
            { title: 'Online Users', info: `${summary.onlineUserCount}`, icon: 'person', link: '/control-tower/online-users' },
        ]
    }
}