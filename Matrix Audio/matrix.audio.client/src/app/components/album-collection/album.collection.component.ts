import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AlbumCollection } from '../../models/album';
import { AppSettingService } from '../../services/appsetting.service';
import { RouterModule } from '@angular/router';
import { LazyLoadImageDirective } from '../../pipes/lazyload.pipe';
import { CommonModule } from '@angular/common';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { SummaryPipe } from '../../pipes/summary.pipe';
import { CardLayout } from '../../models/views';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';

@Component({
    selector: 'mtx-album-collection',
    templateUrl: 'album.collection.component.html',
    imports: [RouterModule, CommonModule, ImageUrlPipe,
        LazyLoadImageDirective, SummaryPipe, TextToHtmlPipe]
})
export class AlbumCollectionComponent implements OnInit {
    @Input() albumCollection?: AlbumCollection;
    @Input() cardLayout: CardLayout = 'card';
    @Output() albumLoaded = new EventEmitter<AlbumCollection>();

    constructor(
        private readonly appSettingService: AppSettingService,
    ) { }

    ngOnInit() {
        this.albumLoaded.emit(this.albumCollection);
    }

    get placeholder(): string {
        return this.appSettingService.defaultSplash;
    }

    playAlbumCollection() {
    }
}