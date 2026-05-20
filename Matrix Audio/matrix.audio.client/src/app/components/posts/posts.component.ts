import { Component, OnInit } from '@angular/core';
import { Post } from '../../models/post';
import { PostService } from '../../services/post.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ImageUrlPipe } from '../../pipes/image.pipe';
import { SummaryPipe } from '../../pipes/summary.pipe';
import { BannerComponent } from '../banner/banner.component';

@Component({
    selector: 'mtxt-posts',
    templateUrl: './posts.component.html',
    imports: [CommonModule, RouterModule, ImageUrlPipe, SummaryPipe, BannerComponent]
})
export class PostsComponent implements OnInit {
    posts: Post[] = [];

    constructor(private postService: PostService) { }

    async ngOnInit() {
        this.posts = await this.postService.getRecents();
    }
}
