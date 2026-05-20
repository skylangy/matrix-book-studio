export interface ILocalizer {
    locale: string;
    translate(value: string): string;
}