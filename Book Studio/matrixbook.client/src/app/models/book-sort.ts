import { Book } from "./book";

export type SortOrder = 'asc' | 'desc';

class Sorter<T> {
    private primarySortFn: ((a: T, b: T) => number) = (a, b) => 0;

    constructor() { }

    sortBy(property: keyof T, order: SortOrder = 'asc') {
        this.primarySortFn = this.createSortFunction(property, order);
        return this;
    }

    thenBy(secondaryProperty: keyof T, order: SortOrder = 'asc') {
        let secondarySortFn = this.createSortFunction(secondaryProperty, order);
        let primarySort = this.primarySortFn;

        this.primarySortFn = (a, b) => {
            let result = primarySort(a, b);

            if (result === 0) {
                return secondarySortFn(a, b);
            }

            return result;
        };

        return this;
    }

    sort(array: T[]) {
        return array.sort(this.primarySortFn);
    }


    private createSortFunction(property: keyof T, order: SortOrder) {
        return (a: T, b: T) => {
            const aValue = a[property];
            const bValue = b[property];

            if (order === 'asc') {
                return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
            } else {
                return bValue < aValue ? -1 : bValue > aValue ? 1 : 0;
            }
        };
    }
}

export class BookSorter {

    public static sort(books: Book[], sortProp?: keyof Book, thenByProp?: keyof Book, sortOrder: SortOrder = 'asc', thenOrder: SortOrder = 'asc'): void {
        let sorter = new Sorter<Book>().sortBy(sortProp!, sortOrder).thenBy(thenByProp!, thenOrder);
        sorter.sort(books);
    }
}