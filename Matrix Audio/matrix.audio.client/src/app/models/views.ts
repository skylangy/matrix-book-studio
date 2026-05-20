
export type AlbumLayout = 'grid' | 'list' | 'horizontal';

export type ArtistLayout = 'grid' | 'list' | 'horizontal';

export type CardLayout = 'card' | 'compact' | 'horizontal';

export type CategoryLayout = 'grid' | 'list' | 'horizontal' | 'card';

export type AlbumSource = 'recents' | 'likes' | 'suggested' | 'history' | 'favorites'
    | 'category' | 'tag' | 'search' | 'playlist' | 'downloads'
    | 'playByWeek' | 'playByMonth' | 'playByYear'
    | '';

export type AlbumCollectionSource = 'recents' | '';

export type ArtistSource = 'recents' | 'populars' | 'all';

export type CardSize = 'small' | 'medium' | 'large';

export type LoadingStyles = 'book' | 'step' | 'card' | 'spinner';