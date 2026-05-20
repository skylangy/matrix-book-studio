
import { Injectable } from '@angular/core';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging-services';


@Injectable({
    providedIn: 'root'
})
export class HighlightService {
    private logger: ILogger;
    private highlightDecorations: any | undefined = undefined;
    private maxParagraphLength = 3000;

    constructor(loggingService: LoggingService) {
        this.logger = loggingService.getLogger('HighlightService');
    }

    highlightLongParagraph(editor: any) {
        let model = editor.getModel();
        let content: string = model.getValue();
        if (content.length < this.maxParagraphLength) {
            return;
        }

        if (!this.highlightDecorations) {
            this.highlightDecorations = editor.createDecorationsCollection([]);
        }

        let paragraphs: string[] = content.split(/\n+/gm);

        let decorations: any[] = [];
        for (let paragraph of paragraphs) {
            if (paragraph.length > this.maxParagraphLength) {
                const start = content.indexOf(paragraph);
                const end = start + paragraph.length;
                const startLineNumber = model.getPositionAt(start).lineNumber;
                const endLineNumber = model.getPositionAt(end).lineNumber;

                console.log('start:', start, 'end:', end, 'startLineNumber:', startLineNumber, 'endLineNumber:', endLineNumber);
                decorations.push({
                    range: {
                        startLineNumber: startLineNumber,
                        startColumn: 1,
                        endLineNumber: endLineNumber,
                        endColumn: 1
                    },
                    options: {
                        isWholeLine: true,
                        className: 'long-paragraph-highlight'
                    }
                });
            }
        }

        this.logger?.log('Long paragraphs:', decorations);
        this.highlightDecorations.set(decorations);

        return decorations[0];
    }
}