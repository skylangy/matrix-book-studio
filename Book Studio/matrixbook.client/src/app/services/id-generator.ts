import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class IdGeneratorService {
    constructor() { }

    generateId(prefix = 'id'): string {
        return prefix + '-' + Math.random().toString(36).substring(2, 16);
    }
}