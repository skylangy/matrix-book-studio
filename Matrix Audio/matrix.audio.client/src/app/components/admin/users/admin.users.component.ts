import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { User } from '../../../models/user';
import { UserService } from '../../../services/user.service';
import { AdminPagerComponent } from '../common/pager.component';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-admin-users',
    templateUrl: 'admin.users.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})

export class AdminUsersComponent extends ManageViewBase<User> {
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
        return this.userService.getUsers(this.page, this.pageSize);
    }

    override async search() {
        this.page = 1;
        this.items = await this.userService.searchUsers(this.searchText, this.page, this.pageSize);
    }
}