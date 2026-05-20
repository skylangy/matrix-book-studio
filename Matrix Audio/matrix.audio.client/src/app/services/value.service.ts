import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ValueService {
    constructor() { }

    formatSeconds(value?: number): string {
        if (!value || value < 0) {
            return '00:00:00';
        }

        const hours = Math.floor(value / 3600);
        const minutes = Math.floor((value % 3600) / 60);
        const seconds = Math.floor(value % 60);

        const formattedHours = this.padZero(hours);
        const formattedMinutes = this.padZero(minutes);
        const formattedSeconds = this.padZero(seconds);

        return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
    }

    formatDate(date: Date): string {
        return date.toISOString().split('T')[0];
    }

    ago(value: number): string {
        if (!value) return '';

        const currentTime = new Date().getTime();
        const timestamp = new Date(value).getTime();
        const difference = currentTime - timestamp;

        const minutes = Math.floor(difference / 60000); // 1 minute = 60,000 milliseconds
        const seconds = Math.floor(difference / 1000);

        if (minutes > 0) {
            return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
        } else {
            return `${seconds} second${seconds > 1 ? 's' : ''} ago`;
        }
    }

    private padZero(value: number): string {
        return value < 10 ? '0' + value : '' + value;
    }
}