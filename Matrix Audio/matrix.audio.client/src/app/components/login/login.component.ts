import { Component, OnInit } from '@angular/core';
import { SplitLayoutComponent } from '../split-layout/split-layout.component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';
import { ImagePaneComponent } from '../img-pane/img-pane.compnent';
import { AlertComponent } from '../alert/alert.component';
import { FormBasedComponent } from '../form-component/form.component';
import { Router } from '@angular/router';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';

@Component({
    selector: 'mtx-login',
    templateUrl: './login.component.html',
    imports: [RouterModule, ReactiveFormsModule, ImagePaneComponent, SplitLayoutComponent, AlertComponent,
        TranslatePipe
    ]
})
export class LoginComponent extends FormBasedComponent implements OnInit {
    private readonly logger?: ILogger;

    constructor(
        private router: Router,
        private authService: AuthService,
        loggingService: LoggingService) {
        super();
        this.logger = loggingService.getLogger('Login');
    }

    ngOnInit(): void {
        this.dataForm = new FormGroup({
            email: new FormControl('', [Validators.required, Validators.email]),
            password: new FormControl('', [Validators.required])
        });
    }

    async login(dataFormValue: any) {
        if (this.operation) {
            this.operation.body = '';
        }

        const formValues = { ...dataFormValue };
        const user = {
            email: formValues.email,
            password: formValues.password
        };

        let result = await this.authService.login(user.email, user.password);

        if (result) {
            this.logger?.info('Login successful', result);
            this.router.navigate(['/']);
        } else {
            this.operation = { body: 'Login failed', type: 'danger' };
        }
    }
}
