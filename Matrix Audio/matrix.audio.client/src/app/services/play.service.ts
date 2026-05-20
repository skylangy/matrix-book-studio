import { Injectable, signal } from '@angular/core';
import { Episode } from '../models/episode';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';
import { PlayRecordService } from './play.record.service';
import { AlbumService } from './album.service';
import { Album } from '../models/album';
import { AuthService } from './auth.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { AudioRange } from '../models/audio-range';
import { PromptService } from './prompt.service';
import { IAudioPlayer } from '../models/playerbase';
import { BrowserDetectionService } from './browser.service';
import { SingleAudioPlayer } from '../models/single-audio-player';
import { Title } from '@angular/platform-browser';
import { AppSettingService } from './appsetting.service';
import { SettingNames } from '../models/app-setting';

@Injectable({ providedIn: 'root' })
export class PlayService {
    private _audioPlayer: IAudioPlayer;
    private _playList: Episode[] = [];
    private _volume: number = 0.5;
    private _duration: number = 0;              // the total duration of the audio
    private _currentTime = signal(0);           // current position in seconds
    private _initialOffset: number = 0;
    private _sleepSeconds: number = 0;
    private _chunkSize: number = 1024 * 1024;
    private _currentEnd: number = 0;
    private _playbackRate: number = 1;
    private _isPlaying: boolean = false;
    private _isMuted: boolean = false;
    private _isRepeating: boolean = false;
    private _enableRecord: boolean = true;
    private _autoPlayNext: boolean = false;
    private _lastRecordTime: number = 0;
    private _progressSubject = new BehaviorSubject<number>(0);
    private _isBufferingSubject = new BehaviorSubject<boolean>(false);
    private _playStoppedSubject = new BehaviorSubject<void>(undefined);
    private _sleepTimerStoppedSubject = new BehaviorSubject<void>(undefined);
    private _currentAlbum: Album | undefined = undefined;
    private _currentEpisode: Episode | undefined = undefined;
    private _sleepTimer: any;
    private readonly logger: ILogger;
    private readonly _streamer: EpisodeStreamer;

    constructor(
        private readonly authService: AuthService,
        private readonly albumService: AlbumService,
        private readonly playRecordService: PlayRecordService,
        private readonly promptService: PromptService,
        private readonly browserService: BrowserDetectionService,
        private readonly appSettingService: AppSettingService,
        private readonly titleService: Title,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('PlayService');

        let osName = this.browserService.getOSInfo();
        this.logger.info(`OS: ${osName}, browser: ${this.browserService.getBrowserInfo()}, MediaSource supported: ${this.browserService.isMediaSourceSupported()}`);
        if (osName === 'iOS' || !this.browserService.isMediaSourceSupported()) {
            this._audioPlayer = new SingleAudioPlayer(loggingService.getLogger('SingleAudioPlayer'));
            this.logger.info(`Using StreamAudioPlayer for audio playback`);
        }
        else {
            this._audioPlayer = new SingleAudioPlayer(loggingService.getLogger('SingleAudioPlayer'));
            this.logger.info(`Using MediaSourceAudioPlayer for audio playback`);
        }

        this._streamer = new EpisodeStreamer(albumService, loggingService.getLogger('EpisodeStreamer'));

        this.initializePlayer();
    }

    get isPlaying(): boolean {
        return this._isPlaying;
    }

    get isRepeating(): boolean {
        return this._isRepeating;
    }

    get duration(): number {
        return this._duration;
    }

    get currentEpisode(): Episode | undefined {
        return this._currentEpisode;
    }

    set currentEpisode(episode: Episode | undefined) {
        this._currentEpisode = episode;
    }

    get canPlay(): boolean {
        return this.currentEpisode !== undefined;
    }

    get position(): number {
        return this._currentTime();
    }

    set position(position: number) {
        this.seekTo(position);
    }

    get volume(): number {
        return this._volume;
    }

    set volume(value: number) {
        this._volume = value;
        this._audioPlayer.setVolume(value);
    }

    get sleepSeconds(): number {
        return this._sleepSeconds;
    }

