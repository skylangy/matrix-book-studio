
export interface WorkProgress {
    timestamp?: Date;
    name?: string;
    description?: string;
    status?: string;
    category?: string;
    total?: number;
    current?: number;
    success?: boolean;
}