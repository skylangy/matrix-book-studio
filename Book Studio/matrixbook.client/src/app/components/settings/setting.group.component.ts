import { Component, Input, OnInit } from '@angular/core';
import { OptionGroup } from 'src/app/models/options';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'mtx-setting-group',
    templateUrl: './setting.group.component.html',

    imports: [FormsModule]
})
export class SettingGroupComponent implements OnInit {
    @Input() group?: OptionGroup;

    constructor() { }

    ngOnInit(): void { }
}
