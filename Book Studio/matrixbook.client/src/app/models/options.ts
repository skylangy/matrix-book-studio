
export interface ValueInfo {
    value: any;

}

export interface OptionItem {
    id: string;
    name: string;
    displayName: string;
    group?: string;
    value?: any;
    valueType?: string;
}

export interface OptionGroup {
    groupName: string;
    items: OptionItem[];
}

export interface IOptions {
    items: OptionItem[];
}