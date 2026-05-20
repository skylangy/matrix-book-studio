import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { AlbumsComponent } from '../albums/albums.component';
import { PersonsComponent } from '../persons/persons.component';
import { CategoriesComponent } from '../categories/categories.component';

@Component({
    selector: 'mtx-search',
    templateUrl: './search.component.html',
    imports: [HeaderContentComponent, AlbumsComponent, PersonsComponent,
        CategoriesComponent
    ]
})
export class SearchComponent implements OnInit {
    searchText = '';

    constructor() { }

    ngOnInit(): void { }

    search() {
    }
}
