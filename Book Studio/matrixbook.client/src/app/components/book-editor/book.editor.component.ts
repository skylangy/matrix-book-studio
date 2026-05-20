import { Component, ElementRef, EventEmitter, HostListener, Input, NgZone, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { IEditor } from 'src/app/models/editor';
import { ILogger } from 'src/app/models/logger';
import { ApiService } from 'src/app/services/api-service';
import { AuthorService } from 'src/app/services/author-service';
import { ChapterService } from 'src/app/services/chapter-service';
import { ClipboardService } from 'src/app/services/clipboard-service';
import { ExportService } from 'src/app/services/export-service';
import { ImageService } from 'src/app/services/image-service';
import { KeyService } from 'src/app/services/key-service';
import { MemoryService } from 'src/app/services/memory-service';
import { ModalService } from 'src/app/services/modal-service';
import { NotificationService } from 'src/app/services/notification-service';
import { OptionService } from 'src/app/services/option-service';
import { StatusService } from 'src/app/services/status-service';
import { TextTransformService } from 'src/app/services/text-service';
import { VoiceService } from 'src/app/services/voice-service';
import { CopyOnClickDirective } from '../../directives/copy-onclick';
import { Book } from '../../models/book';
import { EditorContext } from '../../models/editor-context';
import { EditorPane } from '../../models/editor-pane';
import { EditorView } from "../../models/editor-view";
import { BookService } from '../../services/book-service';
import { LoggingService } from '../../services/logging-services';
import { BookPreviewComponent } from '../book-preview/book.preview.component';
import { RibbonComponent } from '../toolbar/ribbon.component';
import { ToolPanesComponent } from '../toolpane/toolpanes.component';
import { EditorViewCreator } from './editor.view.creator';
import { TextEditorComponent } from './text.editor.component';

@Component({
    selector: 'mtx-book-editor',
    templateUrl: './book.editor.component.html',
    imports: [
        RibbonComponent,
        TextEditorComponent,
        FormsModule,
        ToolPanesComponent,
        BookPreviewComponent,
        CopyOnClickDirective
    ],
})
export class BookEditorComponent implements OnInit, OnDestroy {
    @ViewChild('editor') textEditor?: TextEditorComponent;
    @ViewChild('editorArea') editorArea?: ElementRef;
    @ViewChild('contentArea') contentArea?: ElementRef;
    @ViewChild('toolPaneArea') toolPaneArea?: ElementRef;
    @ViewChild('leftPaneArea') leftPaneArea?: ElementRef;
    @ViewChild('root') rootDiv?: ElementRef;

    @Input() book?: Book;
    @Output() editorLoaded = new EventEmitter<any>();

    editorContext?: EditorContext;
    editorView?: EditorView;
    editorRaw?: any;
    editor?: IEditor;
    isNewBook = false;
    contentChangedTimeout = 200;
    contentChangedTimer: any;

    private logger?: ILogger;

    constructor(
        private activateRoute: ActivatedRoute,
        private router: Router,
        private apiService: ApiService,
        private bookService: BookService,
        private authorService: AuthorService,
        private optionService: OptionService,
        private exportService: ExportService,
        private chapterService: ChapterService,
        private modalService: ModalService,
        private notificationService: NotificationService,
        private voiceService: VoiceService,
        private memoryService: MemoryService,
        private loggingService: LoggingService,
        private statusService: StatusService,
        private keyService: KeyService,
        private clipboardService: ClipboardService,
        private textTransformService: TextTransformService,
        private imageService: ImageService,
        private zone: NgZone) {
        this.logger = this.loggingService?.getLogger('BookEditor');
        this.logger.enabled = true;
    }

    get content(): string | undefined {
        return this.book?.content;
    }

    set content(value: string | undefined) {
        if (this.book) {
            if (this.book.content !== value) {
                this.book.content = value;
                this.editorContext!.isDirty = true;
            }
        }
    }

    get isDirty(): boolean {
        return this.editorContext?.isDirty || false;
    }

    @HostListener('document:keydown', ['$event'])
    handleKeyDown(event: KeyboardEvent): void {
        this.keyService.handleKeyDown(event, this);
    }

    ngOnInit(): void {
        this.logger?.log('Book editor init', this.router.url);

        this.activateRoute.params.subscribe(async params => {
            let bookId = params['id?'];

            if (bookId) {
                this.book = await this.bookService.getBook(bookId);
                this.logger?.log('Book loaded with', bookId, params, this.book);
            } else {
                this.logger?.log('Create a new book', bookId, params);
                this.book = {
                    language: this.voiceService.defaultLanguage,
                    voiceName: this.voiceService.defaultVoice,
                    title: this.memoryService.get<string>('bookTitle'),
                    author: this.memoryService.get<string>('bookAuthor'),
                    categoryIds: this.memoryService.get<string[]>('bookCategory') || [],
                };
                this.isNewBook = true;
            }
            this.initializeEditor();
        });
    }

    ngOnDestroy(): void {
        this.statusService.setContext(undefined);
        this.editorRaw = null;
    }

    onRawEditorReady(editor: any) {
        this.editorRaw = editor;
        if (this.book === undefined) {
            return;
        }

        this.initializeEditor();
    }

    initializeEditor() {
        if (this.editorRaw === undefined || this.book === undefined) {
            return;
        }

        this.zone.run(async () => {
            this.logger?.log('Initialize editor with book in zone', this.book?.title);
            this.editorContext = {
                book: this.book,
                rawEditor: this.textEditor?.editor,
                showPreview: false,
                configuration: this.createContextConfig(),
                contentChanged: new Subject<string>(),
                bookService: this.bookService,
                authorService: this.authorService,
                notificationService: this.notificationService,
                router: this.router,
                logger: this.logger,
                optionService: this.optionService,
                exportService: this.exportService,
                chapterService: this.chapterService,
                modalService: this.modalService,
                apiService: this.apiService,
                memoryService: this.memoryService,
                clipboardService: this.clipboardService,
                textTransformService: this.textTransformService,
                imageService: this.imageService
            };

            let viewCreator = new EditorViewCreator(this.editorContext);
            viewCreator.layoutAction = () => { this.layout(); };
            this.editorView = viewCreator.createView();
            this.editor = viewCreator;
            this.editorContext.editor = this.editor;
            let length = this.editorRaw.getValue().length;
            if (length > 500000)
                this.contentChangedTimeout = 1000;

            if (this.isNewBook) {
                viewCreator.showBookProperty();
            }

            this.statusService.setContext({
                book: this.book,
                editorContext: this.editorContext,
            });

            this.editorRaw.onDidChangeModelContent((e: any) => {
                clearTimeout(this.contentChangedTimer);
                this.contentChangedTimer = setTimeout(() => {
                    this.zone.run(() => {
                        let value = this.editorRaw.getValue();
                        this.content = value;
                        this.editorContext!.contentChanged!.next(value);
                    });
                }, this.contentChangedTimeout);
            });

            this.layout();
            this.editorLoaded.emit(this.editorContext);
        });
    }

    createContextConfig(): { [key: string]: any } {
        return {
            'showOutlinePane': true,
            'showFormatPane': false,
            'showLoggingPane': false,
        };
    }

    paneClosed(pane: EditorPane) {
        pane.tag.isActive = false;
        this.textEditor?.layout();
    }

    layout() {
        let fullWidth = this.rootDiv?.nativeElement.clientWidth;
        let contentHeight = this.contentArea?.nativeElement.clientWidth;
        let toolPaneWidth = this.toolPaneArea?.nativeElement.clientWidth;

        let contentWidth = fullWidth! - toolPaneWidth!;
        this.textEditor?.layout(contentWidth, contentHeight);

        setTimeout(() => {
            this.textEditor?.layout();
        }, 100);
    }
}
