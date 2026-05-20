import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Action } from '../../models/action';
import { Album } from '../../models/album';
import { Episode } from '../../models/episode';
import { ILogger } from '../../models/logger';
import { LoadingStyles } from '../../models/views';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { MillsecondsToTimePipe } from '../../pipes/millsecond.pipe';
import { SecondsToTimePipe } from '../../pipes/seconds.pipe';
import { SummaryPipe } from '../../pipes/summary.pipe';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { AlbumService } from '../../services/album.service';
import { AuthService } from '../../services/auth.service';
import { ClipboardService } from '../../services/clipboard.service';
import { LoggingService } from '../../services/logging.service';
import { PlayRecordService } from '../../services/play.record.service';
import { PlayService } from '../../services/play.service';
import { PromptService } from '../../services/prompt.service';
import { TranslateService } from '../../services/translate.service';
import { UserService } from '../../services/user.service';
import { ViewService } from '../../services/view.service';
import { LoadingComponent } from '../loading/loading.component';
import { ProgressComponent } from '../progress/progress.component';
import { SigninPromptComponent } from '../signin-prompt/signin-prompt.component';


@Component({
    selector: 'mtx-album-details',
    templateUrl: './album-details.component.html',
    imports: [CommonModule, RouterModule, ImageUrlPipe, TextToHtmlPipe, SecondsToTimePipe,
        ProgressComponent, SigninPromptComponent, SummaryPipe, MillsecondsToTimePipe,
        LoadingComponent, TranslatePipe]
})
export class AlbumDetailsComponent implements OnInit {
    private logger: ILogger
    album: Album | undefined = undefined;
    isLoading = false;
    loadingStyle: LoadingStyles = 'book';

    actions: Action[] = [];
    episodeActions: Action[] = [];

    constructor(
        private readonly router: Router,
        private readonly authService: AuthService,
        private readonly activatedRoute: ActivatedRoute,
        private readonly userService: UserService,
        private readonly albumService: AlbumService,
        private readonly playService: PlayService,
        private readonly playRecordService: PlayRecordService,
        private readonly clipboardService: ClipboardService,
        private readonly promptService: PromptService,
        private readonly viewService: ViewService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('AlbumDetailsComponent');
    }

    ngOnInit() {
        this.viewService.scrollToTopOnNavEnd(this.router);
        this.viewService.scrollToTop();

        this.actions = this.initActions();
        this.episodeActions = this.initEpisodeActions();

        this.activatedRoute.params.subscribe(async params => {
            let id = params['id'];
            if (!id)
                return;
            this.isLoading = true;
            this.album = await this.albumService.getAlbum(id)

            this.logger.info(`Loading album details for id:`, this.album?.episodes);
            this.album.episodes = this.albumService.sortEpisodesByChapter(this.album.episodes!);

            for (let episode of this.album.episodes!) {
                episode.progress = await this.getPlayProgress(episode);
            }
            this.logger.info('Current user', this.userService.userId, this.userService.subscription, this.userService.hasSubscription);
            this.isLoading = false;
        });

        let subscription = this.userService.subscription;
        if (!subscription || !subscription.isActive) {
            this.logger.info('User is not subscribed', subscription);
            this.actions.unshift({ name: 'Buy', icon: 'bag', size: 'lg-xx', isEnable: () => this.isLoggedIn, action: () => this.play() });
        }
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get hasSubscription(): boolean {
        return this.userService.hasSubscription;
    }

    async like() {
        if (!this.album)
            return;
        this.logger.info(`Liking album ${this.album.id}, ${this.userService.userId}`);
        await this.userService.likeAlbum(this.album.id);
        this.promptService.showSuccess(this.translateService.translate('Like'), this.translateService.translate('Album liked'));
    }

    async favorite() {
        if (!this.album)
            return;
        await this.userService.favoriteAlbum(this.album.id);
        this.promptService.showSuccess(this.translateService.translate('Favorite'), this.translateService.translate('Album added to your favorite list'));
    }

    async addToPlayList() {
        await this.userService.addToPlayList(this.album?.id);
        this.promptService.showSuccess(this.translateService.translate('Playlist'), this.translateService.translate('Album added to your play list'));
    }

    async downloadAlbum() {
        this.logger.info(`Downloading album ${this.album?.title}`);
        await this.userService.downloadAlbum(this.album?.id);
    }

    async play() {
        if (!this.album || !this.album.episodes || this.album.episodes.length === 0)
            return;

        this.logger.info(`Playing album ${this.album.id}`);
        this.playService.playAlbum(this.album);
    }

    async share() {
        let url = window.location.href;

        this.clipboardService.copyToClipboard(url);
        this.promptService.showSuccess('Share', 'Album address copied to clipboard');
    }

    getPlayStatusIcon(episode: Episode): string {
        if (this.playService.isPlaying && this.playService.currentEpisode?.id === episode.id)
            return 'pause-circle text-success-emphasis';
        return 'play-circle';
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

    async getPlayProgress(episode: Episode): Promise<number> {
        if (!this.album || !this.isLoggedIn)
            return 0;
        let record = await this.playRecordService.getEpisodeRecord(this.album!.id!, episode.id!);

        if (record && episode.duration) {
            let progress = (record.position / record.duration!) * 100;
            return progress;
        }
        return 0;
    }

    private initActions(): Action[] {
        return [
            { name: this.translateService.translate('Like'), icon: 'hand-thumbs-up', size: 'lg-x', isEnable: () => this.isLoggedIn, action: () => this.like() },
            { name: this.translateService.translate('Favorite'), icon: 'star', size: 'lg-x', isEnable: () => this.isLoggedIn, action: () => this.favorite() },
            { name: this.translateService.translate('Add to my play list'), icon: 'plus-square', size: 'lg-x', isEnable: () => this.isLoggedIn, action: () => this.addToPlayList() },
            { name: this.translateService.translate('Download'), icon: 'cloud-arrow-down', size: 'lg-x', isEnable: () => this.isLoggedIn, action: () => this.downloadAlbum() },
            { name: this.translateService.translate('Share'), icon: 'share', size: 'lg-x', isEnable: () => true, action: () => this.share() },
            { name: this.translateService.translate('Play'), icon: 'play-circle', size: 'lg-xx', isEnable: () => this.hasSubscription, action: () => this.play() },
        ];
    }

    private initEpisodeActions(): Action[] {
        return [
            {
                name: this.translateService.translate('Download'), size: 'lg-x',
                getIcon: (episode: Episode) => 'cloud-arrow-down',
                isEnable: (index: number) => this.hasSubscription,
                action: (episode: Episode) => this.downloadEpisod(episode)
            },
            {
                name: this.translateService.translate('Play'), size: 'lg-xx',
                getIcon: (episode: Episode) => this.getPlayStatusIcon(episode),
                isEnable: (index: number) => {
                    return this.canPlayAlbum() || (!this.hasSubscription && index < 3);
                },
                action: (episode: Episode) => this.playEpisode(episode)
            },
        ];
    }

    private canPlayAlbum(): boolean {
        return this.userService.canPlayAlbum(this.album?.level || 1000);
    }
}
