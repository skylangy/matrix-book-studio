import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { BackgroundImageDirective } from 'src/app/directives/background-image.directive';
import { Book } from 'src/app/models/book';
import { BookCardConfig } from 'src/app/models/book-config';
import { ILogger } from 'src/app/models/logger';
import { WorkProgress } from 'src/app/models/work-progress';
import { BookService } from 'src/app/services/book-service';
import { LoggingService } from 'src/app/services/logging-services';
import { WorkProgressService } from 'src/app/services/work-progress-service';
import { BookComponent } from '../book/book.component';
import { LoadingComponent } from '../loading/loading.component';

@Component({
    selector: 'mtx-publish-queue',
    templateUrl: './publish-queue.component.html',
    imports: [
        CommonModule,
        BookComponent,
        BackgroundImageDirective,
        LoadingComponent,
        CdkDropList, CdkDrag
    ],
})
export class PublishQueueComponent implements OnInit {
    headerImage = './assets/images/splashes/photo-15.jpg';
    title = 'Ready to Publish';
    books: Book[] = [];
    isLoading = signal(false);
    cardConfig: BookCardConfig = {
        showDelete: true,
        showDone: true,
    };
    isPublishQueueyEmpty = signal(true);
    publishingItems: WorkProgress[] = [];

    private logger: ILogger;

    constructor(private readonly bookService: BookService,
        private readonly loggingService: LoggingService,
        private readonly workProgressService: WorkProgressService,
    ) {
        this.logger = this.loggingService.getLogger('PublishQueue');
    }

    async ngOnInit() {
        this.load();
    }

    async load() {
        this.isLoading.set(true);
        this.books = await this.bookService.getPublishQueue();
        this.isLoading.set(false);

        this.publishingItems = await this.bookService.getWorkItems();
        this.isPublishQueueyEmpty.set(this.publishingItems.length === 0);

        this.logger.log(`Loaded books in the publish queue`, this.publishingItems);
    }

    async drop(event: CdkDragDrop<Book[]>) {

        let draggedBook = this.books[event.previousIndex];
        if (!draggedBook) {
            this.logger.warn(`Dropped book not found at index ${event.previousIndex}`);
            return;
        }

        moveItemInArray(this.books, event.previousIndex, event.currentIndex);

        this.logger.log(`Dropped book "${draggedBook.title}" from ${event.previousIndex} to ${event.currentIndex}`);
        this.bookService.updatePublishOrder(draggedBook.id!, event.currentIndex);
    }
}
