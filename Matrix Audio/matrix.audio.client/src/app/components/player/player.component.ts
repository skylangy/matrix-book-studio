import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToggleButtonComponent } from '../toggle-button/toggle-button.component';
import { PlayService } from '../../services/play.service';
import { Episode } from '../../models/episode';
import { SecondsToTimePipe } from '../../pipes/seconds.pipe';
import { AudioPlayer } from '../../models/audio-player';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { LoggingService } from '../../services/logging.service';
import { ILogger } from '../../models/logger';
import { RouterModule } from '@angular/router';
import { ProgressComponent } from '../progress/progress.component';
import { LocalDataService } from '../../services/local.data.service';
import { PlayerSetting } from '../../models/player-setting';
import { NamedSelectableValue } from '../../models/namevalue';

@Component({
    selector: 'mtx-player',
    templateUrl: './player.component.html',
    imports: [CommonModule, FormsModule, RouterModule, ToggleButtonComponent, SecondsToTimePipe, ImageUrlPipe, ProgressComponent]
})
export class PlayerComponent implements OnInit {

    @Input() allowRepeat = true;
    @Input() allowSleep = true;
    progressHeight = 3;
    playerSetting = new PlayerSetting();

    audioPlayer: AudioPlayer;
    private _isCompact = false;
    private _episode: Episode | undefined = undefined;
    private readonly logger: ILogger;

    constructor(
        private readonly playService: PlayService,
        private readonly localDataService: LocalDataService,
        loggingService: LoggingService
    ) {
        this.audioPlayer = new AudioPlayer(playService);

        this.logger = loggingService.getLogger('PlayerComponent');
    }

    ngOnInit() {
        this.playService.sleepTimerStopped.subscribe(() => {
            this.playerSetting.sleepSettings.forEach(s => s.selected = false);
            this.playerSetting.sleepSettings[0].selected = true;
        });

        this.isCompact = this.localDataService.get(PlayerSetting.compactKey) || false;

        const speed = this.localDataService.get(PlayerSetting.speedKey);
        if (speed) {
            this.playerSetting.speedSettings.forEach(s => s.selected = false);
            const speedSetting = this.playerSetting.speedSettings.find(s => s.value === speed);
            if (speedSetting) {
                speedSetting.selected = true;
                this.playService.playbackRate = speedSetting.value;
            }
        }
    }

    @Input()
    get isCompact() {
        return this._isCompact;
    }

    set isCompact(value: boolean) {
        this._isCompact = value;
        this.localDataService.set(PlayerSetting.compactKey, value);
    }

    @Input()
    get episode() {
        return this._episode;
    }

    set episode(value: Episode | undefined) {
        this._episode = value;

        if (this.playService.currentEpisode === undefined) {
            this.playService.currentEpisode = value;
            this.playService.currentAlbum = value?.album;
        }
    }

    get isRepeating() {
        return this.playService.isRepeating;
    }

    get epipsodeUrl(): string {
        return `episode/${this.episode?.albumId}/${this.episode?.id}`;
    }

    get sleepIcon(): string {
        return this.playerSetting.getSleepIcon(this.playService.isPlaying);
    }

    get speedIcon(): string {
        return this.playerSetting.getSpeedIcon(this.playService.playbackRate);
    }

    get progress(): number {
        if (!this.audioPlayer.duration) {
            return 0;
        }
        return this.audioPlayer.currentPosition / this.audioPlayer.duration * 100;
    }

    get modeIcon(): string {
        return this.isCompact ? 'arrows-angle-expand' : 'arrows-angle-contract';
    }

    toggleMode() {
        this.isCompact = !this.isCompact;
    }

    toggleRepeat() {
        this.playService.toggleRepeat();
    }

    toggleSleep() {
    }

    setSleep(sleepSetting: NamedSelectableValue) {
        this.playerSetting.sleepSettings.forEach(s => s.selected = false);
        sleepSetting.selected = true;
        this.playService.sleepSeconds = sleepSetting.value;
    }

    setSpeed(speedSetting: NamedSelectableValue) {
        this.playerSetting.speedSettings.forEach(s => s.selected = false);
        speedSetting.selected = true;
        this.playService.playbackRate = speedSetting.value;
        this.localDataService.set(PlayerSetting.speedKey, speedSetting.value);
    }

    playNext() {
        this.audioPlayer.next();
    }

    playPrevious() {
        this.audioPlayer.prev();
    }
}
