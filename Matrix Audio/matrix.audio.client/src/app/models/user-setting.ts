export interface UserSetting {
    id?: string;
    name?: string;
    value?: string;
}

export interface UserSettings {
    id?: string;
    userId?: string;
    settings?: UserSetting[];
}