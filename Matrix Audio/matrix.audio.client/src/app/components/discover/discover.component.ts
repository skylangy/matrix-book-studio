import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { AlbumsComponent } from '../albums/albums.component';
import { CategoriesComponent } from '../categories/categories.component';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ArtistsComponent } from '../artists/artists.component';
import { ViewService } from '../../services/view.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-discover',
    templateUrl: './discover.component.html',
    imports: [FormsModule, HeaderContentComponent, AlbumsComponent, CategoriesComponent, ArtistsComponent,
        TranslatePipe
    ]
})
export class DiscoveryComponent implements OnInit {
    searchText = '';
    pageSize = 18;

    private logger: ILogger;

    constructor(
        private router: Router,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('DiscoveryComponent');
    }

    ngOnInit(): void {
    }

    async search() {
        this.logger.info('Search:', this.searchText);
        this.router.navigate(['/public/albums/search/list/horizontal', this.searchText, 50, 1]);
    }
}
