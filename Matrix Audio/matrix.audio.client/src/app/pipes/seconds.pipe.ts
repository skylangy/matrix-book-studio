import { Pipe, PipeTransform } from '@angular/core';
import { ValueService } from '../services/value.service';

@Pipe({ name: 'toTime', })
export class SecondsToTimePipe implements PipeTransform {

    constructor(private valueService: ValueService) { }

    transform(value?: number): string {
        return this.valueService.formatSeconds(value);
    }

    private padZero(value: number): string {
        return value < 10 ? '0' + value : '' + value;
    }
}