import { Injectable } from '@angular/core';
import { IModal } from '../models/modal';
import { Subject } from 'rxjs';
import { IModalService } from '../models/modal-service';

@Injectable({
    providedIn: 'root'
})
export class ModalService implements IModalService {
    onShowModal: Subject<IModal> = new Subject<IModal>();

    constructor() { }

    openModal(modal: IModal): void {
        this.onShowModal.next(modal);
    }
}