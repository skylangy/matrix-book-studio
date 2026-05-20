export interface IBookAction {
    name?: string;
    description?: string;
    route?: string;
    label?: string;
    icon?: string;
    color?: string;
    visible?: boolean;
    func?: () => void;
}