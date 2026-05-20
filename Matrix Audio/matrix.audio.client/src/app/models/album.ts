import { Artist } from "./artist";
import { Entity } from "./entity";
import { Episode } from "./episode";

export interface Album extends Entity {

    title?: string;
    author?: string;
    authorId?: string;
    description?: string;
    imageWideSplash?: string;
    imageSquareSplash?: string;
    dateCreated?: Date;
    dateUpdated?: Date;
    status?: string; //draft, published, archived, deleted
    episodeCount?: number;
    duration?: number;
    tags?: string[];
    categories?: string[];
    likes?: number;
    commentCount?: number;
    downloadCount?: number;
    playCount?: number;
    durationInSeconds?: number;
    level?: number;
    artistId?: string;
    artistName?: string;

    artist?: Artist;
    episodes?: Episode[];

    isSelected?: boolean;
}

export interface AlbumCollection extends Entity {
    name?: string;
    description?: string;
    image?: string;
    count?: number;
    albums: Album[];
    albumIds?: { [key: string]: number };
    dateCreated?: Date;
    dateUpdated?: Date;
}