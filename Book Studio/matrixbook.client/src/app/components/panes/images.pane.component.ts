import { Component, OnInit } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { LoggingService } from 'src/app/services/logging-services';
import { ILogger } from 'src/app/models/logger';
import { FileUploadService } from 'src/app/services/file-upload-service';
import { ImageService } from 'src/app/services/image-service';
import { BookResource } from 'src/app/models/book-resource';
import { BookService } from 'src/app/services/book-service';
import { NotificationService } from 'src/app/services/notification-service';
import { FileUploaderComponent } from '../file-uploader/file.uploader.component';

@Component({
    selector: 'mtx-images-pane',
    templateUrl: './images.pane.component.html',
    imports: [FileUploaderComponent],
})
export class ImagesPaneComponent extends PaneBaseComponent {
    images: BookResource[] = [];
    files: File[] = [];
    showUpload = false;

    private readonly logger?: ILogger;

    constructor(
        private bookService: BookService,
        private fileUploadService: FileUploadService,
        private imageService: ImageService,
        private notificationService: NotificationService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('ImagesPaneComponent');
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.images = this.loadImages();
        this.logger?.log(`Images of ${this.book?.title}`, this.book?.defaultImageId, this.images);
        this.logger?.log('image Pane', this.book);
    }

    toggleUpload() {
        this.showUpload = !this.showUpload;
    }

    async upload() {
        let bookName = this.book?.title;
        if (!bookName || this.files.length === 0) {
            return;
        }

        try {
            for (let file of this.files) {
                await this.fileUploadService.uploadBookImage(file, bookName);
            }

            // reload images
            await this.reloadBook();
            await this.reload();
            this.showUpload = false;
            this.notificationService.showSuccess(bookName, 'Upload image successfully.');
        }
        catch (e) {
            this.logger?.log('upload failed:', e);
            this.notificationService.showFail(bookName, 'Upload image failed.');
        }
        finally {
            this.files = [];
        }
    }

    async deleteImage(image: BookResource) {
        if (!image)
            return;

        let bookName = this.book?.title!;
        try {
            await this.imageService.deleteImage(bookName, image.url!);
            await this.reload();

            this.notificationService.showSuccess(bookName, 'Delete image successfully.');
        }
        catch (e) {
            this.notificationService.showFail(bookName, 'Delete image failed.');
        }
    }

    onFileReady(files: File[]) {
        this.logger?.log('onFileReady', files);
        this.files = files;
    }

    async reload() {
        let book = await this.bookService.getBook(this.book?.id!);
        this.images = this.loadImages();
    }

    loadImages(): BookResource[] {
        let resources = [];
        for (let imageId of this.book?.imageIds!) {
            resources.push({
                id: imageId,
                fullUrl: this.imageService.getImageUrlById(imageId)
            });
        }

        if (resources.length === 0 && this.book?.defaultImageId) {
            resources.push({
                id: this.book?.defaultImageId,
                fullUrl: this.imageService.getImageUrlById(this.book?.defaultImageId)
            });
        }

        return resources;
    }

    canUpload() {
        return this.files && this.files.length > 0;
    }
}
