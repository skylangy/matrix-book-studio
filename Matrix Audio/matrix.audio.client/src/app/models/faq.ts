export interface Faq {
    id: string;
    question: string;
    answer: string;
    category: string;
    dateCreated: Date;
    dateUpdated: Date;
    isActive: boolean;
    tags: string[];
}