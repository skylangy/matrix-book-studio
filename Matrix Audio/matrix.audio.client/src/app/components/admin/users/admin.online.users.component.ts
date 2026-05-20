import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { AdminPagerComponent } from '../common/pager.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ManageViewBase } from '../common/manageviewbase';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'mtx-admin-online-users',
    templateUrl: 'admin.online.users.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent,
        CommonModule, RouterModule, FormsModule]
})
export class AdminOnlineUsersComponent extends ManageViewBase<User> {
    constructor(private readonly userService: UserService) {
        super();
        this.icon = 'person';
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.actions = [
            {
                name: 'Refresh',
                icon: 'arrow-clockwise',
                action: async () => {
                    await this.reload();
                }
            }
        ];
    }

    override async loadItems(): Promise<User[]> {
        return this.userService.getOnlineUsers(this.page, this.pageSize);
    }
}