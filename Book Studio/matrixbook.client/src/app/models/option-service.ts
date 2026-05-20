import { IOptions } from "./options";

export interface IOptionService {
    getOptions(): Promise<IOptions>;

    getConfigValue<T>(name: string, defaultValue?: T): T;
}