import { Component, OnInit } from '@angular/core';
import { FormBasedComponent } from '../form-component/form.component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { PromptService } from '../../services/prompt.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-email-subscribe',
    templateUrl: 'email-subscribe.component.html',
    imports: [ReactiveFormsModule, TranslatePipe]
})

export class EmailSubscribeComponent extends FormBasedComponent implements OnInit {
    constructor(
        private readonly userService: UserService,
        private readonly promptService: PromptService,
    ) {
        super();
    }

    ngOnInit() {
        this.dataForm = new FormGroup({
            email: new FormControl('', [Validators.required, Validators.email])
        });
    }

    async onSubmit(form: any) {
        const formValues = { ...form };
        const model = {
            email: formValues.email
        };

        let result = await this.userService.subscribe(model.email);
        if (result.success) {
            this.dataForm.reset();
            this.promptService.showSuccess('Succeed', result.message!);
        } else {
            this.promptService.showError('Failed', result.message!);
        }
    }

}