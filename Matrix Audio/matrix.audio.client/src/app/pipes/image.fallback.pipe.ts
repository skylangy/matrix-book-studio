
import { Directive, ElementRef, Renderer2 } from '@angular/core';
import { AppSettingService } from '../services/appsetting.service';

@Directive({
    selector: 'img[defaultFallback]'
})
export class ImageFallbackDirective {

    constructor(private el: ElementRef,
        private renderer: Renderer2,
        private appSettingService: AppSettingService) {
        const imgElement = this.el.nativeElement;
        imgElement.onerror = () => this.setFallbackImage();
    }

    private setFallbackImage(): void {
        this.el.nativeElement.src = this.appSettingService.defaultSplash;
    }
}