import { Component } from '@angular/core';
import { Artist } from '../../../models/artist';
import { ILogger } from '../../../models/logger';
import { LoggingService } from '../../../services/logging.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ManageViewBase } from '../common/manageviewbase';
import { AdminPagerComponent } from '../common/pager.component';
import { LoadingComponent } from '../../loading/loading.component';
import { RouterModule } from '@angular/router';
import { PersonAvatarPipe } from '../../../pipes/person.avatar.pipe';
import { ArtistService } from '../../../services/artist.service';
import { PromptService } from '../../../services/prompt.service';
import { ImageService } from '../../../services/image.service';

@Component({
    selector: 'mtx-admin-artists',
    templateUrl: 'admin.artists.component.html',
    imports: [HeaderContentComponent, AdminContainerComponent, LoadingComponent,
        PersonAvatarPipe,
        RouterModule, CommonModule, FormsModule, AdminPagerComponent]
})
export class AdminArtistsComponent extends ManageViewBase<Artist> {
    private readonly logger: ILogger;

    constructor(
        private readonly artistService: ArtistService,
        private readonly promptService: PromptService,
        private readonly imageService: ImageService,
        loggingService: LoggingService
    ) {
        super();
        this.logger = loggingService.getLogger('AdminArtistsComponent');
        this.icon = 'people';
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
                    this.promptService.showSuccess('Scan Artist', 'Start scanning artist metadata');
                    let result = await this.artistService.scanArtistMetadata();
                    this.promptService.showSuccess('Scan Artist', result.message || 'Scan Artist Metadata Success');
                }
            },
            {
                name: 'Repair Splash',
                icon: 'image',
                action: async () => {
                    await this.artistService.fixArtistAvatar();
                }
            }
        ];
    }

    async loadItems(): Promise<Artist[]> {
        return await this.artistService.getAllArtists(this.page, this.pageSize);
    }

    override  async search() {
        this.page = 1;
        this.items = await this.artistService.searchArtists(this.searchText, this.page, this.pageSize);
    }

    async delete(item: Artist) {
        if (item) {
            await this.artistService.deleteArtist(item.id!);
            this.items = await this.loadItems();
        }
    }

    onImgError(event: Event) {
        const img = event.target as HTMLImageElement;
        img.src = this.imageService.defaultArtistAvatarUrl;
    }
}