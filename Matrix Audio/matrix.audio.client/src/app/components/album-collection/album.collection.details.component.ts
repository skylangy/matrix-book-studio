import { Component, Input, OnInit } from '@angular/core';
import { AlbumCollection } from '../../models/album';
import { ActivatedRoute, Router } from '@angular/router';
import { CardLayout, LoadingStyles } from '../../models/views';
import { AlbumService } from '../../services/album.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { CommonModule } from '@angular/common';
import { AlbumComponent } from '../album/album.component';

@Component({
    selector: 'mtx-album-collection-details',
    templateUrl: 'album.collection.details.component.html',
    imports: [CommonModule, AlbumComponent]
})
export class AlbumCollectionDetailsComponent implements OnInit {
    @Input() albumCollection?: AlbumCollection;
    isLoading = false;
    loadingStyle: LoadingStyles = 'book';
    cardLayout: CardLayout = 'horizontal';
    private logger: ILogger

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly albumService: AlbumService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('AlbumCollectionDetailsComponent');
    }

    ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            let id = params['id'];
            if (!id)
                return;
            this.isLoading = true;
            this.albumCollection = await this.albumService.getAlbumCollection(id)

            this.logger.info('Album collection loaded: ', this.albumCollection);
            this.isLoading = false;
        });
    }
}