import { Component, OnInit } from '@angular/core';
import { ToggleButtonComponent } from '../toggle-button/toggle-button.component';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PlayService } from '../../services/play.service';
import { PlayerComponent } from '../player/player.component';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { SecondsToTimePipe } from '../../pipes/seconds.pipe';
import { AudioPlayer } from '../../models/audio-player';
import { FormsModule } from '@angular/forms';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { PlayerSetting } from '../../models/player-setting';
import { NamedSelectableValue } from '../../models/namevalue';
import { LocalDataService } from '../../services/local.data.service';

@Component({
    selector: 'mtx-foot-player',
    templateUrl: './foot-player.component.html',
    imports: [RouterModule,
        CommonModule,
        FormsModule,
        ToggleButtonComponent,
        PlayerComponent,
        ImageUrlPipe,
        SecondsToTimePipe]
})
export class FootPlayerComponent implements OnInit {
    audioPlayer: AudioPlayer;
    playerSetting = new PlayerSetting();
    private logger: ILogger;

    constructor(
        public readonly playService: PlayService,
        private readonly localDataService: LocalDataService,
        loggingService: LoggingService) {
        this.audioPlayer = new AudioPlayer(playService);
        this.logger = loggingService.getLogger('FootPlayerComponent');
    }

    async ngOnInit() {
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

    get albumId(): string {
        return this.playService.currentAlbum?.id ?? '';
    }

    get episodeId(): string {
        return this.playService.currentEpisode?.id ?? '';
    }

    get sleepIcon(): string {
        return this.playerSetting.getSleepIcon(this.playService.isPlaying);
    }

    get speedIcon(): string {
        return this.playerSetting.getSpeedIcon(this.playService.playbackRate);
    }

    toggleMute(mute: boolean) {
        this.playService.toggleMute();
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
}
