import { Album } from "./album";

export interface Artist {
    id?: string;
    name?: string;
    description?: string;
    image?: string;
    albums?: Album[];
    albumsCount?: number;
    isHidden?: boolean;
    dateCreated?: Date;
}