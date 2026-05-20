import { INotification } from "./notification";

export interface INotificationService {
    showNotification(notification: INotification): void;

    showSuccess(title?: string, content?: string): void;

    showFail(title?: string, content?: string): void;
}