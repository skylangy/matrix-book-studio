import { Component, Input, OnInit, signal } from '@angular/core';
import { AlbumCollection } from '../../models/album';
import { AlbumService } from '../../services/album.service';
import { LoadingComponent } from '../loading/loading.component';
import { AlbumCollectionComponent } from './album.collection.component';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { AlbumCollectionSource, AlbumLayout, CardLayout } from '../../models/views';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '../../services/translate.service';

@Component({
    selector: 'mtx-album-collections',
    templateUrl: 'album.Collections.component.html',
    imports: [LoadingComponent, AlbumCollectionComponent]
})
export class AlbumCollectionsComponent implements OnInit {
    @Input() albumCollections?: AlbumCollection[] = [];
    @Input() layout: AlbumLayout = 'list';
    @Input() cardLayout: CardLayout = 'card';
    @Input() sourceType: AlbumCollectionSource = 'recents';
    @Input() loadNextPage = signal(false);
    @Input() emptyMessage = 'No album collection found';
    @Input() indexToNotify = -4;
    @Input() title = '';
    @Input() pageSize = 18;

    currentPage = 1;
    isLoading = signal(false);
    retrievers = {
        '': (albumService: AlbumService) => { return []; },
        'recents': (albumService: AlbumService) => { return albumService.getAlbumCollections(this.currentPage, this.pageSize); },

    };
    private logger: ILogger;

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly albumService: AlbumService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AlbumCollectionsComponent');
    }

    async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            this.isLoading.set(true);
            this.albumCollections = [];
            this.pageSize = params['count'] ?? this.pageSize;
            this.sourceType = params['source'] as AlbumCollectionSource ?? this.sourceType;
            this.layout = params['layout'] as AlbumLayout ?? this.layout;
            this.cardLayout = params['cardlayout'] as CardLayout ?? this.cardLayout;
            this.title = params['title'] ?? this.title;
            this.loadNextPage.set(params['paging'] === 'y' ? true : this.loadNextPage());

            this.title = this.translateService.translate(this.title);

            try {
                let retriever = this.retrievers[this.sourceType];
                if (!retriever) {
                    this.logger.error('No data retriever found:', this.sourceType);
                    this.albumCollections = [];
                } else {
                    this.albumCollections = await retriever(this.albumService);
                }

                this.logger.info('Loaded album collections:', this.sourceType, this.albumCollections);
            } catch (error) {
                this.logger.error('Error loading album collections:', error);
                this.albumCollections = [];
            } finally {
                this.isLoading.set(false);
            }
        });

    }

    get hasAlbumCollections(): boolean {
        return this.albumCollections !== undefined && this.albumCollections.length > 0;
    }
}