import { InjectionToken } from '@angular/core';

export const MONACO_EDITOR_CONFIG = new InjectionToken('MONACO_EDITOR_CONFIG');

export interface MonacoEditorConfig {
    baseUrl?: string;
    requireConfig?: { [key: string]: any; };
    defaultOptions?: { [key: string]: any; };
    monacoRequire?: Function;
    onMonacoLoad?: Function;
}

export interface EditorModel {
    value: string;
    language?: string;
    uri?: any;
}