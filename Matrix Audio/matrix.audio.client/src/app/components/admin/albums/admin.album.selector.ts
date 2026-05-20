import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AlbumService } from '../../../services/album.service';
import { ManageViewBase } from '../common/manageviewbase';
import { Album } from '../../../models/album';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { AdminPagerComponent } from '../common/pager.component';
import { LoadingComponent } from '../../loading/loading.component';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ILogger } from '../../../models/logger';
import { LoggingService } from '../../../services/logging.service';

@Component({
    selector: 'mtx-admin-album-selector',
    templateUrl: 'admin.album.selector.html',
    imports: [AdminContainerComponent, AdminPagerComponent,
        LoadingComponent, RouterModule, CommonModule, FormsModule]
})
export class AdminAlbumSelectorComponent extends ManageViewBase<Album> {
    @Output() albumSelected = new EventEmitter<Album[]>();
    selectedAlbums: Album[] = [];
    private logger: ILogger;

    constructor(
        private readonly albumService: AlbumService,
        loggingService: LoggingService) {
        super();
        this.icon = 'book';
        this.pageSize = 12;
        this.logger = loggingService.getLogger('AdminAlbumSelectorComponent');
    }

    override async loadItems(): Promise<Album[]> {
        this.isLoading = true;
        let albums = await this.albumService.getAlbums(this.page, this.pageSize);
        this.isLoading = false;
        return albums;
    }

    override async search() {
        this.isLoading = true;
        this.items = await this.albumService.search(this.searchText, this.page, this.pageSize);
        this.logger.info('Search result: ', this.items);
        this.isLoading = false;
    }

    select(album: Album) {
        if (album) {
            album.isSelected = !album.isSelected;

            if (album.isSelected) {
                this.selectedAlbums.push(album);
            } else {
                this.selectedAlbums = this.selectedAlbums.filter(a => a.id !== album.id);
            }
            this.albumSelected.emit(this.selectedAlbums);
        }
    }

    selectAll() {
        this.items.forEach(a => {
            a.isSelected = true;
            this.selectedAlbums.push(a);
        });
        this.albumSelected.emit(this.selectedAlbums);
    }
}