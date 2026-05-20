import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ILogger } from 'src/app/models/logger';
import { NamedValue } from 'src/app/models/named-value';
import { SpeechConfig, SpeechLanguage, SpeechServiceConfig } from 'src/app/models/speech-config';
import { LoggingService } from 'src/app/services/logging-services';
import { VoiceService } from 'src/app/services/voice-service';

@Component({
    selector: 'mtx-voice-selector',
    templateUrl: './voice.selector.component.html'
})
export class VoiceSelectorComponent implements OnInit {
    @Input() orientation: 'vertical' | 'horizontal' = 'vertical';
    @Output() serviceChange = new EventEmitter<string>();
    @Output() languageChange = new EventEmitter<string>();
    @Output() voiceChange = new EventEmitter<string>();

    private _selectedLanguage?: SpeechLanguage
    private _selectedVoice?: NamedValue<string>;
    private _selectedService?: SpeechServiceConfig;
    private serviceName?: string;
    private languageName?: string;
    private voiceName?: string;

    speechConfig?: SpeechConfig;
    langulages?: SpeechLanguage[];
    private logger?: ILogger;

    constructor(private voiceService: VoiceService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('VoiceSelectorComponent');
        this.logger.enabled = true;
    }

    get selectedService(): SpeechServiceConfig | undefined {
        return this._selectedService;
    }
    set selectedService(value: SpeechServiceConfig | undefined) {
        this.logger?.log('selectedService value', value);
        if (this._selectedService !== value) {
            this._selectedService = value;
        }
    }

    get selectedLanguage(): SpeechLanguage | undefined {
        return this._selectedLanguage;
    }
    set selectedLanguage(value: SpeechLanguage | undefined) {
        this.logger?.log('selectedLanguage value', value);
        if (this._selectedLanguage !== value) {
            this._selectedLanguage = value;
            this.language = value?.value || '';
        }
    }

    get selectedVoice(): NamedValue<string> | undefined {
        return this._selectedVoice;
    }
    set selectedVoice(value: NamedValue<string> | undefined) {
        this.logger?.log('selectedVoice value', value);
        if (this._selectedVoice !== value) {
            this._selectedVoice = value;
            this.voice = value?.value || '';
        }
    }

    @Input()
    get service(): string | undefined {
        return this.serviceName;
    }
    set service(value: string | undefined) {
        this.logger?.log('service value', value, this.serviceName);
        if (value && (this.serviceName !== value || !this.selectedService)) {
            this.serviceName = value;
            this.selectedService = this.speechConfig?.services.find(x => x.name === value);
            this.serviceChange.emit(value);
        }
    }

    @Input()
    get language(): string | undefined {
        return this.languageName;
    }
    set language(value: string | undefined) {
        this.logger?.log('language value', value, this.languageName, this.selectedLanguage);
        if (value && (this.languageName !== value || !this.selectedLanguage)) {
            this.languageName = value;
            this.selectedLanguage = this.langulages?.find(x => x.value === value);
            this.languageChange.emit(value);
        }
    }

    @Input()
    get voice(): string | undefined {
        return this.voiceName;
    }
    set voice(value: string | undefined) {
        this.logger?.log('voice value', value, this.voiceName);
        if (value && (this.voiceName !== value || !this.selectedVoice)) {
            this.voiceName = value;
            this.selectedVoice = this._selectedLanguage?.voices.find(x => x.value === value);
            this.voiceChange.emit(value);
        }
    }

    async ngOnInit() {
        this.speechConfig = await this.voiceService.getSpeechConfig();
        if (!this.speechConfig) {
            this.logger?.log('SpeechConfig is not loaded');
            return;
        }

        this.logger?.log('SpeechserviceConfig', this.speechConfig);

        if (!this.serviceName)
            this.serviceName = this.voiceService.defaultService;
        if (this.service || !this.selectedService)
            this._selectedService = this.speechConfig?.services.find(x => x.name === this.service);

        this.logger?.log('selectedService', this.serviceName, this.selectedService);

        this.langulages = this.selectedService?.languages || [];

        if (this.language || !this.selectedLanguage)
            this._selectedLanguage = this.langulages?.find(x => x.value === this.language);

        this.logger?.log('selectedLanguage', this.languageName, this._selectedLanguage);
        this.logger?.log('voices', this.voiceName, this.selectedVoice);

        if (!this.voiceName)
            this.voiceName = this.voiceService.defaultVoice;

        if (this.voice || !this.selectedVoice)
            this._selectedVoice = this._selectedLanguage?.voices.find(x => x.value === this.voiceName) || this._selectedLanguage?.voices[0];

        this.logger?.log('selectedVoice', this.voiceName, this.selectedVoice);
    }

    setService(service: SpeechServiceConfig): void {
        if (this.serviceName === service.name)
            return;

        this.logger?.log('setService', service);
        this.service = service.name;
        this.selectedService = service;

        this.langulages = service.languages;
        let language = service.languages.find(x => x.value === service.language);
        this.selectLanguage(language!);
    }

    selectLanguage(language: SpeechLanguage): void {
        this.selectedLanguage = language;
        this.language = language.value;

        this.logger?.log('selectLanguage', this.selectedService);
        let voice = language?.voices.find(x => x.value === this.selectedService?.voiceName) || language.voices[0];

        this.selectVoice(voice);
    }

    selectVoice(voice: NamedValue<string>): void {
        this.selectedVoice = voice;
        this.voice = voice.value;
    }
}
