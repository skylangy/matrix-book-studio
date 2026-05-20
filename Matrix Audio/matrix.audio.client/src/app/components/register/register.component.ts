import { Component, OnInit } from '@angular/core';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ImagePaneComponent } from '../img-pane/img-pane.compnent';
import { Router, RouterModule } from '@angular/router';
import { SplitLayoutComponent } from '../split-layout/split-layout.component';
import { AlertComponent } from '../alert/alert.component';
import { FormBasedComponent } from '../form-component/form.component';
import { AuthService } from '../../services/auth.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'mtx-register',
    templateUrl: './register.component.html',
    imports: [RouterModule, ReactiveFormsModule, ImagePaneComponent, SplitLayoutComponent, AlertComponent,
        TranslatePipe
    ]
})
export class RegisterComponent extends FormBasedComponent implements OnInit {

    constructor(
        private router: Router,
        private formBuilder: NonNullableFormBuilder,
        private authService: AuthService) {
        super();
    }

    ngOnInit(): void {
        this.dataForm = this.formBuilder.group({
            email: new FormControl('', [Validators.required, Validators.email]),
            name: new FormControl('', [Validators.required]),
            password: new FormControl('', [Validators.required])
        });
    }

    async register(dataFormValue: any) {
        const formValues = { ...dataFormValue };
        const user = {
            email: formValues.email,
            name: formValues.name,
            password: formValues.password
        };

        let result = await this.authService.register(user.email, user.name, user.password);
        if (result) {
            this.operation = { body: 'Registration successful', type: 'success' };

            setTimeout(() => {
                this.router.navigate(['/login']);
            }, 2000);
        } else {
            this.operation = { body: 'Registration failed', type: 'danger' };
        }
    }
}
