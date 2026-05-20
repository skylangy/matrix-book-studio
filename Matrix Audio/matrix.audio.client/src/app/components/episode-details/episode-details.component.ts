import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, ViewportScroller } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { AlbumService } from '../../services/album.service';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { Episode } from '../../models/episode';
import { PlayService } from '../../services/play.service';
import { Subscription } from 'rxjs';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { AuthService } from '../../services/auth.service';
import { Album } from '../../models/album';
import { UserService } from '../../services/user.service';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { SecondsToTimePipe } from '../../pipes/seconds.pipe';
import { LoadingComponent } from '../loading/loading.component';
import { ViewService } from '../../services/view.service';
import { LoadingStyles } from '../../models/views';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-episode-details',
    templateUrl: './episode-details.component.html',
    imports: [CommonModule, RouterModule, TextToHtmlPipe, SecondsToTimePipe, ImageUrlPipe, LoadingComponent, TranslatePipe]
})
export class EpisodeDetailsComponent implements OnInit, OnDestroy {
    @ViewChild('episodeContent') episodeContent!: ElementRef<HTMLDivElement>;

    isLoading = false;
    loadingStyle: LoadingStyles = 'book';
    album: Album | undefined = undefined;
    episode: Episode | undefined = undefined;
    progress = 0;
    episodeActions = [
        {
            name: 'Play', size: 'lg-xx',
            getIcon: (episode: Episode) => this.getPlayStatusIcon(episode),
            isEnable: (index: number) => {
                index = this.getIndex();
                return this.isLoggedIn && index < 3;
            },
            action: (episode: Episode) => this.playEpisode(episode)
        },
        {
            name: 'Download', size: 'lg-x',
            getIcon: (episode: Episode) => 'cloud-arrow-down',
            isEnable: (index: number) => this.isLoggedIn,
            action: (episode: Episode) => this.downloadEpisod(episode)
        },
        {
            name: 'Scroll', size: 'lg-x',
            getIcon: (episode: Episode) => this.getScrollIcon(),
            isEnable: (index: number) => {
                return true;
            },
            action: (episode: Episode) => this.autoScroll = !this.autoScroll
        }
    ]
    private autoScroll = true;
    private subscription!: Subscription;
    private logger: ILogger;

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly albumService: AlbumService,
        private readonly playService: PlayService,
        private readonly authService: AuthService,
        private readonly userService: UserService,
        private readonly viewService: ViewService,
        private readonly router: Router,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('EpisodeDetailsComponent');
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get duration(): number {
        if (!this.episode)
            return 0;
        return this.episode.duration! / 1000;
    }

    async ngOnInit() {
        this.viewService.scrollToTopOnNavEnd(this.router);
        this.viewService.scrollToTop();

        this.activatedRoute.params.subscribe(async params => {
            let albumId = params['albumId'];
            let episodeId = params['episodeId'];
            if (!albumId || !episodeId) {
                return;
            }

            this.isLoading = true;
            this.album = await this.albumService.getAlbum(albumId);
            this.album.episodes = this.albumService.sortEpisodesByChapter(this.album.episodes!);
            this.logger.info(`Load album ${this.album?.title}`, this.album)
            this.episode = await this.albumService.getEpisode(albumId, episodeId);

            for (let episode of this.album?.episodes ?? []) {
                episode.album = this.album;
                episode.image = this.album.imageWideSplash;
            }

            this.logger.info(`Load episode ${this.episode?.title}`);

            this.isLoading = false;
        })

        this.subscription = this.playService.progressChanged.subscribe(progress => {
            if (this.playService.currentEpisode?.id === this.episode?.id) {
                this.progress = progress;
                this.scrollConent(this.progress);
            }
        });
    }

    ngOnDestroy(): void {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    getIndex(): number {
        if (this.album && this.episode) {
            let index = this.album.episodes?.findIndex(e => e.id === this.episode?.id);
            return index ?? 0;
        }
        return 0;
    }

    scrollConent(progress: number) {
        if (!this.episodeContent || !this.autoScroll)
            return;

        let container = this.episodeContent.nativeElement;
        let maxScroll = container.scrollHeight - container.clientHeight;
        let position = maxScroll * progress - 5;

        // this.logger.info(`Scrolling content to ${progress}, maxScroll: ${container.scrollHeight} - ${container.clientHeight} = ${maxScroll} position: ${position}`);
        container.scrollTop = position;
    }

    getPlayStatusIcon(episode: Episode): string {
        if (!episode) {
            return 'play-circle';
        }
        if (this.playService.isPlaying && this.playService.currentEpisode?.id === episode.id)
            return 'pause-circle text-success-emphasis';
        return 'play-circle';
    }

    getScrollIcon(): string {
        return this.autoScroll ? 'arrow-up-circle text-success-emphasis' : 'arrow-up-circle';
    }

    async playEpisode(episode: Episode) {
        episode.album = this.album;

        if (episode.id === this.playService.currentEpisode?.id && this.playService.isPlaying) {
            this.logger.info(`Pause episode ${episode.title}`);
            await this.playService.pause();
        }
        else {
            this.logger.info(`Play episode ${episode.title}`);
            this.playService.currentAlbum = this.album;
            await this.playService.play(episode);
        }
    }

    async downloadEpisod(episode: Episode) {
        this.logger.info(`Downloading episode ${episode.title}`);

        this.userService.downloadEpisode(this.album?.id, episode.id, `${episode.title}.mp3`);
    }
}
