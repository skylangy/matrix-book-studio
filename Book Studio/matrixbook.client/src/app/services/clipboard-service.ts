
import { Injectable } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { IClipboardService } from '../models/clipboard-service';

@Injectable({
    providedIn: 'root'
})
export class ClipboardService implements IClipboardService {
    // constructor(private clipboard: Clipboard) { }

    async read(): Promise<string> {
        try {
            const text = await navigator.clipboard.readText();
            return text;

        } catch (err) {
            return '';
        }
    }
}