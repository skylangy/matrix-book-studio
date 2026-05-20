import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';


@Injectable({
    providedIn: 'root',
})
export class ClipboardService {

    private logger: ILogger;

    constructor(loggingService: LoggingService) {
        this.logger = loggingService.getLogger('ClipboardService');
    }

    /**
     * Copies text to the clipboard.
     * @param text The text to copy.
     */
    async copyToClipboard(text: string): Promise<void> {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
            } else {
                // Fallback for browsers without modern clipboard API
                this.fallbackCopyToClipboard(text);
            }
        } catch (err) {
            this.logger.error('Failed to copy text to clipboard: ', err);
        }
    }

    /**
     * Reads text from the clipboard.
     * @returns A promise resolving to the clipboard contents or an empty string on error.
     */
    async readFromClipboard(): Promise<string> {
        try {
            if (navigator.clipboard && navigator.clipboard.readText) {
                return await navigator.clipboard.readText();
            }
            this.logger.warn('Clipboard API not supported.');
            return '';
        } catch (err) {
            this.logger.error('Failed to read clipboard contents: ', err);
            return '';
        }
    }

    /**
     * Fallback for copying text using a temporary textarea element.
     * @param text The text to copy.
     */
    private fallbackCopyToClipboard(text: string): void {
        const textarea = document.createElement('textarea');
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        textarea.value = text;
        document.body.appendChild(textarea);
        textarea.select();
        try {
            document.execCommand('copy');
        } catch (err) {
            this.logger.error('Fallback: Failed to copy text to clipboard: ', err);
        } finally {
            document.body.removeChild(textarea);
        }
    }
}
