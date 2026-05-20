import { Component, OnInit, signal } from '@angular/core';
import { Book } from 'src/app/models/book';
import { IBookAction } from 'src/app/models/book-action';
import { GroupedBooks } from 'src/app/models/groupedBooks';
import { ILogger } from 'src/app/models/logger';
import { OptionNames } from 'src/app/models/options-names';
import { BookService } from 'src/app/services/book-service';
import { ExportService } from 'src/app/services/export-service';
import { LoggingService } from 'src/app/services/logging-services';
import { MaintenanceService } from 'src/app/services/maintenance-service';
import { NotificationService } from 'src/app/services/notification-service';
import { OptionService } from 'src/app/services/option-service';
import { BackgroundImageDirective } from '../../directives/background-image.directive';
import { BookActionComponent } from '../book-action/book.action.component';
import { BooksComponent } from '../books/books.component';
import { LoadingComponent } from '../loading/loading.component';

@Component({
    selector: 'mtx-welcome',
    templateUrl: './welcome.component.html',
    imports: [
        BackgroundImageDirective,
        BookActionComponent,
        BooksComponent,
        LoadingComponent
    ],
})
export class WelcomeComponent implements OnInit {
    headerImage = './assets/images/splashes/photo-13.jpg';
    title = 'Matrix Book Studio';
    subtitle = 'Version 1.0.0';
    isLoading = signal(false);

    _books: Book[] = [];
    bookActions: IBookAction[] = [
        { name: 'New Book', label: 'Create', visible: true, color: 'primary', icon: 'file-plus', route: '/new', description: 'Create an empty book' },
        { name: 'New Author', label: 'Create', visible: true, color: 'secondary', icon: 'file-person', route: '/new-author', description: 'Create a new author' },
        { name: 'Export Text Content', label: 'Export', visible: true, color: 'secondary', icon: 'box-arrow-right', func: () => { this.exportService.exportFinishedBookContent(); }, description: 'Export finished book' },
        { name: 'Combine Videos', label: 'Combine', visible: true, color: 'secondary', icon: 'gpu-card', func: () => { this.exportService.combineFinishedBooksVideos(); }, description: 'Combine generated videos' },
        {
            name: 'Clean Combine', label: 'Clean', visible: true, color: 'secondary', icon: 'trash2', func: async () => {
                let message = await this.maintenanceService.cleanCombineVideos();
                this.notificationService.showSuccess('Clean Video', message);
            }, description: 'Clean combine videos'
        },
        {
            name: 'Clean WAV', label: 'Clean', visible: true, color: 'secondary', icon: 'trash2-fill', func: async () => {
                let message = await this.maintenanceService.cleanWavFiles();
                this.notificationService.showSuccess('Clean WAV', message);
            }, description: 'Clean WAV files'
        },
        {
            name: 'Subtitle Schedule', label: 'Subtitle Schedule', visible: true, color: 'secondary', icon: 'film', func: async () => {
                let result = await this.bookService.generateSubtitleJobSchedule();
                this.logger.log('Subtitle schedule result', result);
            }, description: 'Generate subtitle schedule file'
        },
        { name: 'Import Book', label: 'Import', visible: false, color: 'primary', icon: 'file-plus-fill', route: '/import', description: 'Import book from text file' },
    ]
    groups: GroupedBooks[] = [];
    private _filter = '';
    private logger: ILogger;

    constructor(
        private readonly bookService: BookService,
        private readonly optionService: OptionService,
        private readonly exportService: ExportService,
        private readonly maintenanceService: MaintenanceService,
        private readonly notificationService: NotificationService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('WelcomeComponent');
    }

    async ngOnInit(): Promise<void> {
        await this.optionService.getOptions();
        this.refresh();
    }

    get books() {
        return this._books;
    }

    get filter() {
        return this._filter;
    }

    set filter(value: string) {
        this._filter = value;
        this.refresh();
    }

    async refresh() {
        this.isLoading.set(true);
        this.logger.log(`Refresh books with filter ${this.filter}`);
        let recentCount = this.optionService.getConfigValue<number>(OptionNames.recentBookCount, 12);

        this._books = await this.bookService.getRecentBooks(recentCount, this.filter);
        for (let book of this._books) {
            book.isVisible = true;
        }
        this.isLoading.set(false);
    }


}
