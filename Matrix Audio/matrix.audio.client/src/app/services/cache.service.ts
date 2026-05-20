import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class CacheService {
    private cache = new Map<string, any>();

    constructor() { }

    set(key: string, value: any, ttl: number = 300000): void {
        const expiry = new Date().getTime() + ttl;
        this.cache.set(key, { value, expiry });
    }

    get(key: string): any {
        const cached = this.cache.get(key);
        if (!cached) {
            return null;
        }

        const now = new Date().getTime();
        if (now > cached.expiry) {
            this.cache.delete(key);
            return null;
        }

        return cached.value;
    }

    clear(key: string): void {
        this.cache.delete(key);
    }

    clearAll(): void {
        this.cache.clear();
    }
}
