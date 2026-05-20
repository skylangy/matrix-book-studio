import { Pipe, PipeTransform } from '@angular/core';
import { Author } from '../models/author';
import { ImageService } from '../services/image-service';

@Pipe({
    name: 'authorImage',

})
export class AuthorImagePipe implements PipeTransform {
    private defaultImage = 'assets/images/avatars/default.png';

    constructor(private imageService: ImageService) {
    }

    transform(author?: Author): string {
        if (!author) {
            return this.defaultImage;
        }

        let imageUrl = this.defaultImage;
        if (author.image) {
            imageUrl = this.imageService.getAuthorImageUrl(author.name!, author.image!);
        } else if (!author.image && author.images && author.images.length > 0) {
            author.image = author.images[0]?.url ?? '';
            imageUrl = this.imageService.getAuthorImageUrl(author.name!, author.image!);
        }
        return imageUrl;
    }
}