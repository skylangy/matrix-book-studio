import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VideoMeta } from 'src/app/models/video-meta';
import { LocalSettingService } from 'src/app/services/local-setting.service';
import { LoggingService } from 'src/app/services/logging-services';
import { VideoService } from 'src/app/services/video-service';
import { HeaderComponent } from '../header/header.component';
import { LoadingComponent } from '../loading/loading.component';
import { WorkViewComponent } from '../work-view/work.view.component';
import { VideoCardComponent } from './video.card.component';


@Component({
    selector: 'mtx-video-manager',
    templateUrl: 'video.manage.component.html',
    imports: [CommonModule,
        FormsModule,
        HeaderComponent,
        LoadingComponent,
        VideoCardComponent]
})
export class VideoManageComponent extends WorkViewComponent {
    filterText = '';
    isLoading = signal(false);
    videos = signal<VideoMeta[]>([]);

    constructor(
        private router: Router,
        private activateRoute: ActivatedRoute,
        private videoService: VideoService,
        private localSettingService: LocalSettingService,
        private loggingService: LoggingService
    ) {
        super();
        this.title = 'Videos';
        this.bannerImage = './assets/images/splashes/photo-11.jpg';
    }

    override async ngOnInit() {
        this.isLoading.set(true);
        this.videos.set(await this.videoService.getAllVideos());
        console.log('VideoManageComponent: videos', this.videos());
        this.isLoading.set(false);
    }

    get filter() {
        return this.filterText;
    }

    set filter(value: string) {
        if (this.filterText !== value) {
            this.filterText = value;
            this.onFilterChanged(value);
        }
    }

    onFilterChanged(filter: string): void {

    }

    createNewVideo() {
        this.router.navigate(['/videos', 'new']);
    }

    onVideoDeleted(id: string) {
        this.videos.set(this.videos().filter(video => video.id !== id));
    }
}