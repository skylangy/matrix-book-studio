import { Entity } from "./entity";

export interface ImageResource extends Entity {
    width: number;
    height: number;
    fileName?: string | null;
    folderName?: string | null;
}