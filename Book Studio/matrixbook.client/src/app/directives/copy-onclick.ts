import { Directive, ElementRef, HostListener } from '@angular/core';
import { NotificationService } from '../services/notification-service';

@Directive({
    selector: '[appCopyOnClick]'
})
export class CopyOnClickDirective {
    constructor(private el: ElementRef,
        private notificationService: NotificationService,
    ) { }

    @HostListener('click')
    copyText() {
        const text = this.el.nativeElement.innerText || this.el.nativeElement.textContent;
        if (text) {
            navigator.clipboard.writeText(text.trim()).then(() => {
                this.notificationService.showSuccess('Text Copied', 'The text has been copied to your clipboard.');
            }).catch(err => {
                this.notificationService.showFail('Copy Failed', 'Failed to copy text to clipboard.');
            });
        }
    }
}