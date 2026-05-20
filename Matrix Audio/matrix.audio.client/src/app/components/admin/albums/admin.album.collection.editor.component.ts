import { Component } from '@angular/core';
import { CdkDragDrop, CdkDropList, CdkDrag, moveItemInArray } from '@angular/cdk/drag-drop';
import { Album, AlbumCollection } from '../../../models/album';
import { EditorBase } from '../common/editor.base';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { ImageUrlPipe } from '../../../pipes/image.pipe';
import { MillsecondsToTimePipe } from '../../../pipes/millsecond.pipe';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ILogger } from '../../../models/logger';
import { AlbumService } from '../../../services/album.service';
import { ValueService } from '../../../services/value.service';
import { PromptService } from '../../../services/prompt.service';
import { LoggingService } from '../../../services/logging.service';
import { FileUploaderComponent } from '../../file-uploder/file.uploader.component';
import { AdminAlbumSelectorComponent } from './admin.album.selector';

@Component({
    selector: 'mtx-admin-album-collection-editor',
    templateUrl: 'admin.album.collection.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ImageUrlPipe, FileUploaderComponent,
        AdminAlbumSelectorComponent,
        ReactiveFormsModule, CommonModule, FormsModule,
        CdkDropList, CdkDrag]
})
export class AdminAlbumCollectionEditorComponent extends EditorBase<AlbumCollection> {
    private logger: ILogger;
    selectedAlbums: Album[] = [];

    constructor(private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly albumService: AlbumService,
        private readonly valueService: ValueService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'book';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.albumService.getAlbumCollectionAdmin(id);

                this.dataForm.patchValue(this.model);
            } else {
                let now = new Date();
                this.model = {
                    id: Math.random().toString(36).substring(2),
                    name: '',
                    description: '',
                    count: 0,
                    albums: [],
                    dateCreated: now,
                    dateUpdated: now,
                };
            }
            this.logger.info('Album collection loaded', this.model);
        }
        );
    }

    override createForm() {
        return new FormGroup({
            name: new FormControl('', [Validators.required]),
            description: new FormControl('', [Validators.required]),
            level: new FormControl(1000, [Validators.required, Validators.min(0)]),
        });
    }

    override async onSubmit() {
        if (this.dataForm.invalid) {
            this.logger.warn('Form is invalid', this.dataForm.errors);
            return;
        }

        if (this.model) {
            // this.model = { ...this.model, ...this.dataForm.value };
            this.model!.name = this.dataForm.value.name;
            this.model!.description = this.dataForm.value.description;
            this.model!.albumIds = {};

            if (this.model!.albums) {
                let index = 0;
                for (let album of this.model!.albums) {
                    if (!this.model!.albumIds.hasOwnProperty(album.id!)) {
                        this.model!.albumIds[album.id!] = index++;
                    }
                }
            }

            this.logger.info('Album collection to update', this.model);

            await this.albumService.updateAlbumCollection(this.model);
            this.promptService.showSuccess('Collection saved', `Album collection "${this.model.name}" saved`);
        }
    }

    override back() {
        this.router.navigate(['/control-tower/album-collections']);
    }

    albumSelected(albums: Album[]) {
        this.logger.info('Albums selected', albums);
        this.selectedAlbums = albums;
    }

    addAlbums() {
        if (!this.model!.albums) {
            this.model!.albums = [];
        }
        for (let album of this.selectedAlbums) {
            if (!this.model!.albums.find(a => a.id === album.id)) {
                this.model!.albums.push(album);
            }

            if (!this.model!.image) {
                this.model!.image = album.imageWideSplash;
            }
        }
    }

    removeAlbum(album: Album) {
        if (this.model!.albums && album) {
            this.model!.albums = this.model!.albums.filter(a => a.id !== album.id);
        }
    }

    async drop(event: CdkDragDrop<Album[]>) {
        if (!this.model!.albums) {
            return;
        }

        let draggedAlbum = this.model!.albums[event.previousIndex];
        if (!draggedAlbum) {
            return;
        }
        moveItemInArray(this.model!.albums, event.previousIndex, event.currentIndex);
    }

    removeSplash() { }

    onFileReady(event: any) { }
}