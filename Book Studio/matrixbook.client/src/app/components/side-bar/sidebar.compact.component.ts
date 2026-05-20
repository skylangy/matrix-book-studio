import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthorService } from 'src/app/services/author-service';
import { BookService } from 'src/app/services/book-service';
import { ExportService } from 'src/app/services/export-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { BaseSidebarComponent } from './sidbar.component.base';

@Component({
    selector: 'mtx-compact-sidebar',
    templateUrl: './sidebar.compact.component.html',
    imports: [RouterModule],
})
export class CompactSidebarComponent extends BaseSidebarComponent {
    constructor(
        exportService: ExportService,
        notificationservice: NotificationService,
        authorService: AuthorService,
        bookService: BookService,
        loggingService: LoggingService) {
        super(exportService, notificationservice,
            authorService, bookService, loggingService);
    }
}
