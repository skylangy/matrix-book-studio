import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { INotification } from 'src/app/models/notification';
import { NotificationService } from 'src/app/services/notification-service';
import { TimeAgoPipe } from '../../directives/time-ago.pipe';
import { NgClass } from '@angular/common';

@Component({
    selector: 'mtx-notification',
    templateUrl: './notification.component.html',

    imports: [NgClass, TimeAgoPipe],
})
export class NotificationComponent implements OnInit, OnDestroy {
    @Input() notification?: INotification;
    @Input() delay: number = 10000;

    showToast: boolean = false;
    private subscription?: Subscription;

    constructor(private notificationService: NotificationService) { }

    ngOnInit() {
        this.subscription = this.notificationService.noShowNotification.subscribe(notification => {
            this.notification = notification;
            this.showToast = true;

            setTimeout(() => {
                this.showToast = false;
            }, this.delay);
        });
    }

    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    closeToast() {
        this.showToast = false;
    }
}
