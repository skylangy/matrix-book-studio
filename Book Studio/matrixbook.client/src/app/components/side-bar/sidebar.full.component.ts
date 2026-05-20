import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BookService } from 'src/app/services/book-service';
import { ChapterService } from 'src/app/services/chapter-service';
import { ExportService } from 'src/app/services/export-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { BaseSidebarComponent } from './sidbar.component.base';

@Component({
    selector: 'mtx-side-bar',
    templateUrl: './sidebar.full.component.html',
    imports: [RouterModule],
})
export class FullSidebarComponent extends BaseSidebarComponent {

    constructor(bookService: BookService,
        chapterService: ChapterService,
        exportService: ExportService,
        notificationService: NotificationService,
        loggingService: LoggingService) { super(exportService, loggingService); }

}
