import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Album } from '../../models/album';
import { RouterModule } from '@angular/router';
import { CardLayout } from '../../models/views';
import { CommonModule } from '@angular/common';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { LazyLoadImageDirective } from '../../pipes/lazyload.pipe';
import { PlayService } from '../../services/play.service';
import { SummaryPipe } from '../../pipes/summary.pipe';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { UserService } from '../../services/user.service';
import { PromptService } from '../../services/prompt.service';
import { AlbumService } from '../../services/album.service';
import { AppSettingService } from '../../services/appsetting.service';
import { ImageService } from '../../services/image.service';

@Component({
    selector: 'mtx-album',
    templateUrl: './album.component.html',
    imports: [RouterModule, CommonModule, ImageUrlPipe,
        LazyLoadImageDirective, SummaryPipe, TextToHtmlPipe]
})
export class AlbumComponent implements OnInit {
    @Input() album?: Album;
    @Input() cardLayout: CardLayout = 'card';
    @Input() showUnfavorite = false;
    @Input() showRemoveFromPlaylist = false;

    @Output() albumRemoved = new EventEmitter<Album>();
    @Output() albumLoaded = new EventEmitter<Album>();

    private logger: ILogger;

    constructor(
        private readonly albumService: AlbumService,
        private readonly playService: PlayService,
        private readonly userService: UserService,
        private readonly promptService: PromptService,
        private readonly appSettingService: AppSettingService,
        private readonly imageService: ImageService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AlbumComponent');
    }

    ngOnInit(): void {
        this.albumLoaded.emit(this.album);
    }

    get placeholder(): string {
        return this.appSettingService.defaultSplash;
    }

    async playAlbum() {
        this.logger.info('Playing album: ', this.album?.title);
        this.album = await this.albumService.getAlbum(this.album?.id!);
        this.playService.playAlbum(this.album!);
    }

    async unfavoriteAlbum() {
        this.logger.info('Unfavorite album: ', this.album?.title);
        await this.userService.unfavoriteAlbum(this.album?.id);
        this.albumRemoved.emit(this.album!);
        this.promptService.showSuccess('Removed', 'Album removed from favorites');
    }

    async removeFromPlaylist() {
        this.logger.info('Remove from playlist: ', this.album?.title);
        await this.userService.removeFromPlayList(this.album?.id);
        this.albumRemoved.emit(this.album!);
        this.promptService.showSuccess('Removed', 'Album removed from playlist');
    }

    onImgError(event: Event) {
        const img = event.target as HTMLImageElement;
        img.src = this.imageService.defaultAlbumSplashUrl;
    }
}