    set sleepSeconds(value: number) {
        this._sleepSeconds = value;
        if (value > 0) {
            this._sleepTimer = setTimeout(() => {
                this.logger.info(`Sleep timer expired, stopping playback, sleep time: ${value}s`);
                this.stop();
                clearTimeout(this._sleepTimer);
                this._sleepSeconds = 0;
                this._sleepTimerStoppedSubject.next();
            }, value * 1000);
        } else if (this._sleepTimer) {
            clearTimeout(this._sleepTimer);
            this._sleepSeconds = 0;
            this._sleepTimerStoppedSubject.next();
        }
    }

    get playbackRate(): number {
        return this._playbackRate;
    }

    set playbackRate(value: number) {
        if (this.playbackRate !== value && value > 0 && value <= 2) {
            this._playbackRate = value;
            this._audioPlayer.playbackRate = value;
        }
    }

    get sleepTimerStopped(): Observable<void> {
        return this._sleepTimerStoppedSubject.asObservable();
    }

    get currentAlbum(): Album | undefined {
        return this._currentAlbum;
    }

    set currentAlbum(album: Album | undefined) {
        if (this.currentAlbum !== album) {
            this._playList = album?.episodes || [];
            this._currentAlbum = album;
        }
    }

    get progressChanged(): Observable<number> {
        return this._progressSubject.asObservable();
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get appName(): string {
        return this.appSettingService.getConfig(SettingNames.AppName, '') || '';
    }

    add(episode: Episode) {
        this._playList.push(episode);
    }

    async playAlbum(album: Album) {
        if (this.currentAlbum !== album) {
            this.stop();
            this.currentAlbum = album;
            this._currentTime.set(0);
        }

        this._playList = album.episodes!;
        let record = await this.playRecordService.getAlbumRecord(album.id!);
        let episodeToPlay: Episode | undefined = undefined;
        if (record) {
            episodeToPlay = album.episodes!.find(e => e.id === record.episodeId);
        }
        else {
            episodeToPlay = album.episodes![0];
        }
        if (!episodeToPlay!.album)
            episodeToPlay!.album = album;
        this._autoPlayNext = true;

        this.play(episodeToPlay!);
    }

    async play(episode: Episode, startFromSecond: number = -1) {
        try {
            this.logger.info(`Play episode: ${episode.title}, start from: ${startFromSecond}, is playing: ${this._isPlaying}`);


            if (episode !== undefined && episode !== this.currentEpisode) {
                this.stop();
            }

            if (this._isPlaying) {
                return;
            }

            this.currentEpisode = episode;
            this._duration = episode.duration! / 1000;

            if (startFromSecond > 0) {
                this._initialOffset = startFromSecond;
            } else {
                let record = await this.playRecordService.getEpisodeRecord(episode.album?.id!, episode.id!);
                this._initialOffset = record ? record.position : 0;

                this.logger.info(`Episode record position: ${episode.title} ${record?.position.toFixed(2)}s ${this.formatTime(record?.position)}`);
                this.logger.info(`Episode record duration: ${episode.title} ${record?.duration.toFixed(2)}s ${this.formatTime(record?.duration)}`);
            }

            if (this._initialOffset >= episode.duration! / 1000) {
                this.logger.warn(`Invalid initial offset: ${this._initialOffset}, episode duration: ${episode.duration! / 1000}, will play next episode`);

                this._currentTime.set(episode.duration! / 1000);
                this.stop();
                if (this._autoPlayNext) {
                    this.next();
                }
                return;
            }

            this._currentTime.set(this._initialOffset);
            this._currentEnd = this._initialOffset; // this.toBytes(this._initialOffset);

            this.logger.info(`Playing episode: '${episode.title}' at '${this._currentTime().toFixed(2)}s'`);
            this.logger.info(`Episode duration: ${(episode.duration! / 1000).toFixed(2)}s, ${this.formatTime(episode.duration! / 1000)}`);
            this.logger.info(`Resume position: ${this._currentEnd}`);

            let album = episode.album;
            if (!album) {
                this.logger.error(`Album is not set for the episode: ${episode.title}`);
                return;
            }

            this._audioPlayer.sourceUrl = this.albumService.getAlbumStreamUrl(episode.id!);
            if (this._audioPlayer.supportChunked) {
                this._duration = episode.duration ? episode.duration / 1000 : 0 || 0;
                let range = await this.loadRange(this._currentEnd, this._chunkSize);
                if (!range) {
                    this.logger.error(`Failed to load initial chunk for episode: ${episode.title}`);
                    this.promptService.showError('Error playing episode', `An error occurred while playing the episode: Failed to load initial chunk`);
                    return;
                }
                this._currentEnd = range!.end;
                this.logger.info(`Initial chunk loaded with duration: ${range?.duration.toFixed(2)}s`, range);

                await this._audioPlayer.appendRange(range!);
            } else {
                this.logger.info(`Play start at ${this._initialOffset.toFixed(2)}s`);
                this._audioPlayer.currentTime = this._initialOffset;
            }
            this._audioPlayer.play();
            this._isPlaying = true;
            this.titleService.setTitle(`${this.appName} - ${album.title} ${episode.title}`);
            this.recordPlayTime();
        } catch (error) {
            this.logger.error(`Error playing episode: `, error);
            this.promptService.showError('Error playing episode', `An error occurred while playing the episode: ${error}`);
        }
    }

    async pause() {
        this._audioPlayer.pause();
        this._isPlaying = false;

        this.recordPlayTime();
    }

    async resume() {
        this._audioPlayer.resume();
        this._isPlaying = true;

        this.recordPlayTime();
    }

    async stop() {
        this.logger.info(`Stop playing episode: ${this.currentEpisode?.title}`);
        this._audioPlayer.stop();
        this._isPlaying = false;

        this.recordPlayTime();

        this._playStoppedSubject.next();
    }

    next() {
        let index = this._playList.indexOf(this.currentEpisode!);
        this.logger.info(`Next index: ${index}`);
        if (index < this._playList.length - 1) {
            let episode = this._playList[index + 1];
            if (!episode.album)
                episode.album = this.currentAlbum;
            this.play(episode);
        }
    }

    previous() {
        const index = this._playList.indexOf(this.currentEpisode!);
        if (index > 0) {
            const episode = this._playList[index - 1];
            if (!episode.album) {
                episode.album = this.currentAlbum;
            }
            this.play(episode);
        }
    }

    canGoPrevious(): boolean {
        return this.isLoggedIn && this._playList.indexOf(this.currentEpisode!) > 0;
    }

    canGoNext(): boolean {
        return this.isLoggedIn && this._playList.indexOf(this.currentEpisode!) < this._playList.length - 1;
    }

    toggleMute() {
        this._isMuted = !this._isMuted;
        this._audioPlayer.muted = this._isMuted;
    }

    toggleRepeat() {
        this._isRepeating = !this._isRepeating;
        this._audioPlayer.loop = this._isRepeating;
    }

    recordPlayTime() {
        if (!this._enableRecord || !this.currentEpisode)
            return;

        this.logger.info(`Record play time: ${this.currentEpisode.title} ${this._currentTime().toFixed(2)}s ${this.formatTime(this._currentTime())}`);

        this.playRecordService.recordEpisode(
            this.currentEpisode!.album!.id!,
            this.currentEpisode!.id!,
            this._currentTime(),
            this.duration);
    }

    private initializePlayer() {
        this._audioPlayer
            .setPullNextCallback(async () => {
                this._isBufferingSubject.next(true);
                this.logger.info(`Ready to load next chunk at: ${this._currentEnd}`);
                let range = await this.loadRange(this._currentEnd, this._chunkSize * 2);
                if (range) {
                    await this._audioPlayer.appendRange(range!);
                }
                this._isBufferingSubject.next(false);
            })
            .setOntimeupdateCallback(() => {
                this.logger.info(`Player current time changed ${this._audioPlayer.currentTime}, ${this._initialOffset}, ${this._currentTime()}`);
                if (this._currentTime() === this._audioPlayer.currentTime) {
                    this.logger.info(`Current time not changed, ignore`);
                    return;
                }
                this._currentTime.set(this._audioPlayer.currentTime);

                let offset = new Date().getTime() - this._lastRecordTime;
                if (offset > 15000) {
                    this.logger.info(`Current time ${this._currentTime().toFixed(2)}s, duration: ${this.duration.toFixed(2)}s, offset: ${this._initialOffset.toFixed(2)}s`);
                    this.recordPlayTime();
                    this._lastRecordTime = new Date().getTime();
                }

                if (this._progressSubject) {
                    const progress = this._currentTime() / this.duration;
                    this._progressSubject.next(progress);
                }

                this.logger.info(`Current time: ${this._currentTime().toFixed(2)}s`);
            })
            .setOnendCallback(() => {
                this.logger.info(`Stream audio player ended, prepare next episode`);
                this.stop();
                this.next();
            })
            .setOnplayCallback(() => {
                this._isPlaying = true;
                this.play(this.currentEpisode!, this._currentTime());
            })
            .setOnpauseCallback(() => {
                this._isPlaying = false;
                this.pause();
            })
            .setOnseekedCallback(() => {
                this._currentTime.set(this._audioPlayer.currentTime);
            });
    }

    private async seekTo(positionInSeconds: number) {
        if (this._currentTime() === positionInSeconds) {
            return;
        }
        this.stop();

        if (this._audioPlayer.supportChunked) {
            const positionInBytes = this.toBytes(positionInSeconds);
            this._currentEnd = positionInBytes;

            this.logger.info(`Seek to position: ${positionInSeconds.toFixed(2)}, from ${this._currentTime().toFixed(2)}, bytes: ${positionInBytes}`);
            this.stop();
            this._currentTime.set(positionInBytes);
        }

        this.logger.info(`Seek to position: ${positionInSeconds.toFixed(2)}, from ${this._currentTime().toFixed(2)}, total: ${this.duration.toFixed(2)}`);

        this.play(this.currentEpisode!, positionInSeconds);
    }

    private async loadRange(start: number, size: number): Promise<AudioRange | undefined> {
        try {
            if (!this.currentEpisode) {
                this.logger.info(`Current episode is not set`);
                return;
            }

            let range = await this._streamer.loadRange(this.currentEpisode!, start, size);

            this._currentEnd = range?.end || this.currentEpisode?.fileLength!;
            return range;
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            this.logger.error(`Error loading chunk: `, error);
            throw new Error(`Failed to load audio chunk: ${errorMessage}`);
        }
    }

    private toBytes(time: number, bitrate: number = 160): number {
        let bytesPerSecond = bitrate * 1000 / 8;
        return Math.floor(time * bytesPerSecond);
    }

    private formatTime(seconds: number): string {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = Math.floor(seconds % 60);

        // Ensure two-digit formatting for each unit
        const pad = (n: number) => n.toString().padStart(2, '0');

        return `${pad(hours)}:${pad(minutes)}:${pad(secs)}`;
    }
}

export class EpisodeStreamer {
    constructor(
        private readonly albumService: AlbumService,
        private readonly logger: ILogger) { }

    async loadRange(episode: Episode, start: number, size: number): Promise<AudioRange | undefined> {
        try {
            this.logger.info(`Loading audio chunk from: ${start}`);
            let isLast = false;
            if (start + size > episode.fileLength!) {
                size = episode.fileLength! - start;
                isLast = true;
                this.logger.info(`Adjusting chunk size to: ${size}`);
            }

            if (size === 0) {
                this.logger.info(`Chunk size is zero, reached end of the episode`);
                return;
            }

            let blob = await this.albumService.streamEpisode(episode.id!, start, size);
            let duration = this.toDuration(blob.size);
            let range: AudioRange = new AudioRange(
                blob,
                start,
                start + blob.size,
                duration
            );
            range.isLast = isLast;
            this.logger.info(`Chunk loaded from '${start}' to '${start + blob.size}', duration: ${duration.toFixed(2)}s`);

            return range;
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            this.logger.error(`Error loading chunk: `, error);
            throw new Error(`Failed to load audio chunk: ${errorMessage}`);
        }
    }

    private toDuration(bytes: number, bitrate: number = 160): number {
        const bitrateBps = bitrate * 1000;
        const durationSeconds = (bytes * 8) / bitrateBps;
        return durationSeconds;
    }
}
