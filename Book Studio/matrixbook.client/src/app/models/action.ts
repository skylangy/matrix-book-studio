export interface Action {
    id: string;
    name: string;
    description?: string;
    icon?: string;
    enabled?: boolean;
    visible?: boolean;
    execute: () => void;
}

