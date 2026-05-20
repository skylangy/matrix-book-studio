import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'filter'
})
export class BookFilterPipe implements PipeTransform {

    transform(items: any[] | undefined, filter: string): any[] {
        if (!items || !filter) {
            return items || [];
        }

        filter = filter.toLowerCase();
        return items.filter(item => item.title.toLowerCase().includes(filter)
            || item.author.toLowerCase().includes(filter));
    }

}