import { Component, OnInit } from '@angular/core';
import { ManageViewBase } from '../common/manageviewbase';
import { AlbumCollection } from '../../../models/album';
import { ILogger } from '../../../models/logger';
import { AlbumService } from '../../../services/album.service';
import { PromptService } from '../../../services/prompt.service';
import { LoggingService } from '../../../services/logging.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { AdminPagerComponent } from '../common/pager.component';
import { LoadingComponent } from '../../loading/loading.component';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'mtx-admin-album.collections',
    templateUrl: 'admin.album.collections.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent, LoadingComponent, RouterModule, CommonModule, FormsModule]
})
export class AdminAlbumCollectionsComponent extends ManageViewBase<AlbumCollection> {
    private readonly logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly albumService: AlbumService,
        private readonly promptService: PromptService,
        loggingService: LoggingService) {
        super();

        this.logger = loggingService.getLogger('AdminAlbumsComponent');
        this.icon = 'bookmarks';
    }

    override async ngOnInit() {
        await super.ngOnInit();

        this.actions = [
            {
                name: 'New',
                icon: 'bookmark-plus',
                action: async () => {
                    await this.reload(); this.router.navigate(['/control-tower/new/album-collection']);
                }
            },
            {
                name: 'Refresh',
                icon: 'arrow-clockwise',
                action: async () => {
                    await this.reload();
                }
            }
        ];

        this.logger.info('AdminAlbumCollectionsComponent initialized', this.items);
    }

    override loadItems(): Promise<AlbumCollection[]> {
        return this.albumService.getAlbumCollectionsAdmin(this.page, this.pageSize);
    }
}