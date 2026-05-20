import { CommonModule } from '@angular/common';
import { Component, HostListener, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Narration, NarrationBlock, NarrationBlockType } from 'src/app/models/narration';
import { GroupedNavItem, NavItem } from 'src/app/models/nav-item';
import { TaskStatus } from 'src/app/models/task-status';
import { IdGeneratorService } from 'src/app/services/id-generator';
import { KeyService } from 'src/app/services/key-service';
import { NarrationService } from 'src/app/services/narration-service';
import { RibbonComponent } from '../toolbar/ribbon.component';
import { NarrationBlockEditorComponent } from './narration.block.editor.component';
import { NarrationPropertyPaneComponent } from './narration.property.component';

@Component({
    selector: 'mtx-narration-editor',
    templateUrl: 'narration.editor.component.html',
    imports: [CommonModule, FormsModule, RouterModule,
        RibbonComponent,
        NarrationBlockEditorComponent,
        NarrationPropertyPaneComponent]
})

export class NarrationEditorComponent implements OnInit {
    fileGroup = 'file';
    homeGroup = 'home';
    navItems: NavItem[] = [
        { name: 'new', text: 'New', icon: 'file', group: this.fileGroup, action: () => { this.new(); } },
        { name: 'save', text: 'Save', icon: 'floppy', group: this.fileGroup, action: () => { this.save(); } },
        { name: 'openFolder', text: 'Open Video Folder', icon: 'folder2-open', group: this.fileGroup, action: async () => { await this.openVideoFolder(); } },
        { name: 'file-separator1', isSeparator: true, group: this.fileGroup },
        { text: 'Close', icon: 'backspace', group: this.fileGroup, action: () => { } },

        // home
        { name: 'new', text: 'New', icon: 'file', group: this.homeGroup, action: () => { this.new(); } },
        { name: 'save', text: 'Save', icon: 'floppy', group: this.homeGroup, action: () => { this.save(); } },
        { name: 'sep1', isSeparator: true, group: this.homeGroup },
        { name: 'addBlock', text: 'Add Block', icon: 'plus-square', group: this.homeGroup, action: () => { this.addBlock(); } },
        { name: 'sep2', isSeparator: true, group: this.homeGroup },
        { name: 'export', text: 'Export', icon: 'send text-success', group: this.homeGroup, action: () => { this.export(); } }

    ];
    fileNavItems?: NavItem[] = this.filterByGroup(this.navItems, this.fileGroup);
    mainNav?: GroupedNavItem[] = [
        { title: 'Home', items: this.filterByGroup(this.navItems, this.homeGroup), target: '#home-tab', isActive: true },
    ];

    @Input() model!: Narration;
    isNew = false;
    selectedBlock?: NarrationBlock;

    constructor(
        private readonly activateRoute: ActivatedRoute,
        private readonly router: Router,
        private readonly narrationService: NarrationService,
        private readonly idGeneratorService: IdGeneratorService,
        private readonly keyService: KeyService) { }

    ngOnInit() {
        this.activateRoute.params.subscribe(async params => {
            let videoId = params['id?'];
            if (videoId) {
                this.model = await this.narrationService.getById(videoId);
            } else {
                this.model = {
                    id: this.idGeneratorService.generateId(),
                    title: '',
                    blocks: [],
                    category: '',
                    tags: '',
                    status: TaskStatus.InProgress,
                    dateCreated: new Date(),
                    dateUpdated: new Date()
                };
                this.addBlock();
                this.isNew = true;
            }
        });
    }

    @HostListener('document:keydown', ['$event'])
    handleKeyDown(event: KeyboardEvent): void {
        this.keyService.handleKeyDown(event, this, this.keyService.narrationEditorName);
    }

    filterByGroup(items: NavItem[], group: string): NavItem[] {
        return items.filter(item => item.group === group);
    }

    async new() {
    }

    async save() {
    }

    async openVideoFolder() {
    }

    async export() {
    }

    async addBlock() {
        let block = {
            id: this.idGeneratorService.generateId(),
            content: '',
            type: NarrationBlockType.Statement,
            order: this.model.blocks.length + 1
        };
        this.model.blocks.push(block);
    }

    onBlockSelected(block: NarrationBlock) {
        this.selectedBlock = block;
    }

    onBlockDeleted(blockId: string) {
        this.model.blocks = this.model.blocks.filter(block => block.id !== blockId);

        let order = 1;
        for (let block of this.model.blocks) {
            block.order = order++;
        }
    }
}