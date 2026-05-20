import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'mtx-error',
    templateUrl: 'error.component.html',
})

export class ErrorComponent implements OnInit {
    errorMessage: string | null = null;

    constructor(private route: ActivatedRoute, private router: Router) { }

    ngOnInit(): void {
        this.errorMessage = this.route.snapshot.queryParamMap.get('error');
    }

    goHome(): void {
        this.router.navigate(['/']);
    }
}