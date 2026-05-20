import { Component, OnInit } from '@angular/core';
import { HeaderContentComponent } from '../header-content/header-content.component';
import { AlbumsComponent } from '../albums/albums.component';
import { AuthService } from '../../services/auth.service';
import { SigninPromptComponent } from '../signin-prompt/signin-prompt.component';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-library',
    templateUrl: './library.component.html',
    imports: [HeaderContentComponent, AlbumsComponent, SigninPromptComponent, TranslatePipe]
})
export class LibraryComponent implements OnInit {
    constructor(private authService: AuthService) { }

    ngOnInit(): void { }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }
}
