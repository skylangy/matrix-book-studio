import { AudioRange } from "./audio-range";
import { ILogger } from "./logger";

export interface IAudioPlayer {
    duration: number;
    currentTime: number;
    secondOffsetToPullNext: number;
    playbackRate: number;
    muted: boolean;
    loop: boolean;
    enableLogging: boolean;
    supportChunked: boolean;
    sourceUrl: string;

    appendRange(range: AudioRange): Promise<void>;
    setVolume(volume: number): void;
    seek(time: number): void;
    play(): void;
    stop(): void;
    pause(): Promise<void>;
    resume(): Promise<void>;
    setPullNextCallback(callback: () => void): IAudioPlayer;
    setOntimeupdateCallback(callback: () => void): IAudioPlayer;
    setOnendCallback(callback: () => void): IAudioPlayer;
    setOnplayCallback(callback: () => void): IAudioPlayer;
    setOnpauseCallback(callback: () => void): IAudioPlayer;
    setOnseekedCallback(callback: () => void): IAudioPlayer;
}

export abstract class PlayerBase implements IAudioPlayer {
    protected _pullNextRangeCallback: (() => void) | null = null;
    protected _ontimeupdateCallback: (() => void) | null = null;
    protected _onendCallback: (() => void) | null = null;
    protected _onplayCallback: (() => void) | null = null;
    protected _onpauseCallback: (() => void) | null = null;
    protected _onseekedCallback: (() => void) | null = null;

    protected _playbackDuration = 0;
    protected _isPlaying = false;
    protected _rangeBuffer: AudioRange[] = [];
    protected _secondOffsetToPullNext = 10;
    protected _duration = 0;
    protected _currentTime = 0;
    protected _startTime = 0;
    protected _lastDuration = 0;
    protected _playbackRate = 1;
    protected _muted: boolean = false;
    protected _supportChunked: boolean = true;
    protected _sourceUrl = '';

    loop: boolean = false;
    enableLogging: boolean = true;

    constructor(protected readonly logger: ILogger | undefined = undefined) { }

    get sourceUrl(): string {
        return this._sourceUrl;
    }

    set sourceUrl(value: string) {
        this._sourceUrl = value
        this.onSourceUrlChanged();
    }

    get supportChunked(): boolean {
        return this._supportChunked;
    }

    get duration(): number {
        return this._duration;
    }

    set duration(value: number) {
        this._duration = value;
    }

    get currentTime(): number {
        return this._currentTime;
    }

    set currentTime(value: number) {
        if (this._currentTime !== value) {
            this._currentTime = value;
            this.onCurrentTimeChanged();
        }
    }

    get secondOffsetToPullNext(): number {
        return this._secondOffsetToPullNext;
    }

    set secondOffsetToPullNext(value: number) {
        this._secondOffsetToPullNext = value;
    }

    get muted(): boolean {
        return this._muted;
    }

    set muted(value: boolean) {
        this.setMuted(value);
    }

    get playbackRate(): number {
        return this._playbackRate;
    }

    set playbackRate(value: number) {
        this._playbackRate = value;
        this.onPlaybackRateChanged();
    }

    abstract appendRange(range: AudioRange): Promise<void>;
    abstract setVolume(volume: number): void;
    abstract seek(time: number): void;
    abstract play(): void;
    abstract stop(): void;
    abstract pause(): Promise<void>;
    abstract resume(): Promise<void>;

    setPullNextCallback(callback: () => void): IAudioPlayer {
        this._pullNextRangeCallback = callback;
        return this;
    }

    setOntimeupdateCallback(callback: () => void): IAudioPlayer {
        this._ontimeupdateCallback = callback;
        return this;
    }

    setOnendCallback(callback: () => void): IAudioPlayer {
        this._onendCallback = callback;
        return this;
    }

    setOnplayCallback(callback: () => void): IAudioPlayer {
        this._onplayCallback = callback;
        return this
    }

    setOnpauseCallback(callback: () => void): IAudioPlayer {
        this._onpauseCallback = callback;
        return this
    }

    setOnseekedCallback(callback: () => void): IAudioPlayer {
        this._onseekedCallback = callback;
        return this;
    }

    protected onSourceUrlChanged(): void { }

    protected onCurrentTimeChanged(): void { }

    protected onPlaybackRateChanged(): void { }

    protected setMuted(muted: boolean): void {
        this._muted = muted;
    }

    protected logInfo(message: string): void {
        if (!this.enableLogging) return;
        this.logger?.info(message);
    }

    protected logError(message: string, error?: unknown): void {
        if (!this.enableLogging) return;
        this.logger?.error(message, error);
    }

    protected async togglePlayback(play: boolean, audioElement: HTMLAudioElement): Promise<void> {
        try {
            if (play) {
                await audioElement.play();
                if (audioElement.playbackRate !== this.playbackRate) {
                    audioElement.playbackRate = this.playbackRate;
                }
            } else {
                audioElement.pause();
            }
        } catch (error) {
            this.logError('Failed to play', error);
        }
    }
}
