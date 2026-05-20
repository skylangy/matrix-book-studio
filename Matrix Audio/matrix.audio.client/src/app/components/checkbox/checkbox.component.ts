import { Component, Input, OnInit, output } from '@angular/core';

@Component({
    selector: 'mtx-checkbox',
    templateUrl: './checkbox.component.html',
    imports: []
})
export class CheckboxComponent implements OnInit {
    @Input() label?: string;
    @Input() isChecked: boolean = false;
    isCheckedChange = output<boolean>();

    id = Math.random().toString(36).substring(2);

    constructor() { }

    ngOnInit(): void { }

    onCheckedChange(event: Event) {
        this.isChecked = (event.target as HTMLInputElement).checked;
        this.isCheckedChange.emit(this.isChecked);
    }
}
