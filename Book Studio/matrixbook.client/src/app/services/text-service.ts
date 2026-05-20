import { Injectable } from '@angular/core';
import { ITextTransformService } from '../models/text-transform-service';
import * as OpenCC from 'opencc-js';

@Injectable({
    providedIn: 'root'
})
export class TextTransformService implements ITextTransformService {
    toSimplify(text: string): string {
        if (!text) {
            return '';
        }

        let content = OpenCC.Converter({ from: 'hk', to: 'cn' })(text);
        return content;
    }
}