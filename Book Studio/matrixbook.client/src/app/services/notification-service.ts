import { Injectable } from '@angular/core';
import { INotification } from '../models/notification';
import { Subject } from 'rxjs';
import { INotificationService } from '../models/notification-service';

@Injectable({
    providedIn: 'root'
})
export class NotificationService implements INotificationService {
    noShowNotification = new Subject<INotification>();

    constructor() { }

    showNotification(notification: INotification): void {
        this.noShowNotification.next(notification);
    }

    showSuccess(title?: string, content?: string): void {
        let notification: INotification = {
            title: title,
            content: content,
            type: 'success',
            time: new Date().getTime() - 1000
        };

        this.showNotification(notification);
    }

    showFail(title?: string, content?: string): void {
        let notification: INotification = {
            title: title,
            content: content,
            type: 'warning',
            time: new Date().getTime() - 1000
        };

        this.showNotification(notification);
    }
}