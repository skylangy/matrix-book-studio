
import { AudioRange } from "./audio-range";
import { ILogger } from "./logger";
import { PlayerBase } from "./playerbase";

export class SingleAudioPlayer extends PlayerBase {
    private readonly audioElement: HTMLAudioElement;

    constructor(logger?: ILogger) {
        super(logger);

        this._supportChunked = false;
        this.audioElement = this.createAudioElement();

        this.initializeEventListeners();
    }

    async appendRange(range: AudioRange): Promise<void> {
        this.logInfo(`Appended range is not supported`);
    }

    setVolume(volume: number): void {
        this.audioElement.volume = Math.max(0, Math.min(volume, 1));
        this.logInfo(`Volume set to: ${volume}`);
    }

    seek(time: number): void {
        if (time >= 0 && time <= this._duration) {
            this.audioElement.currentTime = time;
            this._currentTime = time;
            this.logInfo(`Seeked to time: ${time}`);
        } else {
            this.logError(`Seek time out of range: ${time}`);
        }
    }

    play(): void {
        this.togglePlayback(true, this.audioElement);
        this._isPlaying = true;
        this.logInfo('Playback started');
    }

    stop(): void {
        this.audioElement.pause();
        this.audioElement.currentTime = 0;
        this._isPlaying = false;
        this.logInfo('Playback stopped');
    }

    async pause(): Promise<void> {
        this.audioElement.pause();
        this._isPlaying = false;
        this.logInfo('Playback paused');
    }

    async resume(): Promise<void> {
        await this.togglePlayback(true, this.audioElement);
        this._isPlaying = true;
        this.logInfo('Playback resumed');
    }

    protected override onSourceUrlChanged(): void {
        const source = document.createElement('source');
        source.src = this.sourceUrl;
        source.type = 'audio/mpeg';
        this.audioElement.replaceChildren(source);
        this.audioElement.load();

        this.logInfo(`Source URL changed to: ${this._sourceUrl}`);
    }

    protected override onCurrentTimeChanged(): void {
        this.audioElement.currentTime = this._currentTime;
        this.logInfo(`Current time changed to: ${this._currentTime}`);
    }

    protected override setMuted(value: boolean): void {
        this.audioElement.muted = value;
        this._muted = value;
        this.logInfo(`Muted set to: ${value}`);
    }

    protected override onPlaybackRateChanged(): void {
        this.audioElement.playbackRate = this._playbackRate;
        this.logInfo(`Playback rate set to: ${this._playbackRate}`);
    }

    private createAudioElement(): HTMLAudioElement {
        const audio = new Audio();

        audio.autoplay = false;
        audio.muted = this._muted;
        audio.loop = this.loop;
        audio.preload = 'auto';

        this.logInfo('Audio element created dynamically');
        return audio;
    }

    private initializeEventListeners(): void {
        this.audioElement.ontimeupdate = () => {
            this._currentTime = this.audioElement.currentTime;
            this._ontimeupdateCallback?.();
        };

        this.audioElement.onended = () => {
            this._onendCallback?.();
        };

        this.audioElement.onplay = () => {
            this._isPlaying = true;
            this._onplayCallback?.();
        };

        this.audioElement.onpause = () => {
            this._isPlaying = false;
            this._onpauseCallback?.();
        };

        this.audioElement.onseeked = () => {
            this._onseekedCallback?.();
        }

        this.logInfo('Audio element event listeners initialized');
    }
}