import { BookExportModel } from "./book-export-model";


export interface IExportService {
    exportChapters(exportModel: BookExportModel): Promise<void>;

    exportBookContent(id: string): Promise<void>;

    exportBookSubtitle(id: string): Promise<void>;

    exportFinishedBookContent(): Promise<void>;

    combineVideos(id: string): Promise<void>;

    combineFinishedBooksVideos(): Promise<void>;
}