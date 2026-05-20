import { Component, EventEmitter, Input, NgZone, OnInit, Output, ViewChild, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from '../../services/logging-services';
import { MonacoEditorComponent } from '../monaco-editor/monaco.editor.component';
import { BookLanguage } from './book.language';

declare var monaco: any;

@Component({
    selector: 'mtx-text-editor',
    templateUrl: './text.editor.component.html',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => TextEditorComponent),
        multi: true
    }],
    imports: [MonacoEditorComponent, FormsModule]
})
export class TextEditorComponent implements OnInit, ControlValueAccessor {
    @ViewChild('editor') monacoEditor?: MonacoEditorComponent;

    @Input() options: any;
    @Output() onEditorReady = new EventEmitter<any>();
    @Output() onEditorContentChange = new EventEmitter<string>();

    value?: string = '';
    onChange: any = () => { };
    onTouched: any = () => { };

    bookLanguage = new BookLanguage();
    private logger?: ILogger;

    constructor(private zone: NgZone,
        private loggingService: LoggingService) {
        this.logger = this.loggingService?.getLogger('TextEditor');
        this.logger.enabled = false;
    }

    get editor() {
        return this.monacoEditor?.editor;
    }

    ngOnInit(): void {
        this.options = {};
    }

    writeValue(value?: any): void {
        this.value = value;
    }

    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    setDisabledState?(isDisabled: boolean): void {

    }

    layout(width?: number, height?: number) {
        this.monacoEditor?.layout(width, height);
    }

    afterEditorInit(editor: any) {
        this.zone.run(() => {
            this.onEditorReady.emit(editor);
        });

        this.bookLanguage.buildMarkers(editor);
        this.bookLanguage.initCommands(monaco.editor, editor);
    }

    beforeEditorInit(rawOptions: any) {
        this.bookLanguage = new BookLanguage();
        this.bookLanguage.initialize();

        this.options = {
            theme: this.bookLanguage.themeName,
            language: this.bookLanguage.languageName,
            fontSize: this.bookLanguage.fontSize,
            wordWrap: 'on',
            value: this.value,
            unicodeHighlight: {
                ambiguousCharacters: false,
                invisibleCharacters: false,
            }
        };

        rawOptions = Object.assign({}, rawOptions, this.options)

        this.monacoEditor!.options = rawOptions;
    }
}
