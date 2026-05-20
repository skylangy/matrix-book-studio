import { Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';
import { AlbumService } from '../services/album.service';
import { ImageService } from '../services/image.service';

@Directive({
    selector: '[lazyImage]'
})
export class LazyImageDirective implements OnInit {
    @Input('lazyImage') src?: string;
    @Input() placeholder: string = '';

    constructor(
        private readonly el: ElementRef<HTMLImageElement>,
        private readonly renderer: Renderer2,
        private readonly imageService: ImageService,
        private readonly albumService: AlbumService) {
        this.placeholder = this.imageService.defaultArtistAvatarUrl;
    }

    ngOnInit(): void {
        const img = this.el.nativeElement;
        this.renderer.setAttribute(img, 'src', this.placeholder);

        if (this.src) {
            // console.log('Loading image:', this.src);
            const image = new Image();
            let src = this.albumService.getImageUrl(this.src);
            image.src = src;
            image.onload = () => {
                this.renderer.setAttribute(img, 'src', src);
            };
        }
    }
}
