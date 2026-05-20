import { NamedValue } from "./named-value";

export interface SpeechConfig {
    services: SpeechServiceConfig[];
}

export interface SpeechServiceConfig {
    name: string;
    language: string;
    voiceName: string;
    languages: SpeechLanguage[];
}

export interface SpeechLanguage {
    name: string;
    value: string;
    voices: SpeechVoice[];
}

export interface SpeechVoice {
    name: string;
    value: string;
    tag: string;
}