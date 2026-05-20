
import { Component, OnInit } from '@angular/core';
import { Notification } from '../../models/notification';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'mtx-notification',
    templateUrl: './notification.component.html',
    imports: [CommonModule]
})
export class NotificationComponent implements OnInit {
    notification: Notification | undefined;
    isVisible = false;

    constructor() { }

    ngOnInit(): void {
        this.notification = {
            id: 1,
            title: 'Well done!',
            message: `p>Aww yeah, you successfully read this important alert message. This example text is going to run a bit longer so
            that you can see how spacing within an alert works with this kind of content.</p>
        <hr>
        <p class="mb-0">Whenever you need to, be sure to use margin utilities to keep things nice and tidy.</p>
    `,
            startDate: new Date(2024, 4, 12),
            expirationDate: new Date(2024, 4, 15),
        };

        this.isVisible = this.updateVisible();
    }

    updateVisible(): boolean {
        if (this.notification && this.notification.startDate && this.notification.expirationDate) {
            const now = new Date();

            let result = now >= this.notification.startDate && now <= this.notification.expirationDate;
            return result;
        }
        return false;
    }
}
