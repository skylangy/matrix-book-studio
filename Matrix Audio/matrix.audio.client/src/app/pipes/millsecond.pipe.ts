import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'msToTime',
})
export class MillsecondsToTimePipe implements PipeTransform {

    transform(value?: number): string {
        if (!value)
            return '00:00:00';

        // Create a TimeSpan equivalent in JavaScript
        let milliseconds = value;
        let hours = Math.floor(milliseconds / 3600000);
        milliseconds %= 3600000;
        let minutes = Math.floor(milliseconds / 60000);
        milliseconds %= 60000;
        let seconds = Math.floor(milliseconds / 1000);

        // Format as hh:mm:ss
        return `${this.pad(hours, 2)}:${this.pad(minutes, 2)}:${this.pad(seconds, 2)}`;
    }

    // Helper method to pad numbers with leading zeros
    private pad(num: number, size: number): string {
        let s = num + '';
        while (s.length < size) s = '0' + s;
        return s;
    }
}
