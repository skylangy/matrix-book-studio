import { AudioRange } from "./audio-range";
import { ILogger } from "./logger";
import { PlayerBase } from "./playerbase";

export class HTMLAudioPlayer extends PlayerBase {
    private _audio: HTMLAudioElement;
    private _preloadAudio: HTMLAudioElement;

    private _currentRange: AudioRange | null = null;
    private _rangeMap = new Map<Date, AudioRange>();

    constructor(logger: ILogger | undefined = undefined) {
        super(logger);

        this._audio = new Audio();
        this._preloadAudio = new Audio();
        this._preloadAudio.preload = 'auto';

        this._audio.ontimeupdate = this.handleTimeUpdate.bind(this);
        this._audio.onended = this.handleEnded.bind(this);
    }

    override get currentTime(): number {
        this._currentTime = this._playbackDuration + this._audio.currentTime;
        return this._currentTime;
    }

    override set currentTime(value: number) {
        this._currentTime = value;
    }

    async appendRange(range: AudioRange): Promise<void> {
        if (!range) {
            throw new Error('AudioRange is required');
        }

        try {
            this._rangeBuffer.push(range);

            if (!this._currentRange) {
                this._currentRange = range;
            }

            if (!this._isPlaying) {
                this.loadAndPlayRange(range);
            }

            if (this._rangeBuffer.length > 1) {
                this.preloadNextRange();
            }

            this.logInfo(`Audio range appended to queue, length: ${this._rangeBuffer.length}, range duration: ${range.duration.toFixed(2)}s`);
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            throw new Error(`Failed to append audio range: ${errorMessage}, range: ${range.start}-${range.end}, duration: ${range.duration.toFixed(2)}s`);
        }
    }

    setVolume(volume: number): void {
        this._audio.volume = Math.min(Math.max(volume, 0), 1);
    }

    seek(time: number): void {
        if (time < 0 || time > this._audio.duration) return;

        this._audio.currentTime = time;
    }

    play(): void {
        try {
            if (this._isPlaying) return;
            this._audio.play();
            this._isPlaying = true;
        } catch (e) {
            this.logError(`Error while trying to play: ${e}`);
        }
    }

    stop(): void {
        if (this._isPlaying) {
            this._isPlaying = false;
            this._audio.pause();
            this._audio.currentTime = 0;
            this._playbackDuration = 0;
            this.logInfo(`Playback stopped, buffer length: ${this._rangeBuffer.length}`);
        }
    }

    async pause(): Promise<void> {
        if (!this._isPlaying) return;

        this._audio.pause();
        this._isPlaying = false;
        this.logInfo(`Playback paused, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}`);
    }

    async resume(): Promise<void> {
        if (this._isPlaying) return;

        this._audio.play();
        this._isPlaying = true;
        this.logInfo(`Resumed playback, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}`);
    }

    dispose(): void {
        this.stop();
        this._rangeBuffer.forEach(range => this.releaseRange(range));
        this._audio.src = '';
        this._preloadAudio.src = '';
    }

    protected override setMuted(muted: boolean): void {
        super.setMuted(muted);
        this._audio.muted = muted;
    }

    private loadAndPlayRange(range: AudioRange): void {
        if (!range.blobUrl) {
            range.blobUrl = URL.createObjectURL(range.blob);
        }

        this._audio.src = range.blobUrl!;
        this._audio.load();
    }

    private preloadNextRange(): void {
        const index = this._rangeBuffer.indexOf(this._currentRange!);
        const nextRange = this._rangeBuffer[index + 1];
        if (nextRange && nextRange.blobUrl) {
            this._preloadAudio.src = nextRange.blobUrl;
            this._preloadAudio.load();
            this._preloadAudio.oncanplaythrough = () => {
                this.logInfo(`Next range loaded: ${this._preloadAudio.duration.toFixed(2)}s, ${nextRange.start}-${nextRange.end}`);
            };
        }
    }

    private handleTimeUpdate() {
        const timeRemaining = this._currentRange?.duration! - this._audio.currentTime;

        // this.logInfo(`Remain: ${timeRemaining.toFixed(2)}s, Current time: ${this._audio.currentTime.toFixed(2)}s`);
        // this.logInfo(`Audio duration: ${this._audio.duration.toFixed(2)}s, range duration: ${this._currentRange?.duration.toFixed(2)}s, length: ${this._rangeBuffer.length}`);

        if (timeRemaining <= this._secondOffsetToPullNext &&
            !this._rangeMap.has(this._currentRange!.dateCreated)) {

            this.logInfo(`Time to pull next range: ${timeRemaining.toFixed(2)}s`);
            this._pullNextRangeCallback?.();
            this._rangeMap.set(this._currentRange!.dateCreated, this._currentRange!);
        }

        this._ontimeupdateCallback?.();
    }

    private async handleEnded() {
        if (!this._currentRange) {
            this.logError("No current range available.");
            return;
        }

        const index = this._rangeBuffer.indexOf(this._currentRange);
        if (index === -1) {
            this.logError("Current range not found in buffer.");
            return;
        }

        const range = this._currentRange;
        const nextRange = this._rangeBuffer[index + 1];

        if (nextRange) {
            try {
                this._currentRange = nextRange;
                this.logInfo(`Prepare to play next range: ${nextRange.start}-${nextRange.end}, length: ${this._rangeBuffer.length}`);
                this._playbackDuration += this._audio.duration;
                this._audio.src = this._preloadAudio.src;
                this._audio.currentTime = 0;
                await this._audio.play();
            }
            catch (err) {
                this.logError(`Playback error: ${err}`);
                throw err;
            }
            finally {
                this._rangeBuffer.splice(index, 1);
                this.releaseRange(range!);
                this.logInfo(`Range released: buffer length: ${this._rangeBuffer.length}, duration ${this._audio.duration.toFixed(2)}s, index: '${index}' removed`);
            }
        } else {
            this._isPlaying = false;
            this.logInfo(`No more ranges to play, buffer length: ${this._rangeBuffer.length}`);
            this._onendCallback?.();
        }
    }

    private releaseRange(range: AudioRange): void {
        range.release();
        this._rangeMap.delete(range.dateCreated);
    }
}