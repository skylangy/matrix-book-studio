
import { Component, OnInit } from '@angular/core';
import { ILogger } from 'src/app/models/logger';
import { NavItem } from 'src/app/models/nav-item';
import { AuthorService } from 'src/app/services/author-service';
import { BookService } from 'src/app/services/book-service';
import { ExportService } from 'src/app/services/export-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';

@Component({
    selector: 'app-name',
    template: '',
})
export class BaseSidebarComponent implements OnInit {
    title = 'Book Studio';
    mainNavItems: NavItem[] = [
        { text: 'Home', icon: 'house', route: '/welcome', isActive: true },
        { text: 'Publish Queue', icon: 'list-nested', route: '/publish-queue', isActive: false },
        { text: 'Authors', icon: 'people', route: '/author-manage', isActive: false },
        { text: 'Book by Categories', icon: 'archive', route: '/categories', isActive: false },
        { text: 'Book by Tags', icon: 'tag', route: '/tags', isActive: false },
        { text: 'Book by Authors', icon: 'person', route: '/authors', isActive: false },
        { text: 'In Progress Books', icon: 'square', route: '/status/In Progress', isActive: false },
        { text: 'Finished Books', icon: 'check-square', route: '/status/Finished', isActive: false },
        { text: 'Videos', icon: 'camera-video', route: '/videos', isActive: false },
        { text: 'Narration', icon: 'cassette', route: '/narrations', isActive: false },
        { text: 'Templates', icon: 'aspect-ratio', route: '/video/templates', isActive: false },
        { text: 'Statistics', icon: 'activity', route: '/statistics', isActive: false },
    ];
    secondaryNavItems: NavItem[] = [
        { text: 'Settings', icon: 'gear', route: '/settings', isActive: false },
    ];

    activeNavItem!: NavItem;
    private logger: ILogger;

    constructor(
        private readonly exportService: ExportService,
        private readonly notificationservice: NotificationService,
        private readonly authorService: AuthorService,
        private readonly bookService: BookService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('SidebarComponent');
    }

    ngOnInit(): void {
        this.activeNavItem = this.mainNavItems[0];
    }

    navItemClicked(navItem: NavItem) {
        this.activeNavItem.isActive = false;
        navItem.isActive = true;
        this.activeNavItem = navItem;
    }

    async exportFinishedBookContent() {
        await this.exportService.exportFinishedBookContent();
    }

    async generateMetadata() {
        this.notificationservice.showSuccess('Generate Metadata', 'Start generating metadata for Finished Books');
        await this.exportService.generateMetadataForFinishedBook();
        this.notificationservice.showSuccess('Generate Metadata', 'Finished generating metadata for Finished Books');
    }

    async syncAuthors() {
        this.notificationservice.showSuccess('Sync Authors', 'Start syncing authors');
        await this.authorService.syncAuthors();
        this.notificationservice.showSuccess('Sync Authors', 'Finished syncing authors');
    }

    async syncFinishedBooks() {
        this.notificationservice.showSuccess('Sync Finished Books', 'Start syncing finished books');
        await this.bookService.syncFinishedToLibrary();
        this.notificationservice.showSuccess('Sync Finished Books', 'Finished syncing finished books');
    }
}
