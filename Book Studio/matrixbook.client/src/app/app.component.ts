import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { MainStatusbarComponent } from './components/main-statusbar/statusbar.component';
import { ModalComponent } from './components/modal/modal-component';
import { MONACO_EDITOR_CONFIG, MonacoEditorConfig } from './components/monaco-editor/models';
import { NotificationComponent } from './components/notification/notification.component';
import { CompactSidebarComponent } from './components/side-bar/sidebar.compact.component';
import { VideoResourceSelectorModalComponent } from './components/video/video.resource.selector.modal.component';
import { ModalService } from './services/modal-service';
import { ResourceSelectService } from './services/resource-select-service';
import { SignalRService } from './services/signal-service';

const monacoConfig: MonacoEditorConfig = {
  baseUrl: 'assets',
  defaultOptions: { scrollBeyondLastLine: true },
  onMonacoLoad: () => {
    console.log('Monaco Editor loaded successfully from app component.');
  }
};

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  imports: [RouterModule, CompactSidebarComponent, NotificationComponent, ModalComponent,
    MainStatusbarComponent,
    VideoResourceSelectorModalComponent
  ],
  providers: [
    { provide: MONACO_EDITOR_CONFIG, useValue: monacoConfig },
  ]
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild('modalRef') modalRef!: ModalComponent;
  @ViewChild('resourceSelector') resourceSelector!: VideoResourceSelectorModalComponent;

  title = 'Book Studio';

  private modalSubscription?: Subscription;
  private resourceSelectSubscription?: Subscription;

  constructor(
    private modalService: ModalService,
    private resourceSelectService: ResourceSelectService,
    private signalRService: SignalRService) {
  }

  ngOnInit(): void {
    this.modalSubscription = this.modalService.onShowModal.subscribe(modal => {
      this.modalRef.openModal(modal);
    });

    this.resourceSelectSubscription = this.resourceSelectService.onShowModal.subscribe(modal => {
      this.resourceSelector.openModal();
    });

    this.signalRService.startConnection();
  }

  ngOnDestroy(): void {
    if (this.modalSubscription) {
      this.modalSubscription.unsubscribe();
    }

    if (this.resourceSelectSubscription) {
      this.resourceSelectSubscription.unsubscribe();
    }
  }
}
