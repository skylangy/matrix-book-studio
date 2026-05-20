import { Component, Input, OnInit, Signal, signal } from '@angular/core';
import { Artist } from '../../models/artist';
import { ArtistComponent } from '../artist/artist.component';
import { ArtistLayout, ArtistSource, CardSize } from '../../models/views';
import { ActivatedRoute } from '@angular/router';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { FormsModule } from '@angular/forms';
import { LoadingComponent } from '../loading/loading.component';
import { ArtistService } from '../../services/artist.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { TranslateService } from '../../services/translate.service';


@Component({
    selector: 'mtx-artists',
    templateUrl: './artists.component.html',
    imports: [FormsModule, ArtistComponent, LoadingComponent, TranslatePipe]
})
export class ArtistsComponent implements OnInit {
    @Input() artists: Artist[] = [];
    @Input() layout: ArtistLayout = 'grid';
    @Input() cardSize: CardSize = 'medium';
    @Input() sourceType: ArtistSource = 'recents';
    @Input() title = '';
    @Input() pageSize = 18;
    @Input() loadNextPage = signal(false);
    isLoading = false;
    private indexToNotify = -4;
    private currentPage = 1;

    private _searchText: Signal<string> = signal('');

    private logger: ILogger;

    retrievers = {
        '': (artistService: ArtistService) => { return []; },
        'recents': (artistService: ArtistService) => { return artistService.getRecentArtists(this.currentPage, this.pageSize); },
        'populars': (artistService: ArtistService) => { return artistService.getPopularArtists(this.currentPage, this.pageSize); },
        'all': (artistService: ArtistService) => { return artistService.getAllArtists(this.currentPage, this.pageSize); },
    };

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly artistService: ArtistService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('ArtistsComponent');
    }

    async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            if (params['page'])
                this.currentPage = params['page'];
            if (params['pageSize'])
                this.pageSize = params['pageSize'];
            if (params['source'])
                this.sourceType = params['source'] as ArtistSource;
            if (params['layout'])
                this.layout = params['layout'] as ArtistLayout;
            if (params['cardsize'])
                this.cardSize = params['cardsize'] as CardSize;
            if (params['title'])
                this.title = params['title'];

            if (this.sourceType === 'all') {
                this.loadNextPage.set(true);
            }

            try {
                this.isLoading = true;

                this.title = this.translateService.translate(this.title);

                let retriever = this.retrievers[this.sourceType];
                if (!retriever) {
                    this.artists = [];
                } else {
                    this.artists = await retriever(this.artistService);
                }
            } catch (error) {
                this.logger.error('Error loading artists:', error);
                this.artists = [];
            } finally {
                this.isLoading = false;
            }
        });
    }

    get searchText() {
        return this._searchText();
    }

    set searchText(value: string) {
        this._searchText = signal(value);
        this.search();
    }

    async search() {
        let searchValue = this._searchText();
        this.logger.info('Search', this.searchText);
        for (let artist of this.artists) {
            if (artist.name && artist.name.toLowerCase().includes(searchValue.toLowerCase())) {
                artist.isHidden = false;
            } else {
                artist.isHidden = true;
            }
        }
    }

    async onArtistLoaded(artist: Artist) {
        if (!this.loadNextPage() ||
            this.artists.at(this.indexToNotify)?.id !== artist.id)
            return;

        let retriever = this.retrievers[this.sourceType];
        if (retriever) {
            this.currentPage++;
            let artists = await retriever(this.artistService);
            if (artists) {
                this.artists = this.artists.concat(artists);
            }
        }
    }
}
