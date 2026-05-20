import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Prompt } from '../../models/prompt';
import { PromptService } from '../../services/prompt.service';
import { Subscription } from 'rxjs';
import { NgClass } from '@angular/common';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';

@Component({
    selector: 'mtx-prompt',
    templateUrl: './prompt.component.html',
    imports: [NgClass, TimeAgoPipe]
})
export class PromptComponent implements OnInit, OnDestroy {

    @Input() prompt: Prompt | undefined;
    @Input() delay: number = 15000;

    showToast: boolean = false;
    private subscription?: Subscription;

    constructor(private readonly promptService: PromptService) {

    }

    ngOnInit(): void {
        this.subscription = this.promptService.displayPrompt.subscribe(prompt => {
            this.prompt = prompt;

            this.showToast = true;

            setTimeout(() => {
                this.showToast = false;
            }, this.delay);
        });
    }

    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    closeToast() {
        this.showToast = false;
    }
}

