import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class LocalDataService {

    public get<T>(key: string, defaultValue?: T): T | undefined {
        let value = localStorage.getItem(key);

        if (value) {
            return JSON.parse(value);
        }
        return defaultValue;
    }

    public set<T>(key: string, value: T): void {
        if (value === undefined) {
            localStorage.removeItem(key);
            return;
        }
        localStorage.setItem(key, JSON.stringify(value));
    }

    public remove(key: string): void {
        localStorage.removeItem(key);
    }

    public clear(): void {
        localStorage.clear();
    }
}