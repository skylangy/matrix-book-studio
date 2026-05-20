import { Injectable } from '@angular/core';

import { Subject } from 'rxjs';
import { IModal } from '../models/modal';

@Injectable({
    providedIn: 'root'
})
export class ModalService {
    onShowModal: Subject<IModal> = new Subject<IModal>();

    constructor() { }

    openModal(modal: IModal): void {
        this.onShowModal.next(modal);
    }
}