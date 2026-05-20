export interface MediaResource {
    id: string;
    type: string;
    name: string;
    url: string;
    fullUrl?: string;
    metadata?: any;
    isHidden?: boolean;
    isSelected?: boolean;
}

export interface MediaResourceGroup {
    name: string;
    resources: MediaResource[];
    subGroups?: MediaResourceGroup[];
}