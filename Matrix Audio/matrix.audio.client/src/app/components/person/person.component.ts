import { Component, Input, OnInit } from '@angular/core';
import { Artist } from '../../models/artist';
import { RouterModule } from '@angular/router';
import { PersonAvatarPipe } from '../../pipes/person.avatar.pipe';

@Component({
    selector: 'mtx-person',
    templateUrl: './person.component.html',
    imports: [RouterModule, PersonAvatarPipe]
})
export class PersonComponent implements OnInit {

    @Input() author?: Artist;

    constructor() { }

    ngOnInit(): void { }
}
