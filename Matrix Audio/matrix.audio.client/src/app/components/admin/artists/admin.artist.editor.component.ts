import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { ImageUrlPipe } from '../../../pipes/image.pipe';
import { CommonModule } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { ValueService } from '../../../services/value.service';
import { Artist } from '../../../models/artist';
import { AlbumService } from '../../../services/album.service';
import { PromptService } from '../../../services/prompt.service';
import { ArtistService } from '../../../services/artist.service';

@Component({
    selector: 'mtx-admin-artist-editor',
    templateUrl: 'admin.artist.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ImageUrlPipe,
        ReactiveFormsModule, CommonModule, FormsModule, RouterModule]
})
export class AdminArtistEditorComponent extends EditorBase<Artist> {
    private logger: ILogger;

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly artistService: ArtistService,
        private readonly promptService: PromptService,
        loggingService: LoggingService
    ) {
        super();
        this.icon = 'people';
        this.logger = loggingService.getLogger('AdminAlbumEditorComponent');
    }

    override async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            const id = params['id'];
            if (id) {
                this.model = await this.artistService.getArtist(id);

                this.dataForm.patchValue(this.model);
            }
            this.logger.info('Artist loaded', this.model);
        }
        );
    }

    override createForm() {
        return new FormGroup({
            name: new FormControl('', [Validators.required]),
            alias: new FormControl('', [Validators.required]),
            description: new FormControl('', [Validators.required]),
            dateCreated: new FormControl(new Date()),
            dateUpdated: new FormControl(new Date()),
            image: new FormControl('')
        });
    }

    override  async onSubmit() {
        if (this.dataForm.invalid) {
            this.logger.warn('Form is invalid', this.dataForm.errors);
            return;
        }

        if (this.model) {
            const artist = this.dataForm.value as Artist;
            artist.id = this.model.id;

            await this.artistService.updateArtist(artist);

            this.logger.info('Artist updated', artist);
            this.promptService.showSuccess('Success', `Artist '${artist.name}' is updated successfully`);
        }
    }

    override back() {
        this.router.navigate(['/control-tower/artists']);
    }

    removeAvatar() { }
}