import { Injectable } from '@angular/core';
import {
    CtrlAltVHandler, CtrlJHandler, CtrlNHandler, CtrlOHandler,
    CtrlQHandler, CtrlSHandler, KeyHandler, VideoCtrlSHandler
} from '../models/key-handlers';


@Injectable({
    providedIn: 'root'
})
export class KeyService {

    get bookEditorName(): string {
        return 'BookEditor';
    }

    get videoEditorName(): string {
        return 'VideoEditor';
    }

    get narrationEditorName(): string {
        return 'NarrationEditor';
    }

    private editorKeyHandlers: Map<string, KeyHandler[]> = new Map([
        [this.bookEditorName, [
            new CtrlSHandler(),
            new CtrlQHandler(),
            new CtrlNHandler(),
            new CtrlJHandler(),
            new CtrlOHandler(),
            new CtrlAltVHandler()
        ]],
        [this.videoEditorName, [
            new VideoCtrlSHandler()
        ]]
    ]);

    handleKeyDown(event: KeyboardEvent, context?: any, editorType: string = 'BookEditor'): void {
        const handlers = this.editorKeyHandlers.get(editorType) || [];
        for (let handler of handlers) {
            if (handler.canHandle(event)) {
                handler.handleKeyDown(event, context);
                break;
            }
        }
    }
}