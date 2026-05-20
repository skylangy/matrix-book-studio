import { Component } from '@angular/core';
import { PaneBaseComponent } from './pane.base.component';
import { Chapter } from 'src/app/models/chapter';
import { ImageService } from 'src/app/services/image-service';

@Component({
    template: '',

})
export class ExportBasePaneComponent extends PaneBaseComponent {
    chapters: Chapter[] = [];
    exportType: string = '';

    constructor(private imageService: ImageService) { super(); }

    override async ngOnInit() {
        super.ngOnInit();
        await this.refresh();
    }

    async refresh() {
        if (this.context && this.context.rawEditor) {
            this.chapters = [];

            let chapterService = this.context.chapterService;
            if (chapterService) {
                let model = this.context.rawEditor.getModel();
                let content = model.getValue();
                this.chapters = chapterService.toChapters(model, content, this.book?.title);
            }
        }
    }

    getSelectedChapters(): Chapter[] {
        return this.chapters.filter(c => c.isSelected);
    }

    toggleSelectAll() {
        let selected = this.getSelectedChapters().length > 0;
        for (let chapter of this.chapters) {
            chapter.isSelected = !selected;
        }
    }

    exportSelected() {
        let chapters = this.getSelectedChapters();
        for (let chapter of chapters) {
            chapter.chunks = this.context?.chapterService?.toChunks(chapter);
        }
        this.exportChapters(chapters);
    }

    async exportChapters(chapters: Chapter[]) {
        let exportService = this.context.exportService;
        let bookName = this.context.book!.title!;
        if (exportService === undefined || bookName === undefined) {
            return;
        }

        try {
            let book = this.context.book!;
            let image = await this.imageService.getImageResourceById(book.defaultImageId!);

            exportService.exportChapters({
                bookName: bookName,
                author: book.author!,
                type: this.exportType,
                chapters: chapters,
                image: image?.fileName,
                speechService: book.speechService,
                language: book.language,
                voiceName: book.voiceName
            });
            this.context?.editor?.showBottomPane('LoggingPane');
        } catch (e) {
            this.context.notificationService?.showFail('Export', `Export with error: ${e}`);
        }
    }
}
