import { Component } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { WorkProgressService } from 'src/app/services/work-progress-service';
import { WorkProgress } from 'src/app/models/work-progress';
import { SignalRService } from 'src/app/services/signal-service';
import { DatePipe, CommonModule } from '@angular/common';

@Component({
    selector: 'mtx--log-pane',
    templateUrl: './log.pane.component.html',
    imports: [CommonModule, DatePipe],
})
export class LogPaneComponent extends PaneBaseComponent {
    workItems: WorkProgress[] = [];

    constructor(
        private workProgressService: WorkProgressService,
        private signalRService: SignalRService) {
        super();
    }

    override async ngOnInit() {
        await this.load();
        this.signalRService.addMessageListener(async (workProgress: WorkProgress) => {
            if (workProgress.category === this.book?.title) {
                console.log('Received work progress', workProgress);
                this.workItems.unshift(workProgress);
            }
        });
    }

    async load() {
        let name = this.book?.title || '';
        this.workItems = await this.workProgressService.getWorkProgresses(name);
    }

    calculateProgress(workItem: WorkProgress): string {
        if (workItem.total === 0) {
            return '0';
        }
        return `${((workItem.current || 0) / (workItem.total || 100)) * 100}%`;
    }
}
