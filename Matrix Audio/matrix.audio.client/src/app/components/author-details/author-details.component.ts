import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { AlbumsComponent } from '../albums/albums.component';
import { PersonAvatarPipe } from '../../pipes/person.avatar.pipe';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { ArtistService } from '../../services/artist.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-author-details',
    templateUrl: './author-details.component.html',
    imports: [RouterModule, HeaderContentComponent, AlbumsComponent, PersonAvatarPipe, TextToHtmlPipe, TranslatePipe]
})
export class AuthorDetailsComponent implements OnInit {
    @Input() author?: Artist;

    constructor(
        private activatedRoute: ActivatedRoute,
        private router: Router,
        private artistService: ArtistService
    ) { }

    ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            let id = params['id'];
            if (!id)
                return;
            this.author = await this.artistService.getArtist(id)
        })
    }
}
