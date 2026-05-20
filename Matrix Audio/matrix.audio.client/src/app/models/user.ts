
export interface User {
    id?: string;
    firstName?: string;
    lastName?: string;
    email?: string;
    name?: string;
    imageId?: string;
    bio?: string;
    level?: number;
    readBooks?: number
    dateCreated?: Date;
    dateUpdated?: Date;
    dateLastLogin?: Date;
    isLocked?: boolean;
}

export interface UserSubscription {
    startDate?: Date;
    endDate?: Date;
    isActive?: boolean;
    name?: string;
    period?: number;
    subscriptionId?: string;
}