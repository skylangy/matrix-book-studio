import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { PostService } from '../../services/post.service';
import { Post } from '../../models/post';
import { TextToHtmlPipe } from '../../pipes/tohtml.pipe';
import { RouterModule } from '@angular/router';
import { SummaryPipe } from '../../pipes/summary.pipe';

@Component({
    selector: 'mtx-post-slide',
    templateUrl: './post-slide.component.html',
    imports: [CommonModule, RouterModule, ImageUrlPipe, TextToHtmlPipe, SummaryPipe]
})
export class PostSlideComponent implements OnInit {
    @Input() showControls = true;
    @Input() showIndicators = true;

    posts: Post[] = [];

    constructor(private postService: PostService) { }

    async ngOnInit() {
        this.posts = await this.postService.getRecents();
    }
}
