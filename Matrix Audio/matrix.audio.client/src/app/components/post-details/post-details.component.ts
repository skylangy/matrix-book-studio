import { Component, OnInit } from '@angular/core';
import { Post } from '../../models/post';
import { PostService } from '../../services/post.service';
import { ActivatedRoute } from '@angular/router';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { CommonModule } from '@angular/common';
import { ImageUrlPipe } from '../../pipes/image.pipe';

@Component({
    selector: 'mtx-post-details',
    templateUrl: './post-details.component.html',
    imports: [CommonModule, TextToHtmlPipe, ImageUrlPipe]
})
export class PostDetailsComponent implements OnInit {
    post: Post | undefined = undefined;

    constructor(private activatedRoute: ActivatedRoute,
        private postService: PostService) { }

    async ngOnInit() {
        this.activatedRoute.params.subscribe(async params => {
            let id = params['id'];
            if (!id)
                return;
            this.post = await this.postService.getPost(id);
            console.log(this.post);
        });
    }
}
