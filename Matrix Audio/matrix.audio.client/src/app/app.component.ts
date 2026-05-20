import { Component, HostListener, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PlayService } from './services/play.service';
import { InitializerService } from './services/initializer.service';
import { Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  constructor(
    private playService: PlayService,
    private initializerService: InitializerService,
    private titleService: Title
  ) { }

  async ngOnInit() {
    this.titleService.setTitle(environment.title);
    await this.initializerService.initialize();
  }

  @HostListener('window:beforeunload', ['$event'])
  handleBeforeUnload(event: any) {
    this.playService.stop();
  }
}
