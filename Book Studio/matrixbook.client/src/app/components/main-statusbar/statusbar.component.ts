import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Book } from 'src/app/models/book';
import { EditorContext } from 'src/app/models/editor-context';
import { StatusContext } from 'src/app/models/status-context';
import { WorkProgress } from 'src/app/models/work-progress';
import { SignalRService } from 'src/app/services/signal-service';
import { StatusService } from 'src/app/services/status-service';
import { NgClass, DecimalPipe, DatePipe } from '@angular/common';

@Component({
    selector: 'mtx-main-statusbar',
    templateUrl: './statusbar.component.html',

    imports: [
        NgClass,
        DecimalPipe,
        DatePipe,
    ],
})
export class MainStatusbarComponent implements OnInit {
    now: Date = new Date();
    isVisiable = true;
    currentProgress?: WorkProgress;
    statusContext?: StatusContext;

    textCount: number = 0;
    chapterCount: number = 0;
    chunkCount: number = 0;
    warnings: string[] = [];

    private statusSubscription?: Subscription;
    private contentSubscription?: Subscription;

    constructor(
        private statusService: StatusService,
        private signalRService: SignalRService) { }

    ngOnInit(): void {
        this.statusSubscription = this.statusService.contextSubject.subscribe((context) => {
            this.statusContext = context;

            if (this.statusContext) {
                this.onContentChanged();
                this.startListen();
            }
            else {
                this.onContentChanged();
                this.stopListen();
            }
        });

        this.signalRService.addMessageListener(async (workProgress: WorkProgress) => {
            this.currentProgress = workProgress;
        });
    }

    ngOnDestroy(): void {
        this.stopListen();
        this.statusSubscription?.unsubscribe();
        this.contentSubscription?.unsubscribe();
    }

    get context(): EditorContext | undefined {
        return this.statusContext?.editorContext;
    }

    get book(): Book | undefined {
        return this.statusContext?.book;
    }

    get isDirty(): boolean {
        return this.statusContext?.editorContext?.isDirty || false;
    }

    get hasBook(): boolean {
        return this.statusContext?.book !== undefined;
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
        this.chunkCount = chapters?.reduce((count, chapter) => count + (chapter.chunks ? chapter.chunks.length : 0), 0) || 0;
    }

    updateWarnings() {
        this.warnings = [];
        let bookTitle = this.statusContext?.book?.title;
        if (!bookTitle) {
            this.warnings.push('Need book title');
        }
        let splash = this.statusContext?.book?.imageIds?.length || 0;
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
