import { Injectable } from '@angular/core';
import { IMemoryService } from '../models/memory-service';

@Injectable({
    providedIn: 'root'
})
export class MemoryService implements IMemoryService {
    private memory: Map<string, any> = new Map<string, any>();

    constructor() { }

    public get<T>(key: string): T | undefined {
        return this.memory.get(key);
    }

    public set<T>(key: string, value: T): void {
        this.memory.set(key, value);
    }
}