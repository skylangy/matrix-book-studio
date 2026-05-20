import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { AlbumService } from '../../../services/album.service';
import { Album } from '../../../models/album';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { AdminPagerComponent } from '../common/pager.component';
import { LoadingComponent } from '../../loading/loading.component';
import { Router, RouterModule } from '@angular/router';
import { PromptService } from '../../../services/prompt.service';

@Component({
    selector: 'mtx-admin-albums',
    templateUrl: 'admin.albums.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, AdminPagerComponent, LoadingComponent, RouterModule, CommonModule, FormsModule]
})
export class AdminAlbumsComponent extends ManageViewBase<Album> {
    private readonly logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly albumService: AlbumService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        super();
        this.logger = loggingService.getLogger('AdminAlbumsComponent');
        this.icon = 'book';
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
                name: 'Scan Metadata',
                icon: 'folder-plus',
                action: async () => {
                    this.promptService.showSuccess('Scan Album', 'Start scanning album metadata');
                    let result = await this.albumService.scanAlbumMetadata();
                    this.promptService.showSuccess('Scan Album', result.message || 'Scan Album Metadata Success');
                }
            },
            {
                name: 'Repair Splash',
                icon: 'image',
                action: async () => {
                    await this.albumService.fixAlbumSplash();
                }
            }
        ];
    }


    override async loadItems(): Promise<Album[]> {
        this.isLoading = true;
        let albums = await this.albumService.getAlbums(this.page, this.pageSize);
        this.isLoading = false;
        return albums;
    }

    override async search() {
        this.isLoading = true;
        this.page = 1;
        this.items = await this.albumService.search(this.searchText, this.page, this.pageSize);
        this.isLoading = false;
    }
}