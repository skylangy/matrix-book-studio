import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { StatusContext } from '../models/status-context';

@Injectable({
    providedIn: 'root'
})
export class StatusService {
    contextSubject = new Subject<StatusContext | undefined>();

    setContext(context: StatusContext | undefined) {
        this.contextSubject.next(context);
    }
}