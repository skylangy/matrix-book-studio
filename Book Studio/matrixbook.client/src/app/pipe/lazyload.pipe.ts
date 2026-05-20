import { Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';

@Directive({
    selector: '[mtxLazyLoadImage]',

})
export class LazyLoadImageDirective implements OnDestroy {
    @Input('mtxLazyLoadImage') src: string = '';
    private observer: IntersectionObserver | undefined = undefined;

    constructor(private el: ElementRef, private renderer: Renderer2) {
        this.initObserver();
    }

    private initObserver(): void {
        this.observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.loadImage();
                    observer.unobserve(entry.target);
                }
            });
        });
        this.observer.observe(this.el.nativeElement);
    }

    private loadImage(): void {
        this.renderer.setAttribute(this.el.nativeElement, 'src', this.src);
    }

    ngOnDestroy(): void {
        if (this.observer) {
            this.observer.disconnect();
        }
    }
}