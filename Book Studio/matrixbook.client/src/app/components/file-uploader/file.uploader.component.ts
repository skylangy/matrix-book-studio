import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
    selector: 'mtx-file-uploader',
    templateUrl: './file.uploader.component.html',
})
export class FileUploaderComponent implements OnInit {
    @Input() acceptTypes = '.jpg, .jpg, .png';
    @Output() onFileReady = new EventEmitter<File[]>();
    selectedFiles: File[] = [];
    images: string[] = [];

    constructor() { }

    get hasImages(): boolean {
        return this.images && this.images.length > 0;
    }

    ngOnInit(): void { }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        let droppedFile = event.dataTransfer?.files[0];
        if (droppedFile && droppedFile.type.startsWith('image/')) {
            let reader = new FileReader();
            reader.onload = (e) => {
                this.images.push(e.target?.result as string);
            };
            reader.readAsDataURL(droppedFile);
        }

        this.handleFiles(event?.dataTransfer?.files);
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        this.handleFiles(input.files);
    }

    private handleFiles(files: FileList | null | undefined): void {
        if (files) {
            for (let i = 0; i < files.length; i++) {
                this.selectedFiles.push(files.item(i)!);
            }

            this.onFileReady.emit(this.selectedFiles || []);
        }
    }

    deleteImage(image: string): void {
        let index = this.images.indexOf(image);
        if (index >= 0) {
            this.images.splice(index, 1);
        }
    }
}
