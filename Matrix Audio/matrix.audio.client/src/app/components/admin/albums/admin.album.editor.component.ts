import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AlbumService } from '../../../services/album.service';
import { Album } from '../../../models/album';
import { AdminContainerComponent } from '../admin-container/admin.container.component';
import { HeaderContentComponent } from '../../header-content/header-content.component';
import { ImageUrlPipe } from '../../../pipes/image.pipe';
import { CommonModule, DatePipe } from '@angular/common';
import { EditorBase } from '../common/editor.base';
import { LoggingService } from '../../../services/logging.service';
import { ILogger } from '../../../models/logger';
import { ValueService } from '../../../services/value.service';
import { Episode } from '../../../models/episode';
import { MillsecondsToTimePipe } from '../../../pipes/millsecond.pipe';
import { NamedSelectableValue } from '../../../models/namevalue';
import { PromptService } from '../../../services/prompt.service';

@Component({
    selector: 'mtx-admin-album-editor',
    templateUrl: 'admin.album.editor.component.html',
    imports: [
        HeaderContentComponent, AdminContainerComponent,
        ImageUrlPipe, MillsecondsToTimePipe,
        ReactiveFormsModule, CommonModule, FormsModule]
})
export class AdminAlbumEditorComponent extends EditorBase<Album> {
    private logger: ILogger;

    albumInfo: { name: string, value: string }[] = [];
    episodes: Episode[] = [];
    levels: NamedSelectableValue[] = [
        { name: 'Public', value: 1000, selected: true },
        { name: 'Preimum', value: 5000, selected: false },
        { name: 'PreimumPro', value: 10000, selected: false }
    ];

    constructor(
        private readonly router: Router,
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
            const albumId = params['id'];
            if (albumId) {
                this.model = await this.albumService.getAlbum(albumId);

                this.episodes = this.albumService.sortEpisodesByChapter(this.model.episodes ?? []);

                this.albumInfo = [
                    { name: 'Author', value: this.model.artist?.name ?? this.model.artistName ?? '' },
                    { name: 'Updated', value: new DatePipe('en').transform(this.model.dateUpdated) ?? '' },
                    { name: 'Duration', value: this.valueService.formatSeconds(this.model.durationInSeconds) ?? '0' },
                    { name: 'Episodes', value: this.model.episodeCount?.toString() ?? '0' },
                    { name: 'Play', value: this.model.playCount?.toString() ?? '0' },
                    { name: 'Likes', value: this.model.likes?.toString() ?? '0' },
                    { name: 'Downloads', value: this.model.downloadCount?.toString() ?? '0' },
                    { name: 'Comments', value: this.model.commentCount?.toString() ?? '0' },
                ];

                this.dataForm.patchValue(this.model);
            }
            this.logger.info('Album loaded', this.model);
        }
        );
    }

    override createForm() {
        return new FormGroup({
            title: new FormControl('', [Validators.required]),
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
            this.model!.title = this.dataForm.value.title;
            this.model!.description = this.dataForm.value.description;
            this.model!.level = this.dataForm.value.level;

            this.logger.info('Album to update', this.model);

            await this.albumService.updateAlbum(this.model);
            this.promptService.showSuccess('Album saved', `Album "${this.model.title}" saved`);
        }
    }

    override back() {
        this.router.navigate(['/control-tower/albums']);
    }
}