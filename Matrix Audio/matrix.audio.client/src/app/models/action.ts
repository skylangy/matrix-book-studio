export interface Action {
    name?: string;
    description?: string;
    icon?: string;
    size?: string;
    getIcon?: (arg?: any) => string;
    isEnable?: (arg?: any) => boolean;
    action?: (arg?: any) => void;
}
