import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { UserService } from '../../../services/user.service';
import { Message } from '../../../models/message';
import { AdminPagerComponent } from '../common/pager.component';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-admin-messages',
    templateUrl: 'admin.messages.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent, CommonModule, FormsModule, RouterModule]
})
export class AdminMessagesComponent extends ManageViewBase<Message> {
    constructor(private readonly userService: UserService) {
        super();
        this.icon = 'chat-dots';
    }

    override async ngOnInit() {
        super.ngOnInit();
    }

    override async loadItems(): Promise<Message[]> {
        return this.userService.getMessages(this.page, this.pageSize);
    }

    override async search() {
        this.page = 1;
        this.items = await this.userService.searchMessages(this.searchText, this.page, this.pageSize);
    }
}