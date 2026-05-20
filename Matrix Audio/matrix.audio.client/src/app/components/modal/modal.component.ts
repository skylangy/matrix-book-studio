import { Component, Input, OnInit, Renderer2 } from '@angular/core';
import { IModal, IModalAction } from '../../models/modal';

@Component({
    selector: 'mtx-modal',
    templateUrl: './modal.component.html',
    styles: [`
    .modal {
      display: none;
    }
    .show {
      display: block;
    }
  `],

})
export class ModalComponent implements OnInit {
    @Input() modal?: IModal;
    isVisible: boolean = false;

    constructor(private readonly renderer: Renderer2) {

    }

    ngOnInit(): void { }

    openModal(modal?: IModal) {
        this.modal = modal;
        this.isVisible = true;
        this.renderer.addClass(document.body, 'modal-open');
    }

    closeModal() {
        this.isVisible = false;
        this.renderer.removeClass(document.body, 'modal-open');
    }

    buttonClicked(button: IModalAction) {
        if (button.action) {
            if (button.action()) {
                this.closeModal();
            }
        }
    }
}
