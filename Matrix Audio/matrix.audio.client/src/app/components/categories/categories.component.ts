import { Component, Input, OnInit } from '@angular/core';
import { TileCardComponent } from '../tile-card/tile-card.component';
import { CardSize, CategoryLayout } from '../../models/views';
import { AlbumService } from '../../services/album.service';
import { ILogger } from '../../models/logger';
import { LoggingService } from '../../services/logging.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LoadingComponent } from '../loading/loading.component';
import { TranslateService } from '../../services/translate.service';


@Component({
    selector: 'mtx-categories',
    templateUrl: './categories.component.html',
    imports: [TileCardComponent, CommonModule, RouterModule, CommonModule, LoadingComponent]
})
export class CategoriesComponent implements OnInit {
    @Input() cardSize: CardSize = 'medium';
    @Input() layout: CategoryLayout = 'card';
    @Input() groups: any[] = [];
    pageSize = 48;
    isLoading = false;

    private imageMappping = new Map<string, string>([
        ['Autobiography', 'assets/images/category/Autobiography.png'],
        ['Fiction', 'assets/images/category/Fiction.png'],
        ['History', 'assets/images/category/History.png'],
        ['Mystery', 'assets/images/category/Mystery.png'],
        ['Political', 'assets/images/category/Political.png'],
        ['Religion', 'assets/images/category/Religion.png'],
        ['Science', 'assets/images/category/Science.png'],
        ['Sexuality', 'assets/images/category/Sexuality.png'],
        ['Travel', 'assets/images/category/Travel.png'],

        ['1989', 'assets/images/category/1989.png'],
        ['英国', 'assets/images/category/british.png'],
        ['苏联', 'assets/images/category/Soviet Union.png'],
        ['台海', 'assets/images/category/taiwan.png'],
        ['西藏', 'assets/images/category/tibet.png'],
        ['新疆', 'assets/images/category/xinjiang.png'],
        ['蒙古', 'assets/images/category/mongolia.png'],
        ['回忆录', 'assets/images/category/memory book.png'],
        ['邓小平', 'assets/images/category/deng.png'],
        ['赵紫阳', 'assets/images/category/zhaoziyang.png'],
        ['周恩来', 'assets/images/category/zhouenlai.png'],
        ['毛泽东', 'assets/images/category/maozedong.png'],
        ['蒋介石', 'assets/images/category/Chiang Kai-shek.png'],
        ['狄仁杰', 'assets/images/category/direnjie.png'],
        ['中国', 'assets/images/category/china.png'],
        ['中共', 'assets/images/category/communist party.png'],
        ['国民党', 'assets/images/category/national party.png'],
        ['民国', 'assets/images/category/republic of china.png'],
        ['中华民国', 'assets/images/category/republic of china.png'],
        ['文革', 'assets/images/category/culture revolution.png'],
        ['古代', 'assets/images/category/ancient china.png'],
        ['圣经', 'assets/images/category/christ.png'],
        ['基督', 'assets/images/category/christ.png'],
        ['武侠', 'assets/images/category/martial arts.png'],
        ['战争', 'assets/images/category/war.png'],
        ['名著', 'assets/images/category/famous.png'],
        ['朝鲜', 'assets/images/category/north korea.png'],
        ['韩战', 'assets/images/category/korea war.png'],
        ['时评', 'assets/images/category/comment article.png'],
        ['土改', 'assets/images/category/land revolution.png'],
        ['反右', 'assets/images/category/china.png'],
        ['都市传说', 'assets/images/category/ledgendary.png'],
        ['宋慈', 'assets/images/category/songci.png'],
        ['太平天国', 'assets/images/category/tianguo.png'],
        ['大饥荒', 'assets/images/category/famine.png'],
        ['欧洲', 'assets/images/category/europe.png'],
        ['清朝', 'assets/images/category/qing.png'],
        ['慈禧', 'assets/images/category/queen.png'],
        ['悬疑', 'assets/images/category/suspense novels.png'],
    ]);
    private logger: ILogger

    constructor(
        private readonly albumService: AlbumService,
        private readonly translateService: TranslateService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('CategoriesComponent');
    }

    async ngOnInit() {
        this.isLoading = true;
        let groups = await this.albumService.getGroups();
        this.logger.info('Album groups: ', groups);

        for (let group of groups) {
            this.groups.push({
                title: this.translateService.translate(group.name),
                image: this.imageMappping.get(group.name) || 'assets/images/category/novel.png',
                route: group.type === 'Category' ?
                    `/public/albums/category/list/horizontal/${group.name}/${this.pageSize}/1/${group.id}` :
                    `/public/albums/tag/list/horizontal/${group.name}/${this.pageSize}/1/${group.id}`
            });
        }
        this.logger.info('Album groups: ', this.groups);
        this.isLoading = false;
    }
}
