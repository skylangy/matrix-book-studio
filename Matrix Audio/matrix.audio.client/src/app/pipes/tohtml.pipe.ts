import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
    name: 'toHtml',

})
export class TextToHtmlPipe implements PipeTransform {

    constructor(private sanitizer: DomSanitizer) { }

    transform(value?: string): SafeHtml {
        if (!value) {
            return this.sanitizer.bypassSecurityTrustHtml('');
        }

        const formattedText = value
            .split(/\n{2,}/)
            .filter(paragraph => paragraph.trim() !== '') // Remove empty paragraphs
            .map(paragraph => `<p>${paragraph.trim().replace(/\n/g, '<br>')}</p>`)
            .join('');

        return this.sanitizer.bypassSecurityTrustHtml(formattedText);
    }
}