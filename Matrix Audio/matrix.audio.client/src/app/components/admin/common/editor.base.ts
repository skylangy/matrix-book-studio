import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    template: ''
})
export abstract class EditorBase<T> implements OnInit {
    model: T | undefined = undefined;
    icon = '';
    dataForm!: FormGroup;

    constructor() {
        this.dataForm = this.createForm();
    }

    ngOnInit() {

    }

    protected abstract createForm(): FormGroup;

    protected abstract onSubmit(): Promise<void>;

    protected abstract back(): void;
}