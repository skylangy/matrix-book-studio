import { Component } from '@angular/core';
import { Post } from '../../../models/post';
import { LoggingService } from '../../../services/logging.service';
import { PostService } from '../../../services/post.service';
import { ManageViewBase } from '../common/manageviewbase';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminPagerComponent } from '../common/pager.component';
import { ILogger } from '../../../models/logger';
import { Router, RouterModule } from '@angular/router';
import { ModalService } from '../../../services/modal.service';

@Component({
    selector: 'mtx-admin-posts',
    templateUrl: 'admin.posts.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent,
        AdminPagerComponent,
        CommonModule, FormsModule, RouterModule]
})
export class AdminPostsComponent extends ManageViewBase<Post> {
    private readonly logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly postService: PostService,
        private readonly modalService: ModalService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('AdminPostsComponent');
        this.icon = 'file';
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
            },
            {
                name: 'New Post',
                icon: 'file-plus',
                action: () => {
                    this.router.navigate(['/control-tower/new/post']);
                }
            }
        ];
    }

    override async loadItems(): Promise<Post[]> {
        return await this.postService.getAllPosts(this.page, this.pageSize);
    }

    override async search() {
        this.page = 1;
        this.logger.info(`Searching for posts with term: ${this.searchText}`);
        this.items = await this.postService.search(this.searchText, this.page, this.pageSize);
    }

    edit(id?: string) {
        if (id) {
            this.router.navigate(['/control-tower/edit/post', id]);
        }
    }

    async delete(post: Post) {
        if (post) {
            this.modalService.openModal({
                title: 'Delete Post ',
                body: `Are you sure you want to delete this post '${post.title}'?`,
                buttons: [
                    {
                        label: 'Yes',
                        style: 'primary',
                        action: async () => {
                            await this.postService.deletePost(post?.id!);
                            await this.reload();
                            return true;
                        }
                    },
                    {
                        label: 'No',
                        style: 'secondary',
                        action: () => {
                            return true;
                        }
                    }
                ]
            });
        }
    }
}