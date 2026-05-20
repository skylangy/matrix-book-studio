import { moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';
import { Author } from 'src/app/models/author';
import { ILogger } from 'src/app/models/logger';
import { AuthorService } from 'src/app/services/author-service';
import { FileUploadService } from 'src/app/services/file-upload-service';
import { ImageService } from 'src/app/services/image-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { AuthorImagePipe } from '../../pipe/author.image.pipe';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'mtx-author',
    templateUrl: './author.component.html',
    imports: [
        DatePipe,
        AuthorImagePipe,
    ],
})
export class AuthorComponent implements OnInit {
    @Input() author?: Author;
    @Output() authorRemoved = new EventEmitter<Author>();
    @Output() authorEdit = new EventEmitter<Author>();
    @Output() authorLoaded = new EventEmitter<Author>();
    isDragging = false;
    hasValidImage = true;

    private logger: ILogger;

    constructor(
        private authorService: AuthorService,
        private notificationService: NotificationService,
        private imageService: ImageService,
        private fileUploadService: FileUploadService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AuthorComponent');
    }

    ngOnInit(): void {
        // this.authorLoaded.emit(this.author);
    }

    async remove() {
        this.logger.log('Remove author', this.author?.name);
        let result = await this.authorService.removeAuthor(this.author?.id!);
        this.logger?.log('Author removed', result);
        if (result) {
            this.authorRemoved.emit(this.author);
            this.notificationService.showSuccess('Author removed', `Author "${this.author?.name}" is removedd`);
        } else {
            this.notificationService.showFail('Author not removed', `Author "${this.author?.name}" is not removedd`);
        }
    }

    edit() {
        this.logger.log('Edit author', this.author?.name);
        this.authorEdit.emit(this.author);
    }

    open() {
        this.logger.log('Open author', this.author?.name);
        this.authorService.openFolder(this.author?.id!);
    }

    async sync() {
        this.authorService.syncAuthor(this.author?.id!);
        this.notificationService.showSuccess('Author sync', `Author "${this.author?.name}" is synced`);
    }

    hasImage(): boolean {
        return this.author?.image !== '' && this.hasValidImage;
    }

    handleImageError(event: Event) {
        this.hasValidImage = false;
    }

    imageLoaded() {
        this.authorLoaded.emit(this.author);
    }

    @HostListener('dragover', ['$event'])
    onDragOver(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = true;
    }

    @HostListener('dragleave', ['$event'])
    onDragLeave(event: Event) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = false;
    }

    @HostListener('drop', ['$event'])
    onDrop(event: DragEvent) {
        this.logger.log('onDrop', event.dataTransfer?.files);
        event.preventDefault();
        event.stopPropagation();
        this.isDragging = false;

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.processImage(files[0]);
        }
    }

    private async processImage(file: File) {
        if (!file.type.startsWith('image/')) {
            return;
        }
        try {
            this.logger.log('processImage', file.name, this.author?.name!);
            let authorName = this.author?.name!;
            await this.fileUploadService.uploadAuthorImage(file, authorName);

            this.author = await this.authorService.getAuthor(this.author?.id!);
        } catch (error) {
            this.notificationService.showFail('Image upload failed', this.author?.name);
        }
    }
}
