import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ImagePaneComponent } from '../../img-pane/img-pane.compnent';
import { SplitLayoutComponent } from '../../split-layout/split-layout.component';
import { AlertComponent } from '../../alert/alert.component';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { FormBasedComponent } from '../../form-component/form.component';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'mtx-admin-login',
    templateUrl: './admin.login.component.html',
    imports: [RouterModule, ReactiveFormsModule, ImagePaneComponent,
        SplitLayoutComponent, AlertComponent
    ]
})
export class AdminLoginComponent extends FormBasedComponent implements OnInit {

    constructor(
        private router: Router,
        private authService: AuthService) {
        super();
    }

    ngOnInit(): void {
        this.dataForm = new FormGroup({
            email: new FormControl('', [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required]),
            token: new FormControl('', [Validators.required])
        });
    }

    async login(dataFormValue: any) {
        if (this.operation) {
            this.operation.body = '';
        }

        const formValues = { ...dataFormValue };
        const user = {
            email: formValues.email,
            password: formValues.password,
            token: formValues.token
        };

        let result = await this.authService.loginTower(user.email, user.password, user.token);
        if (result) {
            this.router.navigate(['/control-tower']);
        } else {
            this.operation = { body: 'Login failed', type: 'danger' };
        }
    }
}
