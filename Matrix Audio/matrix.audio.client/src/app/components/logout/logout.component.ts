import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'mtx-logout',
    template: '',
    imports: [RouterModule]
})
export class LogoutComponent implements OnInit {
    constructor(
        private readonly router: Router,
        private readonly authService: AuthService) { }

    ngOnInit(): void {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}
