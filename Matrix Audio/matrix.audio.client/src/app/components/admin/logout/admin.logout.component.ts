import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'mtx-admin-logout',
    template: '',
    imports: [RouterModule]
})
export class AdminLogoutComponent implements OnInit {
    constructor(
        private readonly router: Router,
        private readonly authService: AuthService) { }

    ngOnInit(): void {
        this.authService.logout();
        this.router.navigate(['/tower/login']);
    }
}
