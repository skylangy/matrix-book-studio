import { Book } from "./book";
import { Subject } from "rxjs";
import { IBookService } from "./book-service";
import { IRouter } from "./router";
import { INotificationService } from "./notification-service";
import { ILogger } from "./logger";
import { IOptionService } from "./option-service";
import { IExportService } from "./export-service";
import { IChapterService } from "./chapter-service";
import { IModalService } from "./modal-service";
import { IApiService } from "./api-service";
import { IEditor } from "./editor";
import { IMemoryService } from "./memory-service";
import { IClipboardService } from "./clipboard-service";
import { ITextTransformService } from "./text-transform-service";
import { ImageService } from "../services/image-service";
import { IAuthorService } from "./author-service";


export interface EditorContext {
    book?: Book;
    rawEditor?: any;
    isDirty?: boolean;
    showPreview?: boolean;
    contentChanged?: Subject<string>;
    configuration?: { [key: string]: any };

    router?: IRouter;
    bookService?: IBookService;
    authorService?: IAuthorService;
    notificationService?: INotificationService;
    logger?: ILogger;
    optionService?: IOptionService;
    exportService?: IExportService;
    chapterService?: IChapterService;
    modalService?: IModalService;
    apiService?: IApiService;
    editor?: IEditor;
    memoryService?: IMemoryService;
    clipboardService?: IClipboardService;
    textTransformService?: ITextTransformService;
    imageService?: ImageService;
}