import { ExportBook } from "src/app/models/book";
import { IEditor } from "src/app/models/editor";
import { EditorContext } from "src/app/models/editor-context";
import { EditorPane } from "src/app/models/editor-pane";
import { EditorView } from "src/app/models/editor-view";
import { NavItem } from "src/app/models/nav-item";
import { TaskStatus } from "src/app/models/task-status";

/**
 * Steps to add a new pane
 * 1. Add a new nav item
 * 2. Add a new pane
 * 3. Add ng-template for the pane in toolspanes component
 */
export class EditorViewCreator implements IEditor {
    fileGroup = 'file';
    homeGroup = 'home';
    contentGroup = 'content';
    exportGroup = 'export';
    viewGroup = 'view';
    toolsGroup = 'tools';
    locationRight = 'right';
    locationLeft = 'left';
    locationBottom = 'bottom';

    activeRightSidePane?: string;
    activeBottomPane?: string;
    activeLeftPane?: string;

    layoutAction?: () => void;
    navItems: NavItem[] = [
        //file
        { name: 'save', text: 'Save', icon: 'floppy', group: this.fileGroup, action: () => { this.save(); } },
        { name: 'openFolder', text: 'Open Book Folder', icon: 'folder2-open', group: this.fileGroup, action: () => { this.openFolder(); } },
        { name: 'openAuthorFolder', text: 'Open Author Folder', icon: 'folder-check', group: this.fileGroup, action: () => { this.openAuthorFolder(); } },
        { name: 'file-separator1', isSeparator: true, group: this.fileGroup },
        { name: 'reset', text: 'Reset Status', icon: 'align-start', group: this.fileGroup, action: () => { this.resetBookStatus(); } },
        { name: 'finish', text: 'Finish', icon: 'check-all', group: this.fileGroup, action: () => { this.finishBook(); } },
        { name: 'preview', text: 'Preview', icon: 'display', group: this.fileGroup, action: () => { this.preview(); } },
        { name: 'generateMeta', text: 'Generate Metadata', icon: 'easel', group: this.fileGroup, action: () => { this.generateMeta(); } },
        { name: 'publishToLibrary', text: 'Sync to Library', icon: 'collection', group: this.fileGroup, action: () => { this.syncToLibrary(); } },
        { name: 'file-separator2', isSeparator: true, group: this.fileGroup },
        { text: 'Close', icon: 'backspace', group: this.fileGroup, action: () => { this.close(); } },

        // home
        { name: 'save', text: 'Save', icon: 'floppy', group: this.homeGroup, action: () => { this.save(); } },
        { name: 'sep1', isSeparator: true, group: this.homeGroup },
        { name: 'bookProperty', text: 'Book', icon: 'journal', group: this.homeGroup, action: () => { this.showRightSidePane('BookPropertyPane'); } },
        { name: 'sep3', isSeparator: true, group: this.homeGroup },
        { name: 'bookOutline', text: 'Outline', icon: 'card-list', group: this.homeGroup, isActive: true, action: () => { this.showLeftSidePane('OutlinePane'); } },
        { name: 'validate', text: 'Validate', icon: 'card-checklist', group: this.homeGroup, action: () => { this.showRightSidePane('ValidatePane'); } },
        { name: 'actions', text: 'Actions', icon: 'bookmarks', group: this.homeGroup, isActive: true, action: () => { this.showRightSidePane('ActionsPane'); } },
        { name: 'regexes', text: 'Regex', icon: 'regex', group: this.homeGroup, action: () => { this.showRightSidePane('RegexLibPane'); } },
        { name: 'sep-export', isSeparator: true, group: this.homeGroup },
        { name: 'exportAll', text: 'Export All', icon: 'send text-success', group: this.homeGroup, action: () => { this.exportAll(); } },
        { name: 'sep-finish', isSeparator: true, group: this.homeGroup },
        { name: 'finish', text: 'Finish', icon: 'check-all', group: this.homeGroup, action: () => { this.finishBook(); } },

        // content
        { name: 'images', text: 'Images', icon: 'image', group: this.contentGroup, action: () => { this.showRightSidePane('ImagesPane'); } },
        { name: 'wav', text: 'WAV', icon: 'soundwave', group: this.contentGroup, action: () => { this.showRightSidePane('WavPane'); } },
        { name: 'mp3', text: 'MP3', icon: 'filetype-mp3', group: this.contentGroup, action: () => { this.showRightSidePane('Mp3Pane'); } },
        { name: 'video', text: 'Video', icon: 'film', group: this.contentGroup, action: () => { this.showRightSidePane('VideoPane'); } },

        // export
        { name: 'exportWav', text: 'Export WAV', icon: 'soundwave', group: this.exportGroup, action: () => { this.showRightSidePane('ExportWavPane'); } },
        { name: 'exportMp3', text: 'Export MP3', icon: 'filetype-mp3', group: this.exportGroup, action: () => { this.showRightSidePane('ExportMp3Pane'); } },
        { name: 'exportVideo', text: 'Export Video', icon: 'film', group: this.exportGroup, action: () => { this.showRightSidePane('ExportVideoPane'); } },
        { name: 'exportText', text: 'Export Text', icon: 'file-text', group: this.exportGroup, action: () => { this.showRightSidePane('ExportTextPane'); } },
        { name: 'exportSubtitle', text: 'Export Subtitle', icon: 'badge-cc', group: this.exportGroup, action: () => { this.exportSubtitle(); } },
        { name: 'sep4', isSeparator: true, group: this.exportGroup },
        { name: 'exportAll', text: 'Export All', icon: 'send text-success', group: this.exportGroup, action: () => { this.exportAll(); } },
        { name: 'sep41', isSeparator: true, group: this.exportGroup },
        { name: 'combineVideos', text: 'Combine Videos', icon: 'gpu-card', group: this.exportGroup, action: () => { this.combineVideos(); } },

        // view
        { name: 'folder-all', text: 'Fold All', icon: 'arrows-collapse', group: this.viewGroup, action: () => { this.foldAll(); } },
        { name: 'expand-all', text: 'Expand All', icon: 'arrows-expand', group: this.viewGroup, action: () => { this.expandAll(); } },
        { name: 'sep2', isSeparator: true, group: this.viewGroup },
        { name: 'loggingPane', text: 'Messages', icon: 'exclamation-circle', group: this.viewGroup, action: () => { this.showBottomPane('LoggingPane'); } },
        { name: 'sep5', isSeparator: true, group: this.viewGroup },

        // tools
        { name: 'initFolders', text: 'Init Folders', icon: 'folder-plus', group: this.toolsGroup, action: () => { this.initFolders(); } },
        { name: 'fixSplash', text: 'Update Splash', icon: 'images', group: this.toolsGroup, action: () => { this.fixSplash(); } },
        { name: 'sep-tool', isSeparator: true, group: this.toolsGroup },
        { name: 'enhanceMp3', text: 'Enhance MP3', icon: 'collection', group: this.toolsGroup, action: () => { this.enhanceMp3(); } },
        { name: 'sep-tool2', isSeparator: true, group: this.toolsGroup },
        { name: 'publishToLibrary', text: 'Sync to Library', icon: 'collection', group: this.toolsGroup, action: () => { this.syncToLibrary(); } },

        // { name: 'download', text: 'Download', icon: 'cloud-arrow-down', group: this.toolsGroup, action: () => { this.downloadTxt(); } },
        // { name: 'pdf', text: 'PDF', icon: 'file-pdf', group: this.toolsGroup, action: () => { this.downloadPdf(); } },
    ];
    fileNavItems = this.filterByGroup(this.navItems, this.fileGroup);
    panes = [
        {
            name: 'OutlinePane', title: 'Outline', icon: 'card-list',
            context: this.editorContext, location: this.locationLeft,
            tag: this.findNavItem(this.navItems, 'bookOutline')
        },
        {
            name: 'ValidatePane', title: 'Validation', icon: 'card-checklist',
            context: this.editorContext, location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'validate')
        },
        {
            name: 'ActionsPane', title: 'Actions', icon: 'bookmarks',
            context: this.editorContext, location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'actions')
        },
        {
            name: 'FormatPane', title: 'Format', icon: 'compass', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'bookFormat'),
            context: this.editorContext,
        },
        {
            name: 'ImagesPane', title: 'Images', icon: 'image', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'images'),
            context: this.editorContext,
        },
        {
            name: 'WavPane', title: 'WAV', icon: 'soundwave', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'wav'),
            context: this.editorContext,
        },
        {
            name: 'Mp3Pane', title: 'MP3', icon: 'filetype-mp3', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'mp3'),
            context: this.editorContext,
        },
        {
            name: 'VideoPane', title: 'Video', icon: 'film', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'video'),
            context: this.editorContext,
        },
        {
            name: 'BookPropertyPane', title: 'Book', icon: 'journal', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'bookProperty'),
            context: this.editorContext,
        },
        {
            name: 'ExportWavPane', title: 'Export WAV', icon: 'soundwave', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'exportWav'),
            context: this.editorContext,
        },
        {
            name: 'ExportMp3Pane', title: 'Export MP3', icon: 'filetype-mp3', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'exportMp3'),
            context: this.editorContext,
        },
        {
            name: 'ExportVideoPane', title: 'Export Video', icon: 'film', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'exportVideo'),
            context: this.editorContext,
        },
        {
            name: 'ExportTextPane', title: 'Export Text', icon: 'file-text', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'exportText'),
            context: this.editorContext,
        },
        {
            name: 'RegexLibPane', title: 'Regex Library', icon: 'regex', location: this.locationRight,
            tag: this.findNavItem(this.navItems, 'regexes'),
            context: this.editorContext,
        },

        // bottom panes
        {
            name: 'LoggingPane', title: 'Messages', icon: 'exclamation-circle',
            location: this.locationBottom,
            tag: this.findNavItem(this.navItems, 'loggingPane'),
            context: this.editorContext,
        },
    ];

    constructor(private editorContext: EditorContext) { }

    createView(): EditorView {
        let view: EditorView = {
            navItems: this.navItems,
            fileNavItems: this.fileNavItems,
            mainNav: [
                { title: 'Home', items: this.filterByGroup(this.navItems, this.homeGroup), target: '#home-tab', isActive: true },
                { title: 'Content', items: this.filterByGroup(this.navItems, this.contentGroup), target: '#content-tab' },
                { title: 'Export', items: this.filterByGroup(this.navItems, this.exportGroup), target: '#export-tab' },
                { title: 'View', items: this.filterByGroup(this.navItems, this.viewGroup), target: '#view-tab' },
                { title: 'Tools', items: this.filterByGroup(this.navItems, this.toolsGroup), target: '#tools-tab' },
            ],
            panes: this.panes,
            leftPanes: this.filterByLocation(this.panes, this.locationLeft),
            rightPanes: this.filterByLocation(this.panes, this.locationRight),
            bottomPanes: this.filterByLocation(this.panes, this.locationBottom),
            context: this.editorContext
        };

        this.activeRightSidePane = 'ActionsPane';
        this.activeLeftPane = 'OutlinePane';

        this.showLeftSidePane(this.activeLeftPane);
        this.showRightSidePane(this.activeRightSidePane);
        return view;
    }

    filterByGroup(items: NavItem[], group: string): NavItem[] {
        return items.filter(item => item.group === group);
    }

    filterByLocation(items: EditorPane[], location: string): EditorPane[] {
        return items.filter(item => item.location === location);
    }

    findNavItem(items: NavItem[], name: string): NavItem | undefined {
        return items.find(item => item.name === name);
    }

    showRightSidePane(paneName: string) {
        let key = '';
        if (this.activeRightSidePane) {
            let navItem = this.findRightSidePane(this.activeRightSidePane)?.tag;
            if (navItem) {
                navItem.isActive = false;
            }
            key = `show${this.activeRightSidePane}`;
            this.editorContext!.configuration![key] = false;
        }

        key = `show${paneName}`;
        this.editorContext!.configuration![key] = true;
        this.activeRightSidePane = paneName;
        let pane = this.findRightSidePane(this.activeRightSidePane);
        if (pane) {
            pane!.context = this.editorContext;
        }
        let navItem = pane?.tag;
        if (navItem) {
            navItem.isActive = true;
        }

        this.layout();
    }

    showBottomPane(paneName: string) {
        let key = '';
        if (this.activeBottomPane) {
            let navItem = this.findBottomPane(this.activeBottomPane)?.tag;
            if (navItem) {
                navItem.isActive = false;
            }
            key = `show${this.activeBottomPane}`;
            this.editorContext!.configuration![key] = false;
        }

        key = `show${paneName}`;
        this.editorContext!.configuration![key] = true;
        this.activeBottomPane = paneName;
        let navItem = this.findBottomPane(this.activeBottomPane)?.tag;
        if (navItem) {
            navItem.isActive = true;
        }
        this.layout();
    }

    showLeftSidePane(paneName: string) {
        let key = '';
        if (this.activeLeftPane) {
            let navItem = this.findLeftSidePane(this.activeLeftPane)?.tag;
            if (navItem) {
                navItem.isActive = false;
            }
            key = `show${this.activeLeftPane}`;
            this.editorContext!.configuration![key] = false;
        }

        key = `show${paneName}`;
        this.editorContext!.configuration![key] = true;
        this.activeLeftPane = paneName;
        let pane = this.findLeftSidePane(this.activeLeftPane);
        if (pane) {
            pane!.context = this.editorContext;
        }
        let navItem = pane?.tag;
        if (navItem) {
            navItem.isActive = true;
        }

        this.layout();
    }

    showBookProperty() {
        this.showRightSidePane('BookPropertyPane');
    }

    findRightSidePane(name: string) {
        return this.findPane(name, this.locationRight);
    }

    findBottomPane(name: string) {
        return this.findPane(name, this.locationBottom);
    }

    findLeftSidePane(name: string) {
        return this.findPane(name, this.locationLeft);
    }

    findPane(name: string, location: string) {
        return this.panes?.filter(x => x.location === location)?.find(p => p.name === name);
    }

    foldAll() {
        this.editorContext?.rawEditor?.trigger('fold', 'editor.foldAll');
    }

    expandAll() {
        this.editorContext?.rawEditor?.trigger('fold', 'editor.unfoldAll');
    }

    undo() {
        this.editorContext?.rawEditor.trigger('keyboard', 'undo', null);
    }

    redo() {
        this.editorContext?.rawEditor.trigger('keyboard', 'redo', null);
    }

    layout() {
        if (this.layoutAction) {
            this.layoutAction();
        }
    }

    async save() {
        let book = this.editorContext.book;
        try {
            let service = this.editorContext.bookService;
            book!.status = TaskStatus.InProgress;
            if (book?.content?.length == 0) {
                this.editorContext.notificationService?.showFail(
                    'Book not saved',
                    `Book 《${book?.title}》content is blank.`,
                );
                return;
            }
            let result = await service?.updateBookContent(book);

            let memoryService = this.editorContext.memoryService;
            if (memoryService) {
                memoryService.set('bookTitle', book?.title);
                memoryService.set('bookAuthor', book?.author);
                memoryService.set('bookCategory', book?.categoryIds);
            }

            if (this.editorContext.router?.url === '/new') {
                this.editorContext.router?.navigate(['/edit', result?.id, book?.title]);
            }

            this.editorContext.isDirty = false;
            this.editorContext.notificationService?.showSuccess(
                'Book saved',
                `Book 《${book?.title}》 saved successfully.`,
            );
        } catch (error) {
            this.editorContext.logger?.error('Save book failed', error);
            this.editorContext.notificationService?.showFail(
                'Save book failed',
                `Failed to save book 《${book?.title}》.`,
            );
        }
    }

    async openFolder() {
        let bookService = this.editorContext.bookService;
        let book = this.editorContext.book;
        bookService?.openFolder(book?.id!);
    }

    async openAuthorFolder() {
        let authorService = this.editorContext.authorService;
        let book = this.editorContext.book;
        authorService?.openFolder(book?.authorId!);
    }

    insert(text: string, atBegin = true, atEnd = false): void {
        if (!text) return;

        let editor = this.editorContext.rawEditor;
        let position = editor.getSelection();
        if (atBegin) {
            position = position.setStartPosition(position.startLineNumber, 0);
            position = position.setEndPosition(position.startLineNumber, 0);
        } else if (atEnd) {
            position = position.setStartPosition(position.endLineNumber, position.endColumn);
            position = position.setEndPosition(position.endLineNumber, position.endColumn);
        }

        let editOperation = {
            range: position,
            text: text,
            forceMoveMarkers: true
        };

        console.log('Insert', position, atBegin, atEnd);
        editor.executeEdits("insert", [editOperation]);
    }

    async paste() {
        let text = await this.editorContext.clipboardService?.read();
        text = this.editorContext.textTransformService?.toSimplify(text!);
        this.setContent('paste-simplify', text!);
    }

    setContent(actionName: string, content: string): void {
        let editor = this.editorContext.rawEditor;
        let fullRange = editor?.getModel()?.getFullModelRange();
        editor.executeEdits(actionName, [{
            range: fullRange,
            text: content,
        }]);
    }

    close() {
        if (!this.editorContext?.isDirty) {
            this.editorContext.router?.navigate(['/welcome']);
        }
    }

    async exportAll() {
        let modalService = this.editorContext.modalService;
        let notificationService = this.editorContext.notificationService;

        let book = this.editorContext.book!;
        let image = await this.editorContext.imageService?.getImageResourceById(book.defaultImageId!);

        modalService?.openModal({
            title: 'Export All',
            body: 'Export all chapters to WAV, MP3 and Video files.',
            icon: 'info-circle',
            buttons: [
                {
                    label: 'OK',
                    style: 'primary',
                    action: () => {
                        try {
                            let chapterService = this.editorContext.chapterService;
                            let exportService = this.editorContext.exportService;
                            let bookName = this.editorContext.book?.title!;
                            let model = this.editorContext.rawEditor.getModel();
                            let chapters = chapterService?.toChaptersWithChunk(model, model.getValue(), bookName);

                            exportService?.exportChapters({
                                bookName: bookName,
                                author: this.editorContext.book!.author!,
                                type: 'all',
                                chapters: chapters!,
                                speechService: this.editorContext.book!.speechService,
                                language: this.editorContext.book!.language,
                                voiceName: this.editorContext.book!.voiceName,
                                image: image.fileName
                            });

                            this.showBottomPane('LoggingPane');

                            return true;
                        } catch (e) {
                            this.editorContext.notificationService?.showFail('Export', `Export with error: ${e}`);
                            return false;
                        }
                    }
                },
                {
                    label: 'Cancel',
                    style: 'secondary',
                    action: () => {
                        notificationService?.showSuccess('Export All', 'Export all chapters canceled.');
                        return true;
                    }
                }
            ]
        });
    }

    async exportSubtitle() {
        let book = this.editorContext.book;
        let exportService = this.editorContext.exportService;
        exportService?.exportBookSubtitle(book?.id!);
    }

    async combineVideos() {
        let book = this.editorContext.book;
        let exportService = this.editorContext.exportService;
        exportService?.combineVideos(book?.id!);
    }

    async downloadTxt() {
        this.downloadFile('txt');
    }

    async downloadPdf() {
        this.downloadFile('pdf');
    }

    async initFolders() {
        let book = this.editorContext.book;
        let bookService = this.editorContext.bookService;
        bookService?.initFolders(book?.id!);
    }

    async fixSplash() {
        let book = this.editorContext.book;
        let bookService = this.editorContext.bookService;
        await bookService?.fixSplash(book?.id!);

        this.editorContext.book = await this.editorContext.bookService?.getBook(book?.id!);

        this.editorContext.notificationService?.showSuccess(
            'Fix Splash',
            `Splash for book 《${book?.title}》 fixed successfully.`,
        );
        this.showRightSidePane('BookPropertyPane');
    }

    async enhanceMp3() {
        console.log('Enhance MP3');
        let book = this.editorContext.book;
        let bookService = this.editorContext.bookService;
        bookService?.enhanceMp3(book?.id!);
    }

    async syncToLibrary() {
        let notificationService = this.editorContext.notificationService;
        let book = this.editorContext.book;

        notificationService?.showSuccess('Sync to Library', `Start syncing 《${book?.title}》 to library...`);

        let bookService = this.editorContext.bookService;
        let result = bookService?.syncToLibrary(book?.id!);
        if (result) {
            notificationService?.showSuccess('Sync to Library', `Book 《${book?.title}》 synced to library successfully.`);
        }
        else {
            notificationService?.showFail('Sync to Library', `Failed to sync book 《${book?.title}》 to library.`);
        }
    }

    downloadFile(format: string) {
        let bookName = this.editorContext.book?.title!;
        let apiService = this.editorContext?.apiService;
        let url = apiService?.getFullUrl(`book/download/${format}/${bookName}`);

        this.showWindow(url!);
    }

    showWindow(url: string) {
        const newWindow = window.open(url, '_blank', 'width=600,height=400');
        if (newWindow && url) {
            newWindow.location.href = url;
        }
    }

    async resetBookStatus() {
        let book = this.editorContext.book;
        this.editorContext.bookService?.resetBook(book!.id!);
        this.editorContext.notificationService?.showSuccess(
            'Book reset',
            `Book 《${book?.title}》is reset to original status successfully.`,
        );
    }

    async finishBook() {
        let book = this.editorContext.book;
        try {
            book!.status = TaskStatus.Finished;
            book!.wavGenerated = true;
            book!.mp3Generated = true;
            book!.mp4Generated = true;
            book!.srtGenerated = true;
            this.editorContext.bookService?.updateBook(book!);
            this.editorContext.notificationService?.showSuccess(
                'Book finished',
                `Book 《${book?.title}》is marked as finished successfully.`,
            );
        }
        catch (error) {
            this.editorContext.logger?.error('Finish book failed', error);
            this.editorContext.notificationService?.showFail(
                'Finish book failed',
                `Failed to mark book 《${book?.title}》as finished.`,
            );
        }
    }

    preview() {
        this.editorContext.showPreview = !this.editorContext.showPreview;

        if (this.editorContext.showPreview) {
            let book = this.editorContext.book;
            let chapterService = this.editorContext.chapterService;
            let model = this.editorContext.rawEditor.getModel();
            let content = model.getValue();
            let exportBook = book as ExportBook;

            exportBook.chapters = chapterService?.toChaptersWithChunk(model, content, book?.title);
            exportBook.chapterCount = exportBook.chapters?.length;
            exportBook.chunkCount = exportBook.chapters?.reduce(
                (count, chapter) => count + (chapter.chunks ? chapter.chunks.length : 0),
                0
            );
        }
    }

    async generateMeta() {
        let book = this.editorContext.book;
        let result = await this.editorContext.bookService?.generateMeta(book?.id!);
        if (result) {
            this.editorContext.notificationService?.showSuccess(
                'Generate Metadata',
                `Metadata for book 《${book?.title}》 generated successfully.`,
            );
        } else {
            this.editorContext.notificationService?.showFail(
                'Generate Metadata',
                `Failed to generate metadata for book 《${book?.title}》.`,
            );
        }
    }
}