export interface IModalAction {
    label?: string;
    style?: string;
    action?: () => boolean;
}

export interface IModal {
    title?: string;
    body?: string;
    icon?: string;
    buttons?: IModalAction[];
}