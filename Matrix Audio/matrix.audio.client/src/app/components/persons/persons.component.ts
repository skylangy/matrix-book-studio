import { Component, OnInit } from '@angular/core';
import { PersonComponent } from '../person/person.component';
import { Artist } from '../../models/artist';
import { ArtistService } from '../../services/artist.service';

@Component({
    selector: 'mtx-persons',
    templateUrl: './persons.component.html',
    imports: [PersonComponent]
})
export class PersonsComponent implements OnInit {
    authors: Artist[] = [];

    constructor(private artistService: ArtistService) { }

    async ngOnInit() {
        this.authors = await this.artistService.getRecentArtists();
    }
}
