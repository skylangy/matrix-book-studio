export interface IMemoryService {
    get<T>(key: string): T | undefined;
    set<T>(key: string, value: T): void
}