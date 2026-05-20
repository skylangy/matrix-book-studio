import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '../services/translate.service';

@Pipe({
    name: 'translate',
    pure: false,
})
export class TranslatePipe implements PipeTransform, OnDestroy {
    private promise: Promise<string> | null = null;
    private lastKey: string | null = null;
    private lastValue: string | null = null;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly translateService: TranslateService
    ) { }

    ngOnDestroy(): void {
        this.promise = null;
    }

    transform(value: string): string {
        if (this.lastKey == value) {
            return this.lastValue || value;
        }

        this.lastKey = value;
        this.promise = this.getTranslation(value);
        this.promise.then(result => {
            this.lastValue = result;
            this.changeDetector.markForCheck();
        });

        return this.lastValue || value;
    }

    private async getTranslation(value: string): Promise<string> {
        return this.translateService.translate(value);
    }
}