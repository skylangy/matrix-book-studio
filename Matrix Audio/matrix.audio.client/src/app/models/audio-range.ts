
export class AudioRange {
    private _blob: Blob;
    private _start: number;
    private _end: number;
    private _duration: number;
    private _blobUrl?: string;
    private _audioBuffer?: AudioBuffer;
    private _dateCreated: Date;
    private _isLast: boolean = false;

    constructor(blob: Blob, start: number, end: number, duration: number) {
        this._blob = blob;
        this._start = start;
        this._end = end;
        this._duration = duration;

        this._blobUrl = URL.createObjectURL(blob);
        this._dateCreated = new Date();
    }

    get blob(): Blob {
        return this._blob;
    }

    get dateCreated(): Date {
        return this._dateCreated;
    }

    get size(): number {
        return this.blob.size;
    }

    get start(): number {
        return this._start;
    }

    get end(): number {
        return this._end;
    }

    get duration(): number {
        return this._duration;
    }

    get blobUrl(): string | undefined {
        return this._blobUrl;
    }

    set blobUrl(value: string | undefined) {
        this._blobUrl = value;
    }

    get audioBuffer(): AudioBuffer | undefined {
        return this._audioBuffer;
    }

    set audioBuffer(value: AudioBuffer | undefined) {
        this._audioBuffer = value;
    }

    get isLast(): boolean {
        return this._isLast;
    }

    set isLast(value: boolean) {
        this._isLast = value;
    }

    release(): void {
        if (this._blobUrl) {
            URL.revokeObjectURL(this._blobUrl);
        }
    }
}

