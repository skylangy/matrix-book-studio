
import { Injectable } from '@angular/core';
import { SpeechConfig, SpeechLanguage } from '../models/speech-config';
import { BookService } from './book-service';

@Injectable({
    providedIn: 'root'
})
export class VoiceService {
    private Icons = {
        Male: "person-standing",
        Female: "person-standing-dress",
    };
    private isLoaded: boolean = false;

    speechConfig?: SpeechConfig;
    langulages?: SpeechLanguage[];
    defaultService: string = 'Edge';
    defaultLanguage: string = 'zh-CN';
    defaultVoice: string = 'zh-CN-YunjianNeural';

    azureService = 'Azure';
    azureLanguage = 'zh-CN';
    azureVoice = 'zh-CN-YunzeNeural';

    constructor(private readonly bookService: BookService) {

    }

    async getSpeechConfig(): Promise<SpeechConfig> {
        await this.load();
        return this.speechConfig!;
    }

    async load(): Promise<void> {
        if (this.isLoaded) {
            return;
        }

        this.speechConfig = await this.bookService.getSpeechConfig();
        this.isLoaded = true;
    }
}