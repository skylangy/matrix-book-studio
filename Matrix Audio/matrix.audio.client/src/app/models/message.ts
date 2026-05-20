import { Entity } from "./entity";

export interface Message extends Entity {
    subject: string;
    content: string;
    userId: string;
    dateCreated: Date;
}