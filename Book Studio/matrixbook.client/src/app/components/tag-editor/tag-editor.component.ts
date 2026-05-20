import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BookService } from 'src/app/services/book-service';

@Component({
    selector: 'mtx-tag-editor',
    templateUrl: './tag-editor.component.html',
    imports: [FormsModule],
})
export class TagEditorComponent implements OnInit {
    @Input() placeholder = '';
    @Input() color = 'primary';
    @Input() id: string = '';
    @Input() suggestions: string[] = [];

    tagInput: string = '';
    tagValues: string[] = [];
    editingTag = '';

    private tagString?: string = ''
    @Output() tagChange = new EventEmitter();

    constructor(private readonly bookService: BookService) {

    }

    async ngOnInit() {
        this.suggestions = await this.bookService.getTags();
    }

    @Input()
    get tag(): string | undefined {
        return this.tagString;
    }
    set tag(value: string | undefined) {
        if (value) {
            this.tagValues = value.split(',') ?? [];
            this.tagString = value;
        }
        else {
            this.tagValues = [];
            this.tagString = '';
        }
    }

    addTag() {
        if (this.tagInput.trim() !== '' && !this.tagValues.includes(this.tagInput.trim())) {
            this.tagValues.push(this.tagInput.trim());
            this.tagInput = '';
        }
    }

    removeTag(tag: string) {
        const index = this.tagValues.indexOf(tag);
        if (index !== -1) {
            this.tagValues.splice(index, 1);
        }
        this.update();
    }

    inputKeyup(keyArgs: any): void {
        if (keyArgs.keyCode === 13) {
            this.onInputBlur();
        } else if (keyArgs.keyCode === 8) {
            if (!this.editingTag && this.tagValues.length > 0) {
                let last = this.tagValues[this.tagValues.length - 1];
                this.removeTag(last);
            }
        }
    }

    onInputBlur(): void {
        if (this.editingTag) {
            this.tagValues.push(this.editingTag);
            this.update();
            this.editingTag = '';
        }
    }

    clear(): void {
        this.tagValues = [];
        this.update();
    }

    addTagValue(value: string): void {
        if (value && !this.tagValues.includes(value.trim())) {
            this.tagValues.push(value);
            this.update();
        }
    }

    private update(): void {
        if (this.tagValues.length > 0) {
            this.tag = this.tagValues.join(',');
            this.tagChange.emit(this.tag);
        }
        else {
            this.tag = '';
            this.tagChange.emit(this.tag);
        }
    }
}
