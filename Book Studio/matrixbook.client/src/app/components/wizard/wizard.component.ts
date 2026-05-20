import { NgClass } from '@angular/common';
import { Component, ContentChildren, EventEmitter, OnInit, Output, QueryList } from '@angular/core';
import { WizardItemComponent } from './wizard.item.component';

@Component({
    selector: 'mtx-wizard',
    templateUrl: `./wizard.component.html`,
    imports: [NgClass],
})
export class WizardComponent implements OnInit {
    @ContentChildren(WizardItemComponent) items!: QueryList<WizardItemComponent>;
    @Output() finished = new EventEmitter<any>();
    current?: WizardItemComponent;
    index = 0;
    nextLabel = 'Next';

    // constructor(private zone: NgZone) { }

    ngOnInit(): void {

    }

    ngAfterViewInit(): void {
        // this.zone.runOutsideAngular(() => {
        //     setTimeout(() => {
        //         this.zone.run(() => {
        //             this.current = this.items?.first;
        //         });
        //     }, 0);
        // });

        setTimeout(() => {
            this.current = this.items?.first;
        }, 0);
    }

    canGoPrevious(): boolean {
        return (this.current && this.current !== this.items.first) || false;
    }

    goPrevious(): void {
        if (this.current) {
            let context = this.current.context!;
            context.isActive = false;

            this.current = this.items.get(this.index - 1)
            this.index--;
            if (this.current) {
                context = this.current.context!;
                context.isActive = true;
            }

            if (this.current !== this.items.last) {
                this.nextLabel = 'Next';
            }
        }
    }

    canGoNext(): boolean {
        return true; //(this.current && this.current !== this.items.last) || false;
    }

    goNext(): void {
        if (this.current) {

            if (this.current === this.items.last) {
                this.finished.emit();
                return;
            }

            let context = this.current.context!;
            context.isActive = false;

            this.current = this.items.get(this.index + 1)
            this.index++;
            if (this.current) {
                context = this.current.context!;
                context.isActive = true;
            }

            if (this.current === this.items.last) {
                this.nextLabel = 'Finish';
            }
        }
    }
}
