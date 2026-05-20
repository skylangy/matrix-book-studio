import { Component, Input, OnInit, Output, EventEmitter, input } from '@angular/core';

@Component({
    selector: 'mtx-toggle-button',
    templateUrl: './toggle-button.component.html',

})
export class ToggleButtonComponent implements OnInit {
    @Input() onStatusIcon: string = '';
    @Input() offStatusIcon: string = '';
    @Input() title: string = '';
    @Input() buttonStyle = '';
    @Input() isEnabled: boolean = true;

    @Output() statusChanged = new EventEmitter<boolean>();

    private status: boolean = false;
    statusIcon = '';

    constructor() { }

    ngOnInit(): void {
        this.statusIcon = this.isOn ? this.onStatusIcon : this.offStatusIcon;
    }

    @Input()
    get isOn(): boolean {
        return this.status;
    }

    set isOn(value: boolean) {
        this.status = value;
        this.statusIcon = this.status ? this.onStatusIcon : this.offStatusIcon;
    }

    toggleStatus() {
        this.isOn = !this.isOn;
        this.statusIcon = this.isOn ? this.onStatusIcon : this.offStatusIcon;
        this.statusChanged.emit(this.isOn);
    }
}
