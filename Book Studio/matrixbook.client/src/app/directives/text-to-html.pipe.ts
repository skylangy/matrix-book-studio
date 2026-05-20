import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'textToHtml'
})
export class textToHtmlPipe implements PipeTransform {
    transform(value?: string): any {

        let lines: string[] = [];

        for (let line of value?.split('\n')!) {
            lines.push(`<p>${line}</p>`);
        }

        return lines.join('');
    }
}