export interface NavItem {
    name?: string;
    text?: string;
    icon?: string;
    route?: string;
    group?: string;
    isActive?: boolean;
    context?: any
    isSeparator?: boolean;
    action?: () => void;
}

export interface GroupedNavItem {
    target?: string;
    title?: string;
    isActive?: boolean;
    items?: NavItem[];
}