import { FormGroup } from '@angular/forms';
import { IOperationStatus } from '../../models/operation-status';


export class FormBasedComponent {
    dataForm!: FormGroup;
    operation!: IOperationStatus;

    constructor() { }

    get hasOperation(): boolean {
        return this.operation && this.operation.body !== undefined && this.operation.body?.length > 0;
    }

    validateControl = (controlName: string) => {
        let control = this.dataForm.get(controlName);
        return control?.invalid
            && control?.touched
    }

    hasError = (controlName: string, errorName: string) => {
        let control = this.dataForm.get(controlName);
        return control?.hasError(errorName)
    }

    check(controlName: string, errorName: string) {
        let control = this.dataForm.get(controlName);
        return control?.touched && control.hasError(errorName);
    }
}
