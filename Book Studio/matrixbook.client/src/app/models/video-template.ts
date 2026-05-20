

export interface Rect {
    x: number;
    y: number;
    width: number;
    height: number;
}

export interface Location {
    x: number;
    y: number;
}

export interface Size {
    width: number;
    height: number;
}

export interface Margin {
    top: number;
    right: number;
    bottom: number;
    left: number;
}

export interface Transition {
    effect: string;
    duration: number;
}

export const PropertyTypes = {
    text: 'Text',
    resource: 'Resource'
} as const;
export type PropertyType = (typeof PropertyTypes)[keyof typeof PropertyTypes];


export interface TemplateProperty {
    id: string;
    name: string;
    value: string;
    tag?: string;
    type: PropertyType;
}

export interface VideoTemplate {
    id: string;
    name: string;
    description: string;
    thumbnail: string;
    resolution: Size;
    dateCreated: Date;
    dateUpdated: Date;
    tracks: VideoTrack[];
    properties?: TemplateProperty[];
}

export interface VideoTrack {
    id: string;
    name: string;
    elements: MediaElement[];
}

export interface LayoutOptions {
    location?: Location;
    size?: Size;
    align?: Align;
    margin?: Margin;
}

export interface FontOptions {
    family: string;
    fontSize: number;
    weight: 'normal' | 'bold' | 'italic';
    color?: string;
    backgroundColor?: string;
    shadow?: boolean;
}

export const Alignments = {
    top: 'top',
    center: 'center',
    bottom: 'bottom',
    left: 'left',
    right: 'right',
    topLeft: 'topLeft',
    topRight: 'topRight',
    bottomLeft: 'bottomLeft',
    bottomRight: 'bottomRight',
    middleLeft: 'middleLeft',
    middleRight: 'middleRight'
} as const;
export type Align = (typeof Alignments)[keyof typeof Alignments];

export const DurationTypes = {
    fixed: 'Fixed',
    auto: 'Auto',
    full: 'Full'
} as const;
export type DurationType = (typeof DurationTypes)[keyof typeof DurationTypes];

export const StartTypes = {
    absolute: 'Absolute',
    afterPrevious: 'After Previous',
    beforeEnd: 'Before End'
} as const;
export type StartType = (typeof StartTypes)[keyof typeof StartTypes];

export const MediaTypes = {
    audio: 'audio',
    image: 'image',
    video: 'video',
    text: 'text',
    combine: 'combine'
} as const;
export type MediaType = (typeof MediaTypes)[keyof typeof MediaTypes];

export interface MediaElement {
    type: MediaType;
    name: string;
    start: number;
    duration: number;
    fadeIn: number;
    fadeOut: number;
    alpha: number;
    transition?: Transition;
    durationType?: DurationType;
    startType?: StartType;
    layout?: LayoutOptions;
    inputSource?: string;
    textContent?: string;
    children?: MediaElement[];
}
