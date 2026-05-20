import { Album } from './album';
import { Artist } from './artist';

export interface Episode {
    id?: string;
    title?: string;
    content?: string;
    path?: string;
    duration?: number;
    fileLength?: number;
    image?: string;
    dateUpdated?: Date;

    albumId?: string;
    albumTitle?: string;
    artistId?: string;
    artistName?: string;

    album?: Album;
    artist?: Artist;

    comments?: number;
    downloads?: number;
    plays?: number;

    progress?: number;
}