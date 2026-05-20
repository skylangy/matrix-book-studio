import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { MediaResource } from '../models/media-resource';

@Injectable({ providedIn: 'root' })
export class ResourceSelectService {
    onShowModal: Subject<any> = new Subject<any>();
    onResourceSelected: Subject<MediaResource | null> = new Subject<MediaResource | null>();

    constructor() { }

    openModal(modal: any = null) {
        this.onShowModal.next(modal);
    }

    resourceSelected(resource: MediaResource | null) {
        this.onResourceSelected.next(resource);
    }
}