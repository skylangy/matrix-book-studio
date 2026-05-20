import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'timeAgo',
    pure: true
})
export class TimeAgoPipe implements PipeTransform {
    transform(value?: number): string {
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
}
