import { Component } from '@angular/core';
import { Post } from '../../../models/post';
import { PostService } from '../../../services/post.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ILogger } from '../../../models/logger';
import { LoggingService } from '../../../services/logging.service';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { CommonModule } from '@angular/common';
import { PromptService } from '../../../services/prompt.service';
import { FileUploaderComponent } from '../../file-uploder/file.uploader.component';
import { ImageService } from '../../../services/image.service';
import { ImageUrlPipe } from '../../../pipes/image.pipe';
import { EditorBase } from '../common/editor.base';
import { AlbumService } from '../../../services/album.service';

@Component({
    selector: 'admin-post-editor',
    templateUrl: 'admin.post.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        FileUploaderComponent, ImageUrlPipe,
        ReactiveFormsModule, CommonModule, FormsModule]
})
export class AdminPostEditorComponent extends EditorBase<Post> {
    files: File[] = [];

    private logger: ILogger

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly postService: PostService,
        private readonly promptService: PromptService,
        private readonly imageService: ImageService,
        private readonly albumService: AlbumService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('AdminPostEditorComponent');
        this.icon = 'file';
    }

    override ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const postId = params['id'];
            if (postId) {
                this.model = await this.postService.getPost(postId);
            } else {
                const now = new Date();
                this.model = {
                    id: Math.random().toString(36).substring(7),
                    title: '',
                    slug: '',
                    content: '',
                    dateCreated: now,
                    dateUpdated: now,
                    image: ''
                };
            }

            this.dataForm.patchValue(this.model);
            this.logger.info(`Editing post ${postId}`, this.model);

            this.dataForm.get('title')?.valueChanges.subscribe(value => {
                this.updateSlug(value);
            });
        });
    }

    override back() {
        this.router.navigate(['/control-tower/posts']);
    }

    async insertBookList() {
        let end = Date.now();
        let start = end - 1000 * 60 * 60 * 24 * 30; // 30 days ago
        let bookList = await this.albumService.getByDate(new Date(start), new Date(end));
        if (bookList) {
            this.logger.info('Book list', bookList);
            let content = `<ul>`;
            content += bookList.map(book => `<li><a href="/public/album/${book.id}">${book.title} (${this.formatDate(book.dateUpdated)})</a></li>`).join('');

            content += `</ul>`;
            this.logger.info('Book list', content);
            this.dataForm.get('content')?.setValue(this.dataForm.get('content')?.value + content);
        }
    }

    override async onSubmit() {
        if (this.dataForm.invalid) {
            this.logger.warn('Form is invalid', this.dataForm.errors);
            return;
        }

        if (this.model) {
            let imageId = '';
            if (this.files.length > 0) {
                const imageResource = await this.imageService.uploadPostSplash(this.files[0]);
                imageId = imageResource.id;
            }
            const post = this.dataForm.value as Post;
            post.id = this.model.id;
            post.image = imageId;
            await this.postService.updatePost(post);

            this.logger.info('Post updated', post);
            this.promptService.showSuccess('Success', `Post '${post.title}' is updated successfully`);
        }
    }

    onFileReady(files: File[]) {
        this.files = files;
    }

    override createForm(): FormGroup {
        let dataForm = new FormGroup({
            title: new FormControl(this.model?.title, [Validators.required]),
            slug: new FormControl(this.model?.slug),
            content: new FormControl(this.model?.content, [Validators.required]),
            dateCreated: new FormControl(this.model?.dateCreated, [Validators.required]),
            dateUpdated: new FormControl(this.model?.dateUpdated, [Validators.required]),
            image: new FormControl(this.model?.image, []),
        });

        return dataForm;
    }

    async removeSplash() {
        let imageId = this.model?.image;
        if (imageId) {
            this.imageService.deleteImage(imageId);
            this.dataForm.get('image')?.setValue('');
            this.model!.image = '';
            this.logger.info('Splash image removed', this.model);
            await this.postService.removeSplash(this.model!.id!);
        }
    }

    private updateSlug(title: string): void {
        const slug = title
            .toLowerCase()
            .trim()
            .replace(/[\s]+/g, '-') // Replace spaces with dashes
            .replace(/[^\w-]+/g, ''); // Remove non-word characters
        this.dataForm.get('slug')?.setValue(slug, { emitEvent: false });
    }

    private formatDate(date?: Date): string {
        if (!date)
            return '';
        if (typeof date === 'string')
            date = new Date(date);

        console.log(date, typeof date);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

}