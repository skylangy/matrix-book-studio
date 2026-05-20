import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class LocalSettingService {

    public get<T>(key: string, defaultValue?: T): T | undefined {
        let value = localStorage.getItem(key);
        console.log(`get ${key} with ${value}`);
        if (value) {

            return JSON.parse(value);
        }
        return defaultValue;
    }

    public set<T>(key: string, value: T): void {
        console.log(`set ${key} to ${value}`);
        localStorage.setItem(key, JSON.stringify(value));
    }
}