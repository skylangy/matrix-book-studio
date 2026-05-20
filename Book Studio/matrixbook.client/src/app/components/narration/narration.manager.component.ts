import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ILogger } from 'src/app/models/logger';
import { Narration } from 'src/app/models/narration';
import { LoggingService } from 'src/app/services/logging-services';
import { NarrationService } from 'src/app/services/narration-service';
import { HeaderComponent } from '../header/header.component';
import { LoadingComponent } from '../loading/loading.component';
import { WorkViewComponent } from '../work-view/work.view.component';
import { NarrationCardComponent } from './narration.card.component';

@Component({
    selector: 'mtx-narration-manager',
    templateUrl: 'narration.manager.component.html',
    imports: [CommonModule,
        FormsModule,
        HeaderComponent,
        LoadingComponent,
        NarrationCardComponent
    ]
})
export class NarrationManagerComponent extends WorkViewComponent {
    filterText = '';
    isLoading = signal(false);
    models = signal<Narration[]>([]);
    private logger: ILogger;

    constructor(private readonly narrationService: NarrationService,
        loggerService: LoggingService
    ) {
        super();
        this.title = 'Narrations';
        this.bannerImage = './assets/images/splashes/photo-12.jpg';
        this.logger = loggerService.getLogger('NarrationManagerComponent');
    }

    get filter() {
        return this.filterText;
    }

    set filter(value: string) {
        if (this.filterText !== value) {
            this.filterText = value;
            this.onFilterChanged(value);
        }
    }

    override async ngOnInit() {
        this.isLoading.set(true);
        this.models.set(await this.narrationService.getAll());

        this.isLoading.set(false);
    }

    onFilterChanged(filter: string): void {

    }

    createNewNarration() {
    }

    onNarrationDeleted(id: string) {
    }
}