import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NavigationProviderService } from '../../services/navigation.service';

@Component({
    selector: 'mtx-appbar',
    templateUrl: './appbar.component.html',
    imports: [RouterModule]
})
export class AppbarComponent implements OnInit {

    constructor(private readonly navProviderService: NavigationProviderService) { }

    ngOnInit(): void { }

    get navs() {
        return this.navProviderService.getAppNavigation();
    }
}
