import { Component, Input, OnInit, signal } from '@angular/core';
import { AlbumComponent } from '../album/album.component';
import { AlbumService } from '../../services/album.service';
import { Album } from '../../models/album';
import { AlbumLayout, AlbumSource, CardLayout } from '../../models/views';
import { ActivatedRoute } from '@angular/router';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { LoadingComponent } from '../loading/loading.component';
import { TranslateService } from '../../services/translate.service';

@Component({
    selector: 'mtx-albums',
    templateUrl: './albums.component.html',
    imports: [AlbumComponent, LoadingComponent]
})
export class AlbumsComponent implements OnInit {
    @Input() albums?: Album[] = [];
    @Input() layout: AlbumLayout = 'list';
    @Input() cardLayout: CardLayout = 'card';
    @Input() sourceType: AlbumSource = 'recents';
    @Input() title = '';
    @Input() groupId = '';
    @Input() pageSize = 18;
    @Input() loadNextPage = signal(false);
    @Input() showUnfavorite = false;
    @Input() showRemoveFromPlaylist = false;
    @Input() emptyMessage = 'No albums found';
    @Input() indexToNotify = -4;
    isLoading = signal(false);

    private currentPage = 1;


    private logger: ILogger;

    retrievers = {
        '': (albumService: AlbumService) => { return []; },
        'recents': (albumService: AlbumService) => { return albumService.getRecents(this.currentPage, this.pageSize); },
        'likes': (albumService: AlbumService) => { return albumService.getMostLikes(this.currentPage, this.pageSize); },
        'suggested': (albumService: AlbumService) => { return albumService.getSuggested(this.currentPage, this.pageSize); },
        'history': (albumService: AlbumService) => { return albumService.getHistory(this.currentPage, this.pageSize); },
        'favorites': (albumService: AlbumService) => { return albumService.getFavorites(this.currentPage, this.pageSize); },
        'playlist': (albumService: AlbumService) => { return albumService.getPlayList(this.currentPage, this.pageSize); },
        'category': (albumService: AlbumService) => { return albumService.getByCategory(this.groupId, this.currentPage, this.pageSize); },
        'tag': (albumService: AlbumService) => { return albumService.getByTag(this.groupId, this.currentPage, this.pageSize); },
        'downloads': (albumService: AlbumService) => { return albumService.getDownloadAlbums(this.currentPage, this.pageSize); },
        'search': (albumService: AlbumService) => { return albumService.search(this.title, this.currentPage, this.pageSize); },
        'playByWeek': (albumService: AlbumService) => { return albumService.getByPlaysWeek(this.currentPage, this.pageSize); },
        'playByMonth': (albumService: AlbumService) => { return albumService.getByPlaysMonth(this.currentPage, this.pageSize); },
        'playByYear': (albumService: AlbumService) => { return albumService.getByPlaysYear(this.currentPage, this.pageSize); },
        'all': (albumService: AlbumService) => { return albumService.getFavorites(this.currentPage, this.pageSize); },
    };

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly albumService: AlbumService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AlbumsComponent');
    }

    async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            this.isLoading.set(true);
            this.albums = [];
            this.logger.info('Load albums with params:', this.sourceType, params);

            this.pageSize = params['count'] ?? this.pageSize;
            this.sourceType = params['source'] as AlbumSource ?? this.sourceType;
            this.layout = params['layout'] as AlbumLayout ?? this.layout;
            this.cardLayout = params['cardlayout'] as CardLayout ?? this.cardLayout;
            this.title = params['title'] ?? this.title;
            this.groupId = params['groupId'] ?? this.groupId;
            this.loadNextPage.set(params['paging'] === 'y' ? true : this.loadNextPage());

            this.title = this.translateService.translate(this.title);

            try {
                let retriever = this.retrievers[this.sourceType];
                if (!retriever) {
                    this.albums = [];
                } else {
                    this.albums = await retriever(this.albumService);
                }

                this.logger.info('Loaded albums:', this.sourceType, this.albums);
            } catch (error) {
                this.logger.error('Error loading albums:', error);
                this.albums = [];
            } finally {
                this.isLoading.set(false);
            }
        });
    }

    get hasAlbums(): boolean {
        return (this.albums ?? []).length > 0;
    }

    async albumRemoved(album: Album) {
        this.albums = this.albums?.filter(a => a.id !== album.id);
    }

    async albumLoaded(album: Album) {
        if (!this.loadNextPage() ||
            this.albums?.at(this.indexToNotify)?.id !== album.id)
            return;

        let retriever = this.retrievers[this.sourceType];
        if (retriever) {
            this.logger.info('Load next page for:', this.sourceType, this.currentPage);
            this.currentPage++;
            let next = await retriever(this.albumService);
            if (next.length > 0) {
                this.logger.info('Next page loaded:', next);
                this.albums = this.albums?.concat(next);
            }
        }
    }
}
