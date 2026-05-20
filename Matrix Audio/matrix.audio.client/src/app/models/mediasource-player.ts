import { AudioRange } from "./audio-range";
import { ILogger } from "./logger";
import { PlayerBase } from "./playerbase";

export class MediaSourceAudioPlayer extends PlayerBase {
    private _mediaSource: MediaSource | null = null;
    private _sourceBuffer: SourceBuffer | null = null;
    private _audio: HTMLAudioElement | null = null;

    constructor(logger: ILogger | undefined = undefined) {
        super(logger);

        try {
            this._mediaSource = new MediaSource();
            this._audio = new Audio();
            this._audio.preload = 'auto';
            this._audio.src = URL.createObjectURL(this._mediaSource);
            this._audio.ontimeupdate = this.handleTimeUpdate.bind(this);
            this._audio.onended = this.handlePlaybackEnded.bind(this);
            this._audio.oncanplaythrough = () => {
                this.logInfo(`Audio is ready to play, duration: ${this._audio!.duration.toFixed(2)}s`);
            };

            this._mediaSource.addEventListener('sourceopen', this.handleSourceOpen.bind(this));
            this._mediaSource.addEventListener('sourceended', () => { this.logInfo('MediaSource ended'); });
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            this.logError(`Failed to initialize MediaSourceAudioPlayer: ${errorMessage}`);
            throw error;
        }
    }

    override get duration(): number {
        return this._audio?.duration || this._duration;
    }

    override get currentTime(): number {
        return this._audio?.currentTime || this._currentTime;
    }

    async appendRange(range: AudioRange): Promise<void> {
        if (!range) {
            throw new Error('AudioRange is required');
        }

        try {
            if (this._sourceBuffer) {
                const arrayBuffer = await range.blob.arrayBuffer();
                this._sourceBuffer.appendBuffer(arrayBuffer);

                if (range.isLast) {
                    this._sourceBuffer.addEventListener('updateend', () => {
                        if (!this._sourceBuffer!.updating) {
                            this._mediaSource!.endOfStream();
                        }
                    }, { once: true });
                }
                range.release();
                this.logInfo(`Audio range appended, buffer length: range duration: ${range.duration.toFixed(2)}s`);
            } else {
                this.logError('SourceBuffer is not initialized');
                throw new Error('SourceBuffer is not initialized');
            }
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            throw new Error(`Failed to append audio range: ${errorMessage}`);
        }
    }

    setVolume(volume: number): void {
        if (!this._audio)
            return;

        this._audio.volume = Math.min(Math.max(volume, 0), 1);
    }

    seek(time: number): void {
        if (!this._audio || time < 0 || time > this._audio.duration)
            return;

        this.logInfo(`Seeking to ${time.toFixed(2)}s`);
        this._audio.currentTime = time;
    }

    async play(): Promise<void> {
        if (!this._audio || this._isPlaying)
            return;

        await this.togglePlayback(true, this._audio);
        this._isPlaying = true;
    }

    async stop(): Promise<void> {
        if (!this._audio)
            return;

        if (this._isPlaying) {
            await this.togglePlayback(false, this._audio);
            this._audio.currentTime = 0;
            this.logInfo(`Playback stopped, buffer length: ${this._rangeBuffer.length}`);
            this._isPlaying = false;
        }
    }

    async pause(): Promise<void> {
        if (!this._audio || !this._isPlaying)
            return;

        await this.togglePlayback(false, this._audio);
        this.logInfo(`Playback paused, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}`);
        this._isPlaying = false;
    }

    async resume(): Promise<void> {
        if (!this._audio || this._isPlaying)
            return;

        await this.togglePlayback(true, this._audio);
        this.logInfo(`Resumed playback, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}`);
        this._isPlaying = true;
    }

    private handleSourceOpen() {
        if (!this._mediaSource) {
            this.logError('MediaSource is not initialized');
            return;
        }

        if (this._mediaSource.sourceBuffers.length > 0) {
            this.logInfo('Source buffer is already set');
            this._sourceBuffer = this._mediaSource.sourceBuffers[0];
            return;
        }

        this._sourceBuffer = this._mediaSource.addSourceBuffer('audio/mpeg');
        this._sourceBuffer.mode = 'sequence';
        this._sourceBuffer.addEventListener('updateend', () => {
            this.logInfo(`SourceBuffer updated, ${this._sourceBuffer!.buffered}`);
        });
        this.logInfo(`SourceBuffer updated, requesting next range`);
        this._pullNextRangeCallback?.();
    }

    private handleTimeUpdate() {
        if (!this._audio || !this._sourceBuffer || !this._sourceBuffer.buffered.length)
            return;

        this._ontimeupdateCallback?.();

        const bufferEnd = this._sourceBuffer.buffered.end(this._sourceBuffer.buffered.length - 1);
        // this.logInfo(`Time update: ${this._audio.currentTime.toFixed(2)}s, duration: ${this._audio.duration.toFixed(2)}s, buffer end: ${bufferEnd.toFixed(2)}s`);

        const timeRemaining = bufferEnd - this._audio.currentTime;
        if (timeRemaining <= this._secondOffsetToPullNext) {
            this.logInfo(`Pull next range, time remaining: ${timeRemaining.toFixed(2)}s, current time: ${this._audio.currentTime.toFixed(2)}s, buffer end: ${bufferEnd.toFixed(2)}s`);
            this._pullNextRangeCallback?.();
        }
    }

    private handlePlaybackEnded() {
        this.logInfo(`Playback ended, buffer length: ${this._rangeBuffer.length}`);
        this._onendCallback?.();
    }
}

