import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'summary',

})
export class SummaryPipe implements PipeTransform {

    transform(value?: string, maxLength: number = 160): string {
        if (!value) {
            return '';
        }

        if (value.length <= maxLength) {
            return value;
        }

        return value.substring(0, maxLength) + '...';
    }
}
