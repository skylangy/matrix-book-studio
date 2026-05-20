import { Component, OnInit } from '@angular/core';
import { Action } from '../../../models/action';

@Component({
    template: ''
})
export abstract class ManageViewBase<T> implements OnInit {
    items: T[] = [];
    actions: Action[] = [];

    page = 1;
    pageSize = 10;
    useSearch = true;
    icon = '';
    private _searchText = '';

    isLoading = false;

    constructor() { }

    async ngOnInit() {
        this.isLoading = true;
        this.items = await this.loadItems();
        this.isLoading = false;
    }

    get searchText(): string {
        return this._searchText;
    }

    set searchText(value: string) {
        this._searchText = value;

        if (this.searchText === '') {
            this.reload();
        } else {
            this.search();
        }
    }

    get hasItems(): boolean {
        return this.items != null && this.items.length > 0;
    }

    get canSearch(): boolean {
        return this._searchText != null && this._searchText.length > 0;
    }

    abstract loadItems(): Promise<T[]>;

    async reload() {
        this.items = [];
        this.items = await this.loadItems();
    }

    search() {
        this.items = [];
    }

    get canGoPrev(): boolean {
        return this.page > 1;
    }

    async nextPage() {
        this.page++;
        if (this.searchText === '') {
            this.items = await this.loadItems();
        } else {
            this.search();
        }
    }

    async prevPage() {
        this.page--;
        if (this.page < 1) {
            this.page = 1;
        }
        if (this.searchText === '') {
            this.items = await this.loadItems();
        } else {
            this.search();
        }
    }
}