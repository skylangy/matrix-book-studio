import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { FootPlayerComponent } from '../foot-player/foot-player.component';
import { AppbarComponent } from '../appbar/appbar.component';
import { HeaderComponent } from '../header/header.component';
import { PromptComponent } from '../prompt/prompt.component';

@Component({
    selector: 'mtx-home',
    templateUrl: './home.component.html',
    imports: [
        RouterOutlet,
        SidebarComponent,
        FootPlayerComponent,
        HeaderComponent,
        AppbarComponent,
        PromptComponent
    ]
})
export class HomeComponent implements OnInit {

    constructor() {

    }

    async ngOnInit() {

    }

}
