export interface Notification {
    id?: number;
    title?: string;
    message?: string;
    startDate?: Date;
    expirationDate?: Date;
    read?: boolean;
}