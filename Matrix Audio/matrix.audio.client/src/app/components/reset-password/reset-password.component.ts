import { Component, OnInit } from '@angular/core';
import { FormBasedComponent } from '../form-component/form.component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { PromptService } from '../../services/prompt.service';
import { passwordsMatchValidator } from './password-match.validator';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-reset-password',
    templateUrl: 'reset-password.component.html',
    imports: [RouterModule, ReactiveFormsModule, TranslatePipe]
})

export class ResetPasswordComponent extends FormBasedComponent implements OnInit {

    constructor(
        private readonly authService: AuthService,
        private readonly promptService: PromptService,
    ) {
        super();
    }

    ngOnInit() {
        this.dataForm = new FormGroup({
            email: new FormControl('', [Validators.required]),
            oldPassword: new FormControl('', [Validators.required]),
            newPassword: new FormControl('', [Validators.required]),
            confirmPassword: new FormControl('', [Validators.required])
        },
            { validators: passwordsMatchValidator() }
        );
    }

    async onSubmit(form: any) {
        const formValues = { ...form };
        const resetViewmodel = {
            email: formValues.email,
            oldPassword: formValues.oldPassword,
            newPassword: formValues.newPassword,
            confirmPassword: formValues.confirmPassword
        }

        let result = await this.authService.updatePassword(resetViewmodel);
        if (result.success) {
            this.dataForm.reset();
            this.promptService.showSuccess('Succeed', result.message!);
        } else {
            this.promptService.showError('Failed', result.message!);
        }
    }
}