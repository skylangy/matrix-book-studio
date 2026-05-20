import { ViewportScroller } from '@angular/common';
import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class ViewService {
    constructor(private readonly viewportScroller: ViewportScroller) { }

    scrollToTopOnNavEnd(router: Router): void {
        router.events.subscribe((event) => {
            if (event instanceof NavigationEnd) {
                this.viewportScroller.scrollToPosition([0, 0]);
            }
        });
    }

    scrollToTop(): void {
        this.viewportScroller.scrollToPosition([0, 0]);
    }

    scrollToAnchor(anchor: string): void {
        this.viewportScroller.scrollToAnchor(anchor);
    }
}