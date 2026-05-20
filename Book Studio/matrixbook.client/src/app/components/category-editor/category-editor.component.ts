import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BookService } from 'src/app/services/book-service';
import { LoggingService } from 'src/app/services/logging-services';
import { ILogger } from 'src/app/models/logger';

export interface ICategory {
    id?: string;
    name?: string;
    isSelected?: boolean;
}

@Component({
    selector: 'mtx-category-editor',
    templateUrl: './category-editor.component.html',
    imports: [FormsModule]
})
export class CategoryEditorComponent implements OnInit {
    private categoryValue?: string = '';
    private categoryIdsValue?: string[] = [];
    categoryNames: string[] =
        ['Art', 'Fiction',
            'Science', 'History', 'Mystery', 'Horror',
            'Religion', 'Political', 'Autobiography',
            'Travel', 'Romance', 'Sexuality',
            'Other'];
    categories: ICategory[] = this.categoryNames.map((category: string) => ({
        name: category,
        isSelected: false,
    }));

    @Input() id: string = '';
    @Output() categoryChange = new EventEmitter<string | undefined>();
    @Output() categoryIdsChange = new EventEmitter<string[] | undefined>();
    private logger?: ILogger;

    constructor(private readonly bookService: BookService,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('CategoryEditorComponent');
    }

    @Input()
    get category(): string | undefined {
        return this.categoryValue;
    }

    set category(value: string | undefined) {
        if (value) {
            this.categoryValue = value;
            let categories = this.categoryValue.split(',').map(category => category.trim());

            this.categories.forEach(category => {
                category.isSelected = categories.includes(category.name || '');
            });
        } else {
            this.categories.forEach(category => {
                category.isSelected = false;
            });
        }
    }

    @Input()
    get categoryIds(): string[] | undefined {
        return this.categoryIdsValue;
    }

    set categoryIds(value: string[] | undefined) {
        this.categoryIdsValue = value;
        if (value) {
            this.categories.forEach(category => {
                category.isSelected = this.categoryIdsValue?.includes(category.id || '');
            });
        } else {
            this.categories.forEach(category => {
                category.isSelected = false;
            });
        }
    }

    async ngOnInit() {
        let categories = await this.bookService.getGroupCategories();

        this.categories = categories.map((category: any) => ({
            id: category.id,
            name: category.name,
            isSelected: false,
        }));

        this.categories.forEach(category => {
            category.isSelected = this.categoryIdsValue?.includes(category.id || '');
        });
        this.categoryValue = this.categories.filter(c => c.isSelected).map(c => c.name).join(', ');
    }

    onCategoryChanged(category: ICategory) {
        this.categoryValue = this.categories.filter(c => c.isSelected).map(c => c.name).join(', ');
        let categryIds = this.categories.filter(c => c.isSelected).map(c => c.id).filter((id): id is string => id !== undefined);
        this.categoryChange.emit(this.categoryValue);
        this.categoryIdsChange.emit(categryIds);
    }
}
