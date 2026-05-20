import { PlayService } from "../services/play.service";


export class AudioPlayer {
    private playerPosition = 0;
    private playerVolume = 1;

    constructor(private readonly playService: PlayService) {

    }

    get canPlay(): boolean {
        return this.playService.currentEpisode !== undefined;
    }

    get isPlaying() {
        return this.playService.isPlaying;
    }

    get episode() {
        return this.playService.currentEpisode;
    }

    get currentPosition(): number {
        this.playerPosition = this.playService.position;
        return this.playerPosition;
    }

    set currentPosition(value: number) {
        this.playerPosition = value;
        this.playService.position = value;
    }

    get volume(): number {
        return this.playService.volume * 100;
    }

    set volume(value: number) {
        this.playerVolume = value;
        this.playService.volume = value / 100;
    }

    get duration(): number {
        return this.playService.duration;
    }

    get canGoNext(): boolean {
        return this.playService.canGoNext();
    }

    get canGoPrev(): boolean {
        return this.playService.canGoPrevious();
    }

    get playbackRate(): number {
        return this.playService.playbackRate;
    }

    set playbackRate(value: number) {
        this.playService.playbackRate = value;
    }

    playStatusChanged(play: boolean) {
        if (play) {
            if (this.playService.currentEpisode == this.episode) {
                this.playService.resume();
            } else {
                this.playService.play(this.episode!);
            }
        } else {
            this.playService.pause();
        }
    }

    prev() {
        this.playService.previous();
    }

    next() {
        this.playService.next();
    }
}