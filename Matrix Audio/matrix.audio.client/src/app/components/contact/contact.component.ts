import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { SigninPromptComponent } from '../signin-prompt/signin-prompt.component';
import { BannerComponent } from '../banner/banner.component';
import { PromptService } from '../../services/prompt.service';
import { UserService } from '../../services/user.service';
import { FormBasedComponent } from '../form-component/form.component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Faq } from '../../models/faq';
import { FaqService } from '../../services/faq.service';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-contact',
    templateUrl: './contact.component.html',
    imports: [CommonModule, ReactiveFormsModule, SigninPromptComponent, BannerComponent,
        TranslatePipe
    ]
})
export class ContactComponent extends FormBasedComponent implements OnInit {
    private lastMessageDate = 'lastMessageDate';
    faqs: Faq[] = [];

    constructor(
        private readonly authService: AuthService,
        private readonly userService: UserService,
        private readonly faqService: FaqService,
        private readonly promptService: PromptService
    ) {
        super();
    }

    async ngOnInit() {
        this.dataForm = new FormGroup({
            subject: new FormControl('', [Validators.required, Validators.minLength(10)]),
            content: new FormControl('', [Validators.required, Validators.maxLength(500), Validators.minLength(50)]),
        });

        this.faqs = await this.faqService.getFaqs();
        console.log(this.faqs);
    }

    get isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }

    get hasFaqs(): boolean {
        return this.faqs.length > 0;
    }

    async send(dataFormValue: any) {
        let lastMessageDate = localStorage.getItem(this.lastMessageDate);
        if (lastMessageDate) {
            let date = new Date(lastMessageDate);
            let now = new Date();
            let diff = Math.abs(now.getTime() - date.getTime());
            if (diff < 60000 * 5) {
                this.promptService.showError('Failed', 'You can only send a message every 5 minutes');
                return;
            }
        }

        const formValues = { ...dataFormValue };
        const message = {
            userId: this.authService.user?.id,
            subject: formValues.subject,
            content: formValues.content
        };

        let result = await this.userService.leaveMessage(message);
        if (result.success) {
            this.dataForm.reset();
            this.promptService.showSuccess('Succeed', result.message!);
        } else {
            this.promptService.showError('Failed', result.message!);
        }
        localStorage.setItem(this.lastMessageDate, new Date().toISOString());
    }
}
