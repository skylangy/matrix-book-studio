import { Component, Inject, Input, NgZone, OnDestroy, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import { fromEvent } from 'rxjs';
import { ILogger } from 'src/app/models/logger';
import { LoggingService } from 'src/app/services/logging-services';
import { BaseEditor } from './base.editor.component';
import { EditorModel, MONACO_EDITOR_CONFIG, MonacoEditorConfig } from './models';

declare var monaco: any;

@Component({
    selector: 'mtx-monaco-editor',
    templateUrl: './monaco.editor.component.html',
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => MonacoEditorComponent),
        multi: true
    }]
})
export class MonacoEditorComponent extends BaseEditor implements OnDestroy {

    private logger: ILogger;
    private _value: string = '';

    propagateChange = (_: any) => { };
    onTouched = () => { };

    constructor(private zone: NgZone,
        @Inject(MONACO_EDITOR_CONFIG) private editorConfig: MonacoEditorConfig,
        loggingService: LoggingService) {
        super(editorConfig);
        this.logger = loggingService.getLogger('MonacoEditor');
    }

    @Input('options')
    set options(options: any) {
        this._options = Object.assign({}, this.config.defaultOptions, options);
        if (this._editor) {
            this.initMonaco(options, this.insideNg);
        }
    }

    get options(): any {
        return this._options;
    }

    @Input('model')
    set model(model: EditorModel) {
        this.options.model = model;
        if (this._editor) {
            this.initMonaco(this.options, this.insideNg);
        }
    }

    get editor(): any {
        return this._editor;
    }

    protected initMonaco(options: any, insideNg: boolean): void {
        if (!options) {
            return;
        }
        this.logger.log('Initializing Monaco Editor with options:', options);
        const hasModel = !!options?.model;

        if (hasModel) {
            const model = monaco.editor.getModel(options.model.uri || '');
            if (model) {
                options.model = model;
                options.model.setValue(this._value);
            } else {
                options.model = monaco.editor.createModel(options.model.value, options.model.language, options.model.uri);
            }
        }

        if (!this._editor) {
            this.logger.log('Creating Monaco editor');
            if (insideNg) {
                this._editor = monaco.editor.create(this._editorContainer.nativeElement, options);
            } else {
                this.zone.runOutsideAngular(() => {
                    this._editor = monaco.editor.create(this._editorContainer.nativeElement, options);
                })
            }
        }
        this._editor.updateOptions(options);
        this.logger.log('Update Monaco editor value');
        if (!hasModel) {
            this._editor.setValue(this._value);
        }

        this._editor.onDidChangeModelContent((e: any) => {
            const value = this._editor.getValue();

            // value is not propagated to parent when executing outside zone.
            this.zone.run(() => {
                this.propagateChange(value);
                this._value = value;
            });
        });

        this._editor.onDidBlurEditorWidget(() => {
            this.onTouched();
        });

        // refresh layout on resize event.
        if (this._windowResizeSubscription) {
            this._windowResizeSubscription.unsubscribe();
        }

        this._windowResizeSubscription = fromEvent(window, 'resize').subscribe(() => this.layout());
        this.afterEditorInit.emit(this._editor);
    }

    writeValue(value: any): void {
        this._value = value || '';
        setTimeout(() => {
            if (this._editor && !this.options.model) {
                this._editor.setValue(this._value);
            }
        }, 100);
    }

    registerOnChange(fn: any): void {
        this.propagateChange = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    layout(width?: number, height?: number): void {
        if (this._editor) {

            this._editor.layout({ width: width, height: height });
            setTimeout(() => {
                this._editor.layout();
            }, 200);

        }
    }
}
