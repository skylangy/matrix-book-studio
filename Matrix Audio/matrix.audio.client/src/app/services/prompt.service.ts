import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { Prompt } from '../models/prompt';

@Injectable({
    providedIn: 'root'
})
export class PromptService {
    displayPrompt = new Subject<Prompt>();

    constructor() { }

    /**
     * Shows a prompt dialog to the user.
     * @param message The message to display in the prompt dialog.
     * @param defaultValue The default value to display in the input field.
     * @returns The user's input, or null if the user cancels the prompt.
     */
    showPrompt(prompt: Prompt) {
        this.displayPrompt.next(prompt);
    }

    showSuccess(title: string, message: string) {
        this.showPrompt({ title, message, type: 'success' });
    }

    showError(title: string, message: string) {
        this.showPrompt({ title, message, type: 'danger' });
    }

    showWarning(title: string, message: string) {
        this.showPrompt({ title, message, type: 'warning' });
    }
}