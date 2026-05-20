import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'fileSize'
})
export class FileSizePipe implements PipeTransform {
    transform(sizeInBytes?: number): string {
        if (sizeInBytes === 0 || sizeInBytes === undefined)
            return '0 Bytes';

        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
        const i = Math.floor(Math.log(sizeInBytes) / Math.log(1024));

        return parseFloat((sizeInBytes / Math.pow(1024, i)).toFixed(2)) + ' ' + sizes[i];
    }
}
