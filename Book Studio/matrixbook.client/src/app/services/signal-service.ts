import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging-services';
import { WorkProgress } from '../models/work-progress';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection?: HubConnection;
    private logger?: ILogger;

    constructor(loggingService: LoggingService) {
        this.logger = loggingService.getLogger('SignalRService');
    }

    startConnection() {
        this.hubConnection = new HubConnectionBuilder()
            .withUrl('/workprogress')
            .withAutomaticReconnect()
            .build();

        this.hubConnection
            .start()
            .then(() => this.logger?.log('Connection started'))
            .catch(err => this.logger?.error('Error while starting connection: ' + err));
    }

    addMessageListener(callback: (workProgress: WorkProgress) => void) {
        this.hubConnection?.on('WorkProgressChanged', callback);
    }
}