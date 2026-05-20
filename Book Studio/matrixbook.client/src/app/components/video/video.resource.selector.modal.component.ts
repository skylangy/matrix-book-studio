import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MediaResource } from 'src/app/models/media-resource';
import { ResourceSelectService } from 'src/app/services/resource-select-service';
import { VideoResourceSelectorComponent } from './video.resource.selector.component';

@Component({
  selector: 'mtx-video-resource-selector-modal',
  templateUrl: 'video.resource.selector.modal.component.html',
  imports: [CommonModule, FormsModule, VideoResourceSelectorComponent],
  styles: [`
    .modal {
      display: none;
    }
    .show {
      display: block;
    }
  `]
})
export class VideoResourceSelectorModalComponent implements OnInit {
  @Input() showResourceSelector: boolean = false;
  @Input() resourceType: string = 'image';
  @Input() enableUpload: boolean = true;
  @Input() enableTypeSelector: boolean = false;
  @Output() closed = new EventEmitter<void>();
  @Output() resourceSelected = new EventEmitter<MediaResource>();

  resourceTypeToSelect: string = 'image';
  selectedResource: MediaResource | null = null;
  selectResourceAction = (resource: MediaResource) => { };

  constructor(private readonly renderer: Renderer2,
    private readonly resourceSelectService: ResourceSelectService
  ) { }

  ngOnInit() { }

  onResourceSelected(resource: MediaResource) {
    this.selectedResource = resource;
    if (this.selectResourceAction) {
      this.selectResourceAction(resource);
    }
    this.resourceSelectService.resourceSelected(this.selectedResource);
  }

  closeResourceSelector() {
    this.showResourceSelector = false;
    this.renderer.removeClass(document.body, 'modal-open');
  }

  openModal() {
    this.showResourceSelector = true;
    this.renderer.addClass(document.body, 'modal-open');
  }
}