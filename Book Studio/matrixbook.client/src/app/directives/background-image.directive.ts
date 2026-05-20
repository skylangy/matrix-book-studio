
import { CommonModule } from '@angular/common';
import { AfterViewInit, Directive, ElementRef, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[backgroundImage]',
})
export class BackgroundImageDirective implements AfterViewInit {
    private imageUrl?: string = '';
    private filter = false;
    private useGradient = false;

    constructor(
        private renderer: Renderer2,
        private elementRef: ElementRef) {

    }

    @Input()
    get backgroundImage(): string | undefined {
        return this.imageUrl;
    }
    set backgroundImage(value: string | undefined) {
        if (this.imageUrl !== value) {
            this.imageUrl = value;
            this.updateBackground();
        }
    }

    @Input()
    get enableFilter(): boolean {
        return this.filter;
    }
    set enableFilter(value: boolean) {
        if (this.filter !== value) {
            this.filter = value;
            this.updateBackground();
        }
    }

    @Input()
    get gradient(): boolean {
        return this.useGradient;
    }
    set gradient(value: boolean) {
        if (this.useGradient !== value) {
            this.useGradient = value;
            this.updateBackground();
        }
    }


    ngAfterViewInit() {
        // this.updateBackground();
    }

    private updateBackground() {
        let styles: { [key: string]: any } = {};

        if (this.backgroundImage && this.backgroundImage.length > 0) {
            styles = {
                // 'backgroundImage': `url('${this.backgroundImage}')`,
                'background': this.useGradient ? ` linear-gradient(180deg, transparent , transparent, rgb(33, 37, 41)), url('${this.backgroundImage}')`
                    : `url('${this.backgroundImage}')`,
                'backgroundPosition': 'center',
                'backgroundSize': 'cover',
                'background-repeat': 'no-repeat',
                'background-blend-mode': 'normal',
                // 'filter': 'blur(3px)',
                // 'backdrop-filter': 'blur(10px)'
            }

            if (this.enableFilter === true) {
                styles['background-color'] = '#888787d1';
                styles['background-blend-mode'] = 'multiply';
            }
        }

        Object.keys(styles).forEach(newStyle => {
            this.renderer.setStyle(this.elementRef.nativeElement, `${newStyle}`, styles[newStyle]);
        });
    }
}
