import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';


@Component({
    selector: 'mtx-tool-pane',
    templateUrl: './toolpane.component.html',
    imports: [CommonModule, FormsModule],
})
export class ToolPaneComponent implements OnInit {
    @Input() title?: string;
    @Input() icon?: string;
    @Input() context: any;
    @Input() closable = true;
    @Input() scrollBody = false;
    @Output() onClosed = new EventEmitter<void>();

    constructor() { }

    ngOnInit(): void { }

    close() {
        this.onClosed.emit();
    }
}
