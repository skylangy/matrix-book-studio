import { Component, OnInit } from '@angular/core';
import { PostSlideComponent } from '../post-slide/post-slide.component';
import { AlbumsComponent } from '../albums/albums.component';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { NotificationComponent } from '../notification/notification.component';
import { AuthService } from '../../services/auth.service';
import { ArtistsComponent } from '../artists/artists.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { AlbumCollectionsComponent } from '../album-collection/album.collections.component';

@Component({
    selector: 'mtx-welcome',
    templateUrl: './welcome.component.html',
    imports: [PostSlideComponent, AlbumsComponent,
        HeaderContentComponent, NotificationComponent,
        ArtistsComponent, TranslatePipe,
        AlbumCollectionsComponent
    ]
})
export class WelcomeComponent implements OnInit {
    pageSize = 18;

    constructor(private authService: AuthService) { }

    ngOnInit(): void { }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }
}
