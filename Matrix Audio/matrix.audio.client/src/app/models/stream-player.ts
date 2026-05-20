import { AudioRange } from "./audio-range";
import { ILogger } from "./logger";
import { PlayerBase } from "./playerbase";

export class StreamAudioPlayer extends PlayerBase {
    private _audioContext: AudioContext | null = null;
    private _currentSource: AudioBufferSourceNode | null = null;
    private _gainNode: GainNode | null = null;

    constructor(logger: ILogger | undefined = undefined) {
        super(logger);
    }

    async appendRange(range: AudioRange): Promise<void> {
        if (!range) {
            throw new Error('AudioRange is required');
        }

        try {
            this.initContext();
            const buffer = await this.decodeAudioRange(range);
            range.audioBuffer = buffer;

            this._rangeBuffer.push(range);

            this.logInfo(`Audio range appended to queues, length: ${this._rangeBuffer.length}, range duration: ${range.duration.toFixed(2)}s`);
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            throw new Error(`Failed to append audio range: ${errorMessage}, range: ${range.start}-${range.end}, duration: ${range.duration.toFixed(2)}s, state: ${this._audioContext?.state}`);
        }
    }

    setVolume(volume: number): void {
        if (this._gainNode) {
            this._gainNode.gain.value = Math.min(Math.max(volume, 0), 1);
        }
    }

    seek(time: number): void {
        if (time < 0 || time > this._duration) return;

        this.stop();
        this._playbackDuration = time;
        this.play();
    }

    play(): void {
        try {
            this.initContext();
            if (this._isPlaying || !this._audioContext)
                return;
            if (this._audioContext.state === 'suspended') {
                this._audioContext.resume();
            } else if (this._rangeBuffer.length > 0) {
                this._isPlaying = true;
                this.prepareNextBuffer();
            }
        } catch (e) {
            throw e;
        }
    }

    stop(): void {
        this._isPlaying = false;
        this._currentSource?.stop();
        this._currentSource?.disconnect();
        this._currentSource = null;
        this._playbackDuration = 0;
        this.logInfo(`Playback stopped, buffer length: ${this._rangeBuffer.length}`);
    }

    async pause() {
        this.initContext();
        if (!this._isPlaying || !this._audioContext)
            return;

        await this._audioContext.suspend();
        this._isPlaying = false;
        this.logInfo(`Playback paused, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}, state: ${this._audioContext.state}`);
    }

    async resume(): Promise<void> {
        this.initContext();
        if (!this._audioContext)
            return;

        if (this._audioContext.state === 'suspended') {
            this._audioContext.resume()
                .then(() => {
                    this._isPlaying = true;
                    this._pullNextRangeCallback?.();
                    this.monitorProgress(this._lastDuration);
                    this.logInfo(`Resumed playback, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}, state: ${this._audioContext!.state}`);
                })
                .catch((error) => {
                    this.logError('Failed to resume playback', error);
                }).finally(() => {
                    this.logInfo(`Resume playback finished, buffer length: ${this._rangeBuffer.length}`);
                });
        }
    }

    private initContext() {
        if (!this._audioContext) {
            this._audioContext = new AudioContext();
            this._gainNode = this._audioContext.createGain();
            this._startTime = this._audioContext.currentTime;
        }
    }

    private playBuffer(range: AudioRange): void {
        const source = this._audioContext!.createBufferSource();
        source.buffer = range.audioBuffer!;
        source.connect(this._gainNode!).connect(this._audioContext!.destination);
        source.onended = () => {
            this.logInfo(`Audio source buffer ended, playing ${this._isPlaying}`);
            if (this._isPlaying) {
                this.prepareNextBuffer();
            }
        };
        source.start();
        this._currentSource = source;
    }

    private checkForNextChunk(duration: number): void {
        const check = () => {
            const timeRemaining = duration - this._audioContext!.currentTime;
            if (timeRemaining <= this._secondOffsetToPullNext) {
                this.logInfo(`Ready to pull next chunk ${timeRemaining.toFixed(2)}s`);
                this._pullNextRangeCallback?.();
            } else {
                requestAnimationFrame(check);
            }
        };
        requestAnimationFrame(check);
    }

    private prepareNextBuffer(): void {
        if (!this._isPlaying || this._rangeBuffer.length === 0) {
            this.logInfo(`Buffer is empty, no more buffers to play, buffer length: ${this._rangeBuffer.length}, playing: ${this._isPlaying}`);
            return;
        }

        const range = this._rangeBuffer.shift();
        if (!range) {
            this.logInfo(`Buffer is empty, no more buffers to play, ${this._rangeBuffer.length}`);
            return;
        }

        this.logInfo(`Range: ${range.start}-${range.end}`);
        this.logInfo(`Buffer Duration: ${range.audioBuffer!.duration.toFixed(2)}s`);
        this.logInfo(`Duration: ${this.duration.toFixed(2)}s`);


        this.playBuffer(range);
        this.checkForNextChunk(range.duration);

        this._playbackDuration += range.audioBuffer!.duration;
        this.logInfo(`Playback Duration: ${this._playbackDuration.toFixed(2)}s`);

        this.monitorProgress(range.audioBuffer!.duration);
    }

    private async decodeAudioRange(range: AudioRange): Promise<AudioBuffer> {
        if (this._audioContext!.state === 'suspended') {
            await this._audioContext!.resume();
        }
        const arrayBuffer = await range.blob.arrayBuffer();

        return await this._audioContext!.decodeAudioData(arrayBuffer);
    }

    private monitorProgress(duration: number): void {
        this._lastDuration = duration;
        const startTime = this._audioContext!.currentTime;
        const endTime = startTime + duration;
        const throttleInterval = 0.25;          // Update interval in seconds (250ms)
        let lastUpdateTime = startTime;         // Store the last update time

        this.logInfo(`Start monitor progress, start time: ${startTime.toFixed(2)}, duration: ${duration.toFixed(2)}s`);

        const monitor = () => {
            if (!this._isPlaying) {
                this.logInfo('Playback stopped, no more monitoring');
                return;
            }

            const currentTime = this._audioContext!.currentTime;
            if (currentTime >= endTime) {
                this.logInfo('Playback completed, no more monitoring');
                return;
            }

            this._currentTime = this._playbackDuration + currentTime;

            if (currentTime - lastUpdateTime >= throttleInterval) {
                this._ontimeupdateCallback?.();
                // this.logInfo(`Progress: currentTime: ${this.currentTime.toFixed(2)}s, context currentTime: ${this._audioContext!.currentTime.toFixed(2)}`);
                lastUpdateTime = currentTime;
            }
            requestAnimationFrame(monitor);
        };
        monitor();
    }

}
