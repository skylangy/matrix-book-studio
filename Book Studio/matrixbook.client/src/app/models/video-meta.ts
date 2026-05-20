import { MediaResource } from "./media-resource";

export interface VideoMeta {
    id: string;
    name?: string;
    title?: string;
    subtitle?: string;
    content?: string;
    bottomNote?: string;
    category?: string;
    tag?: string;
    status?: string;
    speechService: string;
    language: string;
    voiceName: string;
    contentFontFamily?: string;
    contentFontSize?: number;

    introImage?: MediaResource;
    outroImage?: MediaResource;
    contentImages?: MediaResource[];
    introAudio?: MediaResource;
    logo?: VideoLogo;
    duration?: number;
    width?: number;
    height?: number;
    dateCreated?: Date;
    dateUpdated?: Date;
}


export interface VideoLogo {
    image?: MediaResource;
    text?: string;
    fontFamily?: string;
    fontSize?: number;
    shadow?: boolean;
}

export class VideoResources {
    static bibleLogo: VideoLogo = {
        image: { id: 'e052130b-f2f8-4b7e-86fe-7f00459f66a3', name: 'bible-logo.png', type: 'image', url: 'Image\\bible_logo.png' },
        fontSize: 36,
        fontFamily: '方正启体繁体',
        text: '恩典笔记',
        shadow: true
    };

    static defaultIntroImage: MediaResource = {
        id: 'dd5e5a97-e338-4f78-ad2b-c47744fd34be',
        name: 'Youtube-Short-Intro.jpg',
        type: 'image',
        url: 'Image\\Youtube-Short-Intro.jpg'
    };

    static defaultOutroImage: MediaResource = {
        id: '0aed0037-0a51-4c8b-b941-389a9dd7b89c',
        name: 'Youtube-Short-Outro.jpg',
        type: 'image',
        url: 'Image\\Youtube-Short-Outro.jpg'
    };

    static defaultIntroAudio: MediaResource = {
        id: 'e6a07a8a-32c5-48c7-b600-1e98958f055d',
        name: 'ding.mp3',
        type: 'audio',
        url: 'Audio\\ding.mp3'
    };

    static bibleSubtitle = 'Verse of the Day';
    static bibleNotes: string[] = [
        'Let His Word guide your day',
        'God’s Word is a lamp to your feet',
        'Daily bread for your soul',
        'Be still and know that He is God',
        'Start your day with Scripture.',
        'Meditate on this verse today',
        'Share this verse with someone who needs it',
        'Pause and reflect',
        'Let this truth sink into your heart.',
        'A whisper from heaven for your day.',
        'One verse can change everything.',
        'Truth never fades.',
        'He speaks still. Listen.',
        // Faith & Presence
        "His Word still speaks in the silence.",
        "Breathe in truth. Exhale grace.",
        "He is near in every word.",
        "A quiet whisper from eternity.",
        "Sacred words for a noisy world.",

        // Light & Hope
        "Let light rise from every line.",
        "Hope wrapped in holy letters.",
        "Even the darkest night bows to truth.",
        "This verse carries the morning.",
        "Let His light find you here.",

        // Peace & Assurance
        "Rest in the rhythm of His voice.",
        "Peace flows from promised words.",
        "When hearts tremble His Word stands.",
        "Let your soul lean on this.",
        "Be still — this is eternal.",

        // Eternal & Powerful
        "Ancient words. Eternal truth.",
        "Spoken once echoing forever.",
        "One verse Endless depth.",
        "Let this truth mark your day.",
        "He writes on hearts not pages."
    ];

    static getBibleNote(): string {
        const randomIndex = Math.floor(Math.random() * this.bibleNotes.length);
        return this.bibleNotes[randomIndex];
    }
}