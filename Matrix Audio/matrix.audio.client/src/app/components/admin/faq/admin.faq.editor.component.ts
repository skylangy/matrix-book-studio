import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { CommonModule } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { ValueService } from '../../../services/value.service';
import { Faq } from '../../../models/faq';
import { FaqService } from '../../../services/faq.service';
import { PromptService } from '../../../services/prompt.service';

@Component({
    selector: 'mtx-admin-faq-editor',
    templateUrl: 'admin.faq.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminFaqEditorComponent extends EditorBase<Faq> {
    private logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly valueService: ValueService,
        private readonly faqService: FaqService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'person-raised-hand';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.faqService.getFaq(id);

                this.dataForm.patchValue(this.model);
            } else {
                const now = new Date();
                this.model = {
                    id: Math.random().toString(36).substring(7),
                    question: '',
                    answer: '',
                    category: '',
                    tags: [],
                    dateCreated: now,
                    dateUpdated: now,
                    isActive: true
                };
            }
            this.logger.info('Faq loaded', this.model);
        });
    }

    override createForm() {
        return new FormGroup({
            question: new FormControl('', [Validators.required]),
            answer: new FormControl('', [Validators.required]),
            category: new FormControl('', [Validators.required]),
            tags: new FormControl('', [Validators.required]),
        });
    }

    override async onSubmit() {
        if (this.dataForm.invalid) {
            this.logger.warn('Form is invalid', this.dataForm.errors);
            return;
        }

        if (this.model) {
            const faq = this.dataForm.value as Faq;
            faq.id = this.model.id;

            this.logger.info('Updating faq', this.dataForm.value.tags);
            if (this.dataForm.value.tags) {
                const tags: string = this.dataForm.value.tags;
                faq.tags = tags.split(',');
            }

            this.logger.info('Updating faq', faq);
            await this.faqService.updateFaq(faq);

            this.logger.info('Faq updated', faq);
            this.promptService.showSuccess('Success', `Post '${faq.question}' is updated successfully`);
        }
    }

    override back() {
        this.router.navigate(['/control-tower/faqs']);
    }
}