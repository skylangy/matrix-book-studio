export interface EditorPane {
    name?: string;
    title?: string;
    icon?: string;
    location?: string;
    context?: any;
    tag?: any;
    closeEvent?: () => void;
    onLoad?: () => void;
}