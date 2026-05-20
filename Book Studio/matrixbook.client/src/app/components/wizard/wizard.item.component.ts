import { Component, Input, OnInit, TemplateRef } from '@angular/core';
import { WizardItem } from './wizard-item';
import { NgTemplateOutlet } from '@angular/common';

@Component({
  selector: 'mtx-wizard-item',
  template: `
    @if (isActive) {
      <div>
        <ng-template #default let-context>
          @if (context) {
          }
        </ng-template>
        <ng-content></ng-content>
        <ng-container *ngTemplateOutlet="template? template: default; context: { $implicit: context }" ></ng-container>
      </div>
    }
    `,

  imports: [NgTemplateOutlet],
})
export class WizardItemComponent implements OnInit {
  @Input() template!: TemplateRef<any>;
  @Input() context?: WizardItem;

  constructor() { }

  ngOnInit(): void { }

  get isActive(): boolean {
    return this.context?.isActive || false;
  }
}
