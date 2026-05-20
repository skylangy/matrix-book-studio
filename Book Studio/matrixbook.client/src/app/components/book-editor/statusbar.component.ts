import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { Book } from 'src/app/models/book';
import { EditorContext } from 'src/app/models/editor-context';
import { WorkProgress } from 'src/app/models/work-progress';
import { SignalRService } from 'src/app/services/signal-service';
import { NgClass, DecimalPipe, DatePipe } from '@angular/common';

@Component({
    selector: 'mtx-statusbar',
    templateUrl: './statusbar.component.html',
    imports: [
        NgClass,
        DecimalPipe,
        DatePipe,
    ],
})
export class StatusbarComponent implements OnInit, OnDestroy {
    @Input() book?: Book;
    private contentSubscription?: Subscription;
    _context?: EditorContext;
    currentProgress?: WorkProgress;

    textCount: number = 0;
    chapterCount: number = 0;
    chunkCount: number = 0;
    warnings: string[] = [];

    constructor(private signalRService: SignalRService) { }

    @Input()
    get context(): EditorContext | undefined {
        return this._context;
    }
    set context(value: EditorContext | undefined) {
        this.stopListen();
        this._context = value;
        this.startListen();
    }

    get isDirty(): boolean {
        return this.context?.isDirty || false;
    }

    ngOnInit(): void {
        this.updateCount();
        this.startListen();

        this.signalRService.addMessageListener(async (workProgress: WorkProgress) => {
            this.currentProgress = workProgress;
        });
    }

    ngOnDestroy(): void {
        this.stopListen();
    }

    onContentChanged() {
        this.updateCount();
        this.updateWarnings();
    }

    updateCount() {
        let content = this.context?.rawEditor?.getValue();
        let model = this.context?.rawEditor?.getModel();
        this.textCount = content?.length || 0;

        let chapterService = this.context?.chapterService;
        let chapters = chapterService?.toChaptersWithChunk(model, content, '');

        this.chapterCount = chapters?.length || 0;
        this.chunkCount = chapters?.reduce(
            (count, chapter) => count + (chapter.chunks ? chapter.chunks.length : 0),
            0
        ) || 0;
    }

    updateWarnings() {
        this.warnings = [];
        let bookTitle = this.book?.title;
        if (!bookTitle) {
            this.warnings.push('Need book title');
        }
        let splash = this.book?.imageIds?.length || 0;
        if (splash < 1 && !this.book?.defaultImageId) {
            this.warnings.push('Need at least one splash image');
        }
    }

    startListen() {
        if (this.context) {
            this.contentSubscription = this.context.contentChanged?.subscribe(() => {
                this.onContentChanged();
            });
        }
    }

    stopListen() {
        if (this.contentSubscription) {
            this.contentSubscription.unsubscribe();
        }
    }
}
