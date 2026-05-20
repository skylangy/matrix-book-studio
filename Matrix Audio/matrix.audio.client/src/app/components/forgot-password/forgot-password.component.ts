import { Component, OnInit } from '@angular/core';
import { FormBasedComponent } from '../form-component/form.component';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';
import { ImagePaneComponent } from '../img-pane/img-pane.compnent';
import { AlertComponent } from '../alert/alert.component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SplitLayoutComponent } from '../split-layout/split-layout.component';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-forgot-password',
    templateUrl: './forgot-password.component.html',
    imports: [RouterModule, ReactiveFormsModule, ImagePaneComponent, SplitLayoutComponent, AlertComponent,
        TranslatePipe
    ]
})
export class ForgotPasswordComponent extends FormBasedComponent implements OnInit {
    constructor(private authService: AuthService) {
        super();
    }

    ngOnInit(): void {
        this.dataForm = new FormGroup({
            email: new FormControl('', [Validators.required, Validators.email])
        });
    }

    async reset(dataFormValue: any) {
        const formValues = { ...dataFormValue };
        const email = formValues.email;

        let result = await this.authService.resetPassword(email);
        if (result) {
            this.operation = { body: 'Password reset email sent', type: 'success' };
        } else {
            this.operation = { body: 'Password reset failed', type: 'danger' };
        }
    }
}
