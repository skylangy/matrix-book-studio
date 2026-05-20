import { SpeechModel } from './speech-model';

export interface NarrationBlock {
    id?: string;
    type?: string;
    order?: number;
    speech?: SpeechModel;
    content?: string;
    contentFontFamily?: string;
    contentFontSize?: number;
    contentColor?: string;
}

export interface Narration {
    id: string;
    title: string;
    template?: string;
    category?: string;
    tags?: string;
    blocks: NarrationBlock[];
    dateCreated?: Date;
    dateUpdated?: Date;
    status?: string;
    speechPrimary?: SpeechModel;
    speechSecondary?: SpeechModel;
}

export class NarrationBlockType {
    static readonly Statement = 'Statement';
    static readonly Note = 'Note';

    static readonly Types = [
        NarrationBlockType.Statement,
        NarrationBlockType.Note
    ];
}