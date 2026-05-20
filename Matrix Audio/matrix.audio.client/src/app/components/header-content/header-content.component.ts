import { Component, Input, OnInit, input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateService } from '../../services/translate.service';

@Component({
    selector: 'mtx-header-content',
    templateUrl: './header-content.component.html',
    imports: [RouterModule]
})
export class HeaderContentComponent implements OnInit {
    @Input() header: string = '';
    @Input() subHeader: string = '';
    @Input() moreLink: string = 'Show more';
    @Input() moreLinkUrl: string = '';
    @Input() icon: string = '';

    constructor(private readonly translateService: TranslateService) { }

    ngOnInit(): void {
        this.moreLink = this.translateService.translate(this.moreLink);
    }
}
