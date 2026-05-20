import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { EditorContext } from '../../models/editor-context';
import { EditorPane } from '../../models/editor-pane';
import { NgTemplateOutlet } from '@angular/common';
import { ToolPaneComponent } from './toolpane.component';
import { LogPaneComponent } from '../panes/log.pane.component';
import { RegexPaneComponent } from '../panes/regex.pane.component';
import { ExportTextPaneComponent } from '../panes/export.text.pane.component';
import { ExportVideoPaneComponent } from '../panes/export.video.pane.component';
import { ExportMp3PaneComponent } from '../panes/export.mp3.pane.component';
import { ExportWavPaneComponent } from '../panes/export.wav.pane.component';
import { VideoPaneComponent } from '../panes/video.pane.component';
import { Mp3PaneComponent } from '../panes/mp3.pane.component';
import { WavPaneComponent } from '../panes/wav.pane.component';
import { ImagesPaneComponent } from '../panes/images.pane.component';
import { BookPropertyPaneComponent } from '../panes/book.property.pane.component';
import { ActionsPaneComponent } from '../panes/actions.pane.component';
import { ValidatePaneComponent } from '../panes/validate.pane.component';
import { OutlinePaneComponent } from '../panes/outline.pane.component';

@Component({
    selector: 'mtx-tool-panes',
    templateUrl: './toolpanes.component.html',
    imports: [
        ActionsPaneComponent,
        OutlinePaneComponent,
        ValidatePaneComponent,
        BookPropertyPaneComponent,
        ImagesPaneComponent,
        WavPaneComponent,
        Mp3PaneComponent,
        VideoPaneComponent,
        ExportWavPaneComponent,
        ExportMp3PaneComponent,
        ExportVideoPaneComponent,
        ExportTextPaneComponent,
        RegexPaneComponent,
        LogPaneComponent,
        ToolPaneComponent,
        NgTemplateOutlet,
    ]
})
export class ToolPanesComponent implements OnInit {
    @Input() context?: EditorContext;
    @Input() panes?: EditorPane[] = [];
    @Output() paneClosed = new EventEmitter<EditorPane>();

    activePane?: EditorPane;

    constructor() { }

    ngOnInit(): void {
        if (this.panes && this.panes!.length > 0) {
            this.activePane = this.panes?.[0];
        }
    }

    closePane(pane: EditorPane) {
        if (pane) {
            let key = `show${pane.name}`;
            this.context!.configuration![key] = false;
            this.paneClosed.emit(pane);
        }
    }

    switchTo(pane: EditorPane) {
        let key = ''
        if (this.activePane) {
            key = `show${this.activePane.name}`;
            this.context!.configuration![key] = false;
        }

        if (pane) {
            key = `show${pane.name}`;
            this.context!.configuration![key] = true;
            this.activePane = pane;
        }
    }

    getPaneActive(pane: EditorPane): boolean {
        let key = `show${pane.name}`;
        return this.context?.configuration?.[key];
    }
}
