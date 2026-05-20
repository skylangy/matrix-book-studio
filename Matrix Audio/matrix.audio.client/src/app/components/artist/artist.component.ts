import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Artist } from '../../models/artist';
import { ILogger } from '../../models/logger';
import { CardSize } from '../../models/views';
import { LazyImageDirective } from '../../pipes/lazy.image.directive';
import { ImageService } from '../../services/image.service';
import { LoggingService } from '../../services/logging.service';

@Component({
    selector: 'mtx-artist',
    templateUrl: './artist.component.html',
    imports: [RouterModule, LazyImageDirective]
})
export class ArtistComponent implements OnInit {

    @Input() artist?: Artist;
    @Input() cardSize: CardSize = 'medium';
    @Output() artistLoaded = new EventEmitter<Artist>();
    private readonly logger: ILogger;

    constructor(
        private readonly imageService: ImageService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('ArtistComponent');
    }

    ngOnInit(): void {
        this.artistLoaded.emit(this.artist);
    }

    onImgError(event: Event) {
        const img = event.target as HTMLImageElement;
        img.src = this.imageService.defaultArtistAvatarUrl
    }
}
