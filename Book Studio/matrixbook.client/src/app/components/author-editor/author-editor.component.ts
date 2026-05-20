import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Author } from 'src/app/models/author';
import { ILogger } from 'src/app/models/logger';
import { ConvertToSimplifyStep, TextProcessor } from 'src/app/models/text-processor';
import { AuthorService } from 'src/app/services/author-service';
import { LoggingService } from 'src/app/services/logging-services';
import { NotificationService } from 'src/app/services/notification-service';
import { AuthorImagePipe } from '../../pipe/author.image.pipe';
import { HeaderComponent } from '../header/header.component';
import { WorkViewComponent } from '../work-view/work.view.component';

@Component({
    selector: 'mtx-author-editor',
    templateUrl: './author-editor.component.html',
    imports: [
        HeaderComponent,
        FormsModule,
        AuthorImagePipe,
    ],
})
export class AuthorEditorComponent extends WorkViewComponent implements OnInit {
    @Input() author: Author | null = {};
    @Input() showBanner = true;
    @Output() authorSaved = new EventEmitter<Author>();
    @Output() authorCanceled = new EventEmitter<Author>();
    isNewAuthor = false;

    private logger?: ILogger;

    constructor(private activateRoute: ActivatedRoute,
        private router: Router,
        private authorService: AuthorService,
        private notificationService: NotificationService,
        private loggingService: LoggingService) {
        super();
        this.title = 'Author';
        this.subtitle = 'Eidt Author';
        this.bannerImage = './assets/images/splashes/photo-2.jpg';

        this.logger = this.loggingService?.getLogger('AuthorEditor');
    }

    override async ngOnInit() {
        this.activateRoute.params.subscribe(async params => {
            let authorId = params['id?'];

            if (authorId) {
                this.author = await this.authorService.getAuthor(authorId);
                this.logger?.log('Author id after get name', authorId, params);
            } else if (!this.author) {
                this.logger?.log('Create a new author', authorId, params);
                this.author = {};
                this.isNewAuthor = true;
            }
        });
    }

    async save() {
        this.author = await this.authorService.updateAuthor(this.author!);
        this.notificationService.showSuccess('Author saved', `Author "${this.author?.name}" saved`);
        this.authorSaved.emit(this.author!);
    }

    cancel() {
        this.authorCanceled.emit(this.author!);
    }

    prePaste(event: ClipboardEvent) {
        event.preventDefault();

        let pastedText = event.clipboardData?.getData('text') || '';

        pastedText = this.removePatterns(pastedText);
        pastedText = this.convertToSimplifiedChinese(pastedText);

        // Optionally insert the processed text into the textarea
        const textarea = event.target as HTMLTextAreaElement;
        const start = textarea.selectionStart || 0;
        const end = textarea.selectionEnd || 0;
        const textBefore = textarea.value.substring(0, start);
        const textAfter = textarea.value.substring(end);

        // Replace the textarea value with the new text
        textarea.value = textBefore + pastedText + textAfter;

        // Optionally, update the cursor position
        const cursorPosition = start + pastedText.length;
        textarea.setSelectionRange(cursorPosition, cursorPosition);

        if (this.author)
            this.author.description = textarea.value;
    }

    removePatterns(text: string): string {
        return text.replace(/\[.*?\]/g, '');
    }

    convertToSimplifiedChinese(text: string): string {

        const processor = new TextProcessor().use(new ConvertToSimplifyStep());
        const context = {}
        return processor.process(text, context);
    }

    clearImages() {
        this.authorService.clearImages(this.author?.id!);
    }
}
